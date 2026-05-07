<#
.SYNOPSIS
    Builds and signs the SoundSwitch installer from a pre-populated directory.

.DESCRIPTION
    This script is focused exclusively on installer compilation and code signing:

    1. Validates that the target directory contains the expected binaries.
    2. Signs the application binaries using tools\Sign-Binary.ps1.
    3. Locates Inno Setup 6 (ISCC.exe) and compiles the installer directly.
    4. Signs the resulting installer using tools\Sign-Binary.ps1.

    It does NOT build from source, generate HTML documentation, or bundle
    assets — those responsibilities belong to Publish-Release.ps1.

    The target directory must be the canonical Final\ directory at the
    repository root because Inno Setup (setup.iss) references it via a
    hardcoded relative path.

    Requires PowerShell 7+ (ships with Windows 11).

.PARAMETER FinalDir
    Path to the directory containing the binaries, documentation, and assets
    to package.  Defaults to .\Final (relative to the repository root).
    Must be the canonical Final\ directory because Inno Setup references it
    via a hardcoded relative path in Installer\scripts\app_defines.iss.

.PARAMETER SkipSigning
    Skip code signing even when signtool is available.

.PARAMETER CertificateName
    Subject name (CN) of the code-signing certificate passed to Sign-Binary.ps1.
    Defaults to "OpenSource Developer, Antoine Aflalo".

.PARAMETER InstallerReleaseState
    The release state label passed to Inno Setup (e.g. Release, Beta, Nightly).

.EXAMPLE
    .\tools\Build-Installer.ps1
    Builds and signs the installer from the default Final\ directory.

.EXAMPLE
    .\tools\Build-Installer.ps1 -SkipSigning
    Builds the installer without code signing.

.EXAMPLE
    .\tools\Build-Installer.ps1 -InstallerReleaseState Beta
    Builds the installer with "Beta" label.
#>

#Requires -Version 7.0

[CmdletBinding()]
param(
    [string]$FinalDir = (Join-Path (Split-Path $PSScriptRoot -Parent) 'Final'),

    [switch]$SkipSigning,

    [string]$CertificateName = 'OpenSource Developer, Antoine Aflalo',

    [string]$InstallerReleaseState = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Paths ────────────────────────────────────────────────────────────────────

$repoRoot    = Split-Path $PSScriptRoot -Parent
$FinalDir    = [System.IO.Path]::GetFullPath($FinalDir)
$signScript  = Join-Path $PSScriptRoot 'Sign-Binary.ps1'
$projectName = 'SoundSwitch'
$cliProject  = 'SoundSwitch.CLI'

# ── Locate Inno Setup (ISCC.exe) ────────────────────────────────────────────

function Find-InnoSetup {
    <#
    .SYNOPSIS
        Locates ISCC.exe from the Inno Setup 6 installation.
    .DESCRIPTION
        Searches the Windows registry for the Inno Setup 6 install location,
        matching the logic used by the CI workflow test-installer-build.yml.
    .OUTPUTS
        The full path to ISCC.exe, or $null if not found.
    #>
    $registryPaths = @(
        'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1',
        'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Inno Setup 6_is1'
    )

    foreach ($regPath in $registryPaths) {
        if (Test-Path $regPath) {
            $installDir = (Get-ItemProperty -Path $regPath -ErrorAction SilentlyContinue).InstallLocation
            if ($installDir) {
                $isccPath = Join-Path $installDir 'ISCC.exe'
                if (Test-Path $isccPath) {
                    return $isccPath
                }
            }
        }
    }

    # Fallback: check PATH
    $onPath = Get-Command 'ISCC.exe' -ErrorAction SilentlyContinue
    if ($onPath) {
        return $onPath.Source
    }

    return $null
}

# ── Validate Final\ ─────────────────────────────────────────────────────────

Write-Host "SoundSwitch Installer Builder" -ForegroundColor White
Write-Host "============================`n"

if (-not (Test-Path $FinalDir)) {
    throw "Directory not found at $FinalDir. Populate it with binaries first (e.g. via Publish-Release.ps1)."
}

$fileCount = (Get-ChildItem $FinalDir -Recurse -File -ErrorAction SilentlyContinue).Count
if ($fileCount -eq 0) {
    throw "Directory at $FinalDir is empty. Populate it with binaries first."
}
Write-Host "Using $FinalDir ($fileCount files)" -ForegroundColor Cyan

# ── Step 1: Sign binaries ────────────────────────────────────────────────────

$canSign = -not $SkipSigning -and (Get-Command 'signtool.exe' -ErrorAction SilentlyContinue)

if ($canSign) {
    Write-Host "`n=== Signing binaries ===" -ForegroundColor White

    $dirs = @('win-x64', 'win-arm64') | ForEach-Object { Join-Path $FinalDir $_ } | Where-Object { Test-Path $_ }

    if (-not $dirs) {
        # Fallback: old flat layout
        $dirs = @($FinalDir)
    }

    $binaries = foreach ($dir in $dirs) {
        @("$projectName.exe", "$cliProject.exe") |
            ForEach-Object { Join-Path $dir $_ } |
            Where-Object { Test-Path $_ }
    }

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

# ── Step 2: Build installer (Inno Setup) ─────────────────────────────────────

Write-Host "`n=== Building installer ===" -ForegroundColor White

$isccExe = Find-InnoSetup
if (-not $isccExe) {
    throw "Inno Setup 6 (ISCC.exe) not found. Run tools\Install-BuildTools.ps1 first."
}
Write-Host "  Using ISCC: $isccExe" -ForegroundColor DarkGray

$installerDir = Join-Path $FinalDir 'Installer'
if (-not (Test-Path $installerDir)) {
    New-Item -ItemType Directory -Path $installerDir -Force | Out-Null
}

# Clean previous installer files
Get-ChildItem $installerDir -Filter '*Installer.exe' -ErrorAction SilentlyContinue |
    Remove-Item -Force

$setupIss = Join-Path $repoRoot 'Installer\setup.iss'
if (-not (Test-Path $setupIss)) {
    throw "Installer\setup.iss not found at $setupIss."
}

Write-Host "  Compiling: ISCC $setupIss /DReleaseState=$InstallerReleaseState"
& $isccExe $setupIss "/DReleaseState=$InstallerReleaseState"
if ($LASTEXITCODE -ne 0) {
    throw "Inno Setup compilation failed with exit code $LASTEXITCODE."
}

# Move installer output from Final\ to Final\Installer\
$builtInstallers = Get-ChildItem $FinalDir -Filter '*Installer.exe' -File
foreach ($ins in $builtInstallers) {
    $dest = Join-Path $installerDir $ins.Name
    Move-Item $ins.FullName $dest -Force
    Write-Host "  Moved $($ins.Name) -> Installer\"
}

# ── Step 3: Sign installer ───────────────────────────────────────────────────

Write-Host "`n=================================" -ForegroundColor White
Write-Host "Installer built successfully!" -ForegroundColor Green

$installers = Get-ChildItem $installerDir -Filter '*Installer.exe'
if ($installers) {
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

Write-Host ""
