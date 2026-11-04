# SCRIPT-TEMPLATE.md — skeleton for `tools/Test-<Feature>.ps1`

Mirrors `tools/Test-MuteTarget.ps1`. Rules the skeleton encodes:

- `#Requires -Version 7.0` first line; `[CmdletBinding()]`; `-ErrorActionPreference = 'Stop'`
  and `Set-StrictMode -Version Latest`.
- Params: `-All`, optionally `-Audio` (only if the feature touches the COM audio
  stack), `-List`, `-NoBuild`. The `-Audio` support is three pieces (param, test
  project path, execution branch) — include all three or none.
- `$featureFixtures` = the feature's fixture list, each with a one-line comment
  naming the behaviour it covers.
- **Known-flaky tests are excluded by an explicit test filter on every run mode
  (`-All` included), and the exclude list lives in exactly one place in the
  script.** `$flakyExclusions` below is composed into the single filter that
  every run of the test project uses. A documented exclusion that does not apply
  on `-All` is a bug: the suite still fails and the doc lies.
- One solution build, then reuse it: after the build `-NoBuild` sets itself, so
  `dotnet test` runs with `--no-restore --no-build` and never rebuilds.

Replace `<Feature>`, fixture names, and comments; delete the `-Audio` param,
project path, and branch together when the feature does not need audio tests;
keep the trailing `All requested tests passed.` summary.

---

```powershell
#Requires -Version 7.0
<#
.SYNOPSIS
    Runs the test coverage for the <feature> feature.

.DESCRIPTION
    Runs the unit tests covering <feature> in SoundSwitch.Tests: <areas>.

    By default only the feature-focused fixtures run, which takes seconds. Use
    -All to run the entire SoundSwitch.Tests suite, or -Audio to also run the
    SoundSwitch.Audio.Manager.Tests project (COM/hardware dependent, Windows
    only).

    <FlakyTestName>.<TestName> is excluded on every run mode (including -All):
    it is a timing-sensitive race that CI also skips and fails intermittently
    on fast local machines regardless of code changes. The rest of the fixture
    still runs.

.PARAMETER All
    Run the full SoundSwitch.Tests suite instead of just the feature fixtures.

.PARAMETER Audio
    Additionally run the SoundSwitch.Audio.Manager.Tests project (COM audio
    stack, Windows only, slower).

.PARAMETER List
    List the feature test fixtures without running anything.

.EXAMPLE
    pwsh tools\Test-<Feature>.ps1
    Runs the feature-focused unit tests (fast path).

.EXAMPLE
    pwsh tools\Test-<Feature>.ps1 -All
    Runs the whole SoundSwitch.Tests suite.
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

if (-not (Test-Path $testProject)) {
    throw "Test project not found: $testProject"
}

# Feature fixtures: everything that exercises the <feature> feature.
$featureFixtures = @(
    '<FixtureName1>',   # <what it covers>
    '<FixtureName2>'    # <what it covers>
)

# Known-flaky tests, excluded on every run mode (documented above). One place,
# used by the main path AND -All, so the documented exclusion actually applies.
# The filter key is the test METHOD name (Name!), not the fixture type name.
$flakyExclusions = @(
    'Name!=<TestName>'   # <why: timing-sensitive race; CI skips it too>
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

Invoke-TestProject -Project $testProject -Filter $testFilter -Label '<Feature> feature tests'

if ($Audio) {
    $audioTestProject = Join-Path $repoRoot 'SoundSwitch.Audio.Manager.Tests\SoundSwitch.Audio.Manager.Tests.csproj'
    if (-not (Test-Path $audioTestProject)) {
        throw "Audio test project not found: $audioTestProject"
    }
    else {
        Invoke-TestProject -Project $audioTestProject -Filter $null -Label 'SoundSwitch.Audio.Manager.Tests (COM audio stack)'
    }
}

Write-Host ''
Write-Host 'All requested tests passed.' -ForegroundColor Green
Write-Host ''
Write-Host 'End-to-end check (requires a running SoundSwitch + CLI):'
Write-Host '  pwsh tools\Test-<Feature>EndToEnd.ps1'   # delete if no E2E layer
```
