<#
.SYNOPSIS
    Downloads and extracts the latest SoundSwitch release or beta build artifact.

.DESCRIPTION
    Uses the GitHub API to find the latest release (or pre-release / beta)
    artifact zip, downloads it, and extracts it into the Final\ directory so
    that the local Build-Installer.ps1 can sign binaries and build the installer.

    Requires PowerShell 7+ (ships with Windows 11).

.PARAMETER Channel
    Release channel to download: 'release' (default) or 'beta'.

.PARAMETER Token
    Optional GitHub personal access token for higher API rate limits and
    access to private repositories.  Can also be set via the GITHUB_TOKEN
    environment variable.

.PARAMETER OutputDir
    Directory to extract the artifact into (default: .\Final).

.PARAMETER Repository
    GitHub repository in owner/repo format (default: Belphemur/SoundSwitch).

.EXAMPLE
    .\tools\Download-Release.ps1
    Downloads the latest stable release artifact into .\Final

.EXAMPLE
    .\tools\Download-Release.ps1 -Channel beta
    Downloads the latest beta / pre-release artifact into .\Final

.EXAMPLE
    .\tools\Download-Release.ps1 -Channel release -Token ghp_xxxx -OutputDir C:\Build\Final
#>

[CmdletBinding()]
param(
    [ValidateSet('release', 'beta')]
    [string]$Channel = 'release',

    [string]$Token = $env:GITHUB_TOKEN,

    [string]$OutputDir = (Join-Path $PSScriptRoot '..\Final'),

    [string]$Repository = 'Belphemur/SoundSwitch'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Helpers ──────────────────────────────────────────────────────────────────

function Get-GitHubHeaders {
    $headers = @{
        'Accept'     = 'application/vnd.github+json'
        'User-Agent' = 'SoundSwitch-Download/1.0'
    }
    if ($Token) {
        $headers['Authorization'] = "Bearer $Token"
    }
    return $headers
}

function Find-ReleaseAsset {
    param(
        [Parameter(Mandatory)]
        [string]$ApiUrl,
        [Parameter(Mandatory)]
        [hashtable]$Headers,
        [Parameter(Mandatory)]
        [bool]$IncludePreRelease
    )

    Write-Host "Fetching releases from $ApiUrl ..."
    $releases = Invoke-RestMethod -Uri $ApiUrl -Headers $Headers -Method Get

    if (-not $releases -or $releases.Count -eq 0) {
        throw "No releases found for $Repository."
    }

    $selected = $null
    foreach ($rel in $releases) {
        if ($IncludePreRelease -or -not $rel.prerelease) {
            $selected = $rel
            break
        }
    }

    if (-not $selected) {
        throw "No matching $Channel release found for $Repository."
    }

    Write-Host "Selected release: $($selected.tag_name) (pre-release: $($selected.prerelease))"

    # Find the zip asset (pattern: SoundSwitch-v*.zip)
    $asset = $selected.assets | Where-Object {
        $_.name -match '^SoundSwitch-v.+\.zip$'
    } | Select-Object -First 1

    if (-not $asset) {
        throw "No SoundSwitch zip asset found in release $($selected.tag_name). Available assets: $(($selected.assets | ForEach-Object { $_.name }) -join ', ')"
    }

    return @{
        Name       = $asset.name
        Url        = $asset.browser_download_url
        Version    = $selected.tag_name
        PreRelease = $selected.prerelease
    }
}

# ── Main ─────────────────────────────────────────────────────────────────────

$apiBase = "https://api.github.com/repos/$Repository/releases"
$headers = Get-GitHubHeaders

# For beta channel, we include pre-releases; the API returns releases sorted
# by creation date descending, so we pick the first matching one.
$includePreRelease = $Channel -eq 'beta'

$assetInfo = Find-ReleaseAsset -ApiUrl "${apiBase}?per_page=30" -Headers $headers -IncludePreRelease $includePreRelease

Write-Host ""
Write-Host "Downloading $($assetInfo.Name) from $($assetInfo.Version) ..."

$tempZip = Join-Path ([System.IO.Path]::GetTempPath()) $assetInfo.Name

$downloadHeaders = @{
    'User-Agent' = 'SoundSwitch-Download/1.0'
}
if ($Token) {
    $downloadHeaders['Authorization'] = "Bearer $Token"
}

Invoke-WebRequest -Uri $assetInfo.Url -OutFile $tempZip -Headers $downloadHeaders

if (-not (Test-Path $tempZip)) {
    throw "Download failed: $tempZip was not created."
}

$zipSize = (Get-Item $tempZip).Length
Write-Host "Downloaded $([math]::Round($zipSize / 1MB, 2)) MB -> $tempZip"

# Prepare output directory
$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (Test-Path $OutputDir) {
    Write-Host "Cleaning existing output directory: $OutputDir"
    Remove-Item $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# Extract
Write-Host "Extracting to $OutputDir ..."
Expand-Archive -Path $tempZip -DestinationPath $OutputDir -Force

# Clean up temp file
Remove-Item $tempZip -Force -ErrorAction SilentlyContinue

$fileCount = (Get-ChildItem $OutputDir -Recurse -File).Count
Write-Host ""
Write-Host "Done! Extracted $fileCount files to $OutputDir"
Write-Host "Release: $($assetInfo.Version) ($Channel)"
Write-Host ""
Write-Host "You can now run tools\Build-Installer.ps1 to sign binaries and build the installer."
