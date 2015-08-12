using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SoundSwitch.Models;

namespace SoundSwitch.Util
{
    class ToolStripDeviceItem : ToolStripDropDownItem
    {
        public AudioDevice AudioDevice { get; set; }

        public ToolStripDeviceItem(EventHandler onClick, AudioDevice audioDevice) : base(audioDevice.FriendlyName, null, onClick)
        {
            AudioDevice = audioDevice;
        }
    }
}
