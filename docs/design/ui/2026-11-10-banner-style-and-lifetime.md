# Separate banner style from microphone notification lifetime

**Status:** Implemented on `feat/banner-style`
**Author:** SoundSwitch contributors
**Branch:** `feat/banner-style`
**Base:** `origin/dev`
**Issue:** [SoundSwitch#2574](https://github.com/Belphemur/SoundSwitch/issues/2574)
**Related:** [Banner manager](../../banner-manager.md)

## 1. Problem

Previously, `NotificationBanner` used normal presentation for fading microphone
notifications, while `MicrophoneMuteBannerManager` hardcoded compact presentation
for persistent notifications. Users could not select compact fading banners.
Persistent microphone On notifications expired after 1.5 seconds.

## 2. Goals

1. Offer one global Normal/Compact banner style, independent of lifetime.
2. Make both microphone states respect None, Fading, and Persistent identically.
3. Preserve target scoping, click actions, focus safety, and stacking.

## 3. Non-goals

- Per-notification style overrides or changes to native Windows toast presentation.
- New notification modes or changes to audio device selection.

## 4. UX

**Style: Normal / Compact** appears under Banner options, immediately before Display
Info. It affects device, profile, app-rule, microphone, and custom-position preview
banners. The default is Normal, including upgrades.
Style changes refresh visible windows, including their content and icons, without
restarting their timers or replacing click actions.

Normal microphone banners retain the original fading presentation: the original
muted/unmuted icon and a single title containing the microphone name. Compact retains
the original persistent presentation: its microphone icon, short On/Off title, and
separate microphone name. Either presentation supports either lifetime.

For either microphone state: None shows nothing, Fading uses On-screen time before
fading out, and Persistent remains until the state changes or leaves scope. When
both states are persistent, update the same per-microphone window in place.

## 5. Investigation

`SoundSwitch/Framework/Banner/BannerForm.cs` already handles TTL separately from
font/icon scaling. Compact uses 0.8x fonts/padding and comparatively larger icons,
not uniform scaling. Generic device icons have varying source resolutions; enlarging
them in the title's table row pushes the description away from the title.
No new Windows integration is needed. Existing non-activating window safeguards stay.

## 6. Design

Persist `BannerStyle` (`Normal = 0`, `Compact = 1`) in `SoundSwitchConfiguration`;
expose it through the notification settings model. Like background color and scale,
the renderer reads the global appearance setting. Remove caller-specific compact
flags, so lifetime cannot implicitly choose presentation.

`NotificationContentBuilder.BuildMicrophoneBanner` supplies the original normal
content and an optional `BannerContent` compact variant in `BannerData.CompactContent`.
The variant contains only title, text, and image, not lifetime or click behavior.
`BannerForm.ApplyPresentation` selects the variant for the global style both when
showing a notification and when refreshing it. Shared Windows-toast content remains
unchanged.

All compact banners use the existing microphone layout: the icon occupies the title
row, with the description below it. One shared presentation path accepts title, text,
and image; content variants do not select different layouts.
The shared icon box is 24×24 at 100% scale, matching the 16×16 microphone resources
at 1.5×. It scales with the user setting, independently of source image resolution;
`PictureBox.Zoom` preserves aspect ratio. Normal presentation retains native-image
scaling. Lifetime and click behavior are unchanged.

Generalize `MicrophoneMuteBannerManager` to track either persistent state. Reconcile
the current state of every in-scope microphone against its selected mode. Reconciliation
never emits fading notifications. Existing persistent windows are updated, not replaced.
Changing modes, notification type, target, or device availability removes stale windows.
Recompute stacked offsets after an appearance change. `BannerManager` keeps temporary
banners in arrival order so removals and subsequent additions cannot reorder the stack.
Keep all form work on the UI thread.

Fading banners are tracked by microphone ID so state changes can replace them without
affecting unrelated notifications.

Ensure `BannerForm.SetData` stops a previous finite timer when switching to infinite
TTL, and updates the timer interval when reusing a form with another finite TTL.

### Migration

The additive property defaults to Normal; no destructive conversion or migration
marker is necessary. `Migrate()` normalizes invalid enum values idempotently.

| Before | After |
|---|---|
| Missing style | Normal |
| Explicit Normal/Compact | Unchanged |
| Invalid style | Normal |
| Existing microphone modes/time | Unchanged; persistent On now truly persists |

## 7. Telemetry

No new events. Preserve existing banner and microphone click telemetry.

## 8. Localization

| SettingsStrings key | English value |
|---|---|
| `banner.style` | Style |
| `banner.style.normal` | Normal |
| `banner.style.compact` | Compact |

Update the base microphone-mode tooltip to explain symmetric timing. Other languages
fall back to English for new keys until translated.

## 9. Files touched

| File | Change |
|---|---|
| `SoundSwitch/Framework/Banner/BannerStyle.cs` | Style enum |
| `SoundSwitch/Framework/Configuration/{I,}SoundSwitchConfiguration.cs` | Persistence/default/validation |
| `SoundSwitch/Model/{INotificationSettings,AppModel.NotificationSettings}.cs` | Global appearance property |
| `SoundSwitch/Framework/Banner/{BannerData,BannerForm,BannerAppearance}.cs` | Global rendering, live refresh, timer correctness |
| `SoundSwitch/Framework/Banner/BannerManager.cs` | Reflow temporary stacks after appearance changes |
| `SoundSwitch/Framework/Banner/MicrophoneMute/MicrophoneMuteBannerManager.cs` | Symmetric persistent state tracking/content |
| `SoundSwitch/Framework/NotificationManager/{NotificationManager,Notification/NotificationBanner}.cs` | Persistent state reconciliation and consistent fading content |
| `SoundSwitch/Framework/NotificationManager/Notification/NotificationContentBuilder.cs` | Shared banner-specific microphone content; unchanged toast content |
| `SoundSwitch/Model/AppModel.DeviceService.cs` | Supply both current microphone states |
| `SoundSwitch/Model/AppModel.cs` | Reconcile disconnected devices regardless of scope setting |
| `SoundSwitch/UI/Forms/Settings.cs` | Localized style selector |
| `SoundSwitch/Localization/SettingsStrings.resx` | Labels and mode tooltip |
| `SoundSwitch.Tests/*Banner*Tests.cs` | Appearance, lifecycle, migration, layout regression coverage |
| `website/src/configuration/notifications.md`, `img/Documentation/Notifications.png` | User guidance and automated screenshot |
| `docs/banner-manager.md` | Persistent-state lifecycle documentation |
| `tools/Test-BannerStyle{,EndToEnd}.ps1`, `tools/README.md` | Repeatable focused and live validation |
| `tools/Control-SoundSwitch.ps1` | Opt-in discovery of untitled banner windows |
| `tools/Test-MuteTarget.ps1`, `.agents/skills/mute-target-testing/SKILL.md` | Test routing and coverage guidance |

## 10. Test plan

### Automated

- `dotnet build SoundSwitch.sln -c Debug`.
- Run `SoundSwitch.Tests`, including style persistence/default/invalid-value tests,
  both persistent states, finite/infinite timer transitions, original style-specific
   microphone content/icons, live content refresh without TTL/action changes,
   shared compact layout/icon bounds across source sizes, and reordered DPI layout.
- Cover mixed Fading/Persistent/None transitions for either microphone state and style,
  single/stacked banners, preservation of unrelated banners, and consistent microphone names.
- `pwsh tools/Test-MuteTarget.ps1` and `pwsh tools/Test-MuteTargetEndToEnd.ps1`.
- Capture Notifications using `tools/Capture-DocumentationScreenshot.ps1`.
- Run `npm run docs:build` in `website/`.

### Live checks

1. Select each style and switch devices; verify fonts/icon proportions and dismissal.
2. For both microphone states, test None/Fading/Persistent and mixed combinations.
3. Verify persistent On remains past 1.5 seconds and Fading uses On-screen time.
4. Change style with persistent banners visible; check stacking and no focus stealing.
5. Change target/scope/modes, disconnect a microphone, and restart with each state.
6. Compare Normal microphone banners to the original fading presentation and Compact
   to the original persistent presentation, for both states and both lifetimes.
7. Compare Compact device and microphone banners: title-row icons, matching text spacing,
   and consistent icon sizing. Confirm Normal device banners are unchanged.
8. Confirm Style is before Display Info, then refresh the Notifications screenshot.
9. Toggle rapidly with one state Fading and the other Persistent: only the new state
   remains. Compare microphone names after Settings changes and hotkey events.

### Validation status

Debug solution build, 491 app tests, banner-style E2E (32 checks), mute-target E2E
(14 checks), and website build passed. The known-flaky refresh test is excluded.
Notifications screenshot updated at 100% display scaling.

## 11. Decisions

| # | Decision | Rationale |
|---|---|---|
| 1 | Global Normal/Compact, not per-event styles or a legacy mode | Simple, consistent UI |
| 2 | Persistent On is indefinite, not 1.5 seconds | Mode names must mean the same for both states |
| 3 | Reuse existing renderer and managers | Avoid parallel presentation/lifecycle implementations |
| 4 | Preserve toast content | This change targets banners only |
| 5 | Preserve each original microphone presentation, not identical content across styles | Normal must match the old fading look; Compact must match the old persistent look, independently of lifetime |
| 6 | Share the microphone compact layout and 24×24 icon box across all banner types; reject the separate two-row-spanning device layout | Keep title alignment, spacing, and icon sizing consistent without source-resolution-dependent growth |
| 7 | Label the selector Style and place it before Display Info | Keep appearance choices together rather than append Style after Scale |

## 12. Future work

- Per-notification style overrides only if separately requested.
