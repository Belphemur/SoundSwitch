using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Services;

namespace SoundSwitch.Tests;

/// <summary>
/// Identity matching between NT-device store paths and drive-letter process paths — the
/// volume-relative suffix comparison the picker and the assignment reconciler share.
/// </summary>
[TestFixture]
public class AppRuleIdentityMatcherTests
{
    [TestCase(@"\Device\HarddiskVolume3\Apps\Spotify\Spotify.exe", @"C:\Apps\Spotify\Spotify.exe", true)]
    [TestCase(@"\Device\HarddiskVolume7\Program Files\WindowsApps\Spotify.exe", @"D:\Program Files\WindowsApps\Spotify.exe", true)]
    [TestCase(@"\Device\HarddiskVolume3\Apps\Spotify\Spotify.exe", @"C:\Other\Spotify\Spotify.exe", false)]
    [TestCase(@"C:\Apps\Spotify\Spotify.exe", @"C:\Apps\Spotify\Spotify.exe", true)]
    [TestCase(@"C:\Apps\Spotify\Spotify.exe", "Different.exe", false)]
    [TestCase(@"Spotify.exe", @"C:\Any\Where\Spotify.exe", true)]
    // A suffix that continues a longer directory/component name is a different app:
    [TestCase(@"\Device\HarddiskVolume2\Tools\MyApps\Setup.exe", @"C:\Apps\Setup.exe", false)]
    [TestCase(@"C:\Tools\MyApps\Setup.exe", @"\Device\HarddiskVolume2\Apps\Setup.exe", false)]
    [TestCase(@"Spotify.exe", @"C:\Any\Where\MySpotify.exe", false)]
    [TestCase("", @"C:\Apps\Spotify\Spotify.exe", false)]
    [TestCase(null, @"C:\Apps\Spotify\Spotify.exe", false)]
    [TestCase(@"C:\Apps\Spotify\Spotify.exe", null, false)]
    public void SameApp_MatchesVolumeRelatively(string left, string right, bool expected)
    {
        AppRuleIdentityMatcher.SameApp(left, right).Should().Be(expected);
    }

    [Test]
    public void EndsWithAtComponentBoundary_RequiresSeparator()
    {
        AppRuleIdentityMatcher.EndsWithAtComponentBoundary(@"C:\Tools\MyApps\Setup.exe", @"Apps\Setup.exe").Should().BeFalse();
        AppRuleIdentityMatcher.EndsWithAtComponentBoundary(@"C:\Tools\Apps\Setup.exe", @"Apps\Setup.exe").Should().BeTrue();
        AppRuleIdentityMatcher.EndsWithAtComponentBoundary(@"MySpotify.exe", "Spotify.exe").Should().BeFalse();
        AppRuleIdentityMatcher.EndsWithAtComponentBoundary("Spotify.exe", "Spotify.exe").Should().BeTrue("the whole path is the final component");
    }

    [Test]
    public void SameApp_IsCaseInsensitive()
    {
        AppRuleIdentityMatcher.SameApp(
            @"\Device\HarddiskVolume3\Apps\SPOTIFY\Spotify.EXE",
            @"c:\apps\spotify\spotify.exe").Should().BeTrue();
    }
}
