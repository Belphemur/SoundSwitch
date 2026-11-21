# ADR: Consolidate the mechanisms shared by Profile and App Rules

## Status

Accepted (2026-11-21) — owner directive: implement as one PR from a worktree,
delegated to OpenCode. The two judgment calls added beyond the directive are
isolated in §Judgment calls (owner-vetoable; veto = drop that decision's
commit, nothing else changes).

## Context

The DRY/SOLID/KISS overlap audit of the Profile and App Rules features (after
the AppModel decomposition, #2622) found three mechanisms the two features
share that have drifted into duplicates:

1. **Two `WindowMonitor` instances.** `CompositionRoot.cs` constructs a
   separate `new WindowMonitor()` for `ProfileManager` and for
   `AppSoundLockManager`. Each constructor registers its own
   `SetWinEventHook`, so every foreground change fires the OS hook twice and
   runs the expensive `Process.GetProcessById` + `MainModule` walk twice, and
   both subscribers do near-identical work off the same event.
2. **Three device-resolution paths.** `AppRuleDeviceResolver` (Services) is
   the canonical resolver — exact id, then stable device-instance path, then
   unique `NameClean`, ambiguity flagged (all matching knowledge in
   `DeviceReadOnlyCollection.Match`). `ProfileManager.CheckDeviceAvailable`
   re-implements the same match + ambiguity logging inline. And
   `NotificationManager.CheckDeviceAvailable` is a third, weaker variant
   (`FirstOrDefault(Equals)`, no fallbacks, no ambiguity handling): a device
   that survives a driver update resolves for App Rules but silently fails for
   the profile notification icon.
3. **Three uncoordinated hotkey registries.** Global hotkeys
   (`SettingsService`), profile hotkeys (`ProfileHotkeyManager`), and app-rule
   hotkeys (`AppSoundLockManager.ReloadHotkeys`) each register directly
   against `WindowsAPIAdapter`. A cross-system conflict surfaces only as a raw
   `RegisterHotKey` failure — "may conflict with another hotkey" — instead of
   naming which owner holds the combination.

These predate the #2622 refactor (the old `InitializeMain` also passed two
`WindowMonitor` instances); the refactor faithfully carried them forward.

## Decision

Three consolidations, one PR, one commit each, in this order:

**D1 — `WindowMonitor` becomes a container singleton.**
Register `WindowMonitor` once in `CompositionRoot`; both managers receive the
same instance. One hook, one process walk per foreground change, two
subscribers. Behavior-neutral: both subscribers keep their own handler logic
and registration order.

**D2 — one device-resolution path.**
Rename `AppRuleDeviceResolver` → `DeviceResolver` (feature-neutral; its
matching knowledge already lives in `DeviceReadOnlyCollection.Match`).
`ProfileManager.CheckDeviceAvailable` and
`NotificationManager.CheckDeviceAvailable` route through it and are deleted.
The ambiguity warning is logged once, at the resolver.

**D3 — one hotkey registration point.**
Introduce a single registry (e.g. `HotkeyRegistry`) that all three owners
register through. On a registration failure the registry knows which owner
already holds the combination and reports it in a localized message
(e.g. "already used by profile X / app rule Y / the playback switch") instead
of a raw failure. `WindowsAPIAdapter` remains the Win32 layer underneath; the
registry owns the owner→hotkey knowledge, not the P/Invoke.

### Decision log (DRY > SOLID > KISS)

1. **D1 (DRY).** One OS hook where two existed; the loser was the doubled
   per-event process walk — a real runtime cost, not just duplication.
2. **D2 (DRY).** One resolution policy where three drifted; the loser was
   per-call-site policy — the notification variant had already silently
   diverged (no driver-update survival), which is what duplicated policy does.
3. **D3 (DRY + SOLID).** One namespace of hotkey ownership where three
   uncoordinated ones produced unexplainable conflicts; the loser was raw
   `RegisterHotKey` failures the user cannot act on. The abstraction cost is
   accepted because all three owners already share the same underlying
   mechanism and only the ownership knowledge was missing.

## Judgment calls (owner-vetoable)

1. **D2 behavior superset:** `NotificationManager` gains the
   id → instance-path → unique-`NameClean` fallback, so profile notifications
   resolve devices that previously silently failed after a driver update.
   Cost: a behavior change in spirit (more notifications resolve correctly);
   reverting = keep a documented weaker variant for notifications only.
2. **D3 new abstraction:** `HotkeyRegistry` adds one interface + localized
   conflict strings. Cost: a new concept in the WinAPI layer; reverting = drop
   commit D3, the two mechanical consolidations stand alone.

## Rollout

One PR from a worktree cut from dev: commit 1 = D1, commit 2 = D2, commit 3 =
D3 (each conventional, each leaving the solution buildable). Gates per commit:
Release build 0 errors; test suite green except the two documented
environment-only buckets (single-device VM, layered-window-over-ssh).
Re-fetch before push. PR against `dev` citing this ADR. The owner merges.

## Non-goals

- Profile-internal and App-Rules-internal audit findings (trigger type-switch
  boilerplate, obsolete `ValidateAddProfile` dead code, etc.) — separate
  cleanups, not shared mechanisms.
- No change to trigger semantics, profile switching behavior, or rule
  enforcement semantics; the app-rules ADR exclusions (one rule both
  mechanisms, no registry writes except §5.1 deletion, deferred-clear §6.3)
  stand unchanged.
- No new settings, no persistence format changes, no UI redesign.

## Consequences

- One foreground event costs one process walk instead of two.
- One device-resolution policy with one ambiguity log line; a stored device
  that survives a driver update resolves everywhere, consistently.
- Hotkey conflicts between systems become user-explainable instead of a
  silent log line.
- `AppRuleDeviceResolver`'s call sites (App Rules, tests) rename with the
  class; no consumer-visible change.
