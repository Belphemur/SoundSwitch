using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SoundSwitch.UI.Forms.Components;

namespace SoundSwitch.UI.Forms
{
    public partial class DeviceSelectorMenu : Form
    {
        private readonly Dictionary<string, IconMenuItem.DataContainer> _currentPayloads = new();
        private readonly object _lock = new();
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
        }

        public void SetData(IEnumerable<IconMenuItem.DataContainer> payloads)
        {
            lock (_lock)
            {
                SetLocationToCursor();
                var top = 5;

                var payloadsArray = payloads.ToArray();
                var newPayloads = payloadsArray.ToDictionary(container => container.Id);
                var toRemove = _currentPayloads.Keys.Except(newPayloads.Keys);
                var toAdd = newPayloads.Keys.Except(_currentPayloads.Keys);
                var toModify = _currentPayloads.Keys.Intersect(newPayloads.Keys);

                var needRearrange = false;

                foreach (var id in toRemove)
                {
                    Controls.RemoveByKey(id);
                    _currentPayloads.Remove(id);
                    needRearrange = true;
                }

                foreach (var key in toModify)
                {
                    _currentPayloads[key].OverrideMetadata(newPayloads[key]);
                }


                foreach (var key in toAdd)
                {
                    var payload = newPayloads[key];
                    var control = new IconMenuItem(payload);
                    Controls.Add(control);
                    _currentPayloads.Add(payload.Id, payload);
                    needRearrange = true;
                }

                if (needRearrange)
                {
                    foreach (var payload in payloadsArray)
                    {
                        var control = Controls[payload.Id];
                        control.Top = top;
                        top += control.Height;
                    }
                    Height = 0;
                }


                Show();
            }
        }

        private void SetLocationToCursor()
        {
            SetDesktopLocation(Cursor.Position.X, Cursor.Position.Y);
            Location = Cursor.Position;
        }
    }
}