# SCRIPT-TEMPLATE.md — skeleton for `tools/Test-<Feature>.ps1`

Mirrors `tools/Test-MuteTarget.ps1`. Rules the skeleton encodes:

- `#Requires -Version 7.0` first line; `[CmdletBinding()]`; `-ErrorActionPreference = 'Stop'`
  and `Set-StrictMode -Version Latest`.
- Params: `-All`, optionally `-Audio` (only if the feature touches the COM audio
  stack), `-List`, `-NoBuild`.
- `$featureFixtures` = the feature's fixture list, each with a one-line comment
  naming the behaviour it covers.
- **Known-flaky tests are excluded by an explicit test filter on every run mode
  (`-All` included), and the exclude list lives in exactly one place in the
  script.** `$flakyExclusions` below is composed into the single filter that
  every run of the test project uses. A documented exclusion that does not apply
  on `-All` is a bug: the suite still fails and the doc lies.
- One solution build, then reuse it (`-NoBuild` sets itself).

Replace `<Feature>`, fixture names, and comments; delete the `-Audio` branch if
not applicable; keep the trailing `All requested tests passed.` summary.

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

    <FlakyTestName>.<TestName> is excluded on every run mode: it is a
    timing-sensitive race that CI also skips and fails intermittently on fast
    local machines regardless of code changes. The rest of the fixture still
    runs.

.PARAMETER All
    Run the full SoundSwitch.Tests suite instead of just the feature fixtures.

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
$flakyExclusions = @(
    'Name!=<FlakyTestName>'   # <why: timing-sensitive race; CI skips it too>
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

# One filter for every run mode: fixture selection (if any) + flaky exclusions.
if ($All) {
    $testFilter = $flakyExclusions -join '&'
}
else {
    $fixtureFilter = ($featureFixtures | ForEach-Object { "FullyQualifiedName~$_" }) -join '|'
    $testFilter = "($fixtureFilter)&$($flakyExclusions -join '&')"
}

Invoke-TestProject -Project $testProject -Filter $testFilter -Label '<Feature> feature tests'

Write-Host ''
Write-Host 'All requested tests passed.' -ForegroundColor Green
Write-Host ''
Write-Host 'End-to-end check (requires a running SoundSwitch + CLI):'
Write-Host '  pwsh tools\Test-<Feature>EndToEnd.ps1'   # delete if no E2E layer
```
