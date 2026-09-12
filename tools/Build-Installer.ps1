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

.PARAMETER Architectures
    Architectures to compile installers for: 'win-x64' and/or 'win-arm64'.
    Accepts a comma-separated string (e.g. 'win-x64,win-arm64') or an array.
    Defaults to BOTH architectures.  One ISCC pass is run per architecture
    with /DTargetArch=<arch>.

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

    [string]$InstallerReleaseState = 'Release',

    [string[]]$Architectures = @('win-x64', 'win-arm64')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Paths ────────────────────────────────────────────────────────────────────

$repoRoot    = Split-Path $PSScriptRoot -Parent
$FinalDir    = [System.IO.Path]::GetFullPath($FinalDir)
$signScript  = Join-Path $PSScriptRoot 'Sign-Binary.ps1'
$projectName = 'SoundSwitch'
$cliProject  = 'SoundSwitch.CLI'

# Normalize -Architectures: accept a comma-separated string (e.g.
# 'win-x64,win-arm64', typical for CI inputs) as well as an explicit array.
$Architectures = @(
    $Architectures |
        ForEach-Object { $_ -split ',' } |
        ForEach-Object { $_.Trim() } |
        Where-Object { $_ }
)
if ($Architectures.Count -eq 0) {
    $Architectures = @('win-x64', 'win-arm64')
}
$invalidArchitectures = @($Architectures | Where-Object { $_ -notin @('win-x64', 'win-arm64') })
if ($invalidArchitectures.Count -gt 0) {
    throw "Invalid architecture(s): $($invalidArchitectures -join ', '). Supported values: win-x64, win-arm64."
}

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

    $binaries = Get-ChildItem $FinalDir -Recurse -File -Include '*.exe', '*.dll' |
        Where-Object {
            $_.FullName -notlike (Join-Path $FinalDir 'Installer\*') -and
            (
                $_.Name -like '*SoundSwitch*.exe' -or
                $_.Name -like '*SoundSwitch*.dll'
            )
        } |
        ForEach-Object { $_.FullName }

    if ($binaries) {
        & $signScript -Path $binaries -CertificateName $CertificateName
    }
    else {
        Write-Host "  No binaries found to sign." -ForegroundColor DarkGray
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
Write-Host "  Architectures: $($Architectures -join ', ')" -ForegroundColor DarkGray

$installerDir = Join-Path $FinalDir 'Installer'
if (-not (Test-Path $installerDir)) {
    New-Item -ItemType Directory -Path $installerDir -Force | Out-Null
}

# Clean previous installer files
Get-ChildItem $installerDir -Filter '*Installer*.exe' -ErrorAction SilentlyContinue |
    Remove-Item -Force

$setupIss = Join-Path $repoRoot 'Installer\setup.iss'
if (-not (Test-Path $setupIss)) {
    throw "Installer\setup.iss not found at $setupIss."
}

# One ISCC pass per architecture: setup.iss uses /DTargetArch to select which
# payload (Final\<rid>) is bundled and which installer filename is produced.
# x64 keeps the unsuffixed installer name (legacy tooling compatibility);
# arm64 produces *_arm64.exe.
foreach ($arch in $Architectures) {
    $targetArch = switch ($arch) {
        'win-x64'   { 'x64' }
        'win-arm64' { 'arm64' }
        default     { throw "Unsupported architecture: $arch" }
    }

    Write-Host "  Compiling $targetArch installer: ISCC $setupIss /DReleaseState=$InstallerReleaseState /DTargetArch=$targetArch"
    & $isccExe $setupIss "/DReleaseState=$InstallerReleaseState" "/DTargetArch=$targetArch"
    if ($LASTEXITCODE -ne 0) {
        throw "Inno Setup compilation failed for $targetArch with exit code $LASTEXITCODE."
    }
}

# Move installer output from Final\ to Final\Installer\
$builtInstallers = Get-ChildItem $FinalDir -Filter '*Installer*.exe' -File
foreach ($ins in $builtInstallers) {
    $dest = Join-Path $installerDir $ins.Name
    Move-Item $ins.FullName $dest -Force
    Write-Host "  Moved $($ins.Name) -> Installer\"
}

# ── Step 3: Sign installer ───────────────────────────────────────────────────

Write-Host "`n=================================" -ForegroundColor White
Write-Host "Installer built successfully!" -ForegroundColor Green

$installers = Get-ChildItem $installerDir -Filter '*Installer*.exe'
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
