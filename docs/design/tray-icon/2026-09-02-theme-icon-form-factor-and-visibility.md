# Theme-based tray icon — device-aware form factor + better visibility

**Status:** Draft v3 for review — anchored on issue #2417
**Author:** Hermes
**Branch:** `feat/theme-icon-form-factor` (worktree: `/home/balor/workspace/soundswitch-feat-theme-icon`)
**Base:** `origin/dev` @ `c295ac77`
**Issue:** https://github.com/Belphemur/SoundSwitch/issues/2417 ("Dark mode support - Theme Based issues")
**Reporter:** notsobigguyanymore via Answer Overflow

---

## 1. Problem (from issue #2417)

> systray icon - Theme based ... — 1) the icon is not changing from speaker to headphone when i switch playback devices, 2) the speaker icon is small and still hard to see, not enough contrast.

Two distinct defects, both in `IconChangerThemeBased`:

| # | Symptom | Root cause |
|---|---------|------------|
| 1 | Icon stays the same on every default-device switch | `IconChangerThemeBased.ChangeIcon(DeviceFullInfo, ERole)` (`IconChangerThemeBased.cs:36`) is a **no-op**. The `DefaultDeviceChanged` event in `TrayIcon.cs:285-289` fires it on every change — nothing happens. |
| 2 | Icon is small and low-contrast | `SpeakerIconGenerator` renders glyph `U+E767` ("Volume", 1171 bytes — the most visually complex of the candidate glyphs) at `SystemInformation.SmallIconSize.Width` (16×16 on standard DPI) with `RGB(240,240,240)` / `RGB(30,30,30)` greys. Pure anti-pattern for tray-icon visibility. |

The issue also flags a third item (settings window uses old-style chrome, can't scale, no dark mode) — **out of scope** for this PR; tracked as a follow-up.

## 2. Goals

1. **Icon reflects the active device** in theme mode: speaker / headphone / headset for `eRender`; microphone for `eCapture`.
2. **Icon is bigger and crisper** than the current Segoe-glyph approach.
3. **Ship both light and dark variants** — pure black on transparent for light taskbars; pure white on transparent for dark taskbars.
4. **No font dependency.** Drop `SpeakerIconGenerator` and its `Segoe Fluent Icons` / `Segoe MDL2 Assets` lookup entirely.
5. **Theme adaptation stays in scope**: invert to white for dark taskbars, on demand, cached. (WinUI dark-mode is NOT in scope — that is the third item from #2417 and a separate follow-up.)
6. Keep the public `IconChanger` API and existing non-theme changers untouched.

## 3. Non-goals

- **No `IDeviceTopology` / `PKEY_AudioEndpoint_FormFactor` detection.** v1 detection is by `IconPath` string heuristic + `NameClean` fallback — both fields already exist on `DeviceFullInfo`. Real FormFactor is a follow-up.
- **No changes to `IconChangerNone`, `IconChangerPlayback`, `IconChangerRecording`, `IconChangerAlways`.** They keep using `deviceInfo.SmallIcon` from the OS-assigned icon path.
- **No telemetry.** Out of scope.
- **No new build dependencies.** No SVG pipeline, no Pillow script in the repo, no new package references. The existing resx+ico path is the entire mechanism.
- **No WinUI / settings-window dark-mode work** (the third item from #2417). Out of scope — separate follow-up.

## 4. Design

### 4.1 Asset strategy — Lucide icons, ISC licensed, derived PNGs

Pulled from [lucide-icons/lucide](https://github.com/lucide-icons/lucide) (ISC License — Copyright © 2026 Lucide Icons and Contributors). Mapping:

| Form factor | Lucide source | Why |
|---|---|---|
| Speaker (render)    | `volume-2.svg`  | Speaker cone + 2 sound-wave arcs — explicit "speaker producing sound" |
| Headphone (render) | `headphones.svg` | Over-ear headphones, band visible |
| Headset (render)   | `headset.svg`    | Headphones + a mic boom arm curving down |
| Microphone (capture) | `mic.svg`      | Standard mic |

`volume-2` (not `volume`) because the bare `volume` is a speaker with no waves — ambiguous against the "speaker with sound" intent of the render path.

`headset` (not `headphones`) because the boom-arm is the visual discriminator: headphones (consumer music) vs headset (communications/VoIP).

**Rendering pipeline (one-shot, checked in):**
1. Fetch the four SVG sources from `lucide-icons/lucide@main/icons/<name>.svg` (already done; copies in `/tmp/lucide/`).
2. Render each at 4× oversample to PNG via `cairosvg`, then downsample to 16/20/24/32/40/48/64 with Lanczos.
3. Pack into a single multi-resolution ICO file (manual ICONDIR + ICONDIRENTRY headers, one PNG entry per size).

Result: 4 ICO files, total ~25 KB on disk, each contains 7 resolution entries (16 → 64), 32-bit ARGB. Already rendered to `/tmp/icons/` and ready to drop into the worktree.

**Files to be added to `SoundSwitch.Common/Resources/`** — black + white variants shipped as static assets, no runtime inversion:

| File | Source variant | Colour | Size |
|---|---|---|---|
| `themeIcon_speaker.ico`        | volume-2.svg   | Black on transparent | ~5.4 KB |
| `themeIcon_speaker_white.ico`  | volume-2.svg   | White on transparent | ~6.4 KB |
| `themeIcon_headphone.ico`      | headphones.svg | Black on transparent | ~6.4 KB |
| `themeIcon_headphone_white.ico`| headphones.svg | White on transparent | ~7.2 KB |
| `themeIcon_headset.ico`        | headset.svg    | Black on transparent | ~7.0 KB |
| `themeIcon_headset_white.ico`  | headset.svg    | White on transparent | ~7.8 KB |
| `themeIcon_microphone.ico`     | mic.svg        | Black on transparent | ~6.0 KB |
| `themeIcon_microphone_white.ico`| mic.svg       | White on transparent | ~6.6 KB |

The white variants are pre-rendered once from the same SVGs (RGB-inverted via `ImageOps.invert` on the rendered PNGs, before ICO packing). Runtime selects between the two based on `WindowsThemeHelper.IsDarkModeEnabled()` — same pattern the current `SpeakerIconGenerator` already uses, just over real PNGs in ICO containers instead of a Segoe glyph.

### 4.2 Theme picker

No runtime `ColorMatrix`, no lazy cache. `ThemeIcons` is a thin static lookup:

```csharp
public static Icon GetIcon(IconKind kind, bool isDarkTaskbar)
{
    var handle = isDarkTaskbar ? _white[(int)kind] : _black[(int)kind];
    return handle.Icon;
}
```

Backed by 8 `static readonly IconHandle _black[4]` / `_white[4]` created via `IconExtractor.CreatePermanent` (same pattern as `AudioDeviceIconExtractor.DefaultSpeakersHandle`). Permanent handles are never disposed; the cache is alive for the application lifetime.

### 4.3 Form-factor detection (Option A+B from earlier analysis)

A new internal helper, `DeviceFormFactorDetector.From(DeviceFullInfo)`, returns one of `Speaker | Headphone | Headset | Microphone`.

Priority order:

1. **IconPath heuristic** — match `deviceInfo.IconPath` against a small map of `mmres.dll` indexes (the icon DLL Windows uses for audio endpoints). `eCapture` short-circuits to `Microphone`.

   | mmres.dll index | Meaning                | Map to    |
   |----------------:|------------------------|-----------|
   | `-5004`         | Speakers               | Speaker   |
   | `-5005`         | Headphone              | Headphone |
   | `-5051`         | Headset                | Headset   |
   | `-5044`         | Communications headset | Headset   |
   | `-5006`         | Microphone             | Microphone |
   | `-5052`         | Headset (variant)      | Headset   |

   Match by `Path.GetFileName(path) + ',' + index`. If empty / no match → step 2.

2. **NameClean heuristic** — regex against `DeviceFullInfo.NameClean`. Case-insensitive. Order matters: scan Headphone first.

   ```
   \b(headphone|headset|earbud|earphone|airpods|qc\d|wh-\d)\b  →  Headphone
   \b(headset|game[ ]?com)\b                                      →  Headset
   ```

   Examples: "Bose Headset 700" → Headset (Headset regex matches first); "Sony WH-1000XM4" → Headphone; "AirPods Pro" → Headphone; "Realtek HD Audio" → no match → fallback.

3. **Default** — `Speaker` for `eRender`, `Microphone` for `eCapture`. Safe, never wrong.

### 4.4 New `IconChangerThemeBased` body

```csharp
public void ChangeIcon(UI.Component.TrayIcon trayIcon)
    => trayIcon.ReplaceIcon(ThemeIcons.GetIcon(IconKind.Speaker, WindowsThemeHelper.IsDarkModeEnabled()).Icon);

public void ChangeIcon(UI.Component.TrayIcon trayIcon, DeviceFullInfo deviceInfo, ERole role)
{
    if (role == ERole.eCommunications) return;       // matches IconChangerAbstract's guard
    var kind = DeviceFormFactorDetector.From(deviceInfo);
    trayIcon.ReplaceIcon(ThemeIcons.GetIcon(kind, WindowsThemeHelper.IsDarkModeEnabled()).Icon);
}
```

`ThemeIcons` is a new internal static class. `IconKind` is an internal enum:

```csharp
internal enum IconKind { Speaker, Headphone, Headset, Microphone }
```

### 4.5 Resources.resx wiring

Add 4 new `data` entries to `SoundSwitch.Common/Properties/Resources.resx` modelled on the existing `defaultMicrophone` entry (line 121-124). `Resources.Designer.cs` is auto-regenerated by the build's `PatchLocalizedResourceDesigners` prebuild (per `soundswitch-dev` §5, the regenerated file is committed).

### 4.6 What gets removed

The entire `SpeakerIconGenerator.cs` static font-cache / `GetAvailableIconFont` / Segoe glyph rendering logic. Gone. No Segoe font dependency. No runtime rendering. No `TextRenderingHint.AntiAliasGridFit`.

(`SpeakerIconGenerator` is currently only consumed by `IconChangerThemeBased` — confirmed by grep.)

### 4.7 License / attribution

Lucide is ISC-licensed; the LICENSE text must be included with the source tree.

- Add `SoundSwitch.Common/Resources/LICENSES/Lucide-ISC.txt` containing the verbatim Lucide ISC license.
- Add a credit row to **`README.md`** and **`README.de.md`** in the existing "## Thanks / ### Credits" section, matching the format of the Font Awesome entry already there.
- Add a corresponding credit line to **`Installer/LICENSE.md`** if the installer renders it (verify during implementation).
- A code comment at the top of `Resources.resx` or `AudioDeviceIconExtractor.cs` noting "Lucide-derived icons — see LICENSES/Lucide-ISC.txt" is optional but worth doing for greppability.

### 4.8 Backward compatibility

- `IIconChanger` interface unchanged.
- All other changers unchanged.
- Existing `ReplaceIcon(Icon)` on `TrayIcon` unchanged.
- `IconChangerFactory` unchanged.
- All existing `IconChangerThemeBased` callers keep working. The `ChangeIcon(DeviceFullInfo, ERole)` overload's previous "do nothing" behaviour was itself a bug — the icon should have updated on default-device switches. After this fix it does.

## 5. Tests

`SoundSwitch.Tests/DeviceFormFactorDetectorTests.cs` (NUnit — confirmed via `AudioDeviceIconExtractorTests.cs`):

- `From_RenderEmptyIconPath_ReturnsSpeaker`
- `From_RenderMmres5005_ReturnsHeadphone`
- `From_RenderMmres5051_ReturnsHeadset`
- `From_CaptureMmres5006_ReturnsMicrophone`
- `From_RenderNameWithHeadphone_ReturnsHeadphone`
- `From_RenderNameWithHeadset_ReturnsHeadset`
- `From_RenderNameWithAirPods_ReturnsHeadphone`
- `From_RenderNameWithRealtek_ReturnsSpeaker`
- `From_BoseHeadsetName_ReturnsHeadset`
- `From_NullDevice_ReturnsSpeaker`

`SoundSwitch.Tests/ThemeIconsTests.cs` (new) — covers the lookup + load:

- `GetIcon_Speaker_LightAndDark_Differ`     (sanity: both variants load)
- `GetIcon_SpeakerAndHeadphone_Differ`      (sanity: kind matters)
- `GetIcon_AllFourKinds_LoadSuccessfully`    (smoke: every IconKind produces a non-null Icon)

No tests for visual quality — manual review on the PR.

## 6. File-by-file change list

| File | Change |
|---|---|
| `SoundSwitch.Common/Resources/themeIcon_speaker.ico`        | **NEW** — Lucide `volume-2`, 7 sizes, black |
| `SoundSwitch.Common/Resources/themeIcon_speaker_white.ico`  | **NEW** — Lucide `volume-2`, 7 sizes, white |
| `SoundSwitch.Common/Resources/themeIcon_headphone.ico`      | **NEW** — Lucide `headphones`, 7 sizes, black |
| `SoundSwitch.Common/Resources/themeIcon_headphone_white.ico`| **NEW** — Lucide `headphones`, 7 sizes, white |
| `SoundSwitch.Common/Resources/themeIcon_headset.ico`        | **NEW** — Lucide `headset`, 7 sizes, black |
| `SoundSwitch.Common/Resources/themeIcon_headset_white.ico`  | **NEW** — Lucide `headset`, 7 sizes, white |
| `SoundSwitch.Common/Resources/themeIcon_microphone.ico`     | **NEW** — Lucide `mic`, 7 sizes, black |
| `SoundSwitch.Common/Resources/themeIcon_microphone_white.ico`| **NEW** — Lucide `mic`, 7 sizes, white |
| `SoundSwitch.Common/Resources/LICENSES/Lucide-ISC.txt`  | **NEW** — verbatim ISC license from upstream |
| `SoundSwitch.Common/Properties/Resources.resx`          | Add 8 `data` entries + `metadata` (auto) |
| `SoundSwitch.Common/Properties/Resources.Designer.cs`   | Auto-regenerated by `PatchLocalizedResourceDesigners` |
| `SoundSwitch/Framework/TrayIcon/IconKind.cs`            | **NEW** — internal enum |
| `SoundSwitch/Framework/TrayIcon/ThemeIcons.cs`          | **NEW** — 8 permanent `IconHandle`s (4 black + 4 white) + `GetIcon(IconKind, bool)` |
| `SoundSwitch/Framework/TrayIcon/DeviceFormFactorDetector.cs` | **NEW** — internal static, `From(DeviceFullInfo)` |
| `SoundSwitch/Framework/TrayIcon/SpeakerIconGenerator.cs`     | **DELETE** — superseded by `ThemeIcons` |
| `SoundSwitch/Framework/TrayIcon/IconChanger/Changer/IconChangerThemeBased.cs` | Implement `ChangeIcon(DeviceFullInfo, ERole)` |
| `SoundSwitch.Tests/DeviceFormFactorDetectorTests.cs`         | **NEW** — see §5 |
| `SoundSwitch.Tests/ThemeIconsTests.cs`                      | **NEW** — see §5 |
| `README.md`                                                  | Add Lucide credit line in "## Thanks" |
| `README.de.md`                                               | Add Lucide credit line in "## Thanks" |
| `Installer/LICENSE.md`                                       | Add Lucide credit line if applicable |

## 7. Build & merge

- **Linux partial build only** — `dotnet build SoundSwitch/SoundSwitch.csproj -p:LinuxBuild=true -p:BuildProjectReferences=false`. Per `soundswitch-dev` §5a, this won't catch missing `using`s or `Resources.Designer.cs` regressions — namespace-audit every edited file before claiming done.
- **CI gate.** PR against `dev` (`gh pr create --base dev`). `build / build` must pass.
- **Merge gate.** `scripts/pr-merge-gated.sh` after CI green.

## 8. Risks and mitigations

| Risk | Mitigation |
|---|---|
| `mmres.dll` index conventions change between Windows builds | Fallback chain ends at NameClean → safe default. Log a Warning on first hit of an unknown index. |
| ICO assets fail to load (corrupt / wrong sizes / asset missing in release build) | The existing `CreatePermanentDefaultIcon` pattern wraps load in try/catch and falls back to `SystemIcons.Application` / `Information`. Use the same fallback chain. |
| `IconKind` enum naming conflicts | Internal enum in `SoundSwitch.Framework.TrayIcon` namespace, low blast radius. |
| OpenCode touches non-theme changers | Brief will say: edit ONLY files in §6. No `IconChangerAbstract`, `IconChangerPlayback`, etc. |
| Linux partial build passes but Windows CI fails on missing `using` | Namespace audit before claiming done. |
| LICENSE attribution missing | §4.7 spells out the exact files. Brief will list them. |
| White ICOs are visually slightly different from a hand-tuned white asset | Acceptable — Lucide's stroke icons have clean anti-aliasing and `ImageOps.invert` on the rendered PNGs produces a faithful white-on-transparent. Manual review on the PR confirms. |
| `Installer/LICENSE.md` doesn't actually exist in the final installer build | Verify during implementation; if it's stripped or auto-generated, update the source `Installer/LICENSE.md` and any Inno Setup `.iss` file referencing it. |

## 9. Out of scope (follow-up issues)

- Real `PKEY_AudioEndpoint_FormFactor` via `IDeviceTopology`. For devices with an `Unknown` icon path AND unhelpful name.
- Apply the same visibility bump to `IconChangerPlayback` / `Recording` / `Always` (OS-assigned icons at 32+ px).
- **#2417 third item**: settings window uses old-style chrome, can't scale, no dark mode (WinUI work).

---

**Review checklist for Nidoros:**

- [ ] Asset mapping: Lucide `volume-2` → Speaker, `headphones` → Headphone, `headset` → Headset, `mic` → Microphone. OK?
- [ ] Ship 8 ICOs (4 kinds × 2 colour variants) as static assets; runtime picks via `WindowsThemeHelper.IsDarkModeEnabled()`. OK?
- [ ] Form-factor detection: IconPath heuristic → NameClean regex → safe default. OK to ship without topology-API detection?
- [ ] `role == eCommunications` guard: matches `IconChangerAbstract` behaviour. OK?
- [ ] Drop `SpeakerIconGenerator.cs` entirely. OK?
- [ ] License/attribution: README, README.de, Installer/LICENSE.md, Resources/LICENSES/Lucide-ISC.txt. All four needed?
- [ ] Test plan in §5 sufficient?
- [ ] Follow-up issues for: real FormFactor detection, theme-icon bump for other changers, #2417 settings window.