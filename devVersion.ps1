$latestTag = git describe --tags --abbrev=0
$currentVersion = [Version]$latestTag.substring(1)
$scriptPath = $MyInvocation.MyCommand.Path
$scriptFolder = Split-Path $scriptPath -Parent

$currentDate = (Get-Date)
$newRevision = [int]$currentDate.DayOfYear * 24 + $currentDate.Day + $currentDate.Hour + $currentDate.Minute

$newVersion = [Version]::new($currentVersion.Major, $currentVersion.Minor, $currentVersion.Build, $newRevision)
(Get-Content SoundSwitch/Properties/AssemblyInfo.cs) -replace "AssemblyVersion\(.+\)","AssemblyVersion(`"$newVersion`")" |  Out-File SoundSwitch/Properties/AssemblyInfo.cs

"version=$($newVersion.toString())" |  Out-File $Env:GITHUB_OUTPUT