param(
    [Parameter(Mandatory = $true)]
    [string]$ProjectDirectory
)

$ErrorActionPreference = 'Stop'

$designerFiles = @(
    'Localization\AboutStrings.Designer.cs',
    'Localization\SettingsStrings.Designer.cs',
    'Localization\TrayIconStrings.Designer.cs',
    'Localization\UpdateDownloadStrings.Designer.cs'
)

foreach ($relativePath in $designerFiles) {
    $filePath = Join-Path $ProjectDirectory $relativePath
    if (-not (Test-Path $filePath)) {
        continue
    }

    $content = Get-Content -Path $filePath -Raw
    $updatedContent = $content -replace 'new global::System\.Resources\.ResourceManager\(("SoundSwitch\.Localization\.[^"]+"), typeof\(([^)]+)\)\.Assembly\)', 'new global::SoundSwitch.Localization.LocalizedStringProvider($1, typeof($2).Assembly)'

    if ($updatedContent -ne $content) {
        [System.IO.File]::WriteAllText($filePath, $updatedContent)
    }
}