# Microphone Mute Banner Manager

The `MicrophoneMuteBannerManager` is a UI-thread manager responsible for displaying persistent banner notifications when microphones are muted, and for dismissing them when microphones are unmuted.

## Exclusive Fullscreen Detection

`SoundSwitch/Framework/Banner/ExclusiveFullscreenDetector.cs`

The `ExclusiveFullscreenDetector` determines whether the foreground window is in a fullscreen state that would be disrupted by a Win32 overlay. When detected, notifications are routed to Windows Toast instead of the banner overlay.

### Detection Strategy

The detector uses a multi-signal approach:

1. **Monitor coverage** — The foreground window must cover at least the entire monitor bounds.
2. **Borderless style** — The window must use WS_POPUP without WS_CAPTION, or lack both WS_CAPTION and WS_THICKFRAME.
3. **Shell exclusion** — The window must not belong to explorer.exe (prevents false positives from desktop/taskbar).
4. **WS_EX_TOPMOST** (strong signal) — If present, immediately confirms FSE.
5. **Display mode change** — Compares current display settings to the desktop registry default. A resolution or refresh rate mismatch strongly indicates FSE.
6. **DWM cloaked check** — Windows hidden by DWM (cloaked) are excluded.
7. **Fallback** — A fullscreen borderless window from a non-shell process is treated as potentially FSE. This favors safety (toast notification) over precision.

### Design Philosophy

The detector intentionally errs on the side of caution: it is acceptable to show a toast notification for a borderless-windowed game (minor UX difference) but unacceptable to show a banner overlay that causes an exclusive fullscreen game to minimize.

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
