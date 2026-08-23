# Windows Notification → Toast Refactor

**Status:** Implemented (see PR #2370).
**Branch:** `refactor/windows-notification-toast`.

## 1. Problem

SoundSwitch ships two independent "Windows notification" mechanisms:

1. **Legacy tray balloon tips** — `NotificationWindows` calls `Configuration.Icon.ShowBalloonTip(...)` (`Shell_NotifyIcon`). Windows 10/11 silently reroutes, delays, and caps these balloon tips; they never appear in Action Center and give no per-channel control.
2. **Modern Windows toast** — `ToastBannerAdapter` already hand-builds `ToastGeneric` XML (via `Windows.UI.Notifications` + `Windows.Data.Xml.Dom`, no Microsoft.Toolkit dependency) and is used today by `BannerManager` as its *exclusive-fullscreen fallback*.

A user who selects the **"Windows Notification"** option (`NotificationType.DefaultWindowsNotification`) still gets balloon tips, while the banner channel has already been given a working, modern toast renderer. In addition, the title/text composition logic is duplicated between `NotificationWindows` and `NotificationBanner` with **inconsistent localized wording**:

- Device change title — banner uses `SettingsStrings.tooltipOnHover_option_playbackDevice` / `_recordingDevice` ("Playback Device" / "Recording Device"); Windows uses `TrayIconStrings.playbackChanged` / `recordingChanged` ("SoundSwitch: Playback device switched to" / "SoundSwitch: Recording device switched to").
- Microphone mute — `NotificationWindows` passes `microphoneName` as the balloon *body* in addition to formatting it into the title, effectively duplicating it; `NotificationBanner` puts the name in the title and shows an icon instead.

## 2. Target Architecture

Make `NotificationWindows` render through the existing toast mechanism instead of `ShowBalloonTip`, while consolidating the duplicated content composition.

```
NotificationWindows ──┐
                      ├──► NotificationContentBuilder (pure, static, platform-neutral)
NotificationBanner  ──┘            │
                                   ▼ produces BannerData
                                   │
                                   ├──► ToastNotificationRenderer (Win10 17763+)  ← toast path
                                   └──► legacy balloon (fallback for <17763 or toast failure)
```

The concrete plan:

1. Extract a **shared, pure content builder** that produces the `Title`/`Text`/`Image`/`Priority` for all four notification channels (`DefaultChanged`, `ProfileChanged`, `AppRuleMatched`, `MicrophoneMuteChanged`). This is where DRY and the localized-string unification live. It is platform-neutral and unit-testable.
2. `NotificationWindows` becomes a thin adapter: build a `BannerData` via the shared builder and hand it to the toast renderer; keep a private balloon-tip path as the fallback.
3. `ToastBannerAdapter` is renamed/relocated into a channel-neutral renderer (`ToastNotificationRenderer`) and its `Show` is made to report success/failure so the Windows channel can fall back (see §7).

## 3. SOLID / DIP — static vs interface

`ToastBannerAdapter` is currently an `internal static class`. Two options were considered:

- **Option A (seam):** introduce `IToastNotificationRenderer` with `EnsureRegistered()` and `bool Show(BannerData)`, implemented by the renderer, and have both `NotificationWindows` and `BannerManager` depend on the abstraction.
- **Option B (KISS):** keep it static; extract testability into the *content builder* instead.

**Recommendation: Option B (keep static), with DIP applied at the content layer.**

Reasoning:

- The renderer is a pure OS-integration detail (`Windows.UI.Notifications`) with **no state and no plausible second implementation** in this Windows-desktop-first app. There is nothing to swap at runtime.
- The codebase has **no DI container**; `NotificationWindows` is constructed with `new NotificationWindows()` inside `NotificationFactory`'s static `EnumImplList`. Introducing an interface would force manual constructor-injection through the factory for zero runtime benefit.
- The renderer is Windows-only (CsWinRT) and cannot run on the Linux partial build anyway, so its seam adds no test coverage. The genuinely testable logic is the **title/text/image composition**, which is exactly what step 1 extracts.
- The dependency *inversion* that matters is already achieved by step 1: both notification implementations depend on `NotificationContentBuilder` (a small, stable abstraction) rather than on each other or on duplicated literals.

If a seam is later wanted (e.g. for a mocked toast in UI tests), the rename in §8 to an instance-friendly type leaves the door open without any churn now.

## 4. DRY — shared content builder

New type: `SoundSwitch.Framework.NotificationManager.Notification.NotificationContentBuilder` (`internal static`), returning `BannerData` (the existing, already-consumed DTO — no new DTO needed, since `ToastBannerAdapter` and `BannerManager` both already accept `BannerData`).

Proposed surface (all inputs already available at the call sites):

```csharp
internal static BannerData BuildDefaultChanged(DeviceFullInfo device);
internal static BannerData BuildProfileChanged(Profile.Profile profile, Bitmap icon);
internal static BannerData BuildAppRuleMatched(DeviceFullInfo playback, DeviceFullInfo recording, Bitmap icon);
internal static BannerData BuildMicrophoneMuteChanged(string microphoneName, bool newMuteState);
```

Each method fills `Title`, `Text`, `Image`, and `Priority`, leaving `Position`/`Ttl`/`Opacity`/`DisplayInfo` to the caller (as `NotificationBanner.CreateBannerData()` already does today), since those come from `Configuration` and differ per channel. The builder does **not** own `Configuration`.

Details:

- `BuildDefaultChanged` maps `DataFlow.Render` → `SettingsStrings.tooltipOnHover_option_playbackDevice`, `DataFlow.Capture` → `_recordingDevice`; `Text = device.NameClean`; `Image` from `device.LargeIcon.ToBitmap()`. This also fixes a behavior gap: the balloon path currently shows **no** device icon, whereas the toast supports `appLogoOverride`.
- `BuildProfileChanged` / `BuildAppRuleMatched` reproduce the current banner composition exactly (distinct `NameClean`, `\n` join, `Priority = 1`, `Title = SettingsStrings.appSoundLock_tab` for rules).
- `BuildMicrophoneMuteChanged` produces the *fading* content (`notification_microphone_muted`/`_unmuted` title, `Resources.microphone_muted`/`_unmuted` icon, `Priority = 2`). The **Persistent** path stays banner-specific (`MicrophoneMuteBannerManager`) and is not part of this builder.

### Localized-string discrepancy — decision

The two channels use different device-change wording.

**DECIDED (maintainer, overriding the original recommendation): each channel KEEPS its existing wording.** `NotificationWindows` continues to use `TrayIconStrings.playbackChanged` / `recordingChanged`; `NotificationBanner` continues to use `SettingsStrings.tooltipOnHover_option_playbackDevice` / `_recordingDevice`.

Rationale for rejecting unification:

- `TrayIconStrings.playbackChanged` / `recordingChanged` are **translated into ~20 locales** (`ar`, `bg`, `cs`, `de`, `el-GR`, `es`, `fr`, and more `.resx` files). Unifying would discard that human translation work in exchange for a purely cosmetic consistency win.
- No user-visible wording regression: existing "Windows Notification" users see exactly the text they see today, now rendered as a toast instead of a balloon.
- The DRY goal is still fully met — the *composition logic* (which strings go in Title vs Text, the `DataFlow` switch, image/priority) is shared; only the string constants differ per channel.

**Consequence for the builder API:** `BuildDefaultChanged` takes an explicit wording selector so the single code path serves both channels:

```csharp
internal enum DeviceChangeWording { Banner, WindowsNotification }

internal static BannerData BuildDefaultChanged(DeviceFullInfo device, DeviceChangeWording wording);
```

`Banner` → `SettingsStrings.tooltipOnHover_option_playbackDevice` / `_recordingDevice`.
`WindowsNotification` → `TrayIconStrings.playbackChanged` / `recordingChanged`.

No new `.resx` keys are required, and **no localized string is retired** (this supersedes step 7 in §11 — there is no orphaned-key cleanup).

## 5. Ownership / layering

`ToastBannerAdapter` lives in `SoundSwitch/Framework/Banner`, but after this change it serves a non-banner channel (the Windows notification) as well as the banner's fullscreen fallback.

**Recommendation: rename + move to `SoundSwitch/Framework/Toast/ToastNotificationRenderer.cs`** (new namespace `SoundSwitch.Framework.Toast`).

- The `Framework.Banner` namespace should remain Win32-overlay-centric; the toast renderer is now a cross-channel OS integration and its name `...BannerAdapter` becomes misleading.
- The move is low-churn: only two call sites reference it today (`BannerManager.ShowNotification` and `BannerManager.Setup`), plus the new `NotificationWindows` site.
- The `AppId` constant (`"aaflalo.SoundSwitch.Application"`), the hand-built XML builder, `SaveImageToTemp`, and `EscapeXml` all move with it unchanged.

Fallback (if churn is the overriding concern): keep the file at its current path and only rename the type to `ToastNotificationRenderer`. The behavior is identical; only the namespace semantics differ. The recommended move is preferred for correctness.

## 6. Behavior deltas the user will notice

| Aspect | Before (balloon) | After (toast) |
|---|---|---|
| Action Center | Not shown | Toast appears in and persists in Action Center until `ExpirationTime` |
| Persistence | ~500–1000 ms transient | Respects `Ttl` (`BannerOnScreenTime`), removed on expiry |
| Popup vs quiet | Always popup | `SuppressPopup` when `Ttl < 4s` (mirrors banner behavior) |
| Device icon | Tray icon only | Real device icon (`appLogoOverride`, circle-cropped) |
| OS floor | Any Windows | Windows 10 1809 (17763)+ |
| Wording | "SoundSwitch: Playback device switched to" | Unchanged — `TrayIconStrings.playbackChanged` is kept (see §4) |

These are the correct modern semantics, but they are **visible**: toasts linger in Action Center and can be dismissed/focused by the user, which balloon tips could not.

## 7. OS gating, failure fallback, and swallowed exceptions

`ToastBannerAdapter.Show` currently wraps its body in `try/catch` and only logs (`Log.Warning`), returning `void`. For the banner fullscreen fallback this is acceptable (the alternative overlay cannot render in FSE anyway). For the **Windows notification channel it is not**: the user explicitly chose this channel, so a silent no-op is the worst outcome.

**Decision:**

1. Change `ToastNotificationRenderer.Show(BannerData)` to return `bool` (`true` on success, `false` on any caught exception). Keep the `Log.Warning`.
2. `NotificationWindows` keeps a private legacy balloon path (the existing `ShowBalloonTip` calls, reusing the unified wording) and uses it when:
   - `!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763)` — pre-1809 Windows has no toast API, so balloon is the only native option.
   - `ToastNotificationRenderer.Show(...)` returns `false` — toast registration/display failed (missing AppUserModelID, etc.).
3. `BannerManager` ignores the return value (its FSE path has no meaningful overlay fallback, and this is unchanged behavior).

`NotificationWindows.IsAvailable()` remains `true` — it must stay selectable on pre-17763 Windows because it now degrades to balloon rather than disappearing from the settings UI.

## 8. AppUserModelID / EnsureRegistered ownership

`EnsureRegistered()` is currently called only from `BannerManager.Setup()`, guarded by the 17763 check. If a user selects the Windows notification channel but the banner channel is never exercised, `EnsureRegistered` may never run and the toast may fail to register the AUMID.

**Decision:** move the responsibility off `BannerManager` onto a single always-run startup path:

- Call `ToastNotificationRenderer.EnsureRegistered()` from `NotificationManager.Init()` (which is always constructed), keeping the same `IsWindowsVersionAtLeast(10,0,17763)` guard.
- `EnsureRegistered()` is idempotent (it only creates a registry subkey) and thread-safe (registry + reflection, no UI affinity), so it needs no `WindowsFormsSynchronizationContext`.
- Remove the call from `BannerManager.Setup()` to avoid a second redundant call, or leave it harmlessly (idempotent). Recommended: remove it and let `NotificationManager.Init()` be the single owner.

## 9. Thread affinity & the shared-singleton constraint

- **Toast path has no UI-thread affinity.** `ToastNotificationManager.CreateToastNotifier(...).Show(...)` is thread-safe; the adapter's own doc already states "Safe to call from any thread." `NotificationWindows.Notify*` may be invoked from the device-change event path (background), and unlike the overlay path in `BannerManager` (which marshals through `_syncContext.Send`), the toast path must **not** be marshaled.
- **Singleton constraint.** `NotificationFactory` holds one static `NotificationWindows` instance shared across the SwitchDevice / SwitchProfile / MicrophoneMute channels (its `Configuration` is overwritten per `BuildNotification`, but with identical values). Therefore `NotificationWindows` must remain **stateless**: no instance fields, all per-call state flows through method parameters and the read-only `Configuration` during `Notify*`. The shared `NotificationContentBuilder` and `ToastNotificationRenderer` are `static` and stateless, which satisfies this by construction.

## 10. Enum / config compatibility

- **Do not resurrect `NotificationType.ToastNotification` (value `4`) as a selectable option.** It is retired; `SoundSwitchConfiguration.Migrate()` already rewrites persisted `ToastNotification` → `BannerNotification` (and `CustomNotification` → `SoundNotification`). The toast is an implementation/rendering detail of the *existing* `DefaultWindowsNotification` channel, not a new enum member.
- **No config schema change is required.** `NotificationType.DefaultWindowsNotification` keeps its value and meaning; only its rendering changes. `Migrate()` needs no new branch, and no `MigratedFields` entry is added.

## 11. Files to change (ordered, implementable steps)

1. **`SoundSwitch/Framework/NotificationManager/Notification/NotificationContentBuilder.cs`** (new) — pure static Title/Text/Image/Priority composition for the four channels; the DRY home and the localized-string unification point.
2. **`SoundSwitch/Framework/Toast/ToastNotificationRenderer.cs`** (rename/move of `ToastBannerAdapter.cs`) — channel-neutral toast renderer; `Show` returns `bool`; carries `AppId`, XML builder, `SaveImageToTemp`, `EscapeXml`, `EnsureRegistered`.
3. **`SoundSwitch/Framework/Banner/BannerManager.cs`** — update two references to `ToastNotificationRenderer`; remove/redundant-ize the `EnsureRegistered` call.
4. **`SoundSwitch/Framework/NotificationManager/Notification/NotificationWindows.cs`** — replace `ShowBalloonTip` with `NotificationContentBuilder` + `ToastNotificationRenderer.Show`, add private balloon fallback; keep telemetry call.
5. **`SoundSwitch/Framework/NotificationManager/Notification/NotificationBanner.cs`** — replace inline Title/Text/Image composition with `NotificationContentBuilder` (delete the now-duplicated body).
6. **`SoundSwitch/Framework/NotificationManager/NotificationManager.cs`** — add `ToastNotificationRenderer.EnsureRegistered()` to `Init()` (single owner, 17763-guarded).
7. **`SoundSwitch/Localization/`** — **no changes.** No new keys, and no key retired: `TrayIconStrings.playbackChanged` / `recordingChanged` stay in use by the Windows channel (see §4).

## 12. Test / validation strategy

- **Linux partial build** (full solution cannot build on Linux — CsWinRT projection generation in `SoundSwitch.Audio.Manager` is Windows-only; `CS0006` there is pre-existing/expected):
  ```
  dotnet build SoundSwitch/SoundSwitch.csproj -c Debug -p:LinuxBuild=true -p:BuildProjectReferences=false
  ```
- **Windows CI is the real compile gate** (CsWinRT toast code, `Windows.UI.Notifications`).
- **Unit tests** (Windows only): `NotificationContentBuilder` is pure and platform-neutral — add tests in `SoundSwitch.Tests` asserting title/text/image/priority for each channel, including the `DataFlow` switch and the wording unification. Follow the existing `ExclusiveFullscreenDetectorTests` pattern as a model.
- **Manual matrix:** Win10 1809+ toast shows and persists in Action Center; Win10 pre-1809 / older falls back to balloon; forced toast failure (unregister AUMID) falls back to balloon; banner FSE fallback still routes to toast.

## 13. Website / docs update (do not edit yet)

`website/src/configuration/notifications.md` — the **Notifications** page already lists "Windows Toast" vs "Banner Notification" and documents the banner→toast fullscreen fallback. It needs a short user-facing note that the **"Windows Notification"** option now renders as a native Windows toast (appears in and persists in Action Center, follows the on-screen time, requires Windows 10 1809+, falls back to a legacy balloon on older Windows). Validate with `cd website && npm run docs:build`.

## 14. Decisions (resolved by maintainer)

All open questions are settled; implementation follows **Option 1** below.

1. **Wording** — **REJECTED unification.** Each channel keeps its existing strings; `TrayIconStrings.playbackChanged` / `recordingChanged` are preserved because they carry ~20 locales of translation. The builder takes a `DeviceChangeWording` selector (§4).
2. **`EnsureRegistered` ownership** — **APPROVED:** move to `NotificationManager.Init()` as the single owner; remove the `BannerManager.Setup()` call.
3. **Renderer location** — **APPROVED:** move + rename to `SoundSwitch/Framework/Toast/ToastNotificationRenderer.cs`.
4. **Orphaned localization keys** — **N/A**, nothing is orphaned given decision 1.

Also approved for the same PR:

- `ToastNotificationRenderer.Show()` returns `bool` so the Windows channel can fall back to a balloon tip.
- Unit tests for `NotificationContentBuilder` in `SoundSwitch.Tests`.
- Short user-facing note in `website/src/configuration/notifications.md`.

### Chosen implementation option

1. **Full unified builder + renamed/moved renderer + balloon fallback (this design).** ← selected
2. Same, but renderer rename-in-place. Identical behavior, weaker namespace semantics.
3. Minimal swap with no DRY extraction or fallback — rejected.
