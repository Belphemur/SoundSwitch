$currentVersion = [Version]$(git describe --tags --abbrev=0).substring(1)
$currentBuild = $Env:GITHUB_RUN_ID
$scriptPath = $MyInvocation.MyCommand.Path
$scriptFolder = Split-Path $scriptPath -Parent

$newBuild = [int]$currentBuild.substring(0,5)
$newRevision = [int]$currentBuild.substring(5)

$newVersion = [Version]::new($currentVersion.Major, $currentVersion.Minor, $newBuild, $newRevision)

Set-Content -Path "$scriptFolder\version" -Value $newVersion.toString()

echo $newVersion.toString()

