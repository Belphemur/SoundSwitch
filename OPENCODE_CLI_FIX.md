# OpenCode Task: Fix CLI telemetry compilation errors

## Status

TelemetryService.cs is already fixed (gating via `if (!_enabled) return`). ToggleMicrophoneMute return type fixed. But CLI commands still reference `TelemetryService` directly which won't compile because `SoundSwitch.CLI.csproj` doesn't reference `SoundSwitch`.

## What to do

### 1. SoundSwitchApplicationContext.cs — wire IPC handler

The IPC message types already exist:
- `SoundSwitch.IPC/Pipe/Messages/Cli/CliCommandExecuted.cs` — `{ Command: string, ExitCode: int }`
- `SoundSwitch.IPC/Pipe/Messages/Cli/CliCommandExecutedResponse.cs` — `{ Success: bool }`

Add a new case in `HandlePipeMessageAsync` (around line 148, after the `MuteRequest` case):

```csharp
using SoundSwitch.IPC.Pipe.Messages.Cli;

// Add this case:
case CliCommandExecuted cliCmd:
    TelemetryService.TrackCliCommand(cliCmd.Command, cliCmd.ExitCode);
    return new CliCommandExecutedResponse { Success = true };
```

Add `using SoundSwitch.Framework.Telemetry;` if not already present (it should be — the file already uses TelemetryService elsewhere).

### 2. CLI commands — replace direct TelemetryService calls with IPC

The CLI project only references `SoundSwitch.IPC`, NOT `SoundSwitch`. So all `using SoundSwitch.Framework.Telemetry;` imports and `TelemetryService.TrackCliCommand(...)` calls must be removed and replaced with IPC calls.

Files to modify (all in `SoundSwitch.CLI/Commands/`):

**MuteCommand.cs** — command name: `"mute"`
**SwitchCommand.cs** — command name: `"switch"`
**ProfileCommand.cs** — command name: `"profile"`
**SettingsCommand.cs** — command name: `"settings"`
**StatusCommand.cs** — command name: `"status"`
**DevicesCommand.cs** — command name: `"devices"`

For each command:
1. Remove `using SoundSwitch.Framework.Telemetry;`
2. Add `using SoundSwitch.IPC.Pipe.Messages.Cli;`
3. Remove the `TelemetryService.TrackCliCommand("...", exitCode);` line
4. Add this after computing exitCode (before `return exitCode;`):
```csharp
try
{
    await NamedPipe.SendRequestAsync<CliCommandExecutedResponse>(
        PipeConstants.GetUserPipeName(),
        new CliCommandExecuted { Command = "<command_name>", ExitCode = exitCode },
        cancellationToken);
}
catch { /* best-effort, don't fail the command */ }
```

Replace `<command_name>` with the actual command name for each file.

### 3. Verify

After all changes, run:
```
dotnet build SoundSwitch/SoundSwitch.csproj -c Debug -p:LinuxBuild=true -p:BuildProjectReferences=false
```
(With DOTNET_ROOT=/home/balor/.dotnet and PATH including $DOTNET_ROOT)

### 4. Commit

Amend the previous commit:
```
git add -A
git commit --amend --no-edit
git push --force-with-lease origin feat/telemetry-usage-metrics
```

## Important notes

- The IPC messages `CliCommandExecuted` and `CliCommandExecutedResponse` already exist — just wire them
- SoundSwitchApplicationContext.cs already imports `SoundSwitch.Framework.Telemetry` and uses `TelemetryService` (e.g. in the `MuteRequest` case at line ~128)
- CLI commands already import `SoundSwitch.IPC.Pipe` and use `NamedPipe.SendRequestAsync` — the pattern is familiar
- The IPC send is best-effort — catch and ignore exceptions so the CLI command still returns its exit code
