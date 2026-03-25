using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.Banner;

public class PositionCenterRight : IPosition
{
    public BannerPosition TypeEnum => BannerPosition.CenterRight;
    public string Label => "Center Right";

    public Point GetScreenPosition(Screen screen, int height, int width, int offset, Point? customPosition = null) =>
        new(APosition.PositionRight(screen, width), APosition.PositionCenterY(screen, height));
}
