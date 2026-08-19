#nullable enable
using SoundSwitch.IPC.Pipe;
using SoundSwitch.IPC.Pipe.Messages.Cli;
using SoundSwitch.IPC.Pipe.Messages.GetSwitchableDevices;
using SoundSwitch.IPC.Pipe.Messages.Models;

using Spectre.Console;
using Spectre.Console.Cli;

namespace SoundSwitch.CLI.Commands;

public class DevicesCommand : AsyncCommand<DevicesCommand.Settings>
{
    public class Settings : JsonCommandSettings
    {
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var exit = settings.Json
            ? await ExecuteJsonAsync(cancellationToken)
            : await ExecuteTableAsync(cancellationToken);
        try
        {
            await NamedPipe.SendRequestAsync<CliCommandExecutedResponse>(
                PipeConstants.GetUserPipeName(),
                new CliCommandExecuted { Command = "devices" },
                cancellationToken);
        }
        catch { /* best-effort */ }
        return exit;
    }

    private static async Task<int> ExecuteJsonAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await NamedPipe.SendRequestAsync<GetSwitchableDevicesResponse>(
                PipeConstants.GetUserPipeName(),
                new GetSwitchableDevicesRequest(),
                cancellationToken);

            if (!response.Success)
            {
                JsonOutput.WriteError(response.Error ?? "Failed to retrieve devices");
                return 1;
            }

            JsonOutput.Write(new
            {
                playbackDevices = response.PlaybackDevices,
                recordingDevices = response.RecordingDevices
            });
            return 0;
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
                .StartAsync("Fetching devices...", async _ =>
                {
                    var response = await NamedPipe.SendRequestAsync<GetSwitchableDevicesResponse>(
                        PipeConstants.GetUserPipeName(),
                        new GetSwitchableDevicesRequest(),
                        cancellationToken);

                    if (!response.Success)
                    {
                        AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(response.Error ?? "Failed to retrieve devices")}");
                        return 1;
                    }

                    var table = new Table()
                        .AddColumn("Type")
                        .AddColumn("Device")
                        .Border(TableBorder.Rounded);

                    foreach (var device in response.PlaybackDevices)
                    {
                        table.AddRow("[green]Playback[/]", $"[green]{Markup.Escape(device)}[/]");
                    }

                    foreach (var device in response.RecordingDevices)
                    {
                        table.AddRow("[red]Recording[/]", $"[red]{Markup.Escape(device)}[/]");
                    }

                    AnsiConsole.Write(table);
                    return 0;
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(ex.Message)}");
            return 1;
        }
    }
}
