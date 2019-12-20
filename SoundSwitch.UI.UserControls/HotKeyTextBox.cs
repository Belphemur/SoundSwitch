using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using SoundSwitch.Common.WinApi.Keyboard;

namespace SoundSwitch.UI.UserControls
{
    public class HotKeyTextBox : TextBox
    {
        protected override void OnKeyUp(KeyEventArgs e)
        {
            HotKeys.ModifierKeys modifierKeys = 0;
            var displayString = "";
            foreach (var pressedModifier in KeyboardWindowsAPI.GetPressedModifiers())
            {
                if ((pressedModifier & Keys.Modifiers) == Keys.Control)
                {
                    modifierKeys |= HotKeys.ModifierKeys.Control;
                    displayString += "Ctrl+";
                }

                if ((pressedModifier & Keys.Modifiers) == Keys.Alt)
                {
                    modifierKeys |= HotKeys.ModifierKeys.Alt;
                    displayString += "Alt+";
                }

                if ((pressedModifier & Keys.Modifiers) == Keys.Shift)
                {
                    modifierKeys |= HotKeys.ModifierKeys.Shift;
                    displayString += "Shift+";
                }

                if (pressedModifier == Keys.LWin || pressedModifier == Keys.RWin)
                {
                    modifierKeys |= HotKeys.ModifierKeys.Win;
                    displayString += "Win+";
                }
            }

            var normalPressedKeys = KeyboardWindowsAPI.GetNormalPressedKeys();
            var key = normalPressedKeys.FirstOrDefault();


            if (key == Keys.None)
            {
                Text = displayString;
                ForeColor = Color.Crimson;
            }
            else
            {
                Text = displayString + key;
                Tag = new HotKeys(e.KeyCode, modifierKeys);
            }

            e.Handled = true;
            base.OnKeyUp(e);
        }
    }
}