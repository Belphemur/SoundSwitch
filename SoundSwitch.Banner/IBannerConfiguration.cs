using System.Drawing;
using System;

namespace SoundSwitch.Banner;

/// <summary>
/// Configuration provider for the banner service.
/// </summary>
public interface IBannerConfiguration
{
    /// <summary>
    /// Gets the opacity of the banner (0-100).
    /// </summary>
    int Opacity { get; }

    /// <summary>
    /// Gets how long the banner should stay visible.
    /// </summary>
    TimeSpan Ttl { get; }

    /// <summary>
    /// Gets the maximum number of concurrent banners.
    /// </summary>
    int MaxConcurrentBanners { get; }

    /// <summary>
    /// Gets which screen the banner should be shown on.
    /// </summary>
    ShowOnScreen ShowOn { get; }

    /// <summary>
    /// Gets the custom position for the banner if applicable.
    /// </summary>
    Point? CustomPosition { get; }
}
