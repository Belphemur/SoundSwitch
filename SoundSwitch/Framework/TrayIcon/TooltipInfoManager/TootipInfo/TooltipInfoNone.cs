﻿/********************************************************************
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

using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.TooltipInfoManager.TootipInfo;

public class TooltipInfoNone : ITooltipInfo
{
    public TooltipInfoTypeEnum TypeEnum => TooltipInfoTypeEnum.None;
    public string Label => SettingsStrings.none;

    /// <summary>
    /// The text to display for this ToolTip
    /// </summary>
    /// <returns></returns>
    public string TextToDisplay() => null;

    public override string ToString() => Label;
}