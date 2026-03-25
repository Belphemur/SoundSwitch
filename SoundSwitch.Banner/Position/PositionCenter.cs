using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.Banner;

public class PositionCenter : IPosition
{
    public BannerPosition TypeEnum => BannerPosition.Center;
    public string Label => "Center";

    public Point GetScreenPosition(Screen screen, int height, int width, int offset, Point? customPosition = null) =>
        new(APosition.PositionCenterX(screen, width), APosition.PositionCenterY(screen, height));
}
