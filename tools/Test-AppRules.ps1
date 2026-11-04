#Requires -Version 7.0
<#
.SYNOPSIS
    Runs the test coverage for the App Rules identity-based assignment feature.

.DESCRIPTION
    Runs the unit tests covering the App Rules / identity assignment feature in
    SoundSwitch.Tests: the enforcement enum round-trip + migration normalization +
    assignment validator constraints, the ownership reconcile state machine (ledger vs
    Windows' per-app persisted-endpoint store), the ledger persistence itself, the
    volume-relative identity matching shared with the process picker, and the
    DefaultEndpoint store parsing the picker and reconciler read.

    The fixtures are deterministic: AppRuleAssignmentReconcilerTests and
    PersistedDefaultEndpointStoreTests run against scratch registry roots under
    HKCU\Software\SoundSwitch\Tests (unique per run, torn down in the fixture). The only
    excluded test is RefreshDeviceTests.TestMultipleRefresh (see below), which is not
    part of the feature.

    By default only the feature-focused fixtures run, which takes seconds. Use -All to
    run the entire SoundSwitch.Tests suite.

    To verify the feature end-to-end against a real instance, run:
        pwsh tools\Test-AppRulesEndToEnd.ps1
    (requires a desktop session; on a headless box only the unit layer runs).

.PARAMETER All
    Run the full SoundSwitch.Tests suite instead of just the feature fixtures.

.PARAMETER List
    List the feature test fixtures without running anything.

.PARAMETER NoBuild
    Skip the solution build and run the tests against the existing binaries.

.EXAMPLE
    pwsh tools\Test-AppRules.ps1
    Runs the feature-focused unit tests (fast path).

.EXAMPLE
    pwsh tools\Test-AppRules.ps1 -All
    Runs the whole SoundSwitch.Tests suite.
#>
[CmdletBinding()]
param(
    [switch]$All,
    [switch]$List,
    [switch]$NoBuild
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repoRoot = Split-Path -Parent $PSScriptRoot
$testProject = Join-Path $repoRoot 'SoundSwitch.Tests\SoundSwitch.Tests.csproj'

if (-not (Test-Path $testProject)) {
    throw "Test project not found: $testProject"
}

# Feature fixtures: everything that exercises the App Rules identity-assignment feature.
$featureFixtures = @(
    'AppRuleEnforcementTests',             # Enforcement serialization round-trip, migrator fallback to Dynamic, assignment validation (ADR §6.1, §8)
    'AppRuleAssignmentReconcilerTests',    # ownership reconcile state machine vs a scratch persisted store (ADR §6.4)
    'AssignmentLedgerTests',               # ledger entries survive a configuration round-trip (ADR §6.4)
    'AppRuleIdentityMatcherTests',         # NT-device vs drive-letter identity matching (picker + reconciler shared)
    'PersistedDefaultEndpointStoreTests'   # Windows' DefaultEndpoint store parsing against a scratch registry root
)

# Known-flaky tests, excluded on every run mode (documented above). One place, composed
# into the single filter used by the feature path AND -All. The filter key is the test
# METHOD name (Name!), not the fixture type name.
$flakyExclusions = @(
    'Name!=TestMultipleRefresh'   # timing-sensitive COM refresh race in RefreshDeviceTests; CI ignores it (Assert.Ignore) and it fails intermittently on fast machines regardless of code changes
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
    if ($NoBuild) {
        # The solution build already happened (or -NoBuild was requested):
        # reuse the built outputs instead of rebuilding.
        $args += @('--no-restore', '--no-build')
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

# One filter for every run mode: fixture selection (if any) + flaky exclusions.
# Guard against an empty exclusions list: a trailing '&' is an invalid filter.
$exclusionFilter = $flakyExclusions -join '&'
$testFilter = $exclusionFilter
if (-not $All) {
    $fixtureFilter = ($featureFixtures | ForEach-Object { "FullyQualifiedName~$_" }) -join '|'
    $testFilter = "($fixtureFilter)"
    if ($exclusionFilter) {
        $testFilter += "&$exclusionFilter"
    }
}

Invoke-TestProject -Project $testProject -Filter $testFilter -Label 'App Rules identity-assignment feature tests'

Write-Host ''
Write-Host 'All requested tests passed.' -ForegroundColor Green
Write-Host ''
Write-Host 'End-to-end check (requires a desktop session with the app launched by the script):'
Write-Host '  pwsh tools\Test-AppRulesEndToEnd.ps1'
