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
    [TestCase("", @"C:\Apps\Spotify\Spotify.exe", false)]
    [TestCase(null, @"C:\Apps\Spotify\Spotify.exe", false)]
    [TestCase(@"C:\Apps\Spotify\Spotify.exe", null, false)]
    public void SameApp_MatchesVolumeRelatively(string left, string right, bool expected)
    {
        AppRuleIdentityMatcher.SameApp(left, right).Should().Be(expected);
    }

    [Test]
    public void SameApp_IsCaseInsensitive()
    {
        AppRuleIdentityMatcher.SameApp(
            @"\Device\HarddiskVolume3\Apps\SPOTIFY\Spotify.EXE",
            @"c:\apps\spotify\spotify.exe").Should().BeTrue();
    }
}
