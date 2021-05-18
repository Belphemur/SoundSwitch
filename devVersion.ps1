$currentVersion = [Version]$(git describe --tags --abbrev=0).substring(1)
$currentBuild = $Env:GITHUB_RUN_ID
echo "BUILD : $currentBuild";
$newBuild = [int]$currentBuild.substring(0,5)
$newRevision = [int]$currentBuild.substring(5)

$newVersion = [Version]::new($currentVersion.Major, $currentVersion.Minor, $newBuild, $newRevision)

echo $newVersion.toString()
