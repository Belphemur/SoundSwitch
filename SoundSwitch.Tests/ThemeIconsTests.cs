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
        using var light = ThemeIcons.GetIcon(IconKind.Speaker, false).Acquire();
        using var dark = ThemeIcons.GetIcon(IconKind.Speaker, true).Acquire();

        light.Icon.Should().NotBeNull();
        dark.Icon.Should().NotBeNull();
        light.Icon.Should().NotBeSameAs(dark.Icon);
    }

    [Test]
    public void Speaker_And_Headphone_IconsDiffer()
    {
        using var speaker = ThemeIcons.GetIcon(IconKind.Speaker, false).Acquire();
        using var headphone = ThemeIcons.GetIcon(IconKind.Headphone, false).Acquire();

        speaker.Icon.Should().NotBeNull();
        headphone.Icon.Should().NotBeNull();
        speaker.Icon.Should().NotBeSameAs(headphone.Icon);
    }

    [Test]
    public void AllKinds_LoadNonNullIcons()
    {
        foreach (IconKind kind in Enum.GetValues(typeof(IconKind)))
        {
            using var light = ThemeIcons.GetIcon(kind, false).Acquire();
            using var dark = ThemeIcons.GetIcon(kind, true).Acquire();
            light.Icon.Should().NotBeNull();
            dark.Icon.Should().NotBeNull();
        }
    }
}