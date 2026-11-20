#Requires -Version 7.0
<#
.SYNOPSIS
    Captures the microphone mute banner in a controlled environment for visual review.

.DESCRIPTION
    Built for the win11-dev VM (or any Windows box with an audio device):
    writes a known-good SoundSwitchConfiguration.json (persistent mute banner,
    banner notifications everywhere, a selected microphone), launches the
    freshly built SoundSwitch.exe, toggles the microphone mute through the
    public CLI, and captures the banner window with PrintWindow the moment it
    appears. Any branch can be reviewed this way: build the branch, run the
    script, compare the PNG against another branch's PNG.

    Unlike Capture-DocumentationScreenshot.ps1 (Settings-window captures),
    this drives the banner notification itself, so presentation changes
    (Normal/Compact style, icons, layout) can be judged from real renders.

.PARAMETER Style
    BannerStyle to write into the configuration: 'Normal' or 'Compact'.
    Ignored (but tolerated) on branches that predate the setting.

.PARAMETER OutFile
    Output PNG path. Defaults to banner-<style>.png in the current directory.

.PARAMETER NoBuild
    Reuse existing Debug binaries instead of building the solution.

.EXAMPLE
    pwsh tools/Capture-BannerScreenshot.ps1 -Style Compact -OutFile banner-compact.png
    Builds, configures, fires a persistent mute banner and captures it.

.EXAMPLE
    pwsh tools/Capture-BannerScreenshot.ps1 -Style Normal -NoBuild
    Captures the Normal presentation with the existing build.

.NOTES
    Requires an interactive desktop session (WinForms + window enumeration do
    not work from a service session). On a headless VM, run through a
    scheduled task in the console session, e.g.:
      schtasks /create /tn CapBanner /tr "pwsh -NoProfile -ExecutionPolicy Bypass -File <repo>\tools\Capture-BannerScreenshot.ps1 -Style Compact -OutFile C:\Users\hermes\banner.png" /sc once /st 23:59 /f
      schtasks /run /tn CapBanner
#>
[CmdletBinding()]
param(
    [ValidateSet('Normal', 'Compact')]
    [string]$Style = 'Compact',
    [string]$OutFile = "banner-$Style.png",
    [switch]$NoBuild
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$root = Split-Path -Parent $PSScriptRoot

# ── helpers ──────────────────────────────────────────────────────────────────
$log = Join-Path $env:TEMP 'banner-capture.log'
Remove-Item $log -ErrorAction SilentlyContinue
function Write-CapLog([string]$Message) { Add-Content $log "$(Get-Date -Format T) $Message" }

# ── 1. Build ─────────────────────────────────────────────────────────────────
if (-not $NoBuild) {
    & dotnet build (Join-Path $root 'SoundSwitch.sln') -c Debug --nologo -v minimal
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
}

# ── 2. Configuration ──────────────────────────────────────────────────────────
# A deterministic config is the whole point: banner notifications everywhere,
# persistent mute banner, the requested style, and a microphone selected from
# the machine's actual endpoints (the first capture device).
$configPath = Join-Path $env:APPDATA 'SoundSwitch/SoundSwitchConfiguration.json'
$microphone = & (Join-Path $root 'SoundSwitch.CLI/bin/Debug') -ErrorAction SilentlyContinue
# Resolve the first capture endpoint id via the registry (works pre-app-launch).
$capRoot = 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\MMDevices\Audio\Capture'
$micId = (Get-ChildItem $capRoot | Select-Object -First 1).PSChildName
if (-not $micId) { throw 'No capture audio endpoint found - install a virtual audio device first (e.g. VB-CABLE).' }
Write-CapLog "microphone endpoint: $micId"

$cfg = [ordered]@{
    BannerStyle                = $(if ($Style -eq 'Compact') { 1 } else { 0 }) # ignored on pre-style branches
    ChangeCommunications       = $true
    FirstRun                   = $false
    SwitchDeviceNotification   = 5   # BannerNotification
    SwitchProfileNotification  = 5
    MicrophoneMuteNotification  = 5
    MicrophoneMuteBanner       = 2   # Persistent: the banner stays until state changes
    MicrophoneUnmuteBanner     = 1   # Fading
    # The PersistentMuteNotification migration resets MicrophoneMuteBanner to None
    # unless this flag is set AND the migration is marked done:
    PersistentMuteNotification = $true
    MigratedFields             = @('PersistentMuteNotification')
    BannerPosition             = 5   # BottomRight
    BannerDisplayInfo          = 3
    BannerOnScreenTime         = '00:00:30'
    BannerOpacityPercentage    = 100
    MaxNumberNotification      = 1
    NotifyUsingPrimaryScreen   = $true
    BannerScalePercentage      = 100
    SelectedRecordingDeviceListId = @($micId)
    # Mute banners are scoped to MuteRecordingDevice (LimitMuteBannersToTarget):
    MuteRecordingDevice        = @{ Name = 'capture-endpoint'; Id = $micId; Type = 1; IsUsb = $false; DiscoveredAt = (Get-Date).ToUniversalTime().ToString('o') }
    LimitMuteBannersToTarget   = $false
}
$cfgJson = $cfg | ConvertTo-Json -Depth 5
New-Item -ItemType Directory -Force (Split-Path $configPath) | Out-Null
# Back up any existing config so a dev machine is not silently reconfigured.
if (Test-Path $configPath) {
    Copy-Item $configPath "$configPath.bak-$(Get-Date -Format yyyyMMddHHmmss)" -Force
}
Set-Content $configPath $cfgJson -Encoding UTF8
Write-CapLog "config written (style=$Style), previous config backed up"

# ── 3. Launch ────────────────────────────────────────────────────────────────
Get-Process SoundSwitch -ErrorAction SilentlyContinue | Stop-Process -Force
Start-Sleep -Seconds 3
$appExe = (Get-ChildItem (Join-Path $root 'SoundSwitch/bin/Debug') -Filter SoundSwitch.exe -Recurse |
    Sort-Object FullName -Descending | Select-Object -First 1).FullName
$cliExe = (Get-ChildItem (Join-Path $root 'SoundSwitch.CLI/bin/Debug') -Filter SoundSwitch.CLI.exe -Recurse |
    Sort-Object FullName -Descending | Select-Object -First 1).FullName
Write-CapLog "app: $appExe"
Start-Process $appExe -ArgumentList '--disable-updater'
$deadline = (Get-Date).AddSeconds(60)
do { Start-Sleep -Seconds 2; $proc = Get-Process SoundSwitch -ErrorAction SilentlyContinue } until ($proc -or (Get-Date) -gt $deadline)
if (-not $proc) { throw 'SoundSwitch did not start. Check the app log under %APPDATA%\SoundSwitch\Logs.' }
Write-CapLog 'app started'
Start-Sleep -Seconds 12

# ── 4. Trigger ───────────────────────────────────────────────────────────────
$cliOut = & $cliExe mute --toggle 2>&1 | Out-String
Write-CapLog "cli mute: $cliOut"

# ── 5. Capture ────────────────────────────────────────────────────────────────
Add-Type @"
using System;
using System.Runtime.InteropServices;
public class BannerCapture {
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr hdc, uint flags);
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);
    [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc proc, IntPtr lParam);
    [DllImport("user32.dll")] public static extern int GetClassName(IntPtr hwnd, System.Text.StringBuilder text, int max);
    [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr hwnd, out uint pid);
    [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr hwnd);
    public delegate bool EnumProc(IntPtr hWnd, IntPtr lParam);
    [StructLayout(LayoutKind.Sequential)] public struct RECT { public int Left, Top, Right, Bottom; }
}
"@
Add-Type -AssemblyName System.Drawing
$appPid = (Get-Process SoundSwitch -ErrorAction Stop | Select-Object -First 1).Id
$seen = @{}
$captured = 0
$deadline = (Get-Date).AddSeconds(30)
while ((Get-Date) -lt $deadline -and $captured -lt 4) {
    $windows = New-Object System.Collections.ArrayList
    $enumCb = {
        param($hwnd, $l)
        $ownerPid = [uint32]0
        [BannerCapture]::GetWindowThreadProcessId($hwnd, [ref]$ownerPid) | Out-Null
        if ($ownerPid -eq $appPid -and [BannerCapture]::IsWindowVisible($hwnd)) { [void]$windows.Add($hwnd) }
        return $true
    }
    [BannerCapture]::EnumWindows($enumCb, [IntPtr]::Zero) | Out-Null
    foreach ($hwnd in $windows) {
        if ($seen.ContainsKey($hwnd)) { continue }
        $seen[$hwnd] = $true
        $class = New-Object System.Text.StringBuilder 256
        [BannerCapture]::GetClassName($hwnd, $class, 256) | Out-Null
        $rect = New-Object BannerCapture+RECT
        [BannerCapture]::GetWindowRect($hwnd, [ref]$rect) | Out-Null
        $w = $rect.Right - $rect.Left; $h = $rect.Bottom - $rect.Top
        Write-CapLog "window hwnd=$hwnd class=$($class.ToString()) ${w}x${h}"
        if ($w -gt 50 -and $h -gt 30) {
            $bmp = New-Object System.Drawing.Bitmap($w, $h)
            $g = [System.Drawing.Graphics]::FromImage($bmp)
            $hdc = $g.GetHdc()
            [BannerCapture]::PrintWindow($hwnd, $hdc, 2)  # PW_RENDERFULLCONTENT
            $g.ReleaseHdc($hdc); $g.Dispose()
            $out = if ($captured -eq 0) { $OutFile } else { "$($OutFile -replace '\.png$','')-$captured.png" }
            $bmp.Save($out, [System.Drawing.Imaging.ImageFormat]::Png)
            $bmp.Dispose()
            Write-CapLog "captured $out"
            $captured++
        }
    }
    Start-Sleep -Milliseconds 800
}
if ($captured -eq 0) {
    throw "No banner window appeared within 30s. Capture log: $log"
}
Write-CapLog "total captured: $captured"

# ── 6. Cleanup ───────────────────────────────────────────────────────────────
Get-Process SoundSwitch -ErrorAction SilentlyContinue | Stop-Process -Force
Write-Host "Captured $captured banner screenshot(s). Log: $log" -ForegroundColor Green
