using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.UI.Forms.Components
{
    public partial class AudioDeviceBox : UserControl
    {
        private bool _selected;

        public record Payload(Image Image, string Label, bool Selected)
        {
            public Color Color => Selected ? Color.Teal : Color.Black;
        }


        public AudioDeviceBox(Payload payload)
        {
            InitializeComponent();
            
            iconBox.DataBindings.Add(nameof(PictureBox.Image), payload, nameof(payload.Image), false, DataSourceUpdateMode.OnPropertyChanged);
            deviceName.DataBindings.Add(nameof(Label.Text), payload, nameof(payload.Label), false, DataSourceUpdateMode.OnPropertyChanged);
            DataBindings.Add(nameof(BackColor), payload, nameof(payload.Color), false, DataSourceUpdateMode.OnPropertyChanged);
        }
    }
}