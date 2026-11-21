# Profile volume settings

## Status

Proposed — mockup phase. Owner directive (2026-11-21): add per-profile volume
settings for **both recording and playback**, exposed as an **advanced**
feature in the profile form. Implementation is gated on mockup review and the
open questions below.

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
   public sealed class ProfileVolumeSetting // 0 < volume <= 100
   {
       public bool Enabled { get; set; }
       public float Volume { get; set; }
   }
   ```
   `Profile.PlaybackVolume` and `Profile.Recording` slot equivalents:
   `PlaybackVolume` applies to the profile's render devices, `RecordingVolume`
   to its capture devices. `null` (absent in JSON) = feature off — existing
   configs deserialize unchanged, no migration needed (Newtonsoft ignores
   missing members on old files and adds them on save).
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
   the slider binding trivial.

## Open questions

- **Q1 — re-apply semantics.** Force the preset on *every* activation (the
  reporter's ask: re-switching to the profile resets 90 → 50), or only when
  the default device actually changed? Forcing on every activation also
  stomps a manual tweak made while the profile is already active (hotkey
  re-press, window re-focus). Lean: force every activation — that is
  literally the requested behavior; the "stomp" is the feature.
- **Q2 — mute in the preset?** Include a mute toggle per flow, or
  volume-only for the first cut? Lean: volume-only — mute has interplay with
  the mic-mute banner feature and is easy to add later without schema break
  (adding a field is backward compatible).
- **Q3 — restore on deactivate.** When a window/process trigger deactivates
  and `RestoreState` puts back the pre-switch defaults, should it also
  restore the volumes the profile overrode? Requires `SaveCurrentState` to
  snapshot `MasterVolumeLevelScalar` alongside the device list. Lean: yes —
  otherwise a volume-applying trigger profile leaves the previous devices at
  the profile's volume permanently.

## Consequences

- Profile activation becomes a device switch *plus* optional volume
  normalization; both recording and playback are covered per the owner
  directive.
- `Profile.Copy()` must deep-copy the new settings; `UpsertProfileExtended`
  binds them; banner/notification text is unchanged (volume is silent
  best-effort — see non-goals).
- Telemetry: extend `TrackProfileActivated` context with whether volume
  presets applied (counts usage for the release notes).

## Non-goals

- Per-app volume (session-level mixing) — only endpoint master volume.
- Fading/ramping to the target volume.
- Notification copy changes for volume.
- Touching `KeepVolumeEnabled` behavior (tray cycling stays as-is).

## Mockup

Built before implementation (owner directive). UI-only branch:
`UpsertProfileExtended` gains the collapsed advanced section, new model
fields serialize but `ProfileManager.SwitchAudio` is untouched. Real WinForms
renders captured from the VM for owner review; Q1–Q3 answered against the
mockup, then folded in here before the implementation brief.
