using AudioEndPointControllerWrapper;

namespace SoundSwitch.Framework.TooltipInfoManager.TootipInfo
{
    public interface ITooltipInfo
    {
        /// <summary>
        /// The text to display for this tooltip
        /// </summary>
        /// <returns></returns>
        string TextToDisplay();

        /// <summary>
        /// Type of the Tooltip info
        /// </summary>
        /// <returns></returns>
        TooltipInfoTypeEnum Type();
    }
}