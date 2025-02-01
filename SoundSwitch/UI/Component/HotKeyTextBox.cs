using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Framework.WinApi.Keyboard;

namespace SoundSwitch.UI.Component
{
    public class HotKeyTextBox : TextBox
    {
        public class Event : EventArgs
        {
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [DefaultValue(null)]
        public HotKey HotKey
        {
            get => _hotKey;
            set
            {
                _hotKey = value;
                Text = value?.Display() ?? "";
            }
        }

        private HotKey _hotKey;
        private bool _listenToHotkey;

        [Browsable(true)]
        public event EventHandler<Event> HotKeyChanged;

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ListenToHotkey
        {
            get => _listenToHotkey;
            set
            {
                _listenToHotkey = value;
                if (value)
                {
                    WindowsAPIAdapter.HotKeyPressed += WindowsAPIAdapterOnHotKeyPressed;
                }
                else
                {
                    WindowsAPIAdapter.HotKeyPressed -= WindowsAPIAdapterOnHotKeyPressed;
                }
            }
        }

        public void CleanHotKeyChangedHandler()
        {
            HotKeyChanged = null;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && ListenToHotkey)
            {
                WindowsAPIAdapter.HotKeyPressed -= WindowsAPIAdapterOnHotKeyPressed;
            }
            base.Dispose(disposing);
        }

        private void WindowsAPIAdapterOnHotKeyPressed(object? sender, WindowsAPIAdapter.KeyPressedEventArgs e)
        {
            Invoke(new Action(() =>
            {
                if (!Visible)
                {
                    return;
                }

                HotKey = e.HotKey;
                ForeColor = Color.Green;
                HotKeyChanged?.Invoke(this, new Event());
            }), null);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            HotKey.ModifierKeys modifierKeys = 0;
            foreach (var pressedModifier in KeyboardWindowsAPI.GetPressedModifiers())
            {
                {
                    switch (pressedModifier)
                    {
                        case Keys.Control:
                            modifierKeys |= HotKey.ModifierKeys.Control;
                            break;
                        case Keys.Alt:
                            modifierKeys |= HotKey.ModifierKeys.Alt;
                            break;
                        case Keys.Shift:
                            modifierKeys |= HotKey.ModifierKeys.Shift;
                            break;
                        case Keys.LWin:
                        case Keys.RWin:
                            modifierKeys |= HotKey.ModifierKeys.Win;
                            break;
                    }
                }
            }

            var normalPressedKeys = KeyboardWindowsAPI.GetNormalPressedKeys();
            var key = normalPressedKeys.FirstOrDefault();


            if (key == Keys.None)
            {
                Text = new HotKey(key, modifierKeys).Display();
                ForeColor = Color.Crimson;
            }
            else
            {
                HotKey = new HotKey(key, modifierKeys);
                ForeColor = Color.Green;
                HotKeyChanged?.Invoke(this, new Event());
            }

            e.Handled = true;
            base.OnKeyUp(e);
        }
    }
}