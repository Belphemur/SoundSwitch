using SoundSwitch.Properties;

namespace SoundSwitch.Framework.TooltipInfoManager.TootipInfo
{
    public class TooltipInfoBoth : ITooltipInfo
    {
        /// <summary>
        ///     The text to display for this tooltip
        /// </summary>
        /// <returns></returns>
        public string TextToDisplay()
        {
            return string.Concat(new TooltipInfoPlayback().TextToDisplay(), "\n",
                new TooltipInfoRecording().TextToDisplay());
        }

        /// <summary>
        ///     Type of the Tooltip info
        /// </summary>
        /// <returns></returns>
        public ToolTipInfoTypeEnum Type()
        {
            return ToolTipInfoTypeEnum.Both;
        }

        public override string ToString()
        {
            return TooltipInfo.both;
        }
    }
}