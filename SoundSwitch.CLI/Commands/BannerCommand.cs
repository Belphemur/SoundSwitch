#nullable enable
using SoundSwitch.Banner;
using SoundSwitch.IPC.Pipe.Messages.TriggerSwitch;
using SoundSwitch.IPC.Pipe.Messages.ShowBanner;
using SoundSwitch.IPC.Pipe;
using Spectre.Console.Cli;
using Spectre.Console;
using System.ComponentModel;

namespace SoundSwitch.CLI.Commands;

public class BannerCommand : AsyncCommand<BannerCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "[message]")]
        [Description("The message to display in the banner")]
        public string? Message { get; set; }

        [CommandOption("-t|--title")]
        [Description("The title of the banner")]
        public string? Title { get; set; }

        [CommandOption("-i|--image")]
        [Description("The image source (ID, path, URL, or Base64)")]
        public string? Image { get; set; }

        [CommandOption("--ttl")]
        [Description("Time to live in seconds")]
        public int? Ttl { get; set; }

        [CommandOption("-s|--screen")]
        [Description("Target screen (PrimaryScreen, ActiveScreen, FollowCursor)")]
        public ShowOnScreen? Screen { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        try
        {
            var request = new ShowBannerRequest
            {
                Title = settings.Title ?? "SoundSwitch",
                Text = settings.Message ?? string.Empty,
                ImageSource = settings.Image,
                Ttl = settings.Ttl.HasValue ? TimeSpan.FromSeconds(settings.Ttl.Value) : null,
                Screen = settings.Screen
            };

            return await AnsiConsole.Status()
                .StartAsync("Sending banner request...", async _ =>
                {
                    var response = await NamedPipe.SendRequestAsync<TriggerSwitchResponse>(PipeConstants.GetUserPipeName(),
                        request, cancellationToken);

                    if (response.Success)
                    {
                        AnsiConsole.MarkupLine("[green]Successfully sent banner request[/]");
                        return 0;
                    }

                    AnsiConsole.MarkupLine("[red]Failed to send banner request[/]");
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
