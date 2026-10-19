using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using FluentAssertions;
using NUnit.Framework;

using SoundSwitch.Framework.Banner.BannerPosition.Position;

namespace SoundSwitch.Tests;

[TestFixture]
public class BannerPositionTests
{
    private const int WIDTH = 120;
    private const int HEIGHT = 60;
    private const int OFFSET = 10;

    /// <summary>
    /// A display that does not start at the desktop origin, which is the layout the position maths
    /// has to get right: it used to mix screen-relative offsets with desktop-absolute coordinates,
    /// which placed banners on the wrong monitor.
    /// </summary>
    private static readonly Rectangle OffsetBounds = new(1920, -600, 2560, 1440);

    [Test]
    public void TopPositionsUseTheGivenBoundsEdges()
    {
        new PositionTopLeft().GetPosition(OffsetBounds, HEIGHT, WIDTH, OFFSET)
            .Should().Be(new Point(OffsetBounds.Left + 50, OffsetBounds.Top + 60 + OFFSET));
        new PositionTopCenter().GetPosition(OffsetBounds, HEIGHT, WIDTH, OFFSET)
            .Should().Be(new Point(OffsetBounds.Left + (OffsetBounds.Width - WIDTH) / 2, OffsetBounds.Top + 60 + OFFSET));
        new PositionTopRight().GetPosition(OffsetBounds, HEIGHT, WIDTH, OFFSET)
            .Should().Be(new Point(OffsetBounds.Right - WIDTH - 50, OffsetBounds.Top + 60 + OFFSET));
    }

    [Test]
    public void CenterPositionsUseTheGivenBoundsCenter()
    {
        new PositionCenterLeft().GetPosition(OffsetBounds, HEIGHT, WIDTH, OFFSET)
            .Should().Be(new Point(OffsetBounds.Left + 50, OffsetBounds.Top + (OffsetBounds.Height - HEIGHT) / 2));
        new PositionCenter().GetPosition(OffsetBounds, HEIGHT, WIDTH, OFFSET)
            .Should().Be(new Point(OffsetBounds.Left + (OffsetBounds.Width - WIDTH) / 2,
                OffsetBounds.Top + (OffsetBounds.Height - HEIGHT) / 2));
        new PositionCenterRight().GetPosition(OffsetBounds, HEIGHT, WIDTH, OFFSET)
            .Should().Be(new Point(OffsetBounds.Right - WIDTH - 50, OffsetBounds.Top + (OffsetBounds.Height - HEIGHT) / 2));
    }

    [Test]
    public void BottomPositionsUseTheGivenBoundsBottomEdge()
    {
        new PositionBottomLeft().GetPosition(OffsetBounds, HEIGHT, WIDTH, OFFSET)
            .Should().Be(new Point(OffsetBounds.Left + 50, OffsetBounds.Bottom - HEIGHT - 60 - OFFSET));
        new PositionBottomCenter().GetPosition(OffsetBounds, HEIGHT, WIDTH, OFFSET)
            .Should().Be(new Point(OffsetBounds.Left + (OffsetBounds.Width - WIDTH) / 2,
                OffsetBounds.Bottom - HEIGHT - 60 - OFFSET));
        new PositionBottomRight().GetPosition(OffsetBounds, HEIGHT, WIDTH, OFFSET)
            .Should().Be(new Point(OffsetBounds.Right - WIDTH - 50, OffsetBounds.Bottom - HEIGHT - 60 - OFFSET));
    }

    [Test]
    public void ScreenOverloadsMatchTheBoundsOverloads()
    {
        var screen = Screen.AllScreens[0];

        new PositionBottomCenter().GetScreenPosition(screen, HEIGHT, WIDTH, OFFSET)
            .Should().Be(new PositionBottomCenter().GetPosition(screen.Bounds, HEIGHT, WIDTH, OFFSET));
    }

    [Test]
    public void CustomPositionWithNoSavedPointIsCentredOnTheSelectedScreen()
    {
        // The banner was never dragged, so it must not end up pinned to the desktop origin.
        var screen = Screen.AllScreens[0];

        PositionCustom.ResolvePosition(screen, Point.Empty, false, HEIGHT, WIDTH)
            .Should().Be(new Point(
                screen.Bounds.Left + (screen.Bounds.Width - WIDTH) / 2,
                screen.Bounds.Top + (screen.Bounds.Height - HEIGHT) / 2));
    }

    [Test]
    public void CustomPositionKeepsASavedPointAtTheDesktopOrigin()
    {
        // (0,0) is both the default value and the reachable top-left corner of the primary
        // display, so the saved point cannot double as the "never placed" marker. The same point
        // must therefore produce two different results depending on the flag.
        var primaryBounds = new Rectangle(0, 0, 1920, 1080);

        PositionCustom.ResolvePosition(primaryBounds, Point.Empty, true, HEIGHT, WIDTH)
            .Should().Be(Point.Empty);

        PositionCustom.ResolvePosition(primaryBounds, Point.Empty, false, HEIGHT, WIDTH)
            .Should().Be(new Point(900, 510));
    }

    [Test]
    public void CustomPositionKeepsASavedPointInsideItsOwnScreen()
    {
        var savedPosition = new Point(OffsetBounds.Left + 20, OffsetBounds.Top + 20);

        PositionCustom.ResolvePosition(OffsetBounds, savedPosition, true, HEIGHT, WIDTH)
            .Should().Be(savedPosition);
    }

    [Test]
    public void CustomPositionIsClampedBackIntoItsOwnScreen()
    {
        // Past the bottom-right edge: pulled back in, without drifting to the screen centre.
        PositionCustom.ResolvePosition(OffsetBounds, new Point(OffsetBounds.Right, OffsetBounds.Bottom), true, HEIGHT, WIDTH)
            .Should().Be(new Point(OffsetBounds.Right - WIDTH, OffsetBounds.Bottom - HEIGHT));

        // Before the top-left edge of a screen that is not at the desktop origin.
        PositionCustom.ResolvePosition(OffsetBounds, new Point(-1000, -2000), true, HEIGHT, WIDTH)
            .Should().Be(new Point(OffsetBounds.Left, OffsetBounds.Top));
    }

    /// <summary>
    /// Integration check against a real display that is not at the desktop origin. Kept separate
    /// from the bounds tests above so the geometry stays covered on single-monitor machines.
    /// </summary>
    [Test]
    public void PositionsMatchTheConnectedDisplayBounds()
    {
        var screen = Screen.AllScreens.FirstOrDefault(candidate =>
            candidate.Bounds.Left != 0 || candidate.Bounds.Top != 0);
        if (screen == null)
            Assert.Ignore("Requires a display outside the desktop origin.");

        var bounds = screen.Bounds;
        new PositionTopRight().GetPosition(bounds, HEIGHT, WIDTH, OFFSET)
            .Should().Be(new Point(bounds.Right - WIDTH - 50, bounds.Top + 60 + OFFSET));
        new PositionBottomCenter().GetPosition(bounds, HEIGHT, WIDTH, OFFSET)
            .Should().Be(new Point(bounds.Left + (bounds.Width - WIDTH) / 2,
                bounds.Bottom - HEIGHT - 60 - OFFSET));
    }
}