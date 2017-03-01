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

using System.Collections.Generic;
using System.Linq;
using AudioEndPointControllerWrapper;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler
{
    public abstract class ADeviceCycler : IDeviceCycler
    {
        private readonly IDictionary<AudioDeviceType, IAudioDevice> _lastDevices = new Dictionary<AudioDeviceType, IAudioDevice>();

        public abstract DeviceCyclerTypeEnum TypeEnum { get; }
        public abstract string Label { get; }

        /// <summary>
        /// Cycle the audio device for the given type
        /// </summary>
        /// <param name="type"></param>
        public abstract bool CycleAudioDevice(AudioDeviceType type);

        /// <summary>
        /// Get the next device that need to be set as Default
        /// </summary>
        /// <param name="audioDevices"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected IAudioDevice GetNextDevice(ICollection<IAudioDevice> audioDevices, AudioDeviceType type)
        {
            IAudioDevice lastDevice;
            _lastDevices.TryGetValue(type, out lastDevice);

            var defaultDev = lastDevice ??
                             audioDevices.FirstOrDefault(device => device.IsDefault(Role.Console)) ??
                             audioDevices.Last();
            var next = audioDevices.SkipWhile((device, i) => !Equals(device, defaultDev)).Skip(1).FirstOrDefault() ??
                       audioDevices.ElementAt(0);
            return next;
        }

        /// <summary>
        /// Attempts to set active device to the specified name
        /// </summary>
        /// <param name="device"></param>
        public bool SetActiveDevice(IAudioDevice device)
        {
            using (AppLogger.Log.InfoCall())
            {
                AppLogger.Log.Info("Set Default device", device);
                if (!AppModel.Instance.SetCommunications)
                {
                    device.SetAsDefault(Role.Console);
                    device.SetAsDefault(Role.Multimedia);
                }
                else
                {
                    AppLogger.Log.Info("Set Default Communication device", device);
                    device.SetAsDefault(Role.All);
                }
                _lastDevices[device.Type] = device;
                return true;
            }
        }
    }
}