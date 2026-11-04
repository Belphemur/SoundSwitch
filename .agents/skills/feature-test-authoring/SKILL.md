---
name: Feature test authoring
description: Author a per-feature testing layer for a SoundSwitch feature — unit fixtures, a tools/Test-<Feature>.ps1 runner, an optional end-to-end script, and its own <feature>-testing skill doc — following the proven mute-target pattern. Use this when adding test coverage for a feature, creating Test-<Feature>.ps1, making an end-to-end test for a feature, or writing a feature testing skill.
---

# Feature test authoring

Every shipped feature should get a self-contained testing layer a reviewer can
run with one command: focused unit fixtures, a live-control surface it reuses
(`Control-SoundSwitch.ps1`), an optional end-to-end script, and a
`<feature>-testing` skill that tells agents and humans exactly what to run.

`tools/Test-MuteTarget.ps1` + `tools/Test-MuteTargetEndToEnd.ps1` +
`.agents/skills/mute-target-testing/SKILL.md` is the reference implementation.
Read all three alongside this skill before writing anything.

## What you produce

| Artifact | Path | Purpose |
| --- | --- | --- |
| Unit fixtures | `SoundSwitch.Tests/*.cs` (or existing ones) | Fast, deterministic coverage of the feature's logic |
| Feature runner | `tools/Test-<Feature>.ps1` | One command for the fixtures; build + filter assembly |
| E2E test (optional) | `tools/Test-<Feature>EndToEnd.ps1` | Drives the real app; asserts user-visible behaviour |
| Feature skill | `.agents/skills/<feature>-testing/SKILL.md` | Route table: which change → which command(s) |

Skills that describe **testing** live under `.agents/skills/` (like
`mute-target-testing` and `end-to-end-testing`). The harness auto-discovers a
new `SKILL.md` in that directory — no manifest to update.

## Decide which layers

| Your change touches | Unit layer | Live control | E2E layer |
| --- | --- | --- | --- |
| Pure logic (parsing, math, matching, serialization, layout math) | required | no | no |
| A persisted setting that changes behaviour | required | while iterating | required |
| Tray/hotkey/CLI action reaching the audio stack or UI | fixtures for the logic | yes, manual | required |
| Notification/banner appearance or lifecycle | fixtures | yes | yes |
| Device arrival/removal behaviour | fixtures | yes | yes |
| Brand-new responses, tab names, screenshots, or user-visible flow (docs-visible) | required | yes | yes + `documentation-sync` rides along |

Load the `end-to-end-testing` skill when writing the E2E script, the
`documentation-sync` skill when the change is user-visible, and the
`dry-solid-kiss` skill for the fixture code.

## Authoring workflow

1. **Enumerate the feature's observable behaviour.** For each behaviour, note the
   cheapest layer that can catch a regression: config persistence → unit;
   UI wiring of that setting → E2E; notification content → unit; banner actually
   pinned → E2E.
2. **Map behaviours to fixtures.** Extend existing fixtures where they fit
   (`SoundSwitch.Tests`), add new ones when they don't. Keep one fixture per
   area so the runner's fixture list stays a readable map.
3. **Write the runner** `tools/Test-<Feature>.ps1` from `SCRIPT-TEMPLATE.md`:
   - `#Requires -Version 7.0`, `[CmdletBinding()]`, params `-All`, `-Audio`
     (only if the feature touches the COM audio stack), `-List`, `-NoBuild`.
   - A `$featureFixtures` array: fixture name + one-line comment on what it
     covers.
   - `-List` prints the fixtures and exits; `-All` runs the whole suite; the
     default runs only the feature fixtures (seconds, not minutes).
   - One build invocation before any test run.
4. **Known-flaky tests: exclude by explicit test filter on every run mode**
   (including `-All`). Keep the exclude list in exactly one place in the script
   and compose it into a single filter used by everything. A doc comment that
   says "this is known flaky" without a filter that applies on `-All` is a bug —
   it describes a suite that still fails.
5. **Write the E2E script** (if the decision table asked for one) from
   `tools/Test-MuteTargetEndToEnd.ps1` and the `end-to-end-testing` skill.
   Launch with `--disable-updater`, poll `CLI status` for readiness (never a
   guessed sleep), and build on `Control-SoundSwitch.ps1` primitives — never
   re-implement UIA/Win32 interop. The reference's cleanup is not the model to
   copy: put the teardown in `finally` **in this order**:
   - Stop only the instance *you* launched (track it with a `$launched` flag,
     never by process name and never a stale PID file), honouring `-KeepRunning`.
   - **Then** restore `%APPDATA%\SoundSwitch\SoundSwitchConfiguration.json`
     (or delete it if the test created it). Restoring the file while an app is
     still running loses: the app caches its config in memory and the next save
     (on exit or on change) clobbers your restore. Stop → restore.
   - `-KeepRunning` trades away that safety: the app still runs with the test's
     config in memory, so a restored file can be overwritten by the app's next
     save. Either skip the restore and leave the backup where it is, printing
     its path for the user to reapply after stopping the app, or document the
     limitation in the script's help.
   - Prefer failing fast when an instance is already running. If
     `-AllowExistingInstance` attaches to a live app, do not restore the file
     while that app runs: keep the backup, print its path, and tell the user to
     reapply it after stopping the app — the app would otherwise write its
     in-memory state over the restore, which the file restore alone cannot undo.
     If the file was absent before the run, there is no backup: print that the
     test created the file and tell the user to delete it after stopping the app.
6. **Write the `<feature>-testing` skill** from `TEMPLATE.md`, moving per-feature
   facts (real commands, fixture table, automation ids, gotchas) in.
7. **Register:** add one line for each new script to `tools/README.md`; the
   `<feature>-testing` SKILL.md routes changes to the scripts.
8. **Validate:** `pwsh tools\Test-<Feature>.ps1 -List`, then the default run
   (green), then `-All` if your fixtures interact with shared state. Push a
   conventional commit: `tests(<feature>): ...` does not trigger a release but
   can still appear in generated release notes; `chore(<area>): ...` for pure
   tooling avoids both. `feat`/`fix` trigger a release.

## Before pushing

- Logic-only change (`SoundSwitch/Tests` fixtures, serializer, resolver math):
  `pwsh tools\Test-<Feature>.ps1` green.
- Anything that changes what the feature does at runtime: also run
  `pwsh tools\Test-<Feature>EndToEnd.ps1` locally; it needs microphones and a
  desktop session — never rely on CI for it.
- A known-flaky test was hit: add an explicit filter exclusion (one place in the
  script, applied on every path) rather than retrying locally or rerunning CI.
- A user-visible setting or flow was added/renamed/removed: load
  `documentation-sync` — website page and screenshot changes belong in the same
  PR.
- New script added: `tools/README.md` lists it, help block has SYNOPSIS/
  DESCRIPTION/PARAMETERS/EXAMPLES, and `#Requires -Version 7.0` is the first
  line.
