using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using HotKey = SoundSwitch.Framework.WinApi.Keyboard.HotKey;
using KeyboardWindowsAPI = SoundSwitch.Framework.WinApi.Keyboard.KeyboardWindowsAPI;

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
        [Browsable(true)] public event EventHandler<Event> HotKeyChanged;

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