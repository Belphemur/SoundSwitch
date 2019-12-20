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
        public class HotKeyChangedEventArgs : EventArgs
        {
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [DefaultValue(null)]
        public HotKeys HotKeys
        {
            get => _hotKeys;
            set
            {
                _hotKeys = value;
                Text = value?.Display() ?? "";
                HotKeyChanged?.Invoke(this, new HotKeyChangedEventArgs());
            }
        }

        private HotKeys _hotKeys;
        [Browsable(true)]
        public event EventHandler<HotKeyChangedEventArgs> HotKeyChanged;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            HotKeys.ModifierKeys modifierKeys = 0;
            foreach (var pressedModifier in KeyboardWindowsAPI.GetPressedModifiers())
            {
                if ((pressedModifier & Keys.Modifiers) == Keys.Control)
                {
                    modifierKeys |= HotKeys.ModifierKeys.Control;
                }

                if ((pressedModifier & Keys.Modifiers) == Keys.Alt)
                {
                    modifierKeys |= HotKeys.ModifierKeys.Alt;
                }

                if ((pressedModifier & Keys.Modifiers) == Keys.Shift)
                {
                    modifierKeys |= HotKeys.ModifierKeys.Shift;
                }

                if (pressedModifier == Keys.LWin || pressedModifier == Keys.RWin)
                {
                    modifierKeys |= HotKeys.ModifierKeys.Win;
                }
            }

            var normalPressedKeys = KeyboardWindowsAPI.GetNormalPressedKeys();
            var key = normalPressedKeys.FirstOrDefault();


            if (key == Keys.None)
            {
                Text = new HotKeys(key, modifierKeys).Display();
                ForeColor = Color.Crimson;
            }
            else
            {
                HotKeys = new HotKeys(key, modifierKeys);
                ForeColor = Color.Green;
            }

            e.Handled = true;
            base.OnKeyUp(e);
        }
    }
}