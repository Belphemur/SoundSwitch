using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.Banner;

public class PositionBottomLeft : IPosition
{
    public BannerPosition TypeEnum => BannerPosition.BottomLeft;
    public string Label => "Bottom Left";

    public Point GetScreenPosition(Screen screen, int height, int width, int offset, Point? customPosition = null) =>
        new(APosition.PositionLeft(screen), APosition.PositionBottom(screen, height, offset));
}
