using FluentAssertions;
using NuGet.Versioning;
using NUnit.Framework;

namespace SoundSwitch.Tests;
[TestFixture]
public class VersionTest
{
    [Test]
    public void TestSemanticVersionBetaSmallerThanRelease()
    {
        var beta = SemanticVersion.Parse("1.0.0-beta.1");
        var release = SemanticVersion.Parse("1.0.0");
        beta.Should().BeLessThan(release);
    }
}