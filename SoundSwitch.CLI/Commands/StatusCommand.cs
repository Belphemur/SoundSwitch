#nullable enable
using SoundSwitch.IPC.Pipe;
using SoundSwitch.IPC.Pipe.Messages.Cli;
using SoundSwitch.IPC.Pipe.Messages.GetActiveDevices;

using Spectre.Console;
using Spectre.Console.Cli;

namespace SoundSwitch.CLI.Commands;

public class StatusCommand : AsyncCommand<StatusCommand.Settings>
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
                new CliCommandExecuted { Command = "status", ExitCode = exitCode },
                cancellationToken);
        }
        catch { /* best-effort */ }
        return exitCode;
    }

    private static async Task<int> ExecuteJsonAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await NamedPipe.SendRequestAsync<GetActiveDevicesResponse>(
                PipeConstants.GetUserPipeName(),
                new GetActiveDevicesRequest(),
                cancellationToken);

            if (!response.Success)
            {
                JsonOutput.WriteError(response.Error ?? "Failed to retrieve status");
                return 1;
            }

            JsonOutput.Write(new
            {
                activeProfile = response.ActiveProfile,
                playbackDevice = response.PlaybackDevice,
                recordingDevice = response.RecordingDevice,
                playbackCommunicationDevice = response.PlaybackCommunicationDevice,
                recordingCommunicationDevice = response.RecordingCommunicationDevice
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
                .StartAsync("Fetching status...", async _ =>
                {
                    var response = await NamedPipe.SendRequestAsync<GetActiveDevicesResponse>(
                        PipeConstants.GetUserPipeName(),
                        new GetActiveDevicesRequest(),
                        cancellationToken);

                    if (!response.Success)
                    {
                        AnsiConsole.MarkupLine($"[red]Error:[/] {Markup.Escape(response.Error ?? "Failed to retrieve status")}");
                        return 1;
                    }

                    var table = new Table()
                        .AddColumn("Category")
                        .AddColumn("Device / Profile")
                        .Border(TableBorder.Rounded);

                    table.AddRow("Active Profile", response.ActiveProfile is null ? "[grey]None[/]" : Markup.Escape(response.ActiveProfile));
                    table.AddRow("[green]Playback[/]", $"[green]{Markup.Escape(response.PlaybackDevice)}[/]");
                    table.AddRow("[red]Recording[/]", $"[red]{Markup.Escape(response.RecordingDevice)}[/]");
                    table.AddRow("[green]Playback Comm[/]", $"[green]{Markup.Escape(response.PlaybackCommunicationDevice)}[/]");
                    table.AddRow("[red]Recording Comm[/]", $"[red]{Markup.Escape(response.RecordingCommunicationDevice)}[/]");

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
