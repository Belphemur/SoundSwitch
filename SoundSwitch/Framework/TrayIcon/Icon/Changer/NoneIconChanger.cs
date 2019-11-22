using SoundSwitch.Framework.Audio.Device;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public class NoneIconChanger : IIconChanger
    {
        public IconChangerFactory.Enum TypeEnum => IconChangerFactory.Enum.None;
        public string Label => TrayIconStrings.iconChanger_none;

        public bool ChangeIcon(DeviceInfo deviceInfo)
        {
            return false;
        }
    }
}