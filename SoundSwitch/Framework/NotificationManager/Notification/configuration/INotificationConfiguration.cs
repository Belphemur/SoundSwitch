using System.IO;
using System.Windows.Forms;
using SoundSwitch.Framework.Audio;

namespace SoundSwitch.Framework.NotificationManager.Notification.Configuration
{
    public interface INotificationConfiguration
    {
        NotifyIcon Icon { get; set; }
        Stream DefaultSound { get; set; }
        CachedSound CustomSound { get; set; }
    }
}