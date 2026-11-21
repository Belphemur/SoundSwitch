#Requires -Version 7.0
<#
.SYNOPSIS
    Captures the profile-volume mockup screenshots from the real SoundSwitch UI.

.DESCRIPTION
    Mockup deliverable for issue #2547 (ADR:
    docs/design/profile/2026-11-21-profile-volume-settings.md). Builds the app,
    kills any running SoundSwitch instance first (one named pipe and one config
    file per instance — a capture needs a fresh one), launches the freshly built
    Debug exe, opens Settings on the Profiles tab and clicks Add so the real
    UpsertProfileExtended form opens, then captures it twice:

      1. default state — the advanced volume section collapsed, which must look
         identical to today's form plus the collapsed disclosure row;
      2. expanded state — the disclosure opened, both "Set volume" checkboxes
         checked and the sliders set through UI Automation.

    The capture itself uses PrintWindow(PW_RENDERFULLCONTENT) cropped to the
    window bounds, like tools/Capture-DocumentationScreenshot.ps1. The UI
    interaction helpers come from tools/Control-SoundSwitch.ps1 (dot-sourced),
    and every step writes to the log so a failure is diagnosable from it.

    On displays too short for the form (the mockup VM's 800x600 basic adapter,
    for example), the window manager clamps the window height the same way it
    does for today's form - both captures then show the window as the OS allows
    it, with the whole advanced section visible in the expanded shot and the
    trigger list absorbing the clamp, exactly like the current form does.

    The script needs an interactive desktop session (UI Automation + window
    capture). When driven from a non-interactive agent session, run it through
    a scheduled task on the logged-on user (schtasks /IT) and poll the log for
    the final line: MOCKUP-CAPTURE-COMPLETE or MOCKUP-CAPTURE-FAILED.

.PARAMETER OutputDirectory
    Directory the PNGs are written to. Defaults to <repo>\artifacts.

.PARAMETER Configuration
    Build configuration to run. Defaults to Debug.

.PARAMETER TabName
    Localized display name of the Profiles tab (pass the translated label on a
    non-English UI). Defaults to "Profiles".

.PARAMETER PlaybackVolume, RecordingVolume
    Slider positions to set in the expanded capture. Defaults to 35 and 80.

.PARAMETER NoBuild
    Skip the build step and use the existing binaries.

.PARAMETER TimeoutSeconds
    How long to wait for the app, windows and controls. Defaults to 90.

.EXAMPLE
    pwsh tools\Capture-ProfileVolumeMockup.ps1
    Builds Debug, captures artifacts\mockup-default.png and artifacts\mockup-expanded.png.
#>
[CmdletBinding()]
param(
    [string]$OutputDirectory,

    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [string]$TabName = 'Profiles',

    [ValidateRange(0, 100)]
    [int]$PlaybackVolume = 35,

    [ValidateRange(0, 100)]
    [int]$RecordingVolume = 80,

    [switch]$NoBuild,

    [int]$TimeoutSeconds = 90
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$RepoRoot = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path (Join-Path $RepoRoot 'SoundSwitch.sln'))) {
    throw "Could not locate the repository root from '$PSScriptRoot'."
}

if (-not $OutputDirectory) {
    $OutputDirectory = Join-Path $RepoRoot 'artifacts'
}

# The repo's .NET lives in C:\dotnet on the build VM; DOTNET_ROOT makes the
# framework-dependent apphosts (SoundSwitch.exe, SoundSwitch.CLI.exe) find it
# even when PATH does not carry it.
if (Test-Path 'C:\dotnet') {
    $env:DOTNET_ROOT = 'C:\dotnet'
    if ((Test-Path 'C:\dotnet\dotnet.exe') -and ($env:Path -notlike '*C:\dotnet*')) {
        $env:Path = "C:\dotnet;$env:Path"
    }
}

# --- assertions ---------------------------------------------------------------------

$script:Failures = 0
$script:Checks = 0

function Assert-True {
    param([Parameter(Mandatory)][bool]$Condition, [Parameter(Mandatory)][string]$Message)
    $script:Checks++
    if ($Condition) { Write-Host "  PASS  $Message" -ForegroundColor Green }
    else { $script:Failures++; Write-Host "  FAIL  $Message" -ForegroundColor Red }
}

# --- guard: kill any running instance first -----------------------------------------

foreach ($instance in @(Get-Process -Name 'SoundSwitch' -ErrorAction SilentlyContinue)) {
    Write-Host "Killing running SoundSwitch instance (PID $($instance.Id))..." -ForegroundColor Yellow
    $instance | Stop-Process -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 300
}

try {
    # --- build -----------------------------------------------------------------------

    if (-not $NoBuild) {
        Write-Host 'Building the application and CLI...' -ForegroundColor Cyan
        foreach ($project in @('SoundSwitch\SoundSwitch.csproj', 'SoundSwitch.CLI\SoundSwitch.CLI.csproj')) {
            & dotnet build (Join-Path $RepoRoot $project) -c $Configuration --nologo -v minimal
            if ($LASTEXITCODE -ne 0) {
                throw "dotnet build failed for '$project'."
            }
        }
    }

    # --- helpers from the live-app control surface -------------------------------------

    # Control-SoundSwitch.ps1 declares its own param(...) block, and dot-sourcing
    # executes it in THIS scope - it would clobber same-named variables such as
    # $OutputDirectory and $TimeoutSeconds with its defaults. Keep the capture's
    # own values under dedicated names instead.
    $pngDirectory = $OutputDirectory
    $captureTimeout = $TimeoutSeconds

    . (Join-Path $PSScriptRoot 'Control-SoundSwitch.ps1')

    function Resolve-MockupExecutable {
        param([Parameter(Mandatory)][string]$ProjectDirectory, [Parameter(Mandatory)][string]$FileName)
        $found = Resolve-SoundSwitchExecutable -ProjectDirectory $ProjectDirectory -FileName $FileName
        if (-not $found) {
            throw "$FileName not found under $ProjectDirectory\bin\$Configuration. Build first or drop -NoBuild."
        }
        return $found
    }

    function Set-MockupCheckBox {
        <# Toggles a check box on (no-op when already checked). #>
        param([Parameter(Mandatory)]$Element, [Parameter(Mandatory)][string]$AutomationId)

        $toggle = $null
        if (-not $Element.TryGetCurrentPattern([System.Windows.Automation.TogglePattern]::Pattern, [ref]$toggle)) {
            throw "Check box '$AutomationId' does not expose a TogglePattern."
        }

        if ($toggle.Current.ToggleState -ne [System.Windows.Automation.ToggleState]::On) {
            $toggle.Toggle()
            Start-Sleep -Milliseconds 150
        }
    }

    function Set-MockupTrackBar {
        <#
            Sets a track bar position through UI Automation. The WinForms TrackBar proxy
            exposes a ValuePattern (with a direct set + read-back); the keyboard fallback
            (Home + Large/Small steps) drives the same value-changed events a user
            triggers when no value surface is available at all.
        #>
        param(
            [Parameter(Mandatory)]$Element,
            [Parameter(Mandatory)][string]$AutomationId,
            [Parameter(Mandatory)][ValidateRange(0, 100)][int]$Value
        )

        $valuePattern = $null
        if ($Element.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$valuePattern)) {
            $valuePattern.SetValue([string]$Value)
            Start-Sleep -Milliseconds 150
            $actual = [int][Math]::Round([double]$valuePattern.Current.Value)
            Assert-True -Condition ($actual -eq $Value) -Message "Track bar '$AutomationId' reads back $actual (requested $Value)"
            return
        }

        Write-Host "  '$AutomationId' has no ValuePattern; falling back to keyboard input..." -ForegroundColor Yellow
        $null = $Element.SetFocus()
        Start-Sleep -Milliseconds 150
        $handle = $Element.Current.NativeWindowHandle
        function Send-Key([uint32]$vk) {
            [SoundSwitch.AppControl.Native]::PostMessage([IntPtr]$handle, 0x0100, [IntPtr]$vk, [IntPtr]::Zero) | Out-Null
            [SoundSwitch.AppControl.Native]::PostMessage([IntPtr]$handle, 0x0101, [IntPtr]$vk, [IntPtr]::Zero) | Out-Null
            Start-Sleep -Milliseconds 15
        }

        Send-Key 0x24      # VK_HOME -> position 0
        for ($i = 0; $i -lt [int][Math]::Floor($Value / 10); $i++) { Send-Key 0x22 }  # VK_NEXT, one LargeChange (10)
        for ($i = 0; $i -lt ($Value % 10); $i++) { Send-Key 0x27 }                    # VK_RIGHT, one SmallChange
        Start-Sleep -Milliseconds 200

        # Keyboard input raises the same value-changed events; without a value
        # surface the read-back assertion is simply skipped (the PNG shows the result).
    }

    # --- launch a fresh instance --------------------------------------------------------

    $appExe = Resolve-MockupExecutable -ProjectDirectory 'SoundSwitch' -FileName 'SoundSwitch.exe'
    $cliExe = Resolve-MockupExecutable -ProjectDirectory 'SoundSwitch.CLI' -FileName 'SoundSwitch.CLI.exe'

    Write-Host "Starting $($appExe.FullName) ..." -ForegroundColor Cyan
    $process = Start-SoundSwitchApp -AppExe $appExe.FullName -CliExe $cliExe.FullName -TimeoutSeconds $captureTimeout

    # --- open Settings on the Profiles tab, then the Add-profile dialog -----------------

    Write-Host "Opening Settings..." -ForegroundColor Cyan
    $settings = Open-SoundSwitchSettings -CliExe $cliExe.FullName -ProcessId $process.Id -TimeoutSeconds $captureTimeout

    if (-not (Select-SoundSwitchTab -Handle $settings.Handle -TabName $TabName)) {
        throw "The Settings window does not have a '$TabName' tab."
    }
    Write-Host "Selected the '$TabName' tab." -ForegroundColor Cyan

    $preClickHandles = @(Get-SoundSwitchWindows -ProcessId $process.Id | ForEach-Object { $_.Handle.ToInt64() })
    $addButton = Find-SoundSwitchButton -Handle $settings.Handle -ButtonId 'addProfileButton'
    if ($null -eq $addButton) {
        Write-SoundSwitchDiagnostics -ProcessId $process.Id
        throw "The 'Add' button (addProfileButton) was not found on the '$TabName' tab."
    }

    Write-Host "Clicking 'addProfileButton'..." -ForegroundColor Cyan
    Invoke-SoundSwitchElement -Element $addButton

    $profileForm = Wait-SoundSwitchNewWindow -ProcessId $process.Id -KnownHandles $preClickHandles -TimeoutSeconds $captureTimeout
    Write-Host "The profile form opened: '$($profileForm.Title)'" -ForegroundColor Cyan

    try {
        # --- capture 1: default (advanced collapsed) ------------------------------------

        Start-Sleep -Milliseconds 600
        $defaultPath = Join-Path $pngDirectory 'mockup-default.png'
        Write-Host "Capturing the collapsed form..." -ForegroundColor Cyan
        $defaultShot = Save-SoundSwitchScreenshot -Handle $profileForm.Handle -Path $defaultPath
        Write-Host "Saved $($defaultShot.Path) ($($defaultShot.Width)x$($defaultShot.Height))." -ForegroundColor Green

        # The volume controls stay hidden behind the collapsed disclosure: either UIA
        # does not expose them at all (WinForms drops invisible controls from the
        # automation tree) or reports them off-screen.
        $hiddenCheckbox = Find-SoundSwitchElement -Handle $profileForm.Handle -AutomationId 'playbackVolumeCheckBox'
        Assert-True -Condition ($null -eq $hiddenCheckbox -or $hiddenCheckbox.Current.IsOffscreen) -Message "The playback volume group is hidden while the disclosure is collapsed"

        # --- expand the advanced section ------------------------------------------------

        $toggle = Find-SoundSwitchButton -Handle $profileForm.Handle -ButtonId 'advancedToggleButton'
        if ($null -eq $toggle) {
            Write-SoundSwitchDiagnostics -ProcessId $process.Id
            throw "The Advanced disclosure (advancedToggleButton) was not found in the profile form."
        }

        Write-Host "Expanding the Advanced section..." -ForegroundColor Cyan
        Invoke-SoundSwitchElement -Element $toggle
        Start-Sleep -Milliseconds 400

        $visibleCheckbox = Find-SoundSwitchElement -Handle $profileForm.Handle -AutomationId 'playbackVolumeCheckBox'
        Assert-True -Condition ($visibleCheckbox -and -not $visibleCheckbox.Current.IsOffscreen) -Message "The playback volume group is on screen after expanding"

        # --- enable both volume presets and set the sliders ------------------------------

        foreach ($group in @(
                @{ Id = 'playbackVolumeCheckBox'; Bar = 'playbackVolumeTrackBar'; Value = $PlaybackVolume },
                @{ Id = 'recordingVolumeCheckBox'; Bar = 'recordingVolumeTrackBar'; Value = $RecordingVolume }
            )) {
            $checkBox = Find-SoundSwitchElement -Handle $profileForm.Handle -AutomationId $group.Id
            if ($null -eq $checkBox) {
                Write-SoundSwitchDiagnostics -ProcessId $process.Id
                throw "Check box '$($group.Id)' was not found in the profile form."
            }

            Write-Host "Checking '$($group.Id)' and setting its slider to $($group.Value)..." -ForegroundColor Cyan
            Set-MockupCheckBox -Element $checkBox -AutomationId $group.Id

            $trackBar = Find-SoundSwitchElement -Handle $profileForm.Handle -AutomationId $group.Bar
            if ($null -eq $trackBar) {
                throw "Track bar '$($group.Bar)' was not found in the profile form."
            }
            Assert-True -Condition $trackBar.Current.IsEnabled -Message "The '$($group.Bar)' slider is enabled while its box is checked"
            Set-MockupTrackBar -Element $trackBar -AutomationId $group.Bar -Value $group.Value
        }

        # --- capture 2: expanded with both presets --------------------------------------

        Start-Sleep -Milliseconds 400
        $expandedPath = Join-Path $pngDirectory 'mockup-expanded.png'
        Write-Host "Capturing the expanded form..." -ForegroundColor Cyan
        $expandedShot = Save-SoundSwitchScreenshot -Handle $profileForm.Handle -Path $expandedPath
        Write-Host "Saved $($expandedShot.Path) ($($expandedShot.Width)x$($expandedShot.Height))." -ForegroundColor Green

        # --- verify the PNGs are real renders -------------------------------------------
        #
        # A flat WinForms window PNG compresses to tens of KB even at full window size,
        # so "non-trivial" is judged by rendered content, not bytes: the image must
        # decode, carry sane dimensions and enough distinct colours that a blank or
        # clipped-to-nothing render is impossible.

        Add-Type -AssemblyName System.Drawing
        foreach ($shot in @($defaultShot, $expandedShot)) {
            $file = Get-Item $shot.Path
            Assert-True -Condition ($file.Length -ge 10KB) -Message "$($file.Name) is not an empty capture ($([int]($file.Length / 1KB)) KB)"
            Assert-True -Condition ($shot.Width -ge 600 -and $shot.Height -ge 500) -Message "$($file.Name) has sane dimensions ($($shot.Width)x$($shot.Height))"

            # A valid, decodable PNG: the signature check plus System.Drawing decode.
            $bytes = [System.IO.File]::ReadAllBytes($shot.Path)
            $signatureOk = ($bytes.Length -gt 8 -and
                $bytes[0] -eq 0x89 -and $bytes[1] -eq 0x50 -and $bytes[2] -eq 0x4E -and $bytes[3] -eq 0x47)
            Assert-True -Condition $signatureOk -Message "$($file.Name) carries a PNG signature"
            $image = $null
            try {
                $image = [System.Drawing.Image]::FromFile($shot.Path)
                Assert-True -Condition ($image.Width -eq $shot.Width -and $image.Height -eq $shot.Height) -Message "$($file.Name) decodes at the captured size"

                $colors = [System.Collections.Generic.HashSet[int]]::new()
                $bitmap = [System.Drawing.Bitmap]$image
                for ($y = 0; $y -lt $bitmap.Height; $y += 4) {
                    for ($x = 0; $x -lt $bitmap.Width; $x += 4) {
                        $null = $colors.Add($bitmap.GetPixel($x, $y).ToArgb())
                        if ($colors.Count -ge 400) { break }
                    }
                    if ($colors.Count -ge 400) { break }
                }
                Assert-True -Condition ($colors.Count -ge 100) -Message "$($file.Name) rendered real content ($($colors.Count)+ distinct colors sampled)"
            }
            finally {
                if ($image) { $image.Dispose() }
            }
        }

        # --- summary ---------------------------------------------------------------------

        if ($script:Failures -gt 0) {
            Write-Host "MOCKUP-CAPTURE-FAILED: $($script:Failures) of $($script:Checks) checks failed." -ForegroundColor Red
            exit 1
        }

        Write-Host "All $($script:Checks) checks passed." -ForegroundColor Green
        Write-Host "MOCKUP-CAPTURE-COMPLETE default=$($defaultShot.Path) expanded=$($expandedShot.Path)" -ForegroundColor Green
        exit 0
    }
    finally {
        # Close what was opened, inner dialog first (the profile form is non-modal,
        # but closing order keeps Settings coherent).
        [SoundSwitch.AppControl.Native]::PostMessage($profileForm.Handle, $Script:WmClose, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
        Start-Sleep -Milliseconds 300
        [SoundSwitch.AppControl.Native]::PostMessage($settings.Handle, $Script:WmClose, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
        Start-Sleep -Milliseconds 300
    }
}
catch {
    Write-Host "MOCKUP-CAPTURE-FAILED: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    # The instance this script launched is stopped again; config writes on exit are
    # the app's own (a profile is only added by an explicit Save, which this capture
    # never presses).
    if ((Get-Command Stop-SoundSwitchApp -ErrorAction SilentlyContinue) -and
        @(Get-Process -Name 'SoundSwitch' -ErrorAction SilentlyContinue).Count -gt 0) {
        Stop-SoundSwitchApp -ErrorAction SilentlyContinue
    }
}
