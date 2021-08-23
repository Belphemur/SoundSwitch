using System.Windows.Forms;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.UI.Forms.Components
{
    public partial class AudioDeviceBox : UserControl
    {
        public AudioDeviceBox(DeviceFullInfo device)
        {
            iconBox.DataBindings.Add(nameof(PictureBox.Image), device, nameof(device.LargeIcon), false, DataSourceUpdateMode.OnPropertyChanged);
            deviceName.DataBindings.Add(nameof(Label.Text), device, nameof(device.NameClean), false, DataSourceUpdateMode.OnPropertyChanged);
        }
    }
}