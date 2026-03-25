using SoundSwitch.Banner.CLI.Infrastructure;
using SoundSwitch.Banner;
using Spectre.Console.Cli;
using Spectre.Console;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System;

namespace SoundSwitch.Banner.CLI.Commands;

public class ShowCommand : Command<ShowCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-t|--title")]
        [Description("The title of the notification.")]
        public string Title { get; set; } = "SoundSwitch Test";

        [CommandOption("-m|--message")]
        [Description("The message content.")]
        public string Message { get; set; } = "This is a test notification.";

        [CommandOption("-p|--persistent")]
        [Description("Enable persistent notification.")]
        public bool Persistent { get; set; }

        [CommandOption("-o|--opacity")]
        [DefaultValue(100)]
        public int Opacity { get; set; }

        [CommandOption("-d|--duration")]
        [DefaultValue(3)]
        public int DurationSeconds { get; set; }

        [CommandOption("--sound")]
        [Description("Path to a sound file to play.")]
        public string? SoundPath { get; set; }

        [CommandOption("--pos")]
        [Description("Position (TopLeft, TopRight, etc.)")]
        public BannerPosition Position { get; set; } = BannerPosition.TopLeft;
    }

    public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var config = new CliBannerConfiguration
        {
            Opacity = settings.Opacity,
            Ttl = TimeSpan.FromSeconds(settings.DurationSeconds)
        };

        var audioService = new CliBannerAudioService();
        
        // Ensure SynchronizationContext for WinForms
        var syncContext = new WindowsFormsSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(syncContext);

        BannerService.Setup();
        var bannerService = new BannerService(config, audioService, null);

        var request = new BannerRequest
        {
            Title = settings.Title,
            Text = settings.Message,
            Priority = 1,
            SoundPath = settings.SoundPath
        };

        AnsiConsole.MarkupLine($"[green]Showing banner:[/] {settings.Title} - {settings.Message} " + (settings.Persistent ? "[yellow](Persistent)[/]" : ""));
        
        bannerService.Show(request, settings.Position, settings.Persistent);

        // Run the message loop to keep the banner alive
        System.Windows.Forms.Application.Run();

        return 0;
    }
}
