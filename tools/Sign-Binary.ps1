<#
.SYNOPSIS
    Signs one or more executables with signtool using modern algorithms.

.DESCRIPTION
    Wraps signtool.exe to sign binaries with SHA-256 digest and a SHA-256
    RFC 3161 timestamp, avoiding the deprecated SHA-1 algorithm.

    Requires PowerShell 7+ (ships with Windows 11).

    The certificate is located by its subject name (CN).  By default the
    script looks for the Certum SimplySign certificate issued to
    "OpenSource Developer, Antoine Aflalo".

    The script retries up to -MaxRetries times on transient failures
    (e.g. timestamping server unavailable).

.PARAMETER Path
    One or more file paths to sign.

.PARAMETER CertificateName
    Subject name (CN) of the code-signing certificate.
    Defaults to "OpenSource Developer, Antoine Aflalo".

.PARAMETER TimestampUrl
    RFC 3161 timestamp server URL.
    Defaults to http://time.certum.pl (Certum's timestamp authority).

.PARAMETER MaxRetries
    Maximum number of signing attempts per file.  Defaults to 3.

.PARAMETER RetryDelaySec
    Seconds to wait between retries.  Defaults to 5.

.EXAMPLE
    .\tools\Sign-Binary.ps1 -Path Final\SoundSwitch.exe
    Signs a single executable with the default certificate.

.EXAMPLE
    .\tools\Sign-Binary.ps1 -Path Final\SoundSwitch.exe, Final\SoundSwitch.CLI.exe
    Signs multiple executables in one call.

.EXAMPLE
    .\tools\Sign-Binary.ps1 -Path Final\Installer\SoundSwitch_Installer.exe -CertificateName "My Cert"
    Signs the installer with a custom certificate name.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory, ValueFromPipeline, ValueFromPipelineByPropertyName)]
    [Alias('FullName')]
    [string[]]$Path,

    [string]$CertificateName = 'OpenSource Developer, Antoine Aflalo',

    [string]$TimestampUrl = 'http://time.certum.pl',

    [int]$MaxRetries = 3,

    [int]$RetryDelaySec = 5
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ── Locate signtool ─────────────────────────────────────────────────────────

$signtool = Get-Command 'signtool.exe' -ErrorAction SilentlyContinue
if (-not $signtool) {
    throw "signtool.exe not found on PATH. Run tools\Install-BuildTools.ps1 first."
}
Write-Host "Using signtool: $($signtool.Source)" -ForegroundColor DarkGray

# ── Sign each file ──────────────────────────────────────────────────────────

foreach ($file in $Path) {
    if (-not (Test-Path -LiteralPath $file)) {
        throw "File not found: $file"
    }

    $resolvedPath = (Resolve-Path -LiteralPath $file).Path
    $fileName     = [System.IO.Path]::GetFileName($resolvedPath)

    Write-Host "Signing $fileName ..." -ForegroundColor Cyan

    $attempt = 0
    $signed  = $false

    while (-not $signed -and $attempt -lt $MaxRetries) {
        $attempt++

        if ($attempt -gt 1) {
            Write-Host "  Retry $attempt/$MaxRetries in $RetryDelaySec seconds ..." -ForegroundColor Yellow
            Start-Sleep -Seconds $RetryDelaySec
        }

        # signtool sign
        #   /n   — certificate subject name
        #   /fd  — file digest algorithm (SHA-256)
        #   /tr  — RFC 3161 timestamp URL
        #   /td  — timestamp digest algorithm (SHA-256)
        #   /v   — verbose output
        & signtool.exe sign `
            /n  $CertificateName `
            /fd sha256 `
            /tr $TimestampUrl `
            /td sha256 `
            /v  $resolvedPath

        if ($LASTEXITCODE -eq 0) {
            $signed = $true
            Write-Host "  $fileName signed successfully." -ForegroundColor Green
        }
        else {
            Write-Warning "  signtool failed (exit code $LASTEXITCODE) for $fileName (attempt $attempt/$MaxRetries)."
        }
    }

    if (-not $signed) {
        throw "Failed to sign $fileName after $MaxRetries attempts."
    }
}
