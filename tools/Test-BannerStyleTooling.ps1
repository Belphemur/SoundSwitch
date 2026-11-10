#Requires -Version 7.0
<#
.SYNOPSIS
    Runs deterministic banner E2E cleanup and UI lookup checks without touching audio or launching the app.
.DESCRIPTION
    Loads only function definitions from Test-BannerStyleEndToEnd.ps1, substituting
    endpoint control, UI Automation, and the app process with fakes. Configuration
    and PID files are confined to a unique temporary directory.
.EXAMPLE
    pwsh tools/Test-BannerStyleTooling.ps1
#>
[CmdletBinding()]
param()
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$tokens = $null
$parseErrors = $null
$ast = [System.Management.Automation.Language.Parser]::ParseFile(
    (Join-Path $PSScriptRoot 'Test-BannerStyleEndToEnd.ps1'), [ref]$tokens, [ref]$parseErrors)
if ($parseErrors.Count -gt 0) { throw ($parseErrors | Out-String) }
foreach ($definition in $ast.FindAll({ param($node) $node -is [System.Management.Automation.Language.FunctionDefinitionAst] }, $false)) {
    Invoke-Expression $definition.Extent.Text
}

$directory = Join-Path ([IO.Path]::GetTempPath()) "SoundSwitchBannerTooling-$([Guid]::NewGuid())"
New-Item $directory -ItemType Directory | Out-Null
$configPath = Join-Path $directory 'config.json'
$backupPath = Join-Path $directory 'backup.json'
$Script:PidFile = Join-Path $directory 'instance.pid'
$checks = 0
function Check([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw "FAIL: $Message" }
    $script:checks++
    Write-Host "PASS: $Message" -ForegroundColor Green
}
function Expect-Error([scriptblock]$Action, [string]$Pattern) {
    $message = $null
    try { & $Action } catch { $message = $_.Exception.Message }
    Check ($null -ne $message -and $message -like $Pattern) "Expected diagnostic: $Pattern"
}
function Read-SoundSwitchControlPid {
    if (Test-Path $Script:PidFile) { Get-Content $Script:PidFile }
}
function Invoke-MicrophoneControl([string[]]$Arguments) {
    $script:lastEndpointArguments = $Arguments
    if ($script:failRestore) { throw 'Recorded endpoint disconnected.' }
    [pscustomobject]@{ id = $Arguments[1]; isMuted = [bool]::Parse($Arguments[2]) }
}
function Find-SoundSwitchElement {
    param($Handle, $AutomationId)
    if ($script:vanishedElement) { throw 'Element is no longer available.' }
    return $null
}
function Get-Banners { return @() }

try {
    $microphone = [pscustomobject]@{ id = 'original-endpoint' }
    $originalMute = $true
    $hadConfig = $true
    $failRestore = $false
    $lastEndpointArguments = @()
    $instance = [pscustomobject]@{ Id = 12345; HasExited = $true }
    Set-Content $backupPath 'original configuration'
    Set-Content $configPath 'scratch configuration'
    Set-Content $Script:PidFile $instance.Id

    Restore-BannerStyleTestState
    Check (($lastEndpointArguments -join '|') -eq 'set|original-endpoint|true') 'Crash cleanup restores the captured endpoint, not the current default'
    Check ((Get-Content $configPath) -eq 'original configuration') 'Crash cleanup restores the configuration'
    Check (-not (Test-Path $Script:PidFile)) 'Crash cleanup removes its stale PID record'

    $instance = [pscustomobject]@{ Id = 12345; HasExited = $false }
    $instance | Add-Member ScriptMethod Kill { $this.HasExited = $true }
    $instance | Add-Member ScriptMethod WaitForExit { }
    Set-Content $configPath 'scratch configuration'
    Set-Content $Script:PidFile 98765
    $failRestore = $true
    Expect-Error { Restore-BannerStyleTestState } '*Recorded endpoint disconnected*'
    Check $instance.HasExited 'Endpoint restoration failure still stops the owned app'
    Check ((Get-Content $configPath) -eq 'original configuration') 'Endpoint restoration failure still restores configuration'
    Check ((Get-Content $Script:PidFile) -eq '98765') 'Cleanup preserves another invocation''s ownership record'

    $originalMute = $null
    $hadConfig = $false
    Restore-BannerStyleTestState
    Check (-not (Test-Path $configPath)) 'Cleanup removes scratch configuration when no original existed'

    $vanishedElement = $false
    Expect-Error { Get-BannerElementSnapshot ([IntPtr]123) 'lblTop' 'Compact/Fading' } '*Compact/Fading*lblTop*not found*'
    $vanishedElement = $true
    Expect-Error { Get-BannerElementSnapshot ([IntPtr]123) 'lblTitle' 'Normal/Persistent' } '*Normal/Persistent*lblTitle*no longer available*'
    Expect-Error { Get-RequiredBanner 'Compact/Fading' } '*No banner was found for Compact/Fading*'

    Write-Host "TOOLING PASSED: $checks checks." -ForegroundColor Green
}
finally {
    # Only the unique scratch files created by this test are removed.
    foreach ($path in @($configPath, $backupPath, $Script:PidFile)) {
        if (Test-Path $path) { Remove-Item -LiteralPath $path -Force }
    }
    Remove-Item -LiteralPath $directory
}
