using System.Collections.Generic;
using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.TooltipInfoManager.TootipInfo;

namespace SoundSwitch.Framework.TooltipInfoManager
{
    public class TooltipInfoFactory : AbstractFactory<ToolTipInfoTypeEnum, ITooltipInfo>
    {
        private static readonly IDictionary<ToolTipInfoTypeEnum, ITooltipInfo> TooltipInfos = new Dictionary
            <ToolTipInfoTypeEnum, ITooltipInfo>
        {
            {ToolTipInfoTypeEnum.Playback, new TooltipInfoPlayback()},
            {ToolTipInfoTypeEnum.Recording, new TooltipInfoRecording()},
            {ToolTipInfoTypeEnum.Both, new TooltipInfoBoth()}
        };

        public TooltipInfoFactory() : base(TooltipInfos)
        {
        }
    }
}