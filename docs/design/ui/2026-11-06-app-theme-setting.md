# Application theme setting — System / Light / Dark

**Status:** Proposed (implemented on `feat/theme-setting`, worktree branch)
**Author:** Hermes
**Branch:** `feat/theme-setting`
**Base:** `dev` (`f33dd9b5`)
**Issue:** [Belphemur/SoundSwitch#2567](https://github.com/Belphemur/SoundSwitch/issues/2567) — "dark theme settings still not fully working"
**Related:** `docs/design/ui/2026-09-03-dark-theme-winforms.md` (this ADR supersedes its non-goal *"No per-user Force light / Force dark / Follow system preference"*), `docs/design/ui/2026-09-14-startup-order-setcolormode-fix.md`

---

## 1. Problem

Since PR #2420 the app calls `Application.SetColorMode(SystemColorMode.System)` in
`SoundSwitch/Program.cs:135` and unconditionally follows the OS `AppsUseLightTheme`
registry value. There is no way for the user to opt out.

Issue #2567 shows the result on **Windows 10**: the settings window is a broken
mix of light surfaces, dark group-box captions and partially darkened custom paint.
Maintainer and reporter both confirm Windows 11 renders correctly ("I've got that
on my Windows 10 install too, on Windows 11 it works as expected"). A user in the
issue asks the obvious follow-up: *"Is there a setting to disable dark theme?"* —
there is none.

### 5. Why dark mode fails on Windows 10 (investigation)

`Application.SetColorMode(SystemColorMode.System)` resolves dark mode only when the
platform can render it. The .NET 10 documentation for
[`Application.SetColorMode`](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.application.setcolormode)
states:

> "Note that the dark color mode is only available from Windows 11 on or later
> versions. If the system is set to a high contrast mode, the dark mode is not
> available."

Two independent axes therefore disagree on Windows 10 with a dark OS app-mode:

| Axis | Windows 10 behaviour | Source |
|---|---|---|
| WinForms framework palette (`SetColorMode(System)`) | Dark is **not available**; controls fall back to the classic (light) palette, with partial exceptions → the mixed rendering captured in the #2567 screenshots | Microsoft docs, §6-§7 limitations in the 2026-09-03 dark-theme ADR (Win10 title bar stays light, ComboBox popups stay light) |
| SoundSwitch custom paint (`WindowsThemeHelper.IsDarkModeEnabled()`, `SoundSwitch/Framework/WinApi/WindowsThemeHelper.cs`) | Reads the registry directly and returns `true` → outlines, ListView group headers, `IconTextComboBox`, `HotKeyTextBox`, tray icon all paint dark | `WindowsThemeHelper.cs` has no Windows-version guard |

On Windows 11 the two axes agree (dark works end-to-end); on Windows 10 they
diverge, which is exactly what makes it look "half-baked". The bug is not in
SoundSwitch's custom paint per se — it is that SoundSwitch paints dark on a
platform where the framework cannot.

## 2. Goals

1. Add a user-facing **theme setting** under *General* (with the language setting):
   **Follow Windows (System)** / **Light** / **Dark**, persisted like every other
   setting in `SoundSwitchConfiguration`.
2. `System` (the default) keeps today's behaviour unchanged: follow the OS app mode.
3. Force **Light** or **Dark** also overrides every custom-painted surface (the
   single `IsDarkModeEnabled()` axis) and the theme-based tray icon.
4. Unify the two axes behind one decision function so the framework palette and
   the custom paint can never disagree again (the root cause of #2567).
5. On **Windows 10**, where WinForms dark mode is only partially implemented
   (Microsoft docs), resolve **everything to the coherent light palette** instead
   of today's broken mix.
6. Changing the mode requires an application restart (same UX contract as the
   language setting), because `Application.SetColorMode` must run before any
   form is created.

## 3. Non-goals

- No live theme switching of already-created forms. `SetColorMode` does not
  reliably recolour existing windows (documented limitation, 2026-09-03 ADR §5.2);
  the restart prompt is the accepted contract.
- No rework of the banner UI. `BannerForm` remains untouched (standing directive).
- No High-Contrast handling beyond what the framework already does.
- No third-party theming library.
- No Windows-10 dark-mode completion effort (uxtheme hacks) — deferred to future work.
- No new telemetry events.

## 4. UX

New group box **Theme** on the *General* tab (`appSettingTabPage`), in the right
column below the *Language* group box, containing a single
`DropDownList` combo:

| Option | Label | Behaviour |
|---|---|---|
| System | Follow Windows | Framework follows the OS `AppsUseLightTheme` (current behaviour; unchanged on Win11) |
| Light | Light | Forced light palette, regardless of the OS setting |
| Dark | Dark | Forced dark palette |

Choosing a different value prompts *“The theme will change after restarting
SoundSwitch. Do you want to restart now?”* (Yes/No, same pattern as
`LanguageComboBox_SelectedIndexChanged` in `SoundSwitch/UI/Forms/Settings.cs:1507`).
Selecting the already-active value does nothing.

## 5. Investigation

Already covered in **§1.5** above; the decisive evidence is the Microsoft Learn
note and the `WindowsThemeHelper.cs` / `Program.cs:135` pair of code sites. The
app targets `net10.0-windows10.0.17763.0` (`SoundSwitch/SoundSwitch.csproj:11`),
so Windows 10 support must be kept; a runtime capability check is needed rather
than a compile-time assumption.

## 6. Design

### 6.1 Data model (new files in `SoundSwitch/Framework/Theme/`)

```csharp
namespace SoundSwitch.Framework.Theme;

/// <summary>Which theme the application follows.</summary>
public enum ThemeMode
{
    System = 0, // default: follow the Windows app mode
    Light = 1,
    Dark = 2,
}

public interface IThemeMode : IEnumImpl<ThemeMode>;   // TypeEnum + localized Label
```

Config: `public ThemeMode ThemeMode { get; set; } = ThemeMode.System;` in
`SoundSwitch/Framework/Configuration/SoundSwitchConfiguration.cs`. **No migration
is needed**: Newtonsoft deserializes a missing property to the default value,
which keeps today's behaviour; an out-of-range persisted int falls through the
`switch` default and is treated as `System` instead of throwing.

Model: `ThemeMode ThemeMode { get; set; }` on `IAppSettings`
(`SoundSwitch/Model/IAppSettings.cs`), implemented in
`SoundSwitch/Model/AppModel.AppSettings.cs` mirroring `IconDoubleClick` (get →
`AppConfigs.Configuration`, set → no-op guard, assign, `Save()`).

### 6.2 The single decision function (`ThemeResolver.cs`)

One static, side-effect-free decision is the source of truth for both consumers
(framework colour mode and custom paint):

```csharp
/// <summary>
/// WinForms renders dark mode correctly only on Windows 11 (build 22000+);
/// Microsoft docs: "the dark color mode is only available from Windows 11 on
/// or later versions". On older hosts everything resolves to light.
/// </summary>
internal static Func<bool> DarkModeSupported { get; set; }  // test seam
    = () => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);

internal static bool ResolveIsDarkMode(ThemeMode mode, bool osDarkMode, bool darkModeSupported)
{
    if (!darkModeSupported) return false;          // Win10: coherent light palette (#2567 fix)
    return mode switch
    {
        ThemeMode.Light => false,
        ThemeMode.Dark => true,
        _ => osDarkMode,                           // System
    };
}
```

- **`Program.cs`** — `Application.SetColorMode(ThemeResolver.ResolveColorMode())`
  in place of the hardcoded `SystemColorMode.System` (same position, so the
  startup-order contract from the 2026-09-14 ADR is preserved).
- **`WindowsThemeHelper.ReadDarkModeEnabled()`** — keeps its registry read as the
  *OS* input and wraps it with the resolver:
  `return ThemeResolver.ResolveIsDarkMode(ThemeResolver.ReadThemeMode(), ReadOsAppDarkMode(), ThemeResolver.DarkModeSupported());`
  The existing `DarkModeProvider` test seam stays untouched, so
  `SoundSwitch.Tests` keeps working unchanged.

### 6.3 Factory and Settings UI

Following the `IconChangerFactory` pattern
(`SoundSwitch/Framework/TrayIcon/IconChanger/IconChangerFactory.cs`):

- `ThemeFactory : AbstractFactory<ThemeMode, IThemeMode>` with `ThemeModeSystem`,
  `ThemeModeLight`, `ThemeModeDark` (labels from `SettingsStrings`).
- `Settings.Designer.cs`: `themeGroupBox` (300×55 at the free spot in the right
  column of `appSettingTabPage`, below `languageGroupBox`) with
  `themeComboBox` (`DropDownStyle.DropDownList`, docked fill, wired to
  `ThemeComboBox_SelectedIndexChanged`).
- `Settings.cs`: populate with `new ThemeFactory().ConfigureListControl(themeComboBox); themeComboBox.SelectedValue = AppConfigs.Configuration.ThemeMode;`
  in the existing population block, localize the group-box text in `SetTexts`,
  and handle the change with `SetComboBoxValue<ThemeMode>` + the restart prompt
  copied from the language handler.
- `ApplyTheme()` needs no change: it already derives every colour from
  `WindowsThemeHelper.IsDarkModeEnabled()`, which now honours the forced mode —
  after a restart the custom paint follows the choice automatically.

### 6.4 Behaviour

| Trigger | Result |
|---|---|
| Startup, `ThemeMode = System` (default, incl. every existing config) | Identical to today on Win11; on Win10 with dark OS app-mode the whole app is now coherent **light** instead of the broken mix (the deliberate #2567 fix) |
| Startup, `Light` | Classic colour mode + light custom paint, on any Windows version |
| Startup, `Dark` | Dark colour mode + dark custom paint on Win11; on Win10 clamped to light (platform cannot render it) |
| OS theme flip while running (System mode, Win11) | As today: `SystemThemeChanged` → `RefreshTheme()`; unaffected |
| Setting change | Saved immediately, restart prompted; without restart the new mode applies at next launch |

### 6.5 Migration

None: new nullable-in-practice JSON property defaulting to `System` = current
behaviour (`Migrate()` untouched).

## 7. Telemetry

No new events.

## 8. Localization

New keys in `SoundSwitch/Localization/SettingsStrings.resx` (English first;
community translations follow separately):

| Key | English value |
|---|---|
| `theme` | `Theme` |
| `themeSystem` | `Follow Windows` |
| `themeLight` | `Light` |
| `themeDark` | `Dark` |
| `themeRestartRequired` | `The theme will change after restarting SoundSwitch.\nDo you want to restart now?` |

(reuses the existing `restartRequired.caption` for the prompt title;
`SettingsStrings.Designer.cs` is regenerated by the existing
`PatchLocalizedResourceDesigners` prebuild target and committed.)

## 9. Files touched

| File | Change |
|---|---|
| `docs/design/ui/2026-11-06-app-theme-setting.md` | This ADR |
| `SoundSwitch/Framework/Theme/ThemeMode.cs` | New enum |
| `SoundSwitch/Framework/Theme/IThemeMode.cs` | New impl interface |
| `SoundSwitch/Framework/Theme/ThemeResolver.cs` | New: decision function + colour-mode mapping + test seams |
| `SoundSwitch/Framework/Theme/ThemeFactory.cs` | New: factory + `ThemeModeSystem/Light/Dark` label impls |
| `SoundSwitch/Framework/Configuration/SoundSwitchConfiguration.cs` | `ThemeMode` property (default `System`) |
| `SoundSwitch/Framework/Configuration/ISoundSwitchConfiguration.cs` | Interface property |
| `SoundSwitch/Program.cs` | `SetColorMode(ThemeResolver.ResolveColorMode())` |
| `SoundSwitch/Framework/WinApi/WindowsThemeHelper.cs` | Route `ReadDarkModeEnabled` through the resolver |
| `SoundSwitch/Model/IAppSettings.cs` | `ThemeMode` property |
| `SoundSwitch/Model/AppModel.AppSettings.cs` | Persisting property |
| `SoundSwitch/UI/Forms/Settings.Designer.cs` | `themeGroupBox` + `themeComboBox` |
| `SoundSwitch/UI/Forms/Settings.cs` | Populate, localize, change handler with restart prompt |
| `SoundSwitch/Localization/SettingsStrings.resx` (+ generated `.Designer.cs`) | 5 new keys |
| `SoundSwitch.Tests/ThemeResolverTests.cs` | New tests |
| website settings page + `img/Documentation/...` | Documentation sync (same PR) |

## 10. Test plan

### Automated (`SoundSwitch.Tests`, xUnit)

`ThemeResolverTests` driving the pure decision function through the existing
test-seam pattern (`WindowsThemeHelper.DarkModeProvider` precedent):

1. `Light` → `SystemColorMode.Classic` / not dark, on any host, OS dark or light.
2. `Dark` on Windows 11 host (`OperatingSystem.IsWindowsVersionAtLeast(10,0,22000)` seam = true) → `SystemColorMode.Dark` / dark.
3. `Dark` on a Windows 10 host → **light** (clamped).
4. `System` on Win11 host, OS dark / OS light → dark / light respectively (defaults unchanged).
5. `System` on Win10 host with OS dark → **light** (regression test for #2567).
6. `WindowsThemeHelper.IsDarkModeEnabled()` honours the forced mode via the test seams (config untouched).

### Manual

1. `dotnet build SoundSwitch.sln -c Debug`.
2. Windows 11, OS dark, fresh config → app follows Windows; set *Light* →
   restart prompt → Yes → everything light; set *Dark* → everything dark;
   `Follow Windows` behaves as before.
3. Windows 10 (dual boot per the #2567 reporter) with OS dark → app coherent
   light in `System`; *Light* works; verify no mixed rendering anywhere.
4. Change theme, decline restart → app keeps working; restart later applies it.

## 11. Decisions

| # | Decision | Rationale |
|---|---|---|
| 1 | One decision function (`ThemeResolver`) consumed by both `SetColorMode` and `WindowsThemeHelper` | DRY/SOLID: the disagreement between the two axes *is* the root cause of #2567; a single seam also matches the existing `DarkModeProvider` test pattern instead of adding a new one |
| 2 | Clamp dark → light on Windows 10 (< build 22000), including explicit *Dark* and *System*-on-dark | Microsoft: dark color mode "only available from Windows 11"; #2567 shows the mixed state is worse than light. Coherent light UI replaces broken mixed UI; explicit *Dark* on Win10 cannot be honoured by the framework |
| 3 | Restart required on change, reusing the language-setting prompt pattern | `SetColorMode` is startup-only; live re-colour is documented as unreliable (2026-09-03 ADR §5.2). Reuses an established UX contract instead of a new one |
| 4 | Rejected: live `SetColorMode(...)` re-application | Documented as inconsistent for existing standard controls; would reintroduce exactly the #2567 class of bug on every flip |
| 5 | Rejected: hide/disable "Dark" on Windows 10 | Disabling options per-OS complicates the UI and tests; the clamp makes the choice harmless and keeps one item set everywhere |
| 6 | Rejected: keep hardcoded light-only (drop dark entirely) | Discards a working Win11 feature; the setting is additive and `System` preserves current Win11 behaviour byte-for-byte |
| 7 | Config value is the new `ThemeMode` enum in JSON, no migration | Missing property deserializes to default `System` = current behaviour; unknown ints fall through to `System` |

## 12. Future work

- Re-enable dark on Windows 10 if WinForms/uxtheme ever ships a complete Win10 dark palette — the clamp is encapsulated in `ThemeResolver.DarkModeSupported` and is a one-line change.
- Per-surface dark-theme completion on Windows 10 (title bar, ComboBox popups) — tracked in the 2026-09-03 ADR §6 limitations.
- A telemetry event for theme-mode distribution, if wanted later.
