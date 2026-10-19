using FluentAssertions;
using NUnit.Framework;

using SoundSwitch.Framework.Banner;

namespace SoundSwitch.Tests;

[TestFixture]
public class BannerMonitorTests
{
    [Test]
    public void PinnedDeviceMatchesTheConfiguredMonitor()
    {
        BannerForm.IsPinnedDevice(@"\\.\DISPLAY1", @"\\.\DISPLAY2").Should().BeFalse();
        BannerForm.IsPinnedDevice(@"\\.\DISPLAY2", @"\\.\DISPLAY2").Should().BeTrue();
    }

    [Test]
    public void PinnedDeviceMatchesIgnoringCase()
    {
        // Displays are identified by name, not by handle, and the stored name is not guaranteed to
        // keep its casing across a configuration round-trip.
        BannerForm.IsPinnedDevice(@"\\.\DISPLAY2", @"\\.\display2").Should().BeTrue();
        BannerForm.IsPinnedDevice(@"\\.\display2", @"\\.\DISPLAY2").Should().BeTrue();
    }

    [TestCase(null)]
    [TestCase("")]
    public void NoPinnedDeviceNeverMatches(string pinnedDeviceName)
    {
        // Unpinned is the default, so every display must fall through to the cursor/primary rule.
        BannerForm.IsPinnedDevice(@"\\.\DISPLAY1", pinnedDeviceName).Should().BeFalse();
    }

    [Test]
    public void DisconnectedPinnedDeviceNeverMatches()
    {
        // The monitor was unplugged: the banner must fall back rather than disappear.
        BannerForm.IsPinnedDevice(@"\\.\DISPLAY1", @"\\.\DISPLAY9").Should().BeFalse();
    }
}