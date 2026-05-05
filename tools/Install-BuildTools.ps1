<#
.SYNOPSIS
    Installs the build tools required to compile and package SoundSwitch.

.DESCRIPTION
    Uses winget to install GitHub CLI, Inno Setup 6, Certum SimplySign Desktop (cloud
    certificate provider), Python 3, and the .NET SDK (version auto-detected from the
    project's TargetFramework).  Locates signtool.exe from an
    existing Windows Kits installation; if not present, downloads a
    standalone signtool.exe from the Delphier/SignTool GitHub repository;
    if that also fails, installs the Windows SDK via winget as a last
    resort.  The signtool.exe is used by the signing/build scripts (such
    as Sign-Binary.ps1 and Build-Installer.ps1) together with the Certum
    SimplySign certificate to sign the application executables and the
    generated installer.

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

function Get-DotNetSdkVersion {
    <#
    .SYNOPSIS
        Reads the TargetFramework from SoundSwitch.csproj and returns the
        corresponding .NET major version number (e.g. 10 for net10.0-windows).
    .PARAMETER CsprojPath
        Optional path to SoundSwitch.csproj.  Defaults to the location
        relative to this script's directory.
    .OUTPUTS
        Integer major version number, or 0 if it cannot be determined.
    #>
    param(
        [string]$CsprojPath
    )

    if (-not $CsprojPath) {
        if (-not $PSScriptRoot) {
            Write-Warning 'PSScriptRoot is not set; cannot locate SoundSwitch.csproj.'
            return 0
        }
        $CsprojPath = Join-Path $PSScriptRoot '..\SoundSwitch\SoundSwitch.csproj'
    }

    if (-not (Test-Path $CsprojPath)) {
        Write-Warning "Could not locate SoundSwitch.csproj at '$CsprojPath'."
        return 0
    }

    try {
        [xml]$project = Get-Content $CsprojPath -ErrorAction Stop
        $framework = $project.Project.PropertyGroup.TargetFramework | Select-Object -First 1
        $match = [System.Text.RegularExpressions.Regex]::Match($framework, '^net(?<major>\d+)')
        if ($match.Success) {
            return [int]$match.Groups['major'].Value
        }
        Write-Warning "Could not parse TargetFramework '$framework' from SoundSwitch.csproj."
        return 0
    }
    catch {
        Write-Warning "Failed to read SoundSwitch.csproj: $_"
        return 0
    }
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

function Test-SignToolWorks {
    <#
    .SYNOPSIS
        Runs signtool.exe without arguments to verify it can execute.
        Returns $true if the binary starts successfully, $false otherwise
        (e.g. missing DLLs or SxS runtime errors).
    .PARAMETER Path
        Full path to signtool.exe.
    #>
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path $Path)) { return $false }

    try {
        # signtool with no args prints usage and exits with code 1 — that is fine.
        # We only care that it starts at all (no DLL / SxS crash).
        & $Path 2>&1 | Out-Null
        # Exit code 0 or 1 both mean the binary ran successfully
        return ($LASTEXITCODE -le 1)
    }
    catch {
        return $false
    }
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
        ForEach-Object {
            $versionText = $_.Directory.Parent.Name
            try {
                [PSCustomObject]@{
                    DirectoryName = $_.DirectoryName
                    Version       = [version]$versionText
                }
            }
            catch {
                $null
            }
        } |
        Sort-Object Version -Descending |
        Select-Object -First 1

    if ($found) { return $found.DirectoryName } else { return $null }
}

function Install-SignToolFromGitHub {
    <#
    .SYNOPSIS
        Downloads a standalone signtool.exe from the Delphier/SignTool GitHub
        repository and extracts it to a local directory.  Runs signtool.exe
        without arguments to verify the binary works.
    .OUTPUTS
        The directory containing the extracted signtool.exe, or $null on failure.
    #>
    $installDir = Join-Path $env:LOCALAPPDATA 'SignTool'

    # Already downloaded previously? Verify it still works.
    $existing = Join-Path $installDir 'signtool.exe'
    if (Test-Path $existing) {
        if (Test-SignToolWorks $existing) { return $installDir }
        Write-Warning "  Cached signtool.exe at $installDir is broken — re-downloading."
        Remove-Item $installDir -Recurse -Force -ErrorAction SilentlyContinue
    }

    Write-Host "  Downloading signtool.exe from GitHub (Delphier/SignTool) ..." -ForegroundColor Yellow

    try {
        # Query the GitHub API for the latest release
        $releaseUrl = 'https://api.github.com/repos/Delphier/SignTool/releases/latest'
        $release = Invoke-RestMethod -Uri $releaseUrl -Headers @{ Accept = 'application/vnd.github+json' } -ErrorAction Stop

        # Find the x64 zip asset
        $asset = $release.assets | Where-Object { $_.name -like '*x64*.zip' } | Select-Object -First 1
        if (-not $asset) {
            Write-Warning "  No x64 asset found in the latest Delphier/SignTool release."
            return $null
        }

        # Download and extract
        $zipPath = Join-Path ([System.IO.Path]::GetTempPath()) $asset.name
        Invoke-WebRequest -Uri $asset.browser_download_url -OutFile $zipPath -ErrorAction Stop

        if (Test-Path $installDir) { Remove-Item $installDir -Recurse -Force }
        New-Item -ItemType Directory -Path $installDir -Force | Out-Null
        Expand-Archive -Path $zipPath -DestinationPath $installDir -Force
        Remove-Item $zipPath -Force -ErrorAction SilentlyContinue

        # Locate signtool.exe (may be at root or nested in a subdirectory)
        $signtoolExe = Join-Path $installDir 'signtool.exe'
        $resultDir = $installDir
        if (-not (Test-Path $signtoolExe)) {
            $found = Get-ChildItem -Path $installDir -Recurse -Filter 'signtool.exe' | Select-Object -First 1
            if (-not $found) {
                Write-Warning "  signtool.exe not found after extracting the downloaded archive."
                return $null
            }
            $signtoolExe = $found.FullName
            $resultDir = $found.DirectoryName
        }

        # Smoke-test: run without args to verify the binary is functional
        if (-not (Test-SignToolWorks $signtoolExe)) {
            Write-Warning "  Downloaded signtool.exe failed smoke test — binary may have missing dependencies."
            Remove-Item $installDir -Recurse -Force -ErrorAction SilentlyContinue
            return $null
        }

        Write-Host "  signtool.exe downloaded to $resultDir" -ForegroundColor Green
        return $resultDir
    }
    catch {
        Write-Warning "  Failed to download signtool.exe from GitHub: $_"
        return $null
    }
}

function Install-SignTool {
    <#
    .SYNOPSIS
        Ensures signtool.exe is available.  Tries, in order:
          1. Existing signtool.exe on PATH
          2. Existing Windows Kits installation
          3. Standalone download from Delphier/SignTool on GitHub
          4. Windows SDK installation via winget
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
        # Try downloading a standalone signtool.exe from GitHub
        $signtoolDir = Install-SignToolFromGitHub
    }
    if (-not $signtoolDir) {
        # Last resort: install the full Windows SDK via winget
        Write-Host "  signtool.exe not found — installing Windows SDK via winget ..." -ForegroundColor Yellow
        Install-WingetPackage -Id 'Microsoft.WindowsSDK.10.0.26100' -Name 'Windows SDK 10.0.26100' -Scope $Scope

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

# 1. GitHub CLI — used by Publish-Release.ps1 to interact with GitHub releases
Install-WingetPackage -Id 'GitHub.cli' -Name 'GitHub CLI' -Scope $Scope

# 2. Inno Setup 6 — installer compiler
Install-WingetPackage -Id 'JRSoftware.InnoSetup' -Name 'Inno Setup 6' -Scope $Scope

# 3. Certum SimplySign Desktop — cloud certificate provider used for code signing
Install-WingetPackage -Id 'Certum.SmartSignSimplySignDesktop' -Name 'Certum SimplySign Desktop' -Scope $Scope

# 4. signtool.exe — used by Sign-Binary.ps1/Build-Installer.ps1 (with the
#    Certum certificate) to sign executables and the installer.
#    Located from an existing Windows Kits installation, downloaded from
#    Delphier/SignTool on GitHub, or the full Windows SDK is installed
#    via winget as a last resort.
Install-SignTool

# 5. Python 3 — used for markdown-to-HTML documentation generation
Install-WingetPackage -Id 'Python.Python.3.14' -Name 'Python 3.14' -Scope $Scope

# 6. .NET SDK — required to build and test the application.
#    Version is derived automatically from the project's TargetFramework.
$dotNetMajor = Get-DotNetSdkVersion
if ($dotNetMajor -gt 0) {
    Install-WingetPackage -Id "Microsoft.DotNet.SDK.$dotNetMajor" -Name ".NET SDK $dotNetMajor" -Scope $Scope
}
else {
    Write-Warning "Unable to detect .NET SDK version from SoundSwitch.csproj; skipping .NET SDK installation."
}

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
Write-Host "  - GitHub CLI                  (used by Publish-Release.ps1 for release management)"
Write-Host "  - Inno Setup 6               (installer compiler)"
Write-Host "  - Certum SimplySign Desktop   (cloud certificate provider for code signing)"
Write-Host "  - signtool.exe                (used by Sign-Binary.ps1 for signing)"
Write-Host "  - Python 3.14                (markdown-to-HTML documentation)"
if ($dotNetMajor -gt 0) {
    Write-Host "  - .NET SDK $dotNetMajor               (build and test the application)"
}
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Restart your terminal to pick up PATH changes"
Write-Host "  2. Run .\tools\Build-Installer.ps1 to build the installer"
Write-Host ""
