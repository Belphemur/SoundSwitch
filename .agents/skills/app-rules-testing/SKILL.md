---
name: App Rules testing
description: Test the App Rules identity-based assignment feature — unit fixtures, a live-app control script, and an end-to-end test that drives the real Settings UI, Windows' per-app endpoint store, and the restore-default. Use this when working on AppRuleEnforcement, the assignment ledger/reconciler, the rule editor's eligibility note, or reviewing such changes.
---

# App Rules testing

App Rules can route a program by its process identity instead of a window title. There is no
enforcement choice: a rule that targets one concrete application (no Window Title pattern)
also writes its route into Windows' per-app persisted endpoint store, and the dynamic
process/foreground/hotkey path stays on top as the fallback.

Testing has three layers. Use the cheapest layer that can catch your change.

| Layer | Command | Time | Needs a running app | Needs real devices |
| --- | --- | --- | --- | --- |
| Unit tests | `pwsh tools\Test-AppRules.ps1` | ~10 s | no | no (COM needed for the reconciler fixture) |
| Live control | `pwsh tools\Control-SoundSwitch.ps1 ...` | seconds | yes (script can launch) | optional |
| End-to-end | `pwsh tools\Test-AppRulesEndToEnd.ps1` | ~1–2 min | yes (script launches) | yes |

## What the feature does

1. **Enforcement kinds.** `AppRuleEnforcement.Dynamic` matches at process sighting;
   `Assignment` makes Windows keep the route by exe identity, even across restarts
   (`SoundSwitch/Model/AppRuleEnforcement.cs`, `SoundSwitch/Services/AppSoundLockManager.cs`).
   The editor only offers assignment for Automatic rules with a concrete exe path and no
   window-title pattern (`AppRuleAssignmentValidator`).
2. **Ownership ledger.** `AssignmentLedgerEntry` in the config records every route
   SoundSwitch wrote (any enforcement kind). `AppRuleAssignmentReconciler` adopts
   pre-ledger records, stamps writes (`EnsureEntry`), drops stale entries, and on rule
   deletion/disable restores the default immediately — it detaches live sessions and
   deletes the identity's record key (`DeleteRecords`), which also covers a closed app.
   The pending entry (`ClearRequestedAt`) is now only the retry path when the store
   cannot be written.
3. **Windows' store.** `PersistedDefaultEndpointStore` reads
   `HKCU\Software\Microsoft\Multimedia\Audio\DefaultEndpoint` and deletes a record by
   identity (enumeration, not the un-reversible hash); writing a route is still the
   engine's job. The App Rules list shows a read-only "Windows assignments" section
   merged with the rules so one identity appears once.

## Layer 1 — unit tests

```powershell
pwsh tools\Test-AppRules.ps1          # feature fixtures (~2 s after build)
pwsh tools\Test-AppRules.ps1 -List    # list the fixtures
pwsh tools\Test-AppRules.ps1 -All     # full SoundSwitch.Tests suite
pwsh tools\Test-AppRules.ps1 -NoBuild # skip the build
```

`RefreshDeviceTests.TestMultipleRefresh` is excluded on every run mode (including `-All`):
it is a timing-sensitive COM refresh race that CI also skips. The exclusion lives in one
place in the script and is composed into the single filter every run uses.

| Area | Fixture |
| --- | --- |
| Enforcement hydration, migration fallback, validator constraints | `AppRuleEnforcementTests` |
| Reconcile state machine (adoption, stale-drop, deferred clear, session-less clears) | `AppRuleAssignmentReconcilerTests` |
| Ledger round-trip and snapshot enumeration | `AssignmentLedgerTests` |
| NT-device vs drive-letter identity matching | `AppRuleIdentityMatcherTests` |
| Store parsing, token direction, drift tolerance | `PersistedDefaultEndpointStoreTests` |

## Layer 2 — control a live app

```powershell
pwsh tools\Control-SoundSwitch.ps1 -Cli 'devices'
pwsh tools\Control-SoundSwitch.ps1 -Tab 'App Rules' -Click addAppSoundRuleButton -Screenshot app-rule-editor
pwsh tools\Control-SoundSwitch.ps1 -Stop
```

Automation ids (from `Settings.Designer.cs` / `UpsertAppSoundLockRule.Designer.cs`):
`appSoundLockTabPage` (App Rules tab), `addAppSoundRuleButton`, `deleteAppSoundRuleButton`,
`rdoEnforcementDynamic`, `rdoEnforcementAssignment`, `txtProcessPath`, `txtPattern`,
`cmbPlayback`, `btnSave`, `lblAssignmentHelper`. In the auto-opened process picker:
`dgvProcesses`, `txtFilter`, `btnSelect`. Names are localized; automation ids are not.

For documentation screenshots use `pwsh tools\Capture-DocumentationScreenshot.ps1 -Name
AppRules -Tab 'App Rules' -Click addAppSoundRuleButton` (see `documentation-sync`).

## Layer 3 — end-to-end test

```powershell
pwsh tools\Test-AppRulesEndToEnd.ps1
pwsh tools\Test-AppRulesEndToEnd.ps1 -NoBuild -AllowExistingInstance
```

It backs up `%APPDATA%\SoundSwitch\SoundSwitchConfiguration.json`, stages an empty config
while no instance runs, launches with `--disable-updater`, and polls `CLI status` for
readiness (no guessed sleeps). Asserted:

1. **A** — the editor offers no enforcement control (the radios are gone) and keeps the
   process path / window title fields; closing saves nothing.
2. **B** — a scratch tone-playing exe gets a rule with a non-default playback device and,
   because it is one concrete application, saves as an identity assignment: config rule +
   ledger entry, the engine-written store record, and exactly one row in the merged list.
3. **D** — the same exe is *playing*, so the picker lists it under "Using audio now":
   filtering the grouped list, selecting the row and confirming with **Select** fills the
   editor's process field. Closing saves nothing.
4. **C** — deleting the rule while the exe is closed removes the rule, deletes the app's
   store record right away and releases the ledger claim; the relaunched exe carries no
   route.

The original config is restored, the scratch exe and only the instance this test launched
are stopped, and only store records named after the scratch exe are deleted.

## Gotchas that cost real time

- **Dot-sourced parameters shadow your locals.** `Control-SoundSwitch.ps1` has a `[switch]$Status`
  (and `$Start`, `$Stop`, `$Cli`, `$Force`, `$Json`, ...) in its param block; dot-sourcing
  creates those variables in your scope. Assigning an object to `$status` throws
  "Cannot convert PSCustomObject to SwitchParameter". Name locals `$statusInfo` etc.
- **Same-name parameter/local case-insensitivity.** `Set-TextValue -Value $x` with a local
  `$value` reuses the *string* parameter; the pattern object must be `$pattern`.
- **The process picker is modal and auto-opens.** Clicking Add opens the editor *and* the
  grouped picker; the editor is disabled until the picker closes. Close it with `WM_CLOSE`
  and only then touch the editor fields.
- **No enforcement radio exists.** Eligibility is derived (Automatic, one pair, concrete
  path, no Window Title) and cannot be chosen. Only a cosmetic note differs between an
  eligible and a dynamic-only rule, and it is not reliably exposed through this MSAA proxy —
  do not assert it; assert the saved rule instead.
- **Use `BM_CLICK` for buttons that open a modal.** The UIA `Invoke()` pattern blocks until
  the dialog closes (`Invoke-SoundSwitchElement` already posts `BM_CLICK`).
- **Native MessageBox buttons are invisible to UIA here.** The delete confirmation exposes
  no Button children; answer it with `PostMessage(hwnd, WM_COMMAND, IDYES=6, 0)`.
- **A route needs a live audio session.** The engine only persists a per-app endpoint for a
  process that is producing audio: the scratch exe loops a WAV (`SoundPlayer.PlaySync`).
- **The list always has a "Windows assignments" header row.** Count rows matching the
  identity; never assert the list has one item overall.
- **Deletion is immediate, closed app included.** The record key is deleted by identity, so
  after deleting a rule the ledger has no claim and the store has no record — assert that
  straight away. A pending `ClearRequestedAt` entry only means the store could not be
  written (retry path), so never assert pending as the normal outcome.
- **The store's key naming is undocumented.** Derive direction from the token's trailing
  interface GUID (render `e6327cad…`, capture `2eef81be…`), never from the key name.
- **Never delete store keys you did not create.** Clean up only records whose (Default)
  value ends in the scratch exe name, and only after stopping the app.
- **Config is in-memory.** Stop the app before restoring the file; restoring under a live
  app loses on its next save. With `-KeepRunning`/`-AllowExistingInstance` the restore is
  skipped and the backup path is printed.
- **Deep UIA descendant reads are unreliable.** Walk children one level at a time
  (`Get-UiElements`) instead of `FindFirst(Descendants, …)` and read `.Current` per hop.

## Before pushing

- Reconciler, ledger, matcher, store, migration or validator change: `pwsh tools\Test-AppRules.ps1`.
- Editor eligibility, enforcement persistence, list merge, delete/restore-default: also
  `pwsh tools\Test-AppRulesEndToEnd.ps1` (desktop session + real endpoints).
- Picker visuals or the "Windows assignments" section layout: capture the screenshot flow.
- A user-visible setting/flow changed: load `documentation-sync` — website and screenshots
  ship in the same PR.
