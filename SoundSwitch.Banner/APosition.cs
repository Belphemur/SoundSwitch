using System.Windows.Forms;

namespace SoundSwitch.Banner;

public static class APosition
{
    public static int PositionTop(Screen screen, int offset) => screen.Bounds.Y + 60 + offset;
    public static int PositionLeft(Screen screen) => screen.Bounds.X + 50;
    public static int PositionBottom(Screen screen, int height, int offset) => screen.Bounds.Height - height - PositionTop(screen, offset);
    public static int PositionRight(Screen screen, int width) => screen.Bounds.Width - width - PositionLeft(screen);
    public static int PositionCenterX(Screen screen, int width) => (screen.Bounds.Width - width) / 2;
    public static int PositionCenterY(Screen screen, int height) => (screen.Bounds.Height - height) / 2;
}
