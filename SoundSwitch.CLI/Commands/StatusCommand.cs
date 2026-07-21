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
        try
        {
            return await AnsiConsole.Status()
                .StartAsync("Fetching status...", async _ =>
                {
                    var response = await NamedPipe.SendRequestAsync<GetActiveDevicesResponse>(
                        PipeConstants.GetUserPipeName(),
                        new GetActiveDevicesRequest(),
                        cancellationToken);

                    if (settings.Json)
                    {
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

                    var table = new Table()
                        .AddColumn("Category")
                        .AddColumn("Device / Profile")
                        .Border(TableBorder.Rounded);

                    table.AddRow("Active Profile", response.ActiveProfile ?? "[grey]None[/]");
                    table.AddRow("[green]Playback[/]", $"[green]{response.PlaybackDevice}[/]");
                    table.AddRow("[red]Recording[/]", $"[red]{response.RecordingDevice}[/]");
                    table.AddRow("[green]Playback Comm[/]", $"[green]{response.PlaybackCommunicationDevice}[/]");
                    table.AddRow("[red]Recording Comm[/]", $"[red]{response.RecordingCommunicationDevice}[/]");

                    AnsiConsole.Write(table);
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
