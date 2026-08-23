# Spec: Update-subsystem telemetry

**Goal:** know which `UpdateMode` users run, and the update funnel
(checked → available → installed). All calls go through `TelemetryService`
(existing static class, gated on `AppConfigs.Configuration.Telemetry`).

## Metric contract (code is source of truth)

Counters (add to `SoundSwitch/Framework/Telemetry/TelemetryService.cs`):

| Metric | Attributes | Meaning |
|--------|-----------|---------|
| `soundswitch.update.mode` | `value` = Silent \| Notify \| Never | Emitted on every change of the setting, AND once at startup (baseline) |
| `soundswitch.update.check` | `trigger` = manual | User clicked "Check for update" in the tray menu |
| `soundswitch.update.available` | `mode` = Silent \| Notify \| Never | A newer release was found and offered (NewVersionReleased fired) |
| `soundswitch.update.installed` | `mode` = Silent \| Notify, `result` = success \| signature_error \| failed | An install was attempted/applied |

Mirror the existing method style exactly:
```csharp
public static void TrackUpdateMode(UpdateMode mode)
{
    if (!AppConfigs.Configuration.Telemetry) return;
    SentrySdk.Metrics.EmitCounter("soundswitch.update.mode", 1,
        Attributes(("value", mode.ToString())), null);
}
public static void TrackUpdateCheck(string trigger)
{
    if (!AppConfigs.Configuration.Telemetry) return;
    SentrySdk.Metrics.EmitCounter("soundswitch.update.check", 1,
        Attributes(("trigger", trigger)), null);
}
public static void TrackUpdateAvailable(UpdateMode mode)
{
    if (!AppConfigs.Configuration.Telemetry) return;
    SentrySdk.Metrics.EmitCounter("soundswitch.update.available", 1,
        Attributes(("mode", mode.ToString())), null);
}
public static void TrackUpdateInstalled(UpdateMode mode, string result)
{
    if (!AppConfigs.Configuration.Telemetry) return;
    SentrySdk.Metrics.EmitCounter("soundswitch.update.installed", 1,
        Attributes(("mode", mode.ToString()), ("result", result)), null);
}
```
`TelemetryService.cs` needs `using SoundSwitch.Framework.Updater;` for `UpdateMode`.

## Wiring (exact file:line anchors in this worktree)

1. `SoundSwitch/Model/AppModel.AppSettings.cs`
   - `UpdateMode` setter (~line 105): inside `if (value != AppConfigs.Configuration.UpdateMode)`, after computing `value`, call `TelemetryService.TrackUpdateMode(value);` before `UpdateModeChanged?.Invoke`. (`using SoundSwitch.Framework.Telemetry;` already present at line 22.)
   - `CheckForUpdate()` (~line 185): first line `TelemetryService.TrackUpdateCheck("manual");`

2. `SoundSwitch/Model/SoundSwitchApplicationContext.cs`
   - In constructor, after `AppModel.Instance.InitializeMain(deviceActiveLister, Program.SkipUpdate);` (line 51), add baseline: `TelemetryService.TrackUpdateMode(SoundSwitch.Framework.Configuration.AppConfigs.Configuration.UpdateMode);` (add `using SoundSwitch.Framework.Telemetry;`).
   - In `NewVersionReleased` handler `case UpdateMode.Silent:` (line 55), before `new AutoUpdater("/VERYSILENT").Update(...)` add `TelemetryService.TrackUpdateAvailable(UpdateMode.Silent);` and after the `Update(...)` call add `TelemetryService.TrackUpdateInstalled(UpdateMode.Silent, "success");`

3. `SoundSwitch/UI/Component/TrayIcon.cs`
   - In `SetEventHandlers`, `NewVersionReleased` handler: in `case UpdateMode.Notify:` add `TelemetryService.TrackUpdateAvailable(UpdateMode.Notify);` before `_context.Send(_ => { NewReleaseAvailable(...); }, null);`; in `case UpdateMode.Never:` add `TelemetryService.TrackUpdateAvailable(UpdateMode.Never);` before `DownloadRelease`. (add `using SoundSwitch.Framework.Telemetry;`)

4. `SoundSwitch/UI/Forms/UpdateDownloadForm.cs`
   - `_releaseFile.Downloaded` (line 87): before `new UpdateRunner().RunUpdate(_releaseFile, "/SILENT");` add `TelemetryService.TrackUpdateInstalled(SoundSwitch.Framework.Configuration.AppConfigs.Configuration.UpdateMode, "success");`. In the signature-failure branch (right after `SignatureChecker.IsValid` returns non-null, before `return;`) add `TrackUpdateInstalled(..., "signature_error");`.
   - `_releaseFile.DownloadFailed` (line 80): add `TelemetryService.TrackUpdateInstalled(SoundSwitch.Framework.Configuration.AppConfigs.Configuration.UpdateMode, "failed");`. (add `using SoundSwitch.Framework.Telemetry;`)

## Docs / disclosure (must be updated together — telemetry-sync skill)

5. `docs/TELEMETRY_DESIGN.md`: add §5.8 "Update subsystem" mapping the 4 counters + attributes; add them to the canonical inventory.
6. `website/src/legal/telemetry.md`: add the 4 update counters to the "what is sent" list (use "pseudonymous" wording, no identifiers beyond existing).
7. `Terms.md` AND `Terms.txt` (RST!) AND `website/src/legal/terms.md`: add the update counters under the telemetry-terms "data gathered" bullets; bump "Last updated" on `Terms.md`+`Terms.txt`.

Do NOT add UI. Do NOT send any new identifier. `UpdateMode` is an enum value (categorical), not PII.

## Build / verify
- Partial Linux build (Audio.Manager CsWinRT fails pre-existing; ignore CS0006 there):
  `dotnet build SoundSwitch/SoundSwitch.csproj -c Debug -p:LinuxBuild=true -p:BuildProjectReferences=false`
  This compiles the touched project against committed refs and catches syntax errors.
- Windows CI is the real gate.
- After editing, run `git status` and `git diff --stat` to confirm changes landed (this is mandatory — do not report done without a non-empty diff).
