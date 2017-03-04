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

using AudioEndPointControllerWrapper;
using SoundSwitch.Framework.Factory;

namespace SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler
{
    public interface IDeviceCycler : IEnumImpl<DeviceCyclerTypeEnum>
    {
        /// <summary>
        /// Cycle the audio device for the given type
        /// </summary>
        /// <param name="type"></param>
        bool CycleAudioDevice(AudioDeviceType type);

        /// <summary>
        ///     Attempts to set active device to the specified name
        /// </summary>
        /// <param name="device"></param>
        bool SetActiveDevice(IAudioDevice device);
    }
}