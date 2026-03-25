namespace SoundSwitch.Banner;

/// <summary>
/// Defines which screen a banner should be shown on.
/// </summary>
public enum ShowOnScreen
{
    /// <summary>
    /// Show on the primary screen.
    /// </summary>
    PrimaryScreen,

    /// <summary>
    /// Show on the active screen (where the mouse cursor is).
    /// </summary>
    ActiveScreen,

    /// <summary>
    /// Show on the screen where the mouse cursor is (Manual/Follow Cursor).
    /// </summary>
    FollowCursor
}
