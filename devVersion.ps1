[CmdletBinding()]
param(
    [string]$Version
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$latestTagRef = ''
if ([string]::IsNullOrWhiteSpace($Version)) {
    $latestTagRef = git describe --tags --abbrev=0
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($latestTagRef)) {
        throw "Unable to determine the latest git tag."
    }

    $latestTag = $latestTagRef
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

    # Revision = commits since the base tag: bounded (bumping the tag resets it) and
    # monotonic within a release train. A .NET AssemblyFileVersion component is a
    # 16-bit ushort (max 65535); the previous time-of-day revision (~340k daily)
    # silently wrapped there, e.g. 7.2.1.341394 became 7.2.1.13714 in the binaries.
    $revision = git rev-list --count "$latestTagRef..HEAD"
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($revision)) {
        throw "Unable to count commits since $latestTagRef."
    }

    if ([int]$revision -ge 65536) {
        throw "Commit count since $latestTagRef ($revision) reached the 16-bit revision limit. Cut a new release so the next nightly starts from its tag."
    }

    $Version = (New-Object System.Version($major, $minor, $build, [int]$revision)).ToString()
}

# Stamp the version into the committed AssemblyInfo.cs. The app project sets
# <GenerateAssemblyInfo>false</GenerateAssemblyInfo>, so these attributes are the
# ONLY thing that reaches the compiled binaries - MSBuild /p: properties cannot
# override them (that mismatch is exactly what broke feed/installer/app parity).
$assemblyInfoPath = Join-Path $PSScriptRoot 'SoundSwitch\Properties\AssemblyInfo.cs'
if (-not (Test-Path $assemblyInfoPath)) {
    throw "AssemblyInfo.cs not found at $assemblyInfoPath."
}
$assemblyInfo = Get-Content $assemblyInfoPath -Raw

$numericVersion = $Version -replace '[-+].*$', ''
if ($numericVersion -notmatch '^\d+\.\d+\.\d+(\.\d+)?$') {
    throw "Version '$Version' does not contain a valid numeric file version."
}

# Every AssemblyFileVersion component is a 16-bit ushort (max 65535); reject
# out-of-range components on the explicit -Version path too, not just the
# auto-computed one.
foreach ($component in $numericVersion.Split('.')) {
    if ([int]$component -ge 65536) {
        throw "Version '$Version' contains a component ($component) exceeding the 16-bit AssemblyFileVersion limit (65535)."
    }
}

$fileVersion = $numericVersion
$parts = $numericVersion.Split('.')
if ($parts.Length -eq 3) {
    $fileVersion = "$numericVersion.0"
}

if ($assemblyInfo -notmatch 'AssemblyFileVersion\(".*?"\)') {
    throw "AssemblyFileVersion attribute was not found in $assemblyInfoPath."
}
if ($assemblyInfo -notmatch 'AssemblyInformationalVersion\(".*?"\)') {
    throw "AssemblyInformationalVersion attribute was not found in $assemblyInfoPath."
}

$assemblyInfo = $assemblyInfo -replace 'AssemblyFileVersion\(".*?"\)', "AssemblyFileVersion(`"$fileVersion`")"
$assemblyInfo = $assemblyInfo -replace 'AssemblyInformationalVersion\(".*?"\)', "AssemblyInformationalVersion(`"$Version`")"
Set-Content -Path $assemblyInfoPath -Value $assemblyInfo -Encoding utf8

if ($Env:GITHUB_OUTPUT) {
    "version=$Version" | Out-File -FilePath $Env:GITHUB_OUTPUT -Encoding utf8 -Append
}

Write-Output $Version
