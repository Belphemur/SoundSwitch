using SoundSwitch.Properties;

namespace SoundSwitch.Framework.TooltipInfoManager.TootipInfo
{
    public class TooltipInfoNone : ITooltipInfo
    {
        public TooltipInfoTypeEnum TypeEnum { get; } = TooltipInfoTypeEnum.None;

        public string TextToDisplay()
        {
            return null;
        }


        public override string ToString()
        {
            return TooltipInfo.none;
        }
    }
}