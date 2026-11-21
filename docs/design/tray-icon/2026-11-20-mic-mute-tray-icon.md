# Tray icon type: microphone mute state

**Status:** Proposed
**Author:** Hermes
**Base:** `origin/dev` @ `45e1a0c6`
**Issue:** [Belphemur/SoundSwitch#2620](https://github.com/Belphemur/SoundSwitch/issues/2620) — tray icon showing the mute status of the default communication microphone
**Related:** `docs/design/tray-icon/2026-09-02-theme-icon-form-factor-and-visibility.md` (Lucide asset pipeline, `ThemeIcons`, `IconChangerThemeBased`)

---

## 1. Problem

A user cannot make the SoundSwitch tray icon show whether the microphone is muted. The five existing icon types (`SoundSwitch/Framework/TrayIcon/IconChanger/IconChanger.cs`) — None / Recording / Playback / Always / ThemeBased — all key off the *default device* (playback or recording), never off a device's *state*. The mute state is exactly what the user cares about in calls ("this matters much more to me than my mic/headset statuses"), and today it is only surfaced through banners/toasts (`MicrophoneMuteBannerManager`) which disappear or must be consciously enabled.

Windows itself keeps no tray affordance for mic mute; the sound mixer flyout requires a click-through. A passive, always-visible indicator in the SoundSwitch icon fills that gap.

## 2. Goals

1. New icon type **"Microphone Mute State"**: the tray icon shows a microphone glyph — muted (struck-through) or live — matching the mute state of the **default communication microphone** (`eCapture` + `eCommunications`, same role resolution the mute toggler uses).
2. The icon updates live on any mute change of that microphone (own hotkey, SoundSwitch banner click, Windows settings/sndvol, or the physical device's hardware mute button), including the initial state at startup and theme switches.
3. Theme-aware: black glyph on light taskbars, white glyph on dark taskbars, same as `IconChangerThemeBased`.
4. The state must never go stale: mute events for other microphones or non-mute volume changes must not repaint the icon with the wrong state.

## 3. Non-goals

- **No changes to the five existing icon types.** They keep their current semantics (`IconChangerNone/Playback/Recording/Always/ThemeBased` untouched).
- **No config schema change.** `SwitchIcon` is a plain enum persisted as int in `SoundSwitchConfiguration`; adding enum member `MuteState = 5` needs no migration (old configs deserialize to their existing values).
- **No banner/notification changes.** The banner system keeps working exactly as before, independent of the icon type.
- **No per-device mute icon.** Only the default communication microphone is tracked (the device the mute toggler acts on when none is configured). Individual device mute states stay a banner concern.

## 4. UX

Settings → General tab → **Systray Icon** dropdown gains a sixth entry:

| Entry | Behaviour |
|---|---|
| **Microphone Mute State** | Tray icon shows `themeIcon_mic_muted[_white].ico` while the default communication microphone is muted, `themeIcon_microphone[_white].ico` while it is live. |

Tray icon menu, double-click actions, tooltips, banners: unchanged. Selecting the type applies immediately (the settings form already calls `ChangeIcon` on selection, `UI/Forms/Settings.cs:1477`).

## 5. Investigation

- Mute events already flow into the app: `AudioDeviceLister.DeviceVolumeChanged` → `AppModel.HandleMicrophoneMuteChanged` (`Model/AppModel.DeviceService.cs:119`) filters `payload.Device.Type == EDataFlow.eCapture && payload.MuteChanged`. The event carries `DeviceFullInfo Device`, `bool WasMuted`, `bool IsMuted` (`SoundSwitch.Common/Framework/Audio/Device/DeviceVolumeChangedPayload.cs`). The icon changer just needs the same stream.
- Which microphone is "the" microphone: `MicrophoneMuteToggler` resolves the configured mute device or falls back to `GetDefaultAudioDevice(eCapture, eCommunications)` (`Framework/Audio/Microphone/MicrophoneMuteToggler.cs:34`). The icon must key off the same resolution so icon and toggler never disagree.
- Initial state: `TrayIcon.UpdateIcon()` runs at startup; the new changer can read the current mute state directly via `AudioSwitcher.Instance` / `AppModel.GetMicrophoneMuteState()`.
- Icons: `ThemeIcons.GetIcon(IconKind, bool isDarkTaskbar)` (`Framework/TrayIcon/ThemeIcons.cs`) is the existing theme-aware cache; adding two resource entries and one `IconKind` extends it without touching its contract.
- Theme switches: `TrayIcon` subscribes `WindowsAPIAdapter.SystemThemeChanged` and re-invokes the parameterless `ChangeIcon` for `ThemeBased` and `MuteState`; the changer re-resolves `WindowsThemeHelper.IsDarkModeEnabled()` on every repaint, so the glyph flips colour immediately on a taskbar theme change.

## 6. Design

### 6.1 Data model

```csharp
// SoundSwitch/Framework/TrayIcon/IconChanger/IconChanger.cs
public enum IconChanger
{
    None = 0,
    Recording = 1,
    Playback = 2,
    Always = 3,
    ThemeBased = 4,
    MuteState = 5
}

// SoundSwitch/Framework/TrayIcon/IconKind.cs — new member
internal enum IconKind
{
    Speaker, Headphone, Headset, Microphone, MicrophoneMuted
}
```

`IconKind` is internal and `ThemeIcons` indexes by `(int)kind`, so `MicrophoneMuted` appends to the array — no re-order risk.

### 6.2 New changer

New `IconChangerMuteState : IIconChanger` in `Framework/TrayIcon/IconChanger/Changer/`:

```csharp
public IconChanger TypeEnum => IconChanger.MuteState;
public string Label => TrayIconStrings.iconChanger_muteState;

public void ChangeIcon(UI.Component.TrayIcon trayIcon)
{
    // initial state / generic refresh — reads current mute state of the
    // configured-or-default communication microphone
    var muted = ResolveCurrentMuteState();          // see 6.4
    using var handle = ThemeIcons.GetIcon(muted ? IconKind.MicrophoneMuted : IconKind.Microphone, IsDarkTaskbar()).Acquire();
    trayIcon.ReplaceIcon(handle.Icon);
}

public void ChangeIcon(UI.Component.TrayIcon trayIcon, DeviceFullInfo deviceInfo, ERole role)
{
    // Device-driven overload is a no-op for this type: the icon keys off mute
    // state, not the default device. (Keeping the interface's contract.)
}
public void ChangeIcon(UI.Component.TrayIcon trayIcon, bool isMuted)  // new dedicated path
{
    using var handle = ThemeIcons.GetIcon(isMuted ? IconKind.MicrophoneMuted : IconKind.Microphone, IsDarkTaskbar()).Acquire();
    trayIcon.ReplaceIcon(handle.Icon);
}
```

To avoid `IIconChanger` interface churn across all six implementations, `IconChangerMuteState` exposes the dedicated `ChangeIcon(trayIcon, bool isMuted)` overload as its own public method; the parameterless `ChangeIcon` resolves the live state and delegates to it, so it doubles as the changer's shared paint primitive.

Review follow-up (ordering): `DeviceFullInfo` raises each volume event inside `Task.Run`, so two rapid toggles can be handled out of order and an older payload could overwrite a newer paint. The mute-event path in `TrayIcon` therefore never paints the event's historical boolean: it queues the parameterless re-read onto the UI thread, and the last queued update always resolves the microphone's final state.

### 6.3 Event wiring

In `UI/Component/TrayIcon.SetEventHandlers()`, subscribe to the existing `AppModel` mute stream (new event on `IAppModel`):

```csharp
public interface IAppModel
{
    // existing members unchanged…

    /// <summary>Default communication microphone's mute state changed.</summary>
    event EventHandler<MicrophoneMuteChangedEventArgs> DefaultMicMuteChanged;
}

public class MicrophoneMuteChangedEventArgs : EventArgs
{
    public string DeviceId { get; init; }
    public string DeviceName { get; init; }
    public bool IsMuted { get; init; }
}
```

`AppModel.HandleMicrophoneMuteChanged` (which already receives every capture mute change) raises `DefaultMicMuteChanged` **only when the changed device is the resolved mute target** (configured device id, else default communication capture). This guarantees the icon repaints only on the tracked mic's transitions and never on other mics. The handler in `TrayIcon` checks `AppConfigs.Configuration.SwitchIcon == IconChanger.MuteState`, then queues a re-read of the live mute state onto the UI thread (see the ordering note in §6.2) rather than painting the event's boolean.

Mute-target changes: selecting a different configured mute microphone, or a device arrival/removal that re-resolves the stored target, changes *which* microphone the icon must follow without any mute transition firing. `IAppModel` therefore also exposes `MuteTargetChanged` (plain `EventArgs`), raised from the `MuteRecordingDevice` setter and from the `DeviceListRefreshed` subscription; when `SwitchIcon == MuteState`, `TrayIcon` re-invokes the parameterless changer so the icon reflects the newly tracked microphone's state.

Startup correctness: `TrayIcon.UpdateIcon()` (parameterless `ChangeIcon`) already runs at init — it reads the live mute state so the icon is right before any event arrives.

Default-device-changed subscription: when the *default communication microphone changes* (device swap), the new default's mute state may differ from the old one's. The existing `DefaultDeviceChanged` handler calls the **device-driven** overload (`ChangeIcon(trayIcon, Device, Role)`) for all types, which is a no-op for `MuteState`. The handler therefore branches: for `MuteState`, a **capture + eCommunications** default change re-invokes the parameterless changer, which re-reads the live mute state and repaints. Other types take their previous path unchanged. (During implementation this replaced an earlier draft that assumed the handler already routed through the parameterless path — it does not.)

### 6.4 Mute state resolution

`IconChangerMuteState.ResolveCurrentMuteState()` calls `AppModel.Instance.GetMicrophoneMuteState()` — the model method that already resolves configured-then-default mic and reads `EndpointVolume.Mute` (`Model/AppModel.DeviceService.cs:270`). No new COM plumbing. Failure (no mic): keep the unmuted glyph and log; a mute event later will correct it.

### 6.5 Assets

Two new ICOs in `SoundSwitch.Common/Resources/` (black + white), rendered from Lucide `mic-off.svg` / `mic.svg` by the exact pipeline of the theme-icon ADR (4× oversample → Lanczos → 7-size PNG-in-ICO, 16–64 px):

| File | Source | Colour | Size |
|---|---|---|---|
| `themeIcon_mic_muted.ico`        | lucide `mic-off.svg` | black on transparent | ~7.0 KB |
| `themeIcon_mic_muted_white.ico`  | lucide `mic-off.svg` | white on transparent | ~7.6 KB |

(`themeIcon_microphone[_white].ico` already exist for the unmuted state — no re-render, they ship unchanged.)

Resx entries in `SoundSwitch.Common/Properties/Resources.resx` (ResXFileRef pattern identical to `themeIconHeadset`):

```xml
<data name="themeIconMicMuted" type="System.Resources.ResXFileRef, System.Windows.Forms">
  <value>..\Resources\themeIcon_mic_muted.ico;System.Drawing.Icon, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a</value>
</data>
<data name="themeIconMicMutedWhite" type="System.Resources.ResXFileRef, System.Windows.Forms">
  <value>..\Resources\themeIcon_mic_muted_white.ico;System.Drawing, ... (same)</value>
</data>
```

`ThemeIcons.CreateIcons` appends the two resource names after `themeIconMicrophone`; `GetFallbackIcon` maps `MicrophoneMuted` to the existing `SystemIcons.Information` fallback (same branch as `Microphone`).

## 7. Telemetry

No new events; the existing `TelemetryService.TrackMicMute` / banner events are untouched.

## 8. Localization

| Key | English value |
|---|---|
| `TrayIconStrings.iconChanger.muteState` | `Microphone Mute State` |

Base `.resx` only; translations follow via the usual localization pipeline (all other locales inherit the English string until translated).

## 9. Files touched

| File | Change |
|---|---|
| `SoundSwitch/Framework/TrayIcon/IconChanger/IconChanger.cs` | add `MuteState = 5` |
| `SoundSwitch/Framework/TrayIcon/IconChanger/Changer/IconChangerMuteState.cs` | new changer (theme-aware, mute-state keyed) |
| `SoundSwitch/Framework/TrayIcon/IconChanger/IconChangerFactory.cs` | register `IconChangerMuteState` |
| `SoundSwitch/Framework/TrayIcon/IconKind.cs` | add `MicrophoneMuted` |
| `SoundSwitch/Framework/TrayIcon/ThemeIcons.cs` | extend `CreateIcons` + fallback mapping |
| `SoundSwitch/UI/Component/TrayIcon.cs` | subscribe `DefaultMicMuteChanged` when type is MuteState |
| `SoundSwitch/Model/IAppModel.cs` / `AppModel*.cs` | new event `DefaultMicMuteChanged` + raise in `HandleMicrophoneMuteChanged` for the tracked mic |
| `SoundSwitch/Localization/TrayIconStrings.resx` | new key `iconChanger.muteState` |
| `SoundSwitch.Common/Properties/Resources.resx` (+Designer via build) | new icon resource entries |
| `SoundSwitch.Common/Resources/themeIcon_mic_muted[_white].ico` | new assets |
| `website/src/configuration/general/behavior.md` | document the sixth icon type |
| `docs/design/tray-icon/2026-11-20-mic-mute-tray-icon.md` | this ADR |

## 10. Test plan

### Automated

`SoundSwitch.Tests`: new `IconChangerMuteStateTests` covering: glyph selection (muted → `MicrophoneMuted`, live → `Microphone`), `DeviceFullInfo`-overload is a no-op, mute-changed event flow filters to the tracked device id, initial-state resolution fallback when no mic is present.

### Manual

1. `dotnet build SoundSwitch.sln -c Release` (win11-dev VM).
2. Select "Microphone Mute State" in Settings → General → Systray Icon: icon shows the current mic state immediately.
3. Mute via SoundSwitch hotkey, via sndvol, via the mic's hardware mute button: icon flips within the event latency.
4. Change default communication mic between a muted and an unmuted device: icon follows.
5. Toggle Windows dark/light theme: icon re-renders on next repaint in the right colour.
6. Switch to any other icon type: mute events no longer repaint the tray icon.

## 11. Decisions

| # | Decision | Rationale |
|---|---|---|
| 1 | New icon *type* (6th dropdown entry) keyed off mute state, not extending ThemeBased with an overlay | Issue asks for an icon type; a separate entry keeps ThemeBased's meaning ("device form factor") clean and composable with future types. Rejected: overlaying a mute badge on the device icon — extra rendering machinery for a 16px canvas, and it changes existing users' visual language. |
| 2 | Track the **default communication** microphone (configured mute target first, `eCommunications` fallback) | Matches `MicrophoneMuteToggler` exactly — icon and hotkey can never disagree. Rejected: console-role mic — the toggler never uses it. Rejected: "any mic muted" — ambiguous state when multiple mics exist. |
| 3 | Mute event reused from the existing `DeviceVolumeChanged` stream, filtered to the tracked device | No new COM subscription, no new thread; the stream is already UI-marshalled and running. |
| 4 | Theme-aware via `ThemeIcons` + `WindowsThemeHelper` | Consistent with the existing five types' theme handling; two-colour set keeps the asset pipeline identical (ADR 2026-09-02). |
| 5 | Dedicated `ChangeIcon(trayIcon, bool)` overload on the concrete class, no `IIconChanger` change | Avoids changing the interface for all six implementations (DRY over speculative generality). The parameterless path resolves the live state and delegates to it; the mute-event path re-reads via the parameterless path (§6.2 ordering note) and the `DefaultDeviceChanged` branch still pattern-matches the concrete type. |
| 6 | Config: plain enum append, no migration | `SwitchIcon` persists as int; new member value `5` is additive. |

## 12. Future work

- Settings preview icons in the dropdown (showing actual glyphs next to each entry) — cosmetic, deferred.
- Hardware-mute sync beyond default mic (per-device mute glyph keying) — deferred with the per-device banner scope.
