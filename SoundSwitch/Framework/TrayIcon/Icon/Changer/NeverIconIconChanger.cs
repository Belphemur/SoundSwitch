using System;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Localization;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public class NeverIconIconChanger : IIconChanger
    {
        public IconChangerFactory.ActionEnum TypeEnum => IconChangerFactory.ActionEnum.Never;
        public string Label => TrayIconStrings.iconChanger_none;

        public bool NeedsToChangeIcon(DeviceInfo deviceInfo)
        {
            return false;
        }

        public void ChangeIcon(UI.Component.TrayIcon trayIcon)
        {
            trayIcon.ReplaceIcon(Resources.Switch_SoundWave);
        }

        public void ChangeIcon(UI.Component.TrayIcon trayIcon, DeviceFullInfo deviceInfo)
        {
        }
    }
}