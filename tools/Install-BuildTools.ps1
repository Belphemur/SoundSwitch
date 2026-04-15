<#
.SYNOPSIS
    Installs the build tools required to compile and package SoundSwitch.

.DESCRIPTION
    Uses winget to install Inno Setup 6, Certum SimplySign Desktop (cloud
    certificate provider), and Python 3.  Also downloads signtool.exe from the
    lightweight Microsoft.Windows.SDK.BuildTools NuGet package (instead of the
    full multi-GB Windows SDK).  Inno Setup uses signtool together with the
    Certum SimplySign certificate to sign both the application executables and
    the generated installer.

    Run this script once on a fresh Windows 11 machine to prepare the
    environment for building the SoundSwitch installer.

.PARAMETER Scope
    Installation scope: 'machine' (default, requires elevation) or 'user'.

.PARAMETER SignToolDir
    Directory to install signtool.exe into. Defaults to
    $env:LOCALAPPDATA\SoundSwitch\BuildTools\signtool.

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
    [string]$Scope = 'machine',

    [string]$SignToolDir = (Join-Path $env:LOCALAPPDATA 'SoundSwitch\BuildTools\signtool')
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

function Install-SignTool {
    <#
    .SYNOPSIS
        Downloads signtool.exe from the Microsoft.Windows.SDK.BuildTools NuGet
        package (~22 MB) instead of the full Windows SDK (several GB).
    #>
    param(
        [Parameter(Mandatory)]
        [string]$InstallDir
    )

    Write-Host "`nChecking signtool.exe ..." -ForegroundColor Cyan

    # Already on PATH?
    if (Test-CommandExists 'signtool.exe') {
        Write-Host "  signtool.exe is already available on PATH." -ForegroundColor Green
        return
    }

    # Already in our managed directory?
    $signtoolExe = Join-Path $InstallDir 'signtool.exe'
    if (Test-Path $signtoolExe) {
        Write-Host "  signtool.exe found at $InstallDir" -ForegroundColor Green
        # Ensure it's on PATH for this session
        if ($env:Path -notlike "*$InstallDir*") {
            $env:Path = "$InstallDir;$env:Path"
        }
        return
    }

    # Download the NuGet package and extract signtool
    $nugetPkg = 'microsoft.windows.sdk.buildtools'
    $nugetVer = '10.0.26100.7705'
    $nupkgUrl = "https://api.nuget.org/v3-flatcontainer/$nugetPkg/$nugetVer/$nugetPkg.$nugetVer.nupkg"

    Write-Host "  Downloading signtool from NuGet ($nugetPkg $nugetVer) ..." -ForegroundColor Yellow

    $tempDir  = Join-Path ([System.IO.Path]::GetTempPath()) "sdk-buildtools-$PID"
    $tempZip  = "$tempDir.zip"

    try {
        Invoke-WebRequest -Uri $nupkgUrl -OutFile $tempZip -UseBasicParsing
        Expand-Archive -Path $tempZip -DestinationPath $tempDir -Force

        # Locate x64 signtool.exe
        $found = Get-ChildItem -Path $tempDir -Recurse -Filter 'signtool.exe' |
            Where-Object { $_.DirectoryName -match '[/\\]x64$' } |
            Select-Object -First 1

        if (-not $found) {
            throw "signtool.exe (x64) not found in the NuGet package."
        }

        # Copy just signtool.exe and its manifest
        New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
        Copy-Item $found.FullName -Destination $InstallDir -Force
        $manifest = Join-Path $found.DirectoryName 'signtool.exe.manifest'
        if (Test-Path $manifest) {
            Copy-Item $manifest -Destination $InstallDir -Force
        }

        Write-Host "  signtool.exe installed to $InstallDir" -ForegroundColor Green
    }
    finally {
        Remove-Item $tempZip  -Force   -ErrorAction SilentlyContinue
        Remove-Item $tempDir  -Recurse -Force -ErrorAction SilentlyContinue
    }

    # Add to user PATH permanently so future sessions find signtool
    $userPath = [System.Environment]::GetEnvironmentVariable('Path', 'User')
    if ($userPath -notlike "*$InstallDir*") {
        [System.Environment]::SetEnvironmentVariable('Path', "$InstallDir;$userPath", 'User')
        $env:Path = "$InstallDir;$env:Path"
        Write-Host "  Added $InstallDir to user PATH." -ForegroundColor Green
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

# 3. signtool.exe — used by Inno Setup (with the Certum certificate) to sign
#    executables and the installer.
#    Downloaded from the SDK BuildTools NuGet package (lightweight, ~22 MB).
Install-SignTool -InstallDir $SignToolDir

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
Write-Host "  - Inno Setup 6               (installer compiler)"
Write-Host "  - Certum SimplySign Desktop   (cloud certificate provider for code signing)"
Write-Host "  - signtool.exe                (used by Inno Setup for signing; from SDK BuildTools NuGet package)"
Write-Host "  - Python 3.14                (markdown-to-HTML documentation)"
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Restart your terminal to pick up PATH changes"
Write-Host "  2. Run .\tools\Build-Installer.ps1 to build the installer"
Write-Host ""
