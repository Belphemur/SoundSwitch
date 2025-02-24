using SoundSwitch.IPC.Pipe;
using SoundSwitch.IPC.Pipe.Messages.GetProfileList;
using SoundSwitch.IPC.Pipe.Messages.TriggerProfile;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SoundSwitch.CLI.Commands;

public class ProfileCommand : AsyncCommand<ProfileCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-l|--list")]
        public bool List { get; set; }

        [CommandOption("-n|--name")]
        public string? Name { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            if (settings.List)
            {
                return await AnsiConsole.Status()
                    .StartAsync("Fetching profiles...", async _ =>
                    {
                        var response = await NamedPipe.SendRequestAsync<GetProfileListResponse>(PipeConstants.GetUserPipeName(),
                            new GetProfileListRequest());

                        var table = new Table()
                            .AddColumn("Profile")
                            .AddColumn(new TableColumn("[green]Playback[/]"))
                            .AddColumn(new TableColumn("[green]Playback Comm[/]"))
                            .AddColumn(new TableColumn("[red]Recording[/]"))
                            .AddColumn(new TableColumn("[red]Recording Comm[/]"))
                            .Border(TableBorder.Rounded);

                        foreach (var profile in response.Profiles)
                        {
                            table.AddRow(
                                profile.Name,
                                $"[green]{profile.PlaybackDevice}[/]",
                                $"[green]{profile.PlaybackCommunicationDevice}[/]",
                                $"[red]{profile.RecordingDevice}[/]",
                                $"[red]{profile.RecordingCommunicationDevice}[/]"
                            );
                        }

                        AnsiConsole.Write(table);
                        return 0;
                    });
            }

            if (string.IsNullOrEmpty(settings.Name))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Profile name is required unless --list is specified");
                return 1;
            }

            return await AnsiConsole.Status()
                .StartAsync($"Triggering profile {settings.Name}...", async _ =>
                {
                    var response = await NamedPipe.SendRequestAsync<TriggerProfileResponse>(PipeConstants.GetUserPipeName(),
                        new TriggerProfileRequest { ProfileName = settings.Name });

                    if (response.Success)
                    {
                        AnsiConsole.MarkupLine($"[green]Successfully triggered profile {settings.Name}[/]");
                        return 0;
                    }

                    AnsiConsole.MarkupLine($"[red]Failed to trigger profile:[/] {response.Error}");
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