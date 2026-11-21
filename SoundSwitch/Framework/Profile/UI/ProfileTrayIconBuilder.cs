using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using SoundSwitch.Audio.Manager.Interop.Enum;

using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.Profile.UI;

public class ProfileTrayIconBuilder(IAppInfrastructure infrastructure)
{
    // Deferred access on purpose (Kody review on PR #2622): the profile manager's DI
    // factory takes the tray icon for its ShowError delegate, so dereferencing it from
    // the tray icon constructor re-entered Lazy<ProfileManager> mid-construction and
    // crashed startup. The façade's lazy getters are only hit here, when the menu is
    // actually built — after the bootstrapper constructed both managers.
    private ProfileManager ProfileManager => infrastructure.ProfileManager;

    private IAudioDeviceLister AudioDeviceLister => infrastructure.AudioDeviceLister;

    /// <summary>
    /// Get the menu items for profile that needs to be shown in the menu
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ToolStripMenuItem> GetMenuItems()
    {
        return (ProfileManager?.Profiles ?? new Profile[0])
            .Where(profile => profile.Triggers.Any(trigger => trigger.Type == TriggerFactory.Enum.TrayMenu))
            .OrderBy(profile => profile.Name)
            .Select(BuildMenuItem);
    }

    private ProfileToolStripMenuItem BuildMenuItem(Profile profile)
    {
        Image image = null;
        try
        {
            var appTrigger = profile.Triggers.FirstOrDefault(trigger => trigger.ApplicationPath != null);
            if (appTrigger != null)
            {
                using var iconHandle = IconExtractor.Extract(appTrigger.ApplicationPath, 0, false);
                image = iconHandle.ToBitmap();
            }
        }
        catch (Exception)
        {
            // ignored
        }

        foreach (var wrapper in profile.Devices)
        {
            if (image != null)
                break;

            try
            {
                var device = AudioDeviceLister.GetDevices(wrapper.DeviceInfo.Type, EDeviceState.Active).FirstOrDefault(info => info.Equals(wrapper.DeviceInfo));
                using var iconHandle = device?.SmallIcon;
                image = iconHandle?.ToBitmap();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        image ??= Resources.profile_menu_icon;

        return new ProfileToolStripMenuItem(profile, image, profileClicked => ProfileManager.SwitchAudio(profileClicked));
    }
}
