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
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Model;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler
{
    public class DeviceCyclerAll : ADeviceCycler
    {
        public override DeviceCyclerTypeEnum TypeEnum => DeviceCyclerTypeEnum.All;
        public override string Label => SettingsStrings.cycleThroughOptionAllAudioDevices;

        /// <summary>
        /// Cycle the audio device for the given type
        /// </summary>
        /// <param name="type"></param>
        public override bool CycleAudioDevice(DataFlow type)
        {
            IReadOnlyCollection<DeviceFullInfo> audioDevices;
            switch (type)
            {
                case DataFlow.Render:
                    audioDevices = AppModel.Instance.ActiveAudioDeviceLister.PlaybackDevices;
                    break;
                case DataFlow.Capture:
                    audioDevices = AppModel.Instance.ActiveAudioDeviceLister.RecordingDevices;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            switch (audioDevices.Count)
            {
                case 0:
                    throw new AppModel.NoDevicesException();
                case 1:
                    return false;
            }

            return SetActiveDevice(GetNextDevice(audioDevices.ToList(), type));
        }
    }
}