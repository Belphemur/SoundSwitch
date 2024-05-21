$latestTag = git describe --tags --abbrev=0
# Split the version string into major, minor, build, and revision components
$versionParts = $latestTag.Split('.')
$major = $versionParts[0]
$minor = $versionParts[1]
$build = $versionParts[2]

# Handle the beta version separately
if ($versionParts.Length -gt 3) {
    # Assuming the beta version follows the pattern 'major.minor.build-beta'
    $revision = $versionParts[3].Split('-')[0] # Extract the numeric part before '-beta'
} else {
    # No beta version, so use the last part as the revision
    $revision = $versionParts[3]
}

# Construct the new version with the calculated revision
$newRevision = [int]$currentDate.DayOfYear * 24 + $currentDate.Day + $currentDate.Hour + $currentDate.Minute
$newVersion = New-Object System.Version($major, $minor, $build, $revision)

# Replace the AssemblyVersion in AssemblyInfo.cs
(Get-Content SoundSwitch/Properties/AssemblyInfo.cs) -replace "AssemblyVersion\(.+\)", "AssemblyVersion(`"$newVersion`")" | Set-Content SoundSwitch/Properties/AssemblyInfo.cs

# Output the new version to GITHUB_OUTPUT
"version=$($newVersion.ToString())" | Set-Content $Env:GITHUB_OUTPUT
