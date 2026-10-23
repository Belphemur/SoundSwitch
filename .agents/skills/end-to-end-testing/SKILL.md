---
name: End-to-end testing
description: Write and run end-to-end tests that drive a live SoundSwitch instance — launching the app, sending CLI/IPC commands, selecting Settings controls with UI Automation, and asserting user-visible behaviour with safe config/lifecycle isolation. Use this when adding an E2E test for a user-visible feature (new setting, hotkey, tray action, notification, device behaviour) or when reviewing one.
---

# End-to-end testing

Unit tests prove logic; end-to-end tests prove the thing a user actually sees and
does. In SoundSwitch an E2E test launches the real executable, drives it through
its public surfaces (CLI over the named pipe, and UI Automation for the WinForms
Settings window), and asserts observable outcomes.

`tools/Test-MuteTargetEndToEnd.ps1` is the reference implementation. Read it
alongside this skill.

## When to write one

Write an E2E test when the behaviour spans layers that unit tests cannot join:

- a setting is persisted and then changes what the app does (mute target, hotkey,
  banner style, profile trigger);
- a tray/hotkey/CLI action must reach the real audio stack;
- a notification/banner must appear, update, or disappear;
- device arrival/removal changes behaviour.

Do **not** write one for pure formatting, parsing, or math — unit tests are
faster and deterministic.

## The three layers

| Layer | Tool | What it proves |
| --- | --- | --- |
| Unit | `dotnet test` (e.g. `tools/Test-MuteTarget.ps1`) | Logic, serialization, matching, layout math |
| Control | `tools/Control-SoundSwitch.ps1` | One action works against a live app |
| E2E | e.g. `tools/Test-MuteTargetEndToEnd.ps1` | A user-visible workflow works end to end |

Build E2E tests **on top of** `Control-SoundSwitch.ps1` instead of re-implementing
Win32 interop. It already provides:

- `Get-SoundSwitchProcess`, `Start-SoundSwitchApp`, `Stop-SoundSwitchApp`
- `Invoke-SoundSwitchCli` (CLI over the named pipe)
- `Get-SoundSwitchWindows`, `Select-SoundSwitchTab`, `Find-SoundSwitchButton`,
  `Invoke-SoundSwitchElement`, `Set-SoundSwitchMuteTarget`
- `Open-SoundSwitchSettings`, `Wait-SoundSwitchNewWindow`
- `Save-SoundSwitchScreenshot`

Dot-source it, or call the script with the action flags. Add a new reusable helper
there when the interaction is generic; keep feature-specific assertions in the
E2E test.

## Skeleton

```powershell
#Requires -Version 7.0
[CmdletBinding()]
param(
    [switch]$NoBuild,
    [switch]$AllowExistingInstance,
    [switch]$KeepRunning,
    [int]$TimeoutSeconds = 90
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$RepoRoot      = Split-Path -Parent $PSScriptRoot
$ControlScript = Join-Path $PSScriptRoot 'Control-SoundSwitch.ps1'
$ConfigPath    = Join-Path $env:APPDATA 'SoundSwitch\SoundSwitchConfiguration.json'
$BackupPath    = Join-Path $env:TEMP 'SoundSwitchControl\SoundSwitchConfiguration.backup.json'

# --- assertions ---------------------------------------------------------------------
$script:Failures = 0
$script:Checks = 0

function Assert-True {
    param([Parameter(Mandatory)][bool]$Condition, [Parameter(Mandatory)][string]$Message)
    $script:Checks++
    if ($Condition) { Write-Host "  PASS  $Message" -ForegroundColor Green }
    else            { $script:Failures++; Write-Host "  FAIL  $Message" -ForegroundColor Red }
}

function Assert-Equal {
    param([Parameter(Mandatory)]$Expected, [Parameter(Mandatory)]$Actual, [Parameter(Mandatory)][string]$Message)
    Assert-True -Condition ("$Expected" -eq "$Actual") -Message "$Message (expected '$Expected', got '$Actual')"
}

# --- config isolation ---------------------------------------------------------------
# ... backup, restore in finally (see below)

# --- lifecycle ----------------------------------------------------------------------
# ... launch / attach, stop the instance you started in finally

# --- scenario -----------------------------------------------------------------------
# ... drive the app, then assert

if ($script:Failures -gt 0) {
    Write-Host "END-TO-END FAILED: $($script:Failures) of $($script:Checks) checks failed." -ForegroundColor Red
    exit 1
}
Write-Host "END-TO-END PASSED: $($script:Checks) checks." -ForegroundColor Green
```

## Non-negotiable rules

1. **Isolate user state.** Back up
   `%APPDATA%\SoundSwitch\SoundSwitchConfiguration.json` before touching the app
   and restore it in `finally`, even on failure. If it did not exist, delete the
   one the test created.
2. **Clean up the app.** Stop only instances this test started. Track that with a
   local `$launched` flag (not a PID file — a previous `-Start` must not be
   killed). Honour `-KeepRunning` for debugging.
3. **Never assume a single instance.** The app uses one named pipe and one config
   file. If an instance is already running, either fail with a clear message or
   attach with `-AllowExistingInstance`.
4. **Launch with `--disable-updater`.** Otherwise a background update check can
   interfere with timing and networking.
5. **Wait for readiness, don't sleep.** Poll `SoundSwitch.CLI.exe status` until it
   exits 0 (IPC pipe up) with a deadline. Never `Start-Sleep` a guessed duration.
6. **Assert observable behaviour.** Prefer CLI output, config content, and window
   state over internal state.
7. **Restore the mute state** if the test toggles it, so the machine is unchanged.
8. **Report a summary and exit non-zero on failure**, so CI and humans see a clear
   verdict.

## Config isolation

```powershell
$backupDirectory = Split-Path -Parent $BackupPath
if (-not (Test-Path $backupDirectory)) {
    New-Item -ItemType Directory -Path $backupDirectory -Force | Out-Null
}

$hadConfig = Test-Path $ConfigPath
if ($hadConfig) { Copy-Item $ConfigPath $BackupPath -Force }

try {
    # ... scenario ...
}
finally {
    if ($hadConfig -and (Test-Path $BackupPath)) {
        Copy-Item $BackupPath $ConfigPath -Force
    }
    elseif (-not $hadConfig -and (Test-Path $ConfigPath)) {
        Remove-Item $ConfigPath -Force -ErrorAction SilentlyContinue
    }
}
```

The app writes the config on change and on a clean exit. If you need it to load a
hand-edited config, **stop the app first**, edit the file, then start it.

## Launch and readiness

```powershell
$process = Start-Process -FilePath $appExe -ArgumentList '--disable-updater' -PassThru
$deadline = (Get-Date).AddSeconds($TimeoutSeconds)
while ((Get-Date) -lt $deadline) {
    if ($process.HasExited) { throw "SoundSwitch exited immediately (code $($process.ExitCode))." }
    & $cliExe status *> $null
    if ($LASTEXITCODE -eq 0) { break }
    Start-Sleep -Milliseconds 500
}
```

`status` is the cheapest IPC readiness probe: it fails fast while the pipe is not
registered yet.

## UI Automation cookbook

Prefer **automation ids** over visible labels: labels are localized and change.

| Task | How |
| --- | --- |
| Find Settings window | window that exposes `ControlType.TabItem` descendants (`Select-SoundSwitchTab` returns `$false` otherwise) |
| Select a tab | find `TabItem` by `Name`, `SelectionItemPattern.Select()` |
| Find a control | `PropertyCondition(AutomationIdProperty, '<designerName>')` on descendants |
| Combo box | `ExpandCollapsePattern.Expand()` → find `ControlType.ListItem` children → `SelectionItemPattern.Select()` → `Collapse()` |
| Button | `Find-SoundSwitchButton` by name or automation id; prefer the one with `IsOffscreen = $false` |
| Click that opens a modal | `PostMessage(hwnd, BM_CLICK, 0, 0)` — the UIA `Invoke()` pattern blocks until the dialog closes |
| Detect a dialog | snapshot window handles before the click, then wait for a handle not in the snapshot (`Wait-SoundSwitchNewWindow`) |
| Capture | `PrintWindow(hwnd, hdc, PW_RENDERFULLCONTENT)` cropped to `GetWindowRect` |

Automation ids come from the WinForms designer (`Settings.Designer.cs`) or the
programmatic `Name = "..."` assignment (e.g. the mute target combo uses
`muteTargetComboBox`).

Closing a window: `PostMessage(hwnd, WM_CLOSE, 0, 0)`. Avoid `Close()` via UIA,
which can block on modal confirmation.

## Parsing CLI output

The CLI is human-oriented by default. Parse with an anchored regex and throw a
useful error when it does not match, e.g.:

```powershell
$match = [regex]::Match($output, '^(?<name>.+?) is (?:now |currently )(?<state>muted|unmuted)\s*$',
    [System.Text.RegularExpressions.RegexOptions]::Multiline)
if (-not $match.Success) { throw "Unexpected output: $output" }
```

Where a command supports `--json`, prefer it over text parsing.

## Gotchas that cost real time

- **`Set-StrictMode -Version Latest` + indexing.** `$array[0]` on an empty array
  throws `Index was outside the bounds of the array`. Use
  `$array | Select-Object -First 1` or check `.Count` first.
- **Strict mode + missing JSON properties.** The config serializer uses
  `NullValueHandling.Ignore`, so a `null` setting is *absent*, not `null`.
  Guard with `$config.PSObject.Properties.Name -contains '<Name>'` before reading.
- **Strict mode + `$null`.** Accessing a property on `$null` throws. Guard every
  optional object.
- **`$existing.Count` on a single object.** `Get-Process` returns a scalar when one
  process matches; wrap in `@(...)`.
- **`$MyInvocation.InvocationType` does not exist.** To detect dot-sourcing use
  `$MyInvocation.InvocationName`.
- **Splatting vs. literal flags.** `& $script @args -NoBuild` works, but keep the
  hashtable and literal flags visually separate to avoid binding surprises.
- **Localized UI.** Tab and button *names* are translated; automation ids and
  control types are not. Prefer the latter.
- **Single instance.** Launching a second instance silently hands off to the first
  (single named pipe) or fails; check first.
- **Config clobbering.** A test that edits the config while the app is running
  races with the app's own saves. Stop, edit, start.
- **PowerShell writes to stderr.** `dotnet`/`git` often emit progress on stderr;
  judge success by exit code, not by an empty stderr stream.

## Registering the test

1. Put it in `tools/` and name it after the workflow:
   `tools/Test-<Feature>EndToEnd.ps1`.
2. Add a one-line entry to `tools/README.md`.
3. Add the command to the matching skill (or create one) so reviewers can run it.
4. If the test exposes a reusable interaction, add a helper to
   `Control-SoundSwitch.ps1` rather than duplicating interop.
5. Keep the unit-test script separate (e.g. `tools/Test-<Feature>.ps1`) so the fast
   path stays fast.

## Running in a review

```powershell
pwsh tools\Test-<Feature>.ps1              # fast, no app needed
pwsh tools\Test-<Feature>EndToEnd.ps1      # real app, real devices
```

Only the E2E layer needs microphones, a desktop session, and a running app; make
that requirement explicit in the script help so a contributor on a headless box
knows to stick to the unit layer.
