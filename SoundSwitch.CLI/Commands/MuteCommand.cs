using SoundSwitch.IPC.Pipe;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SoundSwitch.CLI.Commands;

public class MuteCommand : AsyncCommand<MuteCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[state]")]
        [CommandOption("-s|--state")]
        public bool? State { get; set; }

        [CommandOption("-t|--toggle")]
        public bool Toggle { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
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
                        AnsiConsole.MarkupLine($"[blue]{currentState.DeviceName}[/] is currently {(currentState.IsMuted ? "[red]muted[/]" : "[green]unmuted[/]")}");
                        return 0;
                    }

                    // Determine target state
                    var targetState = settings.Toggle ? !currentState.IsMuted : settings.State!.Value;

                    // Set new state if different from current
                    if (targetState != currentState.IsMuted)
                    {
                        var muteResponse = await NamedPipe.SendRequestAsync<MicrophoneStateResponse>(
                            PipeConstants.GetUserPipeName(),
                            new MuteRequest { Mute = targetState });

                        if (!muteResponse.Success)
                        {
                            AnsiConsole.MarkupLine("[red]Failed to change microphone state[/]");
                            return 1;
                        }

                        AnsiConsole.MarkupLine($"[blue]{muteResponse.DeviceName}[/] is now {(muteResponse.IsMuted ? "[red]muted[/]" : "[green]unmuted[/]")}");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[blue]{currentState.DeviceName}[/] already {(currentState.IsMuted ? "[red]muted[/]" : "[green]unmuted[/]")}");
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