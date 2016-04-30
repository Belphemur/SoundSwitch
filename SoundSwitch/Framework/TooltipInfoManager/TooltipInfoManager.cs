using System;
using System.Reflection;
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
        /// <summary>
        /// Currently active tooltip info
        /// </summary>
        public static TooltipInfoTypeEnum CurrentTooltipInfo
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
            Fixes.SetNotifyIconText(_icon, $"{Application.ProductName}\n{text}");

            //_icon.ShowBalloonTip(1000, $"{Application.ProductName}: {TooltipInfo.titleTooltip}", text, ToolTipIcon.Info)

        }

        ~TooltipInfoManager()
        {
            _icon.BalloonTipClosed -= IconOnBalloonTipClosed;
            _icon.BalloonTipShown -= IconOnBalloonTipShown;
        }
    }

    public class Fixes
    {
        public static void SetNotifyIconText(NotifyIcon ni, string text)
        {
            //if (text.Length >= 128) throw new ArgumentOutOfRangeException("Text limited to 127 characters");
            Type t = typeof(NotifyIcon);
            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            t.GetField("text", hidden).SetValue(ni, text);
            if ((bool)t.GetField("added", hidden).GetValue(ni))
                t.GetMethod("UpdateIcon", hidden).Invoke(ni, new object[] { true });
        }
    }
}