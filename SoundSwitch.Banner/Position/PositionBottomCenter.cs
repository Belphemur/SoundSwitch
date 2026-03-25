using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.Banner;

public class PositionBottomCenter : IPosition
{
    public BannerPosition TypeEnum => BannerPosition.BottomCenter;
    public string Label => "Bottom Center";

    public Point GetScreenPosition(Screen screen, int height, int width, int offset, Point? customPosition = null) =>
        new(APosition.PositionCenterX(screen, width), APosition.PositionBottom(screen, height, offset));
}
