#nullable enable
using SoundSwitch.IPC.Pipe;
using SoundSwitch.IPC.Pipe.Messages.Microphone;
using SoundSwitch.IPC.Pipe.Messages.Mute;

using Spectre.Console;
using Spectre.Console.Cli;

namespace SoundSwitch.CLI.Commands;

public class MuteCommand : AsyncCommand<MuteCommand.Settings>
{
    public class Settings : JsonCommandSettings
    {
        [CommandArgument(0, "[state]")]
        [CommandOption("-s|--state")]
        public bool? State { get; set; }

        [CommandOption("-t|--toggle")] public bool Toggle { get; set; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (settings.Json)
        {
            return await ExecuteJsonAsync(settings, cancellationToken);
        }

        return await ExecuteTableAsync(settings, cancellationToken);
    }

    private static async Task<int> ExecuteJsonAsync(Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var currentState = await NamedPipe.SendRequestAsync<MicrophoneStateResponse>(
                PipeConstants.GetUserPipeName(),
                new MicrophoneStateRequest(),
                cancellationToken);

            if (!currentState.Success)
            {
                JsonOutput.WriteError("Failed to get microphone state");
                return 1;
            }

            // No action requested: just report the current state.
            if (!settings.State.HasValue && !settings.Toggle)
            {
                WriteMicrophoneState(currentState);
                return 0;
            }

            var targetState = settings.Toggle ? !currentState.IsMuted : settings.State!.Value;

            // Already in the requested state: no change needed.
            if (targetState == currentState.IsMuted)
            {
                WriteMicrophoneState(currentState);
                return 0;
            }

            var muteResponse = await NamedPipe.SendRequestAsync<MicrophoneStateResponse>(
                PipeConstants.GetUserPipeName(),
                new MuteRequest { Mute = targetState },
                cancellationToken);

            if (!muteResponse.Success)
            {
                JsonOutput.WriteError("Failed to change microphone state");
                return 1;
            }

            WriteMicrophoneState(muteResponse);
            return 0;
        }
        catch (Exception ex)
        {
            JsonOutput.WriteError(ex.Message);
            return 1;
        }
    }

    private static void WriteMicrophoneState(MicrophoneStateResponse state) =>
        JsonOutput.Write(new { deviceName = state.DeviceName, isMuted = state.IsMuted });

    private static async Task<int> ExecuteTableAsync(Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            return await AnsiConsole.Status()
                .StartAsync("Managing microphone state...", async _ =>
                {
                    // First get current state
                    var currentState = await NamedPipe.SendRequestAsync<MicrophoneStateResponse>(
                        PipeConstants.GetUserPipeName(),
                        new MicrophoneStateRequest());

                    if (!currentState.Success)
                    {
                        AnsiConsole.MarkupLine("[red]Failed to get microphone state[/]");
                        return 1;
                    }

                    // Just display current state if no action requested
                    if (!settings.State.HasValue && !settings.Toggle)
                    {
                        AnsiConsole.MarkupLine(
                            $"[blue]{currentState.DeviceName}[/] is currently {(currentState.IsMuted ? "[red]muted[/]" : "[green]unmuted[/]")}");
                        return 0;
                    }

                    // Determine target state
                    var targetState = settings.Toggle ? !currentState.IsMuted : settings.State!.Value;

                    // Set new state if different from current
                    if (targetState != currentState.IsMuted)
                    {
                        var muteResponse = await NamedPipe.SendRequestAsync<MicrophoneStateResponse>(
                            PipeConstants.GetUserPipeName(),
                            new MuteRequest { Mute = targetState }, cancellationToken);

                        if (!muteResponse.Success)
                        {
                            AnsiConsole.MarkupLine("[red]Failed to change microphone state[/]");
                            return 1;
                        }

                        AnsiConsole.MarkupLine(
                            $"[blue]{muteResponse.DeviceName}[/] is now {(muteResponse.IsMuted ? "[red]muted[/]" : "[green]unmuted[/]")}");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine(
                            $"[blue]{currentState.DeviceName}[/] already {(currentState.IsMuted ? "[red]muted[/]" : "[green]unmuted[/]")}");
                    }

                    return 0;
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 1;
        }
    }
}
