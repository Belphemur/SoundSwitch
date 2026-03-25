using System.Collections.Generic;
using System.Linq;

namespace SoundSwitch.Banner;

/// <summary>
/// Factory to provide banner position implementations.
/// </summary>
public class BannerPositionFactory
{
    private static readonly Dictionary<BannerPosition, IPosition> Positions = new()
    {
        { BannerPosition.TopLeft, new PositionTopLeft() },
        { BannerPosition.TopCenter, new PositionTopCenter() },
        { BannerPosition.TopRight, new PositionTopRight() },
        { BannerPosition.CenterLeft, new PositionCenterLeft() },
        { BannerPosition.Center, new PositionCenter() },
        { BannerPosition.CenterRight, new PositionCenterRight() },
        { BannerPosition.BottomLeft, new PositionBottomLeft() },
        { BannerPosition.BottomCenter, new PositionBottomCenter() },
        { BannerPosition.BottomRight, new PositionBottomRight() },
        { BannerPosition.Custom, new PositionCustom() }
    };

    /// <summary>
    /// Gets the position implementation for the specified enum value.
    /// </summary>
    public IPosition Get(BannerPosition position) => Positions[position];

    /// <summary>
    /// Gets all available position implementations.
    /// </summary>
    public IEnumerable<IPosition> GetAll() => Positions.Values;
}
