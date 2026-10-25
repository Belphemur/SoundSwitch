#Requires -Version 7.0
<#
.SYNOPSIS
    Run and control a SoundSwitch instance for end-to-end testing.

.DESCRIPTION
    A general-purpose control surface for a live SoundSwitch instance, built on the
    same primitives as Capture-DocumentationScreenshot.ps1 (public CLI/IPC surface,
    UI Automation, PrintWindow) but for testing instead of documentation capture:

      1. Builds the app and CLI (unless -NoBuild) and launches SoundSwitch.exe with
         --disable-updater, or attaches to an existing instance (-AllowExistingInstance).
      2. Sends CLI commands through the app's public IPC surface (mute, devices,
         profile, status, switch, settings) and reports the result.
      3. Opens the Settings window, selects tabs and clicks buttons with UI Automation.
      4. Captures any app window with PrintWindow into a PNG for verification.
      5. Stops the instance it launched (-Stop, or automatically unless -KeepRunning).

    Dot-source the script to get the control functions in your session and drive the
    app interactively:
        . .\tools\Control-SoundSwitch.ps1
        Start-SoundSwitchApp
        Invoke-SoundSwitchCli 'mute'
        Open-SoundSwitchSettings
        Select-SoundSwitchTab 'Recording'

.PARAMETER Start
    Build (unless -NoBuild), launch a fresh instance and wait until its IPC pipe
    answers. Refuses to run while another instance is active (one named pipe and
    one config file per instance) unless -AllowExistingInstance is given.

.PARAMETER Stop
    Stop the instance this tool launched (tracked via a PID file). Never touches an
    instance you started yourself unless -Force is also given.

.PARAMETER Cli
    CLI command line to send to the running instance, e.g. -Cli 'mute' or
    -Cli 'mute -t'. Launches/attaches automatically when none is running (unless
    -NoLaunch). Pass -Json for machine-readable output when supported.

.PARAMETER OpenSettings
    Open the Settings window via the CLI and wait for it to appear.

.PARAMETER Tab
    Settings tab to select after opening Settings (localized display name, e.g.
    "Recording", "Notifications"). Implies -OpenSettings.

.PARAMETER MuteTarget
    Entry to select in the "Mute microphone" combo box on the Recording tab, by
    visible label (e.g. "Windows default input" or a microphone name) or by
    zero-based index. Implies -OpenSettings and selects the Recording tab when
    needed. This drives the mute-target feature through the real UI.

.PARAMETER Click
    Accessible name or automation id of a button to click on the current Settings
    tab (e.g. addAppSoundRuleButton). Implies -OpenSettings; the dialog the button
    opens (not Settings) is what -Screenshot then captures.

.PARAMETER Screenshot
    Name of a PNG to capture from the active app window (Settings, or the dialog
    opened by -Click). Written under -OutputDirectory; rooted paths are kept.

.PARAMETER ListWindows
    List the visible top-level windows of the app process and exit.

.PARAMETER Status
    Print a short report (running instances, tracked PID, resolved binaries) and exit.

.PARAMETER Configuration
    Build configuration to run. Defaults to Debug.

.PARAMETER NoBuild
    Skip the build step and use the existing binaries.

.PARAMETER NoLaunch
    Never launch SoundSwitch; fail if none is running.

.PARAMETER AllowExistingInstance
    Allow attaching to and controlling an already running instance.

.PARAMETER Force
    With -Stop, also stop an instance this tool did not launch.

.PARAMETER KeepRunning
    Leave the app running after the requested action completes.

.PARAMETER Json
    Forward --json to CLI commands that support it (mute, devices, status...).

.PARAMETER OutputDirectory
    Directory -Screenshot PNGs are written to when not rooted. Defaults to
    <repo>\img\Documentation.

.PARAMETER TimeoutSeconds
    How long to wait for the app's IPC pipe and for windows to appear. Defaults to 60.

.EXAMPLE
    pwsh tools\Control-SoundSwitch.ps1 -Start
    Builds and launches SoundSwitch, waits for its IPC pipe.

.EXAMPLE
    pwsh tools\Control-SoundSwitch.ps1 -Cli 'mute'
    Reports the mute state of the configured mute target (or default input).

.EXAMPLE
    pwsh tools\Control-SoundSwitch.ps1 -Cli 'mute -t'
    Toggles mute on the configured target; banner behaviour can be observed live.

.EXAMPLE
    pwsh tools\Control-SoundSwitch.ps1 -Tab Recording -Screenshot mute-target-settings
    Opens Settings on the Recording tab and captures it to img\Documentation\mute-target-settings.png.

.EXAMPLE
    pwsh tools\Control-SoundSwitch.ps1 -Stop
    Stops the instance this tool launched.

.EXAMPLE
    . .\tools\Control-SoundSwitch.ps1; Start-SoundSwitchApp; Invoke-SoundSwitchCli 'mute'; Stop-SoundSwitchApp
    Interactive session using the exposed functions.
#>
[CmdletBinding()]
param(
    [switch]$Start,
    [switch]$Stop,
    [string]$Cli,
    [switch]$OpenSettings,
    [string]$Tab,
    [string]$MuteTarget,
    [string]$Click,
    [string]$Screenshot,
    [string]$OutputDirectory,
    [switch]$ListWindows,
    [switch]$Status,
    [switch]$Json,
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',
    [switch]$NoBuild,
    [switch]$NoLaunch,
    [switch]$AllowExistingInstance,
    [switch]$Force,
    [switch]$KeepRunning,
    [switch]$KeepOpen,
    [int]$TimeoutSeconds = 60
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path (Join-Path $RepoRoot 'SoundSwitch.sln'))) {
    throw "Could not locate the repository root from '$PSScriptRoot'."
}

# --- Win32 interop ------------------------------------------------------------------

if (-not ('SoundSwitch.AppControl.Native' -as [type])) {
    Add-Type -Language CSharp -TypeDefinition @'
namespace SoundSwitch.AppControl
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
$Script:PidFile = Join-Path $env:TEMP 'SoundSwitchControl\instance.pid'

# --- Process lifecycle --------------------------------------------------------------

function Get-SoundSwitchProcess {
    <# Running SoundSwitch processes, oldest first (the first instance owns the pipe). #>
    @(Get-Process -Name 'SoundSwitch' -ErrorAction SilentlyContinue | Sort-Object StartTime)
}

function Read-SoundSwitchControlPid {
    if (Test-Path $Script:PidFile) {
        return Get-Content $Script:PidFile -ErrorAction SilentlyContinue
    }
    return $null
}

function Start-SoundSwitchApp {
    <#
        Launches a fresh SoundSwitch.exe with --disable-updater and waits until its
        IPC pipe answers. Throws when another instance is already running (they
        share one named pipe and one config file) unless -AllowExistingInstance.
    #>
    param(
        [string]$AppExe,
        [string]$CliExe,
        [switch]$AllowExistingInstance,
        [int]$TimeoutSeconds = 60
    )

    $existing = @(Get-SoundSwitchProcess)
    if ($existing.Count -gt 0) {
        if (-not $AllowExistingInstance) {
            $ids = ($existing | ForEach-Object { $_.Id }) -join ', '
            throw "SoundSwitch is already running (PID: $ids). Close it first, or pass -AllowExistingInstance."
        }
        Write-Host "Attaching to the running SoundSwitch instance (PID $($existing[0].Id))." -ForegroundColor Yellow
        return $existing[0]
    }

    Write-Host "Starting $AppExe ..." -ForegroundColor Cyan
    $process = Start-Process -FilePath $AppExe -ArgumentList '--disable-updater' -PassThru

    # The IPC pipe is registered once the app is initialized; CLI 'status' is a
    # cheap readiness probe.
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    $ready = $false
    while ((Get-Date) -lt $deadline) {
        if ($process.HasExited) {
            throw "SoundSwitch exited immediately after start (exit code $($process.ExitCode))."
        }
        & $CliExe status *> $null
        if ($LASTEXITCODE -eq 0) {
            $ready = $true
            break
        }
        Start-Sleep -Milliseconds 500
    }

    if (-not $ready) {
        $process | Stop-Process -Force -ErrorAction SilentlyContinue
        throw "SoundSwitch did not become ready within $TimeoutSeconds seconds."
    }

    $directory = Split-Path -Parent $Script:PidFile
    if (-not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }
    Set-Content -Path $Script:PidFile -Value $process.Id

    Write-Host "SoundSwitch is ready (PID $($process.Id))." -ForegroundColor Green
    return $process
}

function Stop-SoundSwitchApp {
    <#
        Stops the instance this tool launched (tracked via the PID file). With -Force,
        also stops instances started outside this tool.
    #>
    param([switch]$Force)

    $trackedId = Read-SoundSwitchControlPid
    $processes = @(Get-SoundSwitchProcess)
    if ($processes.Count -eq 0) {
        Write-Host 'No SoundSwitch instance is running.' -ForegroundColor DarkGray
        if ($trackedId) { Remove-Item $Script:PidFile -Force -ErrorAction SilentlyContinue }
        return
    }

    foreach ($process in $processes) {
        $isTracked = $trackedId -eq $process.Id
        if (-not $isTracked -and -not $Force) {
            Write-Host "Leaving instance PID $($process.Id) running (not started by this tool; use -Force to stop it)." -ForegroundColor Yellow
            continue
        }

        if ($isTracked) {
            $process | Wait-Process -Timeout 5 -ErrorAction SilentlyContinue
        }
        if (-not $process.HasExited) {
            $process | Stop-Process -Force -ErrorAction SilentlyContinue
            Write-Host "Stopped SoundSwitch PID $($process.Id)$($(if ($isTracked) { ' (launched by this tool)' } else { '' }))." -ForegroundColor Green
        }
    }

    Remove-Item $Script:PidFile -Force -ErrorAction SilentlyContinue
}

# --- CLI / IPC ----------------------------------------------------------------------

function Resolve-SoundSwitchExecutable {
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

function Invoke-SoundSwitchCli {
    <#
        Sends a CLI command to the running app through its public IPC surface and
        returns the exit code. Output is streamed to the host; pass -Quiet to
        suppress it. CLI commands fail (exit code 1) when no instance is running.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$CliExe,

        [Parameter(Mandatory = $true)]
        [string]$Command,

        [string]$ExtraArguments = '',

        [switch]$Quiet
    )

    $arguments = $Command.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)
    if ($ExtraArguments) {
        $arguments += ($ExtraArguments.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries) | Where-Object { $_ })
    }

    if (-not $Quiet) {
        Write-Host "> $([System.IO.Path]::GetFileName($CliExe)) $($arguments -join ' ')" -ForegroundColor DarkCyan
    }

    $output = & $CliExe @arguments 2>&1
    $code = $LASTEXITCODE
    if (-not $Quiet -and $output) {
        $output | ForEach-Object { Write-Host $_ }
    }
    return $code
}

# --- UI Automation ------------------------------------------------------------------

function Get-SoundSwitchWindows {
    <#
        Visible, titled top-level windows owned by the given process. The Settings
        form is the only such window normally (the app has no "main" form); the
        message-pump and tray windows are hidden and title-less.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [int]$ProcessId
    )

    $windows = [System.Collections.Generic.List[object]]::new()
    $callback = [SoundSwitch.AppControl.Native+EnumWindowsProc] {
        param([IntPtr]$handle, [IntPtr]$lParam)

        if ([SoundSwitch.AppControl.Native]::IsWindowVisible($handle) -and
            [SoundSwitch.AppControl.Native]::GetWindowTextLength($handle) -gt 0) {
            [uint32]$windowProcessId = 0
            [SoundSwitch.AppControl.Native]::GetWindowThreadProcessId($handle, [ref]$windowProcessId) | Out-Null
            if ([int]$windowProcessId -eq $ProcessId) {
                $title = [System.Text.StringBuilder]::new(512)
                [SoundSwitch.AppControl.Native]::GetWindowText($handle, $title, $title.Capacity) | Out-Null
                $windows.Add([pscustomobject]@{ Handle = $handle; Title = $title.ToString() })
            }
        }

        return $true
    }

    [SoundSwitch.AppControl.Native]::EnumWindows($callback, [IntPtr]::Zero) | Out-Null
    return $windows
}

function Select-SoundSwitchTab {
    <#
        Selects the tab with the given name inside a window and returns $true when
        the window exposes a tab control (and the requested tab was found, or no
        tab name was given). Returns $false for windows without tabs.
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

function Find-SoundSwitchButton {
    <#
        Finds a button inside the window by accessible name or automation id,
        preferring the one actually on screen (the same label can exist on several
        tab pages).
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

function Invoke-SoundSwitchElement {
    <#
        Clicks a control without blocking. The UI Automation Invoke pattern waits
        for the click handler to return, so clicking a button that opens a modal
        dialog hangs the caller until the dialog is closed. Posting WM_COMMAND's
        BM_CLICK to the button's own window handle returns immediately instead.
    #>
    param(
        [Parameter(Mandatory = $true)]
        $Element
    )

    $windowHandle = $Element.Current.NativeWindowHandle
    if ($windowHandle -ne 0) {
        [SoundSwitch.AppControl.Native]::PostMessage([IntPtr]$windowHandle, $Script:BmClick, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
        return
    }

    $invoke = $null
    if ($Element.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invoke)) {
        $invoke.Invoke()
        return
    }

    throw "The control '$($Element.Current.Name)' cannot be invoked."
}

function Set-SoundSwitchMuteTarget {
    <#
        Selects an entry in the "Mute microphone" combo box on the Recording tab
        through UI Automation. -Target is either the visible label (e.g.
        "Windows default input" or a microphone name) or a zero-based index.
        Selects the Recording tab automatically when the combo box is not
        discoverable on the currently visible page.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [IntPtr]$WindowHandle,

        [Parameter(Mandatory = $true)]
        [string]$Target,

        [int]$TimeoutSeconds = 30
    )

    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes

    $window = [System.Windows.Automation.AutomationElement]::FromHandle($WindowHandle)
    if ($null -eq $window) {
        throw 'Could not attach UI Automation to the Settings window.'
    }

    $comboCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::AutomationIdProperty, 'muteTargetComboBox')

    $combo = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $comboCondition)
    if ($null -eq $combo) {
        # The combo lives on the Recording tab: select each tab until it shows up.
        $tabCondition = [System.Windows.Automation.PropertyCondition]::new(
            [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
            [System.Windows.Automation.ControlType]::TabItem)
        foreach ($tab in $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, $tabCondition)) {
            $selection = $tab.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
            $selection.Select()
            Start-Sleep -Milliseconds 150
            $combo = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $comboCondition)
            if ($null -ne $combo) {
                break
            }
        }
    }

    if ($null -eq $combo) {
        throw "The 'Mute microphone' combo box (muteTargetComboBox) was not found in the Settings window."
    }

    $expand = $combo.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
    $expand.Expand()

    $itemCondition = [System.Windows.Automation.PropertyCondition]::new(
        [System.Windows.Automation.AutomationElement]::ControlTypeProperty,
        [System.Windows.Automation.ControlType]::ListItem)

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    $items = @()
    while ((Get-Date) -lt $deadline) {
        $items = @($combo.FindAll([System.Windows.Automation.TreeScope]::Descendants, $itemCondition))
        if ($items.Count -gt 0) { break }
        Start-Sleep -Milliseconds 100
    }

    if ($items.Count -eq 0) {
        $expand.Collapse()
        throw "The 'Mute microphone' list did not open."
    }

    $chosen = $null
    if ($Target -match '^\d+$') {
        $index = [int]$Target
        if ($index -ge 0 -and $index -lt $items.Count) { $chosen = $items[$index] }
    }
    else {
        $chosen = $items | Where-Object { $_.Current.Name -eq $Target } | Select-Object -First 1
        if ($null -eq $chosen) {
            # Fall back to a case-insensitive / trimmed comparison.
            $chosen = $items | Where-Object { $_.Current.Name.Trim() -ieq $Target.Trim() } | Select-Object -First 1
        }
    }

    if ($null -eq $chosen) {
        $available = ($items | ForEach-Object { $_.Current.Name }) -join ' | '
        $expand.Collapse()
        throw "Mute target '$Target' was not found. Available: $available"
    }

    $chosen.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Milliseconds 150
    try { $expand.Collapse() } catch { }

    return $chosen.Current.Name
}

function Save-SoundSwitchScreenshot {
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

    if ([SoundSwitch.AppControl.Native]::IsIconic($Handle)) {
        [SoundSwitch.AppControl.Native]::ShowWindow($Handle, $Script:SwRestore) | Out-Null
    }

    [SoundSwitch.AppControl.Native]::SetForegroundWindow($Handle) | Out-Null
    Start-Sleep -Milliseconds 150

    $rect = [SoundSwitch.AppControl.Native+RECT]::new()
    if (-not [SoundSwitch.AppControl.Native]::GetWindowRect($Handle, [ref]$rect)) {
        throw 'GetWindowRect failed for the target window.'
    }

    $width = $rect.Right - $rect.Left
    $height = $rect.Bottom - $rect.Top
    if ($width -le 0 -or $height -le 0) {
        throw "Unexpected window size: ${width}x${height}."
    }

    Add-Type -AssemblyName System.Drawing

    $bitmap = [System.Drawing.Bitmap]::new($width, $height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $hdc = $graphics.GetHdc()
    $captured = $false
    try {
        $captured = [SoundSwitch.AppControl.Native]::PrintWindow($Handle, $hdc, $Script:PwRenderFullContent)
    }
    finally {
        $graphics.ReleaseHdc($hdc)
        $graphics.Dispose()
    }

    if (-not $captured) {
        $bitmap.Dispose()
        throw 'PrintWindow failed to render the target window.'
    }

    $directory = Split-Path -Parent $Path
    if ($directory -and -not (Test-Path $directory)) {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
    }

    $bitmap.Save($Path, [System.Drawing.Imaging.ImageFormat]::Png)
    $bitmap.Dispose()

    return [pscustomobject]@{ Path = $Path; Width = $width; Height = $height }
}

# --- Orchestration helpers ----------------------------------------------------------

function Open-SoundSwitchSettings {
    <#
        Opens the Settings window via the CLI (public IPC) and waits for a window
        with a tab control to appear. Returns the window info of Settings.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [string]$CliExe,

        [Parameter(Mandatory = $true)]
        [int]$ProcessId,

        [int]$TimeoutSeconds = 60
    )

    $opened = $false
    for ($attempt = 1; $attempt -le 20 -and -not $opened; $attempt++) {
        & $CliExe settings *> $null
        if ($LASTEXITCODE -eq 0) {
            $opened = $true
        }
        else {
            Start-Sleep -Milliseconds 500
        }
    }

    if (-not $opened) {
        throw "Failed to open Settings via '$(Split-Path -Leaf $CliExe) settings'."
    }

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        foreach ($candidate in @(Get-SoundSwitchWindows -ProcessId $ProcessId)) {
            if (Select-SoundSwitchTab -Handle $candidate.Handle) {
                return $candidate
            }
        }
        Start-Sleep -Milliseconds 300
    }

    throw "The Settings window did not appear within $TimeoutSeconds seconds."
}

function Wait-SoundSwitchNewWindow {
    <#
        Waits for a top-level window of the process that was not in $knownHandles
        (e.g. a dialog opened by a button click) and returns it.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [int]$ProcessId,

        [Parameter(Mandatory = $true)]
        [long[]]$KnownHandles,

        [int]$TimeoutSeconds = 60
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        foreach ($candidate in @(Get-SoundSwitchWindows -ProcessId $ProcessId)) {
            if ($KnownHandles -notcontains $candidate.Handle.ToInt64()) {
                return $candidate
            }
        }
        Start-Sleep -Milliseconds 250
    }

    throw "No new window appeared within $TimeoutSeconds seconds."
}

# --- Mode detection: dot-sourced = library mode, otherwise run the requested action --

$Script:IsDotSourced = $MyInvocation.InvocationName -ne $MyInvocation.MyCommand.Name -and
    $MyInvocation.InvocationName -notmatch '\\'
$hasAction = $Start -or $Stop -or $Cli -or $OpenSettings -or $Tab -or $MuteTarget -or $Click -or $Screenshot -or $ListWindows -or $Status

if ($Script:IsDotSourced -and -not $hasAction) {
    # Library mode: expose the functions for interactive control. Nothing to run.
    return
}

if (-not $hasAction) {
    Write-Host 'No action requested. Pass -Start, -Stop, -Cli, -OpenSettings, -Tab, -Click, -Screenshot, -ListWindows or -Status,' -ForegroundColor Yellow
    Write-Host 'or dot-source this script to use the control functions interactively:' -ForegroundColor Yellow
    Write-Host '    . .\tools\Control-SoundSwitch.ps1; Start-SoundSwitchApp; Invoke-SoundSwitchCli ''mute''' -ForegroundColor Yellow
    return
}

# --- One-shot action mode -----------------------------------------------------------

if ($Status) {
    $existing = @(Get-SoundSwitchProcess)
    $trackedId = Read-SoundSwitchControlPid
    Write-Host 'SoundSwitch instances:' -ForegroundColor Cyan
    if ($existing.Count -eq 0) {
        Write-Host '  (none running)'
    }
    else {
        foreach ($instance in $existing) {
            $marker = if ($instance.Id -eq $trackedId) { ' [launched by this tool]' } else { '' }
            Write-Host ("  PID {0}{1} (start time {2})" -f $instance.Id, $marker, $instance.StartTime)
        }
    }
    $appExe = Resolve-SoundSwitchExecutable -ProjectDirectory 'SoundSwitch' -FileName 'SoundSwitch.exe'
    $cliExe = Resolve-SoundSwitchExecutable -ProjectDirectory 'SoundSwitch.CLI' -FileName 'SoundSwitch.CLI.exe'
    Write-Host ("App binary: {0}" -f ($(if ($appExe) { $appExe.FullName } else { 'NOT FOUND (build first)' })))
    Write-Host ("CLI binary: {0}" -f ($(if ($cliExe) { $cliExe.FullName } else { 'NOT FOUND (build first)' })))
    return
}

# -Stop needs no build and no running instance of ours; handle it first.
if ($Stop) {
    Stop-SoundSwitchApp -Force:$Force
    return
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

$appExe = Resolve-SoundSwitchExecutable -ProjectDirectory 'SoundSwitch' -FileName 'SoundSwitch.exe'
$cliExe = Resolve-SoundSwitchExecutable -ProjectDirectory 'SoundSwitch.CLI' -FileName 'SoundSwitch.CLI.exe'
if (-not $appExe) {
    throw "SoundSwitch.exe not found under SoundSwitch\bin\$Configuration. Build first or drop -NoBuild."
}
if (-not $cliExe) {
    throw "SoundSwitch.CLI.exe not found under SoundSwitch.CLI\bin\$Configuration. Build first or drop -NoBuild."
}

# -Start launches and reports, leaving the instance running so follow-up calls can
# attach to it.
if ($Start) {
    $started = Start-SoundSwitchApp -AppExe $appExe.FullName -CliExe $cliExe.FullName -AllowExistingInstance:$AllowExistingInstance -TimeoutSeconds $TimeoutSeconds
    Write-Host "SoundSwitch PID $($started.Id) is running. Use -Cli, -Tab, -Screenshot or -ListWindows to control it, and -Stop to stop it." -ForegroundColor Green
    return
}

# Actions below need a live instance; -Cli and window actions imply a launch when
# none is running (unless -NoLaunch).
$needsInstance = $Cli -or $OpenSettings -or $Tab -or $MuteTarget -or $Click -or $Screenshot -or $ListWindows
$launchedThisRun = $false
if ($needsInstance) {
    $existing = @(Get-SoundSwitchProcess)
    if ($existing.Count -eq 0) {
        if ($NoLaunch) {
            throw 'No running SoundSwitch instance and -NoLaunch was specified.'
        }
        $null = Start-SoundSwitchApp -AppExe $appExe.FullName -CliExe $cliExe.FullName -AllowExistingInstance:$AllowExistingInstance -TimeoutSeconds $TimeoutSeconds
        $launchedThisRun = $true
    }
    elseif ($existing.Count -gt 1) {
        Write-Warning "Multiple SoundSwitch instances are running ($(($existing | ForEach-Object Id) -join ', ')); using the first (it owns the IPC pipe)."
    }
}

# Strict mode throws on out-of-bounds indexing, so never index the process array
# directly; Select-Object -First 1 yields $null safely when none is running.
$process = Get-SoundSwitchProcess | Select-Object -First 1
if (-not $process) {
    throw 'No SoundSwitch instance is available to control.'
}

try {
    if ($ListWindows) {
        $windows = @(Get-SoundSwitchWindows -ProcessId $process.Id)
        if ($windows.Count -eq 0) {
            Write-Host 'No visible top-level windows (Settings is closed; the tray and message-pump windows are hidden).' -ForegroundColor DarkGray
        }
        else {
            $windows | Format-Table -AutoSize
        }
        return
    }

    if ($Cli) {
        $extraArgs = $(if ($Json) { '--json' } else { '' })
        $code = Invoke-SoundSwitchCli -CliExe $cliExe.FullName -Command $Cli -ExtraArguments $extraArgs
        if ($code -ne 0) {
            throw "CLI command '$Cli' failed with exit code $code."
        }
        return
    }

    # Settings flow: open, select tab, optionally click a button, optionally capture.
    $settings = $null
    if ($OpenSettings -or $Tab -or $MuteTarget -or $Click -or $Screenshot) {
        Write-Host 'Opening Settings...' -ForegroundColor Cyan
        $settings = Open-SoundSwitchSettings -CliExe $cliExe.FullName -ProcessId $process.Id -TimeoutSeconds $TimeoutSeconds
    }

    if ($Tab -and $settings) {
        if (-not (Select-SoundSwitchTab -Handle $settings.Handle -TabName $Tab)) {
            throw "The Settings window does not have a '$Tab' tab."
        }
        Write-Host "Selected the '$Tab' tab." -ForegroundColor Cyan
    }

    if ($MuteTarget -and $settings) {
        $selected = Set-SoundSwitchMuteTarget -WindowHandle $settings.Handle -Target $MuteTarget -TimeoutSeconds $TimeoutSeconds
        Write-Host "Mute microphone set to '$selected'." -ForegroundColor Green
    }

    # Snapshot the open windows BEFORE any click so the dialog the button opens can
    # be told apart from windows that were already on screen.
    $preClickHandles = @()

    if ($Click -and $settings) {
        $preClickHandles = @(Get-SoundSwitchWindows -ProcessId $process.Id | ForEach-Object { $_.Handle.ToInt64() })

        $button = Find-SoundSwitchButton -Handle $settings.Handle -ButtonId $Click
        if ($null -eq $button) {
            throw "Button '$Click' was not found in the Settings window."
        }

        Write-Host "Clicking '$Click'..." -ForegroundColor Cyan
        Invoke-SoundSwitchElement -Element $button

        $null = Wait-SoundSwitchNewWindow -ProcessId $process.Id -KnownHandles $preClickHandles -TimeoutSeconds $TimeoutSeconds
    }

    if ($Screenshot) {
        $dialog = $null
        if ($Click) {
            try {
                $dialog = Wait-SoundSwitchNewWindow -ProcessId $process.Id -KnownHandles $preClickHandles -TimeoutSeconds 5
            }
            catch {
                # The dialog may have closed already; fall back to Settings.
            }
        }

        $target = $(if ($dialog) { $dialog } else { $settings })
        $fileName = if ([System.IO.Path]::GetExtension($Screenshot) -eq '.png') { $Screenshot } else { "$Screenshot.png" }
        $path = if ([System.IO.Path]::IsPathRooted($fileName)) { $fileName } else {
            $dir = if ($OutputDirectory) { $OutputDirectory } else { Join-Path $RepoRoot 'img\Documentation' }
            Join-Path $dir $fileName
        }

        Start-Sleep -Milliseconds 500   # let the UI settle after tab/click
        $result = Save-SoundSwitchScreenshot -Handle $target.Handle -Path $path
        Write-Host "Saved $($result.Path) ($($result.Width)x$($result.Height))." -ForegroundColor Green

        if ($dialog) {
            [SoundSwitch.AppControl.Native]::PostMessage($dialog.Handle, $Script:WmClose, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
            Start-Sleep -Milliseconds 300
        }
    }

    if ($settings -and -not $KeepRunning -and -not $KeepOpen) {
        [SoundSwitch.AppControl.Native]::PostMessage($settings.Handle, $Script:WmClose, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
    }
}
finally {
    # Only tear down an instance this invocation started; an instance launched by an
    # earlier -Start (or by the user) is left running for the next control call.
    if ($launchedThisRun -and -not $KeepRunning) {
        Stop-SoundSwitchApp
    }
}
