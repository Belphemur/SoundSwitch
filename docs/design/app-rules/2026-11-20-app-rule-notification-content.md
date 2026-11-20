# Name the app and devices in App Rule notifications

**Status:** Proposed
**Author:** Antoine Aflalo (Belphemur)
**Base:** `origin/dev`
**Issue:** [SoundSwitch#2615](https://github.com/Belphemur/SoundSwitch/issues/2615) - Improve Notification Details for App Rules
**Related:** `docs/design/app-rules/2026-10-22-app-rule-modes.md`, `docs/design/ui/2026-11-10-banner-style-and-lifetime.md`

---

## 1. Problem

When an App Rule triggers, the notification shows the app icon and the generic
title "App Rules" (`SettingsStrings.appSoundLock_tab`) with, at best, a bare
device-name list. Two separate defects:

1. **Empty text (the reported symptom).** `NotificationManager.NotifyAppRuleMatched`
   (`SoundSwitch/Framework/NotificationManager/NotificationManager.cs:170`)
   resolves the pair's devices through
   `AppRuleDeviceResolver.Resolve(pair.Playback, deviceService)`, i.e. the
   user's *Selected switcher devices* (`AppModel.DeviceService.AvailablePlaybackDevices`
   intersects with `SelectedDevices`). The switch itself
   (`AppSoundLockManager.SwitchRuleDevice`, `SoundSwitch/Services/AppSoundLockManager.cs:749`)
   and the rule editor (`SoundSwitch/UI/Forms/Settings.cs:2162`) use **all
   active devices** via `AudioDeviceLister`. A rule that targets a device
   outside the switcher selection — e.g. a "silent device" used only through
   App Rules, exactly the reporter's use case — switches fine, but the
   notification-side resolve returns `null`, so
   `NotificationContentBuilder.BuildAppRuleMatched` joins an empty list and the
   toast renders title-only. The user learns nothing.
2. **Contentless wording even when it works.** The text is a bare
   `\n`-joined `NameClean` list: no app name, no role labels. With a
   playback+recording pair you cannot tell which name is which, and the title
   says nothing about which application was switched.

## 2. Goals

1. The notification always names the switched application.
2. The notification names the device for each role (playback, recording) that
   was actually written, with a role label.
3. Device text is never empty: when resolution fails, the stored device name
   from the rule is shown.
4. Keep the existing channels untouched: banner (Normal/Compact), Windows
   toast, sound-only, none.

## 3. Non-goals

- No new settings, no schema changes, no new notification modes.
- No changes to which devices the switch or the rule editor offer.
- No changes to the no-process error notification
  (`appSoundLock.rule.noProcess`).

## 4. UX

All App Rule trigger notifications (process start, foreground switch, hotkey
cycle, assignment enforcement) use one shape, mirroring the Profile
notification's structure:

| Surface | Title | Text |
|---|---|---|
| Toast / balloon / Normal banner | `Spotify switched` (localized: "Switched {0}") | `Playback → Speakers (Realtek HD Audio)` newline `Recording → CABLE Output` — only the roles actually written |
| Compact banner | `Spotify switched` | `Speakers (Realtek) · CABLE Output` (name-only, middot-joined) |

Mockup: `app-rule-notification-content.png` (cards A–E: current broken output,
proposed toast, alternative rejected, banner Normal/Compact).

The app name is the process name known at trigger time (`processName`, already
passed into `NotifyAppRuleMatched`'s caller `ApplyRuleToProcess`); the title
format string carries it, exactly like `profile_notification_text` carries the
profile name.

## 5. Investigation

- The switch path resolves against `AudioDeviceLister` and self-heals the
  stored `DeviceInfo` (id/name drift after driver updates) — the notification
  must resolve from the same source or it can disagree with what actually
  happened.
- `ApplyRuleToProcess` knows exactly which flows were written
  (`flowsWritten`: eRender and/or eCapture) and only notifies when
  `changed == true`; passing those flows lets the text include only the roles
  that moved.
- `AppSoundRuleDevicePair` stores the last-known `DeviceInfo` for each role
  (with `NameClean`), usable as the never-empty fallback.
- No Windows API constraints: this is pure content composition in
  `NotificationContentBuilder` + one plumbing change in `NotificationManager`.

## 6. Design

### 6.1 Behaviour

`AppSoundLockManager.ApplyRuleToProcess` passes the rule's pair plus the
resolved devices from the same `AppRuleDeviceResolver.Resolve(stored,
AppModel.Instance.AudioDeviceLister)` call already used by `SwitchRuleDevice`,
plus `flowsWritten`, plus `processName`, to `NotificationManager.NotifyAppRuleMatched`.
To avoid a second resolution pass, `SwitchRuleDevice`'s resolve results are
returned to its caller (out-parameter or small record) and reused.

`NotificationManager.NotifyAppRuleMatched(rule, pair, processName, resolvedPlayback,
resolvedRecording, flowsWritten, processId)`:

- builds the icon as today (`GetIconForRule`);
- calls `NotificationContentBuilder.BuildAppRuleMatched(processName, pair, playback,
  recording, flowsWritten)` with already-resolved devices (no `deviceService`
  lookup anymore — this removes the empty-text bug class entirely);
- forwards to all four `INotification` implementations as today.

`NotificationContentBuilder.BuildAppRuleMatched`:

- Title: `string.Format(SettingsStrings.appSoundLock.rule.switchedApp, processName)`.
- Text (Normal/toast): one line per flow in `flowsWritten`:
  `"{role} → {deviceName}"` where `{role}` is `SettingsStrings.playback` /
  `SettingsStrings.recording` and `{deviceName}` is the resolved device's
  `NameClean`, or — when resolution returned null — the pair's stored
  `pair.Playback.NameClean` / `pair.Recording.NameClean`.
- Compact banner content (`CompactContent`, as the microphone banner does):
  title same; text is the role device names joined with `" · "`.
- Priority stays 1; image stays the process icon.

`INotification.NotifyAppRuleMatched` signature change is internal to the app
project; the four implementations (`NotificationBanner`, `NotificationWindows`,
`NotificationSound`, `NotificationNone`) update mechanically.

The hotkey path (`ApplyRuleToRunningProcesses`) notifies once per press, after
the match loop, and reports the pair rather than any single match: the process
identity is `matches[0]` as before, and the flow list is the pair's roles that
resolve against the active devices. An already-applied press therefore still
names the pair it selected, and the flow list is never empty for a press —
`CanApplyPair` rejects a pair with no resolvable side before the loop.
Per-match `flowsWritten` is deliberately not used there: matches can write
different flow sets, and the banner answers "which pair is now active", not
"what did one process move". A role whose device is unavailable is omitted,
mirroring the switch that skips it, so the banner never claims a side it did
not apply; each reported role names the resolved device's `NameClean`, with the
pair's stored `NameClean` remaining the builder's fallback for a null resolved
device. The automatic path keeps reporting only the flows actually written.

### 6.2 Migration

No schema change — no migration. (String format only; the old
`appSoundLock_tab` title usage is dropped from notifications, key stays for
the tab itself.)

### 6.3 Data model

No new persisted types. Runtime-only signature change:

```csharp
void NotifyAppRuleMatched(AppSoundRule rule,
    AppSoundRuleDevicePair pair,
    string processName,
    DeviceFullInfo playback,        // resolved against AudioDeviceLister, may be null
    DeviceFullInfo recording,       // resolved against AudioDeviceLister, may be null
    IReadOnlyCollection<EDataFlow> flowsWritten,
    uint processId);
```

## 7. Telemetry

No new events; uses the existing `soundswitch.apprule.activated` counter
(trigger source + hashed process basename) emitted per rule activation.

## 8. Localization

| Key | English value |
|---|---|
| `appSoundLock.rule.switchedApp` | `Switched {0}` |

Role labels reuse existing keys `playback` / `recording`; the arrow separator
is literal (same convention as the profile notification's device list).

## 9. Files touched

| File | Change |
|---|---|
| `SoundSwitch/Framework/NotificationManager/Notification/INotification.cs` | `NotifyAppRuleMatched` signature |
| `SoundSwitch/Framework/NotificationManager/Notification/NotificationContentBuilder.cs` | `BuildAppRuleMatched(processName, pair, playback, recording, flowsWritten)`; compact content |
| `SoundSwitch/Framework/NotificationManager/NotificationManager.cs` | resolve-free forwarding of already-resolved devices |
| `SoundSwitch/Framework/NotificationManager/Notification/NotificationBanner.cs` | mechanical signature update |
| `SoundSwitch/Framework/NotificationManager/Notification/NotificationWindows.cs` | mechanical signature update |
| `SoundSwitch/Framework/NotificationManager/Notification/NotificationSound.cs` | mechanical signature update |
| `SoundSwitch/Framework/NotificationManager/Notification/NotificationNone.cs` | mechanical signature update |
| `SoundSwitch/Services/AppSoundLockManager.cs` | `SwitchRuleDevice` returns resolved devices; `ApplyRuleToProcess` passes them + `processName` |
| `SoundSwitch/Localization/SettingsStrings.resx` (+ Designer) | new key `appSoundLock.rule.switchedApp` |
| `SoundSwitch.Tests/NotificationContentBuilderTests.cs` | updated + new cases (fallback name, role labels, flowsWritten filtering) |
| `website/src/usage/app-rules/automatic.md` | mention notifications name app + devices |

## 10. Test plan

### Automated

`SoundSwitch.Tests` — `NotificationContentBuilderTests`:

1. Title contains the process name; not the "App Rules" tab label.
2. Text contains role-labeled lines for each flow in `flowsWritten`.
3. Playback-only rule → text has one `Playback → …` line, no recording line.
4. Unresolvable device (null `DeviceFullInfo`, stored name in pair) → text
   shows the stored `NameClean`, never empty.
5. Compact content joins names with `" · "`.

### Manual

1. `dotnet build SoundSwitch.sln -c Debug`.
2. Rule targeting a device that is NOT in the switcher selection (the
   reporter's silent-device case): trigger → notification shows `Switched
   <app>` + `Playback → <device>` (previously empty).
3. Playback+recording pair: both lines appear, correctly labeled.
4. Hotkey cycle and process-start triggers: same shape.
5. Banner (Normal and Compact), Windows toast, sound-only, none: all channels
   behave; sound-only ignores text as before.

## 11. Decisions

| # | Decision | Rationale |
|---|---|---|
| 1 | Notification devices resolve from `AudioDeviceLister` (same source as the switch), passed in already-resolved | Removes the empty-text bug class; notification can never disagree with the actual switch. Rejected: keep `deviceService` resolve + fallback patching — leaves two sources of truth that can drift again. |
| 2 | Title names the app (`Switched {0}`), profile-parity shape | The user wants to know *what changed for which app*; Profile notifications already use this shape (`profile_notification_text`). Rejected: keep "App Rules" as title with app in text — the title is the strongest signal and "App Rules" carries no event information. |
| 3 | On resolve failure show the stored `NameClean` from the pair | The stored name is meaningful (user picked it); never-empty guarantees the notification is informative. Rejected: "(unavailable)" placeholder — noise; omitting the line hides that a role was written. |
| 4 | Only roles actually written (`flowsWritten`) get lines | Mirrors what happened; a playback-only rule shouldn't advertise a recording device. |
| 5 | One shared `BuildAppRuleMatched` across channels; Compact banner gets `CompactContent` like the microphone banner | Follows the banner-style ADR (2026-11-10) convention; no per-channel content logic. |

## 12. Future work

- Localized role labels in non-English resources via Weblate (source resx only
  here).
- Consider the same "name the target" treatment for the assignment-enforcement
  silent path if users report wanting visibility there (today deliberately
  silent per 2026-10-22 ADR).
