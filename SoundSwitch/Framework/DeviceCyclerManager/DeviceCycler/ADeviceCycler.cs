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
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler
{
    public abstract class ADeviceCycler : IDeviceCycler
    {
        public abstract DeviceCyclerTypeEnum TypeEnum { get; }
        public abstract string Label { get; }

        /// <summary>
        /// Cycle the audio device for the given type
        /// </summary>
        /// <param name="type"></param>
        public abstract bool CycleAudioDevice(DataFlow type);

        /// <summary>
        /// Get the next device that need to be set as Default
        /// </summary>
        /// <param name="audioDevices"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected DeviceInfo GetNextDevice(IEnumerable<DeviceInfo> audioDevices, DataFlow type)
        {
            var deviceInfos = audioDevices as DeviceInfo[] ?? audioDevices.ToArray();
            var defaultDev = deviceInfos.FirstOrDefault(device => AudioSwitcher.Instance.IsDefault(device.Id, (EDataFlow)type, ERole.eConsole)) ??
                             deviceInfos.Last();
            var next = deviceInfos.SkipWhile((device, i) => device.Id != defaultDev.Id).Skip(1).FirstOrDefault() ??
                       deviceInfos.ElementAt(0);
            return next;
        }

        /// <summary>
        /// Attempts to set active device to the specified name
        /// </summary>
        /// <param name="device"></param>
        public bool SetActiveDevice(DeviceInfo device)
        {

            Log.Information("Set Default device: {Device}", device);
            if (!AppModel.Instance.SetCommunications)
            {
                AudioSwitcher.Instance.SwitchTo(device.Id, ERole.eConsole);
                AudioSwitcher.Instance.SwitchTo(device.Id, ERole.eMultimedia);
                if (AppModel.Instance.SwitchForegroundProgram)
                {
                    AudioSwitcher.Instance.ResetProcessDeviceConfiguration();
                    AudioSwitcher.Instance.SwitchProcessTo(device.Id, ERole.eConsole, (EDataFlow)device.Type);
                    AudioSwitcher.Instance.SwitchProcessTo(device.Id, ERole.eMultimedia, (EDataFlow)device.Type);
                }
               
            }
            else
            {
                Log.Information("Set Default Communication device: {Device}", device);
                AudioSwitcher.Instance.SwitchTo(device.Id, ERole.ERole_enum_count);
                if (AppModel.Instance.SwitchForegroundProgram)
                {
                    AudioSwitcher.Instance.ResetProcessDeviceConfiguration();
                    AudioSwitcher.Instance.SwitchProcessTo(device.Id, ERole.ERole_enum_count, (EDataFlow)device.Type);
                }
            }
            return true;

        }
    }
}