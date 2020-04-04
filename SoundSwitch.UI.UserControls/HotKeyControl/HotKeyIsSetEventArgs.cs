using System;
using System.Windows.Forms;
using SoundSwitch.Common.WinApi.Keyboard;

namespace SoundSwitch.UI.UserControls.HotKeyControl
{
    public delegate void HotKeyIsSetEventHandler(object sender, HotKeyIsSetEventArgs e);

    public class HotKeyIsSetEventArgs : EventArgs
    {
        public HotKey HotKey { get;  }

        public bool Cancel { get; set; }

        public string CancelReason { get; set; }

        public string Shortcut => HotKeyShared.CombineShortcut(HotKey.Modifier, HotKey.Keys);

        public HotKeyIsSetEventArgs(HotKey hotKey)
        {
            HotKey = hotKey;
        }
    }
}