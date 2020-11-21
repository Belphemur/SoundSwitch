$fileToCheck = "$PSScriptRoot\..\github-key.txt"
if (! (Test-Path $fileToCheck -PathType leaf))
{
    echo "Github-key file doesn't exists"
    exit
}

$lastTag =  $(git describe --abbrev=0 --tags $(git rev-parse HEAD~10))

$key = [IO.File]::ReadAllText($fileToCheck)

echo "Generate changelog since tag $lastTag"

github_changelog_generator --token $key --user Belphemur --project SoundSwitch --since-tag $lastTag --output LAST_VERSION_CHANGELOG.MD

$lastChangelog = Get-Content LAST_VERSION_CHANGELOG.MD | Select-Object -SkipLast 4

$lastChangelog + (get-content CHANGELOG.MD | Select-Object -Skip 1) | set-content CHANGELOG.MD
