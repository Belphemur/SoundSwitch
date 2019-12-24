using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SoundSwitch.Common.WinApi.Keyboard;

namespace SoundSwitch.UI.UserControls
{
    public class HotkeyTextBox : TextBox
    {
        public class Event : EventArgs
        {
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [DefaultValue(null)]
        public Hotkey Hotkey
        {
            get => _hotkey;
            set
            {
                _hotkey = value;
                Text = value?.Display() ?? "";
                HotKeyChanged?.Invoke(this, new Event());
            }
        }

        private Hotkey _hotkey;
        [Browsable(true)]
        public event EventHandler<Event> HotKeyChanged;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            Hotkey.ModifierKeys modifierKeys = 0;
            foreach (var pressedModifier in KeyboardWindowsAPI.GetPressedModifiers())
            {
                if ((pressedModifier & Keys.Modifiers) == Keys.Control)
                {
                    modifierKeys |= Hotkey.ModifierKeys.Control;
                }

                if ((pressedModifier & Keys.Modifiers) == Keys.Alt)
                {
                    modifierKeys |= Hotkey.ModifierKeys.Alt;
                }

                if ((pressedModifier & Keys.Modifiers) == Keys.Shift)
                {
                    modifierKeys |= Hotkey.ModifierKeys.Shift;
                }

                if (pressedModifier == Keys.LWin || pressedModifier == Keys.RWin)
                {
                    modifierKeys |= Hotkey.ModifierKeys.Win;
                }
            }

            var normalPressedKeys = KeyboardWindowsAPI.GetNormalPressedKeys();
            var key = normalPressedKeys.FirstOrDefault();


            if (key == Keys.None)
            {
                Text = new Hotkey(key, modifierKeys).Display();
                ForeColor = Color.Crimson;
            }
            else
            {
                Hotkey = new Hotkey(key, modifierKeys);
                ForeColor = Color.Green;
            }

            e.Handled = true;
            base.OnKeyUp(e);
        }
    }
}