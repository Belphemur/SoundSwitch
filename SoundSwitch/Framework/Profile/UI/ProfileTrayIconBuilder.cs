using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.Profile.UI
{
    public class ProfileTrayIconBuilder
    {
        private readonly IAudioDeviceLister _audioDeviceLister;
        private readonly ProfileManager _profileManager;

        public ProfileTrayIconBuilder(IAudioDeviceLister audioDeviceLister, ProfileManager profileManager)
        {
            _audioDeviceLister = audioDeviceLister;
            _profileManager = profileManager;
        }

        /// <summary>
        /// Get the menu items for profile that needs to be shown in the menu
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ToolStripMenuItem> GetMenuItems()
        {
            return _profileManager.Profiles
                                  .Where(profile => profile.Triggers.Any(trigger => trigger.Type == TriggerFactory.Enum.TrayMenu))
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
                    image = IconExtractor.Extract(appTrigger.ApplicationPath, 0, false).ToBitmap();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            if (image == null)
            {
                try
                {
                    var playback = _audioDeviceLister.PlaybackDevices.FirstOrDefault(info => info.Equals(profile.Playback));
                    image = playback?.SmallIcon.ToBitmap();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return new ProfileToolStripMenuItem(profile, image, profileClicked => _profileManager.SwitchAudio(profileClicked));
        }
    }
}