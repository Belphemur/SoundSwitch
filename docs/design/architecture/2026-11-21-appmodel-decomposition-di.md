# ADR: Decompose the AppModel god class and introduce dependency injection

## Status

Accepted (2026-11-21) — all open questions resolved by the owner the same
day; see Resolved questions. Implementation directive: one PR for the whole
refactor, reviewed by LLMs, carried out on the win11-dev VM via OpenCode
(opencode-go/glm-5.3).

## Context

### What exists today

`SoundSwitch.Model.AppModel` is the application's single composition root *and*
runtime service registry, in one class:

- ~1,785 lines across 4 partial files (`AppModel.cs` 257, `AppModel.AppSettings.cs`
  361, `AppModel.DeviceService.cs` 353, `AppModel.NotificationSettings.cs` 398)
- a static singleton `AppModel.Instance`, referenced **317 times across ~25
  files** (UI forms, Framework managers, Services, Program.cs, tests)
- a `InitializeMain(...)` method that is effectively a hand-written,
  order-sensitive composition root: it subscribes `AudioDeviceLister` events,
  wires `NotificationManager(this, this, this)`, instantiates `TrayIcon`,
  `ProfileManager`, `AppSoundLockManager`, registers hotkeys, runs a config
  migration, and starts the update checker — with a documented
  `_initialized` re-entrancy guard throwing `InvalidOperationException` and a
  documented ordering hazard for mute-banner reconciliation at startup.
- Below it sit more singletons with the same shape: `AudioSwitcher.Instance`,
  `MMNotificationClient.Instance`, `JobScheduler.Instance`,
  `WindowsAPIAdapter` (static), `QuickMenuManager<DeviceFullInfo>.Instance`.
- There is no DI container anywhere in the solution today. CPM
  (`Directory.Packages.props`) already pins `Microsoft.Extensions.Caching.Memory`
  11.0.0, so `Microsoft.Extensions.DependencyInjection` would be a sibling
  pin, not a new ecosystem.

The repo has already started the first step without naming it: `IAppModel`
composes four focused sub-interfaces (`IDeviceService`, `INotificationSettings`,
`IAppSettings`, `IAppInfrastructure`), `NotificationManager` takes those
interfaces through constructor injection, and `SoundSwitch.Tests` already fakes
all three of them (`FakeNotificationSettings`, `FakeInfrastructure`, ...) in
`NotificationManagerMuteTests` — the seam exists, it is just **opt-in by
discipline** rather than enforced, and the concrete `AppModel` keeps every
responsibility in one class with shared private state
(`_notificationManager`, `_selectedDevices`, `_initialized`) spanning all four
files.

### The concrete cost

1. **Touch-anything, break-anything.** The partials share private state; a
   change in `AppModel.AppSettings.cs` (e.g. the hotkey registration logic
   that also lives in `InitializeMain`) can break device-service behaviour in
   `AppModel.DeviceService.cs`. There is no compiler-enforced boundary.
2. **Two composition orders to keep in sync.** `Program.cs` /
   `SoundSwitchApplicationContext` construct `CachedAudioDeviceLister`,
   `MMNotificationClient`, `TrayIcon`-adjacent managers, then
   `InitializeMain` constructs the rest — and the mute-banner reconcile
   depends on that exact interleaving (comment in `AppModel.cs:209-221`).
   Any new service has two places to be born and one documented ordering
   hazard to respect.
3. **Untestable lifecycle.** The `_initialized` guard and the static
   `Instance` make it impossible to construct an isolated model for tests;
   tests fake the interfaces instead, which is exactly why they never catch
   wiring bugs in `InitializeMain` — the composition code has no tests.
4. **Silent service-locator drift.** 317 `AppModel.Instance` references are
   the record of every place that chose the locator over constructor
   injection. With 46 references to `AudioSwitcher.Instance` /
   `MMNotificationClient.Instance` below it, the codebase demonstrates the
   drift is already spreading below the model layer.
5. **Startup bugs live in the hardest place to see them.** `InitializeMain`
   does hotkey registration, config migration, update-checker startup and
   banner reconciliation in one method; a bug in any is invisible until the
   app runs, and the ordering comments are the only specification of the
   order.

### Why now

The tray-icon mute-state feature (dev `4ce87785`) added a `MuteStateChanged`
event and touched `IDeviceService.cs`, `AppModel.DeviceService.cs` and
`Events.cs` — the exact seams this refactor formalizes. The sub-interfaces and
test fakes already exist; the refactor formalizes the seam the codebase has
already chosen, rather than inventing a new architecture.

## Decision

The refactor has two independent axes, decided separately:

- **Axis A (structural):** split `AppModel` into real services.
- **Axis B (composition):** how services get constructed and wired.

### A. Target architecture: real services, one responsibility each

Split the four partials into four (or more) service classes with **no shared
private state**:

| Today (partial of one class) | Becomes |
|---|---|
| `AppModel.DeviceService.cs` | `DeviceService` (selected devices, switch/cycle, mute resolution, mute events) |
| `AppModel.NotificationSettings.cs` | `NotificationSettingsService` (banner/notification settings, cached sound) |
| `AppModel.AppSettings.cs` | `SettingsService` (persisted app settings, hotkeys, update mode) |
| `AppModel.cs` (InitializeMain) | `AppBootstrapper` — a single, ordered startup sequence class |
| `AppModel.cs` (Dispose, resume handler) | `AppShutdown` / owned by `AppBootstrapper` |

Rules:

- Each service takes its dependencies through constructor injection and owns
  its state privately.
- `INotificationSettings`, `IDeviceService`, `IDeviceService`,
  `IAppSettings` are already the public contracts — keep them (they are the
  tested seam); move the implementations out of the partial class.
- **DRY first:** `AppConfigs.Configuration` stays the single source of truth
  for persisted settings. `NotificationSettingsService` and `SettingsService`
  become thin façades over it (property get/set → config get/save), not new
  state. The two-layer design (thin config façades + behavior services) is the
  KISS-compliant shape: no duplicated settings storage.
- **The singleton is retired by attrition, not decree:** new code must take
  `IAppModel` sub-interfaces via constructor; `AppModel.Instance` remains
  during migration and is deleted once the 317 references are migrated
  (tracked as the service-locator retirement commit, see Rollout).
- `IAppModel` itself stays as the migration-neutral facade: existing callers
  keep compiling while services are extracted one partial at a time. It is
  deleted in the final phase per the resolved Q2.

### B. Composition: the container (resolved — MS.DI)

Two candidates, evaluated against WinForms reality:

**Option 1 — `Microsoft.Extensions.DependencyInjection` (MS.DI) alone.**

- Pros: industry standard, battle-tested, `AddSingleton/AddTransient`, no
  new repo dependency beyond the package; the codebase already consumes the
  `Microsoft.Extensions.*` family via CPM (Caching.Memory 11.0.0), so it lands
  as a sibling pin.
- Cons: pure service registration (no convention scanning, no env-var
  binding — irrelevant for a desktop app), and someone must still write the
  registration list by hand — which is exactly the "two composition orders"
  problem above, just moved into a new file.

**Option 2 — your `InversionOfControl` library (AddictedProxy repo).**

- Verified shape (read from `/home/balor/workspace/AddictedProxy/InversionOfControl/`):
  - it is a **conventions layer over MS.DI** — `Microsoft.Extensions.DependencyInjection`
    is itself one of its dependencies; it does not replace the container.
  - `IBootstrap` / `IBootstrapConditional` classes are auto-discovered per
    assembly and register their own services — each domain adds one
    `BootstrapX` class next to its services instead of one central registry.
  - `[ServiceLifetime]` attribute, `IBootstrapEnvironmentVariable<,>` env-var
    binding (web-service oriented), an `EnumFactory` source generator
    (`InversionOfOkontrol.Generator`) and a test project with unit coverage.
  - Hosted assumptions: `ILoggingBuilder`, `Microsoft.AspNetCore.Hosting.Abstractions`,
    `Microsoft.Extensions.Options.ConfigurationExtensions`, `Polly.Core` —
    the public API surface of `BootstrapRegister.RegisterBootstrapServices`
    takes `ILoggingBuilder` and `IConfiguration`, i.e. the library is built
    around a **generic host / ASP.NET hosting** model, which a WinForms app
    does not have.
  - Published? To adopt it, SoundSwitch would consume it as a NuGet package
    (published to a feed the repo can consume) or as a git submodule / source
    lift; it is not currently on nuget.org under that name (unverified — Q5).
  - Not tracked by Renovate (SoundSwitch's Renovate config covers nuget.org
    packages); an unpublished private library pin is a supply-chain and
    maintenance liability for a desktop app shipped to ~1M users (per README).
  - Lifecycle: currently author-maintained version-coupled to your other
    project; a bug fix must ship through both repos.

**Decision (Q1, resolved 2026-11-21):** **Option 1, MS.DI alone, for
SoundSwitch.** The decisive factors are hosting model and maintenance: the
IoC library is built
around a generic host with `IConfiguration`/`ILoggingBuilder` and env-var
bindings — a shape SoundSwitch (a tray app whose configuration is a
serialized `.json` via `AppConfigs`, not env vars) doesn't have, and each of
its extra dependencies (`Polly.Core`, Hosting.Abstractions) would be pulled
into a WinForms app that already carries a sizeable dependency surface. The
`IBootstrap` convention and enum factories are genuinely nice, but they solve
registration-list DRY, which MS.DI's explicit registration solves adequately
for a registry that will hold ~15–20 services. KISS wins after DRY: the
convention layer is not worth its hosting-model coupling for this app.
Your own library remains the right choice where it already lives — a hosted
web app with env-var configuration; lifting it here would be DRY-run-amok
(wrong unification: two call sites with different futures).

**If Q1 selects Option 2**, the adoption shape would be: extract
`InversionOfControl` to a standalone repo/NuGet package with the
ASP.NET-hosting dependencies made optional, trim to `IBootstrap` scanning +
`[ServiceLifetime]`, publish, then consume via CPM pin — a separate
mini-project before the refactor starts, and the ADR records that as a
prerequisite.

### C. The startup sequence (replaces `InitializeMain`)

`AppBootstrapper` owns the ordered steps:

1. `WindowsAPIAdapter.Start()` — already in `Program.cs`, stays.
2. Config load + migration (`SwitchForegroundProgram` cleanup path).
3. `AppConfigs` load happens once in `AppBootstrapper` (not lazily in
   property getters).
4. Build the container: `ServiceCollection` with explicit registrations
   (single registry file, single source of truth for the graph) —
   `IDeviceService → DeviceService`, `INotificationSettings →
   NotificationSettingsService`, `IAppSettings → SettingsService`,
   `IAppInfrastructure` façade, `TrayIcon`, `ProfileManager`,
   `AppSoundLockManager`, `NotificationManager`, `AudioDeviceLister`
   (cached), `MMNotificationClient`, `JobScheduler`, `AudioSwitcher`,
   update checker, hotkey handler, mute-resolver chain.
5. Resolve root: `AppBootstrapper.Run()` creates `SoundSwitchApplicationContext`
   and `Application.Run(context)`.
6. On exit: dispose in reverse order (container `Dispose` handles it —
   single teardown path instead of `Program.cs`'s manual `AppModel.Dispose()`
   + `MMNotificationClient` + `JobScheduler` + `NamedPipe` shutdown list).

The mute-banner startup reconcile (AppModel.cs:209-221) becomes an explicit
step with its own method in `DeviceService`, keeping its documented
"never abort init on failure" comment and behavior.

The two mutexes, Sentry init, single-instance pipe check, theme/language
early init, and exception handlers stay in `Program.cs` — they are process
concerns, not service concerns.

**Sequencing rule (recorded in the decision log):** the startup order that
`InitializeMain` encodes as comments becomes explicit, ordered
`AppBootstrapper` steps with the ordering rationale kept as comments at each
step — the same knowledge, moved from scattered comments into the bootstrapper
where the order is enforced by statement order.

### D. Testability gains

- `NotificationManagerMuteTests` fakes (`FakeNotificationSettings`,
  `FakeInfrastructure`) stay valid — the interfaces are unchanged. The
  fakes gain a new use: they can now be composed with the REAL
  `DeviceService` in tests via the container or plain construction, which is
  impossible today (AppModel's ctor is private; `InitializeMain` throws on
  re-entry).
- New tests possible: a `BootstrapperOrderTests` that constructs the
  container and asserts the registration list matches the expected graph,
  and per-service unit tests with fakes for the state that used to be
  private to the partial class.
- The win is not "tests for everything" but **composition tests**: today
  `InitializeMain` has zero coverage; after the refactor, the wiring itself
  is testable for the first time.

### E. Is it worth it? (the maintainability verdict)

Yes, but scoped. The verdict per cost:

- **The partial split is cosmetic — the real problem is the shared-state
  god class + 317-site service locator.** A pure "split into partials"
  already happened and did not fix coupling; partials share state, so the
  boundary is not compiler-enforced.
- **The DI container itself is the smallest part of the win.** The win is
  (a) compiler-enforced boundaries via constructor injection, (b) a single
  visible ordered startup, (c) a container-enforced teardown (reverse
  disposal), (d) testable composition. A container is the mechanism, not
  the goal; the same win could theoretically be achieved by pure manual
  constructor injection in `Program.cs`, but ~20 services with interleaved
  lifetimes make a hand-written factory a second god method.
- **Cost estimate:** 317 reference migrations (mechanical, sed-able per
  file) + 4 service extractions (each small, one partial at a time) + 1
  bootstrapper (the only genuinely new design) + test updates. The dominant
  risk is the startup ordering hazard (mute banners, hotkey registration
  failure tolerance, config migration) — mitigated by keeping the existing
  ordering comments as explicit bootstrapper step comments and by the
  existing test suite as a regression net.
- **Net:** the refactor is worth doing for maintainability; the codebase
  already chose the seam (sub-interfaces + test fakes), so this is
  formalizing an existing architecture, not inventing one. But it is a
  phased single-PR rollout (one commit per phase), not a one-shot rewrite.

### Rollout (resolved — one PR, phased commits)

The owner directed a **single PR** carrying the whole refactor (review is
LLM-based, so the four merge points collapse into one). Clean-history rules
still apply **inside** the PR: one conventional commit per phase, in this
order, each phase buildable before the next starts:

1. **Commit 1 (mechanical, no behavior change):** add the
   `Microsoft.Extensions.DependencyInjection` CPM pin; introduce
   `ServiceCollection` in `Program.cs`; register the four sub-interface
   implementations + TrayIcon/ProfileManager/AppSoundLockManager/
   NotificationManager; resolve them in `SoundSwitchApplicationContext`;
   keep `AppModel.Instance` as a façade delegating to resolved services.
   `AppModel.Instance` call sites unchanged.
2. **Commit 2:** split the four partials into four service classes (each
   partial file becomes its own class taking dependencies by constructor;
   shared private state split by ownership). `AppModel` becomes a thin
   delegating façade for compatibility; `IAppModel` stays as the migration
   façade (Q2).
3. **Commit 3:** migrate the 317 `AppModel.Instance` references to
   constructor injection, mechanical per-file edit (UI forms → ctor param
   via the static factory per Q4; Framework managers → ctor param).
   Migration order within the commit: UI forms → Framework → Services →
   tests. This commit retires the service locator; `AppModel.Instance` and
   the private parameterless path are deleted at its end.
4. **Commit 4 (optional polish, per Q3 = defer):** none — the update
   checker stays a singleton exactly as today. Instead this commit adds
   composition tests (`BootstrapperOrderTests`-style: build the container,
   assert the registration graph, assert dispose order) and removes any
   dead scaffolding left by the migration.

Each commit must keep the solution buildable and the full test suite
green on the win11-dev VM build gate before the next commit starts.

### Non-goals

- No new settings, no user-visible behavior change; banner reconcile and
  hotkey failure tolerance keep their current semantics.
- No change to `AppConfigs.Configuration` persistence format.
- No splitting of `SoundSwitch.UI.Menu`/`UserControls` or other projects.
- No migration of `SoundSwitch.Tests` off interfaces onto container-based
  tests; fakes stay.
- Not solving `QuickMenuManager` / `JobScheduler` singletons (separate issue,
  only AudioSwitcher/MMNotificationClient get container registration so the
  bootstrapper owns their teardown).

### Decision log (DRY > SOLID > KISS)

1. **Keep `AppConfigs.Configuration` as the one settings store (DRY).**
   New services are façades over it; no second copy of settings state.
   Loser: per-service settings state — would duplicate the store and
   re-invent save/migration timing.
2. **Constructor injection even in WinForms designer files (SOLID/DIP).**
   Loser: property injection — would allow half-constructed designer objects
   at runtime.
3. **MS.DI over your IoC library (KISS after DRY).** The library's
   convention scanning duplicates what an explicit registration list does
   for ~20 services, and its host/env-var dependencies drag web-hosting
   assumptions into a tray app. Loser: registration-list DRY via
   `IBootstrap` — real, but small here; your library keeps solving it in
   AddictedProxy where the shape fits.

### Resolved questions (owner decisions, 2026-11-21)

- **Q1 (container) — MS.DI alone.** The InversionOfControl library was
  evaluated in full (see §B) and set aside: it is a conventions layer over
  MS.DI built around a hosted/env-var configuration shape that does not
  fit a WinForms tray app, and it is not published where SoundSwitch's
  Renovate can track it. It may be revisited if it is ever extracted as a
  standalone package with the hosting dependencies optional.
- **Q2 (IAppModel façade) — keep.** `IAppModel` stays as the aggregate
  façade for the whole migration; it is deleted only at the end of Commit 3
  if no caller still needs the aggregate (deferred to implementation
  judgment, recorded here so the deletion is a decision, not an accident).
- **Q3 (update checker lifetime) — defer.** Stays a singleton exactly as
  today, on its existing `LimitedConcurrencyLevelTaskScheduler(1)`; the
  mute-resolution chain stays in `DeviceService`.
- **Q4 (UI forms) — static factory.** Forms take services via constructor
  and expose `SettingsForm.Factory()`-style static factories for designer
  compatibility, matching the repo's existing pattern.
- **Q5 (rollout) — one PR.** Single PR, four phased commits inside it
  (see Rollout); LLM review, not per-phase human merge gates.
- **Q6 (IoC library consumption) — moot.** Q1 chose MS.DI; no submodule,
  feed, or vendoring is introduced.
- **Q7 (CLI) — out of scope, recorded.** `SoundSwitch.CLI` has no
  dependency on `SoundSwitch.Model` today (verified by grep);
  container-based composition keeps the door open for a future CLI
  composition root without designing for it now.

## Consequences

- Compiler-enforced service boundaries where partials shared state; the
  touch-anything-break-anything failure mode ends.
- A single, ordered, testable startup (`AppBootstrapper`) replaces the
  scattered composition across `Program.cs` + `SoundSwitchApplicationContext`
  + `InitializeMain`; the mute-banner reconcile hazard becomes an explicit
  step with its rationale comment kept.
- `AppModel.Instance` is retired by attrition across the PR sequence and
  deleted at the end; service-locator drift (317 sites) ends with it.
- Test composition becomes possible for the real service classes with
  fakes via plain construction — the interfaces are unchanged, so existing
  fakes stay valid.
- Dependency added: `Microsoft.Extensions.DependencyInjection` (Q1), as a
  CPM sibling pin (the family is already pinned at 11.0.0 for Caching).
- Teardown becomes container-owned reverse disposal; `Program.cs`'s manual
  dispose chain shrinks to container Dispose + process concerns.

## Non-goals (consolidated)

- No user-visible behavior change of any kind; existing event semantics,
  mute-banner reconciliation, hotkey failure tolerance, config migration
  path, and `UpdateMode` semantics are preserved.
- No change to `AppConfigs.Configuration` persistence format.
- No restructuring of `SoundSwitch.UI.Menu`, `SoundSwitch.UI.UserControls`,
  other projects, or the `SoundSwitch.CLI` (no model dependency today,
  verified by grep).
- No container-migration of `QuickMenuManager`/`JobScheduler` singletons
  beyond registration needed for teardown ownership.
- No migration of tests off interface-fakes onto container resolution;
  fakes stay the standard seam.
