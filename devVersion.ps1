$latestTag = git describe --tags --abbrev=0

# Remove the leading 'v' character if present
if ($latestTag.StartsWith("v")) {
    $latestTag = $latestTag.Substring(1)
}

# Split the version string into major, minor, build, and revision components
$versionParts = $latestTag.Split('.')
$major = $versionParts[0]
$minor = $versionParts[1]
$build = $versionParts[2].Split('-')[0]

# Calculate the new revision number
$currentDate = Get-Date
$newRevision = [int]$currentDate.DayOfYear * 24 + $currentDate.Day + $currentDate.Hour + $currentDate.Minute

# Construct the new version with the calculated revision
$newVersion = New-Object System.Version($major, $minor, $build, $newRevision)

# Replace the AssemblyVersion in AssemblyInfo.cs
(Get-Content SoundSwitch/Properties/AssemblyInfo.cs) -replace "AssemblyVersion\(.+\)", "AssemblyVersion(`"$newVersion`")" | Set-Content SoundSwitch/Properties/AssemblyInfo.cs

# Output the new version to GITHUB_OUTPUT
"version=$($newVersion.ToString())" | Set-Content $Env:GITHUB_OUTPUT
