[CmdletBinding()]
param(
    [string]$Version
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($Version)) {
    $latestTag = git describe --tags --abbrev=0
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($latestTag)) {
        throw "Unable to determine the latest git tag."
    }

    if ($latestTag.StartsWith('v')) {
        $latestTag = $latestTag.Substring(1)
    }

    $versionParts = $latestTag.Split('.')
    if ($versionParts.Length -lt 3) {
        throw "Latest tag '$latestTag' does not match expected format major.minor.build."
    }

    $buildPart = $versionParts[2].Split('-')[0]
    [int]$major = 0
    [int]$minor = 0
    [int]$build = 0
    if (-not [int]::TryParse($versionParts[0], [ref]$major) -or
        -not [int]::TryParse($versionParts[1], [ref]$minor) -or
        -not [int]::TryParse($buildPart, [ref]$build)) {
        throw "Latest tag '$latestTag' does not contain numeric major, minor, and build components."
    }

    # Revision = total commit count: monotonic, reproducible, and small enough for
    # a .NET AssemblyFileVersion component, which is a 16-bit ushort (max 65535).
    # The previous time-of-day revision (DayOfYear*1440 + hour*60 + minute, ~340k)
    # silently wrapped there: 7.2.1.341394 became 7.2.1.13714 in the built binaries,
    # diverging from the installer filename and the nightly feed.
    $revision = git rev-list --count HEAD
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($revision)) {
        throw "Unable to count commits for the nightly revision."
    }

    if ([int]$revision -ge 65536) {
        throw "Commit count ($revision) reached the 16-bit revision limit. Bump the base version."
    }

    $Version = (New-Object System.Version($major, $minor, $build, [int]$revision)).ToString()
}

if ($Env:GITHUB_OUTPUT) {
    "version=$Version" | Out-File -FilePath $Env:GITHUB_OUTPUT -Encoding utf8 -Append
}

Write-Output $Version
