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

using CoreAudio;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Localization;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.TrayIcon.TooltipInfoManager.TootipInfo
{
    public class TooltipInfoRecording : ITooltipInfo
    {
        private DeviceFullInfo _defaultDevice;
        public TooltipInfoTypeEnum TypeEnum => TooltipInfoTypeEnum.Recording;
        public string Label => SettingsStrings.tooltipOnHoverOptionRecordingDevice;

        public TooltipInfoRecording()
        {
            AppModel.Instance.DefaultDeviceChanged += (sender, @event) =>
            {
                if (@event.Device.Type != DataFlow.Capture)
                    return;
                if (_defaultDevice != null)
                {
                    _defaultDevice.Dispose();
                }
                _defaultDevice = AudioSwitcher.Instance.GetAudioEndpoint(@event.DeviceId);
            };
            _defaultDevice = AudioSwitcher.Instance.GetDefaultAudioEndpoint(EDataFlow.eCapture, ERole.eConsole);
        }

        /// <summary>
        /// The text to display for this ToolTip
        /// </summary>
        /// <returns></returns>
        public string TextToDisplay()
        {
            return _defaultDevice == null
                ? null
                : string.Format("[{1}%] " + SettingsStrings.activeRecording, _defaultDevice.NameClean, _defaultDevice.Volume);
        }

        public override string ToString()
        {
            return Label;
        }
    }
}