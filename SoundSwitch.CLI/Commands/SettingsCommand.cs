using SoundSwitch.IPC.Pipe;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SoundSwitch.CLI.Commands;

public class SettingsCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        try
        {
            return await AnsiConsole.Status()
                .StartAsync("Opening settings...", async _ =>
                {
                    var response = await NamedPipe.SendRequestAsync<OpenSettingsResponse>(PipeConstants.GetUserPipeName(),
                        new OpenSettingsRequest());

                    if (response.Success)
                    {
                        AnsiConsole.MarkupLine("[green]Successfully opened settings[/]");
                        return 0;
                    }

                    AnsiConsole.MarkupLine("[red]Failed to open settings[/]");
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