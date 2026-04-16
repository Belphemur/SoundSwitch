<#
.SYNOPSIS
    Builds the SoundSwitch installer from a local build or pre-populated Final\ directory.

.DESCRIPTION
    This script automates the full installer build process:

    1. Builds the SoundSwitch binaries from source using dotnet publish (or
       skips this step when -SkipBuild is set, assuming Final\ is already
       populated — e.g. by Publish-Release.ps1).
    2. Generates HTML documentation from Markdown sources (Changelog, README, Terms).
    3. Bundles additional assets (images, licenses).
    4. Optionally signs binaries and the final installer with tools\Sign-Binary.ps1.
    5. Compiles the Inno Setup installer.

    This script is focused exclusively on building and code signing.  It does
    not interact with GitHub releases — use Publish-Release.ps1 for the full
    release workflow (download draft, build installer, upload, publish).

    Requires PowerShell 7+ (ships with Windows 11).

.PARAMETER Configuration
    Build configuration: Release (default) or Debug.

.PARAMETER SkipBuild
    When set, skips the dotnet build/publish step and assumes the Final\
    directory is already populated with binaries (e.g. extracted from a
    downloaded release artifact).

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
    .\tools\Build-Installer.ps1 -SkipBuild
    Builds the installer from an already-populated Final\ directory.

.EXAMPLE
    .\tools\Build-Installer.ps1 -SkipSigning -InstallerReleaseState Nightly
    Builds from source without signing, using "Nightly" label for installer.
#>

#Requires -Version 7.0

[CmdletBinding()]
param(
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',

    [switch]$SkipBuild,

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

if ($SkipBuild) {
    Write-Host "`n=== Using pre-populated Final\ directory ===" -ForegroundColor White
    if (-not (Test-Path $finalDir)) {
        throw "Final\ directory not found at $finalDir. Use -SkipBuild only when binaries are already extracted there (e.g. by Publish-Release.ps1)."
    }
    $fileCount = (Get-ChildItem $finalDir -File -ErrorAction SilentlyContinue).Count
    if ($fileCount -eq 0) {
        throw "Final\ directory at $finalDir is empty. Populate it with binaries before using -SkipBuild."
    }
    Write-Host "  Found $fileCount files in $finalDir"
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
