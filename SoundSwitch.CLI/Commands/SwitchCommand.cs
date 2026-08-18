#nullable enable
using SoundSwitch.IPC.Pipe;
using SoundSwitch.IPC.Pipe.Messages.Cli;
using SoundSwitch.IPC.Pipe.Messages.Models;
using SoundSwitch.IPC.Pipe.Messages.TriggerSwitch;

using Spectre.Console;
using Spectre.Console.Cli;

namespace SoundSwitch.CLI.Commands;

public class SwitchCommand : AsyncCommand<SwitchCommand.Settings>
{
    public class Settings : JsonCommandSettings
    {
        [CommandArgument(0, "[type]")]
        [CommandOption("-t|--type")]
        public AudioType Type { get; set; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var exitCode = settings.Json
            ? await ExecuteJsonAsync(settings, cancellationToken)
            : await ExecuteTableAsync(settings, cancellationToken);
        try
        {
            await NamedPipe.SendRequestAsync<CliCommandExecutedResponse>(
                PipeConstants.GetUserPipeName(),
                new CliCommandExecuted { Command = "switch", ExitCode = exitCode },
                cancellationToken);
        }
        catch { /* best-effort */ }
        return exitCode;
    }

    private static async Task<int> ExecuteJsonAsync(Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var response = await NamedPipe.SendRequestAsync<TriggerSwitchResponse>(
                PipeConstants.GetUserPipeName(),
                new TriggerSwitchRequest { Type = settings.Type },
                cancellationToken);

            if (response.Success)
            {
                JsonOutput.Write(new { success = true, type = settings.Type.ToString() });
                return 0;
            }

            JsonOutput.WriteError($"Failed to switch {settings.Type} device");
            return 1;
        }
        catch (Exception ex)
        {
            JsonOutput.WriteError(ex.Message);
            return 1;
        }
    }

    private static async Task<int> ExecuteTableAsync(Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            return await AnsiConsole.Status()
                .StartAsync($"Switching {settings.Type} device...", async _ =>
                {
                    var response = await NamedPipe.SendRequestAsync<TriggerSwitchResponse>(PipeConstants.GetUserPipeName(),
                        new TriggerSwitchRequest { Type = settings.Type }, cancellationToken);

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
