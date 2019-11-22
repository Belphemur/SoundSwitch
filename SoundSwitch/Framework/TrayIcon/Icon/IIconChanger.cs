using SoundSwitch.Framework.Audio.Device;
using SoundSwitch.Framework.Factory;

namespace SoundSwitch.Framework.TrayIcon.Icon
{
    public interface IIconChanger : IEnumImpl<IconChangerFactory.ActionEnum>
    {
        /// <summary>
        /// Should the icon change
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        bool NeedsToChangeIcon(DeviceInfo deviceInfo);

        /// <summary>
        /// What to do when the option is selected
        /// </summary>
        /// <param name="trayIcon"></param>
        void ChangeIcon(Util.TrayIcon trayIcon);
    }
}