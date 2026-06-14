using System.Text.RegularExpressions;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Services;

namespace SoundSwitch.Tests;

[TestFixture]
public class AppSoundLockManagerTests
{
    [Test]
    public void IsRegexOrGlobMatch_ShouldMatchInvalidRegexAsGlobForWindowTitle()
    {
        var matched = AppSoundLockManager.IsRegexOrGlobMatch("Sleepy browser tab", "*Sleepy*", RegexOptions.IgnoreCase);

        matched.Should().BeTrue();
    }

    [Test]
    public void IsRegexOrGlobMatch_ShouldMatchInvalidRegexAsGlobForProcessPattern()
    {
        var matched = AppSoundLockManager.IsRegexOrGlobMatch(@"C:\Apps\Sleepy\App.exe", @"*\Sleepy\*.exe", RegexOptions.IgnoreCase);

        matched.Should().BeTrue();
    }

    [Test]
    public void IsRegexOrGlobMatch_ShouldStillSupportRegexPatterns()
    {
        var matched = AppSoundLockManager.IsRegexOrGlobMatch("Window - YouTube", ".*YouTube.*", RegexOptions.IgnoreCase);

        matched.Should().BeTrue();
    }
}
