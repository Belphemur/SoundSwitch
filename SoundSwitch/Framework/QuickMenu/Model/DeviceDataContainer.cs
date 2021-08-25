using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.UI.Menu.Component;

namespace SoundSwitch.Framework.QuickMenu.Model
{
    public class DeviceDataContainer : IconMenuItem<DeviceFullInfo>.DataContainer
    {
        public DeviceDataContainer(DeviceFullInfo payload, bool selected) : base(payload.LargeIcon, payload.NameClean, selected, payload.Id, payload)
        {
        }
    }
}