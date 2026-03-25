using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.Banner;

public class PositionTopLeft : IPosition
{
    public BannerPosition TypeEnum => BannerPosition.TopLeft;
    public string Label => "Top Left";

    public Point GetScreenPosition(Screen screen, int height, int width, int offset, Point? customPosition = null) =>
        new(APosition.PositionLeft(screen), APosition.PositionTop(screen, offset));
}
