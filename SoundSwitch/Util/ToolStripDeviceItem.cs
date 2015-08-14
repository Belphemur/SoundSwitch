using System;
using System.Windows.Forms;
using AudioEndPointControllerWrapper;

namespace SoundSwitch.Util
{
    internal class ToolStripDeviceItem : ToolStripMenuItem
    {
        public ToolStripDeviceItem(EventHandler onClick, AudioDeviceWrapper audioDevice)
            : base(audioDevice.FriendlyName, null, onClick)
        {
            AudioDevice = audioDevice;
        }

        public AudioDeviceWrapper AudioDevice { get; set; }
    }
}