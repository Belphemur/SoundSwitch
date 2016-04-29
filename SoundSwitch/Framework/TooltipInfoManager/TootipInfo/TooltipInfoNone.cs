using SoundSwitch.Properties;

namespace SoundSwitch.Framework.TooltipInfoManager.TootipInfo
{
    public class TooltipInfoNone : ITooltipInfo
    {
        public string TextToDisplay()
        {
            return null;
        }

        public TooltipInfoTypeEnum Type()
        {
            return TooltipInfoTypeEnum.None;
        }

        public override string ToString()
        {
            return TooltipInfo.none;
        }
    }
}