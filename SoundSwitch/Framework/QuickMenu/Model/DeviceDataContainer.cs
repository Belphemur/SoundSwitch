using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.UI.Menu.Component;

namespace SoundSwitch.Framework.QuickMenu.Model;

public class DeviceDataContainer : IconMenuItem<DeviceFullInfo>.DataContainer
{
    public DeviceDataContainer(DeviceFullInfo payload, bool selected)
        : this(payload, selected, payload.LargeIcon) { }

    /// <summary>
    /// Chained constructor that holds the <paramref name="iconHandle"/> alive across the
    /// base-class construction (which clones the icon immediately), then disposes the handle.
    /// </summary>
    private DeviceDataContainer(DeviceFullInfo payload, bool selected, IconHandle iconHandle)
        : base(iconHandle.Icon, payload.NameClean, selected, payload.Id, payload)
    {
        // The base DataContainer clones the icon in its constructor, so the handle can be
        // released here; the clone is owned and managed by DataContainer independently.
        iconHandle.Dispose();
    }
}
