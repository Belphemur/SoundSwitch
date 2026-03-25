using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.Banner;

public class PositionCustom : IPosition
{
    public BannerPosition TypeEnum => BannerPosition.Custom;
    public string Label => "Custom";

    public Point GetScreenPosition(Screen screen, int height, int width, int offset, Point? customPosition = null) =>
        customPosition ?? Point.Empty;
}
