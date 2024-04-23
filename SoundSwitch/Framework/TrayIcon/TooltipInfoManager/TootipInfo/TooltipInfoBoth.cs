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

namespace SoundSwitch.Framework.TrayIcon.TooltipInfoManager.TootipInfo
{
    public class TooltipInfoBoth : ITooltipInfo
    {
        private readonly TooltipInfoRecording _tooltipInfoRecording = new();
        private readonly TooltipInfoPlayback _tooltipInfoPlayback = new();

        public TooltipInfoTypeEnum TypeEnum => TooltipInfoTypeEnum.Both;
        public string Label => SettingsStrings.tooltipOnHover_option_bothDevices;

        /// <summary>
        /// The text to display for this ToolTip
        /// </summary>
        /// <returns></returns>
        public string TextToDisplay()
        {
            var playbackToDisplay = _tooltipInfoPlayback.TextToDisplay();
            var recordingToDisplay = _tooltipInfoRecording.TextToDisplay();

            if (playbackToDisplay == null || recordingToDisplay == null)
                return null;

            return string.Concat(playbackToDisplay, "\n", recordingToDisplay);
        }

        public override string ToString() => Label;
    }
}