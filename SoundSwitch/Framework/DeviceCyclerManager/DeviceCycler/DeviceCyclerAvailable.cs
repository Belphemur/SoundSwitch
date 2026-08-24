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

using SoundSwitch.Audio.Manager.Interop.Enum;

using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Localization;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.DeviceCyclerManager.DeviceCycler;

public class DeviceCyclerAvailable : ADeviceCycler
{
    public override DeviceCyclerType TypeEnum => DeviceCyclerType.Available;
    public override string Label => SettingsStrings.cycleThrough_option_onlySelectedAudioDevices;

    protected override IEnumerable<DeviceFullInfo> GetDevices(EDataFlow type)
    {
        return type switch
        {
            EDataFlow.eRender  => AppModel.Instance.AvailablePlaybackDevices,
            EDataFlow.eCapture => AppModel.Instance.AvailableRecordingDevices,
            _                => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
