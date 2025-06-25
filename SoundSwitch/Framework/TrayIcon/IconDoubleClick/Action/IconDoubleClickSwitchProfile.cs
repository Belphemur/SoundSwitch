
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.IconDoubleClick.Action;

/// <summary>
/// Implementation for the "Switch Profile" double-click action.
/// When the user double-clicks the system tray icon, this action cycles through the configured audio profiles.
/// This allows users to quickly switch between different audio device configurations and setups.
/// </summary>
internal class IconDoubleClickSwitchProfile : IIconDoubleClick
{
    /// <summary>
    /// Gets the enum type that this implementation corresponds to.
    /// </summary>
    public IconDoubleClickEnum TypeEnum => IconDoubleClickEnum.SwitchProfile;

    /// <summary>
    /// Gets the localized label for this action as displayed in the user interface.
    /// </summary>
    public string Label => SettingsStrings.switchProfile;
}