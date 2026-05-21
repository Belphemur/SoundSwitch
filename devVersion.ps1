[CmdletBinding()]
param(
    [string]$Version
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($Version)) {
    $latestTag = git describe --tags --abbrev=0

    if ($latestTag.StartsWith('v')) {
        $latestTag = $latestTag.Substring(1)
    }

    $versionParts = $latestTag.Split('.')
    if ($versionParts.Length -lt 3) {
        throw "Latest tag '$latestTag' does not match expected format major.minor.build."
    }

    $buildPart = $versionParts[2].Split('-')[0]
    if (-not [int]::TryParse($versionParts[0], [ref]$major) -or
        -not [int]::TryParse($versionParts[1], [ref]$minor) -or
        -not [int]::TryParse($buildPart, [ref]$build)) {
        throw "Latest tag '$latestTag' does not contain numeric major, minor, and build components."
    }

    $currentDate = Get-Date
    $newRevision = ([int]$currentDate.DayOfYear * 1440) + ([int]$currentDate.Hour * 60) + [int]$currentDate.Minute
    $Version = (New-Object System.Version($major, $minor, $build, $newRevision)).ToString()
}

$numericVersion = $Version -replace '[-+].*$', ''
if ($numericVersion -notmatch '^\d+\.\d+\.\d+(\.\d+)?$') {
    throw "Version '$Version' does not contain a valid numeric file version."
}

$numericVersionParts = $numericVersion.Split('.')
if ($numericVersionParts.Length -eq 3) {
    $fileVersion = "$numericVersion.0"
}
elseif ($numericVersionParts.Length -eq 4) {
    $fileVersion = $numericVersion
}
else {
    throw "Version '$Version' must contain 3 or 4 numeric components."
}

$assemblyInfoPath = Join-Path $PSScriptRoot 'SoundSwitch\Properties\AssemblyInfo.cs'
$assemblyInfo = Get-Content $assemblyInfoPath -Raw

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
else {
    Write-Output $Version
}
