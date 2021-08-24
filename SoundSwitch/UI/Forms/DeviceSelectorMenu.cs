using System.Collections.Generic;
using System.Windows.Forms;
using SoundSwitch.UI.Forms.Components;

namespace SoundSwitch.UI.Forms
{
    public partial class DeviceSelectorMenu : Form
    {
        protected override bool ShowWithoutActivation => true;

        /// <summary>
        /// Override the parameters used to create the window handle.
        /// Ensure that the window will be top-most and do not activate or take focus.
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams p = base.CreateParams;
                p.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                p.ExStyle |= 0x00000008; // WS_EX_TOPMOST
                return p;
            }
        }

        public DeviceSelectorMenu()
        {
            InitializeComponent();
            SetLocationToCursor();
        }

        public void SetData(IEnumerable<AudioDeviceBox.Payload> payloads)
        {
            Hide();
            SetLocationToCursor();
            Controls.Clear();
            Height = 0;
            var top = 5;
            foreach (var payload in payloads)
            {
                var control = new AudioDeviceBox(payload);
                control.Top = top;
                Controls.Add(control);
                top += control.Height;
            }
            Show();
        }

        private void SetLocationToCursor()
        {
            SetDesktopLocation(Cursor.Position.X, Cursor.Position.Y);
            Location = Cursor.Position;
        }
    }
}