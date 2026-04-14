<#
.SYNOPSIS
    Installs the build tools required to compile and package SoundSwitch.

.DESCRIPTION
    Uses winget to install Inno Setup 6 and the Windows SDK (for signtool).
    Also ensures Python 3 with the 'markdown' package is available for HTML
    documentation generation.

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

# ── Pre-flight checks ───────────────────────────────────────────────────────

Write-Host "SoundSwitch Build Tools Installer" -ForegroundColor White
Write-Host "=================================`n"

if (-not (Test-CommandExists 'winget')) {
    throw "winget is not available. Please install App Installer from the Microsoft Store or update Windows."
}

# ── Install packages ─────────────────────────────────────────────────────────

# 1. Inno Setup 6 — installer compiler
Install-WingetPackage -Id 'JRSoftware.InnoSetup' -Name 'Inno Setup 6' -Scope $Scope

# 2. Windows SDK — provides signtool.exe for code signing
#    The Windows 11 SDK includes signtool in the bin directory.
Install-WingetPackage -Id 'Microsoft.WindowsSDK.10.0.26100' -Name 'Windows 11 SDK (signtool)' -Scope $Scope

# 3. Python 3 — used for markdown-to-HTML documentation generation
Install-WingetPackage -Id 'Python.Python.3.12' -Name 'Python 3.12' -Scope $Scope

# ── Post-install: Python markdown package ────────────────────────────────────

Write-Host "`nInstalling Python 'markdown' package ..." -ForegroundColor Cyan

# Refresh PATH so that newly installed tools are visible
$machinePath = [System.Environment]::GetEnvironmentVariable('Path', 'Machine')
$userPath = [System.Environment]::GetEnvironmentVariable('Path', 'User')
$env:Path = (($machinePath, $userPath) | Where-Object { $_ }) -join ';'

if (Test-CommandExists 'python') {
    python -m pip install --upgrade pip --quiet
    python -m pip install markdown --quiet
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to install Python 'markdown' package."
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
Write-Host "  - Inno Setup 6       (installer compiler)"
Write-Host "  - Windows 11 SDK     (signtool for code signing)"
Write-Host "  - Python 3.12        (markdown-to-HTML documentation)"
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Restart your terminal to pick up PATH changes"
Write-Host "  2. Run .\tools\Build-Installer.ps1 to build the installer"
Write-Host ""
