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

using System.Linq;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Localization;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.TrayIcon.TooltipInfoManager.TootipInfo
{
    public class TooltipInfoPlayback : ITooltipInfo
    {
        public TooltipInfoTypeEnum TypeEnum => TooltipInfoTypeEnum.Playback;
        public string Label => SettingsStrings.tooltipOnHoverOptionPlaybackDevice;

        /// <summary>
        /// The text to display for this ToolTip
        /// </summary>
        /// <returns></returns>
        public string TextToDisplay()
        {
            var audioDevices = AppModel.Instance.ActiveAudioDeviceLister?.PlaybackDevices;
            if (audioDevices == null)
            {
                return string.Format(SettingsStrings.activePlayback, "Unknown");
            }

            var playbackDefaultDevice = audioDevices
                .FirstOrDefault(device =>
                    AudioSwitcher.Instance.IsDefault(device.Id, (EDataFlow)device.Type, ERole.eConsole));
            return playbackDefaultDevice == null
                ? null
                : string.Format(SettingsStrings.activePlayback, playbackDefaultDevice);
        }

        public override string ToString()
        {
            return Label;
        }
    }
}