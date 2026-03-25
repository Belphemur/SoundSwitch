using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.Banner;

public class PositionTopCenter : IPosition
{
    public BannerPosition TypeEnum => BannerPosition.TopCenter;
    public string Label => "Top Center";

    public Point GetScreenPosition(Screen screen, int height, int width, int offset, Point? customPosition = null) =>
        new(APosition.PositionCenterX(screen, width), APosition.PositionTop(screen, offset));
}
