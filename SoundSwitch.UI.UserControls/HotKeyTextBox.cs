using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SoundSwitch.Common.WinApi.Keyboard;

namespace SoundSwitch.UI.UserControls
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
                HotKeyChanged?.Invoke(this, new Event());
            }
        }

        private HotKey _hotKey;
        [Browsable(true)]
        public event EventHandler<Event> HotKeyChanged;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            HotKey.ModifierKeys modifierKeys = 0;
            foreach (var pressedModifier in KeyboardWindowsAPI.GetPressedModifiers())
            {
                if ((pressedModifier & Keys.Modifiers) == Keys.Control)
                {
                    modifierKeys |= HotKey.ModifierKeys.Control;
                }

                if ((pressedModifier & Keys.Modifiers) == Keys.Alt)
                {
                    modifierKeys |= HotKey.ModifierKeys.Alt;
                }

                if ((pressedModifier & Keys.Modifiers) == Keys.Shift)
                {
                    modifierKeys |= HotKey.ModifierKeys.Shift;
                }

                if (pressedModifier == Keys.LWin || pressedModifier == Keys.RWin)
                {
                    modifierKeys |= HotKey.ModifierKeys.Win;
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
            }

            e.Handled = true;
            base.OnKeyUp(e);
        }
    }
}