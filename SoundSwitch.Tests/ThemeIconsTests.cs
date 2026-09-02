using System;
using FluentAssertions;
using NUnit.Framework;
using SoundSwitch.Framework.TrayIcon;

namespace SoundSwitch.Tests;

[TestFixture]
public class ThemeIconsTests
{
    [Test]
    public void Speaker_IconDiffersBetweenLightAndDarkTaskbar()
    {
        var light = ThemeIcons.GetIcon(IconKind.Speaker, false);
        var dark = ThemeIcons.GetIcon(IconKind.Speaker, true);

        light.Should().NotBeNull();
        dark.Should().NotBeNull();
        light.Should().NotBeSameAs(dark);
    }

    [Test]
    public void Speaker_And_Headphone_IconsDiffer()
    {
        var speaker = ThemeIcons.GetIcon(IconKind.Speaker, false);
        var headphone = ThemeIcons.GetIcon(IconKind.Headphone, false);

        speaker.Should().NotBeNull();
        headphone.Should().NotBeNull();
        speaker.Should().NotBeSameAs(headphone);
    }

    [Test]
    public void AllKinds_LoadNonNullIcons()
    {
        foreach (IconKind kind in Enum.GetValues(typeof(IconKind)))
        {
            ThemeIcons.GetIcon(kind, false).Should().NotBeNull();
            ThemeIcons.GetIcon(kind, true).Should().NotBeNull();
        }
    }
}
