
using System.Linq;
using Serilog;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Localization;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.TrayIcon.IconDoubleClick.Action;

/// <summary>
/// Implementation for the "Switch Profile" double-click action.
/// When the user double-clicks the system tray icon, this action cycles through the configured audio profiles.
/// This allows users to quickly switch between different audio device configurations and setups.
/// </summary>
internal class IconDoubleClickSwitchProfile : IIconDoubleClick
{
    private Profile.Profile _activeTrayProfile;

    /// <summary>
    /// Gets the enum type that this implementation corresponds to.
    /// </summary>
    public IconDoubleClickEnum TypeEnum => IconDoubleClickEnum.SwitchProfile;

    /// <summary>
    /// Gets the localized label for this action as displayed in the user interface.
    /// </summary>
    public string Label => SettingsStrings.switchProfile;

    /// <summary>
    /// Executes the switch profile action by cycling through profiles with tray menu triggers.
    /// </summary>
    /// <param name="trayIcon">The TrayIcon instance (not used for this action)</param>
    public void Execute(UI.Component.TrayIcon trayIcon)
    {
        // Check if there are any profiles with tray menu triggers
        var profiles = AppModel.Instance.ProfileManager.Profiles
            .Where(profile => profile.Triggers.Any(trigger => trigger.Type == TriggerFactory.Enum.TrayMenu))
            .ToList();

        if (!profiles.Any())
        {
            Log.Warning("No profiles available for tray icon double click");
            return;
        }

        var nextProfile = _activeTrayProfile != null && profiles.Contains(_activeTrayProfile)
            ? profiles[(profiles.IndexOf(_activeTrayProfile) + 1) % profiles.Count]
            : profiles.First();

        _activeTrayProfile = nextProfile;

        Log.Information("Switching to profile {profile}", nextProfile.Name);
        AppModel.Instance.ProfileManager.SwitchAudio(nextProfile);
    }
}