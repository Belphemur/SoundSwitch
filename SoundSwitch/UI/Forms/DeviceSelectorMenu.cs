using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SoundSwitch.UI.Forms.Components;
using SoundSwitch.Util.Timer;

namespace SoundSwitch.UI.Forms
{
    public partial class DeviceSelectorMenu : Form
    {
        private readonly Dictionary<string, IconMenuItem.DataContainer> _currentPayloads = new();
        private readonly DebounceDispatcher _debounce = new();
        private bool _hiding = false;
        private readonly MethodInvoker _deleteMethod;
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
                // p.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                p.ExStyle |= 0x00000008; // WS_EX_TOPMOST
                return p;
            }
        }

        public DeviceSelectorMenu()
        {
            InitializeComponent();
            ResetOpacity();
            _deleteMethod = Delete;
        }

        public void SetData(IEnumerable<IconMenuItem.DataContainer> payloads)
        {
            _hiding = false;
            _debounce.Debounce<object>(TimeSpan.FromMilliseconds(1500), _ => BeginInvoke(_deleteMethod));
            ResetOpacity();

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
                var top = 5;
                foreach (var payload in payloadsArray)
                {
                    var control = Controls[payload.Id];
                    control.Top = top;
                    top += control.Height;
                }

                Height = 0;
            }

            Show();
            SetLocationToCursor();
        }

        private void Delete()
        {
            _hiding = true;
            while (Opacity > 0.0)
            {
                Thread.Sleep(50);

                if (!_hiding)
                    break;
                Opacity -= 0.05;
            }

            Hide();
            Dispose();
        }

        private void ResetOpacity()
        {
            Opacity = 0.7D;
        }

        private void SetLocationToCursor()
        {
            SetDesktopLocation(Cursor.Position.X, Cursor.Position.Y);
            Location = Cursor.Position;
        }
    }
}