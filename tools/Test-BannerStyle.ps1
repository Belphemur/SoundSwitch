#Requires -Version 7.0
<#
.SYNOPSIS
    Runs banner style and microphone notification lifetime regression tests.
.DESCRIPTION
    Builds once, then tests persistence, content, timers, reconciliation, appearance,
    and DPI layout. Excludes the known-flaky COM refresh race TestMultipleRefresh
    on every path, including -All. Live UI coverage is in Test-BannerStyleEndToEnd.ps1.
.PARAMETER All
    Run the whole SoundSwitch.Tests suite.
.PARAMETER List
    List feature fixtures without building.
.PARAMETER NoBuild
    Reuse the existing Debug binaries.
.EXAMPLE
    pwsh tools/Test-BannerStyle.ps1
.EXAMPLE
    pwsh tools/Test-BannerStyle.ps1 -All -NoBuild
#>
[CmdletBinding()]
param([switch]$All, [switch]$List, [switch]$NoBuild)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path -Parent $PSScriptRoot
$featureFixtures = @(
    'BannerStyleTests',                 # global style config, timers, both persistent states, reconciliation
    'BannerAppearanceTests',            # icon/font proportions and scale
    'MicrophoneMuteBannerManagerTests', # disposed-window safety and stacking
    'NotificationManagerMuteTests',     # notification initialization safety
    'NotificationContentBuilderTests',  # banner/toast content isolation
    'SettingsBannerLayoutTests'         # appended controls at different DPIs
)
$exclusions = @('Name!=TestMultipleRefresh') # timing-sensitive COM refresh race, skipped in CI too
if ($List) { $featureFixtures | ForEach-Object { Write-Host $_ }; return }
if (-not $NoBuild) {
    & dotnet build (Join-Path $root 'SoundSwitch.sln') -c Debug --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}
$filter = $exclusions -join '&'
if (-not $All) {
    $fixtures = ($featureFixtures | ForEach-Object { "FullyQualifiedName~$_" }) -join '|'
    $filter = "($fixtures)&$filter"
}
& dotnet test (Join-Path $root 'SoundSwitch.Tests/SoundSwitch.Tests.csproj') -c Debug --no-build --no-restore --filter $filter
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host 'Banner style tests passed.' -ForegroundColor Green
