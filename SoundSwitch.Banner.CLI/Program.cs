using SoundSwitch.Banner.CLI.Commands;
using Spectre.Console.Cli;
using System;

namespace SoundSwitch.Banner.CLI;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.AddCommand<ShowCommand>("show");
            config.SetApplicationName("BannerCLI");
        });
        return app.Run(args);
    }
}
