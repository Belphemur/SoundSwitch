using SoundSwitch.Framework.Factory;

namespace SoundSwitch.Framework.TooltipInfoManager.TootipInfo
{
    public interface ITooltipInfo : IEnumImpl<TooltipInfoTypeEnum>
    {

        /// <summary>
        ///     The text to display for this tooltip
        /// </summary>
        /// <returns></returns>
        string TextToDisplay();
    }
}