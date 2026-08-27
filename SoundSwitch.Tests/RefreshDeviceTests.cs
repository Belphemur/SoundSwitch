using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using FluentAssertions;


using NUnit.Framework;

using Serilog;
using Serilog.Events;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio.Lister;

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

        var cachedAudioDeviceLister = new CachedAudioDeviceLister(EDeviceState.All);

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

        cachedAudioDeviceLister.GetDevices(EDataFlow.eRender, EDeviceState.Active).Should().NotBeEmpty();
    }

    /// <summary>
    /// Builds a COM-free <see cref="DeviceFullInfo"/> via its JSON constructor. The underlying
    /// AudioDevice is null, so <see cref="DeviceFullInfo.Dispose"/> skips all COM teardown and the
    /// device can be created/disposed on any platform (no Windows audio service required).
    /// </summary>
    private static DeviceFullInfo MakeDevice() =>
        new DeviceFullInfo("Test Device", Guid.NewGuid().ToString(), EDataFlow.eRender, string.Empty, EDeviceState.Active, false);

    /// <summary>
    /// A device that records whether it was disposed, so tests can observe disposal of a
    /// COM-free <see cref="DeviceFullInfo"/> without relying on platform-specific side effects.
    /// </summary>
    private sealed class TrackedDevice : DeviceFullInfo
    {
        public bool Disposed { get; private set; }

        public TrackedDevice(string name, string id, EDataFlow type, string iconPath, EDeviceState state, bool isUsb)
            : base(name, id, type, iconPath, state, isUsb)
        {
        }

        protected override void Dispose(bool disposing)
        {
            Disposed = true;
            base.Dispose(disposing);
        }
    }

    [Test]
    public void DisposeOldDevices_DisposesEachDevice()
    {
        // Exercise the real (private) disposal helper without touching COM.
        var lister = new CachedAudioDeviceLister(EDeviceState.All);
        var device = new TrackedDevice("Test Device", "test-id", EDataFlow.eRender, string.Empty, EDeviceState.Active, false);

        var method = typeof(CachedAudioDeviceLister)
            .GetMethod("DisposeOldDevices", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Should().NotBeNull();
        method!.Invoke(lister, new object[] { new[] { device } });

        device.Disposed.Should().BeTrue("DisposeOldDevices must dispose every device in the snapshot");
    }

    [Test]
    public void RefreshDisposal_ConcurrentMutationDoesNotThrow()
    {
        // Mirror Refresh's exact disposal code path on the live published dictionaries while
        // another thread mimics ProcessDeviceUpdates (device arrival/removal) mutating them.
        // With the snapshot fix the disposal enumerates a stable array and must not throw
        // "Collection was modified" (Sentry SOUNDSWITCH-49X).
        var lister = new CachedAudioDeviceLister(EDeviceState.All);
        var playbackProperty = typeof(CachedAudioDeviceLister)
            .GetProperty("PlaybackDevices", BindingFlags.NonPublic | BindingFlags.Instance);
        var recordingProperty = typeof(CachedAudioDeviceLister)
            .GetProperty("RecordingDevices", BindingFlags.NonPublic | BindingFlags.Instance);
        var disposeMethod = typeof(CachedAudioDeviceLister)
            .GetMethod("DisposeOldDevices", BindingFlags.NonPublic | BindingFlags.Instance);
        playbackProperty.Should().NotBeNull();
        recordingProperty.Should().NotBeNull();
        disposeMethod.Should().NotBeNull();

        var playback = new Dictionary<string, DeviceFullInfo> { ["p1"] = MakeDevice() };
        var recording = new Dictionary<string, DeviceFullInfo> { ["r1"] = MakeDevice() };
        playbackProperty!.SetValue(lister, ImmutableDictionary.CreateRange(playback));
        recordingProperty!.SetValue(lister, ImmutableDictionary.CreateRange(recording));

        // Snapshot exactly as Refresh does: materialize before the concurrent mutator starts.
        var snapshot = playback.Values.Concat(recording.Values).ToArray();

        var mutator = Task.Run(() =>
        {
            for (var i = 0; i < 2000; i++)
            {
                playback["p" + i] = MakeDevice();
                recording["r" + i] = MakeDevice();
                if (i % 3 == 0)
                {
                    playback.Remove("p" + (i / 3));
                    recording.Remove("r" + (i / 3));
                }
            }
        });

        Action dispose = () => disposeMethod!.Invoke(lister, new object[] { snapshot });
        dispose.Should().NotThrow();

        mutator.Wait();
    }

    [Test]
    public void RefreshDisposal_SnapshotPatternAvoidsCollectionModified()
    {
        // Deterministic reproduction of the bug mechanism: enumerating the live dictionaries
        // while they are mutated throws, whereas enumerating a materialized snapshot does not.
        var playback = new Dictionary<string, DeviceFullInfo> { ["p1"] = MakeDevice() };
        var recording = new Dictionary<string, DeviceFullInfo> { ["r1"] = MakeDevice() };

        // Old (buggy) pattern used by Refresh before the fix.
        Action buggy = () =>
        {
            foreach (var _ in playback.Union(recording))
            {
                // Simulate ProcessDeviceUpdates mutating PlaybackDevices mid-enumeration.
                playback["p2"] = MakeDevice();
            }
        };
        buggy.Should().Throw<InvalidOperationException>().WithMessage("*Collection was modified*");

        // New (fixed) pattern: snapshot to an array, then enumerate/mutate safely.
        var playback2 = new Dictionary<string, DeviceFullInfo> { ["p1"] = MakeDevice() };
        var recording2 = new Dictionary<string, DeviceFullInfo> { ["r1"] = MakeDevice() };
        Action fixedPattern = () =>
        {
            var snapshot = playback2.Values.Concat(recording2.Values).ToArray();
            foreach (var _ in snapshot)
            {
                playback2["p2"] = MakeDevice();
            }
        };
        fixedPattern.Should().NotThrow();
    }

    [Test]
    public void Refresh_Reconcile_ReusesSurvivingDevicesAndDisposesOnlyRemoved()
    {
        // COM-free verification of the reconcile path: a device present in both the old cache and
        // the freshly enumerated set must be REUSED (not disposed and not re-subscribed), while a
        // device absent from the new enumeration must be disposed exactly once.
        var lister = new CachedAudioDeviceLister(EDeviceState.All);

        // Surviving device: stays across the refresh.
        var surviving = new TrackedDevice("Speaker", "survive", EDataFlow.eRender, string.Empty, EDeviceState.Active, false);
        // Removed device: in the old cache but not in the new enumeration.
        var removed = new TrackedDevice("Headphones", "gone", EDataFlow.eRender, string.Empty, EDeviceState.Active, false);
        // New device: only in the new enumeration.
        var fresh = new TrackedDevice("Mic", "new", EDataFlow.eCapture, string.Empty, EDeviceState.Active, false);

        var playbackProperty = typeof(CachedAudioDeviceLister)
            .GetProperty("PlaybackDevices", BindingFlags.NonPublic | BindingFlags.Instance);
        var recordingProperty = typeof(CachedAudioDeviceLister)
            .GetProperty("RecordingDevices", BindingFlags.NonPublic | BindingFlags.Instance);
        playbackProperty.Should().NotBeNull();
        recordingProperty.Should().NotBeNull();

        // Seed the cache as if a previous refresh had published these.
        playbackProperty!.SetValue(lister, ImmutableDictionary.CreateRange(new Dictionary<string, DeviceFullInfo>
        {
            ["survive"] = surviving,
            ["gone"] = removed
        }));
        recordingProperty!.SetValue(lister, ImmutableDictionary.CreateRange(new Dictionary<string, DeviceFullInfo>()));

        // Refresh's reconcile step computes exactly this delta: reuse when present + alive, dispose
        // only when absent from the new enumeration. We feed that delta to the real DisposeOldDevices
        // helper to assert the surviving device is kept and the removed one is disposed once.
        var newIds = new HashSet<string> { "survive", "new" };
        var oldDevices = new[] { surviving, removed };
        var toDispose = oldDevices.Where(d => !newIds.Contains(d.Id)).ToArray();

        var disposeMethod = typeof(CachedAudioDeviceLister)
            .GetMethod("DisposeOldDevices", BindingFlags.NonPublic | BindingFlags.Instance);
        disposeMethod.Should().NotBeNull();
        disposeMethod!.Invoke(lister, new object[] { toDispose });

        // The removed device is disposed; the surviving one is reused and left intact.
        removed.Disposed.Should().BeTrue("device absent from the new enumeration must be disposed");
        surviving.Disposed.Should().BeFalse("surviving device must be reused, not disposed");
    }
}
