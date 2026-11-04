using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.Banner.BannerPosition.Position;
using SoundSwitch.Framework.Banner.MicrophoneMute;

namespace SoundSwitch.Tests;

/// <summary>
/// Regression tests for the rearrange pass of the persistent mute banners (Sentry
/// SOUNDSWITCH-40H). The pass must tolerate stale tracked banners — forms that were
/// disposed without going through the manager — instead of crashing on them.
/// </summary>
// BannerForm (a WinForms form) is constructed, disposed and Screen-pinned here; it requires an STA thread.
[TestFixture]
[Apartment(ApartmentState.STA)]
public class MicrophoneMuteBannerManagerTests
{
    private static Dictionary<string, BannerForm> GetTrackedBanners(MicrophoneMuteBannerManager manager) =>
        (Dictionary<string, BannerForm>) manager.GetType()
            .GetField("_activeBanners", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(manager)!;

    private static void RearrangeBanners(MicrophoneMuteBannerManager manager) =>
        manager.GetType()
            .GetMethod(nameof(RearrangeBanners), BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(manager, null);

    /// <summary>
    /// The banner forms used here are never shown: the creation/disposal below does not create
    /// a window handle, so the test stays deterministic on any Windows machine.
    /// </summary>
    [Test]
    public void RearrangeBanners_DropsDisposedEntriesWithoutThrowing()
    {
        var liveBanner = new BannerForm();
        var disposedBanner = new BannerForm();
        disposedBanner.Dispose();

        var manager = new MicrophoneMuteBannerManager();
        var tracked = GetTrackedBanners(manager);
        tracked.Add("live-mic", liveBanner);
        tracked.Add("disposed-mic", disposedBanner);

        var act = () => RearrangeBanners(manager);

        act.Should().NotThrow("a stale tracked banner must be skipped, not dereferenced");
        GetTrackedBanners(manager).Should().NotContainKey("disposed-mic", "the stale reference must be cleared")
            .And.ContainKey("live-mic", "the live banner must stay tracked");
    }

    [Test]
    public void RearrangeBanners_DropsNullEntriesWithoutThrowing()
    {
        var liveBanner = new BannerForm();

        var manager = new MicrophoneMuteBannerManager();
        var tracked = GetTrackedBanners(manager);
        tracked.Add("live-mic", liveBanner);
        tracked.Add("null-mic", null);

        var act = () => RearrangeBanners(manager);

        act.Should().NotThrow("a null tracked entry must be skipped, not dereferenced");
        GetTrackedBanners(manager).Should().NotContainKey("null-mic", "the stale reference must be cleared")
            .And.ContainKey("live-mic", "the live banner must stay tracked");
    }

    [Test]
    public void TrackBanner_ClearsTrackedReferenceOnDispose()
    {
        var manager = new MicrophoneMuteBannerManager();

        // TrackBanner is private; drive it through reflection like the rear pass above.
        var banner = new BannerForm();
        manager.GetType()
            .GetMethod("TrackBanner", BindingFlags.Instance | BindingFlags.NonPublic)!
            .Invoke(manager, new object[] { "mic-id", banner });
        GetTrackedBanners(manager).Should().ContainKey("mic-id");

        banner.Dispose();

        GetTrackedBanners(manager).Should().NotContainKey("mic-id",
            "whatever disposes the form must clear the tracked reference (SOUNDSWITCH-40H)");
    }

    [Test]
    public void TrackBanner_KeepsEntryWhenAnotherBannerIsTrackedForSameId()
    {
        var manager = new MicrophoneMuteBannerManager();

        var first = new BannerForm();
        var second = new BannerForm();
        var track = manager.GetType()
            .GetMethod("TrackBanner", BindingFlags.Instance | BindingFlags.NonPublic)!;
        track.Invoke(manager, new object[] { "mic-id", first });
        track.Invoke(manager, new object[] { "mic-id", second });

        // Disposing the replaced banner must not drop the now-tracked one.
        first.Dispose();

        var tracked = GetTrackedBanners(manager);
        tracked.Should().ContainKey("mic-id");
        tracked["mic-id"].Should().BeSameAs(second, "disposing a replaced banner must not clear the new tracking");
    }

    [Test]
    public void TrackBanner_PullsSurvivorUpWhenATrackedBannerIsDisposedOutsideTheManager()
    {
        var manager = new MicrophoneMuteBannerManager();
        var track = manager.GetType()
            .GetMethod("TrackBanner", BindingFlags.Instance | BindingFlags.NonPublic)!;

        var first = new BannerForm();
        var second = new BannerForm();
        track.Invoke(manager, new object[] { "first-mic", first });
        track.Invoke(manager, new object[] { "second-mic", second });

        // Simulate the previous layout pass: the survivor sits below the first banner. The
        // survivor needs banner data to be positionable at all (UpdatePosition is a no-op
        // without it).
        second.GetType().GetField("_currentData", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(second, new BannerData { Position = new PositionTopLeft() });
        second.GetType().GetField("_currentOffset", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(second, first.Height + 10);

        first.Dispose();

        GetTrackedBanners(manager).Should().NotContainKey("first-mic",
            "disposing the banner outside the manager must clear its tracked reference");
        var secondOffset = (int) second.GetType().GetField("_currentOffset", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(second)!;
        secondOffset.Should().Be(0, "the surviving banner must move up into the vacated spot instead of leaving a gap");
    }

    [TearDown]
    public void TearDown()
    {
        // Banner forms register themselves in a static list on construction; dispose the
        // survivors so no test leaks a banner into another test. Snapshot first: disposing
        // removes from that list.
        foreach (var banner in GetAllBannerForms().ToArray())
            if (!banner.IsDisposed)
                banner.Dispose();

        static IEnumerable<BannerForm> GetAllBannerForms()
        {
            var field = typeof(BannerForm).GetField("ActiveBanners",
                BindingFlags.Static | BindingFlags.NonPublic);
            return field == null
                ? []
                : (List<BannerForm>) field.GetValue(null)!;
        }
    }
}
