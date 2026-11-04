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

.PARAMETER ThenClick
    Accessible name or automation id of a button to click inside the dialog opened
    by -Click, after -Select is applied. The window that button opens (a
    second-level dialog, e.g. the "Select a process" picker opened from the
    "Add Rule" dialog) is captured instead. Requires -Click.

.PARAMETER ThenClickRow
    Text of a cell in a row to click inside the dialog opened by -Click, after
    -ThenClick is applied (e.g. the collapsible "Show all processes" band of the
    process picker, which has no button to click). The first row with a cell
    containing the text is activated with a simulated mouse click. Requires -Click.

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
    .\tools\Capture-DocumentationScreenshot.ps1 -Name "AppRules Add Hotkey" -Tab "App Rules" -Click addAppSoundRuleButton -Select rdoHotkey
    Opens the App Rules tab, clicks Add, selects the Hotkey mode and captures the "Add Rule" dialog.

.EXAMPLE
    .\tools\Capture-DocumentationScreenshot.ps1 -Name "AppRules Process Picker" -Tab "App Rules" -Click addAppSoundRuleButton -ThenClick btnSelectProcess
    Opens the "Add Rule" dialog, then the "Select a process" picker inside it, and captures the picker.

.EXAMPLE
    .\tools\Capture-DocumentationScreenshot.ps1 -Name "AppRules Process Picker" -Tab "App Rules" -Click addAppSoundRuleButton -ThenClick btnSelectProcess -ThenClickRow "Show all processes"
    Same as above, and additionally activates the collapsible "Show all processes" band so the full process list is visible in the capture.

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

    [string]$ThenClick,

    [string]$ThenClickRow,

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

if ($ThenClick -and -not $Click) {
    throw '-ThenClick requires -Click: it opens a second-level dialog from that dialog.'
}

if ($ThenClickRow -and -not $Click) {
    throw '-ThenClickRow requires -Click: it activates a row inside that dialog.'
}

if ($ThenClickRow -and -not $ThenClick) {
    # Without -ThenClick the flow never resolves the picker's window handle ($dialogHandle
    # stays on the rule editor, or the auto-opened picker is never identified), so the row
    # search would run against the wrong dialog. Give the correction logic the inner dialog
    # it needs.
    throw '-ThenClickRow requires -ThenClick: the row is activated inside the dialog that -ThenClick opens.'
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

        [DllImport("user32.dll")]
        public static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, UIntPtr dwExtraInfo);

        public struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT point);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);
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

function Invoke-ButtonAndFindDialog {
    <#
        Clicks a button and waits until a top-level window of the process appears
        whose handle is not in the given set. Returns the new handle and title.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [int]$ProcessId,

        [Parameter(Mandatory = $true)]
        $Button,

        [Parameter(Mandatory = $true)]
        [string]$ButtonId,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [long[]]$KnownHandles,

        [int]$TimeoutSeconds = 60,

        [int]$WaitSeconds = 0
    )

    Write-Host "Clicking '$ButtonId'..." -ForegroundColor Cyan
    Invoke-Element -Element $Button

    $dialogHandle = [IntPtr]::Zero
    $dialogTitle = $null
    $waitSeconds = if ($WaitSeconds -gt 0) { $WaitSeconds } else { $TimeoutSeconds }
    $dialogDeadline = (Get-Date).AddSeconds($waitSeconds)
    while ((Get-Date) -lt $dialogDeadline -and $dialogHandle -eq [IntPtr]::Zero) {
        foreach ($candidate in @(Get-TopLevelWindows -ProcessId $ProcessId)) {
            if ($KnownHandles -notcontains $candidate.Handle.ToInt64()) {
                $dialogHandle = $candidate.Handle
                $dialogTitle = $candidate.Title
                break
            }
        }

        if ($dialogHandle -eq [IntPtr]::Zero) {
            Start-Sleep -Milliseconds 250
        }
    }

    if ($dialogHandle -eq [IntPtr]::Zero) {
        throw "No window opened after clicking '$ButtonId' within $waitSeconds seconds."
    }

    return [pscustomobject]@{ Handle = $dialogHandle; Title = $dialogTitle }
}

function Select-DialogRow {
    <#
        Activates the row whose cell text contains the given text inside the
        window (e.g. the picker's collapsible "Show all processes" band, which
        has no button whose BM_CLICK can be posted). WinForms surfaces the
        DataGridView rows with evolving control types (Custom/DataItem, and
        even Pane on other framework builds), so the search is deliberately
        type-agnostic: every descendant element whose value carries the text is
        a candidate.

        The click only happens when the row is inside the dialog's visible
        bounds — an off-viewport row would silently take the click elsewhere.

        The dialog is pushed to the top of the z-order first: modal sub-dialogs
        of a non-foreground Settings window stay behind whichever window covers
        them, and a cursor click would land on that window instead of the row.
    #>
    param(
        [Parameter(Mandatory = $true)]
        [IntPtr]$Handle,

        [Parameter(Mandatory = $true)]
        [string]$RowText
    )

    Add-Type -AssemblyName UIAutomationClient
    Add-Type -AssemblyName UIAutomationTypes

    # HWND_TOPMOST; cleared again after the capture (see the SetWindowPos call below),
    # because when the picker is kept open (-KeepOpen/-KeepRunning) it would otherwise
    # remain above every other window.
    [SoundSwitch.DocCapture.Native]::SetWindowPos($Handle, [IntPtr](-1), 0, 0, 0, 0, 0x0043) | Out-Null # SWP_NOMOVE|SWP_NOSIZE|SWP_SHOWWINDOW
    [SoundSwitch.DocCapture.Native]::SetForegroundWindow($Handle) | Out-Null
    Start-Sleep -Milliseconds 200

    $window = [System.Windows.Automation.AutomationElement]::FromHandle($Handle)
    if ($null -eq $window) {
        throw "Window is not reachable through UI Automation."
    }

    $windowRect = [SoundSwitch.DocCapture.Native+RECT]::new()
    if (-not [SoundSwitch.DocCapture.Native]::GetWindowRect($Handle, [ref]$windowRect)) {
        throw 'GetWindowRect failed for the dialog.'
    }

    $candidates = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    foreach ($element in $candidates) {
        $valuePattern = $null
        if (-not $element.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$valuePattern)) {
            continue
        }

        $text = $valuePattern.Current.Value
        if ([string]::IsNullOrEmpty($text) -or $text.IndexOf($RowText, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
            continue
        }

        $rect = $element.Current.BoundingRectangle
        $clickX = [int]($rect.X + $rect.Width / 2)
        $clickY = [int]($rect.Y + $rect.Height / 2)
        if ($rect.IsEmpty -or
            $clickX -lt $windowRect.Left -or $clickX -ge $windowRect.Right -or
            $clickY -lt $windowRect.Top -or $clickY -ge $windowRect.Bottom)
        {
            # The row is off the dialog's viewport (e.g. a counterpart band on
            # an underlying screen): clicking it would miss. Fail loudly instead
            # of logging success and saving the collapsed state.
            throw "Row with cell text '$text' is outside the dialog's visible bounds; scroll it into view first."
        }

        [SoundSwitch.DocCapture.Native]::SetCursorPos($clickX, $clickY) | Out-Null
        Start-Sleep -Milliseconds 150
        [SoundSwitch.DocCapture.Native]::mouse_event(0x0002, 0, 0, 0, [UIntPtr]::Zero) # left down
        Start-Sleep -Milliseconds 60
        [SoundSwitch.DocCapture.Native]::mouse_event(0x0004, 0, 0, 0, [UIntPtr]::Zero) # left up
        Write-Host "Clicked row with cell text '$text'..." -ForegroundColor Cyan
        return
    }

    throw "No row with cell text '$RowText' was found in the dialog."
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

$captureHandle = [IntPtr]::Zero
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
    # Second-level dialog handles opened along the way, closed during cleanup (inner first).
    $openedDialogHandles = [System.Collections.Generic.List[long]]::new()

    if ($Click) {
        # Remember what is already on screen so the dialog opened by the click can be
        # told apart from the Settings window that owns it.
        $existingHandles = @(Get-TopLevelWindows -ProcessId $process.Id | ForEach-Object { $_.Handle.ToInt64() })

        $button = Find-VisibleButton -Handle $settingsHandle -ButtonId $Click
        if ($null -eq $button) {
            throw "Button '$Click' was not found in the Settings window."
        }

        $dialog = Invoke-ButtonAndFindDialog -ProcessId $process.Id -Button $button -ButtonId $Click -KnownHandles $existingHandles -TimeoutSeconds $TimeoutSeconds
        $dialogHandle = $dialog.Handle
        $openedDialogHandles.Add($dialogHandle.ToInt64())
        $captureLabel = "'$($dialog.Title)' dialog"

        if ($Select -or $ThenClick) {
            # Rule editors auto-open their first child dialog as well (the rule editor
            # pops its process picker right after opening). Whichever window the click
            # detection grabbed, the editor is the one carrying the requested controls:
            # give the auto-opened child a moment, look at every other window of that
            # click, prefer the one carrying the target and close the extra copies.
            Start-Sleep -Milliseconds 600
            foreach ($candidate in @(Get-TopLevelWindows -ProcessId $process.Id)) {
                if ($candidate.Handle.ToInt64() -in $existingHandles -or $candidate.Handle -eq $dialogHandle) { continue }
                $target = if ($Select) { Find-VisibleControl -Handle $candidate.Handle -ControlId $Select } else { Find-VisibleButton -Handle $candidate.Handle -ButtonId $ThenClick }
                if ($null -ne $target) {
                    Write-Host "Preferring '$($candidate.Title)' (carries the requested control), closing the extra dialog..." -ForegroundColor Yellow
                    [SoundSwitch.DocCapture.Native]::PostMessage($dialogHandle, $Script:WmClose, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
                    Start-Sleep -Milliseconds 300
                    $dialogHandle = $candidate.Handle
                    $captureLabel = "'$($candidate.Title)' dialog"
                    break
                }
            }
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

        if ($ThenClick) {
            # A second-level dialog (e.g. the process picker opened from the rule
            # dialog): click inside the dialog and capture what that button opens.
            $existingDialogHandles = @(Get-TopLevelWindows -ProcessId $process.Id | ForEach-Object { $_.Handle.ToInt64() })

            $innerButton = Find-VisibleButton -Handle $dialogHandle -ButtonId $ThenClick
            if ($null -eq $innerButton) {
                throw "Button '$ThenClick' was not found in the dialog opened by '$Click'."
            }

            try {
                $innerDialog = Invoke-ButtonAndFindDialog -ProcessId $process.Id -Button $innerButton -ButtonId $ThenClick -KnownHandles $existingDialogHandles -TimeoutSeconds $TimeoutSeconds -WaitSeconds 10
            }
            catch {
                # The dialog may already be open (the Add-rule flow auto-opens its process
                # picker from OnShown, and the editor refuses to stack a second one) =>
                # reuse the already-open dialog instead of waiting for a new window. Only a
                # candidate that is not the just-clicked editor (which still carries the
                # '$ThenClick' button) and not Settings qualifies; anything else is another
                # unrelated window that must not be captured.
                $knownSecondLevel = @(Get-TopLevelWindows -ProcessId $process.Id | Where-Object {
                    $_.Handle.ToInt64() -in $existingDialogHandles -and
                    $_.Handle.ToInt64() -ne $dialogHandle -and
                    $_.Handle.ToInt64() -ne $settingsHandle.ToInt64() -and
                    $null -eq (Find-VisibleButton -Handle $_.Handle -ButtonId $ThenClick)
                })
                # Nothing usable was already open.
                if ($knownSecondLevel.Count -eq 0) {
                    throw "No window opened after clicking '$ThenClick' and none were already open: $_"
                }

                $innerDialog = [pscustomobject]@{ Handle = $knownSecondLevel[0].Handle; Title = $knownSecondLevel[0].Title }
                Write-Host "Reusing the already-open dialog '$($knownSecondLevel[0].Title)'..." -ForegroundColor Yellow
            }

            $dialogHandle = $innerDialog.Handle
            $openedDialogHandles.Add($dialogHandle.ToInt64())
            $captureLabel = "'$($innerDialog.Title)' dialog"
        }

        if ($ThenClickRow) {
            Select-DialogRow -Handle $dialogHandle -RowText $ThenClickRow
            Start-Sleep -Milliseconds 300
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
        # Close the captured dialog and every intermediate second-level dialog (e.g. a rule
        # editor left between Settings and the picker), inner first, so a modal opener does
        # not keep Settings from closing.
        foreach ($openHandle in $openedDialogHandles) {
            if ($openHandle -eq $settingsHandle.ToInt64()) { continue }
            [SoundSwitch.DocCapture.Native]::PostMessage([IntPtr]$openHandle, $Script:WmClose, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
            Start-Sleep -Milliseconds 200
        }

        [SoundSwitch.DocCapture.Native]::PostMessage($settingsHandle, $Script:WmClose, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
    }
}
finally {
    if ($ThenClickRow -and $captureHandle -and $captureHandle -ne [IntPtr]::Zero) {
        # Select-DialogRow pushed the dialog to HWND_TOPMOST to make the click land on the
        # row. Clear that on every exit path (save failure, throw, -KeepOpen/-KeepRunning or an
        # existing instance) so a picker that stays open returns to the normal z-order.
        [SoundSwitch.DocCapture.Native]::SetWindowPos($captureHandle, [IntPtr](-2), 0, 0, 0, 0, 0x0043) | Out-Null # HWND_NOTOPMOST
    }

    if ($launched -and -not $KeepRunning) {
        $process | Wait-Process -Timeout 5 -ErrorAction SilentlyContinue
        if (-not $process.HasExited) {
            $process | Stop-Process -Force -ErrorAction SilentlyContinue
            Write-Host 'Stopped the SoundSwitch instance started by this script.' -ForegroundColor DarkGray
        }
    }
}
