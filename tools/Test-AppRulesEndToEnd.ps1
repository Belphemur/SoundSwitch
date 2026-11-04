#Requires -Version 7.0
<#
.SYNOPSIS
    End-to-end test of the App Rules identity-based assignment feature.

.DESCRIPTION
    Drives a real SoundSwitch instance through UI Automation, the CLI/IPC pipe and Windows'
    per-app persisted endpoint store:

      A. The rule editor exposes the "Enforced by" pair, Processes (dynamic) by default.
         Implemented guard: App assignment only enables with a concrete identity, and a
         window-title pattern disables it again (it is NOT "assignment greys the Window Title
         field"). Closed without saving.
      B. A scratch tone-playing exe is started; a rule is created through the UI with App
         assignment and a non-default playback device. Asserted: the config holds the rule
         and its ledger entry, Windows' store (HKCU\...\Multimedia\Audio\DefaultEndpoint)
         holds the engine-written record, and the list shows ONE row for the identity.
      C. The exe is closed, the rule deleted through the list UI. The rule leaves the config
         and its ledger claim turns pending (deferred restore-default, ADR §6.3). The exe is
         relaunched and the test polls with a deadline for the record and claim to clear.

    Requires an interactive Windows session, real audio endpoints and a briefly audible tone
    loop. On a headless box use the unit layer: pwsh tools\Test-AppRules.ps1.

    Teardown always runs: stops the scratch exe and only the SoundSwitch instance this test
    launched, then restores the config (skipped while an attached/kept instance runs — it
    would clobber the restore from memory), and deletes only the scratch exe's store records.

.PARAMETER NoBuild
    Skip the build step and use the existing binaries.

.PARAMETER AllowExistingInstance
    Attach to an already running SoundSwitch. The configuration restore is skipped while it
    runs; the backup path is printed for manual reapplication after stopping it.

.PARAMETER KeepRunning
    Leave the app running after the test. The configuration restore is skipped for the same
    reason; the backup path is printed.

.PARAMETER TimeoutSeconds
    Timeout for IPC readiness, window discovery, UI interactions and registry polling.
    Defaults to 90.

.EXAMPLE
    pwsh tools\Test-AppRulesEndToEnd.ps1
    Runs the full end-to-end App Rules assignment test.

.EXAMPLE
    pwsh tools\Test-AppRulesEndToEnd.ps1 -NoBuild -AllowExistingInstance
    Tests against an already-built installation and a running instance.
#>
[CmdletBinding()]
param(
    [switch]$NoBuild,
    [switch]$AllowExistingInstance,
    [switch]$KeepRunning,
    [int]$TimeoutSeconds = 90
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$RepoRoot      = Split-Path -Parent $PSScriptRoot
$ControlScript = Join-Path $PSScriptRoot 'Control-SoundSwitch.ps1'
$ConfigPath    = Join-Path $env:APPDATA 'SoundSwitch\SoundSwitchConfiguration.json'
$BackupPath    = Join-Path $env:TEMP 'SoundSwitchControl\SoundSwitchConfiguration.backup.json'
$StorePath     = 'Software\Microsoft\Multimedia\Audio\DefaultEndpoint'
$ScratchDir    = Join-Path $env:TEMP 'SoundSwitchAppRulesE2E'
$ScratchExe    = Join-Path $ScratchDir 'SampleAudioApp.exe'
$ScratchNeedle = 'SampleAudioApp'
$ScratchTone   = Join-Path $env:WINDIR 'Media\Alarm01.wav'

if (-not (Test-Path $ControlScript)) { throw "Control script not found: $ControlScript" }

# --- Assertions ---------------------------------------------------------------------

$script:Failures = 0
$script:Checks   = 0
$script:Skips    = 0

function Assert-True {
    param([Parameter(Mandatory)][bool]$Condition, [Parameter(Mandatory)][string]$Message)
    $script:Checks++
    if ($Condition) { Write-Host "  PASS  $Message" -ForegroundColor Green }
    else            { $script:Failures++; Write-Host "  FAIL  $Message" -ForegroundColor Red }
}

function Assert-Equal {
    param([Parameter(Mandatory)]$Expected, [Parameter(Mandatory)]$Actual, [Parameter(Mandatory)][string]$Message)
    Assert-True -Condition ("$Expected" -eq "$Actual") -Message "$Message (expected '$Expected', got '$Actual')"
}

function Skip-Check {
    param([Parameter(Mandatory)][string]$Message)
    $script:Skips++
    Write-Host "  SKIP  $Message" -ForegroundColor Yellow
}

# Probe until true or the deadline passes; never sleep a guessed readiness duration.
function Wait-Until {
    param([Parameter(Mandatory)][scriptblock]$Probe, [int]$Timeout = 30, [string]$Description = 'condition')
    $deadline = (Get-Date).AddSeconds($Timeout)
    while ((Get-Date) -lt $deadline) {
        if (& $Probe) { return $true }
        Start-Sleep -Milliseconds 300
    }
    Write-Verbose "Timed out waiting for: $Description"
    return $false
}

# --- Configuration helpers ----------------------------------------------------------

function Read-Config {
    if (-not (Test-Path $ConfigPath)) { return $null }
    return (Get-Content $ConfigPath -Raw | ConvertFrom-Json)
}

function Get-RulesOf {
    param($Config)
    if ($null -eq $Config -or $Config.PSObject.Properties.Name -notcontains 'AppSoundRules') { return @() }
    return @($Config.AppSoundRules)
}

function Get-LedgerOf {
    param($Config)
    if ($null -eq $Config -or $Config.PSObject.Properties.Name -notcontains 'AssignmentLedger') { return @() }
    return @($Config.AssignmentLedger | Where-Object { $_ -and ($_.PSObject.Properties.Name -contains 'ExePath') })
}

# NT-device path, drive path or bare name — anything ending in the scratch exe name.
function Test-ScratchIdentity {
    param([string]$Value)
    return -not [string]::IsNullOrEmpty($Value) -and ("$Value" -match 'SampleAudioApp(\.exe)?\s*$')
}

# --- Windows' per-app store (HKCU\...\Multimedia\Audio\DefaultEndpoint) --------------

function Get-StoreRecords {
    # One record per key: (Default) is the NT exe identity, <NNN>_000 values carry MMDEVAPI
    # tokens whose trailing interface guid gives the direction (render/capture).
    $records = @()
    $store = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($StorePath, $false)
    if ($null -eq $store) { return $records }
    try {
        foreach ($keyName in @($store.GetSubKeyNames())) {
            $key = $null
            try { $key = $store.OpenSubKey($keyName, $false) } catch { continue }
            if ($null -eq $key) { continue }
            try {
                $exePath = [string]$key.GetValue($null)
                if ([string]::IsNullOrEmpty($exePath)) { continue }
                $render = ''
                foreach ($name in @($key.GetValueNames())) {
                    if ($name -notmatch '^\d{3}_000$') { continue }
                    $value = [string]$key.GetValue($name)
                    if ($value -match '#\{?e6327cad') { $render = $value }
                }
                if ($render) {
                    $records += [pscustomobject]@{ Key = $keyName; ExePath = $exePath; RenderToken = $render }
                }
            }
            finally { $key.Close() }
        }
    }
    finally { $store.Close() }
    return $records
}

function Get-ScratchStoreRecord {
    return @(Get-StoreRecords | Where-Object { Test-ScratchIdentity $_.ExePath } | Select-Object -First 1)
}

function Remove-ScratchStoreRecords {
    # Only records named after the scratch exe (leftovers or this run's writes).
    $store = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($StorePath, $true)
    if ($null -eq $store) { return 0 }
    $removed = 0
    try {
        foreach ($keyName in @($store.GetSubKeyNames())) {
            $key = $null
            try { $key = $store.OpenSubKey($keyName, $false) } catch { continue }
            if ($null -eq $key) { continue }
            try {
                if (Test-ScratchIdentity ([string]$key.GetValue($null))) {
                    $key.Close(); $key = $null
                    $store.DeleteSubKeyTree($keyName, $false)
                    $removed++
                }
            }
            finally { if ($key) { $key.Close() } }
        }
    }
    finally { $store.Close() }
    return $removed
}

# --- Scratch audio app --------------------------------------------------------------

function New-ScratchApp {
    # Only a process with a live audio session can receive a persisted per-app route from
    # the engine, so the scratch app loops a Windows tone until it is stopped.
    New-Item -ItemType Directory -Path $ScratchDir -Force | Out-Null
    if (-not (Test-Path $ScratchTone)) { throw "A tone WAV for the scratch app was not found ($ScratchTone)." }

    $source = Join-Path $ScratchDir 'SampleAudioApp.cs'
    Set-Content -Path $source -Value @'
using System; using System.Media; using System.Threading;
public class SampleAudioApp {
    public static void Main(string[] args) {
        var tone = args.Length > 0 ? args[0] : "";
        for (;;) { try { new SoundPlayer(tone).PlaySync(); } catch { } try { Thread.Sleep(250); } catch { } }
    }
}
'@
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    if (-not (Test-Path $csc)) { throw "The .NET Framework compiler was not found at $csc." }
    & $csc /nologo /out:"$ScratchExe" /r:System.dll "$source" | ForEach-Object { Write-Host "csc: $_" }
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path $ScratchExe)) { throw "Compiling the scratch app failed (exit $LASTEXITCODE)." }
}

function Start-ScratchApp {
    $process = Start-Process -FilePath $ScratchExe -ArgumentList "`"$ScratchTone`"" -WindowStyle Hidden -PassThru
    if (-not (Wait-Until -Probe { -not $process.HasExited } -Timeout 15 -Description 'scratch app started')) {
        throw 'The scratch app exited immediately.'
    }
    Write-Host "Scratch app running (PID $($process.Id))." -ForegroundColor DarkGray
}

function Stop-ScratchApps {
    Get-Process -Name 'SampleAudioApp' -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
}

# --- App windows and rule-list helpers (Control-SoundSwitch provides the UIA primitives) --

function Close-AppWindow {
    param([Parameter(Mandatory)][IntPtr]$Handle)
    [SoundSwitch.AppControl.Native]::PostMessage($Handle, 0x0010, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
}

function Get-WindowForId {
    # The app window (Settings, editor or picker) that exposes the given automation id.
    param([Parameter(Mandatory)][string]$AutomationId, [int]$TimeoutSeconds = 10)
    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        foreach ($window in @(Get-SoundSwitchWindows -ProcessId $script:AppPid)) {
            if (Find-SoundSwitchElement -Handle $window.Handle -AutomationId $AutomationId) { return $window }
        }
        Start-Sleep -Milliseconds 250
    }
    return $null
}

function Get-ListRows {
    param([Parameter(Mandatory)][IntPtr]$Handle)
    $root = [System.Windows.Automation.AutomationElement]::FromHandle($Handle)
    return @(Get-SoundSwitchDescendantElements -Element $root |
            Where-Object { $_.Current.ControlType -eq [System.Windows.Automation.ControlType]::ListItem } |
            ForEach-Object { [string]$_.Current.Name })
}

function Select-ListRow {
    param([Parameter(Mandatory)][IntPtr]$Handle, [Parameter(Mandatory)][string]$Text)
    $root = [System.Windows.Automation.AutomationElement]::FromHandle($Handle)
    $row = Get-SoundSwitchDescendantElements -Element $root | Where-Object {
        $_.Current.ControlType -eq [System.Windows.Automation.ControlType]::ListItem -and
        "$($_.Current.Name)" -match [regex]::Escape($Text)
    } | Select-Object -First 1
    if (-not $row) { return $false }
    $selection = $null
    if ($row.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$selection)) {
        $selection.Select()
        Start-Sleep -Milliseconds 250
        return $true
    }
    return $false
}

function Get-ComboItemLabels {
    param([Parameter(Mandatory)]$Combo)
    $expand = $Combo.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
    $expand.Expand()
    Start-Sleep -Milliseconds 200
    try {
        return @(Get-SoundSwitchDescendantElements -Element $Combo |
                Where-Object { $_.Current.ControlType -eq [System.Windows.Automation.ControlType]::ListItem } |
                ForEach-Object { ([string]$_.Current.Name).Trim() })
    }
    finally { try { $expand.Collapse() } catch { } }
}

function Open-RuleEditor {
    # Clicks Add with BM_CLICK (the UIA Invoke pattern would block inside the modal editor)
    # and waits for the editor AND the process picker it auto-opens on Shown.
    param([Parameter(Mandatory)][IntPtr]$SettingsHandle, [long[]]$KnownHandles)

    $script:addButton = $null
    $null = Wait-Until -Probe {
        $script:addButton = Find-SoundSwitchElement -Handle $SettingsHandle -AutomationId 'addAppSoundRuleButton'
        $null -ne $script:addButton
    } -Timeout 10 -Description 'Add rule button on the App Rules tab'
    if (-not $script:addButton) { throw 'The Add rule button was not found on the App Rules tab.' }
    Invoke-SoundSwitchElement -Element $script:addButton

    $script:editorWindow = $null
    $script:pickerWindow = $null
    $opened = Wait-Until -Probe {
        foreach ($window in @(Get-SoundSwitchWindows -ProcessId $script:AppPid)) {
            if ($KnownHandles -contains $window.Handle.ToInt64()) { continue }
            if (-not $script:editorWindow -and (Find-SoundSwitchElement -Handle $window.Handle -AutomationId 'rdoEnforcementDynamic')) {
                $script:editorWindow = $window
            }
            if (-not $script:pickerWindow -and ($null -eq $script:editorWindow -or $script:editorWindow.Handle -ne $window.Handle) -and
                (Find-SoundSwitchElement -Handle $window.Handle -AutomationId 'dgvProcesses')) {
                $script:pickerWindow = $window
            }
        }
        ($script:editorWindow -and $script:pickerWindow)
    } -Timeout ([int]($TimeoutSeconds / 2)) -Description 'rule editor and process picker'

    if (-not $opened) { return $null }
    return [pscustomobject]@{ Editor = $script:editorWindow; Picker = $script:pickerWindow }
}

# --- Dot-source the control surface (its one-shot action mode is skipped) ------------
# Dot-sourcing runs Control-SoundSwitch's param block in THIS scope, which resets every
# same-named parameter ($NoBuild, $KeepRunning, $AllowExistingInstance, $TimeoutSeconds...)
# to its default. Capture and restore the values the caller passed. Never name locals
# after those parameters either ($Status is a switch, so `$status = <object>` throws).
$script:PassedOptions = @{
    NoBuild               = [bool]$NoBuild
    AllowExistingInstance = [bool]$AllowExistingInstance
    KeepRunning           = [bool]$KeepRunning
    Timeout               = [int]$TimeoutSeconds
}
. $ControlScript
$NoBuild               = $script:PassedOptions.NoBuild
$AllowExistingInstance = $script:PassedOptions.AllowExistingInstance
$KeepRunning           = $script:PassedOptions.KeepRunning
$TimeoutSeconds        = $script:PassedOptions.Timeout

# --- Configuration isolation --------------------------------------------------------

$backupDirectory = Split-Path -Parent $BackupPath
if (-not (Test-Path $backupDirectory)) { New-Item -ItemType Directory -Path $backupDirectory -Force | Out-Null }

$hadConfig = Test-Path $ConfigPath
if ($hadConfig) {
    Copy-Item -Path $ConfigPath -Destination $BackupPath -Force
    Write-Host "Backed up the configuration to $BackupPath" -ForegroundColor DarkGray
}
if ($hadConfig -and ($KeepRunning -or $AllowExistingInstance)) {
    Write-Warning "The configuration restore is skipped (the app would clobber it on its next save). Reapply $BackupPath after stopping the app."
}

if (-not $NoBuild) {
    Write-Host 'Building the application and CLI...' -ForegroundColor Cyan
    foreach ($project in @('SoundSwitch\SoundSwitch.csproj', 'SoundSwitch.CLI\SoundSwitch.CLI.csproj')) {
        & dotnet build (Join-Path $RepoRoot $project) -c Debug --nologo -v minimal
        if ($LASTEXITCODE -ne 0) { throw "dotnet build failed for '$project'." }
    }
}

$appExe = Resolve-SoundSwitchExecutable -ProjectDirectory 'SoundSwitch' -FileName 'SoundSwitch.exe'
$cliExe = Resolve-SoundSwitchExecutable -ProjectDirectory 'SoundSwitch.CLI' -FileName 'SoundSwitch.CLI.exe'
if (-not $appExe) { throw 'SoundSwitch.exe not found. Build first or drop -NoBuild.' }
if (-not $cliExe) { throw 'SoundSwitch.CLI.exe not found. Build first or drop -NoBuild.' }
$cli = $cliExe.FullName

$script:AppPid = 0
$launched = $false
$stagedConfig = $false

try {
    $instances = @(Get-SoundSwitchProcess)
    if ($instances.Count -gt 0 -and -not $AllowExistingInstance) {
        throw "SoundSwitch is already running (PID: $(($instances | ForEach-Object Id) -join ', ')). Close it first, or pass -AllowExistingInstance."
    }

    $null = Remove-ScratchStoreRecords   # leftovers from an aborted run never shadow assertions

    # A deterministic config (no rules, empty ledger) must be staged while no instance runs.
    if ($instances.Count -eq 0) {
        if ($hadConfig) { Remove-Item -Path $ConfigPath -Force -ErrorAction SilentlyContinue }
        Set-Content -Path $ConfigPath -Value '{}' -Encoding utf8
        $stagedConfig = $true
        Write-Host 'Staged an empty test configuration (no rules, no ledger).' -ForegroundColor DarkGray
        $null = Start-SoundSwitchApp -AppExe $appExe.FullName -CliExe $cli -TimeoutSeconds $TimeoutSeconds
        $launched = $true
    }
    else {
        Write-Host "Using the running SoundSwitch instance (PID $($instances[0].Id))." -ForegroundColor Yellow
    }
    $script:AppPid = (Get-SoundSwitchProcess | Select-Object -First 1).Id

    Write-Host ''
    Write-Host 'Staging the scratch audio app' -ForegroundColor Cyan
    Stop-ScratchApps
    New-ScratchApp
    Start-ScratchApp

    # The system default playback label, from the app's own IPC surface.
    $defaultPlaybackName = ''
    $statusRaw = (& $cli status --json) -join "`n"
    if ($statusRaw) {
        $statusInfo = $statusRaw | ConvertFrom-Json
        if ($statusInfo.PSObject.Properties.Name -contains 'playbackDevice' -and $statusInfo.playbackDevice) {
            $defaultPlaybackName = [string]$statusInfo.playbackDevice
        }
    }
    Assert-True -Condition ($defaultPlaybackName.Length -gt 0) -Message "The CLI reports the system default playback device ('$defaultPlaybackName')"

    Write-Host ''
    Write-Host 'Opening Settings (App Rules tab)' -ForegroundColor Cyan
    $settings = Open-SoundSwitchSettings -CliExe $cli -ProcessId $script:AppPid -TimeoutSeconds $TimeoutSeconds
    $appRulesTab = Find-SoundSwitchElement -Handle $settings.Handle -AutomationId 'appSoundLockTabPage'
    $tabSelected = $false
    if ($appRulesTab) {
        try { $appRulesTab.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select(); $tabSelected = $true } catch { }
    }
    Assert-True -Condition $tabSelected -Message 'The App Rules tab was selected (by automation id)'

    # ================================================================================
    # Scenario A — rule editor: Enforced by pair + identity guard
    # ================================================================================

    Write-Host ''
    Write-Host 'Scenario A: rule editor enforcement radios' -ForegroundColor Cyan

    try {
        $preClickHandles = @(Get-SoundSwitchWindows -ProcessId $script:AppPid | ForEach-Object { $_.Handle.ToInt64() })
        $ruleEditor = Open-RuleEditor -SettingsHandle $settings.Handle -KnownHandles $preClickHandles
        Assert-True -Condition ($null -ne $ruleEditor) -Message 'The rule editor and its auto-opened process picker opened'
        if (-not $ruleEditor) { throw 'The Add-rule dialog did not open the expected windows.' }

        Close-AppWindow -Handle $ruleEditor.Picker.Handle
        $null = Wait-SoundSwitchWindowGone -Handle $ruleEditor.Picker.Handle -TimeoutSeconds 10

        $editor = Get-WindowForId -AutomationId 'rdoEnforcementDynamic' -TimeoutSeconds 10
        $editorHandle = $editor.Handle
        $dynamicSelected = Get-SoundSwitchRadioState -WindowHandle $editorHandle -AutomationId 'rdoEnforcementDynamic'
        $assignmentSelected = Get-SoundSwitchRadioState -WindowHandle $editorHandle -AutomationId 'rdoEnforcementAssignment'
        Assert-True -Condition ($dynamicSelected -eq $true) -Message 'Processes (dynamic) is the default enforcement'
        Assert-True -Condition ($assignmentSelected -eq $false) -Message 'App assignment is not preselected'

        $processField = Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'txtProcessPath'
        $patternField = Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'txtPattern'
        Assert-True -Condition ($null -ne $processField -and $null -ne $patternField) -Message 'The editor exposes the process path and window title fields'
        $assignmentRadio = Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'rdoEnforcementAssignment'
        Assert-True -Condition ($assignmentRadio.Current.IsEnabled -eq $false) -Message 'App assignment is disabled while the process path is empty'

        $null = Set-SoundSwitchText -WindowHandle $editorHandle -AutomationId 'txtProcessPath' -Value 'C:\Windows\Notepad.exe'
        $null = Wait-Until -Probe {
            (Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'rdoEnforcementAssignment').Current.IsEnabled
        } -Timeout 5 -Description 'assignment radio enabled'
        Assert-True -Condition ((Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'rdoEnforcementAssignment').Current.IsEnabled) `
            -Message 'App assignment enables once a concrete identity is set'

        $null = Select-SoundSwitchRadio -WindowHandle $editorHandle -AutomationId 'rdoEnforcementAssignment'
        Assert-True -Condition ((Get-SoundSwitchRadioState -WindowHandle $editorHandle -AutomationId 'rdoEnforcementAssignment') -eq $true) `
            -Message 'App assignment (identity) is selectable'
        Assert-True -Condition ((Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'txtPattern').Current.IsEnabled -eq $true) `
            -Message 'The window title field stays usable while the pattern is empty'

        # Implemented guard: a title pattern disables the assignment option again.
        $null = Set-SoundSwitchText -WindowHandle $editorHandle -AutomationId 'txtPattern' -Value '.*Test window.*'
        $null = Wait-Until -Probe {
            -not (Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'rdoEnforcementAssignment').Current.IsEnabled
        } -Timeout 5 -Description 'assignment disabled by a title pattern'
        Assert-True -Condition ((Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'rdoEnforcementAssignment').Current.IsEnabled -eq $false) `
            -Message 'A window-title pattern disables App assignment (identity-only guard)'
        $null = Set-SoundSwitchText -WindowHandle $editorHandle -AutomationId 'txtPattern' -Value ''
        $null = Wait-Until -Probe {
            (Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'rdoEnforcementAssignment').Current.IsEnabled
        } -Timeout 5 -Description 'assignment enabled again'
        Assert-True -Condition ((Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'rdoEnforcementAssignment').Current.IsEnabled) `
            -Message 'Clearing the pattern re-enables App assignment'

        $helper = Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'lblAssignmentHelper'
        Assert-True -Condition ($null -ne $helper -and "$($helper.Current.Name)".Length -gt 0 -and -not $helper.Current.IsOffscreen) `
            -Message 'The assignment helper text is visible when App assignment is chosen'

        Close-AppWindow -Handle $ruleEditor.Editor.Handle
        $null = Wait-SoundSwitchWindowGone -Handle $ruleEditor.Editor.Handle -TimeoutSeconds 10
        Assert-Equal -Expected 0 -Actual @((Get-RulesOf (Read-Config))).Count -Message 'Closing the dialog without saving creates no rule'
    }
    catch {
        Assert-True -Condition $false -Message "Scenario A hit an exception: $($_.Exception.Message)"
    }

    if ($script:Failures -gt 0) { throw 'Scenario A failed; skipping the remaining scenarios.' }

    # ================================================================================
    # Scenario B — an assignment rule writes the engine's persisted route once
    # ================================================================================

    Write-Host ''
    Write-Host 'Scenario B: creating an assignment rule writes the route' -ForegroundColor Cyan

    try {
        $preClickHandles = @(Get-SoundSwitchWindows -ProcessId $script:AppPid | ForEach-Object { $_.Handle.ToInt64() })
        $ruleEditor = Open-RuleEditor -SettingsHandle $settings.Handle -KnownHandles $preClickHandles
        Assert-True -Condition ($null -ne $ruleEditor) -Message 'The rule editor and picker reopened for rule creation'
        if (-not $ruleEditor) { throw 'The Add-rule dialog did not open for Scenario B.' }

        Close-AppWindow -Handle $ruleEditor.Picker.Handle
        $null = Wait-SoundSwitchWindowGone -Handle $ruleEditor.Picker.Handle -TimeoutSeconds 10

        $editor = Get-WindowForId -AutomationId 'rdoEnforcementDynamic' -TimeoutSeconds 10
        $editorHandle = $editor.Handle
        $null = Set-SoundSwitchText -WindowHandle $editorHandle -AutomationId 'txtProcessPath' -Value $ScratchExe
        $null = Set-SoundSwitchText -WindowHandle $editorHandle -AutomationId 'txtPattern' -Value ''
        $null = Wait-Until -Probe {
            (Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'rdoEnforcementAssignment').Current.IsEnabled
        } -Timeout 5 -Description 'assignment radio enabled for the scratch identity'
        Assert-True -Condition ((Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'rdoEnforcementAssignment').Current.IsEnabled) `
            -Message "The scratch identity '$ScratchExe' is accepted by the identity guard"
        $null = Select-SoundSwitchRadio -WindowHandle $editorHandle -AutomationId 'rdoEnforcementAssignment'
        Assert-True -Condition ((Get-SoundSwitchRadioState -WindowHandle $editorHandle -AutomationId 'rdoEnforcementAssignment') -eq $true) `
            -Message 'Enforced by set to App assignment (identity)'

        # Pick a device that is not the system default: the editor lists active devices only,
        # and the engine can only route to a live endpoint. Labels come from the combo itself
        # (NameClean); the CLI's active-device list disambiguates the default.
        $chosen = $null
        $combo = Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'cmbPlayback'
        $labels = @(Get-ComboItemLabels -Combo $combo)
        $target = $labels | Where-Object { $_ -and $_ -ine $defaultPlaybackName -and "$defaultPlaybackName" -notlike "*$_*" } | Select-Object -First 1
        if (-not $target) {
            # Single-endpoint machines: route to the default so B still covers persistence;
            # the route-away-from-default assertions are skipped below.
            $target = $labels | Select-Object -First 1
        }
        if ($target) {
            try { $chosen = Set-SoundSwitchComboBoxItem -WindowHandle $editorHandle -AutomationId 'cmbPlayback' -Target $target }
            catch { Write-Warning "Could not select the playback device '$target': $($_.Exception.Message)" }
            if ($chosen) { Write-Host "  Playback device: $chosen (system default: $defaultPlaybackName)" -ForegroundColor DarkGray }
        }
        $nonDefault = $null -ne $chosen -and "$chosen" -ine $defaultPlaybackName -and "$defaultPlaybackName" -notlike "*$chosen*"

        $saveButton = Find-SoundSwitchElement -Handle $editorHandle -AutomationId 'btnSave'
        if ($saveButton) { Invoke-SoundSwitchElement -Element $saveButton }
        Assert-True -Condition (Wait-SoundSwitchWindowGone -Handle $ruleEditor.Editor.Handle -TimeoutSeconds 20) `
            -Message 'The rule editor closes after Save (validation passed)'

        $null = Wait-Until -Probe { @((Get-RulesOf (Read-Config))).Count -ge 1 } -Timeout 15 -Description 'rule persisted'
        $rules = @(Get-RulesOf (Read-Config))
        Assert-Equal -Expected 1 -Actual $rules.Count -Message 'Exactly one rule is persisted'
        $rule = $rules | Select-Object -First 1
        if ($null -eq $rule) { throw 'No rule was persisted; cannot continue Scenario B.' }
        Assert-Equal -Expected 1 -Actual $rule.Enforcement -Message 'The persisted rule is an Assignment rule (Enforcement == 1)'
        Assert-True -Condition (Test-ScratchIdentity "$($rule.AssignmentExePath)") -Message "The rule identity is the scratch exe (AssignmentExePath = '$($rule.AssignmentExePath)')"
        Assert-True -Condition ("$($rule.AssignmentExePath)" -ieq "$($rule.ProcessPath)") -Message 'Assignment rules persist the identity as their process match'

        $null = Wait-Until -Probe { @((Get-LedgerOf (Read-Config)) | Where-Object { Test-ScratchIdentity $_.ExePath }).Count -ge 1 } -Timeout 15 -Description 'ledger entry persisted'
        $ledger = @((Get-LedgerOf (Read-Config)) | Where-Object { Test-ScratchIdentity $_.ExePath })
        Assert-Equal -Expected 1 -Actual $ledger.Count -Message 'The ownership ledger gained one entry for the identity write'

        if ($nonDefault) {
            $script:storeRecord = $null
            $written = Wait-Until -Probe { $script:storeRecord = Get-ScratchStoreRecord; $null -ne $script:storeRecord } -Timeout $TimeoutSeconds -Description 'persisted store record'
            Assert-True -Condition $written -Message "Windows' persisted-endpoint store gained a record for $ScratchNeedle"
            if ($script:storeRecord) {
                Assert-True -Condition ($script:storeRecord.RenderToken.Length -gt 0) -Message 'The persisted record carries a render device token'
            }
        }
        else {
            Skip-Check 'Store route assertions skipped (no non-default playback device was selected).'
        }

        $script:rows = @()
        $oneRow = Wait-Until -Probe {
            $script:rows = @(Get-ListRows -Handle $settings.Handle | Where-Object { "$_" -match [regex]::Escape($ScratchNeedle) })
            $script:rows.Count -ge 1
        } -Timeout 15 -Description 'rule row in the list'
        Assert-True -Condition ($oneRow -and $script:rows.Count -eq 1) `
            -Message "Exactly one rule-list row exists for the scratch identity (merged view): [$($script:rows -join '; ')]"
    }
    catch {
        Assert-True -Condition $false -Message "Scenario B hit an exception: $($_.Exception.Message)"
    }

    if ($script:Failures -gt 0) { throw 'Scenario B failed; skipping Scenario C.' }

    # ================================================================================
    # Scenario C — deleting the rule restores the default (deferred clear)
    # ================================================================================

    Write-Host ''
    Write-Host 'Scenario C: deleting the rule restores the default (deferred)' -ForegroundColor Cyan

    try {
        # Closed app: the restore-default is deferred to its next sighting (ADR §6.3).
        Stop-ScratchApps
        $null = Wait-Until -Probe { @(Get-Process -Name 'SampleAudioApp' -ErrorAction SilentlyContinue).Count -eq 0 } -Timeout 10 -Description 'scratch app stopped'

        $selected = Select-ListRow -Handle $settings.Handle -Text $ScratchNeedle
        if (-not $selected) {
            # Fallback: HOME then DOWN to the rule row's display index.
            $index = [array]::IndexOf(@(Get-ListRows -Handle $settings.Handle), $ScratchExe)
            if ($index -lt 0) { $index = 0 }
            Add-Type -AssemblyName System.Windows.Forms
            [SoundSwitch.AppControl.Native]::SetForegroundWindow($settings.Handle) | Out-Null
            [System.Windows.Forms.SendKeys]::SendWait('{HOME}' + ('{DOWN}' * $index))
            Start-Sleep -Milliseconds 300
        }

        $script:deleteButton = $null
        $deleteReady = Wait-Until -Probe {
            $script:deleteButton = Find-SoundSwitchElement -Handle $settings.Handle -AutomationId 'deleteAppSoundRuleButton'
            $null -ne $script:deleteButton -and $script:deleteButton.Current.IsEnabled
        } -Timeout 6 -Description 'delete button enabled'
        Assert-True -Condition $deleteReady -Message 'The Delete rule button is enabled after selecting the rule'

        if ($deleteReady) {
            $preClickHandles = @(Get-SoundSwitchWindows -ProcessId $script:AppPid | ForEach-Object { $_.Handle.ToInt64() })
            Invoke-SoundSwitchElement -Element $script:deleteButton
            $confirmed = $false
            try {
                $null = Confirm-SoundSwitchDialog -ProcessId $script:AppPid -KnownHandles $preClickHandles -Result Yes -Description 'delete confirmation dialog' -TimeoutSeconds 10
                $confirmed = $true
            }
            catch { Write-Warning $_.Exception.Message }
            Assert-True -Condition $confirmed -Message 'The delete confirmation was answered with Yes'
        }

        $null = Wait-Until -Probe { @((Get-RulesOf (Read-Config))).Count -eq 0 } -Timeout 15 -Description 'rule removed from config'
        Assert-Equal -Expected 0 -Actual @((Get-RulesOf (Read-Config))).Count -Message 'The deleted rule is gone from the configuration'

        # Closed app: the entry stays pending (ClearRequestedAt) so the meeting can clear
        # the store record; it is no longer an ownership claim.
        $pending = @((Get-LedgerOf (Read-Config)) | Where-Object { Test-ScratchIdentity $_.ExePath })
        $pendingMarked = @($pending | Where-Object { $_.PSObject.Properties.Name -contains 'ClearRequestedAt' -and $null -ne $_.ClearRequestedAt }).Count
        Assert-True -Condition ($pending.Count -eq 0 -or $pendingMarked -eq $pending.Count) `
            -Message 'The ledger claim is retracted (no claim, or all entries marked ClearRequestedAt)'

        Start-ScratchApp
        $cleared = Wait-Until -Probe { $null -eq (Get-ScratchStoreRecord) } -Timeout $TimeoutSeconds -Description 'deferred restore-default clear'
        Assert-True -Condition $cleared -Message "The deferred restore-default removed the persisted route on the exe's next sighting"

        $null = Wait-Until -Probe { @((Get-LedgerOf (Read-Config)) | Where-Object { Test-ScratchIdentity $_.ExePath }).Count -eq 0 } -Timeout 15 -Description 'ledger claim cleared'
        Assert-Equal -Expected 0 -Actual @((Get-LedgerOf (Read-Config)) | Where-Object { Test-ScratchIdentity $_.ExePath }).Count `
            -Message 'The ledger claim is gone from the configuration'
    }
    catch {
        Assert-True -Condition $false -Message "Scenario C hit an exception: $($_.Exception.Message)"
    }
}
finally {
    Write-Host ''
    Write-Host 'Teardown' -ForegroundColor Cyan

    # Stop only the instance this test launched (never a user instance / stale PID file).
    if ($launched -and -not $KeepRunning) { Stop-SoundSwitchApp }
    if (-not $KeepRunning) { Stop-ScratchApps }

    # Restore only when no instance of ours can clobber the restore from memory.
    if ($stagedConfig -and -not $KeepRunning -and -not $AllowExistingInstance) {
        if ($hadConfig -and (Test-Path $BackupPath)) {
            Copy-Item -Path $BackupPath -Destination $ConfigPath -Force
            Write-Host "Restored the original configuration from $BackupPath" -ForegroundColor DarkGray
        }
        elseif (-not $hadConfig -and (Test-Path $ConfigPath)) {
            Remove-Item -Path $ConfigPath -Force -ErrorAction SilentlyContinue
            Write-Host 'Removed the configuration this test created.' -ForegroundColor DarkGray
        }
    }

    $removed = Remove-ScratchStoreRecords
    Write-Host "Removed $removed persisted-endpoint record(s) for $ScratchNeedle." -ForegroundColor DarkGray

    if (-not $KeepRunning) {
        Stop-ScratchApps
        Remove-Item -Path $ScratchDir -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "Removed the scratch directory ($ScratchDir)." -ForegroundColor DarkGray
    }
}

Write-Host ''
if ($script:Failures -gt 0) {
    Write-Host "END-TO-END FAILED: $($script:Failures) of $($script:Checks) checks failed ($($script:Skips) skipped)." -ForegroundColor Red
    exit 1
}

Write-Host "END-TO-END PASSED: $($script:Checks) checks ($($script:Skips) skipped)." -ForegroundColor Green
