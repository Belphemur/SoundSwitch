using System.Linq;

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

    [TestCase("7.1.0.229925", "7.1.0")]
    [TestCase("1.2.3.456", "1.2.3")]
    [TestCase("1.2.3", "1.2.3")]
    public void TestNightlyVersionTruncation(string rawVersion, string expectedVersion)
    {
        var parts = rawVersion.Split('.');
        var truncated = string.Join(".", parts.Take(3));
        var parsed = SemanticVersion.Parse(truncated);
        parsed.Should().Be(SemanticVersion.Parse(expectedVersion));
    }
}
