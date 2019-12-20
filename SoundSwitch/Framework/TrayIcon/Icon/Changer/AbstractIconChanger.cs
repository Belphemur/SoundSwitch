using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public abstract class AbstractIconChanger : IIconChanger
    {
        public abstract IconChangerFactory.ActionEnum TypeEnum { get; }
        public abstract string Label { get; }
        public abstract bool NeedsToChangeIcon(DeviceInfo deviceInfo);
        public abstract void ChangeIcon(Util.TrayIcon trayIcon);

        public void ChangeIcon(Util.TrayIcon trayIcon, DeviceFullInfo deviceInfo)
        {
            if (!NeedsToChangeIcon(deviceInfo)) return;
            trayIcon.ReplaceIcon(deviceInfo.SmallIcon);
        }
    }
}