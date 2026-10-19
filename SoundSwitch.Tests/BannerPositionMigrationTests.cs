using System.Drawing;

using FluentAssertions;
using NUnit.Framework;

using SoundSwitch.Framework.Configuration;

namespace SoundSwitch.Tests;

/// <summary>
/// Tests the migration that derives <see cref="SoundSwitchConfiguration.HasCustomBannerPosition"/>
/// for configurations written before the flag existed.
/// </summary>
/// <remarks>
/// Without it the flag loads as false on every upgraded install, and the banner drops back to the
/// centre of the screen instead of the placement the user saved.
/// </remarks>
[TestFixture]
public class BannerPositionMigrationTests
{
    [Test]
    public void Migrate_LegacyCustomPosition_DerivesTheFlag()
    {
        var config = new SoundSwitchConfiguration { CustomBannerPosition = new Point(640, 480) };

        config.Migrate().Should().BeTrue();

        config.HasCustomBannerPosition.Should().BeTrue(
            "a point stored before the flag existed is a placement the user chose");
        config.CustomBannerPosition.Should().Be(new Point(640, 480), "the position itself is untouched");
    }

    [Test]
    public void Migrate_EmptyCustomPosition_LeavesTheFlagUnset()
    {
        var config = new SoundSwitchConfiguration { CustomBannerPosition = Point.Empty };

        config.Migrate();

        config.HasCustomBannerPosition.Should().BeFalse(
            "the default point is not a choice, so the banner stays centred");
    }

    [Test]
    public void Migrate_RunsOnlyOnce()
    {
        var config = new SoundSwitchConfiguration { CustomBannerPosition = new Point(640, 480) };
        config.Migrate();
        config.HasCustomBannerPosition.Should().BeTrue();

        // Simulate a later run where the point happens to be back at the origin; the recorded
        // migration must not re-derive the flag from it.
        config.CustomBannerPosition = Point.Empty;
        config.Migrate().Should().BeFalse("every migration has already run");

        config.HasCustomBannerPosition.Should().BeTrue();
    }

    [Test]
    public void Migrate_IsRecordedSoItDoesNotRepeat()
    {
        var config = new SoundSwitchConfiguration { CustomBannerPosition = new Point(640, 480) };

        config.Migrate();

        config.MigratedFields.Should().Contain(nameof(SoundSwitchConfiguration.HasCustomBannerPosition));
    }
}