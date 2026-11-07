# Per-monitor DPI awareness + a resizable app-rules editor

**Status:** Implemented on `fix/2570-app-rules-dialog-scaling`
**Author:** Hermes
**Branch:** `fix/2570-app-rules-dialog-scaling`
**Base:** `origin/dev`
**Issue:** [Belphemur/SoundSwitch#2570](https://github.com/Belphemur/SoundSwitch/issues/2570) - App rules dialog does not properly scale
**PR:** [#2572](https://github.com/Belphemur/SoundSwitch/pull/2572)
**Related:** `docs/design/ui/2026-09-14-startup-order-setcolormode-fix.md` (startup order; partially superseded — see 6.1), `docs/design/app-rules/2026-11-03-app-rules-picker-own-device-list.md`

---

## 1. Problem

On a display scaled to 175%, the add/edit dialog for app-specific rules
(`SoundSwitch/UI/Forms/UpsertAppSoundLockRule.cs`) renders at its 96-DPI design
size — unusably small — and `FixedDialog` (`MaximizeBox = false`) gives the user
no resize workaround.

Two independent defects:

1. **Process level.** The application is never actually per-monitor
   DPI-aware. `Program.OldMain` calls `SetProcessDPIAware()` (`Program.cs:128`)
   and then `Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)`
   (`Program.cs:132`). Empirically (see 5), the first call establishes
   **System** awareness, which makes the second call a no-op that silently
   reports success: the effective mode stays `SystemAware`. Under mixed-DPI
   multi-monitor or per-monitor scaling, no form is rescaled by the
   framework.
2. **Form level.** Even where the framework scales controls, this dialog uses
   absolute positions with no `Anchor` wiring (except `lblPairCount`) and no
   `MinimumSize`, so nothing survives the user enlarging the window, and a
   DPI scale-up cannot reflow it.

## 2. Goals

1. Establish **PerMonitorV2** awareness at process creation, race-free and with a
   single source of truth (the app targets `net10.0-windows` only; `#if NETCORE`
   is always defined).
2. Make the app-rules editor scale correctly at any DPI setting and allow
   the user to resize it (with a sensible minimum) as a workaround for
   layouts we cannot fully control.
3. Meet the minimum screen-reader contract: image-only buttons expose
   accessible names.

## 3. Non-goals

- Auditing/fixing every other form in the app (banners, quick menu, profile
  upsert) — listed as future work; list in section 12.
- Persisting user-selected dialog sizes across sessions.
- Runtime per-monitor rescaling of every already-open window (WinForms gives
  best-effort; validating that is part of the manual plan).
- Dark-mode/theme behaviour — untouched.

## 4. UX

- The app-rules dialog keeps its visual design; it gains a **sizable
  border** (resize handles, `MaximizeBox = false`, `MinimizeBox = false`) and
  a `MinimumSize` equal to the 96-DPI design size.
- When resized: text boxes, combo boxes, the hotkey group, the pair list and
  the description follow the right/bottom edges through anchors; the Save /
  Cancel buttons and the checkbox row stay pinned to the bottom-right /
  bottom-left.
- Screen readers announce the "…" process picker and the three reset buttons
  by name instead of silence.

## 5. Investigation (verified on .NET 10, Microsoft.WindowsDesktop.App 10.0)

A probe app reproduced each startup ordering against
`Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)` and the Win32 APIs:

| Scenario | `GetProcessDpiAwareness` | Effective `Application.HighDpiMode` |
|---|---|---|
| Manifest declares PMv2 (nothing else) | 2 (per-monitor) | `PerMonitorV2` |
| `SetProcessDPIAware()` P/Invoke, then `SetHighDpiMode(PMv2)` | 1 (system) | `SystemAware` — **no exception, silent downgrade** |
| Hidden background form + `SetProcessDPIAware()` + `SetHighDpiMode(PMv2)` (exact `Program.cs` order) | 1 (system) | `SystemAware` |
| Hidden background form + manifest, **no** P/Invoke | 2 (per-monitor) | `PerMonitorV2` |

Conclusions:

- `SetHighDpiMode` cannot elevate a process that is already DPI-aware (the
  awareness context is set once), and it does not throw — the bug is silent.
- A manifest declaration applies at process creation, before any thread or
  window exists, so the `WindowsAPIAdapter` hidden-form race
  (`docs/design/ui/2026-09-14-startup-order-setcolormode-fix.md` §2.2) cannot
  defeat it.
- The legacy `SetProcessDPIAware()` P/Invoke must be removed: it *downgrades*
  a manifest-declared PMv2 process back to System awareness.
- Manifest-declared PMv2 is honoured by .NET (Core) — the declaration is an OS-level
  process property, not a framework one. (The app is single-target
  `net10.0-windows`; the `#if NETCORE` guards in `Program.cs` are always compiled.)
- The WinForms analyzer WFO0003 recommends configuring DPI via
  `Application.SetHighDpiMode`/`ApplicationHighDpiMode`. Both are unusable here:
  the API is what silently degrades to SystemAware (rows 2–3), and the MSBuild
  property only surfaces when the generated `ApplicationConfiguration.Initialize()`
  is called, which this repo's manual startup does not. The project therefore
  suppresses WFO0003 with a comment pointing at this ADR.

## 6. Design

### 6.1 Process level — manifest-authoritative DPI awareness

`app.manifest` gains:

```xml
<asmv3:application xmlns:asmv3="urn:schemas-microsoft-com:asm.v3">
  <asmv3:windowsSettings>
    <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true/pm</dpiAware>
    <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness>
  </asmv3:windowsSettings>
</asmv3:application>
```

`dpiAware=true/pm` is the Vista fallback; `dpiAwareness=PerMonitorV2` wins
where the SMI/2016 schema is understood (Win 8.1+ effectively, Win 10+ for
V2 semantics).

`Program.cs` loses both runtime calls:

- the `SetProcessDPIAware` `[DllImport]` and its invocation (it would
  downgrade the manifest), and
- `Application.SetHighDpiMode(HighDpiMode.PerMonitorV2)` (now redundant —
  keeping two sources of truth re-opens the silent-conflict class of bug).

This supersedes steps 5 and 7 of the startup order documented in
`2026-09-14-startup-order-setcolormode-fix.md` §3.1; that ADR's ordering
*reason* (no window before `SetCompatibleTextRenderingDefault`) is untouched.
The `#if NETCORE` block keeps `SetColorMode(System)` only.

### 6.2 Dialog level — `UpsertAppSoundLockRule`

- `AutoScaleDimensions = (96F, 96F)`, `AutoScaleMode.Dpi` — aligned with
  `SettingsForm` and `About`; Font-based scaling is locale-font-fragile.
- `FormBorderStyle.Sizable` (keep `MaximizeBox/MinimizeBox = false`),
  `MinimumSize` = the 96-DPI design `ClientSize` (560×372).
- Designer wiring: every control gets `Anchor`s (the pattern already used by
  `ProcessSelectionForm`):
  - Top|Left|Right: `lblDescription`, `txtProcessPath`, `txtPattern`,
    `lblRegexNotice`, `lblMode`, `rdoAutomatic`, `rdoHotkey`, `lblPlayback`,
    `cmbPlayback`, `lblRecording`, `cmbRecording`, `lblEnforcement`,
    `pnlEnforcement`, `lblAssignmentHelper`
  - Top|Right: `btnSelectProcess`, `btnPlaybackReset`, `btnRecordingReset`
  - Top|Left|Right|Bottom: `grpHotkey`
  - inside `grpHotkey`: `hotKeyBox` Top|Left|Right; `btnHotKeyReset`
    Top|Right; `lvPairs` all; `btnRemovePair`/`btnMoveUp`/`btnMoveDown`
    Bottom|Left; `lblPairCount`/`btnAddPair`/`cmbPair*` unaffected by the
    bottom edge (Top anchors)
  - Bottom|Left: `chkEnabled`, `chkCaseSensitive`, `chkNotify`
  - Bottom|Right: `btnSave`, `btnCancel`
- `ApplyModeLayout()` keeps switching Automatic↔Hotkey, but positions the
  fixed rows relative to the **current `ClientSize` edges** (after setting
  `ClientSize`), not absolute design points, so anchors and the mode switch
  stay coherent when the user resized the window first. `FontScaleFactor()`
  remains the factor source — in `Dpi` mode it equals `DeviceDpi / 96`.
- `lvPairs` keeps the "first column fixed, two variable columns" layout: on
  the group's resize, redistribute the playback/recording column widths
  proportionally to the design widths (`30 / 235 / 235`).

### 6.3 Accessibility minimums

`LocalizeForm()` sets `AccessibleName` (from localization resources, like
every visible string) on the four controls that have none because their
affordance is an image or a dot:

| Control | New resx key |
|---|---|
| `btnSelectProcess` | `appSoundLock_rule_accessible_selectProcess` |
| `btnPlaybackReset` | `appSoundLock_rule_accessible_resetPlayback` |
| `btnRecordingReset` | `appSoundLock_rule_accessible_resetRecording` |
| `btnHotKeyReset` | `appSoundLock_rule_accessible_resetHotkey` |

English values only in the neutral `.resx`; localizations follow via the
usual `lang:` commits. Designer.cs static properties are regenerated
accordingly.

### 6.4 Migration

No schema or config change. `app.manifest` ships with the binary.

## 7. Telemetry

None new; no existing events are affected.

## 8. Localization

| Key | English value |
|---|---|
| `appSoundLock_rule_accessible_selectProcess` | `Select process` |
| `appSoundLock_rule_accessible_resetPlayback` | `Reset playback device` |
| `appSoundLock_rule_accessible_resetRecording` | `Reset recording device` |
| `appSoundLock_rule_accessible_resetHotkey` | `Reset shortcut` |

## 9. Files touched

| File | Change |
|---|---|
| `SoundSwitch/app.manifest` | Add `windowsSettings` declaring PMv2 (+`true/pm` fallback). |
| `SoundSwitch/Program.cs` | Remove `SetProcessDPIAware` DllImport + call and `SetHighDpiMode(PerMonitorV2)`; NETCORE block keeps only `SetColorMode(System)`. |
| `SoundSwitch/SoundSwitch.csproj` | Suppress WFO0003 with rationale comment (manifest is the deliberate single source of truth). |
| `SoundSwitch/UI/Forms/UpsertAppSoundLockRule.Designer.cs` | AutoScale mode Dpi (96 baseline), Sizable border, MinimumSize, anchors on all controls, accessible-name-supporting designer fields unchanged. |
| `SoundSwitch/UI/Forms/UpsertAppSoundLockRule.cs` | `ApplyModeLayout` positions relative to `ClientSize` edges; `AccessibleName` wiring in `LocalizeForm`; pair-list column redistribution on resize. |
| `SoundSwitch/Localization/SettingsStrings.resx` (+ `.Designer.cs`) | Four new accessible-name keys. |
| `docs/design/ui/2026-11-06-dpi-awareness-scalable-dialogs.md` | This ADR. |
| `SoundSwitch.Tests/…` | New layout-invariant tests (see 10). |

## 10. Test plan

### Automated

`SoundSwitch.Tests` — new fixture
`UpsertAppSoundLockRuleLayoutTests` mirroring the
`SettingsBannerLayoutTests` baseline-record style (no live form; the dialog's
constructor needs `IconTextComboBox`/device lists, so we test the designer
baseline + `ApplyModeLayout` arithmetic as pure records):

For scales {1.0, 1.25, 1.5, 1.75, 2.0} and both modes (Automatic, Hotkey):

- Save/Cancel always inside `ClientSize` with the design margin.
- The checkbox row never overlaps the buttons row.
- `ClientSize ≥ MinimumSize` after `ApplyModeLayout` (resize-side guarantee).
- Automatic and Hotkey layouts keep the same right margin as the designer
  baseline (no anchor drift).

### Manual (Windows, required for the process-level flip)

1. `dotnet build SoundSwitch.sln -c Debug`.
2. Display scaling 175%: open Settings → App Sound Lock → Add rule — the
   dialog renders at scaled size, fully usable; nothing cropped.
3. Resize the dialog smaller/larger; all controls reflow, no overlaps; the
   minimum size is honoured.
4. Switch Automatic ↔ Hotkey after resizing; layout stays coherent.
5. Keyboard-only: Tab through, confirm focus order and screen-reader
   announcements of Select process / Reset buttons; Enter=Save, Esc=Close.
6. Open a dialog on a 100% monitor and drag it to a 175% monitor (mixed
   setup); controls rescale without crashing/UI garbage.
7. Banner, quick menu and tray popouts still appear correctly (regression
   watch for the PMv2 flip).

## 11. Decisions

| # | Decision | Rationale |
|---|---|---|
| 1 | Manifest-declared PMv2, runtime DPI calls removed | Verified silent downgrade otherwise (§5). Manifest is race-free, single-source-of-truth, and covers both targets. |
| 2 | Keep `SetHighDpiMode` alongside the manifest | Rejected: two sources of truth; the call can only re-introduce silent conflicts. |
| 3 | `AutoScaleMode.Dpi` (96,96) on the form | Rejected keeping `Font`: locale font metrics make scale factors vary per language; newer repo forms (Settings, About) already use `Dpi`. |
| 4 | Anchors only (ProcessSelectionForm precedent) | Rejected a TableLayoutPanel rewrite: larger diff, risks regression in the mode-switch logic; anchors are the repo's proven resizable-dialog pattern. |
| 5 | Sizable with `MaximizeBox = false` | Windows convention for dialogs; resize must exist as an accessibility escape hatch but maximize on a formful dialog has no meaning. |
| 6 | PMv2 app-wide now (instead of system-aware + per-form fixes) | The user ask is "every screen… whatever the scaling"; System-aware cannot actually deliver that on mixed-DPI setups. Rest is follow-up hardening (§12). |

## 12. Future work

- Audit remaining forms against PMv2 (`QuickMenu`, `BannerForm`,
  `UpsertProfileExtended`, `ProcessSelectionForm` re-check) — same
  AutoScale/anchor/accessible-name recipe.
- Persist user dialog sizes (profile upsert, app rules) per DPI.
- Full screen-reader review (AutomationProperties beyond names).
- Re-evaluate the `WindowsAPIAdapter` hidden-form thread's own DPI context.
