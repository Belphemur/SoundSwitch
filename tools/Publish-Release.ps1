<#
.SYNOPSIS
    Full release workflow: prepare binaries, build and sign the installer,
    upload to the draft GitHub release, and publish.

.DESCRIPTION
    Orchestrates the complete SoundSwitch release process:

    1. Populates the Final\ directory with binaries — either by downloading
       the build-artifact zip from a draft GitHub release (default) or by
       building from source (-BuildFromSource).
    2. When building from source: generates HTML documentation from Markdown
       and bundles additional assets.  When downloading from a draft release
       the CI pipeline has already included these in the zip, so this step
       is skipped.
    3. Delegates to Build-Installer.ps1 to sign binaries, compile the Inno
       Setup installer, and sign the installer.
    4. Uploads the signed installer(s) to the draft release, replacing any
       existing installer assets.
    5. Extracts the latest section from CHANGELOG.md as the release body,
       asks the user whether they want to prepend additional notes, and
       updates the release body on GitHub.
    6. Asks for confirmation before publishing the release.

    When -BuildFromSource is used, steps 4–6 are skipped (no draft release
    to publish to).

    Requires PowerShell 7+ and an authenticated GitHub CLI (`gh auth login`)
    when downloading from a draft release.

.PARAMETER Channel
    Release channel: 'release' (default) or 'beta'.
    'release' looks for a non-pre-release draft; 'beta' looks for a
    pre-release draft.  Only used when downloading from a draft release.

.PARAMETER Repository
    GitHub repository in owner/repo format (default: Belphemur/SoundSwitch).

.PARAMETER BuildFromSource
    Build binaries from source instead of downloading from a draft release.
    When set, the GitHub CLI is not required and publish steps are skipped.

.PARAMETER Configuration
    Build configuration when -BuildFromSource is set: Release (default) or
    Debug.  Ignored when downloading from a draft release.

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
    .\tools\Publish-Release.ps1 -BuildFromSource
    Build from source and create the installer locally (no publishing).

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

    [switch]$BuildFromSource,

    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',

    [switch]$SkipSigning,

    [string]$CertificateName = 'Open Source Developer Antoine Aflalo',

    [string]$InstallerReleaseState,

    [string]$DotNetMajorVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Derive InstallerReleaseState from Channel when not explicitly provided
if (-not $PSBoundParameters.ContainsKey('InstallerReleaseState')) {
    $InstallerReleaseState = if ($Channel -eq 'beta') { 'Beta' } else { 'Release' }
}

# Detect .NET major version from global.json if not explicitly provided
if (-not $PSBoundParameters.ContainsKey('DotNetMajorVersion')) {
    $globalJson = Join-Path $repoRoot 'global.json'
    if (Test-Path $globalJson) {
        $json = Get-Content $globalJson -Raw | ConvertFrom-Json
        if ($json.sdk.version) {
            # Extract major version from "10.0" -> "10"
            $DotNetMajorVersion = ($json.sdk.version -split '\.')[0]
            Write-Host "  Detected .NET major version: $DotNetMajorVersion (from global.json)" -ForegroundColor DarkGray
        }
    }
    if (-not $DotNetMajorVersion) {
        $DotNetMajorVersion = '10'  # fallback default
    }
}

# ── Paths ────────────────────────────────────────────────────────────────────

$repoRoot    = Split-Path $PSScriptRoot -Parent
$finalDir    = Join-Path $repoRoot 'Final'
$projectName = 'SoundSwitch'
$cliProject  = 'SoundSwitch.CLI'

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

    # Capture stdout only; let stderr pass through to the console for diagnostics
    $json = gh release list --repo $Repository --json tagName,isDraft,isPrerelease,name --limit 30
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to list releases (gh exit code $LASTEXITCODE)."
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

$tag   = $null
$draft = $null

if (-not $BuildFromSource) {
    Assert-GitHubCli
}

# ── Step 1: Populate Final\ ─────────────────────────────────────────────────

if ($BuildFromSource) {
    # ── Build from source ────────────────────────────────────────────────
    Write-Host "=== Building from source ($Configuration) ===" -ForegroundColor White

    # Clean
    foreach ($dir in @('bin', 'obj', 'Release', $finalDir)) {
        $fullPath = if ([System.IO.Path]::IsPathRooted($dir)) { $dir } else { Join-Path $repoRoot $dir }
        if (Test-Path $fullPath) {
            Remove-Item $fullPath -Recurse -Force
        }
    }
    New-Item -ItemType Directory -Path $finalDir -Force | Out-Null

    # Publish CLI first, then main app (main app wins on shared files)
    # Framework-dependent publish: IL assemblies are architecture-agnostic
    $publishDir = $finalDir

    foreach ($proj in @($cliProject, $projectName)) {
        $projPath = Join-Path $repoRoot "$proj\$proj.csproj"
        Write-Host "  Publishing $proj ..."
        dotnet publish -c $Configuration $projPath -o $publishDir
        if ($LASTEXITCODE -ne 0) {
            throw "dotnet publish failed for $proj with exit code $LASTEXITCODE."
        }
    }
}
else {
    # ── Download from draft release ──────────────────────────────────────
    $includePreRelease = $Channel -eq 'beta'
    $draft = Find-DraftRelease -Repository $Repository -IncludePreRelease $includePreRelease

    if (-not $draft) {
        throw "No matching draft $Channel release found for $Repository. Has semantic-release run?"
    }

    $tag = $draft.tagName
    Write-Host "Found draft release: $($draft.name) ($tag)" -ForegroundColor Green
    Write-Host ""

    Write-Host "=== Downloading release artifact ===" -ForegroundColor White

    if (Test-Path $finalDir) {
        Write-Host "  Cleaning existing output directory: $finalDir"
        Remove-Item $finalDir -Recurse -Force
    }
    New-Item -ItemType Directory -Path $finalDir -Force | Out-Null

    # Download the zip asset to a temp directory
    $tempDir = Join-Path ([System.IO.Path]::GetTempPath()) "soundswitch-release-$([guid]::NewGuid().ToString('N').Substring(0,8))"
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

    try {
        Write-Host "  Downloading SoundSwitch zip from $tag ..."
        gh release download $tag --repo $Repository --pattern 'SoundSwitch-v*.zip' --dir $tempDir
        if ($LASTEXITCODE -ne 0) {
            throw "Failed to download release asset from $tag."
        }

        $zipFiles = @(Get-ChildItem $tempDir -Filter 'SoundSwitch-v*.zip')
        if ($zipFiles.Count -eq 0) {
            throw "No SoundSwitch zip asset found in release $tag."
        }
        if ($zipFiles.Count -gt 1) {
            $names = ($zipFiles | ForEach-Object { $_.Name }) -join ', '
            throw "Multiple SoundSwitch zip assets found in release $tag ($names). Expected exactly one."
        }

        $zipFile = $zipFiles[0]
        $zipSize = $zipFile.Length
        Write-Host "  Downloaded $([math]::Round($zipSize / 1MB, 2)) MB -> $($zipFile.Name)"

        # Extract
        Write-Host "  Extracting to $finalDir ..."
        Expand-Archive -Path $zipFile.FullName -DestinationPath $finalDir -Force
    }
    finally {
        Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    }

    $fileCount = (Get-ChildItem $finalDir -Recurse -File).Count
    Write-Host "  Extracted $fileCount files to $finalDir" -ForegroundColor Green
}

Write-Host ""

# ── Step 2: Generate HTML docs & bundle assets (only for source builds) ──────
# When downloading from a draft release, the CI pipeline has already generated
# HTML documentation and bundled assets into the zip artifact.

if ($BuildFromSource) {
    Write-Host "=== Generating HTML documentation ===" -ForegroundColor White

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

    Write-Host ""
}

# ── Step 3: Build installer ─────────────────────────────────────────────────

Write-Host "=== Building installer ===" -ForegroundColor White

$buildScript = Join-Path $PSScriptRoot 'Build-Installer.ps1'
$buildArgs = @{
    FinalDir              = $finalDir
    InstallerReleaseState = $InstallerReleaseState
    CertificateName       = $CertificateName
    DotNetMajorVersion    = $DotNetMajorVersion
}
if ($SkipSigning) {
    $buildArgs['SkipSigning'] = $true
}

& $buildScript @buildArgs

Write-Host ""

# ── Upload and publish (only when downloading from draft) ────────────────────

if (-not $tag) {
    Write-Host "Build-from-source complete. No draft release to publish." -ForegroundColor Green
    Write-Host ""
    return
}

# ── Step 4: Upload installer to draft release ────────────────────────────────

Write-Host "=== Uploading installer to release $tag ===" -ForegroundColor White

$installerDir = Join-Path $finalDir 'Installer'
if (-not (Test-Path $installerDir)) {
    throw "Installer directory not found at $installerDir. Did the build succeed?"
}

$installers = @(Get-ChildItem $installerDir -Filter '*Installer.exe')
if ($installers.Count -eq 0) {
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

$confirm = Read-Host "Publish release ${tag}? (y/N)"
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
