using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.TooltipInfoManager.TootipInfo;

namespace SoundSwitch.Framework.TooltipInfoManager
{
    public class TooltipInfoFactory : AbstractFactory<TooltipInfoTypeEnum, ITooltipInfo>
    {
        private static readonly IEnumImplList<TooltipInfoTypeEnum, ITooltipInfo> TooltipInfos = new EnumImplList
            <TooltipInfoTypeEnum, ITooltipInfo>
        {
            new TooltipInfoPlayback(),
            new TooltipInfoRecording(),
            new TooltipInfoBoth(),
            new TooltipInfoNone()
        };

        public TooltipInfoFactory() : base(TooltipInfos)
        {
        }
    }
}