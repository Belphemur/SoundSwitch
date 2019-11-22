using SoundSwitch.Framework.Audio.Device;
using SoundSwitch.Framework.Factory;

namespace SoundSwitch.Framework.TrayIcon.Icon
{
    public interface IIconChanger : IEnumImpl<IconChangerFactory.Enum>
    {
        /// <summary>
        /// Should the icon change
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        bool ChangeIcon(DeviceInfo deviceInfo);
    }
}