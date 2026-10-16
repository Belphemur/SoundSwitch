using System.Linq;
using System.Windows.Forms;

using FluentAssertions;
using NUnit.Framework;

using SoundSwitch.Framework.Banner.BannerPosition.Position;

namespace SoundSwitch.Tests;

[TestFixture]
public class BannerPositionTests
{
    [Test]
    public void PositionsUseTheSelectedScreensDesktopOrigin()
    {
        var screen = Screen.AllScreens.FirstOrDefault(candidate =>
            candidate.Bounds.Left != 0 || candidate.Bounds.Top != 0);
        if (screen == null)
            Assert.Ignore("Requires a display outside the desktop origin.");

        const int width = 120;
        const int height = 60;
        const int offset = 10;

        new PositionBottomCenter().GetScreenPosition(screen, height, width, offset).Y
            .Should().Be(screen.Bounds.Bottom - height - 60 - offset);
        new PositionBottomCenter().GetScreenPosition(screen, height, width, offset).X
            .Should().Be(screen.Bounds.Left + (screen.Bounds.Width - width) / 2);
        new PositionTopRight().GetScreenPosition(screen, height, width, offset).X
            .Should().Be(screen.Bounds.Right - width - 50);
        new PositionCenter().GetScreenPosition(screen, height, width, offset).Y
            .Should().Be(screen.Bounds.Top + (screen.Bounds.Height - height) / 2);

        PositionCustom.ResolvePosition(screen, System.Drawing.Point.Empty, height, width)
            .Should().Be(new System.Drawing.Point(
                screen.Bounds.Left + (screen.Bounds.Width - width) / 2,
                screen.Bounds.Top + (screen.Bounds.Height - height) / 2));

        var savedPosition = new System.Drawing.Point(screen.Bounds.Left + 20, screen.Bounds.Top + 20);
        PositionCustom.ResolvePosition(screen, savedPosition, height, width)
            .Should().Be(savedPosition);
    }
}
