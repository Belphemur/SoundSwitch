# WinForms dark theme — make the 6 settings forms follow Windows

**Status:** Draft v1 for review
**Author:** Hermes
**Branch:** `feat/ui-dark-theme`
**Base:** `origin/dev`
**Issue:** Resolves the third item of https://github.com/Belphemur/SoundSwitch/issues/2417 ("settings window ... does not follow dark mode") — the first two items were fixed by PR #2418.

---

## 1. Problem

Issue #2417 reported three defects. PR #2418 fixed two of them (the tray icon now reflects device form factor and uses bundled Lucide ICOs for visibility). The third item is still open:

> "Also the setting window, is using the windows old style window, so the size can not be scaled and it does not follow dark mode"

The "old style window" complaint refers to the pre-WinUI standard WinForms chrome: title bar, borders, controls — all rendered in classic light colours regardless of the Windows app-mode. A user running SoundSwitch on Windows 11 with system-wide dark mode sees a glaring white settings panel; on Windows 10 the same complaint appears whenever they enable the dark taskbar.

**Why now, not 3 months ago:** SoundSwitch's TFM is `net10.0-windows10.0.17763.0` (`SoundSwitch/SoundSwitch.csproj:11`). .NET 10 promoted `Application.SetColorMode(SystemColorMode)` from experimental (`WFO5001` opt-in) to **stable**, and added the `ApplyThemingImplicitly` `ControlStyles` flag for opt-in/opt-out per-control (Microsoft Learn, "What's new in Windows Forms for .NET 10"). This means the heavy lifting ships with the framework — no third-party `DarkNet`/`WindowsAPICodePack` library required, no per-control `BackColor` overrides for the common cases.

## 2. Goals

1. All six WinForms (`SettingsForm`, `About`, `ProcessSelectionForm`, `UpdateDownloadForm`, `UpsertAppSoundLockRule`, `UpsertProfileExtended`) follow the OS dark/light setting automatically — no app-level toggle, no user-facing setting to add.
2. Dark mode flips live when the user toggles the OS theme while the app is running (the `WindowsAPIAdapter.SystemThemeChanged` plumbing already exists; this PR reuses it).
3. `BannerForm` is **not** touched (per user directive). Banner styling stays as-is. (`BannerForm` is a WinForms `Form` class — the directive applies to its theming treatment, not its class hierarchy.)
4. No visual regression on Windows 10 light mode (the minimum supported OS). No new build warnings (`WFO5001` was the only one and is no longer relevant on .NET 10).
5. No new package dependencies. No designer-file regeneration needed (the existing forms inherit the theme via framework defaults).

## 3. Non-goals

- No WinUI rewrite. The forms stay WinForms. (A WinUI swap is a separate, larger effort — out of scope.)
- No per-user "Force light / Force dark / Follow system" preference UI. The OS theme is the source of truth. (Adding a preference toggle is a separate follow-up if users ask.)
- No BannerForm changes. The banner is a custom-drawn translucent overlay — not chrome-themed like the forms above. Its appearance is intentionally independent of the system theme.
- No telemetry. Out of scope.
- No RTL regression. The `RightToLeft` wiring (`new LanguageFactory().Get(...).IsRightToLeft ? RightToLeft.Yes : RightToLeft.No`) stays as-is.

## 4. Affected files (verified by grep, 2026-09-03)

| File | Status | Notes |
|---|---|---|
| `SoundSwitch/UI/Forms/Settings.cs` (1433 LOC) | Touched | Custom-painted `RectOutline` borders in `OnPaint`; we map `PenLine` colour to follow the theme. |
| `SoundSwitch/UI/Forms/About.cs` | Touched (construct only) | No custom paint — framework default is enough. |
| `SoundSwitch/UI/Forms/ProcessSelectionForm.cs` | Touched (construct only) | `DataGridView` needs explicit theming (see §6.2). |
| `SoundSwitch/UI/Forms/UpdateDownloadForm.cs` | Touched (construct only) | `TextProgressBar` is owner-drawn — verify it paints correctly in dark mode. |
| `SoundSwitch/UI/Forms/UpsertAppSoundLockRule.cs` | Touched (construct only) | No custom paint. |
| `SoundSwitch/UI/Forms/UpsertProfileExtended.cs` | Touched (construct only) | `IconTextComboBox` is `OwnerDrawFixed` — verify it paints correctly. |
| `SoundSwitch/Framework/WinApi/WindowsThemeHelper.cs` | Reused as-is | `IsDarkModeEnabled()` already correct. |
| `SoundSwitch/Framework/WinApi/WindowsAPIAdapter.cs` | Reused as-is | `SystemThemeChanged` already wired. |
| `SoundSwitch/Program.cs` | Touched | `Application.SetColorMode(SystemColorMode.System)` added before `Application.Run`. |
| `SoundSwitch/UI/Component/IconTextComboBox.cs` | Touched (small) | Owner-draw list item must honour dark theme. |
| `SoundSwitch/UI/Component/TextProgressBar.cs` | Touched (small) | Owner-drawn progress bar must honour dark theme. |
| `SoundSwitch/Framework/Banner/BannerForm.cs` | **NOT TOUCHED** | Per user directive. |

`RoundedCorner.cs` (in `SoundSwitch.UI.Menu`) already calls `DwmSetWindowAttribute` — that's the `DWMWA_WINDOW_CORNER_PREFERENCE`, not the dark-mode attribute; it stays as-is.

## 5. Design

### 5.1 Application-level color mode (the core change)

A single line in `Program.cs`, placed **after** `Application.EnableVisualStyles()` and **before** `Application.Run(appContext)`:

```csharp
Application.SetColorMode(SystemColorMode.System);
```

`SystemColorMode.System` (enum value `1`) makes every form created after this call follow the current OS `AppsUseLightTheme` registry value. `SystemColorMode.Dark` (enum value `2`) forces dark; `Classic` (enum value `0`) keeps the old behaviour. We pick `System` because the user's stated requirement is "follow the theme of Windows".

This automatically:
- Repaints every standard control (Button, CheckBox, RadioButton, TextBox, ComboBox, GroupBox, TabPage, Label, LinkLabel, NumericUpDown, TrackBar, ToolStrip, MenuStrip, ContextMenuStrip, DataGridView, ListView in details mode, StatusStrip, ProgressBar).
- Sets the DWM `DWMWA_USE_IMMERSIVE_DARK_MODE` attribute on the top-level window (Win11 only; on Win10 the title bar stays light — see §7 limitations).

The change is opt-in only at one place; per-form code changes are limited to (a) wiring the live theme-change handler and (b) the two owner-drawn custom controls.

### 5.2 Live theme-change reaction

`WindowsAPIAdapter` already raises `SystemThemeChanged` on `WM_SETTINGCHANGE` when `IsImmersiveColorSetChange` is true (`WindowsAPIAdapter.cs:392-398`). Today, `TrayIcon.cs:309` subscribes and calls `UpdateIcon()` for theme-aware tray icons.

We add a parallel hook: when `SystemThemeChanged` fires and the SettingsForm is open, we call `SettingsForm.RefreshTheme()` (new method). The subscription is created inside the `SettingsForm` constructor (paired with `_deviceListRefreshedSubscription`) and disposed in `OnFormClosed`, matching the existing form-internal subscription pattern.

**Important caveat — what `SetColorMode(SystemColorMode.System)` does and doesn't do at runtime:**

- **At startup** (the first call): every subsequently-created form and control reads the current OS `AppsUseLightTheme` registry value and renders with the matching palette. This is the primary mechanism; it covers all six forms automatically with no per-control code.
- **On OS theme change** (live flip): `SetColorMode` does NOT propagate the new colour to existing standard controls on its own — Microsoft documents this as "the application does not automatically adapt" ([Microsoft Learn — `SetColorMode`](https://learn.microsoft.com/en-us/dotnet/api/system.windows.forms.application.setcolormode)). However, WinForms raises `OnSystemColorsChanged` (and the newer `OnSystemVisualSettingsChanged`) on every control when the OS theme flips, so controls that subscribe to those events can repaint themselves.
- **Our strategy**: `SettingsForm` overrides `OnSystemColorsChanged` and calls `RefreshTheme()` (form-wide `Invalidate(true)` + per-child `Invalidate()`). The custom-painted border (`PenLine` → `OutlineColor`) and the two owner-drawn custom controls (`IconTextComboBox`, `TextProgressBar`) repaint from their new theme-aware brushes. Standard controls are expected to repaint themselves via the framework's built-in handlers — if any standard control is observed to stay in the old colour after a live flip, the fix is targeted `Invalidate()` calls in `RefreshTheme()` (no form restart needed).
- **Why we keep the existing `WindowsAPIAdapter.SystemThemeChanged` hook**: the API adapter fires on `WM_SETTINGCHANGE` for any immersive-colour change (accent, contrast, hot tracking). `OnSystemColorsChanged` fires on `WM_SYSCOLORCHANGE`. Both paths reach `RefreshTheme()` so we don't double-fire on a single user toggle (the framework coalesces, but if it doesn't we can guard with a timestamp).

A future improvement (out of scope) is to call `Application.SetColorMode(...)` again from `RefreshTheme()` with the live mode and then invalidate — Microsoft documents this works for some controls but inconsistently across `WM_PAINT`-driven custom paint. We accept the current strategy's known limitations on live flip; for users who want a 100% clean theme they can close and reopen Settings (loses no persisted state).

### 5.3 Custom-painted borders in SettingsForm

`Settings.cs:82-87` defines:

```csharp
private static Pen PenLine(int width = 1) => new(Color.Gainsboro, width);
private static Rectangle RectOutline(int offsetW, int offsetH, Control topLeft, Control bottomRight) => ...;
```

`Gainsboro` is hardcoded for the outline border on the playback/recording/profiles preview pane. In dark mode that produces a near-invisible border on a dark background. Replace with a theme-aware colour:

```csharp
private static Color OutlineColor => WindowsThemeHelper.IsDarkModeEnabled()
    ? Color.FromArgb(80, 80, 80)   // dark grey on dark bg
    : Color.Gainsboro;
private static Pen PenLine(int width = 1) => new(OutlineColor, width);
```

Plus override `OnSystemColorsChanged` on `SettingsForm` so the outline repaints when the OS theme flips. (Standard `OnPaint` re-reads `PenLine` because it's a new instance per call — but `PenLine` allocates a new `Pen` each call; that's already a minor allocation issue today, separate from this PR. We leave the allocation pattern untouched to keep the diff minimal.)

### 5.4 Custom owner-drawn controls (`IconTextComboBox`, `TextProgressBar`)

Both extend base WinForms controls and disable the framework's automatic drawing. In .NET 10 they need to opt into theming for the parts they draw themselves:

**`IconTextComboBox`** (DrawMode.OwnerDrawFixed dropdown items): override `CreateParams` and call `SetStyle(ControlStyles.ApplyThemingImplicitly, true)` before the base params are read, per the .NET 10 docs. The combo-box *box* (closed state) is rendered by the framework and follows the theme automatically; the *dropdown items* are owner-drawn — we need to verify on Windows CI that the dropdown picks up the dark background. If it doesn't, the override is a per-item `e.Graphics.Clear(...)` switch keyed on `WindowsThemeHelper.IsDarkModeEnabled()` (draw item dark grey background for `e.State.HasFlag(DrawItemState.Selected)`).

**`TextProgressBar`** (UserPaint flag set in constructor): same `CreateParams` opt-in. Its `OnPaint` (`TextProgressBar.cs:60+`) calls `ProgressBarRenderer.DrawHorizontalBar(g, rect)` — this is theme-aware by default but on dark backgrounds the rendered chunk colour is the system accent, which is usually fine. We add an `OnSystemColorsChanged` override that calls `Invalidate()` so the chunk bar re-renders.

### 5.5 What does NOT change

- `BannerForm.cs` — explicit directive.
- `WindowsThemeHelper.IsDarkModeEnabled()` semantics (registry lookup for `Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\SystemUsesLightTheme`).
- `WindowsAPIAdapter.SystemThemeChanged` event semantics.
- The tray icon (`IconChangerThemeBased`) — already handles its own theme selection via the existing `SystemThemeChanged` subscription.
- Designer files (`.Designer.cs`) — no changes needed; everything is wired at runtime.
- Localization (`.resx` strings) — no new strings; the OS provides the colours.
- Existing custom control surfaces (the tray-context-menu, the quick-menu in `SoundSwitch.UI.Menu`) — these are ContextMenuStrips and inherit the theme automatically.

## 6. Known limitations (Windows 10 vs Windows 11)

From Microsoft Learn and dotnet/winforms issues (verified by web search 2026-09-03):

- **Windows 11 only**: DWM `DWMWA_USE_IMMERSIVE_DARK_MODE` (attribute 19) is honoured. Title bar, minimise/maximise/close buttons, and most `UxTheme` controls render dark.
- **Windows 10 1903+**: `SetColorMode(System)` recolours the **content of standard controls** (Button, CheckBox, ComboBox, TabControl, etc.) and respects the system palette, but the **title-bar chrome stays light** because the Win10 DWM does not honour `DWMWA_USE_IMMERSIVE_DARK_MODE`. This is a Windows 10 DWM limitation — there's no documented workaround that doesn't break the title bar on Win11. **We accept this for the Win10 minimum-supported-platform path** and document it in the user-facing release notes. Custom-drawn controls (`IconTextComboBox`, `TextProgressBar`, `SettingsForm`'s outline border) follow the OS theme on both Win10 and Win11 because they paint with our theme-aware brushes.
- **High Contrast mode**: out of scope for this PR. `WindowsThemeHelper.IsDarkModeEnabled()` reads the bare `AppsUseLightTheme` registry value; if the user is in High Contrast AND the system is also dark, the helper still returns `true`, so our custom brush colours will render dark on the HC palette. This is acceptable — HC is its own palette and a separate theming exercise. Future improvement: add an HC guard (`SystemParameters.HighContrast` → force light brush).
- **ListView "Details" view on dark theme**: ListView's header strip and row highlight colour are system-derived and work in .NET 10 dark mode. The custom `profilesListView` / `playbackListView` / `recordingListView` in Settings (`Settings.cs:308, 425-444`) use the default ListView; no code change needed, but verify on Windows CI.
- **`ComboBox` dropdown popup** (separate window from the closed box): the popup's background is rendered by the OS listbox control, which honours dark mode in Win11 22H2+. Pre-22H2 Win11 and all Win10 keep the dropdown light. Documented in the .NET 10 release notes.

## 7. Risk register

| # | Risk | Likelihood | Mitigation |
|---|---|---|---|
| 1 | `Application.SetColorMode` is still flagged as experimental in some intermediate SDK build | Low | CI builds on `windows-latest` with .NET 10 GA SDK (the maintainer's pinned version per `.github/workflows/build.yml`). Verify before merge. |
| 2 | A third-party control inside one of the six forms ignores the theme | Low | All 6 forms use stock WinForms controls + 2 in-repo custom controls (covered in §5.4). No third-party UI libraries are referenced by the forms. |
| 3 | Live theme-change repaint leaves artefacts on SettingsForm | Medium | `RefreshTheme()` calls `Invalidate(true)` then `Update()` to force a full repaint. If visible tearing is reported, fall back to close-and-reopen (loses form state — last resort, not the v1 path). |
| 4 | BannerForm visibility when Settings is open | None | BannerForm is independent — they don't share rendering surfaces. |
| 5 | `DarkMode` regression on existing CI screenshot tests | Low | SoundSwitch.Tests project (`SoundSwitch.Tests/`) is xUnit + Moq; no screenshot tests today. |
| 6 | Localization key count delta from new strings | None | No new strings. The OS provides all colours. |

## 8. Validation

**Cannot use Linux** (CsWinRT prebuild blocks `dotnet build SoundSwitch/SoundSwitch.csproj` on this host per AGENTS.md §5 / soundswitch-dev skill). The only compile gate is Windows CI `build / build`. Local validation sequence:

1. `git diff origin/dev..HEAD --stat` — confirm only the 11 files in §4 were touched (no scope drift).
2. `git diff origin/dev..HEAD -- SoundSwitch/UI/Forms/*.Designer.cs` — must be EMPTY. Designer files are off-limits per UI AGENTS.md.
3. `git log --oneline origin/dev..HEAD` — verify a single conventional-commit message referencing issue #2417.
4. **Namespace audit** (the load-bearing check from soundswitch-dev §5a — Linux can't catch CS0246 here):
   ```bash
   grep -nE 'Application\.|SetColorMode|SystemColorMode|Invalidate|RefreshTheme' \
     SoundSwitch/Program.cs \
     SoundSwitch/UI/Forms/Settings.cs \
     SoundSwitch/UI/Component/IconTextComboBox.cs \
     SoundSwitch/UI/Component/TextProgressBar.cs
   ```
   Verify every `Application.*` call has the `System.Windows.Forms` namespace reachable. Every form file already has `using System.Windows.Forms;` — confirmed during exploration.
5. Windows CI `build / build` is the authoritative gate. After push, monitor `gh pr checks`.

**Manual smoke** (Windows VM, dev-only — not CI-runnable):
- Settings → all six forms visible in both Win10 light and Win11 dark.
- Toggle Windows theme live with SettingsForm open → SettingsForm repaints.
- Open each form, close, reopen → no leaks, no stale colours.
- Tab through all controls with keyboard → no contrast regressions.

## 9. PR plan

One PR, `feat(ui): follow Windows dark theme on all settings forms (#2417, third item)`. Body references issue #2417, the prior #2418 PR, and the .NET 10 dark-mode stabilisation note from Microsoft Learn. Branch: `feat/ui-dark-theme` from `origin/dev`. Worktree lives under the active Hermes workspace per the soundswitch-dev §4 convention.

Commit message:
```text
feat(ui): follow Windows dark theme on WinForms settings

Resolves the third item of #2417. SoundSwitch already targets
net10.0-windows10.0.17763.0; .NET 10 promoted
Application.SetColorMode(SystemColorMode) from experimental to stable
so no new package references are needed.

- Program.cs: Application.SetColorMode(SystemColorMode.System)
  before Application.Run so all subsequently-created forms honour the
  OS AppsUseLightTheme registry value.
- SettingsForm: theme-aware PenLine outline colour + OnSystemColorsChanged
  re-paint + RefreshTheme() hook on WindowsAPIAdapter.SystemThemeChanged.
- IconTextComboBox / TextProgressBar: opt into
  ControlStyles.ApplyThemingImplicitly so the owner-drawn surfaces
  inherit the system palette.
- Subscribe SoundSwitchApplicationContext to SystemThemeChanged so an
  already-open SettingsForm repaints when the OS theme flips.

BannerForm is intentionally untouched per the maintainer's standing
directive. Windows 10 still renders the title-bar chrome in light
mode (DWM limitation; documented in the design doc §6).
```

Conventional-commit type is `feat` (confirmed by maintainer during review) because this is effectively a UI rewrite — every settings form is re-skinned, and from the user's perspective the WinForms chrome they see today is replaced with native dark controls. That justifies a minor release per the repo's semantic-release config.

**Maintainer decisions (received during review)**

1. **Conventional-commit type: `feat` (→ minor release).** Rationale: this is effectively a UI rewrite — every settings form is re-skinned. From the user's perspective it's a fully new surface (the WinForms chrome they see today is replaced with native dark controls), so a minor bump is the honest signal. Final commit message: see §9.
2. **No per-app "always light / always dark / system" preference in this PR.** The OS theme is the source of truth. Can be added as a follow-up if users ask.
3. **`BannerForm` stays untouched for this PR.** The banner is a custom-drawn translucent overlay, intentionally independent of the system theme (it's a notification, not chrome). If a future user reports banner readability in dark mode, that's a separate decision.