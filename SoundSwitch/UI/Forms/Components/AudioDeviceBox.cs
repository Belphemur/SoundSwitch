using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.UI.Forms.Components
{
    public partial class AudioDeviceBox : UserControl
    {
        private bool _selected;

        public record Payload(Image Image, string Label, bool Selected);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [DefaultValue(false)]
        public bool Selected
        {
            get => _selected;
            set
            {
                var oldValue = _selected;
                _selected = value;
                OnSelectedChanged(oldValue, _selected);
            }
        }


        public AudioDeviceBox(Payload payload)
        {
            iconBox.DataBindings.Add(nameof(PictureBox.Image), payload, nameof(payload.Image), false, DataSourceUpdateMode.OnPropertyChanged);
            deviceName.DataBindings.Add(nameof(Label.Text), payload, nameof(payload.Label), false, DataSourceUpdateMode.OnPropertyChanged);
            Selected = payload.Selected;
            
            InitializeComponent();
        }

        protected void OnSelectedChanged(bool oldValue, bool newValue)
        {
            ForeColor = newValue ? Color.Teal : Color.Black;
        }
    }
}