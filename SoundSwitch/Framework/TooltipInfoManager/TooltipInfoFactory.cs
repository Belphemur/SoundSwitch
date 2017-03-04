/********************************************************************
* Copyright (C) 2015-2017 Antoine Aflalo
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
********************************************************************/

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