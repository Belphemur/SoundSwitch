using SoundSwitch.CLI.Commands;

using Spectre.Console.Cli;

namespace SoundSwitch.CLI;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.SetApplicationName("SoundSwitch.CLI");

            config.AddCommand<SwitchCommand>("switch")
                .WithDescription("Switch audio device type")
                .WithExample("switch", "--type", "Recording")
                .WithExample("switch", "--type", "Playback")
                .WithExample("switch", "--type", "Playback", "--json");

            config.AddCommand<ProfileCommand>("profile")
                .WithDescription("Manage audio profiles")
                .WithExample("profile", "--list")
                .WithExample("profile", "--list", "--json")
                .WithExample("profile", "--name", "Headphones + Mic");

            config.AddCommand<SettingsCommand>("settings")
                .WithDescription("Open SoundSwitch settings")
                .WithExample("settings", "--json");

            config.AddCommand<MuteCommand>("mute")
                .WithDescription("Control microphone mute state")
                .WithExample("mute", "--state", "true")
                .WithExample("mute", "--toggle")
                .WithExample("mute")
                .WithExample("mute", "--json");

            config.AddCommand<StatusCommand>("status")
                .WithDescription("Show active profile and current audio devices")
                .WithExample("status")
                .WithExample("status", "--json");

            config.AddCommand<DevicesCommand>("devices")
                .WithDescription("List devices selected for switching")
                .WithExample("devices")
                .WithExample("devices", "--json");
        });

        return await app.RunAsync(args);
    }
}
