using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using SoundSwitch.Common.WinApi.Keyboard;

namespace SoundSwitch.UI.UserControls.HotKeyControl
{
    /// <summary>Allows adding custom hotkeys.
    /// </summary>
    [DefaultProperty("ForceModifiers"), DefaultEvent("HotKeyIsSet"), ToolboxBitmap(typeof(HotKeyControl), "HotKeyControl.png")]
    public partial class HotKeyControl : UserControl
    {
        private string _tooltip;
        private bool _keyisSet;

        [DllImport("user32.dll", EntryPoint = "SendMessageW")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        
        #region **Properties.

        /// <summary>Specifies that the control should force the user to use a modifier.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(true), DefaultValue(true)]
        [Description("Specifies that the control should force the user to use a modifier.")]
        public bool ForceModifiers { get; set; }

        ///// <summary>The value of this property can never be true, even if set.
        ///// </summary>
        //[EditorBrowsable(EditorBrowsableState.Never), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        //public override bool ShortcutsEnabled { get { return TextBox.ShortcutsEnabled; } set { base.ShortcutsEnabled = false; } }

        /// <summary>Gets or sets the current text in the HotKeyControl
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Always), Browsable(false)]
        public override string Text
        {
            get => TextBox.Text;
            set => TextBox.Text = value;
        }
        
        [Browsable(false)]
        public HotKey HotKey
        {
            get
            {
                if (string.IsNullOrEmpty(Text) || Text == Keys.None.ToString()) return new HotKey(Keys.None, HotKey.ModifierKeys.None);
                
                var parseHotkey = HotKeyShared.ParseShortcut(Text);
                return new HotKey((Keys)parseHotkey.GetValue(1), (HotKey.ModifierKeys)parseHotkey.GetValue(0));
            }
            set => Text = HotKeyShared.CombineShortcut(value.Modifier, value.Keys);
        }

        [Browsable(false)]
        public string ToolTip
        {
            get => _tooltip;
            set
            {
                ToolTipProvider.SetToolTip(TextBox, value);
                _tooltip = value;
            }


        }
        #endregion

        #region **Events
        /// <summary>Raised after a valid key is set.
        /// </summary>
        [Description("Raised when a valid key is set")]
        public event HotKeyIsSetEventHandler HotKeyIsSet;
        #endregion

        #region **Constructor
        public HotKeyControl()
        {
            InitializeComponent();
        }
        #endregion

        #region **Helpers
        void UpdateWatermark(string watermark = "")
        {
            if (!IsHandleCreated)
                return;

            IntPtr mem = Marshal.StringToHGlobalUni(watermark);
            SendMessage(TextBox.Handle, 0x1501, (IntPtr)1, mem);
            Marshal.FreeHGlobal(mem);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            if (Height != TextBox.Height)
                Height = TextBox.Height;
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            if (Text.Trim().EndsWith("+")) { Text = String.Empty; }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            //On KeyUp if KeyisSet is False then clear the textbox.
            if (_keyisSet == false)
            {
                Text = String.Empty;
            }
            else
            {
                if (HotKeyIsSet == null) return;
                
                var ex = new HotKeyIsSetEventArgs(HotKey);
                HotKeyIsSet(this, ex);
                if (ex.Cancel)
                {
                    _keyisSet = false;
                    Text      = String.Empty;
                    UpdateWatermark(ex.CancelReason);
                    TextBox.ForeColor = Color.Red;
                }
                else
                {
                    UpdateWatermark();
                    TextBox.ForeColor = Color.Green;
                }
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;  //Suppress the key from being processed by the underlying control.
            Text = string.Empty;  //Empty the content of the textbox
            _keyisSet = false; //At this point the user has not specified a shortcut.

            //Make the user specify a modifier. Control, Alt or Shift.
            //If a modifier is not present then clear the textbox.
            if (e.Modifiers == Keys.None && ForceModifiers)
            {
                MessageBox.Show("You have to specify a modifier like 'Control', 'Alt' or 'Shift'");
                Text = String.Empty;
                return;
            }

            //A modifier is present. Process each modifier.
            //Modifiers are separated by a ",". So we'll split them and write each one to the textbox.
            foreach (var modifier in KeyboardWindowsAPI.GetPressedModifiers())
            {
                switch (modifier)
                {
                    case Keys.None:
                        continue;
                    case Keys.LWin:
                    case Keys.RWin:
                        Text += @"Win + ";
                        continue;
                    default:
                        Text += $@"{modifier} + ";
                        break;
                }
            }

            //KEYCODE contains the last key pressed by the user.
            //If KEYCODE contains a modifier, then the user has not entered a shortcut. hence, KeyisSet is false
            //But if not, KeyisSet is true.
            if (e.KeyCode == Keys.ShiftKey | e.KeyCode == Keys.ControlKey | e.KeyCode == Keys.Menu | e.KeyCode == Keys.LWin | e.KeyCode == Keys.RWin)
            {
                _keyisSet = false;
            }
            else
            {
                Text += e.KeyCode.ToString();
                _keyisSet = true;
            }
        }
        #endregion
    }
}
