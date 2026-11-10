#Requires -Version 7.0
<#
.SYNOPSIS
    Tests global banner style and symmetric microphone lifetimes in the real app.
.DESCRIPTION
    Requires an interactive Windows desktop and a usable default microphone. Refuses
    to touch an existing SoundSwitch instance. Backs up configuration, uses English
    UI, drives style and mode combo boxes through UI Automation, and checks banner
    presence/lifetime for both microphone states. Pins the test to one endpoint and
    restores it independently of the app, even after an app crash. Stops only its
    own process, clears its ownership record, then restores configuration.
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
$microphone = $null
$checks = 0
$microphoneProject = Join-Path $PSScriptRoot 'TestMicrophoneControl/TestMicrophoneControl.csproj'
# Build the independent cleanup tool even with -NoBuild: that switch reuses app binaries.
& dotnet build $microphoneProject -c Debug --nologo -v minimal
if ($LASTEXITCODE -ne 0) { throw 'Microphone control helper build failed.' }
$microphoneHelper = Join-Path $PSScriptRoot 'TestMicrophoneControl/bin/Debug/net10.0-windows/TestMicrophoneControl.dll'

function Assert-True([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw "FAIL: $Message" }
    $script:checks++
    Write-Host "PASS: $Message" -ForegroundColor Green
}
function Invoke-MicrophoneControl([string[]]$Arguments) {
    $output = & dotnet $microphoneHelper @Arguments
    if ($LASTEXITCODE -ne 0) { throw "Microphone control failed: $Arguments" }
    return ($output | Out-String | ConvertFrom-Json)
}
function Set-MicrophoneState([bool]$Muted) {
    if ($null -eq $microphone) { throw 'No microphone endpoint was captured for this test.' }
    $state = $Muted.ToString().ToLowerInvariant()
    $result = Invoke-MicrophoneControl -Arguments @('set', $microphone.id, $state)
    if ($result.id -ne $microphone.id -or $result.isMuted -ne $Muted) {
        throw "Could not set microphone '$($microphone.id)' to $state. No default-device fallback is allowed."
    }
}
function Get-BannerElementSnapshot([IntPtr]$Handle, [string]$AutomationId, [string]$Context) {
    try {
        $element = Find-SoundSwitchElement -Handle $Handle -AutomationId $AutomationId
        if ($null -eq $element) { throw "Control '$AutomationId' was not found." }
        $current = $element.Current
        return [pscustomobject]@{ Name = $current.Name; Height = $current.BoundingRectangle.Height }
    }
    catch {
        throw "Cannot inspect $Context banner (handle $Handle), control '$AutomationId': $($_.Exception.Message)"
    }
}
function Get-RequiredBanner([string]$Context) {
    $banners = @(Get-Banners)
    if ($banners.Count -eq 0) { throw "No banner was found for $Context; it may have closed during lookup." }
    return $banners[0]
}
function Stop-BannerStyleTestInstance {
    try {
        if ($null -ne $instance -and -not $instance.HasExited) {
            $instance.Kill()
            $instance.WaitForExit()
        }
    }
    finally {
        # Do not erase another control invocation's ownership record.
        if ($null -ne $instance -and $instance.HasExited -and (Read-SoundSwitchControlPid) -eq $instance.Id) {
            Remove-Item -LiteralPath $Script:PidFile -Force -ErrorAction SilentlyContinue
        }
    }
}
function Restore-BannerStyleTestState {
    try {
        if ($null -ne $originalMute) {
            Set-MicrophoneState $originalMute
        }
    }
    finally {
        # Stop before restoring the file: an app holding scratch config must not overwrite it.
        try {
            Stop-BannerStyleTestInstance
        }
        finally {
            if ($null -ne $instance -and -not $instance.HasExited) {
                throw "Test instance could not be stopped. Restore configuration from '$backupPath' after stopping it."
            }
            if ($hadConfig) { Copy-Item $backupPath $configPath -Force }
            elseif (Test-Path $configPath) { Remove-Item $configPath -Force }
        }
    }
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
function Get-ExpectedMicrophoneTitle([string]$Style, [bool]$Muted, [string]$MicrophoneName) {
    if ($Style -eq 'Compact') {
        if ($Muted) { return 'Microphone Off' }
        return 'Microphone On'
    }
    if ($Muted) { return "$MicrophoneName is now muted." }
    return "$MicrophoneName is now unmuted."
}
function Wait-ExpectedBanner([string]$ExpectedTitle, [string]$Context, [string]$ExpectedDescription,
    [int]$WaitSeconds = 10) {
    # Endpoint writes complete before the app's asynchronous volume event is rendered.
    # A visible persistent window is not proof that it has adopted the new state.
    $deadline = (Get-Date).AddSeconds($WaitSeconds)
    $lastTitle = '<no banner>'
    $lastError = '<none>'
    do {
        try {
            foreach ($candidate in @(Get-Banners)) {
                $label = Get-BannerElementSnapshot $candidate.Handle 'lblTop' $Context
                $lastTitle = $label.Name
                if ($label.Name -ne $ExpectedTitle) { continue }
                $description = $null
                if (-not [string]::IsNullOrEmpty($ExpectedDescription)) {
                    $description = Get-BannerElementSnapshot $candidate.Handle 'lblTitle' $Context
                    if ($description.Name -ne $ExpectedDescription) { continue }
                }
                return [pscustomobject]@{ Handle = $candidate.Handle; Label = $label; Description = $description }
            }
        }
        catch {
            # A form/control can disappear between discovery and reading Current.
            $lastError = $_.Exception.Message
        }
        if ((Get-Date) -ge $deadline) { break }
        Start-Sleep -Milliseconds 100
    } while ($true)
    throw "Timed out waiting for $Context banner title '$ExpectedTitle' and description '$ExpectedDescription'. Last title: '$lastTitle'. Last lookup error: $lastError"
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
    $microphone = Invoke-MicrophoneControl -Arguments @('snapshot')
    $originalMute = [bool]$microphone.isMuted
    # A minimal scratch config avoids activating user profiles/rules during the test.
    $config = @{
        FirstRun = $false; Language = 0; Telemetry = $false; NotificationAdvancedMode = $true
        SwitchDeviceNotification = 5; SwitchProfileNotification = 2; MicrophoneMuteNotification = 5
        MicrophoneMuteBanner = 0; MicrophoneUnmuteBanner = 0; BannerOnScreenTime = '00:00:04'
        BannerDisplayInfo = 0; BannerScalePercentage = 100; BannerStyle = 0
        LimitMuteBannersToTarget = $true; MaxNumberNotification = 1
        MuteRecordingDevice = @{
            Name = $microphone.name; Id = $microphone.id; Type = 1; IsUsb = $microphone.isUsb
            DiscoveredAt = [DateTime]::UtcNow.ToString('o')
        }
        # Do not replay the legacy Windows per-app endpoint reset on the test machine.
        MigratedFields = @('PersistentMuteNotification', 'SplitNotificationSettings', 'SwitchForegroundProgram_cleanup')
    }
    New-Item (Split-Path -Parent $configPath) -ItemType Directory -Force | Out-Null
    $config | ConvertTo-Json -Depth 20 | Set-Content $configPath -Encoding utf8
    $instance = Start-SoundSwitchApp -AppExe $appExe -CliExe $cliExe -TimeoutSeconds $TimeoutSeconds
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
                $initialBanner = Get-RequiredBanner "$style/$mode initial state"
                $initialTitle = (Get-BannerElementSnapshot $initialBanner.Handle 'lblTop' "$style/$mode initial state").Name
                $match = [regex]::Match($initialTitle, '^(?<name>.+) is now (?:muted|unmuted)\.$')
                if (-not $match.Success) { throw "Unexpected microphone banner title: $initialTitle" }
                $microphoneName = $match.Groups['name'].Value
            }
            foreach ($muted in @($true, $false)) {
                $expectedDescription = if ($style -eq 'Compact') { $microphoneName } else { $null }
                Set-MicrophoneState (-not $muted)
                if ($mode -eq 'Persistent') {
                    # Observe the intermediate state too: the old title might already match
                    # the final state while both audio events are still waiting to render.
                    $oppositeTitle = Get-ExpectedMicrophoneTitle $style (-not $muted) $microphoneName
                    Wait-ExpectedBanner $oppositeTitle "$style/$mode/opposite muted=$(-not $muted)" $expectedDescription | Out-Null
                }
                elseif ($mode -eq 'Fading') { Wait-Banner $false }
                Set-MicrophoneState $muted
                if ($mode -eq 'None') { Assert-BannerRemains 2 $false; continue }
                $context = "$style/$mode/muted=$muted"
                $expectedTitle = Get-ExpectedMicrophoneTitle $style $muted $microphoneName
                $banner = Wait-ExpectedBanner $expectedTitle $context $expectedDescription
                $label = $banner.Label
                Assert-True ($label.Name -eq $expectedTitle) "$style/$mode shows the original style-specific microphone presentation"
                if ($style -eq 'Compact') {
                    $description = $banner.Description
                    Assert-True ($description.Name -eq $microphoneName) 'Microphone names match Settings and live notifications'
                }
                if ($mode -eq 'Persistent') {
                    Assert-BannerRemains 5 $true # exceeds both the old 1.5s exception and configured TTL
                    if (-not $muted) {
                        if ($style -eq 'Normal') { $normalTextHeight = $label.Height }
                        else {
                            Assert-True ($label.Height -lt $normalTextHeight) 'Compact uses smaller text than Normal'
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
    Restore-BannerStyleTestState
}
