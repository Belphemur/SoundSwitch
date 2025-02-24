using SoundSwitch.IPC.Pipe;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SoundSwitch.CLI.Commands;

public class SwitchCommand : AsyncCommand<SwitchCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[type]")]
        [CommandOption("-t|--type")]
        public AudioType Type { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            return await AnsiConsole.Status()
                .StartAsync($"Switching {settings.Type} device...", async _ =>
                {
                    var response = await NamedPipe.SendRequestAsync<TriggerSwitchResponse>(PipeConstants.GetUserPipeName(),
                        new TriggerSwitchRequest { Type = settings.Type });

                    if (response.Success)
                    {
                        AnsiConsole.MarkupLine($"[green]Successfully switched {settings.Type} device[/]");
                        return 0;
                    }

                    AnsiConsole.MarkupLine($"[red]Failed to switch {settings.Type} device[/]");
                    return 1;
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
            return 1;
        }
    }
}