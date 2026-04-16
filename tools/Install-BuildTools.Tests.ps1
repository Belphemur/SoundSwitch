#Requires -Version 7.0
#Requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0' }

<#
.SYNOPSIS
    Pester tests for Install-BuildTools.ps1 helper functions.

.DESCRIPTION
    Extracts and tests the helper functions defined in Install-BuildTools.ps1
    without executing the main script body.  External commands (winget,
    signtool, Get-ChildItem on Windows Kits) are mocked so that the tests are
    fully isolated and do not require any build tools to be installed.
#>

BeforeAll {
    # ── Extract helper functions from the script ────────────────────────────
    #
    # Install-BuildTools.ps1 contains top-level code that runs on dot-source,
    # so we parse the AST and evaluate only the function definitions.
    $scriptPath = Join-Path $PSScriptRoot 'Install-BuildTools.ps1'

    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $scriptPath, [ref]$tokens, [ref]$parseErrors
    )

    # Fail fast if the script has syntax errors — the very issue we fixed.
    if ($parseErrors.Count -gt 0) {
        throw "Install-BuildTools.ps1 has parse errors:`n$($parseErrors | Out-String)"
    }

    # Evaluate each function definition in the current scope.
    $functionDefs = $ast.FindAll(
        { param($node) $node -is [System.Management.Automation.Language.FunctionDefinitionAst] },
        $false   # don't recurse into nested scopes
    )
    foreach ($fn in $functionDefs) {
        . ([scriptblock]::Create($fn.Extent.Text))
    }
}

# ─────────────────────────────────────────────────────────────────────────────
# Test-CommandExists
# ─────────────────────────────────────────────────────────────────────────────
Describe 'Test-CommandExists' {
    It 'returns $true for a command that exists' {
        # 'Get-Command' is always available in PowerShell
        Test-CommandExists 'Get-Command' | Should -BeTrue
    }

    It 'returns $false for a command that does not exist' {
        Test-CommandExists 'NoSuchCommand_xyzzy_12345' | Should -BeFalse
    }
}

# ─────────────────────────────────────────────────────────────────────────────
# Find-SignToolInWindowsKits
# ─────────────────────────────────────────────────────────────────────────────
Describe 'Find-SignToolInWindowsKits' {
    It 'returns $null when the Windows Kits base path does not exist' {
        Mock Test-Path { $false } -ParameterFilter { $Path -like '*Windows Kits*' }

        Find-SignToolInWindowsKits | Should -BeNullOrEmpty
    }

    It 'returns the x64 directory when signtool.exe is found' {
        $fakeDir = 'C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64'
        $fakeFile = [PSCustomObject]@{
            DirectoryName = $fakeDir
            Name          = 'signtool.exe'
        }

        Mock Test-Path { $true } -ParameterFilter { $Path -like '*Windows Kits*' }
        Mock Get-ChildItem { $fakeFile } -ParameterFilter { $Path -like '*Windows Kits*' }

        Find-SignToolInWindowsKits | Should -Be $fakeDir
    }

    It 'returns the highest version when multiple versions exist' {
        $fakeFiles = @(
            [PSCustomObject]@{
                DirectoryName = 'C:\Program Files (x86)\Windows Kits\10\bin\10.0.19041.0\x64'
                Name          = 'signtool.exe'
            },
            [PSCustomObject]@{
                DirectoryName = 'C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64'
                Name          = 'signtool.exe'
            }
        )

        Mock Test-Path { $true } -ParameterFilter { $Path -like '*Windows Kits*' }
        Mock Get-ChildItem { $fakeFiles } -ParameterFilter { $Path -like '*Windows Kits*' }

        # Sort descending means 22621 comes before 19041
        Find-SignToolInWindowsKits | Should -Be 'C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64'
    }

    It 'returns $null when signtool.exe exists only in non-x64 directories' {
        $fakeFile = [PSCustomObject]@{
            DirectoryName = 'C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\arm64'
            Name          = 'signtool.exe'
        }

        Mock Test-Path { $true } -ParameterFilter { $Path -like '*Windows Kits*' }
        Mock Get-ChildItem { $fakeFile } -ParameterFilter { $Path -like '*Windows Kits*' }

        Find-SignToolInWindowsKits | Should -BeNullOrEmpty
    }
}

# ─────────────────────────────────────────────────────────────────────────────
# Install-WingetPackage
# ─────────────────────────────────────────────────────────────────────────────
Describe 'Install-WingetPackage' {
    BeforeEach {
        # Suppress Write-Host output during tests
        Mock Write-Host {}
    }

    It 'skips installation when the package is already installed' {
        # Simulate winget list finding the package
        Mock winget {
            $global:LASTEXITCODE = 0
            'SomePublisher.SomeApp  SomeApp  1.0.0  winget'
        } -ParameterFilter { $args[0] -eq 'list' }

        Install-WingetPackage -Id 'SomePublisher.SomeApp' -Name 'SomeApp'

        Should -Invoke winget -Times 1 -Exactly
    }

    It 'calls winget install when the package is not installed' {
        # First call: winget list – package not found
        Mock winget {
            $global:LASTEXITCODE = 1
            ''
        } -ParameterFilter { $args[0] -eq 'list' }

        # Second call: winget install – success
        Mock winget {
            $global:LASTEXITCODE = 0
        } -ParameterFilter { $args[0] -eq 'install' }

        Install-WingetPackage -Id 'SomePublisher.SomeApp' -Name 'SomeApp'

        Should -Invoke winget -Times 1 -Exactly -ParameterFilter { $args[0] -eq 'install' }
    }

    It 'throws when winget install fails' {
        Mock winget {
            $global:LASTEXITCODE = 1
            ''
        } -ParameterFilter { $args[0] -eq 'list' }

        Mock winget {
            $global:LASTEXITCODE = 42
        } -ParameterFilter { $args[0] -eq 'install' }

        { Install-WingetPackage -Id 'SomePublisher.SomeApp' -Name 'SomeApp' } |
            Should -Throw '*Failed to install*'
    }

    It 'uses the Id as display name when Name is not provided' {
        Mock winget {
            $global:LASTEXITCODE = 0
            'SomePublisher.SomeApp  SomeApp  1.0.0  winget'
        } -ParameterFilter { $args[0] -eq 'list' }

        Install-WingetPackage -Id 'SomePublisher.SomeApp'

        Should -Invoke Write-Host -ParameterFilter {
            $Object -like '*SomePublisher.SomeApp*'
        }
    }

    It 'includes --scope argument when Scope is provided' {
        Mock winget {
            $global:LASTEXITCODE = 1
            ''
        } -ParameterFilter { $args[0] -eq 'list' }

        Mock winget {
            $global:LASTEXITCODE = 0
        } -ParameterFilter { $args[0] -eq 'install' }

        Install-WingetPackage -Id 'SomePublisher.SomeApp' -Name 'SomeApp' -Scope 'user'

        Should -Invoke winget -Times 1 -Exactly -ParameterFilter {
            $args[0] -eq 'install' -and ($args -contains '--scope') -and ($args -contains 'user')
        }
    }
}

# ─────────────────────────────────────────────────────────────────────────────
# Install-SignTool
# ─────────────────────────────────────────────────────────────────────────────
Describe 'Install-SignTool' {
    BeforeAll {
        # Install-SignTool references $Scope from the parent script scope.
        # Set it so the function can find it.
        $script:Scope = 'machine'
    }

    BeforeEach {
        Mock Write-Host {}
    }

    It 'returns early when signtool.exe is already on PATH' {
        Mock Test-CommandExists { $true } -ParameterFilter { $Command -eq 'signtool.exe' }
        Mock Find-SignToolInWindowsKits {}

        Install-SignTool

        Should -Invoke Find-SignToolInWindowsKits -Times 0
    }

    It 'adds the found signtool directory to session PATH' {
        $fakeDir = 'C:\Program Files (x86)\Windows Kits\10\bin\10.0.22621.0\x64'

        Mock Test-CommandExists { $false } -ParameterFilter { $Command -eq 'signtool.exe' }
        Mock Find-SignToolInWindowsKits { $fakeDir }

        # Save and restore PATH
        $originalPath = $env:Path
        $env:Path = 'C:\Windows'
        try {
            Install-SignTool
            $env:Path | Should -BeLike "*$fakeDir*"
        }
        finally {
            $env:Path = $originalPath
        }
    }

    It 'installs WDK via winget when signtool.exe is not found initially' {
        Mock Test-CommandExists { $false } -ParameterFilter { $Command -eq 'signtool.exe' }

        $callCount = 0
        Mock Find-SignToolInWindowsKits {
            $script:callCount++
            if ($script:callCount -eq 1) { return $null }
            return 'C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64'
        }
        Mock Install-WingetPackage {}

        $originalPath = $env:Path
        $env:Path = 'C:\Windows'
        try {
            Install-SignTool

            Should -Invoke Install-WingetPackage -Times 1 -Exactly -ParameterFilter {
                $Id -eq 'Microsoft.WindowsWDK.10.0.26100'
            }
        }
        finally {
            $env:Path = $originalPath
        }
    }

    It 'throws when signtool.exe is not found even after WDK installation' {
        Mock Test-CommandExists { $false } -ParameterFilter { $Command -eq 'signtool.exe' }
        Mock Find-SignToolInWindowsKits { $null }
        Mock Install-WingetPackage {}

        { Install-SignTool } | Should -Throw '*signtool.exe not found*'
    }
}

# ─────────────────────────────────────────────────────────────────────────────
# Script-level parse validation
# ─────────────────────────────────────────────────────────────────────────────
Describe 'Install-BuildTools.ps1 script' {
    It 'has no parse errors' {
        $scriptPath = Join-Path $PSScriptRoot 'Install-BuildTools.ps1'
        $tokens = $null
        $errors = $null
        $null = [System.Management.Automation.Language.Parser]::ParseFile(
            $scriptPath, [ref]$tokens, [ref]$errors
        )
        $errors.Count | Should -Be 0
    }

    It 'contains a #Requires -Version 7.0 directive' {
        $scriptPath = Join-Path $PSScriptRoot 'Install-BuildTools.ps1'
        $content = Get-Content $scriptPath -Raw
        $content | Should -Match '#Requires\s+-Version\s+7\.0'
    }

    It 'defines the expected helper functions' {
        # These were already loaded in BeforeAll, so just verify they exist
        Get-Command Test-CommandExists   -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Install-WingetPackage -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Find-SignToolInWindowsKits -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
        Get-Command Install-SignTool      -ErrorAction SilentlyContinue | Should -Not -BeNullOrEmpty
    }
}
