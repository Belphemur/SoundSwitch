# OpenCode Task: Fix SoundSwitch telemetry PR build failures

Read and follow these instructions exactly. Work in the `feat/telemetry-usage-metrics` branch at `/home/balor/workspace/soundswitch`.

## Context

CI is failing on PR #2346. There are three categories of bugs to fix:
1. TelemetryService.EnsureEnabled() doesn't actually stop execution
2. ToggleMicrophoneMute() returns void but is used as if it returns a value
3. CLI commands directly use TelemetryService but don't have a project reference

## Fix 1: TelemetryService gating (SoundSwitch/Framework/Telemetry/TelemetryService.cs)

The `EnsureEnabled()` method is:
```csharp
private static void EnsureEnabled()
{
    if (!_enabled) return;
}
```
This returns void and does NOT prevent the caller from continuing. Every Track* and AddBreadcrumb method calls EnsureEnabled() then continues to emit regardless.

**Fix:** Change every public method to check `_enabled` directly and return early. Replace the `EnsureEnabled()` pattern with explicit early returns. For example:
```csharp
public static void TrackPlaybackSwitch(string trigger)
{
    if (!_enabled) return;
    SentrySdk.Metrics.EmitCounter("soundswitch.playback.switched", 1,
        Tags(("trigger", trigger)), null);
}
```
Do this for ALL public methods: TrackPlaybackSwitch, TrackRecordingSwitch, TrackMicMute, TrackProfileActivated, TrackProfileCreated, TrackProfileDeleted, TrackProfileActivationFailed, TrackNotificationBanner, TrackNotificationWindows, TrackNotificationSound, TrackCliCommand, TrackDevicesEnumerated, AddBreadcrumb.

Also remove the now-unused `EnsureEnabled()` method and the `using System.Linq` if it's only used by Tags().

## Fix 2: ToggleMicrophoneMute() return type (SoundSwitch/Model/AppModel.DeviceService.cs)

The method currently is:
```csharp
public void ToggleMicrophoneMute()
{
    var result = _microphoneMuteToggler.ToggleDefaultMute();
    if (result == null)
    {
        ErrorTriggered?.Invoke(this, new ExceptionEvent(new Exception("No mic found or unable to toggle mute state")));
        Log.Error("No mic found or unable to toggle mute state");
    }
    else
    {
        Log.Information("Microphone {DeviceName} mute state is now {IsMuted}", result.Value.Name, result.Value.MuteState);
    }
}
```

**Fix:** Change the signature to return the result:
```csharp
public (string DeviceName, bool IsMuted)? ToggleMicrophoneMute()
{
    var result = _microphoneMuteToggler.ToggleDefaultMute();
    if (result == null)
    {
        ErrorTriggered?.Invoke(this, new ExceptionEvent(new Exception("No mic found or unable to toggle mute state")));
        Log.Error("No mic found or unable to toggle mute state");
    }
    else
    {
        Log.Information("Microphone {DeviceName} mute state is now {IsMuted}", result.Value.Name, result.Value.MuteState);
    }
    return result;
}
```

Also update `IDeviceService.cs` line 60 from `void ToggleMicrophoneMute();` to `(string DeviceName, bool IsMuted)? ToggleMicrophoneMute();`.

Do NOT modify `AppModel.AppSettings.cs` — it already handles the nullable return correctly (checks `if (micResult != null)` before accessing `.Value`).

## Fix 3: CLI telemetry via IPC (SoundSwitch.CLI)

The CLI project (`SoundSwitch.CLI.csproj`) only references `SoundSwitch.IPC` — not `SoundSwitch`. All CLI commands directly call `TelemetryService.TrackCliCommand(...)` which won't compile.

**The design already has the IPC infrastructure ready:**
- `SoundSwitch.IPC/Pipe/Messages/Cli/CliCommandExecuted.cs` — request message with `Command` and `ExitCode`
- `SoundSwitch.IPC/Pipe/Messages/Cli/CliCommandExecutedResponse.cs` — response with `Success`

**What needs to happen in SoundSwitchApplicationContext.cs:**
It already has `HandlePipeMessageAsync`. Add a new case for `CliCommandExecuted` that calls `TelemetryService.TrackCliCommand(message.Command, message.ExitCode)` and returns a `CliCommandExecutedResponse { Success = true }`.

**What needs to happen in each CLI command:**
Remove the direct `TelemetryService.TrackCliCommand(...)` calls and replace them with an IPC call via `NamedPipe.SendRequestAsync<CliCommandExecutedResponse>(...)` with a `CliCommandExecuted { Command = "...", ExitCode = exitCode }` message.

The CLI commands to modify:
- `SoundSwitch.CLI/Commands/MuteCommand.cs` — command name: "mute"
- `SoundSwitch.CLI/Commands/SwitchCommand.cs` — command name: "switch"  
- `SoundSwitch.CLI/Commands/ProfileCommand.cs` — command name: "profile"
- `SoundSwitch.CLI/Commands/SettingsCommand.cs` — command name: "settings"
- `SoundSwitch.CLI/Commands/StatusCommand.cs` — command name: "status"
- `SoundSwitch.CLI/Commands/DevicesCommand.cs` — command name: "devices"

For each, remove the `using SoundSwitch.Framework.Telemetry;` import and the `TelemetryService.TrackCliCommand(...)` call. Add a `using SoundSwitch.IPC.Pipe.Messages.Cli;` import and replace with IPC.

Example pattern for a command:
```csharp
// After computing exitCode:
try
{
    await NamedPipe.SendRequestAsync<CliCommandExecutedResponse>(
        PipeConstants.GetUserPipeName(),
        new CliCommandExecuted { Command = "mute", ExitCode = exitCode },
        cancellationToken);
}
catch { /* best-effort, don't fail the command */ }
```

Note: The IPC send is best-effort — if the app isn't running, the pipe send will fail, but that shouldn't break the CLI command's exit code.

The `SoundSwitch.CLI.csproj` should NOT add a project reference to SoundSwitch (TFM mismatch). The IPC approach is correct.

## After fixes

1. Verify build compiles: `dotnet build SoundSwitch/SoundSwitch.csproj -c Debug -p:LinuxBuild=true -p:BuildProjectReferences=false` (with DOTNET_ROOT=/home/balor/.dotnet)
2. Stage all changed files
3. Commit with: `git commit --amend --no-edit` to amend the previous commit
4. Force push: `git push --force-with-lease origin feat/telemetry-usage-metrics`

## Important notes

- Do NOT change AppModel.AppSettings.cs — it already correctly handles the nullable return
- Do NOT add a project reference from CLI to SoundSwitch — use IPC
- The IPC message types (CliCommandExecuted.cs, CliCommandExecutedResponse.cs) already exist — just wire them in
- Make sure SoundSwitchApplicationContext.cs imports the Cli namespace: `using SoundSwitch.IPC.Pipe.Messages.Cli;`
