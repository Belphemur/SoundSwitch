<#
.SYNOPSIS
    Builds the SoundSwitch installer from a local build or downloaded release.

.DESCRIPTION
    This script automates the full installer build process:

    1. If -DownloadRelease is set, downloads the latest release artifact using
       tools\Download-Release.ps1 into the Final\ directory.
    2. Otherwise, builds the SoundSwitch binaries from source using dotnet publish.
    3. Generates HTML documentation from Markdown sources (Changelog, README, Terms).
    4. Bundles additional assets (images, licenses).
    5. Optionally signs binaries and the final installer with tools\Sign-Binary.ps1.
    6. Compiles the Inno Setup installer.

    This is the PowerShell replacement for the local build workflow.  It requires
    the tools from Install-BuildTools.ps1 (Inno Setup, Python with markdown, and
    optionally signtool + Certum SimplySign for code signing).

.PARAMETER Configuration
    Build configuration: Release (default) or Debug.

.PARAMETER DownloadRelease
    When set, downloads a pre-built release instead of building from source.

.PARAMETER Channel
    Release channel when downloading: 'release' (default) or 'beta'.
    Only used when -DownloadRelease is set.

.PARAMETER SkipSigning
    Skip code signing even when signtool is available.

.PARAMETER CertificateName
    Subject name (CN) of the code-signing certificate passed to Sign-Binary.ps1.
    Defaults to "OpenSource Developer, Antoine Aflalo".

.PARAMETER InstallerReleaseState
    The release state label passed to Inno Setup (e.g. Release, Beta, Nightly).

.EXAMPLE
    .\tools\Build-Installer.ps1
    Builds from source (Release config) and creates the installer.

.EXAMPLE
    .\tools\Build-Installer.ps1 -DownloadRelease -Channel beta
    Downloads the latest beta release and builds the installer from it.

.EXAMPLE
    .\tools\Build-Installer.ps1 -SkipSigning -InstallerReleaseState Nightly
    Builds from source without signing, using "Nightly" label for installer.
#>

[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',

    [switch]$DownloadRelease,

    [ValidateSet('release', 'beta')]
    [string]$Channel = 'release',

    [switch]$SkipSigning,

    [string]$CertificateName = 'OpenSource Developer, Antoine Aflalo',

    [string]$InstallerReleaseState = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Paths ────────────────────────────────────────────────────────────────────

$repoRoot = Split-Path $PSScriptRoot -Parent
$finalDir = Join-Path $repoRoot 'Final'
$projectName = 'SoundSwitch'
$cliProject = 'SoundSwitch.CLI'

# ── Detect target framework ─────────────────────────────────────────────────

$csprojPath = Join-Path $repoRoot "$projectName\$projectName.csproj"
[xml]$project = Get-Content $csprojPath
$framework = $project.Project.PropertyGroup.TargetFramework | Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($framework)) {
    throw "Unable to determine TargetFramework from $csprojPath"
}

Write-Host "Detected target framework: $framework" -ForegroundColor Cyan

# ── Step 1: Populate Final\ ─────────────────────────────────────────────────

if ($DownloadRelease) {
    Write-Host "`n=== Downloading release artifact ($Channel) ===" -ForegroundColor White
    $downloadScript = Join-Path $PSScriptRoot 'Download-Release.ps1'
    & $downloadScript -Channel $Channel -OutputDir $finalDir
}
else {
    Write-Host "`n=== Building from source ($Configuration) ===" -ForegroundColor White

    # Clean
    foreach ($dir in @('bin', 'obj', 'Release', $finalDir)) {
        $fullPath = if ([System.IO.Path]::IsPathRooted($dir)) { $dir } else { Join-Path $repoRoot $dir }
        if (Test-Path $fullPath) {
            Remove-Item $fullPath -Recurse -Force
        }
    }
    New-Item -ItemType Directory -Path $finalDir -Force | Out-Null

    # Publish CLI first, then main app (main app wins on shared files)
    foreach ($proj in @($cliProject, $projectName)) {
        $projPath = Join-Path $repoRoot "$proj\$proj.csproj"
        Write-Host "  Publishing $proj ..."
        dotnet publish -c $Configuration $projPath -o $finalDir
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet publish failed for $proj with exit code $LASTEXITCODE."
        }
    }
}

# ── Step 2: Generate HTML documentation ──────────────────────────────────────

Write-Host "`n=== Generating HTML documentation ===" -ForegroundColor White

$markdownTool = Join-Path $PSScriptRoot 'markdown_to_html.py'

$mdFiles = @(
    @{ Source = 'CHANGELOG.md'; Output = 'Changelog.html' }
    @{ Source = 'README.md';    Output = 'Readme.html' }
    @{ Source = 'Terms.md';     Output = 'Terms.html' }
    @{ Source = 'README.de.md'; Output = 'Readme.de.html' }
)

foreach ($md in $mdFiles) {
    $srcPath = Join-Path $repoRoot $md.Source
    $outPath = Join-Path $finalDir $md.Output

    if (Test-Path $srcPath) {
        Write-Host "  Converting $($md.Source) -> $($md.Output)"
        python $markdownTool $srcPath -o $outPath
        if ($LASTEXITCODE -ne 0) {
            throw "HTML generation failed for $($md.Source)."
        }
    }
    else {
        Write-Host "  Skipping $($md.Source) (not found)" -ForegroundColor DarkGray
    }
}

# ── Step 3: Bundle additional assets ─────────────────────────────────────────

Write-Host "`n=== Bundling assets ===" -ForegroundColor White

$assets = @(
    @{ Source = 'img\soundSwitched.png';    Dest = $finalDir }
    @{ Source = 'SoundSwitch.CLI\README.md'; Dest = $finalDir }
    @{ Source = 'LICENSE.txt';               Dest = $finalDir }
    @{ Source = 'Terms.txt';                 Dest = $finalDir }
)

foreach ($asset in $assets) {
    $srcPath = Join-Path $repoRoot $asset.Source
    if (Test-Path $srcPath) {
        Copy-Item $srcPath -Destination $asset.Dest -Force
        Write-Host "  Copied $($asset.Source)"
    }
    else {
        Write-Host "  Skipping $($asset.Source) (not found)" -ForegroundColor DarkGray
    }
}

# ── Step 4: Code signing (binaries) ──────────────────────────────────────────

$signScript = Join-Path $PSScriptRoot 'Sign-Binary.ps1'
$canSign    = -not $SkipSigning -and (Get-Command 'signtool.exe' -ErrorAction SilentlyContinue)

if ($canSign) {
    Write-Host "`n=== Signing binaries ===" -ForegroundColor White

    $binaries = @("$projectName.exe", "$cliProject.exe") |
        ForEach-Object { Join-Path $finalDir $_ } |
        Where-Object  { Test-Path $_ }

    if ($binaries) {
        & $signScript -Path $binaries -CertificateName $CertificateName
    }
    else {
        Write-Host "  No executables found to sign." -ForegroundColor DarkGray
    }
}
else {
    if ($SkipSigning) {
        Write-Host "`n=== Skipping signing (-SkipSigning) ===" -ForegroundColor Yellow
    }
    else {
        Write-Host "`n=== Skipping signing (signtool.exe not found) ===" -ForegroundColor Yellow
    }
}

# ── Step 5: Build installer ──────────────────────────────────────────────────

Write-Host "`n=== Building installer ===" -ForegroundColor White

$makeInstallerBat = Join-Path $repoRoot 'Installer\Make-Installer.bat'
if (-not (Test-Path $makeInstallerBat)) {
    throw "Installer\Make-Installer.bat not found at $makeInstallerBat."
}

& cmd.exe /c "`"$makeInstallerBat`" $InstallerReleaseState"
if ($LASTEXITCODE -ne 0) {
    throw "Installer build failed with exit code $LASTEXITCODE."
}

# ── Done ─────────────────────────────────────────────────────────────────────

Write-Host "`n=================================" -ForegroundColor White
Write-Host "Installer built successfully!" -ForegroundColor Green

$installerDir = Join-Path $finalDir 'Installer'
if (Test-Path $installerDir) {
    $installers = Get-ChildItem $installerDir -Filter '*Installer.exe'
    if ($installers) {
        # ── Step 6: Sign the installer ───────────────────────────────────────
        if ($canSign) {
            Write-Host "`n=== Signing installer ===" -ForegroundColor White
            $installerPaths = $installers | ForEach-Object { $_.FullName }
            & $signScript -Path $installerPaths -CertificateName $CertificateName
        }

        Write-Host "`nInstaller(s):"
        foreach ($ins in $installers) {
            Write-Host "  $($ins.FullName)" -ForegroundColor Cyan
        }
    }
}

Write-Host ""
