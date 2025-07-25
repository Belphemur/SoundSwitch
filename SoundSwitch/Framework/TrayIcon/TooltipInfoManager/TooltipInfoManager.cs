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

using System;
using System.Reflection;
using System.Windows.Forms;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.TrayIcon.TooltipInfoManager.TootipInfo;
using SoundSwitch.Util;

namespace SoundSwitch.Framework.TrayIcon.TooltipInfoManager;

public class TooltipInfoManager(NotifyIcon icon)
{

    private static class Fixes
    {
        public static void SetNotifyIconText(NotifyIcon ni, string text)
        {
            //if (text.Length >= 128) throw new ArgumentOutOfRangeException("Text limited to 127 characters");
            Type t = typeof(NotifyIcon);
            BindingFlags hidden = BindingFlags.NonPublic | BindingFlags.Instance;
            t.GetField("_text", hidden)?.SetValue(ni, text);
            if ((bool) t.GetField("_added", hidden).GetValue(ni))
                t.GetMethod("UpdateIcon", hidden).Invoke(ni, new object[] {true});
        }
    }

    private readonly TooltipInfoFactory _tooltipInfoFactory = new();

    /// <summary>
    /// Currently active tooltip info
    /// </summary>
    public static TooltipInfoTypeEnum CurrentTooltipInfo
    {
        get { return AppConfigs.Configuration.TooltipInfo; }
        set
        {
            if (value == AppConfigs.Configuration.TooltipInfo)
                return;

            AppConfigs.Configuration.TooltipInfo = value;
            AppConfigs.Configuration.Save();
        }
    }

    /// <summary>
    /// Show the tooltip with the TrayIcon
    /// </summary>
    public void SetIconText()
    {
        var tooltipInfo = _tooltipInfoFactory.Get(CurrentTooltipInfo);
        var text = tooltipInfo.TextToDisplay();

        if (text == null)
            return;

        //Taken from NotifyIcon.MaxTextSize
        text = text.Truncate(127);
        //Only if changed
        if (icon.Text == text)
        {
            return;
        }
        Fixes.SetNotifyIconText(icon, $"{Application.ProductName}\n{text}");
    }
}