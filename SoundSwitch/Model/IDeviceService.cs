/********************************************************************
 * Copyright (C) 2015 Jeroen Pelgrims
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

using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Model;

/// <summary>
/// Audio device operations: selection, cycling, microphone mute control.
/// </summary>
public interface IDeviceService
{
    /// <summary>
    /// Devices selected for Switching
    /// </summary>
    DeviceCollection<DeviceInfo> SelectedDevices { get; }

    /// <summary>
    /// An union between the Active audio devices and selected playback devices
    /// </summary>
    IEnumerable<DeviceFullInfo> AvailablePlaybackDevices { get; }

    /// <summary>
    /// An union between the Active audio devices and selected recording devices
    /// </summary>
    IEnumerable<DeviceFullInfo> AvailableRecordingDevices { get; }

    /// <summary>
    /// When the selected list of device to switch from is changed.
    /// </summary>
    event EventHandler<DeviceListChanged> SelectedDeviceChanged;

    /// <summary>
    /// The Default device has been changed.
    /// </summary>
    event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged;

    bool SelectDevice(DeviceFullInfo device);
    bool UnselectDevice(DeviceFullInfo device);
    bool SetActiveDevice(DeviceInfo device);
    bool CycleActiveDevice(EDataFlow type);
    (string DeviceName, bool IsMuted)? ToggleMicrophoneMute();
    (string DeviceName, bool IsMuted)? SetMicrophoneMuteState(bool muteState);
    (string DeviceName, bool IsMuted)? SetMicrophoneMuteState(string deviceId, bool muteState);
}
