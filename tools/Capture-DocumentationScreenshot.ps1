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
      4. Selects a settings tab by name with UI Automation and, when -Click is
         given, invokes a button to open a dialog (e.g. "Add app rule").
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
    App Rules, General, Notifications, Troubleshooting. The name is the localized
    display label, so pass the translated text on a non-English UI.
    When omitted, the current tab is captured.

.PARAMETER Click
    Accessible name or automation id of a button to click after the tab is
    selected. The window that button opens (usually a dialog) is captured instead
    of the Settings window, so dialogs such as "Add app rule" or "Add profile" can
    be produced with automation too. The automation id is the control name from the
    designer (e.g. addAppSoundRuleButton, addProfileButton); the visible label
    (e.g. "Add") also works when it is unambiguous on the selected tab.

.PARAMETER Select
    Accessible name or automation id of a radio button, check box, or tab to select
    inside the dialog opened by -Click, before capturing. Use it to capture a
    specific mode, e.g. -Click addAppSoundRuleButton -Select rdoHotkey.

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
    .\tools\Capture-DocumentationScreenshot.ps1 -Name "AppRules Add" -Tab "App Rules" -Click addAppSoundRuleButton
    Opens the App Rules tab, clicks Add and captures the "Add Rule" dialog.

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

    [string]$Click,

    [string]$Select,

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
$Script:BmClick = 0x00F5
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

function Find-VisibleButton {
    <#
        Finds a button inside the window by accessible name or automation id,
        preferring the one actually on screen (the same label can exist on several
        tab pages, e.g. the two "Add" buttons).
    #>
    param(
        [Parameter(Mandatory = $true)]
        [IntPtr]$Handle,

        [Parameter(Mandatory = $true)]
        [string]$ButtonId
    )

    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes

    $window = [System.Windows.Automation.AutomationElement]::FromHandle($Handle)
    if ($null -eq $window) {
        return $null
    }

    $condition = [System.Windows.Automation.AndCondition]::new(
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::Button),
        [System.Windows.Automation.OrCondition]::new(
            [System.Windows.Automation.PropertyCondition]::new(
                [System.Windows.Automation.AutomationElement]::NameProperty, $ButtonId),
            [System.Windows.Automation.PropertyCondition]::new(
                [System.Windows.Automation.AutomationElement]::AutomationIdProperty, $ButtonId)))

    $buttons = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
    $visible = [System.Collections.Generic.List[object]]::new()
    foreach ($button in $buttons) {
        if ($button.Current.AutomationId -eq $ButtonId) {
            return $button
        }

        if (-not $button.Current.IsOffscreen) {
            $visible.Add($button)
        }
    }

    if ($visible.Count -gt 0) {
        return $visible[0]
    }

    return $null
}

function Find-VisibleControl {
    <#
        Finds any control (radio button, check box, tab, ...) inside the window by
        accessible name or automation id, preferring the one actually on screen.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [IntPtr]$Handle,

        [Parameter(Mandatory = $true)]
        [string]$ControlId
    )

    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes

    $window = [System.Windows.Automation.AutomationElement]::FromHandle($Handle)
    if ($null -eq $window) {
        return $null
    }

    $condition = [System.Windows.Automation.OrCondition]::new(
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::NameProperty, $ControlId),
        [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::AutomationIdProperty, $ControlId))

    $controls = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $condition)
    foreach ($control in $controls) {
        if ($control.Current.AutomationId -eq $ControlId) {
            return $control
        }
    }

    foreach ($control in $controls) {
        if (-not $control.Current.IsOffscreen) {
            return $control
        }
    }

    return $null
}

function Select-Element {
    <#
        Selects a radio button, toggles a check box, or invokes a generic control.
    #>
    param(
        [Parameter(Mandatory = $true)]
        $Element
    )

    $selection = $null
    if ($Element.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$selection)) {
        $selection.Select()
        return
    }

    $toggle = $null
    if ($Element.TryGetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern, [ref]$toggle)) {
        if ($toggle.Current.ToggleState -ne [System.Windows.Automation.ToggleState]::On) {
            $toggle.Toggle()
        }
        return
    }

    Invoke-Element -Element $Element
}

function Invoke-Element {
    <#
        Clicks a button without blocking. The UI Automation Invoke pattern waits for
        the click handler to return, so clicking a button that opens a modal dialog
        (ShowDialog) hangs the caller until the dialog is closed. Posting WM_COMMAND's
        BM_CLICK to the button's own window handle returns immediately instead.
    #>
    param(
        [Parameter(Mandatory = $true)]
        $Element
    )

    $windowHandle = $Element.Current.NativeWindowHandle
    if ($windowHandle -ne 0) {
        [SoundSwitch.DocCapture.Native]::PostMessage([IntPtr]$windowHandle, $Script:BmClick, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
        return
    }

    $invoke = $null
    if ($Element.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invoke)) {
        $invoke.Invoke()
        return
    }

    $legacy = $null
    if ($Element.TryGetCurrentPattern([System.Windows.Automation.LegacyIAccessiblePattern]::Pattern, [ref]$legacy)) {
        $legacy.DoDefaultAction()
        return
    }

    throw "The control '$($Element.Current.Name)' cannot be invoked."
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
    $settingsHandle = [IntPtr]::Zero
    $windowTitle = $null

    while ((Get-Date) -lt $deadline -and $settingsHandle -eq [IntPtr]::Zero) {
        foreach ($candidate in @(Get-TopLevelWindows -ProcessId $process.Id)) {
            if (Select-SettingsTab -Handle $candidate.Handle -TabName $Tab) {
                $settingsHandle = $candidate.Handle
                $windowTitle = $candidate.Title
                break
            }
        }

        if ($settingsHandle -eq [IntPtr]::Zero) {
            Start-Sleep -Milliseconds 300
        }
    }

    if ($settingsHandle -eq [IntPtr]::Zero) {
        $tabHint = if ($Tab) { " with the '$Tab' tab" } else { '' }
        throw "Settings window$tabHint was not found within $TimeoutSeconds seconds."
    }

    $captureHandle = $settingsHandle
    $captureLabel = if ($Tab) { "the '$Tab' tab" } else { 'the Settings window' }

    if ($Click) {
        # Remember what is already on screen so the dialog opened by the click can be
        # told apart from the Settings window that owns it.
        $existingHandles = @(Get-TopLevelWindows -ProcessId $process.Id | ForEach-Object { $_.Handle.ToInt64() })

        $button = Find-VisibleButton -Handle $settingsHandle -ButtonId $Click
        if ($null -eq $button) {
            throw "Button '$Click' was not found in the Settings window."
        }

        Write-Host "Clicking '$Click'..." -ForegroundColor Cyan
        Invoke-Element -Element $button

        $dialogHandle = [IntPtr]::Zero
        $dialogDeadline = (Get-Date).AddSeconds($TimeoutSeconds)
        while ((Get-Date) -lt $dialogDeadline -and $dialogHandle -eq [IntPtr]::Zero) {
            foreach ($candidate in @(Get-TopLevelWindows -ProcessId $process.Id)) {
                if ($existingHandles -notcontains $candidate.Handle.ToInt64()) {
                    $dialogHandle = $candidate.Handle
                    $captureLabel = "'$($candidate.Title)' dialog"
                    break
                }
            }

            if ($dialogHandle -eq [IntPtr]::Zero) {
                Start-Sleep -Milliseconds 250
            }
        }

        if ($dialogHandle -eq [IntPtr]::Zero) {
            throw "No window opened after clicking '$Click' within $TimeoutSeconds seconds."
        }

        if ($Select) {
            $control = Find-VisibleControl -Handle $dialogHandle -ControlId $Select
            if ($null -eq $control) {
                throw "Control '$Select' was not found in the dialog opened by '$Click'."
            }

            Write-Host "Selecting '$Select'..." -ForegroundColor Cyan
            Select-Element -Element $control
            Start-Sleep -Milliseconds 200
        }

        $captureHandle = $dialogHandle
    }

    Write-Host "Capturing $captureLabel..." -ForegroundColor Cyan

    Start-Sleep -Milliseconds $RenderedDelayMilliseconds

    if (-not $OutputDirectory) {
        $OutputDirectory = Join-Path $RepoRoot 'img\Documentation'
    }

    $fileName = if ([System.IO.Path]::GetExtension($Name) -eq '.png') { $Name } else { "$Name.png" }
    $targetPath = if ([System.IO.Path]::IsPathRooted($fileName)) { $fileName } else { Join-Path $OutputDirectory $fileName }

    $result = Save-WindowScreenshot -Handle $captureHandle -Path $targetPath
    Write-Host "Saved $($result.Path) ($($result.Width)x$($result.Height))." -ForegroundColor Green

    if (-not $KeepOpen) {
        if ($captureHandle -ne $settingsHandle) {
            [SoundSwitch.DocCapture.Native]::PostMessage($captureHandle, $Script:WmClose, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
            Start-Sleep -Milliseconds 300
        }

        [SoundSwitch.DocCapture.Native]::PostMessage($settingsHandle, $Script:WmClose, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
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
