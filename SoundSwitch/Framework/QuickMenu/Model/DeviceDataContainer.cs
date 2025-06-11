using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.UI.Menu.Component;

namespace SoundSwitch.Framework.QuickMenu.Model;

public class DeviceDataContainer(DeviceFullInfo payload, bool selected)
    : IconMenuItem<DeviceFullInfo>.DataContainer(payload.LargeIcon, payload.NameClean, selected, payload.Id, payload);