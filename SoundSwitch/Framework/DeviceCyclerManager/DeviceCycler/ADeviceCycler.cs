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

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.QuickMenu.Model;
using SoundSwitch.Model;
using SoundSwitch.UI.Menu;

namespace SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler;

public abstract class ADeviceCycler : IDeviceCycler
{
    public abstract DeviceCyclerType TypeEnum { get; }
    public abstract string Label { get; }

    /// <summary>
    /// Cycle the audio device for the given type
    /// </summary>
    /// <param name="type"></param>
    public bool CycleAudioDevice(EDataFlow type)
    {
        var audioDevices = GetDevices(type).ToArray();

        bool CycleDevice()
        {
            var nextDevice = GetNextDevice(audioDevices, type);
            if (AppModel.Instance.QuickMenuEnabled)
            {
                QuickMenuManager<DeviceFullInfo>.Instance.DisplayMenu(audioDevices.Select(info => new DeviceDataContainer(info, info.Id == nextDevice.Id)), @event => SetActiveDevice(@event.Item.Payload));
            }

            return SetActiveDevice(nextDevice);
        }

        return audioDevices switch
        {
            { Length: 0 } => throw new NoDevicesException(),
            { Length: 1 } => HandleSingleDevice(audioDevices[0], type),
            _             => CycleDevice()
        };
    }

    private bool HandleSingleDevice(DeviceFullInfo singleDevice, EDataFlow type)
    {
        using var currentDefaultDevice = AudioSwitcher.Instance.GetDefaultAudioEndpoint(type, ERole.eConsole);
        if (currentDefaultDevice == null || currentDefaultDevice.Id != singleDevice.Id)
        {
            return SetActiveDevice(singleDevice);
        }
        return false;
    }

    protected abstract IEnumerable<DeviceFullInfo> GetDevices(EDataFlow type);

    /// <summary>
    /// Get the next device that need to be set as Default
    /// </summary>
    /// <param name="audioDevices"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private DeviceInfo GetNextDevice(DeviceInfo[] audioDevices, EDataFlow type)
    {
        using var currentDefaultDevice = AudioSwitcher.Instance.GetDefaultAudioEndpoint(type, ERole.eConsole);
        var defaultDev = currentDefaultDevice ?? audioDevices.Last();
        var next = audioDevices.SkipWhile((device, _) => device.Id != defaultDev.Id).Skip(1).FirstOrDefault() ?? audioDevices[0];
        return next;
    }

    /// <summary>
    /// Attempts to set active device to the specified name
    /// </summary>
    /// <param name="device"></param>
    public bool SetActiveDevice(DeviceInfo device)
    {
        if (AppModel.Instance.KeepVolumeEnabled)
            AudioSwitcher.Instance.SetVolumeFromDefaultDevice(device.Type, device.Id);

        Log.Information("Set Default device: {Device}", device);
        if (!AppModel.Instance.SetCommunications)
        {
            AudioSwitcher.Instance.SwitchTo(device.Id, ERole.eConsole);
            AudioSwitcher.Instance.SwitchTo(device.Id, ERole.eMultimedia);
            if (AppModel.Instance.SwitchForegroundProgram)
            {
                AudioSwitcher.Instance.ResetProcessDeviceConfiguration();
                AudioSwitcher.Instance.SwitchForegroundProcessTo(device.Id, ERole.eConsole, device.Type);
                AudioSwitcher.Instance.SwitchForegroundProcessTo(device.Id, ERole.eMultimedia, device.Type);
            }
        }
        else
        {
            Log.Information("Set Default Communication device: {Device}", device);
            AudioSwitcher.Instance.SwitchTo(device.Id, ERole.ERole_enum_count);
            if (AppModel.Instance.SwitchForegroundProgram)
            {
                AudioSwitcher.Instance.ResetProcessDeviceConfiguration();
                AudioSwitcher.Instance.SwitchForegroundProcessTo(device.Id, ERole.ERole_enum_count, device.Type);
            }
        }

        return true;
    }
}
