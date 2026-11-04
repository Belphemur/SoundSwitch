using System;
using System.Collections.Generic;
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
    /// Regression test for Sentry SOUNDSWITCH-49X / GitHub #2554: Refresh must capture the
    /// retained-id set while _cacheLock blocks ProcessDeviceUpdates' in-place edits. Drives the
    /// REAL publication block (<c>PublishRefreshedDevices</c>, reflected, seeded with a cache via
    /// <see cref="SetCache"/>) while a mutator thread mirrors <c>ProcessDeviceUpdates</c>:
    /// triggered the instant the publication swaps the caches in (the sync point), it reads the
    /// CURRENT published dictionary and does indexer-set + Remove edits under the SAME
    /// publication lock.
    /// If the capture is ever moved back outside the lock (the pre-fix pattern), the live
    /// Dictionary's version bumps during the capture and its ctor throws
    /// <see cref="InvalidOperationException"/> "Collection was modified"; the fresh candidates
    /// are seeded with 100k ids per flow so the capture takes milliseconds — an ample window for
    /// the lock-churning mutator to overlap it. With the capture under the lock (this fix), the
    /// publication is serialized against those edits: the detached result is immune, the
    /// mutator's post-publication edits show up in the live cache but never in the captured set,
    /// and the disposal decision disposes only the device absent from the published caches.
    /// </summary>
    [Test]
    public void PublishRefreshedDevices_CapturesRetainedIdsUnderLock_AgainstInPlaceMutator()
    {
        var lister = new CachedAudioDeviceLister(EDeviceState.All);
        var publish = typeof(CachedAudioDeviceLister).GetMethod("PublishRefreshedDevices", InstanceMembers);
        publish.Should().NotBeNull("the test drives the real publication block through reflection");
        // The very same lock instance the production code enters: taking it with Monitor instead
        // of System.Threading.Lock would throw, and any other object would serialize nothing.
        var cacheLock = (Lock)typeof(CachedAudioDeviceLister).GetField("_cacheLock", InstanceMembers)!.GetValue(lister)!;
        var playbackField = typeof(CachedAudioDeviceLister).GetField("_playbackDevices", InstanceMembers)!;
        var recordingField = typeof(CachedAudioDeviceLister).GetField("_recordingDevices", InstanceMembers)!;

        // Pre-refresh published cache: 'survive' is reused by the publication, 'gone' is a device
        // the new enumeration no longer contains (so the disposal decision must dispose it).
        var surviving = new TrackedDevice("Speaker", "survive", EDataFlow.eRender, string.Empty, EDeviceState.Active, false);
        var removed = new TrackedDevice("Headphones", "gone", EDataFlow.eRender, string.Empty, EDeviceState.Active, false);
        var seededPlayback = new Dictionary<string, DeviceFullInfo> { ["survive"] = surviving, ["gone"] = removed };
        SetCache(lister, seededPlayback, new Dictionary<string, DeviceFullInfo>());

        // Freshly enumerated candidates to publish; all values share one COM-free instance:
        // only the keys feed the merge and the retained set, so values need not be distinct.
        var sharedFreshDevice = MakeDevice();
        var freshPlayback = new Dictionary<string, DeviceFullInfo>();
        var freshRecording = new Dictionary<string, DeviceFullInfo>();
        for (var i = 0; i < 100_000; i++)
        {
            freshPlayback["p" + i] = sharedFreshDevice;
            freshRecording["r" + i] = sharedFreshDevice;
        }
        // A fresh instance published for an id whose existing instance survives: the publication
        // must REUSE 'surviving' and reject this fresh rival.
        freshPlayback["survive"] = new TrackedDevice("Speaker fresh", "survive", EDataFlow.eRender, string.Empty, EDeviceState.Active, false);
        var newIds = new HashSet<string>(freshPlayback.Keys.Concat(freshRecording.Keys));

        var publishTask = Task.Run(() => publish!.Invoke(lister,
            new object[] { freshPlayback, freshRecording, newIds }));

        // Mutator mirrors ProcessDeviceUpdates: triggered the instant the publication swaps the
        // caches in (the sync point), it reads the CURRENT published instance and then does
        // indexer-set + Remove edits of it under the SAME publication lock. In the fixed code the
        // capture is inside the same lock critical section, so every edit waits for it; if the
        // capture is ever moved outside the lock (the pre-fix pattern), it runs for milliseconds
        // over the 200k published ids while these consecutive edits churn the Dictionary version,
        // and the capture's ctor throws "Collection was modified".
        var mutator = Task.Run(() =>
        {
            if (!SpinWait.SpinUntil(
                    () => !ReferenceEquals(playbackField.GetValue(lister), seededPlayback),
                    30_000))
            {
                Assert.Fail("the publication never swapped the caches; the mutator had nothing to race");
            }

            for (var i = 0; i < 50_000; i++)
            {
                lock (cacheLock)
                {
                    var playback = (Dictionary<string, DeviceFullInfo>)playbackField.GetValue(lister)!;
                    var recording = (Dictionary<string, DeviceFullInfo>)recordingField.GetValue(lister)!;
                    playback["mutant" + i] = sharedFreshDevice;
                    playback.Remove("mutant" + (i - 2));
                    recording["mutant" + i] = sharedFreshDevice;
                    recording.Remove("mutant" + (i - 2));
                }

                if (i % 512 == 0) Thread.Sleep(1); // keep churning the lock, but yield to the test thread
            }
        });

        var publication = publishTask.Result;
        publication.Should().NotBeNull("PublishRefreshedDevices must complete without throwing");
        var publicationType = publication.GetType();
        var oldDevices = (DeviceFullInfo[])publicationType.GetField("Item1")!.GetValue(publication)!;
        var retainedIds = (HashSet<string>)publicationType.GetField("Item2")!.GetValue(publication)!;
        var rejectedFresh = (List<DeviceFullInfo>)publicationType.GetField("Item3")!.GetValue(publication)!;

        // Publication decision: the existing 'surviving' instance is reused (its fresh rival is
        // rejected), the fresh majority is retained, and 'removed' — absent from the new
        // enumeration — is not. The retained set holds every id of the PUBLISHED caches.
        rejectedFresh.Should().ContainSingle().Which.Should().Be(freshPlayback["survive"]);
        retainedIds.Should().Contain("survive", "reused instances stay published")
            .And.NotContain("gone", "the removed device is absent from the published caches")
            .And.NotContain(key => key.StartsWith("mutant"), "no mutator edit is ever retained")
            .And.HaveCount(newIds.Count);

        // Refresh's disposal decision, on the real helper: only instances absent from the
        // retained set may be disposed.
        var disposeOldDevices = GetDisposeOldDevices();
        disposeOldDevices.Invoke(lister, new object[] { rejectedFresh.ToArray() });
        ((TrackedDevice)freshPlayback["survive"]).Disposed.Should().BeTrue("the fresh instance that lost the reuse race is disposed");
        surviving.Disposed.Should().BeFalse("the reused instance stays published");

        var removedOld = oldDevices.Where(d => !retainedIds.Contains(d.Id)).ToArray();
        removedOld.Should().ContainSingle().Which.Should().Be(removed);
        disposeOldDevices.Invoke(lister, new object[] { removedOld });
        removed.Disposed.Should().BeTrue("device absent from the published caches must be disposed");

        // The mutator's edits land in the LIVE published caches but never in the captured set
        // (the capture was serialized against them by the publication lock).
        mutator.Wait();
        GetCache(lister, "PlaybackDevices").ContainsKey("mutant" + 49_999)
            .Should().BeTrue("in-place mutator edits reach the published cache");
        GetCache(lister, "RecordingDevices").ContainsKey("mutant" + 49_999)
            .Should().BeTrue("in-place mutator edits reach the published cache");
        retainedIds.Should().NotContain("mutant" + 49_999, "the set was captured before the post-publication edits");
    }

    private const BindingFlags InstanceMembers = BindingFlags.NonPublic | BindingFlags.Instance;

    /// <summary>
    /// <see cref="CachedAudioDeviceLister"/> publishes its caches through read-only properties
    /// backed by mutable dictionaries swapped/mutated under its internal lock. Seeding the backing
    /// fields directly lets a test (a) install a cache without enumerating COM endpoints and
    /// (b) keep holding the very same dictionary instance the lister uses, so a concurrent mutator
    /// can edit the live cache the way <c>ProcessDeviceUpdates</c> does.
    /// </summary>
    private static void SetCache(CachedAudioDeviceLister lister,
        Dictionary<string, DeviceFullInfo> playback,
        Dictionary<string, DeviceFullInfo> recording)
    {
        typeof(CachedAudioDeviceLister).GetField("_playbackDevices", InstanceMembers)!.SetValue(lister, playback);
        typeof(CachedAudioDeviceLister).GetField("_recordingDevices", InstanceMembers)!.SetValue(lister, recording);
    }

    /// <summary>
    /// Reads a published cache through the lister's own property, so assertions observe what
    /// readers (and <c>Refresh</c>'s retained-set computation) would see.
    /// </summary>
    private static IReadOnlyDictionary<string, DeviceFullInfo> GetCache(CachedAudioDeviceLister lister, string propertyName)
    {
        var property = typeof(CachedAudioDeviceLister).GetProperty(propertyName, InstanceMembers);
        property.Should().NotBeNull();
        return (IReadOnlyDictionary<string, DeviceFullInfo>)property!.GetValue(lister)!;
    }

    private static MethodInfo GetDisposeOldDevices()
    {
        var method = typeof(CachedAudioDeviceLister).GetMethod("DisposeOldDevices", InstanceMembers);
        method.Should().NotBeNull();
        return method!;
    }

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

        GetDisposeOldDevices().Invoke(lister, new object[] { new[] { device } });

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
        var disposeMethod = GetDisposeOldDevices();

        var playback = new Dictionary<string, DeviceFullInfo> { ["p1"] = MakeDevice() };
        var recording = new Dictionary<string, DeviceFullInfo> { ["r1"] = MakeDevice() };
        // Hand the lister the very dictionaries the mutator below edits, so the concurrent
        // mutation hits the live published caches instead of detached copies.
        SetCache(lister, playback, recording);

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

        Action dispose = () => disposeMethod.Invoke(lister, new object[] { snapshot });
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
        var disposeMethod = GetDisposeOldDevices();

        // Surviving device: stays across the refresh.
        var surviving = new TrackedDevice("Speaker", "survive", EDataFlow.eRender, string.Empty, EDeviceState.Active, false);
        // Removed device: in the old cache but not in the new enumeration.
        var removed = new TrackedDevice("Headphones", "gone", EDataFlow.eRender, string.Empty, EDeviceState.Active, false);
        // New device: only in the new enumeration.
        var fresh = new TrackedDevice("Mic", "new", EDataFlow.eCapture, string.Empty, EDeviceState.Active, false);

        // Seed the cache as if a previous refresh had published these.
        var oldPlayback = new Dictionary<string, DeviceFullInfo>
        {
            ["survive"] = surviving,
            ["gone"] = removed
        };
        SetCache(lister, oldPlayback, new Dictionary<string, DeviceFullInfo>());

        // Snapshot of the pre-refresh cache, taken under the lock by Refresh before publishing.
        var oldDevices = oldPlayback.Values.ToArray();

        // Refresh's reconcile step then publishes a merged cache (reusing surviving instances AND
        // adding fresh ones), and only after that disposes old devices absent from the PUBLISHED
        // cache. The retained set MUST come from the published cache, not from the freshly
        // enumerated ids alone — otherwise a reused (still-published) device would be disposed.
        SetCache(lister, new Dictionary<string, DeviceFullInfo>
        {
            ["survive"] = surviving, // reused from the old cache
            ["new"] = fresh           // freshly enumerated
        }, new Dictionary<string, DeviceFullInfo>());

        var retainedIds = new HashSet<string>(
            GetCache(lister, "PlaybackDevices").Keys.Concat(GetCache(lister, "RecordingDevices").Keys));
        var toDispose = oldDevices.Where(d => !retainedIds.Contains(d.Id)).ToArray();

        disposeMethod.Invoke(lister, new object[] { toDispose });

        toDispose.Should().ContainSingle().Which.Should().Be(removed,
            "only the device missing from the published cache may be disposed");
        removed.Disposed.Should().BeTrue("device absent from the published cache must be disposed");
        surviving.Disposed.Should().BeFalse("reused device is in the published cache, must NOT be disposed");
        fresh.Disposed.Should().BeFalse("fresh device is in the published cache, must NOT be disposed");

        // Regression contrast: deriving the retained set from the freshly enumerated ids alone
        // (the pre-fix behaviour) would schedule the still-published reused instance for disposal.
        var freshlyEnumeratedIds = new HashSet<string> { fresh.Id };
        oldDevices.Where(d => !freshlyEnumeratedIds.Contains(d.Id))
            .Should().Contain(surviving, "the bug this test guards against disposes reused devices");
    }
}
