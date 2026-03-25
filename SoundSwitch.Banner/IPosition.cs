using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.Banner;

/// <summary>
/// Interface for banner position implementations.
/// </summary>
public interface IPosition : IEnumImpl<BannerPosition>
{
    /// <summary>
    /// Gets the screen position for the banner.
    /// </summary>
    Point GetScreenPosition(Screen screen, int height, int width, int offset, Point? customPosition = null);
}
