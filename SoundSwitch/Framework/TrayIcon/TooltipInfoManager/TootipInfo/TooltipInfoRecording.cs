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

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.TooltipInfoManager.TootipInfo
{
    public class TooltipInfoRecording : ITooltipInfo
    {
        public TooltipInfoTypeEnum TypeEnum => TooltipInfoTypeEnum.Recording;
        public string Label => SettingsStrings.tooltipOnHoverOptionRecordingDevice;

        /// <summary>
        /// The text to display for this ToolTip
        /// </summary>
        /// <returns></returns>
        public string TextToDisplay()
        {
            var recordingDevice = AudioSwitcher.Instance.GetDefaultAudioEndpoint(EDataFlow.eCapture, ERole.eConsole);
            return recordingDevice == null
                ? null
                : string.Format(SettingsStrings.activeRecording + " - {1}%", recordingDevice.NameClean, recordingDevice.Volume);
        }

        public override string ToString()
        {
            return Label;
        }
    }
}