using System.Drawing;
using System.Windows.Forms;
using SoundSwitch.Util;

namespace SoundSwitch.UI.Forms.Components
{
    public partial class AudioDeviceBox : UserControl
    {
        public Data CurrentData { get; }

        public record Data(Image Image, string Label, bool Selected)
        {
            public Color Color => Selected ? Color.RoyalBlue.WithOpacity(0x70) : Color.Black.WithOpacity(0x70);
        }


        public AudioDeviceBox(Data data)
        {
            CurrentData = data;
            InitializeComponent();

            base.CreateParams.ExStyle |= 0x20;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            iconBox.BackColor = Color.Transparent;
            deviceName.BackColor = Color.Transparent;

            iconBox.DataBindings.Add(nameof(PictureBox.Image), CurrentData, nameof(CurrentData.Image), false, DataSourceUpdateMode.OnPropertyChanged);
            deviceName.DataBindings.Add(nameof(Label.Text), CurrentData, nameof(CurrentData.Label), false, DataSourceUpdateMode.OnPropertyChanged);
            DataBindings.Add(nameof(BackColor), CurrentData, nameof(CurrentData.Color), false, DataSourceUpdateMode.OnPropertyChanged);
        }
    }
}