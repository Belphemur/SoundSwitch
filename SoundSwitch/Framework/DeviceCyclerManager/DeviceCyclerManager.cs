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

using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Configuration;

namespace SoundSwitch.Framework.DeviceCyclerManager;

public class DeviceCyclerManager
{
    private readonly DeviceCyclerFactory _deviceCyclerFactory = new();

    public static DeviceCyclerTypeEnum CurrentCycler
    {
        get { return AppConfigs.Configuration.CyclerType; }
        set
        {
            if(value == AppConfigs.Configuration.CyclerType)
                return;
            AppConfigs.Configuration.CyclerType = value;
            AppConfigs.Configuration.Save();
        }
    }
    /// <summary>
    /// Cycle the audio device
    /// </summary>
    /// <param name="type"></param>
    public bool CycleDevice(DataFlow type)
    {
        return _deviceCyclerFactory.Get(CurrentCycler).CycleAudioDevice(type);
    }
    /// <summary>
    /// Set the device as Default
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    public bool SetAsDefault(DeviceInfo device)
    {
        return _deviceCyclerFactory.Get(CurrentCycler).SetActiveDevice(device);
    }
}