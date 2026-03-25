using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.Banner;

public class PositionBottomRight : IPosition
{
    public BannerPosition TypeEnum => BannerPosition.BottomRight;
    public string Label => "Bottom Right";

    public Point GetScreenPosition(Screen screen, int height, int width, int offset, Point? customPosition = null) =>
        new(APosition.PositionRight(screen, width), APosition.PositionBottom(screen, height, offset));
}
