using SoundSwitch.Common.Framework.Audio.Device;
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
        /// Change the icon to the current default device
        /// </summary>
        /// <param name="trayIcon"></param>
        void ChangeIcon(UI.Component.TrayIcon trayIcon);
        
        /// <summary>
        ///  Change icon to selected device if match the condition
        /// </summary>
        void ChangeIcon(UI.Component.TrayIcon trayIcon, DeviceFullInfo deviceInfo);
    }
}