---
name: DRY, SOLID, KISS
description: Apply the repository's engineering principles (DRY > SOLID > KISS) when implementing, refactoring, or reviewing C# code in SoundSwitch.
---

# Engineering principles: DRY > SOLID > KISS

Apply these in priority order when they conflict. They guide the change you were
asked to make; they are not a licence to refactor unrelated code.

## 1. DRY — one source of truth

- Extract a value, rule, or algorithm when it is repeated **and** the copies must
  change together: one constant, accessor, resource, or helper.
- Keep theme-aware colours in the existing accessors (`SettingsForm.Get*Color`,
  `WindowsThemeHelper`). Never inline colour literals in controls.
- Keep user-facing text in localization resources.
- Do not deduplicate coincidental similarity. Two short blocks that evolve
  independently are cheaper than the wrong abstraction.

## 2. SOLID — small, replaceable pieces

- Give each type and method one reason to change. UI code stays presentation-only;
  device, configuration, and notification behaviour belongs in `AppModel` and the
  framework services.
- Depend on existing interfaces and factories (`IAudioDeviceLister`, managers,
  `RailSharp.Result<T>` patterns) instead of reaching across layers.
- Extend an existing seam (for example `WindowsThemeHelper.DarkModeProvider`)
  rather than adding a new one.
- Add an interface, base class, or indirection only when there is a second
  consumer or a concrete testing need.

## 3. KISS — the smallest correct change

- Fix the reported problem with the fewest moving parts.
- Prefer the platform API to a custom reimplementation.
- No speculative generality, no unrelated refactors, no formatting-only churn.
- Take the simpler approach even when it is slightly less "pure".

## Repo-specific guardrails

- Use `NameClean`, not `Name`, for displaying or comparing audio devices.
- Localize all visible text.
- Resolve colours through the theme accessors so light and dark mode both work.
- Match the surrounding style and keep diffs focused.
- Preserve WinForms UI-thread affinity.

## Review checklist

- [ ] Is every repeated fact defined exactly once?
- [ ] Does each unit have one reason to change?
- [ ] Is this the simplest change that fixes the issue?
- [ ] Did I add an abstraction with only one caller? (remove it)
- [ ] Are there unrelated edits? (remove them)
- [ ] Does it build, and do the related tests pass?
