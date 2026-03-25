using FluentAssertions;
using NAudio.CoreAudioApi;
using NUnit.Framework;
using Serilog.Events;
using Serilog;
using SoundSwitch.Framework.Audio.Lister;
using System.Threading.Tasks;
using System;

namespace SoundSwitch.Tests;

[TestFixture]
public class RefreshDeviceTests
{
    [OneTimeSetUp]
    public void Setup()
    {
        const string outputTemplate = "[{Timestamp:HH:mm:ss.fff} {Level:u3}]]{Properties} {Message}(at {Caller}){NewLine}{Exception}";

        Log.Logger = new LoggerConfiguration().MinimumLevel.Verbose()
            .WriteTo.Console(LogEventLevel.Verbose, outputTemplate, theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code)
            .CreateLogger();
    }

    [Test]
    public async Task TestMultipleRefresh()
    {
        if (Environment.GetEnvironmentVariable("CI") != null)
        {
            Assert.Ignore("CI doesn't have audio device to make this test work");
        }

        var cachedAudioDeviceLister = new CachedAudioDeviceLister(DeviceState.All);

        var refresh = async () =>
        {
            await Console.Out.WriteLineAsync("Refreshing delayed");
            await Task.Delay(50);
            cachedAudioDeviceLister.Refresh();
        };

        var refreshCancelled = async () =>
        {
            await Console.Out.WriteLineAsync("Refreshing");
            cachedAudioDeviceLister.Refresh();
        };

        await Task.WhenAll(refresh(), refreshCancelled.Should().ThrowAsync<OperationCanceledException>());

        cachedAudioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active).Should().NotBeEmpty();
    }
}
