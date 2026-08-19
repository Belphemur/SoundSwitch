/********************************************************************
 * Copyright (C) 2015 Jeroen Pelgrims
 * Copyright (C) 2015-2024 Antoine Aflalo
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
using System.Linq;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio.Microphone;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.DeviceCyclerManager;

namespace SoundSwitch.Model;

public partial class AppModel
{
    public DeviceCollection<DeviceInfo> SelectedDevices
    {
        get
        {
            if (_selectedDevices != null)
                return _selectedDevices;

            return _selectedDevices = new DeviceCollection<DeviceInfo>(AppConfigs.Configuration.SelectedDevices.OrderBy(info => info.DiscoveredAt));
        }
    }

    public IEnumerable<DeviceFullInfo> AvailablePlaybackDevices =>
        AudioDeviceLister?.GetDevices(DataFlow.Render, DeviceState.Active).IntersectWith(SelectedDevices) ?? Enumerable.Empty<DeviceFullInfo>();

    public IEnumerable<DeviceFullInfo> AvailableRecordingDevices =>
        AudioDeviceLister?.GetDevices(DataFlow.Capture, DeviceState.Active).IntersectWith(SelectedDevices) ?? Enumerable.Empty<DeviceFullInfo>();

    public event EventHandler<DeviceListChanged> SelectedDeviceChanged;
    public event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged;

    #region Selected devices

    /// <summary>
    /// Add the device to the Selected device list
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    public bool SelectDevice(DeviceFullInfo device)
    {
        try
        {
            //Dont add device already selected
            if (SelectedDevices.Contains(device))
            {
                return false;
            }

            device.DiscoveredAt = DateTime.UtcNow;
            SelectedDevices.Add(device);
            AppConfigs.Configuration.SelectedDevices = SelectedDevices.ToHashSet();
        }
        catch (ArgumentException)
        {
            return false;
        }

        SelectedDeviceChanged?.Invoke(this, new DeviceListChanged(SelectedDevices, device.Type));
        AppConfigs.Configuration.Save();

        return true;
    }

    /// <summary>
    /// Remove the device from the Selected device list
    /// </summary>
    /// <param name="device"></param>
    /// <returns></returns>
    public bool UnselectDevice(DeviceFullInfo device)
    {
        bool result;
        try
        {
            result = SelectedDevices.Remove(device);
            AppConfigs.Configuration.SelectedDevices = SelectedDevices.ToHashSet();
        }
        catch (ArgumentException)
        {
            return false;
        }

        if (result)
        {
            SelectedDeviceChanged?.Invoke(this,
                new DeviceListChanged(SelectedDevices, device.Type));
            AppConfigs.Configuration.Save();
        }

        return result;
    }

    #endregion

    /// <summary>
    /// Handles microphone mute state changes and triggers appropriate notifications
    /// </summary>
    /// <param name="payload">The volume changed payload</param>
    private void HandleMicrophoneMuteChanged(DeviceVolumeChangedPayload payload)
    {
        Log.Information("Microphone {DeviceName} mute state changed from {WasMuted} to {IsMuted}",
            payload.Device.NameClean, payload.WasMuted, payload.IsMuted);

        // Notify about the mute state change
        _notificationManager.NotifyMuteChanged(payload.Device.Id, payload.Device.FriendlyName, payload.IsMuted);
    }

    #region Microphone control

    /// <summary>
    /// Toggles the system default microphone's mute state.
    /// </summary>
    /// <remarks>
    /// If no default microphone is found or the operation fails, the method invokes the <c>ErrorTriggered</c> event and logs an error; on success it logs the new mute state.
    /// </remarks>
    public (string DeviceName, bool IsMuted)? ToggleMicrophoneMute()
    {
        var result = _microphoneMuteToggler.ToggleDefaultMute();
        if (result == null)
        {
            ErrorTriggered?.Invoke(this, new ExceptionEvent(new Exception("No mic found or unable to toggle mute state")));
            Log.Error("No mic found or unable to toggle mute state");
        }
        else
        {
            Log.Information("Microphone {DeviceName} mute state is now {IsMuted}", result.Value.Name, result.Value.MuteState);
        }

        return result;
    }

    /// <summary>
    /// Toggles the mute state of the microphone
    /// </summary>
    /// <returns>Tuple with device name and mute state, null if no default microphone found or operation failed</returns>
    public (string DeviceName, bool IsMuted)? SetMicrophoneMuteState(string deviceId, bool muteState)
    {
        var result = _microphoneMuteToggler.SetMicrophoneMuteState(deviceId, muteState);
        if (result == null)
        {
            ErrorTriggered?.Invoke(this, new ExceptionEvent(new Exception("No mic found or unable to toggle mute state")));
        }
        return result;
    }

    /// <summary>
    /// Sets the mute state of the default microphone
    /// </summary>
    /// <param name="muteState">The desired mute state</param>
    /// <returns>Tuple with device name and mute state, null if no default microphone found or operation failed</returns>
    public (string DeviceName, bool IsMuted)? SetMicrophoneMuteState(bool muteState)
    {
        var result = _microphoneMuteToggler.SetDefaultMuteState(muteState);
        if (result == null)
        {
            ErrorTriggered?.Invoke(this, new ExceptionEvent(new Exception("No mic found or unable to set mute state")));
        }
        return result;
    }

    #endregion

    #region Active device

    /// <summary>
    ///     Attempts to set active device to the specified name
    /// </summary>
    /// <param name="device"></param>
    public bool SetActiveDevice(DeviceInfo device)
    {
        try
        {
            return _deviceCyclerManager.SetAsDefault(device);
        }
        catch (Exception ex)
        {
            ErrorTriggered?.Invoke(this, new ExceptionEvent(ex));
        }

        return false;
    }

    /// <summary>
    ///     Cycles the active device to the next device. Returns true if succesfully switched (at least
    ///     as far as we can tell), returns false if could not successfully switch. Throws NoDevicesException
    ///     if there are no devices configured.
    /// </summary>
    public bool CycleActiveDevice(DataFlow type)
    {
        try
        {
            return _deviceCyclerManager.CycleDevice(type);
        }
        catch (Exception exception)
        {
            ErrorTriggered?.Invoke(this, new ExceptionEvent(exception));
        }

        return false;
    }

    #endregion
}
