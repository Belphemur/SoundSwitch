using System.Collections.Generic;
using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.TooltipInfoManager.TootipInfo;

namespace SoundSwitch.Framework.TooltipInfoManager
{
    public class TooltipInfoFactory : AbstractFactory<TooltipInfoTypeEnum, ITooltipInfo>
    {
        private static readonly IDictionary<TooltipInfoTypeEnum, ITooltipInfo> TooltipInfos = new Dictionary
            <TooltipInfoTypeEnum, ITooltipInfo>
        {
            {TooltipInfoTypeEnum.Playback, new TooltipInfoPlayback()},
            {TooltipInfoTypeEnum.Recording, new TooltipInfoRecording()},
            {TooltipInfoTypeEnum.Both, new TooltipInfoBoth()}
        };

        public TooltipInfoFactory() : base(TooltipInfos)
        {
        }
    }
}