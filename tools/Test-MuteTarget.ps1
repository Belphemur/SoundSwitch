#Requires -Version 7.0
<#
.SYNOPSIS
    Runs the test coverage for the microphone mute targeting / banner scope feature.

.DESCRIPTION
    Runs the unit tests covering the microphone mute target feature: configuration
    (persisted target, banner scope flag, legacy migration), device identity
    resolution (the 3-tier match used to find the configured target after driver
    updates or re-enumeration), banner reconciliation/pinning, mute notification
    content, and the settings layout that exposes the feature.

    By default only the feature-focused fixtures run, which takes seconds. Use
    -All to run the entire SoundSwitch.Tests suite, or -Audio to also run the
    SoundSwitch.Audio.Manager.Tests project (COM/hardware dependent, Windows only).

    RefreshDeviceTests.TestMultipleRefresh is excluded: it is a timing-sensitive
    COM refresh race that CI also skips (Assert.Ignore) and fails intermittently
    on fast local machines regardless of code changes. The other tests in the
    fixture (refresh disposal/reconcile) still run.

    To verify the feature end-to-end against a running SoundSwitch instance, use
    the CLI: `SoundSwitch.CLI.exe mute` reports the mute state of the configured
    target (or the Windows default input as fallback), and `-t` toggles it.

.PARAMETER All
    Run the full SoundSwitch.Tests suite instead of just the feature fixtures.

.PARAMETER Audio
    Additionally run the SoundSwitch.Audio.Manager.Tests project (COM audio
    stack, Windows only, slower).

.PARAMETER List
    List the feature test fixtures without running anything.

.EXAMPLE
    pwsh tools\Test-MuteTarget.ps1
    Runs the feature-focused unit tests (fast path).

.EXAMPLE
    pwsh tools\Test-MuteTarget.ps1 -All
    Runs the whole SoundSwitch.Tests suite.

.EXAMPLE
    pwsh tools\Test-MuteTarget.ps1 -Audio
    Runs the feature tests plus the COM audio manager tests.
#>
[CmdletBinding()]
param(
    [switch]$All,
    [switch]$Audio,
    [switch]$List,
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$testProject = Join-Path $repoRoot 'SoundSwitch.Tests\SoundSwitch.Tests.csproj'
$audioTestProject = Join-Path $repoRoot 'SoundSwitch.Audio.Manager.Tests\SoundSwitch.Audio.Manager.Tests.csproj'

if (-not (Test-Path $testProject)) {
    throw "Test project not found: $testProject"
}

# Feature fixtures: everything that exercises the mute target + banner scope feature.
$featureFixtures = @(
    'MuteTargetConfigurationTests',        # persisted mute target + LimitMuteBannersToTarget, legacy migration
    'AppRuleDeviceResolverTests',          # 3-tier device identity match used to resolve the target
    'DeviceReadOnlyCollectionMatchTests',  # matching machinery behind the resolver
    'DeviceInfoSerializationTests',        # DeviceInfo snapshot (de)serialization round-trips
    'BannerMonitorTests',                  # pinned banner monitor detection
    'BannerAppearanceTests',               # banner scale/position appearance resolution
    'BannerPositionTests',                 # banner position bounds
    'BannerPositionMigrationTests',        # banner position config migration
    'NotificationContentBuilderTests',     # mute/mute banner title and icon content
    'SettingsBannerLayoutTests',           # settings UI layout for banner options
    'RefreshDeviceTests',                  # device refresh/cancellation (device list events)
    'DeviceCyclerTests'                    # device cycling across missing/present devices
)

if ($List) {
    Write-Host 'Feature test fixtures:'
    $featureFixtures | ForEach-Object { Write-Host "  $_" }
    return
}

function Invoke-TestProject {
    param(
        [string]$Project,
        [string]$Filter,
        [string]$Label
    )

    Write-Host ''
    Write-Host "=== $Label ===" -ForegroundColor Cyan

    $args = @('test', $Project, '-c', 'Debug')
    if (-not $NoBuild) {
        $args += @('--no-restore')
    }
    if ($Filter) {
        $args += @('--filter', $Filter)
    }
    $args += @('--logger', 'console;verbosity=normal')

    & dotnet @args
    if ($LASTEXITCODE -ne 0) {
        Write-Host "FAILED: $Label" -ForegroundColor Red
        exit $LASTEXITCODE
    }
    Write-Host "PASSED: $Label" -ForegroundColor Green
}

$buildSw = $false
if (-not $NoBuild) {
    Write-Host 'Building solution (Debug)...' -ForegroundColor Cyan
    & dotnet build (Join-Path $repoRoot 'SoundSwitch.sln') -c Debug --nologo -v minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Host 'FAILED: build' -ForegroundColor Red
        exit $LASTEXITCODE
    }
    Write-Host 'PASSED: build' -ForegroundColor Green
    $NoBuild = $true
}

if ($All) {
    Invoke-TestProject -Project $testProject -Filter $null -Label 'SoundSwitch.Tests (full suite)'
}
else {
    $featureFilter = ($featureFixtures | ForEach-Object { "FullyQualifiedName~$_" }) -join '|'
    # Skip the timing-sensitive COM race test; CI ignores it for the same reason.
    $featureFilter = "($featureFilter)&Name!=TestMultipleRefresh"
    Invoke-TestProject -Project $testProject -Filter $featureFilter -Label 'Mute target + banner scope feature tests'
}

if ($Audio) {
    if (-not (Test-Path $audioTestProject)) {
        Write-Warning "Audio test project not found: $audioTestProject"
    }
    else {
        Invoke-TestProject -Project $audioTestProject -Filter $null -Label 'SoundSwitch.Audio.Manager.Tests (COM audio stack)'
    }
}

Write-Host ''
Write-Host 'All requested tests passed.' -ForegroundColor Green
Write-Host ''
Write-Host 'End-to-end manual check (requires a running SoundSwitch + CLI):'
Write-Host '  SoundSwitch.CLI.exe mute        # report mute state of configured target (or default input fallback)'
Write-Host '  SoundSwitch.CLI.exe mute -t     # toggle mute; verify the banner scope behaviour on screen'
