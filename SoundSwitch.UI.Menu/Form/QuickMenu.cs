using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoundSwitch.UI.Menu.Component;
using SoundSwitch.UI.Menu.Util;
using SoundSwitch.UI.Menu.Util.Timer;

namespace SoundSwitch.UI.Menu.Form
{
    public partial class QuickMenu<T> : System.Windows.Forms.Form
    {
        private delegate Task MethodInvoker();

        public record MenuClickedEvent(IconMenuItem<T>.DataContainer Item);

        private readonly Dictionary<string, IconMenuItem<T>.DataContainer> _currentPayloads = new();
        private readonly DebounceDispatcher _debounce = new();
        private bool _hiding = false;
        private bool _isLocationSet = false;
        private readonly MethodInvoker _hideDisposeMethod;
        private readonly TimeSpan _menuTimeOut = TimeSpan.FromSeconds(2);

        [Browsable(true)]
        public event EventHandler<MenuClickedEvent> ItemClicked;

        [Browsable(true)]
        public event EventHandler<MenuClickedEvent> SelectionChanged;

        protected override bool ShowWithoutActivation => true;
        
        internal QuickMenu()
        {
            InitializeComponent();
            _hideDisposeMethod = HideDispose;
            TopMost = true;
        }

        /// <summary>
        /// Clear the event handler for <see cref="ItemClicked"/> and <see cref="SelectionChanged"/>
        /// </summary>
        public void ClearEventHandlers()
        {
            SelectionChanged = null;
            ItemClicked = null;
        }

        public void SetData(IEnumerable<IconMenuItem<T>.DataContainer> payloads)
        {
            DebounceHiding();

            //We want to keep the order of the payload
            var originalOrderPayloads = payloads.ToArray();
            var newPayloadsById = originalOrderPayloads.ToDictionary(container => container.Id);

            var toRemove = _currentPayloads.Keys.Except(newPayloadsById.Keys);
            var toAdd = newPayloadsById.Keys.Except(_currentPayloads.Keys);
            var toModify = _currentPayloads.Keys.Intersect(newPayloadsById.Keys);

            var currentPayloadSizeDifferentNewPayloads = _currentPayloads.Count != originalOrderPayloads.Length;
            var needRearrange = false;

            var controlCollection = Controls;
            foreach (var id in toRemove)
            {
                var control = controlCollection[id];
                controlCollection.Remove(control);
                _currentPayloads.Remove(id);
                control.Dispose();
                needRearrange = true;
            }

            foreach (var key in toModify)
            {
                _currentPayloads[key].OverrideMetadata(newPayloadsById[key]);
            }


            foreach (var key in toAdd)
            {
                var payload = newPayloadsById[key];
                var control = new IconMenuItem<T>(payload);
                control.Click += (_, _) => OnItemClicked(control);
                controlCollection.Add(control);
                _currentPayloads.Add(payload.Id, payload);
                needRearrange = true;
            }

            if (needRearrange)
            {
                var top = 5;
                foreach (var payload in originalOrderPayloads)
                {
                    var control = controlCollection[payload.Id];
                    control.Top = top;
                    top += control.Height;
                }

                Height = 0;
            }

            if (!_isLocationSet || currentPayloadSizeDifferentNewPayloads)
            {
                Region = Region.FromHrgn(RoundedCorner.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            }

            if (!_isLocationSet)
            {
                Show();
                SetLocationToCursor();
                _isLocationSet = true;
            }
        }

        private void DebounceHiding()
        {
            _hiding = false;
            _debounce.Debounce<object>(_menuTimeOut, _ => BeginInvoke(_hideDisposeMethod));
            ResetOpacity();
        }

        private void OnItemClicked(IconMenuItem<T> control)
        {
            DebounceHiding();
            var dataContainer = control.CurrentDataContainer;
            ItemClicked?.Invoke(control, new MenuClickedEvent(dataContainer));

            if (dataContainer.Selected)
                return;

            foreach (var payload in _currentPayloads.Values)
            {
                payload.Selected = payload.Id == dataContainer.Id;
            }

            SelectionChanged?.Invoke(control, new MenuClickedEvent(dataContainer));
        }


        private async Task HideDispose()
        {
            _hiding = true;
            while (Opacity > 0.0)
            {
                await Task.Delay(50);

                if (!_hiding)
                    return;
                Opacity -= 0.05;
            }

            Hide();
            Dispose();
            _isLocationSet = false;
        }

        private void ResetOpacity()
        {
            Opacity = 0.7D;
        }

        private void SetLocationToCursor()
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            Point qmLoc = new Point(
                Math.Min(Cursor.Position.X, screenWidth - Width),
                Math.Min(Cursor.Position.Y, screenHeight - Height)
            );

            SetDesktopLocation(qmLoc.X, qmLoc.Y);
            Location = qmLoc;
            _isLocationSet = true;
        }
    }
}