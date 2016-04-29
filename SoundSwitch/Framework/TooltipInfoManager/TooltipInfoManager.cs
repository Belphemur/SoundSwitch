using System;
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
        public bool IsBallontipVisible { get; set; }

        public TooltipInfoManager(NotifyIcon icon)
        {
            _icon = icon;
            _tooltipInfoFactory = new TooltipInfoFactory();
            _icon.BalloonTipShown += IconOnBalloonTipShown;
            _icon.BalloonTipClosed += IconOnBalloonTipClosed;
        }

        public TooltipInfoTypeEnum CurrentTooltipInfo
        {
            get { return AppConfigs.Configuration.TooltipInfo; }
            set
            {
                if (value == AppConfigs.Configuration.TooltipInfo)
                    return;

                AppConfigs.Configuration.TooltipInfo = value;
                AppConfigs.Configuration.Save();
            }
        }

        private void IconOnBalloonTipClosed(object sender, EventArgs eventArgs)
        {
            IsBallontipVisible = false;
        }

        private void IconOnBalloonTipShown(object sender, EventArgs eventArgs)
        {
            IsBallontipVisible = true;
        }


        /// <summary>
        ///     Show the tooltip with the NotifyIcon
        /// </summary>
        public void ShowTooltipInfo()
        {
            if (IsBallontipVisible)
                return;

            var tooltipInfo = _tooltipInfoFactory.Get(CurrentTooltipInfo);
            var text = tooltipInfo.TextToDisplay();

            if (text == null)
                return;

            _icon.ShowBalloonTip(1000, $"{Application.ProductName}: {TooltipInfo.titleTooltip}", text, ToolTipIcon.Info);
        }

        ~TooltipInfoManager()
        {
            _icon.BalloonTipClosed -= IconOnBalloonTipClosed;
            _icon.BalloonTipShown -= IconOnBalloonTipShown;
        }
    }
}