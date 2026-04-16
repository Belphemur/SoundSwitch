<#
.SYNOPSIS
    Full release workflow: download a draft GitHub release, build and sign the
    installer, upload the signed installer, and publish the release.

.DESCRIPTION
    Orchestrates the complete SoundSwitch release process:

    1. Uses the GitHub CLI (gh) to find the latest draft release created by
       semantic-release for the chosen channel.
    2. Downloads and extracts the build-artifact zip from that draft release
       into the Final\ directory.
    3. Delegates to Build-Installer.ps1 -SkipBuild to generate HTML docs,
       bundle assets, sign binaries, and compile the Inno Setup installer.
    4. Signs the installer (handled by Build-Installer.ps1).
    5. Uploads the signed installer(s) to the draft release, replacing any
       existing installer assets.
    6. Extracts the latest section from CHANGELOG.md as the release body.
    7. Asks the user whether they want to prepend additional notes.
    8. Updates the release body on GitHub.
    9. Asks for confirmation before publishing the release.

    Requires PowerShell 7+ and an authenticated GitHub CLI (`gh auth login`).

.PARAMETER Channel
    Release channel: 'release' (default) or 'beta'.
    'release' looks for a non-pre-release draft; 'beta' looks for a
    pre-release draft.

.PARAMETER Repository
    GitHub repository in owner/repo format (default: Belphemur/SoundSwitch).

.PARAMETER OutputDir
    Directory to extract the artifact into (default: .\Final).

.PARAMETER SkipSigning
    Skip code signing even when signtool is available.

.PARAMETER CertificateName
    Subject name (CN) of the code-signing certificate passed to
    Build-Installer.ps1 / Sign-Binary.ps1.
    Defaults to "OpenSource Developer, Antoine Aflalo".

.PARAMETER InstallerReleaseState
    The release state label passed to Inno Setup (e.g. Release, Beta).

.EXAMPLE
    .\tools\Publish-Release.ps1
    Full release workflow for the latest stable draft release.

.EXAMPLE
    .\tools\Publish-Release.ps1 -Channel beta
    Full release workflow for the latest beta draft release.

.EXAMPLE
    .\tools\Publish-Release.ps1 -SkipSigning
    Full release workflow without code signing.
#>

#Requires -Version 7.0

[CmdletBinding()]
param(
    [ValidateSet('release', 'beta')]
    [string]$Channel = 'release',

    [string]$Repository = 'Belphemur/SoundSwitch',

    [string]$OutputDir = (Join-Path $PSScriptRoot '..\Final'),

    [switch]$SkipSigning,

    [string]$CertificateName = 'OpenSource Developer, Antoine Aflalo',

    [string]$InstallerReleaseState = 'Release'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Helpers ──────────────────────────────────────────────────────────────────

function Assert-GitHubCli {
    if (-not (Get-Command 'gh' -ErrorAction SilentlyContinue)) {
        throw "GitHub CLI (gh) is not installed or not on PATH. Run tools\Install-BuildTools.ps1 first."
    }

    # Verify authentication
    gh auth status 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "GitHub CLI is not authenticated. Run 'gh auth login' first."
    }
}

function Find-DraftRelease {
    <#
    .SYNOPSIS
        Finds the latest draft release for the given channel using gh CLI.
    .OUTPUTS
        A PSCustomObject with tagName, name, isDraft, isPrerelease properties,
        or $null if no matching draft is found.
    #>
    param(
        [Parameter(Mandatory)][string]$Repository,
        [Parameter(Mandatory)][bool]$IncludePreRelease
    )

    Write-Host "Fetching releases from $Repository ..." -ForegroundColor Cyan

    $json = gh release list --repo $Repository --json tagName,isDraft,isPrerelease,name --limit 30 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to list releases: $json"
    }

    $releases = $json | ConvertFrom-Json

    if (-not $releases -or $releases.Count -eq 0) {
        throw "No releases found for $Repository."
    }

    # Filter for draft releases matching the channel
    $drafts = $releases | Where-Object { $_.isDraft }

    if ($IncludePreRelease) {
        $selected = $drafts | Where-Object { $_.isPrerelease } | Select-Object -First 1
    }
    else {
        $selected = $drafts | Where-Object { -not $_.isPrerelease } | Select-Object -First 1
    }

    return $selected
}

function Get-LatestChangelogEntry {
    <#
    .SYNOPSIS
        Extracts the most recent version section from CHANGELOG.md.
    .DESCRIPTION
        Reads the changelog and returns all content from the first '## ['
        heading up to (but not including) the next '## [' heading.
    .OUTPUTS
        The latest changelog section as a string, or $null if not found.
    #>
    param([Parameter(Mandatory)][string]$Path)

    if (-not (Test-Path $Path)) {
        Write-Warning "Changelog not found at $Path"
        return $null
    }

    $lines = Get-Content $Path
    $inSection = $false
    $sectionLines = @()

    foreach ($line in $lines) {
        if ($line -match '^## \[') {
            if ($inSection) { break }
            $inSection = $true
        }
        if ($inSection) {
            $sectionLines += $line
        }
    }

    if ($sectionLines.Count -eq 0) { return $null }
    return ($sectionLines -join "`n").Trim()
}

# ── Pre-flight ───────────────────────────────────────────────────────────────

Write-Host "SoundSwitch Release Publisher" -ForegroundColor White
Write-Host "============================`n"

Assert-GitHubCli

# ── Step 1: Find draft release ───────────────────────────────────────────────

$includePreRelease = $Channel -eq 'beta'
$draft = Find-DraftRelease -Repository $Repository -IncludePreRelease $includePreRelease

if (-not $draft) {
    throw "No matching draft $Channel release found for $Repository. Has semantic-release run?"
}

$tag = $draft.tagName
Write-Host "Found draft release: $($draft.name) ($tag)" -ForegroundColor Green
Write-Host ""

# ── Step 2: Download release artifact ────────────────────────────────────────

Write-Host "=== Downloading release artifact ===" -ForegroundColor White

$OutputDir = [System.IO.Path]::GetFullPath($OutputDir)
if (Test-Path $OutputDir) {
    Write-Host "  Cleaning existing output directory: $OutputDir"
    Remove-Item $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

# Download the zip asset to a temp directory
$tempDir = Join-Path ([System.IO.Path]::GetTempPath()) "soundswitch-release-$([guid]::NewGuid().ToString('N').Substring(0,8))"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

try {
    Write-Host "  Downloading SoundSwitch zip from $tag ..."
    gh release download $tag --repo $Repository --pattern 'SoundSwitch-v*.zip' --dir $tempDir
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to download release asset from $tag."
    }

    $zipFile = Get-ChildItem $tempDir -Filter 'SoundSwitch-v*.zip' | Select-Object -First 1
    if (-not $zipFile) {
        throw "No SoundSwitch zip asset found in release $tag."
    }

    $zipSize = $zipFile.Length
    Write-Host "  Downloaded $([math]::Round($zipSize / 1MB, 2)) MB -> $($zipFile.Name)"

    # Extract
    Write-Host "  Extracting to $OutputDir ..."
    Expand-Archive -Path $zipFile.FullName -DestinationPath $OutputDir -Force
}
finally {
    Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
}

$fileCount = (Get-ChildItem $OutputDir -Recurse -File).Count
Write-Host "  Extracted $fileCount files to $OutputDir" -ForegroundColor Green
Write-Host ""

# ── Step 3: Build installer ─────────────────────────────────────────────────

Write-Host "=== Building installer ===" -ForegroundColor White

$buildScript = Join-Path $PSScriptRoot 'Build-Installer.ps1'
$buildArgs = @(
    '-SkipBuild',
    '-InstallerReleaseState', $InstallerReleaseState,
    '-CertificateName', $CertificateName
)
if ($SkipSigning) {
    $buildArgs += '-SkipSigning'
}

& $buildScript @buildArgs
if ($LASTEXITCODE -ne 0) {
    throw "Build-Installer.ps1 failed with exit code $LASTEXITCODE."
}

Write-Host ""

# ── Step 4: Upload installer to draft release ────────────────────────────────

Write-Host "=== Uploading installer to release $tag ===" -ForegroundColor White

$installerDir = Join-Path $OutputDir 'Installer'
if (-not (Test-Path $installerDir)) {
    throw "Installer directory not found at $installerDir. Did the build succeed?"
}

$installers = Get-ChildItem $installerDir -Filter '*Installer.exe'
if (-not $installers -or $installers.Count -eq 0) {
    throw "No installer files found in $installerDir."
}

foreach ($ins in $installers) {
    Write-Host "  Uploading $($ins.Name) (clobber existing) ..."
    gh release upload $tag $ins.FullName --repo $Repository --clobber
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to upload $($ins.Name) to release $tag."
    }
    Write-Host "  Uploaded $($ins.Name)" -ForegroundColor Green
}

Write-Host ""

# ── Step 5: Set release body from changelog ──────────────────────────────────

Write-Host "=== Preparing release body ===" -ForegroundColor White

$repoRoot = Split-Path $PSScriptRoot -Parent
$changelogPath = Join-Path $repoRoot 'CHANGELOG.md'
$changelogEntry = Get-LatestChangelogEntry -Path $changelogPath

if ($changelogEntry) {
    Write-Host "`nChangelog entry that will be used as the release body:" -ForegroundColor Cyan
    Write-Host "─────────────────────────────────────────────────────"
    Write-Host $changelogEntry
    Write-Host "─────────────────────────────────────────────────────`n"
}
else {
    Write-Warning "Could not extract a changelog entry from $changelogPath."
    $changelogEntry = ''
}

# Ask user for additional notes
$userNotes = Read-Host "Add notes before the changelog (press Enter to skip)"

if (-not [string]::IsNullOrWhiteSpace($userNotes)) {
    $releaseBody = "$userNotes`n`n$changelogEntry"
}
else {
    $releaseBody = $changelogEntry
}

# Update release body
if (-not [string]::IsNullOrWhiteSpace($releaseBody)) {
    $bodyFile = Join-Path ([System.IO.Path]::GetTempPath()) "release-body-$([guid]::NewGuid().ToString('N').Substring(0,8)).md"
    try {
        Set-Content -Path $bodyFile -Value $releaseBody -Encoding UTF8
        gh release edit $tag --repo $Repository --notes-file $bodyFile
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to update release body for $tag."
        }
        Write-Host "  Release body updated." -ForegroundColor Green
    }
    finally {
        Remove-Item $bodyFile -Force -ErrorAction SilentlyContinue
    }
}

Write-Host ""

# ── Step 6: Publish release ──────────────────────────────────────────────────

Write-Host "=== Publish release ===" -ForegroundColor White
Write-Host ""
Write-Host "Release:    $($draft.name)" -ForegroundColor Cyan
Write-Host "Tag:        $tag" -ForegroundColor Cyan
Write-Host "Channel:    $Channel" -ForegroundColor Cyan
Write-Host "Installers: $($installers.Count) file(s)" -ForegroundColor Cyan
Write-Host ""

$confirm = Read-Host "Publish release $tag? (y/N)"
if ($confirm -eq 'y' -or $confirm -eq 'Y') {
    gh release edit $tag --repo $Repository --draft=false
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to publish release $tag."
    }
    Write-Host "`nRelease $tag published successfully!" -ForegroundColor Green
}
else {
    Write-Host "`nRelease NOT published. The draft release has been updated with the installer." -ForegroundColor Yellow
    Write-Host "You can publish later with:  gh release edit $tag --repo $Repository --draft=false" -ForegroundColor Yellow
}

Write-Host ""
