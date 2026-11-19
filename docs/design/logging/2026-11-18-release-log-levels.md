# Release log-level policy: gate shipped-build log verbosity on `DEBUG`

**Status:** Proposed
**Author:** pi agent (release-logs campaign)
**Branch:** `fix/release-log-levels`
**Base:** `origin/dev` (`71f45a10`)

---

## 1. Problem

Users of **shipped builds** (Release / Beta / Nightly) see debug-grade messages in
`%LOCALAPPDATA%`-adjacent `ApplicationPath.Logs/soundswitch.log`, e.g. per-PID
"Access is denied" chatter:

- `SoundSwitch/UI/Forms/ProcessSelectionForm.cs:253` — `Failed to get process path for {ProcessName} (PID: {PID})` (Win32Exception)
- `SoundSwitch/Services/AppSoundLockManager.cs:242/266/293/594/640` — `Couldn't … process PID {PID}` on every protected/system process the app-rule subsystem inspects.

Root cause in `SoundSwitch/Framework/Logger/Configuration/LoggerConfigurator.cs`:

```csharp
Log.Logger = new LoggerConfiguration()
#if RELEASE
                    .MinimumLevel.Debug()
#else
            .MinimumLevel.Verbose()
#endif
```

The gate is keyed on `RELEASE`, but the csproj configurations
(`SoundSwitch/SoundSwitch.csproj` lines 13–33) do **not** all define it:

| Configuration | `DefineConstants` | Preprocessor branch taken | Effective file-sink min level |
|---|---|---|---|
| Debug | `TRACE;DEBUG` | `#else` | Verbose (intended — local dev) |
| Release | `RELEASE` | `#if RELEASE` | **Debug** (the reported bug) |
| Beta | `TRACE;BETA` | `#else` | **Verbose** (worse than Release!) |
| Nightly | `TRACE;NIGHTLY` | `#else` | **Verbose** |

So **all three** shipped configurations write Debug/Verbose events to
`soundswitch.log`, and Beta/Nightly are noisier than Release — the exact opposite
of the intended policy.

## 2. Goals

1. Shipped builds (Release, Beta, Nightly) write `Information` and above to the
   rolling file sink; Debug/Verbose events are not logged.
2. The local `Debug` configuration keeps `Verbose` — developer workflow unchanged.
3. Minimal, targeted change: one preprocessor gate, no `_logger.Debug` call-site
   rewrites.

## 3. Non-goals

- Rewriting the ~14 `_logger.Debug` call sites in the app project (plus
  `Log.Debug` in `Program.cs`/`TrayIcon.cs` and `SoundSwitch.Audio.Manager`); the
  level change covers them by design.
- Log file path, retention, or rolling policy (`ApplicationPath.Logs`,
  `RollingInterval.Day`, 3 retained files) — unchanged.
- Sentry SDK initialization or event level (`MinimumEventLevel = Error`).
- Console sink (already `#if DEBUG`-only).
- Introducing runtime log-level configuration (verbosity setting/CLI flag).

## 4. UX

User-visible behaviour change: the log file produced by shipped builds and shared
via **Troubleshooting → Export Log Files** becomes materially smaller and
readable — it now contains `Information`+ events: application start/shutdown,
version/OS lines, device-switch outcomes (`ADeviceCycler.cs:97/111` —
`Set Default device: {Device}`), app-rule matches (`AppSoundLockManager.cs:519` —
`MATCH: Rule for … matched process …`), profile triggers
(`ProfileManager.cs:214`), warnings and errors. Per-PID access-denied and other
debug-grade diagnostics no longer appear. Local `Debug` builds are unchanged
(full detail for development). No UI surface, setting, or string changes.

## 5. Investigation

- Verified the `DefineConstants` per configuration in
  `SoundSwitch/SoundSwitch.csproj` (table in §1): `RELEASE` is defined **only**
  for the Release configuration; Beta/Nightly fall into `#else` (Verbose).
- Audited every Debug-level emitter to check what a shipped `Information` floor
  would drop:
  - `SoundSwitch/UI/Forms/ProcessSelectionForm.cs:253/308/325`,
    `SoundSwitch/Services/AppSoundLockManager.cs:242/266/293/326/594/640/666`,
    `SoundSwitch/Framework/Profile/ProfileManager.cs:271/276/280/749` — all
    diagnostic chatter or handled per-process failures; nothing support-critical.
  - `SerilogTraceListener` 3.2.0 maps `System.Diagnostics.Trace.Write/WriteLine`
    to Serilog **Debug** (verified empirically with a net10.0 harness against the
    exact NuGet package). Affected `Trace.WriteLine` call sites:
    `SoundSwitch/Framework/WinApi/WindowsAPIAdapter.cs:141/240` ("Thread Race
    Condition …" — self-documented benign, handled races: "Can happen when the
    instance got dispose[d] in its own thread…") and
    `SoundSwitch.Audio.Manager/AudioSwitcher.cs:96/157` ("Default endpoint
    already {deviceId}" — no-op-skip diagnostics). All four are diagnostics; none
    warrants promotion.
  - Device-switch breadcrumbs, the one class of Debug output with potential
    support value, are already `Information`: `ADeviceCycler.cs:97/111`,
    `AppSoundLockManager.cs:372/519/604`, `ProfileManager.cs:214`.
- Sentry sink interaction: Serilog's global `MinimumLevel` gates the pipeline
  **before** any sink, so per-sink minimums (`MinimumBreadcrumbLevel = Debug`)
  can only lower, not raise, the global floor. After this change the shipped
  effective breadcrumb floor becomes Information. Error events are unaffected,
  and Sentry's own default breadcrumb level is Info; the sink settings stay as
  they stand per the owner's direction.
- Website check: `website/src/advanced/troubleshooting.md` and
  `website/src/usage/app-rules/troubleshooting.md` describe **Export Log Files**
  (where to find/export the zip) but no page documents log verbosity, levels, or
  log-file contents — no website change needed.

## 6. Design

### 6.1 Behaviour

`LoggerConfigurator.ConfigureLogger()` keys the pipeline minimum level on
`DEBUG` — the only configuration that should ever see debug-grade output —
instead of `RELEASE`:

```csharp
Log.Logger = new LoggerConfiguration()
#if DEBUG
            .MinimumLevel.Verbose()
#else
            .MinimumLevel.Information()
#endif
```

`DEBUG` is defined only by the Debug configuration (`DefineConstants:
TRACE;DEBUG`), so Debug → Verbose, and Release/Beta/Nightly → Information.
No other statement in the method changes; the console sink remains
`#if DEBUG`-only and the Sentry sink settings remain untouched.

## 7. Telemetry

No new events. The Sentry sink continues to report `Error`+ events
(`MinimumEventLevel = Error`); shipped-build breadcrumbs effectively become
Information+ (see §5).

## 8. Localization

No new user-facing strings — the change alters log file contents only.

## 9. Files touched

| File | Change |
|---|---|
| `docs/design/logging/2026-11-18-release-log-levels.md` | This ADR. |
| `SoundSwitch/Framework/Logger/Configuration/LoggerConfigurator.cs` | Re-key the pipeline minimum level from `#if RELEASE … Debug #else … Verbose` to `#if DEBUG … Verbose #else … Information`. |

## 10. Test plan

### Automated

No new tests: the policy is a preprocessor gate inside `ConfigureLogger()`
(interacting with csproj `DefineConstants`), which the existing test projects do
not exercise. The test projects are Windows-only and cannot run on the Linux
validation box.

### Manual

1. `dotnet build SoundSwitch/SoundSwitch.csproj -c Debug -p:LinuxBuild=true -p:BuildProjectReferences=false` (Linux gate).
2. Windows: build all four configurations; confirm `soundswitch.log` min level
   per configuration: Debug → Verbose; Release, Beta, Nightly → Information
   (no `[DBG]`/`[VRB]` lines; `Set Default device: …` lines still present after
   a switch; per-PID "Couldn't … process PID" lines absent while protected
   processes are inspected).
3. Confirm a crash still produces a Sentry Error event.

## 11. Decisions

| # | Decision | Rationale |
|---|---|---|
| 1 | Single global `#if DEBUG / Verbose / #else / Information / #endif` pipeline gate. | One line of policy, covers every sink and every shipped configuration by construction. Rejected: per-source `MinimumLevel.Override` entries — would need maintaining a list of chatty sources and still misses future ones. |
| 2 | Key on `DEBUG`, not `RELEASE`. | `RELEASE` excludes Beta/Nightly, which is exactly the bug class that produced the Beta/Nightly Verbose surprise. `DEBUG` is defined by the only configuration that should see debug-grade output. |
| 3 | Floor shipped builds at `Information`, not `Debug`. | Debug-grade output is the reported noise; switch events, warnings, and errors are all already `Information`+. Rejected: Debug floor — leaves the reported chatter in shipped logs. |
| 4 | No `_logger.Debug` call-site rewrites or promotions. | The audit (§5) found no support-critical message below Information: switch outcomes and rule matches are already Information; all Debug emitters are chatter or handled benign conditions. |
| 5 | Accept that shipped Sentry breadcrumbs become effectively Information+. | The global minimum gates all sinks; per-sink `MinimumBreadcrumbLevel = Debug` cannot re-open what the pipeline dropped. Sentry's default breadcrumb level is Info and error events are unaffected; sink settings stay untouched per owner direction. Rejected: keeping the global floor at Debug + per-sink `restrictedToMinimumLevel: Information` on the file sink — preserves Debug breadcrumbs but keeps the shipped pipeline creating/enriching Debug events and adds a second preprocessor block, for negligible support value. |
| 6 | No website change. | No `website/src/**` page documents log verbosity or log-file contents (only log *export*), so nothing becomes inaccurate. |

## 12. Future work

- Runtime log-level control (verbosity setting or CLI flag) for support cases
  that need debug-grade detail from a shipped build — deferred; not requested.
- Promote `WindowsAPIAdapter` race-condition notes out of `Trace.WriteLine` if
  they ever become support-relevant — currently benign by their own comments.
