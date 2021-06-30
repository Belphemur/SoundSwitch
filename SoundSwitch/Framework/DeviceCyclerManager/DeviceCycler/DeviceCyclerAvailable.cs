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
using System.Linq;
using NAudio.CoreAudioApi;
using SoundSwitch.Localization;
using SoundSwitch.Model;

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
        public override bool CycleAudioDevice(DataFlow type)
        {
            var audioDevices = (type switch
            {
                DataFlow.Render  => AppModel.Instance.AvailablePlaybackDevices,
                DataFlow.Capture => AppModel.Instance.AvailableRecordingDevices,
                _                => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            }).ToArray();

            return audioDevices switch
            {
                {Length: 0} => throw new AppModel.NoDevicesException(),
                {Length: 1} => false,
                _           => SetActiveDevice(GetNextDevice(audioDevices, type))
            };
        }
    }
}