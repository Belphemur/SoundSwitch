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
using SoundSwitch.Audio.Manager.Interop.Enum;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio.Microphone;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Services;

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
        AudioDeviceLister?.GetDevices(EDataFlow.eRender, EDeviceState.Active).IntersectWith(SelectedDevices) ?? Enumerable.Empty<DeviceFullInfo>();

    public IEnumerable<DeviceFullInfo> AvailableRecordingDevices =>
        AudioDeviceLister?.GetDevices(EDataFlow.eCapture, EDeviceState.Active).IntersectWith(SelectedDevices) ?? Enumerable.Empty<DeviceFullInfo>();

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
        // The "only selected input" option limits mute banners. Sound and toast
        // notifications keep reporting every microphone.
        if (_notificationManager.MicrophoneMuteNotificationIsBanner && !IsMuteBannerInScope(payload.Device.Id))
            return;

        Log.Information("Microphone {DeviceName} mute state changed from {WasMuted} to {IsMuted}",
            payload.Device.NameClean, payload.WasMuted, payload.IsMuted);

        // Notify about the mute state change
        _notificationManager.NotifyMuteChanged(payload.Device.Id, payload.Device.FriendlyName, payload.IsMuted);
    }

    /// <summary>
    /// Resolves the configured mute target against the currently active devices.
    /// Endpoint ids are not stable (driver updates and re-enumeration change them), so the
    /// stored device is matched by exact id, then stable instance path, then unique name.
    /// </summary>
    /// <returns>The active device, or null when no target is configured or it is unavailable.</returns>
    private DeviceFullInfo? ResolveMuteTarget()
    {
        var stored = MuteRecordingDevice;
        if (stored == null || AudioDeviceLister == null) return null;

        var resolved = AppRuleDeviceResolver.Resolve(stored, AudioDeviceLister);
        if (resolved == null) return null;
        // Heal the stored snapshot when the endpoint id or name changed (e.g. after a driver
        // update), so the next lookup stays exact. Write the config directly to avoid
        // re-entering the banner reconcile from the property setter.
        if (!string.Equals(resolved.Id, stored.Id, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(resolved.NameClean, stored.NameClean, StringComparison.Ordinal))
        {
            Log.Information("Mute microphone {OldDevice} re-resolved to {NewDevice}", stored.NameClean, resolved.NameClean);
#pragma warning disable CS0618 // Type or member is obsolete
            AppConfigs.Configuration.MuteRecordingDevice = new DeviceInfo(resolved.Name, resolved.Id, resolved.Type, resolved.IsUsb, DateTime.UtcNow);
#pragma warning restore CS0618 // Type or member is obsolete
            AppConfigs.Configuration.Save();
        }

        return resolved;
    }

    /// <summary>
    /// Resolves the recording endpoint controlled by the mute action. Falls back to the
    /// Windows default input when no target is configured or the configured device is gone;
    /// the selection itself is kept so the device is used again once it comes back.
    /// </summary>
    private string GetConfiguredOrDefaultMuteDeviceId()
    {
        var target = ResolveMuteTarget();
        if (target != null) return target.Id;

        var stored = MuteRecordingDevice;
        if (stored != null)
        {
            Log.Warning("Configured mute microphone {DeviceName} ({DeviceId}) is unavailable; using the Windows default input",
                stored.NameClean, stored.Id);
        }

        return GetDefaultMuteDeviceId();
    }

    /// <summary>
    /// Cached id of the Windows default communication input. Caching avoids a blocking COM
    /// lookup on every microphone mute event; the cache is invalidated on default-device and
    /// device-list changes.
    /// </summary>
    private string GetDefaultMuteDeviceId()
    {
        if (_defaultMuteDeviceIdValid) return _defaultMuteDeviceId;

        using var defaultInput = AudioSwitcher.Instance.GetDefaultAudioDevice(EDataFlow.eCapture, ERole.eCommunications);
        _defaultMuteDeviceId = defaultInput?.Id;
        _defaultMuteDeviceIdValid = true;
        return _defaultMuteDeviceId;
    }

    /// <summary>
    /// Whether mute banners may be shown for the given recording endpoint.
    /// </summary>
    private bool IsMuteBannerInScope(string deviceId) =>
        IsMuteBannerInScope(deviceId, LimitMuteBannersToTarget ? GetConfiguredOrDefaultMuteDeviceId() : null);

    private bool IsMuteBannerInScope(string deviceId, string targetId)
    {
        if (!LimitMuteBannersToTarget) return true;

        return !string.IsNullOrEmpty(targetId) &&
               string.Equals(deviceId, targetId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Reconciles the persistent mute banners with the current mute target and banner scope:
    /// banners for microphones that left the scope are dismissed and in-scope microphones
    /// that are currently muted get their banner back.
    /// </summary>
    private void RefreshMicrophoneMuteBanners()
    {
        if (!_initialized || AudioDeviceLister == null) return;

        var targetId = LimitMuteBannersToTarget ? GetConfiguredOrDefaultMuteDeviceId() : null;
        var inScope = AudioDeviceLister.GetDevices(EDataFlow.eCapture, EDeviceState.Active)
            .Where(device => IsMuteBannerInScope(device.Id, targetId))
            .ToList();

        var mutedInScope = new List<(string Id, string Name)>();
        foreach (var device in inScope)
        {
            var state = _microphoneMuteToggler.GetMuteState(device.Id);
            if (state is { MuteState: true })
                mutedInScope.Add((device.Id, state.Value.Name));
        }

        _notificationManager.SyncMuteBanners(inScope.Select(device => device.Id).ToList(), mutedInScope);
    }

    #region Microphone control

    /// <summary>
    /// Toggles the configured mute microphone's state, or the Windows default input.
    /// </summary>
    /// <remarks>
    /// If no default microphone is found or the operation fails, the method invokes the <c>ErrorTriggered</c> event and logs an error; on success it logs the new mute state.
    /// </remarks>
    public (string DeviceName, bool IsMuted)? ToggleMicrophoneMute()
    {
        var result = _microphoneMuteToggler.ToggleMute(GetConfiguredOrDefaultMuteDeviceId());
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
    /// Gets the mute state of the configured mute microphone, or the Windows default input.
    /// </summary>
    /// <returns>Tuple with device name and mute state, null if no microphone found or state unavailable</returns>
    public (string DeviceName, bool IsMuted)? GetMicrophoneMuteState()
    {
        var deviceId = GetConfiguredOrDefaultMuteDeviceId();
        var result = _microphoneMuteToggler.GetMuteState(deviceId);
        if (result == null) return null;

        // CLI/JSON clients expect the stable DeviceInfo.NameClean form (duplicate-instance
        // suffixes removed), not the raw Windows friendly name.
        var deviceName = new DeviceInfo(result.Value.Name, deviceId ?? string.Empty, EDataFlow.eCapture, false, DateTime.UtcNow).NameClean;
        return (deviceName, result.Value.MuteState);
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
    /// Sets the mute state of the configured mute microphone, or the Windows default input.
    /// </summary>
    /// <param name="muteState">The desired mute state</param>
    /// <returns>Tuple with device name and mute state, null if no default microphone found or operation failed</returns>
    public (string DeviceName, bool IsMuted)? SetMicrophoneMuteState(bool muteState)
    {
        var result = _microphoneMuteToggler.SetConfiguredMuteState(muteState, GetConfiguredOrDefaultMuteDeviceId());
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
    public bool CycleActiveDevice(EDataFlow type)
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
