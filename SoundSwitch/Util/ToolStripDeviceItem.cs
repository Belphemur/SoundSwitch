using System;
using System.Windows.Forms;
using SoundSwitch.Models;

namespace SoundSwitch.Util
{
    internal class ToolStripDeviceItem : ToolStripMenuItem
    {
        public ToolStripDeviceItem(EventHandler onClick, AudioDevice audioDevice)
            : base(audioDevice.FriendlyName, null, onClick)
        {
            AudioDevice = audioDevice;
        }

        public AudioDevice AudioDevice { get; set; }
    }
}