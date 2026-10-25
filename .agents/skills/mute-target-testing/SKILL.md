---
name: Mute target testing
description: Test the microphone mute targeting and banner scope feature — unit tests, a live-app control script, and an end-to-end test that drives the real Settings UI and CLI. Use this when working on mute target selection, mute banner scoping, device identity resolution, or banner reconciliation, or when reviewing such changes.
---

# Mute target testing

The microphone mute targeting feature lets a user pick which recording endpoint the
mute hotkey/CLI acts on, and whether mute **banners** are shown only for that
selected input or for every microphone.

Testing has three layers. Use the cheapest layer that can catch your change, and
run the end-to-end layer before shipping anything that touches the mute target,
banner scope, or device resolution.

| Layer | Command | Time | Needs a running app | Needs real microphones |
| --- | --- | --- | --- | --- |
| Unit tests | `pwsh tools\Test-MuteTarget.ps1` | ~10 s | no | no |
| Live control | `pwsh tools\Control-SoundSwitch.ps1 ...` | seconds | yes (script can launch) | optional |
| End-to-end | `pwsh tools\Test-MuteTargetEndToEnd.ps1` | ~1 min | yes (script launches) | yes |

## What the feature does

1. **Mute target selection.** A `DeviceInfo` snapshot (`MuteRecordingDevice`) is
   persisted in the configuration. When set, mute toggles/queries act on that
   device; when unset or unavailable, they fall back to the Windows default
   communications input. The selection is kept while the device is gone, so it is
   used again once the device returns.
2. **Device identity resolution.** Endpoint ids are not stable across driver
   updates and re-enumeration, so the stored snapshot is matched against live
   devices in three tiers: exact id → stable instance path → unique name
   (`NameClean`). A successful match heals the stored snapshot if the id or name
   changed. An ambiguous name match returns no device.
3. **Banner scope.** When `LimitMuteBannersToTarget` is set and the active
   microphone-mute notification is a banner, mute banners are pinned only for the
   resolved target (or the current default input if no target is configured or it
   is unavailable). Sound and toast notifications keep reporting every microphone.
4. **Banner reconciliation.** When the target, default input, mute style, or device
   list changes, persistent banners are reconciled: out-of-scope banners are
   dismissed, in-scope muted devices get their banner back, all updated in place
   (no flicker).

## Layer 1 — unit tests

```powershell
pwsh tools\Test-MuteTarget.ps1          # feature fixtures (~2 s after build)
pwsh tools\Test-MuteTarget.ps1 -List    # list the fixtures
pwsh tools\Test-MuteTarget.ps1 -All     # full SoundSwitch.Tests suite
pwsh tools\Test-MuteTarget.ps1 -Audio   # + COM audio manager tests (Windows)
pwsh tools\Test-MuteTarget.ps1 -NoBuild # skip the build
```

The script excludes `RefreshDeviceTests.TestMultipleRefresh` via a test filter.
That test is a timing-sensitive COM refresh race which CI also skips
(`Assert.Ignore` when the `CI` env var is set) and which fails intermittently on
fast machines regardless of code changes. The rest of the fixture still runs.

| Area | Fixture |
| --- | --- |
| Persisted target + scope flag | `MuteTargetConfigurationTests` |
| Device identity resolution | `AppRuleDeviceResolverTests` |
| Matching machinery | `DeviceReadOnlyCollectionMatchTests` |
| Snapshot serialization | `DeviceInfoSerializationTests` |
| Banner pinning | `BannerMonitorTests` |
| Banner appearance | `BannerAppearanceTests` |
| Banner position | `BannerPositionTests`, `BannerPositionMigrationTests` |
| Mute notification content | `NotificationContentBuilderTests` |
| Settings layout | `SettingsBannerLayoutTests` |
| Device list events | `RefreshDeviceTests`, `DeviceCyclerTests` |

## Layer 2 — control a live app

`tools/Control-SoundSwitch.ps1` is the general-purpose control surface, built on
the same primitives as `Capture-DocumentationScreenshot.ps1` (public CLI/IPC
surface, UI Automation, `PrintWindow`). It can launch the app, send CLI commands,
select Settings tabs, choose the mute target, click buttons, and capture windows.

```powershell
pwsh tools\Control-SoundSwitch.ps1 -Start                       # build + launch, leave running
pwsh tools\Control-SoundSwitch.ps1 -Cli 'mute'                  # report mute state
pwsh tools\Control-SoundSwitch.ps1 -Cli 'mute -t'               # toggle mute
pwsh tools\Control-SoundSwitch.ps1 -MuteTarget 'Windows default input'
pwsh tools\Control-SoundSwitch.ps1 -MuteTarget 'Line In (High Definition Audio Device)'
pwsh tools\Control-SoundSwitch.ps1 -Tab Recording -Screenshot shot   # capture the Recording tab
pwsh tools\Control-SoundSwitch.ps1 -ListWindows                 # visible app windows
pwsh tools\Control-SoundSwitch.ps1 -Status                      # instances + binaries
pwsh tools\Control-SoundSwitch.ps1 -Stop                         # stop the instance it launched
```

Useful flags: `-NoBuild`, `-NoLaunch`, `-AllowExistingInstance`, `-KeepRunning`,
`-KeepOpen`, `-Json`, `-OutputDirectory`, `-TimeoutSeconds`, `-Force` (with `-Stop`,
also stop an instance it did not launch).

Dot-source it to drive the app interactively:

```powershell
. .\tools\Control-SoundSwitch.ps1
Start-SoundSwitchApp
Invoke-SoundSwitchCli -CliExe <cli.exe> -Command 'mute'
Open-SoundSwitchSettings -CliExe <cli.exe> -ProcessId (Get-Process SoundSwitch).Id
Set-SoundSwitchMuteTarget -WindowHandle $h -Target 'Windows default input'
Save-SoundSwitchScreenshot -Handle $h -Path out.png
Stop-SoundSwitchApp
```

Notes:

- The app allows a single instance (one named pipe, one config file). Close a
  user-started instance or pass `-AllowExistingInstance`.
- `-Start` records the PID under `%TEMP%\SoundSwitchControl\instance.pid`; `-Stop`
  only stops that instance unless `-Force`.
- The mute target combo box is `muteTargetComboBox`; `-MuteTarget` accepts the
  visible label or a zero-based index and selects the Recording tab automatically.
- Settings is opened through the CLI (`settings`), never by simulating tray input.

## Layer 3 — end-to-end test

```powershell
pwsh tools\Test-MuteTargetEndToEnd.ps1
pwsh tools\Test-MuteTargetEndToEnd.ps1 -TargetName "Line In (High Definition Audio Device)"
pwsh tools\Test-MuteTargetEndToEnd.ps1 -NoBuild -KeepRunning
pwsh tools\Test-MuteTargetEndToEnd.ps1 -SkipUnavailableTargetTest
```

It backs up `%APPDATA%\SoundSwitch\SoundSwitchConfiguration.json`, drives the real
UI and CLI, and asserts:

1. A microphone chosen in Settings is persisted as a full `DeviceInfo` snapshot,
   with no legacy `MuteRecordingDeviceId` field.
2. `mute` reports the selected target, and `mute -t` flips the state on it.
3. Choosing "Windows default input" clears the stored target and the CLI reports
   the Windows default input again.
4. A target that matches no connected device (simulated with a ghost snapshot)
   falls back to the Windows default input **and the selection is kept** so it
   resumes when the device returns.

The original configuration is always restored, and the app it launched is stopped,
even when a check fails.

This script is the reference implementation for the `end-to-end-testing` skill; load
that skill when writing a similar test for another feature.

## Before pushing

- Any change under `SoundSwitch/Model/AppModel.DeviceService.cs`,
  `AppRuleDeviceResolver`, `MicrophoneMuteToggler`, or mute configuration:
  run `pwsh tools\Test-MuteTarget.ps1` and `pwsh tools\Test-MuteTargetEndToEnd.ps1`.
- Banner rendering/positioning changes: the unit layer covers them; extend the
  relevant fixture rather than relying on a manual check.
- A user-visible setting was added/renamed/removed: also load the
  `documentation-sync` skill — docs and screenshots change in the same PR.
