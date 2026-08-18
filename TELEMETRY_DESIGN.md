# Telemetry Feature Usage — Design Document

**Project:** SoundSwitch  
**Author:** (to be filled by implementor)  
**Status:** Draft  
**Date:** 2026-08-18  
**Scope:** Add feature-usage telemetry to the existing Sentry integration, gated by the user's `Telemetry` setting, and document it for end users on the website and in Terms.md.

---

## 1. Goals

- Understand **which features real users use** (profiles vs hotkeys, which notifications, CLI adoption, etc.).
- Do this **without adding a second analytics system** — extend the already-integrated Sentry.
- **Respect the existing `Telemetry` toggle** — when off, zero telemetry leaves the machine.
- Document clearly for end users **what is sent, why, and how to disable it**.

---

## 2. Non-Goals

- No new analytics vendor (Matomo, PostHog, etc.) in this iteration.
- No PII: no device names, no file paths, no profile content, no user identity beyond the existing `UniqueInstallationId`.
- No change to the existing crash-reporting path (Sentry errors continue to work independently).

Note: `Environment.UserName` is already sent as `SentryUser.Username` for crash-report labeling purposes; this is disclosed in the privacy documentation and Terms.

---

## 3. Existing State — What We Already Have

### 3.1 Sentry Integration (Program.cs)

| Item | Detail |
|------|--------|
| Package | `Sentry.Serilog` (transitively pulls `Sentry`) |
| DSN | Hard-coded in `Program.cs:64` |
| Environment | `AssemblyUtils.GetReleaseState()` → `Stable` / `Beta` / `Nightly` |
| Release | `{Application.ProductName}@{Application.ProductVersion}` |
| Session tracking | `AutoSessionTracking = AppConfigs.Configuration.Telemetry` (line 69) — already gated |
| User ID | `SentryUser.Id = AppConfigs.Configuration.UniqueInstallationId.ToString()` (line 73) |
| Username | `Environment.UserName` sent as `SentryUser.Username` (line 74) — used as a label on crash reports to help distinguish users during debugging |

**Key observation:** The existing telemetry description in `Terms.md` says "only version shared anonymously", but the code already sends `UniqueInstallationId` (a per-install GUID) and `Environment.UserName`. The design doc and website copy must reflect what the code *actually* does, not an outdated description.

### 3.2 Settings / Persistence

```
AppModel.Telemetry  →  AppConfigs.Configuration.Telemetry  →  SoundSwitchConfiguration.json
```

- `ISoundSwitchConfiguration.Telemetry` (interface, line 108 of ISoundSwitchConfiguration.cs)
- `SoundSwitchConfiguration.Telemetry` defaults to `true` (line 81)
- `AppModel.Telemetry` getter/setter reads/writes config + calls `AppConfigs.Configuration.Save()` (Model/AppModel.AppSettings.cs lines 64-72)
- UI binding: `telemetryCheckbox.DataBindings.Add(..., AppModel.Instance, nameof(AppModel.Telemetry), ...)` (Settings.cs line 274)
- Persistence: `ConfigurationManager.SaveConfiguration()` writes `SoundSwitchConfiguration.json` to `ApplicationPath.Default` (JSON file, Newtonsoft.Json)

### 3.3 What This Means for the Design

- **No new settings UI** — the checkbox already exists.
- **No new persistence** — `AppConfigs.Configuration.Telemetry` is the single source of truth.
- **The gate is simple:** read `AppConfigs.Configuration.Telemetry` at the entry point of every Track* call.

---

## 4. Architecture

### 4.1 Single Entry Point: `TelemetryService`

All telemetry calls go through one static class. It owns:

1. The **enabled check** (reads `AppConfigs.Configuration.Telemetry`).
2. **Counter/Gauge/Distribution** methods that wrap `SentrySdk.Metrics.EmitCounter/ EmitGauge/ EmitDistribution` with `KeyValuePair<string, object>` attributes.
3. **Breadcrumb** methods that wrap `SentrySdk.AddBreadcrumb(...)`.
4. **Optional offline buffer** (see §4.3).

```

The Sentry SDK version in use is `Sentry.Serilog` 6.9.0 (transitively pulls `Sentry` 6.9.0). Metrics are GA and available directly on `SentrySdk.Metrics` — no `Experimental` prefix needed (the `Experimental` namespace was removed after 6.1.0). API signature:

```csharp
// Counter — record one occurrence
SentrySdk.Metrics.EmitCounter("soundswitch.playback.switched", 1,
    new KeyValuePair<string, object>("trigger", "hotkey"));

// Distribution — record a numeric value with optional unit
SentrySdk.Metrics.EmitDistribution("soundswitch.devices.count", 3, MeasurementUnit.None,
    new KeyValuePair<string, object>("device_type", "playback"));

// Gauge — snapshot a value
SentrySdk.Metrics.EmitGauge("soundswitch.queue.depth", 5, MeasurementUnit.None,
    new KeyValuePair<string, object>("queue", "profile_activation"));
```

Supported value types: `byte`, `short`, `int`, `long`, `float`, `double`. Attributes accept `string`, `bool`, integer up to 64-bit, and floating-point up to 64-bit.

Default attributes are auto-attached: `environment`, `release`, `sdk.name`, `sdk.version`, plus `user.id` and `user.name` from the current scope.
┌─────────────────────────────────────────────────────┐
│                    TelemetryService                 │
│  static volatile bool _enabled                     │
│  static void Reload()        ← called on settings  │
│  static bool IsEnabled()                             │
│                                                     │
│  TrackPlaybackSwitch(trigger)    → Metrics.Counter  │
│  TrackRecordingSwitch(trigger)  → Metrics.Counter  │
│  TrackMicMute(trigger, muted)   → Metrics.Counter  │
│  TrackProfileActivated(…)       → Metrics.Counter  │
│  TrackProfileCreated()          → Metrics.Counter  │
│  TrackProfileDeleted()          → Metrics.Counter  │
│  TrackProfileActivationFailed(…) → Metrics.Counter │
│  TrackNotificationBanner(…)     → Metrics.Counter  │
│  TrackNotificationWindows()     → Metrics.Counter  │
│  TrackNotificationSound()       → Metrics.Counter  │
│  TrackCliCommand(cmd, exitCode) → Metrics.Counter  │
│  TrackDevicesEnumerated(type, count) → Metrics.Distribution │
│                                                     │
│  Breadcrumb(category, message)  → AddBreadcrumb    │
└─────────────────────────────────────────────────────┘
```

### 4.2 Enabled Gate

```csharp
public static class TelemetryService
{
    private static volatile bool _enabled;

    public static void Reload()
    {
        _enabled = AppConfigs.Configuration.Telemetry;
    }

    private static void EnsureEnabled()
    {
        if (!_enabled) return; // no-op
    }

    public static void TrackPlaybackSwitch(string trigger)
    {
        EnsureEnabled();
        SentrySdk.Metrics.Counter("soundswitch.playback.switched", 1,
            new Dictionary<string, string> { ["trigger"] = trigger });
    }
}
```

- `_enabled` is `volatile` so reads on hotkey threads see the latest write.
- `Reload()` is called: (a) on app startup, (b) when the settings form saves the telemetry checkbox.
- **No Sentry reinitialization needed** — `AutoSessionTracking` is set once at startup from the same config value. If the user toggles telemetry off mid-session, sessions already in flight may still send; this is acceptable (the setting takes effect for future sessions and future metric events).

### 4.3 Offline Buffer (Optional — Defer Decision)

Sentry's SDK has an internal event queue. For a background WinForms app that is usually online, this is sufficient. Two shutdown considerations:

1. **On exit:** `SentrySdk.FlushAsync(TimeSpan.FromSeconds(2))` before `Application.Exit()` — already partially done via `SentrySdk.EndSession()` at Program.cs:177.
2. **Extended offline:** if the app runs for days without internet and we want to keep telemetry, add a local `ConcurrentQueue<PendingMetric>` with a background timer that drains when connectivity returns.

**Decision:** Defer the offline buffer to a follow-up. The built-in queue + `FlushAsync` on shutdown is sufficient for v1.

### 4.4 What Gets Sent — Data Model

Every metric event includes:

| Field | Source |
|-------|--------|
| Metric name | e.g. `soundswitch.playback.switched` |
| Value | `1` for counters, numeric for distributions |
| Attributes (key-value) | `trigger`, `device_type`, `command`, etc. — categorical only |
| Release | Auto-attached by Sentry SDK (`Application.ProductVersion`) |
| Environment | Auto-attached (`Stable`/`Beta`/`Nightly`) |
| DSN project | Configured in Program.cs |

**No user-attached data beyond what Sentry already sends** (`UniqueInstallationId`, `Environment.UserName`). No device names, no profile content, no file paths.

---

## 5. Feature → Metric Mapping

### 5.1 Core Switching

| Trigger | Metric | Type | Attributes |
|---------|--------|------|------------|
| Playback switch via hotkey | `soundswitch.playback.switched` | Counter | `trigger: hotkey` |
| Playback switch via tray double-click | `soundswitch.playback.switched` | Counter | `trigger: tray` |
| Playback switch via CLI | `soundswitch.playback.switched` | Counter | `trigger: cli` |
| Recording switch via hotkey | `soundswitch.recording.switched` | Counter | `trigger: hotkey` |
| Recording switch via CLI | `soundswitch.recording.switched` | Counter | `trigger: cli` |
| Mic mute via hotkey | `soundswitch.mic.muted` / `soundswitch.mic.unmuted` | Counter | `trigger: hotkey` |
| Mic mute via banner click | `soundswitch.mic.unmuted` | Counter | `trigger: banner` |
| Mic mute via CLI | `soundswitch.mic.muted` / `.unmuted` | Counter | `trigger: cli` |

**Breadcrumb (not metric):** each hotkey press → `Breadcrumb("hotkey", "PlaybackHotKey pressed")` — too granular for metrics, useful context.

### 5.2 Profiles

| Event | Metric | Type | Attributes |
|-------|--------|------|------------|
| Profile activated | `soundswitch.profile.activated` | Counter | `trigger_type: hotkey\|app\|window\|steam\|uwp\|startup`, `profile_id: <hash>` |
| Profile created | `soundswitch.profile.created` | Counter | — |
| Profile deleted | `soundswitch.profile.deleted` | Counter | — |
| Profile activation failed | `soundswitch.profile.activation_failed` | Counter | `reason: device_unavailable\|error\|timeout` |

**`profile_id` is a hash, not the name.** We don't want to send profile names (they can contain user-chosen strings). Use a one-way hash (e.g. `SHA256(profileName).Substring(0,8)`) so we can count activations per profile without knowing the name.

### 5.3 Notifications

| Event | Metric | Type | Attributes |
|-------|--------|------|------------|
| Banner shown | `soundswitch.notification.banner_shown` | Counter | — |
| Banner unmute clicked | `soundswitch.notification.banner_unmute_clicked` | Counter | — |
| Windows notification shown | `soundswitch.notification.windows_shown` | Counter | — |
| Sound notification played | `soundswitch.notification.sound_played` | Counter | — |

### 5.4 CLI

| Event | Metric | Type | Attributes |
|-------|--------|------|------------|
| CLI command invoked | `soundswitch.cli.command` | Counter | `command: switch\|mute\|profiles\|settings\|...`, `exit_code: 0\|1\|...` |

CLI is a separate process. It sends a `CliCommandExecuted` message via NamedPipe IPC to the running SoundSwitch instance, which records the event through `TelemetryService` in `SoundSwitchApplicationContext.HandlePipeMessageAsync`. **Decision: use IPC** — this avoids coupling the CLI to the framework assembly and prevents double counting, since only the running instance processes the event.

### 5.5 System / Background

| Event | Metric | Type | Attributes |
|-------|--------|------|------------|
| Devices enumerated | `soundswitch.devices.count` | Distribution | `device_type: playback\|recording` |
| App startup | (Sentry session — already covered by `AutoSessionTracking`) | — | — |

### 5.6 What NOT to Track

- Every settings pane opened (too noisy — breadcrumb only if useful for debugging).
- Individual profile field changes (privacy-adjacent, no value).
- Per-device switch counts with device names (PII-adjacent).
- Application focus events at high frequency (noise).

---

## 6. Hook Locations — Where to Place Calls

| Code location | What to add | File (approximately) |
|---------------|-------------|----------------------|
| Hotkey hook handler (global key press) | `TrackPlaybackSwitch("hotkey")` / `TrackRecordingSwitch("hotkey")` / `TrackMicMute("hotkey", muted)` + breadcrumb | Framework/WinApi/Keyboard/* |
| Tray icon double-click handler | `TrackPlaybackSwitch("tray")` or breadcrumb depending on configured action | Framework/TrayIcon/* |
| Profile activation success path | `TrackProfileActivated(triggerType, profileNameHash)` | Framework/Profile/* |
| Profile activation failure path | `TrackProfileActivationFailed(reason)` | Framework/Profile/* |
| Profile CRUD (add/delete) | `TrackProfileCreated()` / `TrackProfileDeleted()` | Framework/Profile/* or UI |
| Banner show | `TrackNotificationBanner("shown")` | Framework/Banner/* |
| Banner unmute click | `TrackNotificationBanner("unmute_clicked")` | Framework/Banner/* |
| Windows notification | `TrackNotificationWindows()` | Framework/NotificationManager/* |
| Sound notification played | `TrackNotificationSound()` | Framework/NotificationManager/* |
| CLI command dispatch | `TrackCliCommand(commandName, exitCode)` | SoundSwitch.CLI/* |
| App startup (after Sentry init) | `TelemetryService.Reload()` — already done implicitly via Program.cs reading config | Program.cs |
| Settings form save (telemetry checkbox) | `TelemetryService.Reload()` | Settings.cs or AppModel.Telemetry setter |

**Note on AppModel.Telemetry setter:** The setter already calls `AppConfigs.Configuration.Save()`. Adding `TelemetryService.Reload()` there makes the toggle effective immediately without needing a separate wire from the UI.

---

## 7. Privacy & Documentation

### 7.1 What the User Sees Today

- **Settings UI:** "Telemetry" checkbox with tooltip: *"Gather anonymously which version of SoundSwitch is in use. Only shared with the developer of SoundSwitch."* (SettingsStrings.resx)
- **Terms.md §Telemetry terms:** *"By having the telemetry enabled, you agree to have the version of SoundSwitch you have installed be shared with Us anynoumously using the service provided by Sentry. This information is only used as a way to gather the adoption of new version of the sofware. Data gathered: Version of SoundSwitch"*

### 7.2 What the Code Actually Sends Today

- App version / release / environment (via Sentry SDK)
- `UniqueInstallationId` (per-install GUID) as Sentry user ID
- `Environment.UserName` as Sentry username
- Session count (when `AutoSessionTracking` is on)

**Gap:** The user-facing description is narrower than what's actually sent. The design must fix this — either reduce what's sent to match the description, or expand the description to match the code. Given that `UniqueInstallationId` and `Environment.UserName` are already sent and the DSN is hard-coded to a specific Sentry project, the pragmatic path is to **update the documentation to be accurate**.

### 7.3 What the Expanded Telemetry Sends

In addition to the above, after this feature:

- Counters for feature usage (playback switches, profile activations, notifications, CLI commands) — categorical attributes only, no free-text user content.
- Distribution for device counts at enumeration time.
- Breadcrumbs for hotkey presses and UI interactions (attached to sessions/issues, not standalone events).

**Still not sent:** device names, profile names/content, file paths, application names (except as categorical "app trigger" types), any network identifiers.

### 7.4 Documentation Deliverables

1. **Website: new page** `website/src/privacy/telemetry.md` — short, honest, explains Sentry, what's sent, how to disable.
2. **Website: update** `website/src/configuration/general.md` Telemetry section — replace the one-liner with a link to the privacy page and a 2-sentence summary.
3. **Terms.md:** update §Telemetry terms to reflect the expanded scope (feature usage counters + breadcrumbs, still no PII beyond what was already sent).
4. **SettingsStrings.resx tooltip:** optionally update the tooltip text to point to the website privacy page or give a slightly more accurate description. (Defer — non-blocking for v1.)

---

## 8. Website Copy — Privacy Page

**File:** `website/src/privacy/telemetry.md`

Frontmatter:
```yaml
---
title: Telemetry and Privacy
description: What SoundSwitch telemetry collects, why, and how to disable it.
---
```

**Navbar addition** in `website/src/.vuepress/config.ts`:
```ts
{ text: "Privacy & Telemetry", link: "/privacy/telemetry.md" }
```
Place in the top-level navbar, near "FAQ" or "Advanced".

**Content (draft):**
- Short paragraph: SoundSwitch sends anonymized usage data to help understand which features are used.
- Vendor: Sentry (sentry.io). Link to Sentry's privacy policy.
- What is sent: app version, release channel (Stable/Beta/Nightly), a per-install anonymous ID (not tied to your identity), and counts of feature usage (e.g. "a playback switch happened", "a profile was activated"). No device names, no profile content, no files, no personal data beyond what crash reporting already sends.
- How to disable: uncheck "Telemetry" in Settings → General tab. Takes effect immediately. No restart needed.
- Crash reporting: when telemetry is off, session tracking is also disabled. Crash reports are sent only if the app crashes while telemetry is on.
- Quote Sentry's data residency / privacy page.

---

## 9. Open Questions / Decision Points

| # | Question | Recommendation |
|---|----------|----------------|
| OQ1 | Do we send `Environment.UserName` as Sentry username? | It's already sent. Either remove it (requires code change) or document it honestly. **Document it** — removing it changes crash-report context and is a separate decision. |
| OQ2 | Do we hash profile names for `profile_id`? | Yes — use a short hash so we can count activations per profile without sending names. |
| OQ3 | Do we add an offline buffer beyond Sentry's internal queue? | Defer to v2. Built-in queue + `FlushAsync` on shutdown is sufficient. |
| OQ4 | Should the CLI call `TelemetryService` directly or use IPC? | **Use IPC.** The CLI sends a `CliCommandExecuted` message via NamedPipe; the running SoundSwitch instance records it through `TelemetryService` in `SoundSwitchApplicationContext.HandlePipeMessageAsync`. This avoids framework coupling in the CLI and prevents double counting. |
| OQ5 | Do we update the SettingsStrings tooltip? | Non-blocking for v1. The website privacy page is the primary disclosure; the tooltip can stay short or be updated in a follow-up. |

---

## 10. Implementation Order

1. **Design doc** (this file) — done.
2. **Documentation first:** website privacy page, update general.md, update Terms.md.
3. **`TelemetryService`** class — the static gate + Track* methods. No calls placed yet.
4. **`AppModel.Telemetry` setter** — add `TelemetryService.Reload()` call.
5. **Program.cs** — confirm `TelemetryService.Reload()` is called after `SentrySdk.Init()` (or rely on the setter path).
6. **Wire calls** into hotkey, profile, notification, CLI paths — one area at a time, build-verified.
7. **Flush on shutdown** — confirm `SentrySdk.FlushAsync` is called before exit (Program.cs:177 already has `SentrySdk.EndSession()`; add `FlushAsync` if not present).
8. **Build + verify** — `dotnet build SoundSwitch.sln -c Debug`.
9. **PR** — via OpenCode/gh.

---

## 11. Validation

- Build: `dotnet build SoundSwitch.sln -c Debug`
- Verify the telemetry checkbox still toggles and persists across restarts.
- Verify that with `Telemetry = false`, no metric events leave the machine (can be tested by running with a mock/fake Sentry DSN or by checking that `TelemetryService.IsEnabled()` returns false and Track* methods are no-ops).
- Verify website builds: `cd website && npm run docs:build`.

---

## 12. Risks

- **Thread safety:** hotkey hooks run on background threads. `_enabled` must be `volatile` and the gate must not allocate or lock. The current design uses a simple `volatile bool` read — acceptable.
- **Sentry SDK version:** `Sentry.Serilog` brings a specific `Sentry` version. Verify `SentrySdk.Metrics.Counter` is available in that version before relying on it. If Metrics API is not available, fall back to `SentrySdk.CaptureMessage` with tags as a temporary bridge (less ideal — creates issues instead of metrics).
- **Existing telemetry description accuracy:** changing the documented scope may prompt user questions. The website privacy page should be clear and non-apologetic — "we collect usage counts to know what features matter" is a reasonable stance for a free app.

---

*End of design document.*
