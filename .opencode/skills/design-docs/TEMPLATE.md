# \<Title: short, decision-shaped>

**Status:** Proposed
**Author:** \<name>
**Branch:** `<branch>` (omit until a branch exists)
**Base:** `<base ref>` (e.g. `origin/dev`)
**Issue:** [\<project>#\<n>](\<link>) - \<one-line issue title> (omit if none)
**Related:** PR #\<n>, `docs/design/<topic>/<other>.md`, ...

---

## 1. Problem

What a user or maintainer cannot do, or what is ambiguous today. Concrete, with
the current behaviour named (file/class) and why it falls short. One or two
paragraphs.

## 2. Goals

1. …

## 3. Non-goals

- …

## 4. UX

*(Omit for platform-only decisions.)* Describe the visible surface: fields,
columns, states, transitions. Reference the mockup PNG with a relative path.

## 5. Investigation

*(Include when the decision depends on platform behaviour.)* What Windows/the
platform offers, what it does not, what was verified in the code or on the
running system.

## 6. Design

### 6.x Behaviour

Trigger paths, state changes, edge cases, failure modes.

### 6.y Migration

*(Omit with one line if the schema does not change.)* Idempotency rules and a
before/after table.

### 6.z Data model

C# snippets for new/changed types.

## 7. Telemetry

New events/fields, or "no new events; uses existing …".

## 8. Localization

| Key | English value |
|---|---|
| `key.path` | `Value` |

*(Omit with one line if no new user-facing strings.)*

## 9. Files touched

| File | Change |
|---|---|
| `SoundSwitch/…` | |

## 10. Test plan

### Automated

Which test project, which cases.

### Manual

1. `dotnet build SoundSwitch.sln -c Debug`.
2. Numbered user-level checks.

## 11. Decisions

| # | Decision | Rationale |
|---|---|---|
| 1 | | Include rejected alternatives. |

## 12. Future work

- Explicitly deferred items.
