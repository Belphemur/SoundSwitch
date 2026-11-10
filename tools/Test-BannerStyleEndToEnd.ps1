#Requires -Version 7.0
<#
.SYNOPSIS
    Tests global banner style and symmetric microphone lifetimes in the real app.
.DESCRIPTION
    Requires an interactive Windows desktop and a usable default microphone. Refuses
    to touch an existing SoundSwitch instance. Backs up configuration, uses English
    UI, drives style and mode combo boxes through UI Automation, and checks banner
    presence/lifetime for both microphone states. Restores the microphone state,
    stops only its own process, then restores configuration even on failure.
.PARAMETER NoBuild
    Reuse Debug binaries.
.PARAMETER TimeoutSeconds
    Timeout for app readiness and UI discovery.
.EXAMPLE
    pwsh tools/Test-BannerStyleEndToEnd.ps1
#>
[CmdletBinding()]
param([switch]$NoBuild, [int]$TimeoutSeconds = 60)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
. (Join-Path $PSScriptRoot 'Control-SoundSwitch.ps1') -NoBuild:$NoBuild -TimeoutSeconds $TimeoutSeconds
if (@(Get-SoundSwitchProcess).Count -gt 0) { throw 'Close SoundSwitch before running this isolated test.' }
if (-not $NoBuild) {
    & dotnet build (Join-Path $RepoRoot 'SoundSwitch.sln') -c Debug --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
}
$appExe = (Get-ChildItem (Join-Path $RepoRoot 'SoundSwitch/bin/Debug') -Filter SoundSwitch.exe -Recurse | Select-Object -First 1).FullName
$cliExe = (Get-ChildItem (Join-Path $RepoRoot 'SoundSwitch.CLI/bin/Debug') -Filter SoundSwitch.CLI.exe -Recurse | Select-Object -First 1).FullName
$configPath = Join-Path $env:APPDATA 'SoundSwitch/SoundSwitchConfiguration.json'
$backupPath = Join-Path $env:TEMP 'SoundSwitchControl/BannerStyleConfiguration.backup.json'
$hadConfig = Test-Path $configPath
$instance = $null
$originalMute = $null
$checks = 0

function Assert-True([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw "FAIL: $Message" }
    $script:checks++
    Write-Host "PASS: $Message" -ForegroundColor Green
}
function Get-MicrophoneState {
    $output = & $cliExe mute --json
    if ($LASTEXITCODE -ne 0) { throw "Cannot query default microphone: $output" }
    return ($output | Out-String | ConvertFrom-Json)
}
function Set-MicrophoneState([bool]$Muted) {
    $state = $Muted.ToString().ToLowerInvariant()
    $code = Invoke-SoundSwitchCli -CliExe $cliExe -Command "mute --state $state --json" -Quiet
    if ($code -ne 0) { throw "Cannot set microphone state to $state" }
}
function Get-Banners {
    foreach ($window in @(Get-SoundSwitchWindows -ProcessId $instance.Id -IncludeUntitled)) {
        $label = Find-SoundSwitchElement -Handle $window.Handle -AutomationId 'lblTop'
        if ($null -ne $label) { $window }
    }
}
function Wait-Banner([bool]$Visible) {
    $deadline = (Get-Date).AddSeconds(10)
    do {
        if ((@(Get-Banners).Count -gt 0) -eq $Visible) { return }
        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)
    throw "Banner visibility did not become $Visible"
}
function Assert-BannerRemains([double]$Seconds, [bool]$Visible) {
    $deadline = (Get-Date).AddSeconds($Seconds)
    do {
        if ((@(Get-Banners).Count -gt 0) -ne $Visible) { throw "Banner visibility changed before $Seconds seconds" }
        Start-Sleep -Milliseconds 100
    } while ((Get-Date) -lt $deadline)
    Assert-True $true "Banner visibility stayed $Visible for $Seconds seconds"
}
if ($hadConfig) {
    New-Item (Split-Path -Parent $backupPath) -ItemType Directory -Force | Out-Null
    Copy-Item $configPath $backupPath -Force
    Write-Host "Backed up the configuration to $backupPath" -ForegroundColor DarkGray
}
try {
    # A minimal scratch config avoids activating user profiles/rules during the test.
    $config = @{
        FirstRun = $false; Language = 0; Telemetry = $false; NotificationAdvancedMode = $true
        SwitchDeviceNotification = 5; SwitchProfileNotification = 2; MicrophoneMuteNotification = 5
        MicrophoneMuteBanner = 0; MicrophoneUnmuteBanner = 0; BannerOnScreenTime = '00:00:04'
        BannerDisplayInfo = 0; BannerScalePercentage = 100; BannerStyle = 0
        LimitMuteBannersToTarget = $true; MaxNumberNotification = 1
        # Do not replay the legacy Windows per-app endpoint reset on the test machine.
        MigratedFields = @('PersistentMuteNotification', 'SplitNotificationSettings', 'SwitchForegroundProgram_cleanup')
    }
    New-Item (Split-Path -Parent $configPath) -ItemType Directory -Force | Out-Null
    $config | ConvertTo-Json -Depth 20 | Set-Content $configPath -Encoding utf8
    $instance = Start-SoundSwitchApp -AppExe $appExe -CliExe $cliExe -TimeoutSeconds $TimeoutSeconds
    $originalMute = [bool](Get-MicrophoneState).isMuted
    $settings = Open-SoundSwitchSettings -CliExe $cliExe -ProcessId $instance.Id -TimeoutSeconds $TimeoutSeconds
    Assert-True (Select-SoundSwitchTab -Handle $settings.Handle -TabName Notifications) 'Notifications tab opens'

    foreach ($style in @('Normal', 'Compact')) {
        Set-SoundSwitchComboBoxItem -WindowHandle $settings.Handle -AutomationId bannerStyleComboBox -Target $style | Out-Null
        $stored = Get-Content $configPath -Raw | ConvertFrom-Json
        Assert-True ([int]$stored.BannerStyle -eq [int]($style -eq 'Compact')) "$style style is persisted from Settings"
        foreach ($mode in @('Persistent', 'Fading', 'None')) {
            foreach ($control in @('microphoneMuteBannerComboBox', 'microphoneUnmuteBannerComboBox')) {
                Set-SoundSwitchComboBoxItem -WindowHandle $settings.Handle -AutomationId $control -Target $mode | Out-Null
            }
            if ($style -eq 'Normal' -and $mode -eq 'Persistent') {
                Wait-Banner $true
                $initialBanner = @(Get-Banners)[0]
                $initialTitle = (Find-SoundSwitchElement -Handle $initialBanner.Handle -AutomationId lblTop).Current.Name
                $match = [regex]::Match($initialTitle, '^(?<name>.+) is now (?:muted|unmuted)\.$')
                if (-not $match.Success) { throw "Unexpected microphone banner title: $initialTitle" }
                $microphoneName = $match.Groups['name'].Value
            }
            foreach ($muted in @($true, $false)) {
                Set-MicrophoneState (-not $muted)
                if ($mode -eq 'Fading') { Wait-Banner $false }
                Set-MicrophoneState $muted
                if ($mode -eq 'None') { Assert-BannerRemains 2 $false; continue }
                Wait-Banner $true
                $banner = @(Get-Banners)[0]
                $label = Find-SoundSwitchElement -Handle $banner.Handle -AutomationId lblTop
                $expectedTitle = if ($style -eq 'Compact') {
                    if ($muted) { 'Microphone Off' } else { 'Microphone On' }
                }
                else {
                    if ($muted) { "$microphoneName is now muted." } else { "$microphoneName is now unmuted." }
                }
                Assert-True ($label.Current.Name -eq $expectedTitle) "$style/$mode shows the original style-specific microphone presentation"
                if ($style -eq 'Compact') {
                    $description = Find-SoundSwitchElement -Handle $banner.Handle -AutomationId lblTitle
                    Assert-True ($description.Current.Name -eq $microphoneName) 'Microphone names match Settings and live notifications'
                }
                if ($mode -eq 'Persistent') {
                    Assert-BannerRemains 5 $true # exceeds both the old 1.5s exception and configured TTL
                    if (-not $muted) {
                        if ($style -eq 'Normal') { $normalTextHeight = $label.Current.BoundingRectangle.Height }
                        else {
                            Assert-True ($label.Current.BoundingRectangle.Height -lt $normalTextHeight) 'Compact uses smaller text than Normal'
                        }
                    }
                }
                else {
                    Assert-BannerRemains 2 $true # must not use the old 1.5s timeout
                    Wait-Banner $false
                    Assert-True $true "$style fading banner expires using On-screen time"
                }
            }
        }
    }
    Write-Host "END-TO-END PASSED: $checks checks." -ForegroundColor Green
}
finally {
    try {
        if ($null -ne $instance -and -not $instance.HasExited -and $null -ne $originalMute) {
            Set-MicrophoneState $originalMute
        }
    }
    finally {
        # Stop before restoring the file: an app holding scratch config must not overwrite it.
        if ($null -ne $instance -and -not $instance.HasExited) {
            $instance.Kill()
            $instance.WaitForExit()
        }
        if ($hadConfig) { Copy-Item $backupPath $configPath -Force }
        elseif (Test-Path $configPath) { Remove-Item $configPath -Force }
    }
}
