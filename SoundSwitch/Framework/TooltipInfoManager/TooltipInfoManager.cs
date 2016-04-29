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
        private bool _isBallontipVisible;

        public TooltipInfoManager(NotifyIcon icon)
        {
            _icon = icon;
            _tooltipInfoFactory = new TooltipInfoFactory();
            _icon.BalloonTipShown += IconOnBalloonTipShown;
            _icon.BalloonTipClosed += IconOnBalloonTipClosed;
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

        private void IconOnBalloonTipClosed(object sender, EventArgs eventArgs)
        {
            _isBallontipVisible = false;
        }

        private void IconOnBalloonTipShown(object sender, EventArgs eventArgs)
        {
            _isBallontipVisible = true;
        }


        /// <summary>
        ///     Show the tooltip with the NotifyIcon
        /// </summary>
        public void ShowTooltipInfo()
        {
            if (_isBallontipVisible)
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