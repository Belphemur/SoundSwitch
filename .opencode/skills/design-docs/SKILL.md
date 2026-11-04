---
name: Design docs & ADRs
description: Write and maintain SoundSwitch design documents and ADRs (Architecture Decision Records) under docs/design/. Use this when the user asks for an ADR or design doc, when planning a significant feature, rework, or cross-cutting change, or when implementing a change that already has a design doc (read it first).
---

# Design docs & ADRs

Significant changes get a short, code-grounded design document **before** the
implementation is written. The document is a record of a decision taken at a
point in time, not a wish list: write it as if a future maintainer must be able
to rebuild the feature from it without talking to you.

## When to write one

Write an ADR when a change:

- adds or reworks a **user-facing feature** (new mode, new setting, changed UX flow),
- touches **more than one project** inside the solution (e.g. `SoundSwitch` + `SoundSwitch.Audio.Manager`),
- changes a **data/config schema** that needs migration,
- resolves a design question where several viable options exist.

Do **not** write one for bug fixes, localization, docs-only changes, or obvious
one-line fixes — those belong in the commit message and issue thread instead.

If the change is user-visible, pair this skill with `documentation-sync` (website
pages + screenshots) — the ADR states the *decision*, the website states the
*user-facing how-to*.

## Where ADRs live

| Rule | Value |
|---|---|
| Directory | `docs/design/<topic>/<YYYY-MM-DD>-<short-slug>.md` (topic subdirectories exist: `app-rules/`, `ui/`, `tray-icon/`) |
| Filename | Lowercase kebab-case, **date-prefixed with the date the decision is made**: `2026-10-22-app-rule-modes.md` |
| Mockups | A sibling image file (PNG/WebP) next to the markdown, referenced with a relative path. Generate them deliberately (script, design tool); never a hand-grabbed screenshot. |
| Flat fallback | Long-lived platform plans may live flat in `docs/` (e.g. `NAUDIO_REMOVAL_PLAN.md`) when they are not a single decision, but a topic directory is preferred. |

Link the ADR from the feature's AGENTS.md context, the PR, and (when the feature
ships) the relevant docs page.

## Workflow

1. **Investigate the code first.** Every claim that names a file, class, or
   behaviour must come from reading the repository, not from memory. An ADR
   grounded in guesses is worse than no ADR.
2. **Check prior art.** `glob docs/design/**` for related decisions and read
   them; reference them under `**Related:**`. Never contradict an existing ADR
   without superseding it explicitly.
3. **Check the platform.** For Windows-integration decisions (COM APIs, MMDEVAPI,
   registry, policy), record what the platform offers, what it does not (missing
   public APIs are design constraints with a real impact), and what the APIs are
   called before settling on an approach.
4. **Use the [template](./TEMPLATE.md).** Delete sections that genuinely do not
   apply, but note in one line why (e.g. "No migration needed — schema unchanged.").
5. **Record rejected options** in the Decisions table with the rationale. The
   decision only has value if a future reader can see why the alternative lost.
6. **Keep it current.** While implementing, if reality diverges from the doc,
   amend the doc in the same PR; do not let "Implemented as promised, except…"
   only live in commit history.

## Structure (per template)

- **Header block:** title, `**Status:**` (`Proposed` → `Implemented on <branch>` / `Superseded by <link>`), `**Author:**`, `**Branch:**` / `**Base:**`, `**Issue:**` / `**Related:**`.
- **1. Problem** — what a user/maintainer cannot do today, concretely.
- **2. Goals / 3. Non-goals** — numbered, short, mutually exclusive from Non-goals.
- **4. UX** — only for user-visible decisions: describe the surface, list every field/column/behaviour change.
- **5+. Design** — data model (with C# snippets for new types/schema), behaviour (trigger paths, edge cases, failure modes), migration (idempotent rules + before/after table), telemetry.
- **Localization** — every new user-facing string as a `SettingsStrings.resx` key table (start English, localized files can follow).
- **Files touched** — table of file → change; this is checked against the eventual diff.
- **Test plan** — split automated (which test project, what cases) and manual (build command + numbered user-level checks).
- **Decisions** — numbered table `Decision | Rationale` including rejected alternatives.
- **Future work** — explicitly deferred items, so they are not mistaken for omissions.

## Writing rules

- Plain, direct sentences. No marketing tone. Present tense for decided behaviour ("The hotkey applies pair 1"), subjunctive only for proposed behaviour.
- Name real code: `AppSoundLockManager.SwitchRuleDevice`, `AudioSwitcher.SwitchProcessTo` — not "the manager". Include file paths the first time each component appears.
- Every section earns its place: if a reader cannot act on the section, cut it.
- One ADR, one decision. If a second architectural question appears mid-writing, split it into its own ADR and link it.

## Definition of done

- [ ] Filed under `docs/design/<topic>/<YYYY-MM-DD>-<slug>.md` following the convention.
- [ ] Every named component was verified against the current code (paths, signatures, behaviours).
- [ ] Goals/Non-goals cover the whole change; nothing contradicts an existing ADR.
- [ ] Decisions table includes at least the significant rejected options with rationale.
- [ ] Files-touched table matches the intended scope.
- [ ] Test plan is executable as written (build commands and steps are repo-valid).
- [ ] Status header reflects reality (`Proposed` until it ships on a branch).
