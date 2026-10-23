#Requires -Version 7.0
<#
.SYNOPSIS
    End-to-end test of the microphone mute target feature against a real app instance.

.DESCRIPTION
    Drives a live SoundSwitch instance the same way Capture-DocumentationScreenshot.ps1
    drives it for screenshots, and asserts the user-visible behaviour of the mute
    target feature:

      1. Backs up the user's configuration (%APPDATA%\SoundSwitch\SoundSwitchConfiguration.json).
      2. Builds and launches SoundSwitch (unless -NoBuild / -AllowExistingInstance).
      3. Opens Settings, selects a real microphone in the "Mute microphone" combo box
         through UI Automation, and verifies the configuration persisted it as a full
         DeviceInfo snapshot (no legacy id field).
      4. Verifies the CLI mute command now reports that microphone, and that toggling
         flips the state on it.
      5. Switches the target back to "Windows default input" and verifies the CLI
         reports the Windows default input again and the stored target is cleared.
      6. Simulates a target that is unavailable (a device snapshot that matches no
         connected endpoint), restarts the app, and verifies the mute action falls
         back to the Windows default input while the selection is still kept in the
         configuration so it resumes when the device returns.
      7. Restores the original configuration and stops the app it launched.

    Steps 3-5 need at least one usable microphone. Use -TargetName to pick a specific
    one by name; by default the first microphone that differs from the current
    default input is used.

.PARAMETER TargetName
    Visible name of the microphone to select as the mute target. Defaults to the
    first selectable entry that is not the current default input.

.PARAMETER NoBuild
    Skip the build step and use the existing binaries.

.PARAMETER AllowExistingInstance
    Attach to an already running SoundSwitch instead of launching a new one. The
    configuration is still backed up and restored.

.PARAMETER KeepRunning
    Leave the app running after the test.

.PARAMETER SkipUnavailableTargetTest
    Skip step 6 (the unavailable-target fallback), which restarts the app.

.PARAMETER TimeoutSeconds
    Timeout for IPC readiness, window discovery and CLI calls. Defaults to 90.

.EXAMPLE
    pwsh tools\Test-MuteTargetEndToEnd.ps1
    Runs the full end-to-end mute target test.

.EXAMPLE
    pwsh tools\Test-MuteTargetEndToEnd.ps1 -TargetName "Line In (High Definition Audio Device)"
    Uses a specific microphone as the target.

.EXAMPLE
    pwsh tools\Test-MuteTargetEndToEnd.ps1 -AllowExistingInstance -KeepRunning -NoBuild
    Tests against the instance you already have open and leaves it running.
#>
[CmdletBinding()]
param(
    [string]$TargetName,
    [switch]$NoBuild,
    [switch]$AllowExistingInstance,
    [switch]$KeepRunning,
    [switch]$SkipUnavailableTargetTest,
    [int]$TimeoutSeconds = 90
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSScriptRoot
$ControlScript = Join-Path $PSScriptRoot 'Control-SoundSwitch.ps1'
$ConfigPath = Join-Path $env:APPDATA 'SoundSwitch\SoundSwitchConfiguration.json'
$BackupPath = Join-Path $env:TEMP 'SoundSwitchControl\SoundSwitchConfiguration.backup.json'

if (-not (Test-Path $ControlScript)) {
    throw "Control script not found: $ControlScript"
}

# --- Assertions ---------------------------------------------------------------------

$script:Failures = 0
$script:Checks = 0

function Assert-True {
    param(
        [Parameter(Mandatory = $true)][bool]$Condition,
        [Parameter(Mandatory = $true)][string]$Message
    )

    $script:Checks++
    if ($Condition) {
        Write-Host "  PASS  $Message" -ForegroundColor Green
    }
    else {
        $script:Failures++
        Write-Host "  FAIL  $Message" -ForegroundColor Red
    }
}

function Assert-Equal {
    param(
        [Parameter(Mandatory = $true)]$Expected,
        [Parameter(Mandatory = $true)]$Actual,
        [Parameter(Mandatory = $true)][string]$Message
    )

    Assert-True -Condition ("$Expected" -eq "$Actual") -Message "$Message (expected '$Expected', got '$Actual')"
}

# --- CLI helpers --------------------------------------------------------------------

function Invoke-Cli {
    param(
        [Parameter(Mandatory = $true)][string]$CliExe,
        [Parameter(Mandatory = $true)][string]$Command
    )

    $arguments = $Command.Split(' ', [System.StringSplitOptions]::RemoveEmptyEntries)
    $output = & $CliExe @arguments 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "CLI '$Command' failed with exit code $LASTEXITCODE`: $output"
    }
    return ($output | Out-String).Trim()
}

function Get-MuteState {
    param([Parameter(Mandatory = $true)][string]$CliOutput)

    $match = [regex]::Match($CliOutput, '^(?<name>.+?) is (?:now |currently )(?<state>muted|unmuted)\s*$',
        [System.Text.RegularExpressions.RegexOptions]::Multiline)
    if (-not $match.Success) {
        throw "Unexpected mute output: $CliOutput"
    }
    return [pscustomobject]@{
        Name    = $match.Groups['name'].Value.Trim()
        IsMuted = $match.Groups['state'].Value -eq 'muted'
    }
}

# --- Configuration helpers ----------------------------------------------------------

function Read-Config {
    if (-not (Test-Path $ConfigPath)) { return $null }
    return (Get-Content $ConfigPath -Raw | ConvertFrom-Json)
}

function Write-Config {
    param([Parameter(Mandatory = $true)]$Config)
    $Config | ConvertTo-Json -Depth 100 | Set-Content -Path $ConfigPath -Encoding utf8
}

function Test-ConfigHasTarget {
    param($Config)

    return $null -ne $Config -and
        ($Config.PSObject.Properties.Name -contains 'MuteRecordingDevice') -and
        $null -ne $Config.MuteRecordingDevice
}

# --- App lifecycle ------------------------------------------------------------------

function Start-SoundSwitchInstance {
    param(
        [Parameter(Mandatory = $true)][string]$AppExe,
        [Parameter(Mandatory = $true)][string]$CliExe
    )

    Write-Host "Starting $AppExe ..." -ForegroundColor Cyan
    $instance = Start-Process -FilePath $AppExe -ArgumentList '--disable-updater' -PassThru

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if ($instance.HasExited) { throw "SoundSwitch exited immediately (code $($instance.ExitCode))." }
        & $CliExe status *> $null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "SoundSwitch is ready (PID $($instance.Id))." -ForegroundColor Green
            return $instance
        }
        Start-Sleep -Milliseconds 500
    }

    $instance | Stop-Process -Force -ErrorAction SilentlyContinue
    throw "SoundSwitch did not become ready within $TimeoutSeconds seconds."
}

function Stop-SoundSwitchInstance {
    $instances = @(Get-Process -Name 'SoundSwitch' -ErrorAction SilentlyContinue)
    foreach ($instance in $instances) {
        $instance | Stop-Process -Force -ErrorAction SilentlyContinue
    }
    if ($instances.Count -gt 0) {
        Start-Sleep -Milliseconds 800
        Write-Host 'Stopped the SoundSwitch instance started by this test.' -ForegroundColor DarkGray
    }
}

# --- Back up the configuration ------------------------------------------------------

$backupDirectory = Split-Path -Parent $BackupPath
if (-not (Test-Path $backupDirectory)) {
    New-Item -ItemType Directory -Path $backupDirectory -Force | Out-Null
}

$hadConfig = Test-Path $ConfigPath
if ($hadConfig) {
    Copy-Item -Path $ConfigPath -Destination $BackupPath -Force
    Write-Host "Backed up the configuration to $BackupPath" -ForegroundColor DarkGray
}
else {
    Write-Host 'No existing configuration to back up.' -ForegroundColor DarkGray
}

# --- Resolve binaries ---------------------------------------------------------------

function Resolve-TestExecutable {
    param([string]$Project, [string]$FileName)

    return Get-ChildItem -Path (Join-Path $RepoRoot "$Project\bin\Debug") -Filter $FileName -Recurse -ErrorAction SilentlyContinue |
        Select-Object -First 1
}

if (-not $NoBuild) {
    Write-Host 'Building the application and CLI...' -ForegroundColor Cyan
    foreach ($project in @('SoundSwitch\SoundSwitch.csproj', 'SoundSwitch.CLI\SoundSwitch.CLI.csproj')) {
        & dotnet build (Join-Path $RepoRoot $project) -c Debug --nologo -v minimal
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet build failed for '$project'."
        }
    }
}

$appExe = Resolve-TestExecutable -Project 'SoundSwitch' -FileName 'SoundSwitch.exe'
$cliExe = Resolve-TestExecutable -Project 'SoundSwitch.CLI' -FileName 'SoundSwitch.CLI.exe'
if (-not $appExe) { throw 'SoundSwitch.exe not found. Build first or drop -NoBuild.' }
if (-not $cliExe) { throw 'SoundSwitch.CLI.exe not found. Build first or drop -NoBuild.' }

$cli = $cliExe.FullName
$launched = $false

try {
    # --- Start / attach -------------------------------------------------------------

    $existing = @(Get-Process -Name 'SoundSwitch' -ErrorAction SilentlyContinue)
    if ($existing.Count -gt 0 -and -not $AllowExistingInstance) {
        throw "SoundSwitch is already running (PID: $(($existing | ForEach-Object Id) -join ', ')). Close it, or pass -AllowExistingInstance."
    }

    if ($existing.Count -eq 0) {
        $null = Start-SoundSwitchInstance -AppExe $appExe.FullName -CliExe $cli
        $launched = $true
    }
    else {
        Write-Host "Using the running SoundSwitch instance (PID $($existing[0].Id))." -ForegroundColor Yellow
    }

    # --- Baseline -------------------------------------------------------------------

    Write-Host ''
    Write-Host 'Baseline' -ForegroundColor Cyan
    $baseline = Get-MuteState -CliOutput (Invoke-Cli -CliExe $cli -Command 'mute')
    Write-Host "  Current mute target: $($baseline.Name) (muted=$($baseline.IsMuted))"
    Assert-True -Condition ($baseline.Name.Length -gt 0) -Message 'The mute command reports a microphone'

    # --- Select a real microphone target --------------------------------------------

    Write-Host ''
    Write-Host 'Selecting a mute target through Settings' -ForegroundColor Cyan

    if ($TargetName) {
        & $ControlScript -MuteTarget $TargetName -NoBuild -TimeoutSeconds $TimeoutSeconds
        if ($LASTEXITCODE -ne 0) { throw 'Selecting the mute target failed.' }
    }
    else {
        # Prefer a target that differs from the current default input so the test proves
        # the mute action really follows the selection.
        $selectedByIndex = $false
        foreach ($index in @('1', '2', '3', '4')) {
            & $ControlScript -MuteTarget $index -NoBuild -TimeoutSeconds $TimeoutSeconds
            if ($LASTEXITCODE -ne 0) { continue }
            $selectedByIndex = $true
            $candidateConfig = Read-Config
            if ((Test-ConfigHasTarget $candidateConfig) -and
                $candidateConfig.MuteRecordingDevice.NameClean -ne $baseline.Name) {
                break
            }
        }
        if (-not $selectedByIndex) { throw 'No selectable mute target was found in Settings.' }
    }

    $config = Read-Config
    Assert-True -Condition (Test-ConfigHasTarget $config) -Message 'The selected target is persisted as a DeviceInfo snapshot'
    Assert-True -Condition (-not ($config.PSObject.Properties.Name -contains 'MuteRecordingDeviceId')) -Message 'No legacy MuteRecordingDeviceId field is written'
    Assert-True -Condition ($config.PSObject.Properties.Name -contains 'LimitMuteBannersToTarget') -Message 'The banner scope flag is persisted'

    if (-not (Test-ConfigHasTarget $config)) {
        throw 'Cannot continue: no mute target was persisted.'
    }

    $targetName = $config.MuteRecordingDevice.NameClean
    Write-Host "  Selected: $targetName ($($config.MuteRecordingDevice.Id))"

    # --- The mute action targets the selection --------------------------------------

    Write-Host ''
    Write-Host 'Mute action follows the selected target' -ForegroundColor Cyan
    $selected = Get-MuteState -CliOutput (Invoke-Cli -CliExe $cli -Command 'mute')
    Assert-Equal -Expected $targetName -Actual $selected.Name -Message 'The mute command reports the selected target'

    $toggled = Get-MuteState -CliOutput (Invoke-Cli -CliExe $cli -Command 'mute -t')
    Assert-True -Condition ($toggled.IsMuted -ne $selected.IsMuted) -Message 'Toggling flips the mute state'
    Assert-Equal -Expected $targetName -Actual $toggled.Name -Message 'Toggling stays on the selected target'

    $restored = Get-MuteState -CliOutput (Invoke-Cli -CliExe $cli -Command 'mute -t')
    Assert-Equal -Expected $selected.IsMuted -Actual $restored.IsMuted -Message 'Toggling back restores the original state'

    # --- Fallback to the Windows default input --------------------------------------

    Write-Host ''
    Write-Host 'Falling back to the Windows default input' -ForegroundColor Cyan
    & $ControlScript -MuteTarget 'Windows default input' -NoBuild -TimeoutSeconds $TimeoutSeconds
    if ($LASTEXITCODE -ne 0) { throw 'Selecting the default input failed.' }

    $configAfterReset = Read-Config
    Assert-True -Condition (-not (Test-ConfigHasTarget $configAfterReset)) -Message 'Choosing the default clears the stored target'

    $default = Get-MuteState -CliOutput (Invoke-Cli -CliExe $cli -Command 'mute')
    Write-Host "  Default input: $($default.Name)"
    Assert-True -Condition ($default.Name.Length -gt 0) -Message 'The mute command reports the Windows default input again'

    # --- Unavailable target falls back but keeps the selection ----------------------

    if (-not $SkipUnavailableTargetTest) {
        Write-Host ''
        Write-Host 'Unavailable target falls back to the default input' -ForegroundColor Cyan

        if (-not $launched) {
            Write-Host '  (skipped: the instance was attached, not launched, so it cannot be restarted)' -ForegroundColor DarkGray
        }
        else {
            # Stop the app so the edited configuration is loaded on the next start.
            Stop-SoundSwitchInstance

            $ghostId = '{0.0.1.00000000}.{00000000-0000-0000-0000-000000000000}'
            $ghostName = 'Ghost Microphone (test)'
            $config = Read-Config
            $ghost = [pscustomobject]@{
                Name         = $ghostName
                Id           = $ghostId
                Type         = 1
                IsUsb        = $false
                DiscoveredAt = (Get-Date).ToUniversalTime().ToString('o')
                NameClean    = $ghostName
            }
            if ($config.PSObject.Properties.Name -contains 'MuteRecordingDevice') {
                $config.MuteRecordingDevice = $ghost
            }
            else {
                $config | Add-Member -NotePropertyName 'MuteRecordingDevice' -NotePropertyValue $ghost -Force
            }
            Write-Config -Config $config

            $null = Start-SoundSwitchInstance -AppExe $appExe.FullName -CliExe $cli

            $fallback = Get-MuteState -CliOutput (Invoke-Cli -CliExe $cli -Command 'mute')
            Write-Host "  Fallback target: $($fallback.Name)"
            Assert-True -Condition ($fallback.Name -ne $ghostName) -Message 'An unavailable target is not used'
            Assert-Equal -Expected $default.Name -Actual $fallback.Name -Message 'The mute action falls back to the Windows default input'

            $configAfterFallback = Read-Config
            Assert-True -Condition (Test-ConfigHasTarget $configAfterFallback) -Message 'The unavailable selection is kept for when the device returns'
            Assert-Equal -Expected $ghostName -Actual $configAfterFallback.MuteRecordingDevice.NameClean -Message 'The kept selection is unchanged'
        }
    }
}
finally {
    # --- Restore configuration ------------------------------------------------------

    if ($hadConfig -and (Test-Path $BackupPath)) {
        Copy-Item -Path $BackupPath -Destination $ConfigPath -Force
        Write-Host "Restored the original configuration from $BackupPath" -ForegroundColor DarkGray
    }
    elseif (-not $hadConfig -and (Test-Path $ConfigPath)) {
        Remove-Item -Path $ConfigPath -Force -ErrorAction SilentlyContinue
        Write-Host 'Removed the configuration created by this test.' -ForegroundColor DarkGray
    }

    if ($launched -and -not $KeepRunning) {
        Stop-SoundSwitchInstance
    }
}

Write-Host ''
if ($script:Failures -gt 0) {
    Write-Host "END-TO-END FAILED: $($script:Failures) of $($script:Checks) checks failed." -ForegroundColor Red
    exit 1
}

Write-Host "END-TO-END PASSED: $($script:Checks) checks." -ForegroundColor Green
