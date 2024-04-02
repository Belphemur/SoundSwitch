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
using System.Threading;
using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Model
{
    public interface IAudioDeviceLister : IDisposable
    {

        bool Refreshing { get; }

        void Refresh(CancellationToken token);

        /// <summary>
        /// Get devices per type and state
        /// </summary>
        /// <param name="type"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        DeviceReadOnlyCollection<DeviceFullInfo> GetDevices(DataFlow type, DeviceState state);

        /// <summary>
        /// Process device updates
        /// </summary>
        /// <param name="deviceChangedEvents"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        void ProcessDeviceUpdates(SortedSet<DeviceChangedEvent> deviceChangedEvents);
    }
}