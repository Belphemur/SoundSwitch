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
            var playbackToDisplay = new TooltipInfoPlayback().TextToDisplay();
            var recordingToDisplay = new TooltipInfoRecording().TextToDisplay();

            if(playbackToDisplay == null || recordingToDisplay == null)
                return null;

            return string.Concat(playbackToDisplay, "\n",
                recordingToDisplay);
        }

        /// <summary>
        ///     Type of the Tooltip info
        /// </summary>
        /// <returns></returns>
        public TooltipInfoTypeEnum Type()
        {
            return TooltipInfoTypeEnum.Both;
        }

        public override string ToString()
        {
            return TooltipInfo.both;
        }
    }
}