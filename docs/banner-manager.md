# Microphone Mute Banner Manager

The `MicrophoneMuteBannerManager` is a UI-thread manager responsible for displaying
persistent state banners for microphones. Either muted or unmuted can be persistent,
independently of the global Normal/Compact style. See the
[style and lifetime ADR](design/ui/2026-11-10-banner-style-and-lifetime.md).

## Exclusive Fullscreen Detection

`SoundSwitch/Framework/Banner/ExclusiveFullscreenDetector.cs`

The `ExclusiveFullscreenDetector` determines whether the foreground window is in **true exclusive fullscreen** (FSE) mode — where DWM composition is suspended and no Win32 overlay window can appear. When detected, notifications are routed to Windows Toast instead of the banner overlay.

### Key Insight

In true exclusive fullscreen, the application takes exclusive ownership of the display output via DXGI. DWM is suspended for that monitor, so no Win32 window can be rendered on screen. Toast notifications are the only option because they are handled by the OS notification layer independently of DWM.

Modern games (e.g. Counter-Strike 2) use **borderless fullscreen** with the DXGI flip model, which provides equivalent performance without suspending DWM. In this mode, `BannerForm` is safe to show because it uses `WS_EX_NOACTIVATE` + `ShowWithoutActivation`, which does not trigger `WM_ACTIVATEAPP` in the game.

### Detection Strategy (Layered)

1. **SHQueryUserNotificationState** (primary signal) — The only Windows API that explicitly says "a D3D app is running in exclusive fullscreen" (`QUNS_RUNNING_D3D_FULL_SCREEN`). It is global (not per-window), but it is the strongest available signal.
2. **Display mode change** (secondary signal, gated) — Compares `ENUM_CURRENT_SETTINGS` vs `ENUM_REGISTRY_SETTINGS`. Only fires if the foreground window also covers the monitor, to prevent false positives from unrelated display mode changes on the same monitor.
3. **Shell exclusion** — Windows shell windows (explorer.exe, ShellExperienceHost.exe) are excluded to prevent false positives from the desktop/taskbar.

### What Is NOT Treated as FSE

- Borderless fullscreen games (QUNS will not report D3D FSE, no display mode change) → banner shown normally
- Desktop shell windows (explorer.exe) → banner shown normally
- Any windowed application → banner shown normally
- Always-on-top borderless windows → banner shown normally (WS_EX_TOPMOST alone is NOT used as a FSE signal)

## Location

`SoundSwitch/Framework/Banner/MicrophoneMute/MicrophoneMuteBannerManager.cs`

## Threading Model

- The manager **must be initialized on the UI thread** via `Setup()`.
- It captures the current `SynchronizationContext` and validates that it is a `WindowsFormsSynchronizationContext`.
- All banner updates are dispatched through `_syncContext.Send(...)` to ensure they run on the UI thread.

## Core Behaviors

### Either State — Persistent Banner

When the current microphone state's notification mode is Persistent:

- A `BannerForm` is created (or updated) for that microphone.
- The banner uses `TimeSpan.MaxValue` as its TTL, making it effectively **infinite** until explicitly dismissed.
- Normal shows a status message containing the microphone name. Compact uses a short state title and separate microphone name. Style is independent of lifetime.
- Clicking the banner toggles that microphone through `AppModel.Instance.SetMicrophoneMuteState(microphoneId, !isMuted)`.
- Both persistent states use the same tracked form, updated in place when the state changes.

### Fading and None

`NotificationBanner.NotifyMicrophoneMuteChanged` selects the mode for the new state:

- Fading removes any persistent state banner and uses `BannerManager` with the configured on-screen time, for either On or Off. Clicking a fading banner dismisses it.
- None removes any persistent state banner without showing another notification.
- Reconciliation after mode, target, scope, notification-type, or device-list changes retains only in-scope microphones whose current state is Persistent. It does not replay fading notifications.

### Appearance

`BannerForm` reads global style, color, and scale settings independently of TTL.
Changing style refreshes visible windows without restarting timers; managers
recompute stacked offsets when window sizes change. Persistent banners also honor
the configured opacity and display-info selection.

`BannerForm.ApplyPresentation` selects `BannerData.CompactContent` when available.
All compact banners share the microphone layout: a title-row icon in a scale-adjusted
24×24 box, with the description below. Source image resolution does not affect sizing.

### Multiple Microphones

- The manager supports **stacking multiple banners vertically**.
- Each microphone is tracked independently by its unique `microphoneId` in the `_activeBanners` dictionary.
- `RearrangeBanners()` recalculates the vertical offset for each active banner, spacing them by a constant `SPACING` value.

### Manual Removal

You can call `RemovePersistentMuteBanner(microphoneId)` to immediately remove a specific microphone's persistent banner and rearrange the remaining ones.

## Key Design Points

| Aspect | Detail |
|--------|--------|
| Thread safety | UI-thread only; all operations marshaled via `SynchronizationContext` |
| Focus safety | Banners are non-activating and guarded against focus/activation (e.g., `WS_EX_NOACTIVATE`, `WM_WINDOWPOSCHANGING` guard, `WM_MOUSEACTIVATE` / `WM_ACTIVATE` handling) |
| Persistence | On and Off both use `TimeSpan.MaxValue` when their mode is Persistent |
| Auto-dismiss | Either state in Fading uses configured on-screen time |
| Stacking | Banners arranged vertically with fixed spacing |
| Interactivity | Clicking a persistent state banner toggles that microphone; clicking a fading banner dismisses it |

## Usage Example

```csharp
// Must be called once on the UI thread
MicrophoneMuteBannerManager.Setup();

// Create an instance of the manager
var manager = new MicrophoneMuteBannerManager();

// Update mute state for a specific microphone
manager.UpdateMicrophoneMuteState("mic-id-123", "USB Microphone", isMuted: true);
```
