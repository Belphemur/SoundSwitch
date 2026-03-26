using System.Drawing;
using System;

namespace SoundSwitch.Banner;

/// <summary>
/// Represents a request to show a banner notification.
/// </summary>
public record BannerRequest
{
    /// <summary>
    /// Gets/sets the title of the banner
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Gets/sets the text of the banner
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Duration the banner stays on screen.
    /// </summary>
    public TimeSpan? Ttl { get; init; }

    /// <summary>
    /// Gets/sets an optional image for the banner.
    /// </summary>
    public Image? Image { get; init; }

    /// <summary>
    /// Path to a sound file to play (optional).
    /// </summary>
    public string? SoundPath { get; init; }

    /// <summary>
    /// The device ID to play the sound on (optional).
    /// </summary>
    public string? PlaybackDeviceId { get; init; }

    /// <summary>
    /// Set the priority of the notification.
    /// Higher priority replaces current notification.
    /// </summary>
    public int Priority { get; init; } = -1;

    /// <summary>
    /// When enabled, displays the banner in compact mode.
    /// </summary>
    public bool CompactMode { get; init; }

    /// <summary>
    /// When enabled, allow custom positioning via mouse.
    /// </summary>
    public bool CustomPositionMode { get; init; }

    /// <summary>
    /// Gets/sets the target screen for this banner.
    /// </summary>
    public ShowOnScreen? Screen { get; init; }

    /// <summary>
    /// Callback triggered when the banner is clicked.
    /// </summary>
    public EventHandler? OnClick { get; init; }
}
