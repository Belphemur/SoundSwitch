<#
.SYNOPSIS
    Captures a cropped screenshot of the SoundSwitch UI for the documentation.

.DESCRIPTION
    Automation helper for keeping the screenshots in img\Documentation (published on
    the website as /images/) in sync with the application.

    The script:
      1. Builds the SoundSwitch application and CLI (unless -NoBuild).
      2. Refuses to run while another SoundSwitch instance is active (they share one
         named pipe and one config file), then launches the freshly built
         SoundSwitch.exe with --disable-updater.
      3. Opens the Settings window through the CLI (`SoundSwitch.CLI.exe settings`),
         using the app's public IPC surface instead of simulated tray input.
      4. Selects a settings tab by name with UI Automation.
      5. Captures ONLY the Settings window bounds with PrintWindow(PW_RENDERFULLCONTENT),
         so the PNG is cropped to the window and never includes the desktop.
      6. Saves the PNG into img\Documentation and closes what it opened.

    Requires PowerShell 7+ on Windows.

.PARAMETER Name
    Output file name, with or without the .png extension. A relative sub-path is
    allowed, e.g. "Notifications" or "faq/quick-menu-popup". Defaults to
    img\Documentation when not rooted. Required unless -ListWindows is used.

.PARAMETER Tab
    Settings tab to select before capturing. One of: Playback, Recording, Profiles,
    App Sound Lock, General, Notifications, Troubleshooting. The name is the
    localized display label, so pass the translated text on a non-English UI.
    When omitted, the current tab is captured.

.PARAMETER OutputDirectory
    Directory the PNG is written to when -Name is not rooted.
    Defaults to <repo>\img\Documentation.

.PARAMETER Configuration
    Build configuration to run. Defaults to Debug.

.PARAMETER TimeoutSeconds
    How long to wait for the Settings window to appear. Defaults to 60.

.PARAMETER RenderedDelayMilliseconds
    Extra delay after the tab is selected, before capturing, to let the UI settle.
    Defaults to 800.

.PARAMETER NoBuild
    Skip the build step and use the existing binaries.

.PARAMETER NoLaunch
    Do not launch SoundSwitch; fail if none is running.

.PARAMETER AllowExistingInstance
    Allow capturing an already running SoundSwitch instance instead of failing.

.PARAMETER KeepOpen
    Leave the Settings window open after capturing.

.PARAMETER KeepRunning
    Leave the SoundSwitch instance running when this script launched it.

.PARAMETER ListWindows
    Only list the visible top-level windows of the SoundSwitch process and exit.

.EXAMPLE
    .\tools\Capture-DocumentationScreenshot.ps1 -Name Notifications -Tab Notifications
    Builds the app, opens the Notifications tab and writes img\Documentation\Notifications.png.

.EXAMPLE
    .\tools\Capture-DocumentationScreenshot.ps1 -Name Playback -Tab Playback -NoBuild -KeepOpen
    Captures the Playback tab with the existing build and leaves the window open for review.

.EXAMPLE
    .\tools\Capture-DocumentationScreenshot.ps1 -ListWindows
    Lists the candidate windows so you can diagnose a capture that cannot find Settings.
#>
[CmdletBinding()]
param(
    [string]$Name,

    [string]$Tab,

    [string]$OutputDirectory,

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [int]$TimeoutSeconds = 60,

    [int]$RenderedDelayMilliseconds = 800,

    [switch]$NoBuild,

    [switch]$NoLaunch,

    [switch]$AllowExistingInstance,

    [switch]$KeepOpen,

    [switch]$KeepRunning,

    [switch]$ListWindows
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path (Join-Path $RepoRoot 'SoundSwitch.sln'))) {
    throw "Could not locate the repository root from '$PSScriptRoot'."
}

if (-not $ListWindows -and [string]::IsNullOrWhiteSpace($Name)) {
    throw '-Name is required (except with -ListWindows).'
}

# --- Win32 interop -----------------------------------------------------------------

if (-not ('SoundSwitch.DocCapture.Native' -as [type])) {
    Add-Type -Language CSharp -TypeDefinition @'
namespace SoundSwitch.DocCapture
{
    using System;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class Native
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxCount);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdc, uint flags);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int cmdShow);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }
}
'@
}

$Script:WmClose = 0x0010
$Script:SwRestore = 9
$Script:PwRenderFullContent = 0x00000002

function Get-TopLevelWindows {
    <#
        Visible, titled top-level windows owned by the given process. The Settings
        form is the only such window (the app has no "main" form); the message-pump
        and tray windows are hidden and title-less.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [int]$ProcessId
    )

    $windows = [System.Collections.Generic.List[object]]::new()
    $callback = [SoundSwitch.DocCapture.Native+EnumWindowsProc] {
        param([IntPtr]$handle, [IntPtr]$lParam)

        if ([SoundSwitch.DocCapture.Native]::IsWindowVisible($handle) -and
            [SoundSwitch.DocCapture.Native]::GetWindowTextLength($handle) -gt 0) {
            [uint32]$windowProcessId = 0
            [SoundSwitch.DocCapture.Native]::GetWindowThreadProcessId($handle, [ref]$windowProcessId) | Out-Null
            if ([int]$windowProcessId -eq $ProcessId) {
                $title = [System.Text.StringBuilder]::new(512)
                [SoundSwitch.DocCapture.Native]::GetWindowText($handle, $title, $title.Capacity) | Out-Null
                $windows.Add([pscustomobject]@{ Handle = $handle; Title = $title.ToString() })
            }
        }

        return $true
    }

    [SoundSwitch.DocCapture.Native]::EnumWindows($callback, [IntPtr]::Zero) | Out-Null
    return $windows
}

function Select-SettingsTab {
    <#
        Selects the tab with the given name inside the window and returns $true when
        the window exposes a tab control (and the requested tab was found). Returns
        $false when the window has no tabs, so callers can skip unrelated windows.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [IntPtr]$Handle,

        [string]$TabName
    )

    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes

    $window = [System.Windows.Automation.AutomationElement]::FromHandle($Handle)
    if ($null -eq $window) {
        return $false
    }

    $tabCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::TabItem)
    $tabs = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $tabCondition)
    if ($tabs.Count -eq 0) {
        return $false
    }

    if (-not $TabName) {
        return $true
    }

    foreach ($item in $tabs) {
        if ($item.Current.Name -eq $TabName) {
            $selection = $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
            $selection.Select()
            return $true
        }
    }

    return $false
}

function Save-WindowScreenshot {
    <#
        Captures the window bounds into a PNG. PrintWindow renders the window even
        when it is partly obscured and, combined with GetWindowRect, produces an
        image cropped to the window rather than the whole screen.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [IntPtr]$Handle,

        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if ([SoundSwitch.DocCapture.Native]::IsIconic($Handle)) {
        [SoundSwitch.DocCapture.Native]::ShowWindow($Handle, $Script:SwRestore) | Out-Null
    }

    [SoundSwitch.DocCapture.Native]::SetForegroundWindow($Handle) | Out-Null
    Start-Sleep -Milliseconds 150

    $rect = [SoundSwitch.DocCapture.Native+RECT]::new()
    if (-not [SoundSwitch.DocCapture.Native]::GetWindowRect($Handle, [ref]$rect)) {
        throw 'GetWindowRect failed for the Settings window.'
    }

    $width = $rect.Right - $rect.Left
    $height = $rect.Bottom - $rect.Top
    if ($width -le 0 -or $height -le 0) {
        throw "Unexpected Settings window size: ${width}x${height}."
    }

    Add-Type -AssemblyName System.Drawing

    $bitmap = [System.Drawing.Bitmap]::new($width, $height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $hdc = $graphics.GetHdc()
    $captured = $false
    try {
        $captured = [SoundSwitch.DocCapture.Native]::PrintWindow($Handle, $hdc, $Script:PwRenderFullContent)
    }
    finally {
        $graphics.ReleaseHdc($hdc)
        $graphics.Dispose()
    }

    if (-not $captured) {
        $bitmap.Dispose()
        throw 'PrintWindow failed to render the Settings window.'
    }

    $directory = Split-Path -Parent $Path
    if ($directory -and -not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()

    return [pscustomobject]@{ Path = $Path; Width = $width; Height = $height }
}

# --- Resolve binaries --------------------------------------------------------------

function Resolve-Executable {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ProjectDirectory,

        [Parameter(Mandatory = $true)]
        [string]$FileName
    )

    $binRoot = Join-Path $RepoRoot "$ProjectDirectory\bin\$Configuration"
    if (-not (Test-Path $binRoot)) {
        return $null
    }

    return Get-ChildItem -Path $binRoot -Filter $FileName -Recurse -ErrorAction SilentlyContinue |
        Select-Object -First 1
}

if (-not $NoBuild) {
    Write-Host 'Building the application and CLI...' -ForegroundColor Cyan
    $projects = @('SoundSwitch\SoundSwitch.csproj', 'SoundSwitch.CLI\SoundSwitch.CLI.csproj')
    foreach ($project in $projects) {
        & dotnet build (Join-Path $RepoRoot $project) -c $Configuration --nologo -v minimal
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed for '$project'."
        }
    }
}

$appExe = Resolve-Executable -ProjectDirectory 'SoundSwitch' -FileName 'SoundSwitch.exe'
$cliExe = Resolve-Executable -ProjectDirectory 'SoundSwitch.CLI' -FileName 'SoundSwitch.CLI.exe'
if (-not $appExe) {
    throw "SoundSwitch.exe not found under SoundSwitch\bin\$Configuration. Build first or drop -NoBuild."
}
if (-not $cliExe) {
    throw "SoundSwitch.CLI.exe not found under SoundSwitch.CLI\bin\$Configuration. Build first or drop -NoBuild."
}

# --- Resolve the target process ----------------------------------------------------

$existing = @(Get-Process -Name 'SoundSwitch' -ErrorAction SilentlyContinue)
$process = $null
$launched = $false

if ($existing.Count -gt 0) {
    if (-not $AllowExistingInstance) {
        $ids = ($existing | ForEach-Object { $_.Id }) -join ', '
        throw "SoundSwitch is already running (PID: $ids). Close it first, or pass -AllowExistingInstance to capture that instance."
    }

    $process = $existing[0]
    Write-Host "Using the running SoundSwitch instance (PID $($process.Id))." -ForegroundColor Yellow
}
elseif ($NoLaunch) {
    throw 'No running SoundSwitch instance and -NoLaunch was specified.'
}

if ($ListWindows -and $process) {
    Get-TopLevelWindows -ProcessId $process.Id | Format-Table -AutoSize
    return
}

if (-not $process) {
    Write-Host "Starting $($appExe.FullName) ..." -ForegroundColor Cyan
    $process = Start-Process -FilePath $appExe.FullName -ArgumentList '--disable-updater' -PassThru
    $launched = $true
}

try {
    if ($ListWindows) {
        # Give a freshly launched instance a moment to create its window.
        Start-Sleep -Seconds 2
        Get-TopLevelWindows -ProcessId $process.Id | Format-Table -AutoSize
        return
    }

    Write-Host 'Opening Settings...' -ForegroundColor Cyan
    $opened = $false
    for ($attempt = 1; $attempt -le 20 -and -not $opened; $attempt++) {
        & $cliExe.FullName settings | Out-Null
        if ($LASTEXITCODE -eq 0) {
            $opened = $true
        }
        else {
            Start-Sleep -Milliseconds 500
        }
    }

    if (-not $opened) {
        throw "Failed to open Settings via '$($cliExe.Name) settings'."
    }

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    $handle = [IntPtr]::Zero
    $windowTitle = $null

    while ((Get-Date) -lt $deadline -and $handle -eq [IntPtr]::Zero) {
        foreach ($candidate in @(Get-TopLevelWindows -ProcessId $process.Id)) {
            if ($Tab -and (Select-SettingsTab -Handle $candidate.Handle -TabName $Tab)) {
                $handle = $candidate.Handle
                $windowTitle = $candidate.Title
                break
            }

            if (-not $Tab -and (Select-SettingsTab -Handle $candidate.Handle -TabName $null)) {
                $handle = $candidate.Handle
                $windowTitle = $candidate.Title
                break
            }
        }

        if ($handle -eq [IntPtr]::Zero) {
            Start-Sleep -Milliseconds 300
        }
    }

    if ($handle -eq [IntPtr]::Zero) {
        $tabHint = if ($Tab) { " with the '$Tab' tab" } else { '' }
        throw "Settings window$tabHint was not found within $TimeoutSeconds seconds."
    }

    if ($Tab) {
        Write-Host "Capturing '$Tab' tab from window '$windowTitle'..." -ForegroundColor Cyan
    }
    else {
        Write-Host "Capturing window '$windowTitle'..." -ForegroundColor Cyan
    }

    Start-Sleep -Milliseconds $RenderedDelayMilliseconds

    if (-not $OutputDirectory) {
        $OutputDirectory = Join-Path $RepoRoot 'img\Documentation'
    }

    $fileName = if ([System.IO.Path]::GetExtension($Name) -eq '.png') { $Name } else { "$Name.png" }
    $targetPath = if ([System.IO.Path]::IsPathRooted($fileName)) { $fileName } else { Join-Path $OutputDirectory $fileName }

    $result = Save-WindowScreenshot -Handle $handle -Path $targetPath
    Write-Host "Saved $($result.Path) ($($result.Width)x$($result.Height))." -ForegroundColor Green

    if (-not $KeepOpen) {
        [SoundSwitch.DocCapture.Native]::PostMessage($handle, $Script:WmClose, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
    }
}
finally {
    if ($launched -and -not $KeepRunning) {
        $process | Wait-Process -Timeout 5 -ErrorAction SilentlyContinue
        if (-not $process.HasExited) {
            $process | Stop-Process -Force -ErrorAction SilentlyContinue
            Write-Host 'Stopped the SoundSwitch instance started by this script.' -ForegroundColor DarkGray
        }
    }
}
