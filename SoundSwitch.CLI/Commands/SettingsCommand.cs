using SoundSwitch.IPC.Pipe.Messages.OpenSettings;
using SoundSwitch.IPC.Pipe;
using Spectre.Console.Cli;
using Spectre.Console;

namespace SoundSwitch.CLI.Commands;

public class SettingsCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken)
    {
        try
        {
            return await AnsiConsole.Status()
                .StartAsync("Opening settings...", async _ =>
                {
                    var response = await NamedPipe.SendRequestAsync<OpenSettingsResponse>(
                        PipeConstants.GetUserPipeName(),
                        new OpenSettingsRequest(), cancellationToken);

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
