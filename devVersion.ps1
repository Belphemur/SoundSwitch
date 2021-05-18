$latestTag = git describe --tags --abbrev=0
$currentVersion = [Version]$latestTag.substring(1)
$currentBuild = $Env:GITHUB_RUN_ID
$scriptPath = $MyInvocation.MyCommand.Path
$scriptFolder = Split-Path $scriptPath -Parent

$newBuild = [int]$currentBuild.substring(0,5)
$newRevision = [int]$currentBuild.substring(5)

$newVersion = [Version]::new($currentVersion.Major, $currentVersion.Minor, $newBuild, $newRevision)


echo "::set-output name=version::$($newVersion.toString())"
