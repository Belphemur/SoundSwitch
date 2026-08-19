#nullable enable
using SoundSwitch.IPC.Pipe;
using SoundSwitch.IPC.Pipe.Messages.Cli;
using SoundSwitch.IPC.Pipe.Messages.OpenSettings;

using Spectre.Console;
using Spectre.Console.Cli;

namespace SoundSwitch.CLI.Commands;

public class SettingsCommand : AsyncCommand<SettingsCommand.Settings>
{
    public class Settings : JsonCommandSettings
    {
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var exitCode = settings.Json
            ? await ExecuteJsonAsync(cancellationToken)
            : await ExecuteTableAsync(cancellationToken);
        try
        {
            await NamedPipe.SendRequestAsync<CliCommandExecutedResponse>(
                PipeConstants.GetUserPipeName(),
                new CliCommandExecuted { Command = "settings" },
                cancellationToken);
        }
        catch { /* best-effort */ }
        return exitCode;
    }

    private static async Task<int> ExecuteJsonAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await NamedPipe.SendRequestAsync<OpenSettingsResponse>(
                PipeConstants.GetUserPipeName(),
                new OpenSettingsRequest(),
                cancellationToken);

            if (response.Success)
            {
                JsonOutput.Write(new { success = true });
                return 0;
            }

            JsonOutput.WriteError("Failed to open settings");
            return 1;
        }
        catch (Exception ex)
        {
            JsonOutput.WriteError(ex.Message);
            return 1;
        }
    }

    private static async Task<int> ExecuteTableAsync(CancellationToken cancellationToken)
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
