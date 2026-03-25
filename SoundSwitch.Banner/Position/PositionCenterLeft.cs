using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.Banner;

public class PositionCenterLeft : IPosition
{
    public BannerPosition TypeEnum => BannerPosition.CenterLeft;
    public string Label => "Center Left";

    public Point GetScreenPosition(Screen screen, int height, int width, int offset, Point? customPosition = null) =>
        new(APosition.PositionLeft(screen), APosition.PositionCenterY(screen, height));
}
