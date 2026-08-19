#nullable enable
using SoundSwitch.IPC.Pipe;
using SoundSwitch.IPC.Pipe.Messages.Cli;
using SoundSwitch.IPC.Pipe.Messages.GetProfileList;
using SoundSwitch.IPC.Pipe.Messages.TriggerProfile;

using Spectre.Console;
using Spectre.Console.Cli;

namespace SoundSwitch.CLI.Commands;

public class ProfileCommand : AsyncCommand<ProfileCommand.Settings>
{
    public class Settings : JsonCommandSettings
    {
        [CommandOption("-l|--list")]
        public bool List { get; set; }

        [CommandOption("-n|--name")]
        public string? Name { get; set; }
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
                new CliCommandExecuted { Command = "profile" },
                cancellationToken);
        }
        catch { /* best-effort */ }
        return exitCode;
    }

    private static async Task<int> ExecuteJsonAsync(Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (settings.List)
            {
                var response = await NamedPipe.SendRequestAsync<GetProfileListResponse>(
                    PipeConstants.GetUserPipeName(),
                    new GetProfileListRequest(),
                    cancellationToken);

                JsonOutput.Write(response.Profiles.Select(p => new
                {
                    name = p.Name,
                    playbackDevice = p.PlaybackDevice,
                    playbackCommunicationDevice = p.PlaybackCommunicationDevice,
                    recordingDevice = p.RecordingDevice,
                    recordingCommunicationDevice = p.RecordingCommunicationDevice
                }).ToArray());
                return 0;
            }

            if (string.IsNullOrEmpty(settings.Name))
            {
                JsonOutput.WriteError("Profile name is required unless --list is specified");
                return 1;
            }

            var triggerResponse = await NamedPipe.SendRequestAsync<TriggerProfileResponse>(
                PipeConstants.GetUserPipeName(),
                new TriggerProfileRequest { ProfileName = settings.Name },
                cancellationToken);

            if (triggerResponse.Success)
            {
                JsonOutput.Write(new { success = true, profile = settings.Name });
                return 0;
            }

            JsonOutput.WriteError($"Failed to trigger profile: {triggerResponse.Error}");
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
            if (settings.List)
            {
                return await AnsiConsole.Status()
                    .StartAsync("Fetching profiles...", async _ =>
                    {
                        var response = await NamedPipe.SendRequestAsync<GetProfileListResponse>(PipeConstants.GetUserPipeName(),
                            new GetProfileListRequest(), cancellationToken);

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
                        new TriggerProfileRequest { ProfileName = settings.Name }, cancellationToken);

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
