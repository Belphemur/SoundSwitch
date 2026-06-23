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

    [TestCase("7.1.0.229925", "7.1.29925")]
    [TestCase("1.2.3.456", "1.2.456")]
    [TestCase("1.2.3.100001", "1.2.1")]
    [TestCase("1.2.3", "1.2.3")]
    public void TestNightlyVersionParsing(string rawVersion, string expectedVersion)
    {
        var parts = rawVersion.Split('.');
        SemanticVersion parsed;
        if (parts.Length >= 4 && int.TryParse(parts[3], out var revision))
        {
            var patch = revision % 100_000;
            parsed = new SemanticVersion(int.Parse(parts[0]), int.Parse(parts[1]), patch);
        }
        else
        {
            var truncated = string.Join(".", parts.Take(3));
            parsed = SemanticVersion.Parse(truncated);
        }
        parsed.Should().Be(SemanticVersion.Parse(expectedVersion));
    }
}
