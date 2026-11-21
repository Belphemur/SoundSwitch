# Profile volume settings

## Status

Accepted (2026-11-21) — owner directive: mockup delivered, open the PR, start
the implementation. Q1–Q3 are resolved below with **owner-vetoable defaults**
chosen under that directive (each marked as such); vetoing one reopens only
that decision, not the feature.

## Context

Issue #2547 (Enhancement): users want each profile to carry a preferred volume.
The follow-up comment refines it: not "copy volume across devices" but
"reset to a preferred default per profile" — switched to Work Headset
(preferred 50), manually raised to 90 for a session, then on re-activation the
profile should return to 50 rather than Windows' last-remembered 90.

What exists today:

- `Profile` (`SoundSwitch/Framework/Profile/Profile.cs`) holds four device
  slots (`Playback`, `Communication`, `Recording`, `RecordingCommunication`),
  plus behavior flags (`AlsoSwitchDefaultDevice`, `SwitchForegroundApp`,
  `NotifyOnActivation`, `RestoreDevices`). No volume data.
- `ProfileManager.SwitchAudio` (lines 405/450) is the single choke point for
  every activation path (hotkey, process, window, UWP, Steam, startup, forced,
  tray menu). It resolves each device via `CheckDeviceAvailable`, then
  switches roles. Volume is never touched.
- The audio layer already exposes per-device volume:
  `AudioDevice.EndpointVolume` → `AudioEndpointVolumeClient` with
  `MasterVolumeLevelScalar` (0–1 get/set) and `Mute`, COM-marshalled
  (`SoundSwitch.Audio.Manager/AudioEndpointVolumeClient.cs`).
- The existing "Keep volume level across devices" setting
  (`KeepVolumeEnabled`, consumed only in
  `ADeviceCycler.SetActiveDevice:94` → `SetVolumeFromDefaultDevice`) applies
  **only to tray cycling**, never to profile switches — the two mechanisms are
  orthogonal and do not conflict. Verified: `ProfileManager` never calls
  `SetVolumeFromDefaultDevice`.

Cost of not doing it: per-device volume curation needs external tools
(AHK/NirCMD), which the reporter explicitly calls a reason to stop using
SoundSwitch.

## Decision

1. **Model.** `Profile` gains two optional volume presets:
   ```csharp
   public sealed class ProfileVolumeSetting // 0 <= volume <= 100
   {
       public bool Enabled { get; set; }
       public float Volume { get; set; }
   }
   ```
   `Profile.PlaybackVolume` applies to the profile's render devices,
   `Profile.RecordingVolume` to its capture devices. `null` (absent in JSON) =
   feature off — existing configs deserialize unchanged, no migration needed
   (Newtonsoft ignores missing members on old files and adds them on save).
2. **Granularity: per flow, not per slot.** One playback preset, one
   recording preset — per the owner's framing ("volume settings for both
   recordings and playback"). Per-slot (4 independent sliders) was rejected
   for the first cut: `Playback` and `Communication` usually name the same
   physical endpoint, so 4 sliders mostly duplicate one value and clutter an
   already dense form.
3. **Advanced placement.** The controls live in a collapsed-by-default
   "advanced" disclosure in `UpsertProfileExtended`, so the default form
   remains unchanged for existing users. Checkbox "Set volume" + slider
   (0–100) per flow, matching the issue's requested shape.
4. **Application point.** In `SwitchAudio`, after `CheckDeviceAvailable`
   resolves a device: get the `AudioDevice`, set
   `EndpointVolume.MasterVolumeLevelScalar = preset.Volume / 100f`.
   Failures are logged (Serilog) and skipped — a volume failure must not
   fail the profile switch or raise `_showError` (the switch itself
   succeeded; volume is best-effort per device).
5. **Storage of volume: raw percent.** Store 0–100 as user-facing; convert
   to scalar only at the COM boundary. Keeps the config human-readable and
   the slider binding trivial. `0` is a legal preset (intentionally silent).

## Resolved questions

- **Q1 — re-apply semantics: force on EVERY activation** (both `SwitchAudio`
  overloads, every trigger type). Re-pressing the hotkey or re-focusing the
  window re-applies the preset, stomping a manual tweak made while the
  profile is active — that stomp IS the reporter's ask ("reset to 50 instead
  of last-saved 90"). *Owner-vetoable default.*
- **Q2 — volume-only for the first cut; no mute in the preset.** Mute has
  interplay with the mic-mute banner feature and can be added later without
  schema break (adding a field is backward compatible). The slider's full
  0–100 range is stored; 0 means "preset to silent", not mute.
  *Owner-vetoable default.*
- **Q3 — restore volumes on trigger deactivation: yes.** `SaveCurrentState`
  snapshots `MasterVolumeLevelScalar` alongside each saved device;
  `RestoreState` re-applies those volumes when the pre-switch defaults are
  restored. Without this, a volume-applying trigger profile leaves the
  previous devices permanently at the profile's volume. *Owner-vetoable
  default.*

## Consequences

- Profile activation becomes a device switch *plus* optional volume
  normalization; both recording and playback are covered per the owner
  directive.
- `Profile.Copy()` deep-copies the new settings (done in the mockup phase);
  `UpsertProfileExtended` binds them (done); banner/notification text is
  unchanged (volume is silent best-effort — see non-goals).
- Telemetry: extend `TrackProfileActivated` context with whether volume
  presets applied (counts usage for the release notes).
- Deactivation via `RestoreState` now also restores volumes (Q3), so the
  window/process trigger round-trip returns the system to its true
  pre-trigger state.

## Non-goals

- Per-app volume (session-level mixing) — only endpoint master volume.
- Fading/ramping to the target volume.
- Notification copy changes for volume.
- Touching `KeepVolumeEnabled` behavior (tray cycling stays as-is).

## Mockup (delivered)

UI-only phase landed on `feature/profile-volume` before implementation
(owner rule: mockup first): model + serialization tests (`c5cc67c8`),
collapsed advanced section in `UpsertProfileExtended` (`f0bd7992`), capture
tooling (`fc134e33`). Real WinForms renders captured and verified:
`artifacts/mockup-default.png` (collapsed — form above the disclosure row
pixel-identical to pre-change baseline) and `artifacts/mockup-expanded.png`
(playback checked at 35%, recording checked at 80%). One deviation from the
ADR sketch: the stored range is the full 0–100 (`0 <= volume <= 100`),
not `0 < volume <= 100` — folded into Decision 5 above.
