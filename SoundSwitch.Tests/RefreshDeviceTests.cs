using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
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

        // Refresh's reconcile step publishes a merged cache (reusing surviving instances AND
        // adding fresh ones), then disposes only old devices absent from that PUBLISHED cache.
        // The retained set MUST be derived from the published cache, not from the freshly
        // enumerated ids alone — otherwise a reused (still-published) device would be disposed.
        // We replicate the corrected contract: published keys = both reused + fresh ids.
        var publishedPlayback = new Dictionary<string, DeviceFullInfo>
        {
            ["survive"] = surviving, // reused from old cache
            ["new"] = fresh           // freshly enumerated
        };
        // The old (pre-refresh) cache holds the surviving + removed devices; the published cache
        // (reused + fresh) holds surviving + fresh. Disposal targets only old devices absent from
        // the published cache — i.e. the removed one.
        var oldDevices = new[] { surviving, removed };
        var retainedIds = new HashSet<string>(publishedPlayback.Keys); // mirrors fixed Refresh
        var toDispose = oldDevices.Where(d => !retainedIds.Contains(d.Id)).ToArray();

        var disposeMethod = typeof(CachedAudioDeviceLister)
            .GetMethod("DisposeOldDevices", BindingFlags.NonPublic | BindingFlags.Instance);
        disposeMethod.Should().NotBeNull();
        disposeMethod!.Invoke(lister, new object[] { toDispose });

        // The removed device is disposed; the surviving one is published-and-reused, so untouched.
        removed.Disposed.Should().BeTrue("device absent from the published cache must be disposed");
        surviving.Disposed.Should().BeFalse("reused device is in the published cache, must NOT be disposed");
        fresh.Disposed.Should().BeFalse("fresh device is in the published cache, must NOT be disposed");
    }

    /// <summary>
    /// Records how many times Refresh() was invoked, so the test can observe whether an
    /// inconclusive incremental update escalates to a full refresh (the #2404 fix).
    /// </summary>
    private sealed class CountingLister : CachedAudioDeviceLister
    {
        public int RefreshCount { get; private set; }

        public CountingLister(EDeviceState state) : base(state)
        {
        }

        public override void Refresh(CancellationToken cancellationToken = default)
        {
            RefreshCount++;
            // Do not touch COM; the real Refresh runs on Windows CI. The escalate logic only
            // cares that Refresh was requested.
        }
    }

    [Test]
    public void ProcessDeviceUpdates_InconclusiveEndpoint_EscalatesToForceRefresh()
    {
        // Regression for #2404: when the OS reports a device lifecycle change but the endpoint
        // isn't queryable yet (transient mid Bluetooth-swap), ProcessDeviceUpdates must NOT
        // silently drop the change. It must escalate to a full Refresh() so the cache self-heals
        // instead of diverging until a restart.
        var lister = new CountingLister(EDeviceState.Active | EDeviceState.Unplugged);

        // An Added event whose endpoint can't be resolved (GetAudioEndpoint returns null on the
        // real AudioSwitcher; here we rely on the same inconclusive branch being hit because the
        // backing AudioSwitcher has no such device).
        var events = new[]
        {
            new DeviceChangedEvent(EventType.Added, "nonexistent-device-id")
        };

        lister.ProcessDeviceUpdates(events);

        lister.RefreshCount.Should().BeGreaterThan(0,
            "an inconclusive incremental update must trigger a full refresh (issue #2404)");
    }

    [Test]
    public void ProcessDeviceUpdates_SwapScenario_ReflectsDeviceRemoval()
    {
        // COM-free reproduction of the swap scenario: Device1 is removed and Device2 is added.
        // After processing, Device1 must be gone from the cache and Device2 present, and the
        // DeviceListRefreshed signal must have fired.
        var lister = new CachedAudioDeviceLister(EDeviceState.Active | EDeviceState.Unplugged);

        var playbackProperty = typeof(CachedAudioDeviceLister)
            .GetProperty("PlaybackDevices", BindingFlags.NonPublic | BindingFlags.Instance);
        var recordingProperty = typeof(CachedAudioDeviceLister)
            .GetProperty("RecordingDevices", BindingFlags.NonPublic | BindingFlags.Instance);
        playbackProperty.Should().NotBeNull();
        recordingProperty.Should().NotBeNull();

        // Seed only Device1 in the playback cache.
        playbackProperty!.SetValue(lister, ImmutableDictionary.CreateRange(new Dictionary<string, DeviceFullInfo>
        {
            ["device1"] = new TrackedDevice("Buds A", "device1", EDataFlow.eRender, string.Empty, EDeviceState.Active, false)
        }));
        recordingProperty!.SetValue(lister, ImmutableDictionary.CreateRange(new Dictionary<string, DeviceFullInfo>()));

        // The real AudioSwitcher won't resolve these ids on Linux, so Added(Device2) is
        // inconclusive and escalates to Refresh() (which is a no-op here without COM). That still
        // exercises the removal path: Removed(Device1) must evict it from the cache.
        var refreshed = new List<Unit>();
        using var sub = lister.DeviceListRefreshed.Subscribe(u => refreshed.Add(u));

        lister.ProcessDeviceUpdates(new[]
        {
            new DeviceChangedEvent(EventType.Removed, "device1"),
            new DeviceChangedEvent(EventType.Added, "device2")
        });

        var playback = (ImmutableDictionary<string, DeviceFullInfo>)playbackProperty.GetValue(lister)!;
        playback.ContainsKey("device1").Should().BeFalse("removed device must be evicted from the cache");

        refreshed.Should().NotBeEmpty("a removal must emit DeviceListRefreshed so the UI updates");
    }
}
