# Issue #2009 — Tray icon shows stale default device after waking from sleep

## Problem

When the PC resumes from sleep, the tray icon (and its default-device state) can
show a device that is **not** the one actually producing audio. Sound still plays
from the correct device, but the icon is wrong until the user switches devices or
restarts SoundSwitch.

Reported on Windows 11 with two playback devices (e.g. an external "Audient 1/2"
vs a built-in "Headphone"). After wake, the icon shows the built-in even though
the external is the active default and audio comes from it.

## Root cause

The tray icon is driven by an in-process cache, not by a live query:

1. `MMNotificationClient` keeps `_lastRoleDevice` — a `Dictionary<(DataFlow, Role), deviceId>`
   of what it *believes* is the current default per role.
2. When Windows reports a default-device change, `OnDefaultDeviceChanged` compares
   the new id against `_lastRoleDevice` and, only if different, enqueues a
   `DefaultDeviceChangedEvent` (see `MMNotificationClient.OnDefaultDeviceChanged`).
3. The recurring `ProcessNotificationEventsJob` drains that queue and calls
   `CachedAudioDeviceLister.ProcessDeviceUpdates`, which raises
   `_defaultDeviceChanged` for `DefaultChanged` events.
4. `AppModel.DefaultDeviceChanged` → `TrayIcon` calls `IconChanger.ChangeIcon`,
   updating the visible icon.

On resume from sleep, Windows may transiently flip / re-assert the default device.
If the OS ends up with the *same* default it had before sleep (i.e. no real change
from the OS's point of view), **no `DefaultDeviceChanged` event is fired**. But our
cache/`_lastRoleDevice` and the tray icon may have been left pointing at a stale
value during the resume transition. Result: icon and reality diverge, and nothing
re-syncs them because the OS thinks nothing changed.

Additionally, the device list itself is cached (`CachedAudioDeviceLister`) and is
only re-enumerated when a `DeviceChanged` event is processed. Devices that
re-appear after sleep are not guaranteed to be re-read unless something triggers a
refresh.

## Fix (the chosen approach)

On **resume from sleep**, proactively reconcile the cached default against the
**actual** OS default, and refresh the cached device list so any device that
re-appeared after sleep is visible. If the real default differs from what we
cached, emit a `DefaultDeviceChanged` event through the **existing** pipeline so
the icon (and everything subscribed to `AppModel.DefaultDeviceChanged`) refreshes
using the same code path as a genuine OS change.

Concretely:

1. **`WindowsAPIAdapter`** — in `SystemEventsOnPowerModeChanged`, after the existing
   `ReRegisterAllHotkeys`, also raise a new static event
   `public static event EventHandler SystemResumed;`. (Keep the existing hotkey
   re-registration; this is additive.) This mirrors the existing `SessionUnlocked`
   pattern.

2. **`MMNotificationClient`** — add a public method, e.g.
   `ReconcileDefaultDevices()`, that:
   - Iterates every `(DataFlow flow, Role role)` combination (Render + Capture ×
     Console + Multimedia + Communications), mirroring `MMNotificationClient.Register()`.
   - For each, reads the **real** current default via
     `AudioSwitcher.Instance.GetDefaultAudioEndpoint((EDataFlow)flow, (ERole)role)`.
   - If the returned device id differs from `_lastRoleDevice[(flow, role)]`,
     update `_lastRoleDevice` and enqueue a
     `DefaultDeviceChangedEvent(EventType.DefaultChanged, realDeviceId, role)` into
     `_deviceChangedEvents`, exactly like `OnDefaultDeviceChanged` does.
   - Skip null results and "no change" cases (same guard as today).

3. **Orchestration in `AppModel`** (or wherever the resume is wired) — on
   `WindowsAPIAdapter.SystemResumed`:
   - Call `AudioDeviceLister.Refresh()` first so the device cache is current and the
     device referenced by the enqueued event actually exists in
     `PlaybackDevices`/`RecordingDevices` (otherwise `ProcessDeviceUpdates` logs
     "Can't get device" and drops the icon update).
   - Then call `MMNotificationClient.Instance.ReconcileDefaultDevices()` so any
     divergence produces a `DefaultDeviceChanged` event that the recurring job
     picks up (within ~200 ms) and pushes through to the tray icon.

   Threading: `Refresh()` and `AudioSwitcher` access are COM-marshaled
   (`ComThread.Invoke`), so calling them off the UI thread is safe. Dispatching via
   `Task.Run`/the adapter thread is acceptable; do not block the UI thread. Match
   the existing "avoid race with hotkeys" intent from `SystemEventsOnPowerModeChanged`.

### Why this is correct
- Reuses the **exact** existing event pipeline — no new icon-update code, no new
  code path to diverge from real OS changes.
- Only emits a change event when the real default actually differs from the cache,
  so a no-op resume produces no spurious icon flicker.
- Refreshing the lister cache first ensures the enqueued device id resolves in
  `ProcessDeviceUpdates`.
- `eCommunications` is already skipped by `IconChangerAbstract.ChangeIcon`, so the
  icon continues to follow console/multimedia defaults as today.

## Files to change
- `SoundSwitch/Framework/WinApi/WindowsAPIAdapter.cs` — add `SystemResumed` event;
  raise it in `SystemEventsOnPowerModeChanged`.
- `SoundSwitch/Framework/NotificationManager/MMNotificationClient.cs` — add
  `ReconcileDefaultDevices()`.
- `SoundSwitch/Model/AppModel.cs` (or the resume wiring) — subscribe to
  `SystemResumed` and call `AudioDeviceLister.Refresh()` + `MMNotificationClient.Instance.ReconcileDefaultDevices()`.

## Test / validation notes
- This is Windows-only behavior (COM audio). Linux build cannot validate the
  runtime; rely on Windows CI (`build` / `build` job) for compilation + the
  existing `SoundSwitch.Tests`. Add/extend a unit test if a seam exists to inject a
  fake default (mirror the `AudioSwitcher.SetExtendedPolicyClientForTest` pattern
  if available), otherwise rely on manual verification on Windows.
- Confirm no double-refresh storms: `Refresh()` cancels any in-flight refresh, and
  `ReconcileDefaultDevices` only enqueues on real divergence.

## Out of scope
- The default-device tooltip text already refreshes on hover (`TooltipInfoManager`);
  the persistent wrong visual is the icon, which this fix addresses. Tooltip is
  correct on next hover, so no change needed there.
- Hotkey re-registration on resume is already handled and is left as-is.
