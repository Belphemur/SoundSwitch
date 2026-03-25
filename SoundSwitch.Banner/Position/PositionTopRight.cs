using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.Banner;

public class PositionTopRight : IPosition
{
    public BannerPosition TypeEnum => BannerPosition.TopRight;
    public string Label => "Top Right";

    public Point GetScreenPosition(Screen screen, int height, int width, int offset, Point? customPosition = null) =>
        new(APosition.PositionRight(screen, width), APosition.PositionTop(screen, offset));
}
