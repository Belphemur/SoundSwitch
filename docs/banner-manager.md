# Microphone Mute Banner Manager

The `MicrophoneMuteBannerManager` is a UI-thread manager responsible for displaying persistent banner notifications when microphones are muted, and for dismissing them when microphones are unmuted.

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
