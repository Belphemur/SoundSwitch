# Sort-Imports.ps1
param (
    [string]$path = ".",
    [switch]$whatIf
)

$files = Get-ChildItem -Path $path -Filter "*.cs" -Recurse | Where-Object { 
    $_.FullName -notmatch "\\obj\\" -and 
    $_.FullName -notmatch "\\bin\\" -and 
    $_.FullName -notmatch "\\.git\\" -and
    $_.FullName -notmatch "\\.Designer\.cs$"
}

foreach ($file in $files) {
    Write-Host "Processing $($file.FullName)..." -ForegroundColor Cyan
    $content = Get-Content $file.FullName
    $newContent = @()
    $usings = @()
    $headerDone = $false
    $inUsings = $false
    $usingsEndLine = -1

    for ($i = 0; $i -lt $content.Count; $i++) {
        $line = $content[$i].Trim()
        if ($line -match "^using\s+[^=]+;") {
            $usings += $content[$i]
            $inUsings = $true
            $usingsEndLine = $i
        } elseif ($line -eq "" -and $inUsings) {
            # Skip empty lines inside or immediately after using block
        } elseif ($line -ne "" -and $inUsings) {
            # End of using block
            $sortedUsings = $usings | Sort-Object -Unique
            $newContent += $sortedUsings
            $newContent += ""
            $inUsings = $false
            # Add the current non-using line
            $newContent += $content[$i..($content.Count-1)]
            break
        } elseif ($line -match "^using\s+[^=]+=[^;]+;") {
            # Aliased usings - keep them but maybe sort separately?
            # For now, treat them as non-usings to avoid complex parsing
            if ($usings.Count -gt 0) {
                $sortedUsings = $usings | Sort-Object -Unique
                $newContent += $sortedUsings
                $newContent += ""
                $usings = @()
            }
            $newContent += $content[$i]
            $inUsings = $false
        } else {
            $newContent += $content[$i]
        }
    }

    if ($usings.Count -gt 0 -and $inUsings) {
         $sortedUsings = $usings | Sort-Object -Unique
         $newContent += $sortedUsings
    }

    if ($newContent.Count -gt 0) {
        if ($whatIf) {
            Write-Host "Would write to $($file.FullName)"
        } else {
            $newContent | Set-Content $file.FullName
        }
    }
}
