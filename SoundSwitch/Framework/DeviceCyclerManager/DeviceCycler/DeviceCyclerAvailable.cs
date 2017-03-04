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

using System;
using System.Collections.Generic;
using AudioEndPointControllerWrapper;
using SoundSwitch.Model;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler
{
    public class DeviceCyclerAvailable : ADeviceCycler
    {
        public override DeviceCyclerTypeEnum TypeEnum => DeviceCyclerTypeEnum.Available;
        public override string Label => SettingsStrings.cycleThroughOptionOnlySelectedAudioDevices;

        /// <summary>
        /// Cycle the audio device for the given type
        /// </summary>
        /// <param name="type"></param>
        public override bool CycleAudioDevice(AudioDeviceType type)
        {
            ICollection<IAudioDevice> audioDevices;
            switch (type)
            {
                case AudioDeviceType.Playback:
                    audioDevices = AppModel.Instance.AvailablePlaybackDevices;
                    break;
                case AudioDeviceType.Recording:
                    audioDevices = AppModel.Instance.AvailableRecordingDevices;
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

            return SetActiveDevice(GetNextDevice(audioDevices, type));
        }
    }
}