# Microphone Mute Banner Manager

The `MicrophoneMuteBannerManager` is a UI-thread manager responsible for displaying persistent banner notifications when microphones are muted, and for dismissing them when microphones are unmuted.

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

### Muted State — Persistent Banner

When a microphone is muted (`isMuted = true`):

- A `BannerForm` is created (or updated) for that microphone.
- The banner uses `TimeSpan.MaxValue` as its TTL, making it effectively **infinite** until explicitly dismissed.
- The banner displays the microphone name, a mute icon, and a localized "microphone off" title.
- Clicking the banner toggles the microphone back to unmuted by calling `AppModel.Instance.SetMicrophoneMuteState(microphoneId, false)`.

### Unmuted State — Temporary Notification

When a microphone is unmuted (`isMuted = false`):

- The persistent banner is replaced with a temporary notification.
- This temporary banner has a **TTL of 1.5 seconds** (`TimeSpan.FromMilliseconds(1500)`).
- It shows a "microphone on" icon and title.
- Clicking it toggles the microphone back to muted.
- Once the TTL expires, the banner auto-disposes.

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
| Persistence | Mute banners live forever (`TimeSpan.MaxValue`) |
| Auto-dismiss | Unmute banners auto-dismiss after 1.5s |
| Stacking | Banners arranged vertically with fixed spacing |
| Interactivity | Clicking any banner toggles the microphone's mute state |

## Usage Example

```csharp
// Must be called once on the UI thread
MicrophoneMuteBannerManager.Setup();

// Create an instance of the manager
var manager = new MicrophoneMuteBannerManager();

// Update mute state for a specific microphone
manager.UpdateMicrophoneMuteState("mic-id-123", "USB Microphone", isMuted: true);
```
