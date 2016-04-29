using System.Windows.Forms;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.TooltipInfoManager.TootipInfo;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.TooltipInfoManager
{
    public class TooltipInfoManager
    {
        private readonly NotifyIcon _icon;
        private readonly TooltipInfoFactory _tooltipInfoFactory;

        public TooltipInfoManager(NotifyIcon icon)
        {
            _icon = icon;
            _tooltipInfoFactory = new TooltipInfoFactory();
        }

        public ToolTipInfoTypeEnum CurrentTooltipInfo
        {
            get { return AppConfigs.Configuration.TooltipInfo; }
            set
            {
                AppConfigs.Configuration.TooltipInfo = value;
                AppConfigs.Configuration.Save();
            }
        }

        /// <summary>
        ///     Show the tooltip with the NotifyIcon
        /// </summary>
        public void ShowTooltipInfo()
        {
            var tooltipInfo = _tooltipInfoFactory.Get(CurrentTooltipInfo);
            var text = tooltipInfo.TextToDisplay();

            if (text == null)
                return;

            _icon.ShowBalloonTip(1000, TooltipInfo.titleTooltip, text, ToolTipIcon.Info);
        }
    }
}