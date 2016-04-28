using System.IO;
using System.Windows.Forms;
using SoundSwitch.Framework.Audio;

namespace SoundSwitch.Framework.NotificationManager.Notification.configuration
{
    public class NotificationConfiguration : INotificationConfiguration
    {
        public NotifyIcon Icon { get; set; }
        public Stream DefaultSound { get; set; }
        public CachedSound CustomSound { get; set; }
    }
}