<#
.SYNOPSIS
    Installs the build tools required to compile and package SoundSwitch.

.DESCRIPTION
    Uses winget to install Inno Setup 6, Certum SimplySign Desktop (cloud
    certificate provider), and Python 3.  Locates signtool.exe from an
    existing Windows Kits installation; if not present, installs the Windows
    Driver Kit (WDK) via winget.  The signtool.exe is used by the
    signing/build scripts (such as Sign-Binary.ps1 and Build-Installer.ps1)
    together with the Certum SimplySign certificate to sign the application
    executables and the generated installer.

    Requires PowerShell 7+ (ships with Windows 11).

    Run this script once on a fresh Windows 11 machine to prepare the
    environment for building the SoundSwitch installer.

.PARAMETER Scope
    Installation scope: 'machine' (default, requires elevation) or 'user'.

.EXAMPLE
    .\tools\Install-BuildTools.ps1
    Installs all required tools with machine scope.

.EXAMPLE
    .\tools\Install-BuildTools.ps1 -Scope user
    Installs tools in user scope (no elevation required for some packages).
#>

#Requires -Version 7.0

[CmdletBinding()]
param(
    [ValidateSet('machine', 'user')]
    [string]$Scope = 'machine'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Helpers ──────────────────────────────────────────────────────────────────

function Test-CommandExists {
    param([string]$Command)
    $null -ne (Get-Command $Command -ErrorAction SilentlyContinue)
}

function Install-WingetPackage {
    param(
        [Parameter(Mandatory)]
        [string]$Id,
        [string]$Name,
        [string]$Scope
    )

    $displayName = if ($Name) { $Name } else { $Id }

    Write-Host "`nChecking $displayName ..." -ForegroundColor Cyan

    # Check if already installed via winget
    $installed = winget list --id $Id --exact --accept-source-agreements 2>&1
    if ($LASTEXITCODE -eq 0 -and ($installed | Select-String $Id)) {
        Write-Host "  $displayName is already installed." -ForegroundColor Green
        return
    }

    Write-Host "  Installing $displayName ..." -ForegroundColor Yellow
    $arguments = @(
        'install',
        '--id', $Id,
        '--exact',
        '--source', 'winget',
        '--accept-package-agreements',
        '--accept-source-agreements'
    )
    if ($Scope) {
        $arguments += @('--scope', $Scope)
    }

    winget @arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install $displayName (winget exit code: $LASTEXITCODE)."
    }

    Write-Host "  $displayName installed successfully." -ForegroundColor Green
}

function Find-SignToolInWindowsKits {
    <#
    .SYNOPSIS
        Searches for the x64 signtool.exe inside the Windows Kits directory
        that is created by a Windows SDK installation.
    .OUTPUTS
        The directory containing signtool.exe, or $null if not found.
    #>
    $windowsKitsBase = 'C:\Program Files (x86)\Windows Kits\10\bin'
    if (-not (Test-Path $windowsKitsBase)) { return $null }

    $found = Get-ChildItem -Path $windowsKitsBase -Recurse -Filter 'signtool.exe' |
        Where-Object { $_.DirectoryName -match '[/\\]x64$' } |
        Sort-Object { $_.DirectoryName } -Descending |
        Select-Object -First 1

    if ($found) { return $found.DirectoryName } else { return $null }
}

function Install-SignTool {
    <#
    .SYNOPSIS
        Ensures signtool.exe is available from the Windows Kits.
        Searches existing installations first; installs the Windows Driver Kit
        (WDK) via winget if signtool is not found.
    #>

    Write-Host "`nChecking signtool.exe ..." -ForegroundColor Cyan

    # Already on PATH?
    if (Test-CommandExists 'signtool.exe') {
        Write-Host "  signtool.exe is already available on PATH." -ForegroundColor Green
        return
    }

    # Search in Windows Kits (installed by a previous Windows SDK installation)
    $signtoolDir = Find-SignToolInWindowsKits
    if (-not $signtoolDir) {
        # Install Windows Driver Kit (WDK) via winget to obtain a fully functional signtool.exe
        Write-Host "  signtool.exe not found — installing Windows Driver Kit via winget ..." -ForegroundColor Yellow
        Install-WingetPackage -Id 'Microsoft.WindowsWDK.10.0.26100' -Name 'Windows Driver Kit 10.0.26100' -Scope $Scope

        $signtoolDir = Find-SignToolInWindowsKits
        if (-not $signtoolDir) {
            throw "signtool.exe not found after Windows SDK installation."
        }
    }

    Write-Host "  signtool.exe found at $signtoolDir" -ForegroundColor Green

    # Add to PATH for this session
    if ($env:Path -notlike "*$signtoolDir*") {
        $env:Path = "$signtoolDir;$env:Path"
    }

    # Add to user PATH permanently so future sessions find signtool
    $userPath = [System.Environment]::GetEnvironmentVariable('Path', 'User')
    if ($userPath -notlike "*$signtoolDir*") {
        [System.Environment]::SetEnvironmentVariable('Path', "$signtoolDir;$userPath", 'User')
        Write-Host "  Added $signtoolDir to user PATH." -ForegroundColor Green
    }
}

# ── Pre-flight checks ───────────────────────────────────────────────────────

Write-Host "SoundSwitch Build Tools Installer" -ForegroundColor White
Write-Host "=================================`n"

if (-not (Test-CommandExists 'winget')) {
    throw "winget is not available. Please install App Installer from the Microsoft Store or update Windows."
}

# ── Install packages ─────────────────────────────────────────────────────────

# 1. Inno Setup 6 — installer compiler
Install-WingetPackage -Id 'JRSoftware.InnoSetup' -Name 'Inno Setup 6' -Scope $Scope

# 2. Certum SimplySign Desktop — cloud certificate provider used for code signing
Install-WingetPackage -Id 'Certum.SmartSignSimplySignDesktop' -Name 'Certum SimplySign Desktop' -Scope $Scope

# 3. signtool.exe — used by Sign-Binary.ps1/Build-Installer.ps1 (with the
#    Certum certificate) to sign executables and the installer.
#    Located from an existing Windows Kits installation, or the Windows
#    Driver Kit (WDK) is installed via winget if signtool is not found.
Install-SignTool

# 4. Python 3 — used for markdown-to-HTML documentation generation
Install-WingetPackage -Id 'Python.Python.3.14' -Name 'Python 3.14' -Scope $Scope

# ── Post-install: Python markdown package ────────────────────────────────────

Write-Host "`nInstalling Python 'markdown' package ..." -ForegroundColor Cyan

# Refresh PATH so that newly installed tools are visible
$machinePath = [System.Environment]::GetEnvironmentVariable('Path', 'Machine')
$userPath = [System.Environment]::GetEnvironmentVariable('Path', 'User')
$env:Path = (($machinePath, $userPath) | Where-Object { $_ }) -join ';'

if (Test-CommandExists 'python') {
    python -m pip install --upgrade pip --quiet
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to upgrade pip (exit code $LASTEXITCODE)."
    }
    python -m pip install markdown --quiet
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install Python 'markdown' package (exit code $LASTEXITCODE)."
    }
    Write-Host "  Python 'markdown' package installed." -ForegroundColor Green
}
else {
    Write-Warning "Python was just installed but is not on PATH yet. Please restart your terminal and run: python -m pip install markdown"
}

# ── Summary ──────────────────────────────────────────────────────────────────

Write-Host "`n=================================" -ForegroundColor White
Write-Host "All build tools installed successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Installed tools:"
Write-Host "  - Inno Setup 6               (installer compiler)"
Write-Host "  - Certum SimplySign Desktop   (cloud certificate provider for code signing)"
Write-Host "  - signtool.exe                (used by Sign-Binary.ps1 for signing; from Windows Driver Kit installation)"
Write-Host "  - Python 3.14                (markdown-to-HTML documentation)"
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Restart your terminal to pick up PATH changes"
Write-Host "  2. Run .\tools\Build-Installer.ps1 to build the installer"
Write-Host ""
