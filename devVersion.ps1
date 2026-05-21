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
    $major = $versionParts[0]
    $minor = $versionParts[1]
    $build = $versionParts[2].Split('-')[0]

    $currentDate = Get-Date
    $newRevision = [int]$currentDate.DayOfYear * 24 + $currentDate.Day + $currentDate.Hour + $currentDate.Minute
    $Version = (New-Object System.Version($major, $minor, $build, $newRevision)).ToString()
}

$numericVersion = $Version -replace '[-+].*$', ''
if ($numericVersion -notmatch '^\d+\.\d+\.\d+(\.\d+)?$') {
    throw "Version '$Version' does not contain a valid numeric file version."
}

$assemblyInfoPath = Join-Path $PSScriptRoot 'SoundSwitch\Properties\AssemblyInfo.cs'
$assemblyInfo = Get-Content $assemblyInfoPath -Raw

if ($assemblyInfo -notmatch 'AssemblyFileVersion\(".*?"\)') {
    throw "AssemblyFileVersion attribute was not found in $assemblyInfoPath."
}

if ($assemblyInfo -notmatch 'AssemblyInformationalVersion\(".*?"\)') {
    throw "AssemblyInformationalVersion attribute was not found in $assemblyInfoPath."
}

$assemblyInfo = $assemblyInfo -replace 'AssemblyFileVersion\(".*?"\)', "AssemblyFileVersion(`"$numericVersion`")"
$assemblyInfo = $assemblyInfo -replace 'AssemblyInformationalVersion\(".*?"\)', "AssemblyInformationalVersion(`"$Version`")"
Set-Content -Path $assemblyInfoPath -Value $assemblyInfo -Encoding utf8

if ($Env:GITHUB_OUTPUT) {
    "version=$Version" | Out-File -FilePath $Env:GITHUB_OUTPUT -Encoding utf8 -Append
}
else {
    Write-Output $Version
}
