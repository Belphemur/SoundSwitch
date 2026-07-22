#nullable enable
using System.Text.Json;

using SoundSwitch.IPC.Pipe;
using SoundSwitch.IPC.Pipe.Messages.GetActiveDevices;

using Spectre.Console;
using Spectre.Console.Cli;

namespace SoundSwitch.CLI.Commands;

public class StatusCommand : AsyncCommand<StatusCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--json")]
        public bool Json { get; set; }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (settings.Json)
        {
            return await ExecuteJsonAsync(cancellationToken);
        }

        return await ExecuteTableAsync(cancellationToken);
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
                WriteJsonError(response.Error ?? "Failed to retrieve status");
                return 1;
            }

            var json = JsonSerializer.Serialize(new
            {
                activeProfile = response.ActiveProfile,
                playbackDevice = response.PlaybackDevice,
                recordingDevice = response.RecordingDevice,
                playbackCommunicationDevice = response.PlaybackCommunicationDevice,
                recordingCommunicationDevice = response.RecordingCommunicationDevice
            }, new JsonSerializerOptions { WriteIndented = true });

            Console.WriteLine(json);
            return 0;
        }
        catch (Exception ex)
        {
            WriteJsonError(ex.Message);
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

    private static void WriteJsonError(string message)
    {
        var json = JsonSerializer.Serialize(new { error = message }, new JsonSerializerOptions { WriteIndented = true });
        Console.WriteLine(json);
    }
}
