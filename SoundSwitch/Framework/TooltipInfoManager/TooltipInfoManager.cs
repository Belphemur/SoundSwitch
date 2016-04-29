using System.Windows.Forms;

namespace SoundSwitch.Framework.TooltipInfoManager
{
    public class TooltipInfoManager
    {
        private readonly NotifyIcon _icon;

        public TooltipInfoManager(NotifyIcon icon)
        {
            _icon = icon;
        }
    }
}