# OpenCode Handoff: SoundSwitch Telemetry Feature Usage

**Branch:** `feat/telemetry-usage-metrics` (already created, checked out)  
**Repo:** `/home/balor/workspace/soundswitch`  
**Design doc:** `TELEMETRY_DESIGN.md` (read this first — it's the source of truth)

---

## What's already done (do NOT redo)

- `TELEMETRY_DESIGN.md` — full design document
- `/home/balor/workspace/soundswitch/website/src/privacy/telemetry.md` — new website page
- `/home/balor/workspace/soundswitch/website/src/configuration/general.md` — Telemetry section updated to link to privacy page
- `/home/balor/workspace/soundswitch/Terms.md` — Telemetry terms section expanded
- `/home/balor/workspace/soundswitch/website/src/.vuepress/config.ts` — navbar entry added for Privacy & Telemetry
- `.NET SDK 10.0.400` installed at `/home/balor/.dotnet` (env: `DOTNET_ROOT=/home/balor/.dotnet`, PATH includes it)
- Packages restored for `SoundSwitch/SoundSwitch.csproj` (use `-p:LinuxBuild=true -p:BuildProjectReferences=false` when building main project on Linux)

---

## Remaining work (this is what OpenCode does)

### 1. Create `TelemetryService.cs`

**File:** `SoundSwitch/Framework/Telemetry/TelemetryService.cs` (new file, new directory)

**Namespace:** `SoundSwitch.Framework.Telemetry`

**Purpose:** Single static entry point for all feature-usage telemetry. Every Track* method checks `IsEnabled()` first (reads `AppConfigs.Configuration.Telemetry`). When disabled, all methods are no-ops.

**Exact code:**

```csharp
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;

using Sentry;

using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Framework.Banner;

namespace SoundSwitch.Framework.Telemetry;

/// <summary>
/// Centralized feature-usage telemetry. All calls go through this static class.
/// When AppConfigs.Configuration.Telemetry is false, every method is a no-op.
/// </summary>
public static class TelemetryService
{
    private static volatile bool _enabled;

    /// <summary>
    /// Call once at startup and whenever the Telemetry setting changes.
    /// </summary>
    public static void Reload()
    {
        _enabled = AppConfigs.Configuration.Telemetry;
    }

    public static bool IsEnabled() => _enabled;

    private static void EnsureEnabled()
    {
        if (!_enabled) return;
    }

    // ── Core switching ──────────────────────────────────────────────

    public static void TrackPlaybackSwitch(string trigger)
    {
        EnsureEnabled();
        SentrySdk.Metrics.EmitCounter("soundswitch.playback.switched", 1,
            new KeyValuePair<string, object>("trigger", trigger));
    }

    public static void TrackRecordingSwitch(string trigger)
    {
        EnsureEnabled();
        SentrySdk.Metrics.EmitCounter("soundswitch.recording.switched", 1,
            new KeyValuePair<string, object>("trigger", trigger));
    }

    public static void TrackMicMute(string trigger, bool muted)
    {
        EnsureEnabled();
        SentrySdk.Metrics.EmitCounter(muted ? "soundswitch.mic.muted" : "soundswitch.mic.unmuted", 1,
            new KeyValuePair<string, object>("trigger", trigger));
    }

    // ── Profiles ────────────────────────────────────────────────────

    /// <summary>
    /// Hash the profile name to an 8-char hex so we can count activations
    /// per profile without sending the actual name.
    /// </summary>
    private static string ProfileHash(string name)
    {
        if (string.IsNullOrEmpty(name)) return "unknown";
        var hash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(name));
        return Convert.ToHexString(hash).Substring(0, 8).ToLowerInvariant();
    }

    public static void TrackProfileActivated(TriggerType triggerType, string profileName)
    {
        EnsureEnabled();
        SentrySdk.Metrics.EmitCounter("soundswitch.profile.activated", 1,
            new KeyValuePair<string, object>("trigger_type", triggerType.ToString()),
            new KeyValuePair<string, object>("profile_id", ProfileHash(profileName)));
    }

    public static void TrackProfileCreated()
    {
        EnsureEnabled();
        SentrySdk.Metrics.EmitCounter("soundswitch.profile.created", 1);
    }

    public static void TrackProfileDeleted()
    {
        EnsureEnabled();
        SentrySdk.Metrics.EmitCounter("soundswitch.profile.deleted", 1);
    }

    public static void TrackProfileActivationFailed(string reason)
    {
        EnsureEnabled();
        SentrySdk.Metrics.EmitCounter("soundswitch.profile.activation_failed", 1,
            new KeyValuePair<string, object>("reason", reason));
    }

    // ── Notifications ───────────────────────────────────────────────

    public static void TrackNotificationBanner(string action)
    {
        EnsureEnabled();
        SentrySdk.Metrics.EmitCounter("soundswitch.notification.banner", 1,
            new KeyValuePair<string, object>("action", action));
    }

    public static void TrackNotificationWindows()
    {
        EnsureEnabled();
        SentrySdk.Metrics.EmitCounter("soundswitch.notification.windows_shown", 1);
    }

    public static void TrackNotificationSound()
    {
        EnsureEnabled();
        SentrySdk.Metrics.EmitCounter("soundswitch.notification.sound_played", 1);
    }

    // ── CLI ──────────────────────────────────────────────────────────

    public static void TrackCliCommand(string command, int exitCode)
    {
        EnsureEnabled();
        SentrySdk.Metrics.EmitCounter("soundswitch.cli.command", 1,
            new KeyValuePair<string, object>("command", command),
            new KeyValuePair<string, object>("exit_code", exitCode.ToString()));
    }

    // ── System ──────────────────────────────────────────────────────

    public static void TrackDevicesEnumerated(string deviceType, int count)
    {
        EnsureEnabled();
        SentrySdk.Metrics.EmitDistribution("soundswitch.devices.count", count, MeasurementUnit.None,
            new KeyValuePair<string, object>("device_type", deviceType));
    }

    // ── Breadcrumbs ─────────────────────────────────────────────────

    public static void AddBreadcrumb(string category, string message)
    {
        EnsureEnabled();
        SentrySdk.AddBreadcrumb(new Sentry.Breadcrumb
        {
            Category = category,
            Message = message,
            Level = BreadcrumbLevel.Info
        });
    }
}
```

**Notes:**
- `ProfileHash` uses SHA256 → first 8 hex chars. This is deterministic so we can count activations per profile across sessions without knowing the name.
- `triggerType.ToString()` — the TriggerType enum values are string-backed (e.g. "Hotkey", "Application", "Window", "Steam", "Uwp", "Startup"). Check the enum definition at `SoundSwitch.Framework.Profile.Trigger.TriggerType` to confirm. If it's not a string enum, use a switch expression to map to clean string values.

### 2. Wire `TelemetryService.Reload()` into `AppModel.Telemetry` setter

**File:** `SoundSwitch/Model/AppModel.AppSettings.cs`

The `Telemetry` property setter already calls `AppConfigs.Configuration.Save()`. Add `TelemetryService.Reload()` right after the save:

```csharp
public bool Telemetry
{
    get => AppConfigs.Configuration.Telemetry;
    set
    {
        AppConfigs.Configuration.Telemetry = value;
        AppConfigs.Configuration.Save();
        TelemetryService.Reload();   // ← ADD THIS LINE
    }
}
```

This makes the toggle effective immediately without needing a separate wire from the UI.

### 3. Confirm `TelemetryService.Reload()` is called at startup

**File:** `SoundSwitch/Program.cs`

Line 62-70 already reads `AppConfigs.Configuration.Telemetry` for `AutoSessionTracking`. After `SentrySdk.Init()` (line 77), add:

```csharp
TelemetryService.Reload();
```

Place it right after `SentrySdk.ConfigureScope(...)` (line 80) or after `InitializeLogger()` (line 81). It just needs to be after `SentrySdk.Init()`.

### 4. Add `FlushAsync` on shutdown

**File:** `SoundSwitch/Program.cs`

Line 177: `SentrySdk.EndSession();` — add before it (or after, both fine):

```csharp
await SentrySdk.FlushAsync(TimeSpan.FromSeconds(2));
SentrySdk.EndSession();
```

`OldMain` is already `async Task`, so this is fine.

### 5. Wire Track* calls into feature paths

The design doc §6 lists the hook locations. Here are the exact placements:

#### 5a. Hotkey handler — playback/recording/mic switches

**File:** Find the global hotkey handler. From the repo structure it's likely in `SoundSwitch/Framework/WinApi/Keyboard/` or wherever `PlaybackHotKey`, `RecordingHotKey`, `MuteRecordingHotKey` are consumed. Search for where the hotkey actions are dispatched (look for `SetHotkeyCombination` usage or the action callback).

Add at the point where a switch actually happens:

```csharp
TelemetryService.TrackPlaybackSwitch("hotkey");
// or
TelemetryService.TrackRecordingSwitch("hotkey");
// or
TelemetryService.TrackMicMute("hotkey", muted: true/false);
```

Also add breadcrumbs for each hotkey press (too granular for metrics but useful context):

```csharp
TelemetryService.AddBreadcrumb("hotkey", "PlaybackHotKey pressed");
```

If the same code path handles tray double-click switches, call `TrackPlaybackSwitch("tray")` instead.

#### 5b. Profile activation

**File:** `SoundSwitch/Framework/Profile/ProfileManager.cs`

Find where a profile is actually activated (device switch applied). There should be a method that applies the profile's devices. Add:

```csharp
TelemetryService.TrackProfileActivated(trigger.Type, profile.Name);
```

For failures (device not available, error), add:

```csharp
TelemetryService.TrackProfileActivationFailed(reason: "device_unavailable"); // or "error", etc.
```

#### 5c. Profile CRUD

**File:** Where profiles are added/deleted (likely in `ProfileManager` or in the UI form `UpsertProfileExtended.cs`).

Add on successful add:

```csharp
TelemetryService.TrackProfileCreated();
```

Add on successful delete:

```csharp
TelemetryService.TrackProfileDeleted();
```

#### 5d. Banner notifications

**File:** `SoundSwitch/Framework/Banner/BannerManager.cs` — `ShowNotification(BannerData data)`

Add at the start of `ShowNotification`:

```csharp
TelemetryService.TrackNotificationBanner("shown");
TelemetryService.AddBreadcrumb("notification", $"Banner shown: {data.BannerType}");
```

For the unmute click on the banner — find where the banner form handles the unmute click and add:

```csharp
TelemetryService.TrackNotificationBanner("unmute_clicked");
```

#### 5e. Windows notification / Sound notification

Find where `NotificationManager` plays the Windows balloon or sound. Add:

```csharp
TelemetryService.TrackNotificationWindows();
// or
TelemetryService.TrackNotificationSound();
```

#### 5f. CLI commands

**File:** `SoundSwitch.CLI/Commands/` — each command's `Execute` method.

The CLI project (`SoundSwitch.CLI.csproj`) does NOT reference the main `SoundSwitch` project — it only references `SoundSwitch.IPC`. **This is a problem.** The CLI cannot directly call `TelemetryService` from `SoundSwitch.Framework.Telemetry` because it doesn't have a project reference to `SoundSwitch`.

**Decision needed:** Either:
- (a) Add a project reference from `SoundSwitch.CLI` to `SoundSwitch` (or to `SoundSwitch.Common` if `TelemetryService` can be moved there), OR
- (b) Have the CLI report via IPC to the running SoundSwitch instance, which then records the telemetry.

**Recommendation:** Option (a) is simpler. `TelemetryService` only depends on `Sentry` (already transitively available) and `SoundSwitch.Framework.Configuration` (for `AppConfigs`). If we move `TelemetryService` to `SoundSwitch.Common`, it needs `SoundSwitch.Framework.Configuration` — check whether `SoundSwitch.Common` already references `SoundSwitch.Framework` or if that creates a circular dependency.

**Simplest correct approach:** Add `<ProjectReference Include="..\SoundSwitch\SoundSwitch.csproj" />` to `SoundSwitch.CLI.csproj`. Then in each CLI command's `Execute`, call:

```csharp
TelemetryService.TrackCliCommand(this.GetType().Name, exitCode);
```

Or better — use the command name from the Spectre.Console setup. In `Program.cs`, commands are registered with names like `"switch"`, `"profile"`, `"settings"`, `"mute"`, `"status"`, `"devices"`. Pass the command name explicitly.

Coroutine each command class to accept a `string commandName` parameter and call `TrackCliCommand(commandName, result)`.

### 6. Update `AppConfigs` or `SoundSwitchConfiguration` if needed

No changes needed — `Telemetry` property already exists and is persisted.

### 7. Update `Directory.Packages.props` if Sentry version needs bumping

Current: `Sentry.Serilog` version `6.9.0`. No bump needed — Metrics API is GA in 6.1.0+.

---

## Build and test

```bash
# Restore (main project only, Linux)
dotnet restore SoundSwitch/SoundSwitch.csproj -p:LinuxBuild=true -p:BuildProjectReferences=false

# Build (main project only, Linux)
dotnet build SoundSwitch/SoundSwitch.csproj -c Debug -p:LinuxBuild=true -p:BuildProjectReferences=false
```

Full solution build requires Windows (CsWinRT projection). For Linux validation, build the main project with `LinuxBuild=true`.

The website build (separate step):

```bash
cd website && npm run docs:build
```

This requires Node.js/npm. If not available, skip — the markdown changes are valid.

---

## Commit and PR

Commit message (conventional commits, scope `telemetry`):

```
feat(telemetry): add feature-usage metrics via Sentry with user opt-out gate

- Add TelemetryService static class with Track* methods gated by AppModel.Telemetry
- Wire profile activation, hotkey switches, banner notifications, CLI commands
- Add FlushAsync on shutdown
- Add website privacy/telemetry.md page documenting what is sent and how to disable
- Update Terms.md telemetry section to reflect expanded scope
- Update general.md to link to privacy page
- Add Privacy & Telemetry entry to website navbar
```

Then:

```bash
git add TELEMETRY_DESIGN.md SoundSwitch/Framework/Telemetry/TelemetryService.cs SoundSwitch/Model/AppModel.AppSettings.cs SoundSwitch/Program.cs SoundSwitch/Framework/Profile/ProfileManager.cs SoundSwitch/Framework/Banner/BannerManager.cs SoundSwitch.CLI/SoundSwitch.CLI.csproj SoundSwitch.CLI/Commands/*.cs website/src/privacy/telemetry.md website/src/configuration/general.md Terms.md website/src/.vuepress/config.ts
git commit -m "feat(telemetry): ..."
git push origin feat/telemetry-usage-metrics
```

Then create PR via `gh pr create`:

```bash
gh pr create --title "feat: feature-usage telemetry with opt-out" --body "..."
```

---

## Summary checklist for OpenCode

- [ ] Create `SoundSwitch/Framework/Telemetry/TelemetryService.cs` with the exact code above
- [ ] Add `TelemetryService.Reload()` to `AppModel.Telemetry` setter in `AppModel.AppSettings.cs`
- [ ] Add `TelemetryService.Reload()` after `SentrySdk.Init()` in `Program.cs`
- [ ] Add `await SentrySdk.FlushAsync(...)` before `SentrySdk.EndSession()` in `Program.cs`
- [ ] Wire Track* calls into hotkey handler, profile activation, profile CRUD, banner, notifications, CLI
- [ ] For CLI: add project reference to `SoundSwitch` (or move TelemetryService to Common — verify no circular dep)
- [ ] Build: `dotnet build SoundSwitch/SoundSwitch.csproj -c Debug -p:LinuxBuild=true -p:BuildProjectReferences=false`
- [ ] Commit with conventional commit message
- [ ] Push and create PR
