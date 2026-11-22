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
using System.Reactive.Linq;

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio.Microphone;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.Telemetry;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Services;

namespace SoundSwitch.Model;

/// <summary>
///     Audio device operations: selection, cycling, microphone mute control, the
///     mute-target resolution chain and the persistent mute-banner reconcile.
///     Extracted from the AppModel.DeviceService and AppModel.AppSettings partials;
///     owns the state previously shared across them.
/// </summary>
public class DeviceService : IDeviceService
{
    private readonly ErrorEvents _errorEvents;
    private readonly IAudioDeviceLister _audioDeviceLister;
    private readonly DeviceCyclerManager _deviceCyclerManager;
    private readonly MicrophoneMuteToggler _microphoneMuteToggler;

    private bool _initialized;
    private DeviceCollection<DeviceInfo> _selectedDevices;

    // The default capture endpoint is resolved with a COM call; microphone mute events are
    // frequent, so cache it and invalidate whenever the default or the device list changes.
    // Invalidation and publication share this lock so a lookup that started before an event
    // cannot publish the former default after the cache was invalidated.
    private readonly object _defaultMuteDeviceIdLock = new();
    private string _defaultMuteDeviceId;
    private bool _defaultMuteDeviceIdValid;

    public DeviceService(IAudioDeviceLister audioDeviceLister,
        ErrorEvents errorEvents,
        IAppSettings appSettings,
        SettingsService settingsService,
        NotificationSettingsService notificationSettingsService)
    {
        _audioDeviceLister = audioDeviceLister;
        _errorEvents = errorEvents;
        _deviceCyclerManager = new DeviceCyclerManager(appSettings, this, _audioDeviceLister);
        _microphoneMuteToggler = new MicrophoneMuteToggler(AudioSwitcher.Instance);

        // The settings services are built before the device service (they are usable before
        // the startup sequence runs) and ask it — the reconcile owner — to react to
        // mute-banner-relevant changes. Same reaction points and order as the
        // pre-refactor setters.
        settingsService.MuteRecordingDeviceChanged += (_, _) =>
        {
            RefreshMicrophoneMuteBanners();
            MuteTargetChanged?.Invoke(this, EventArgs.Empty);
        };
        settingsService.LimitMuteBannersToTargetChanged += (_, _) => RefreshMicrophoneMuteBanners();
        notificationSettingsService.MuteBannerReconciliationRequested += (_, _) => RefreshMicrophoneMuteBanners();
    }

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
        _audioDeviceLister.GetDevices(EDataFlow.eRender, EDeviceState.Active).IntersectWith(SelectedDevices);

    public IEnumerable<DeviceFullInfo> AvailableRecordingDevices =>
        _audioDeviceLister.GetDevices(EDataFlow.eCapture, EDeviceState.Active).IntersectWith(SelectedDevices);

    public event EventHandler<DeviceListChanged> SelectedDeviceChanged;
    public event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged;
    public event EventHandler<MicrophoneMuteChangedEventArgs> DefaultMicMuteChanged;
    public event EventHandler MuteTargetChanged;

    /// <summary>
    ///     A microphone mute change is pending notification. The composition root wires
    ///     this to the notification manager, which applies the banner-mode and
    ///     readiness guards and raises the notification — preserving the pre-refactor
    ///     guard-then-log-then-notify order on the same thread.
    /// </summary>
    internal event EventHandler<MuteNotificationRequestEventArgs> MuteNotificationPending;

    /// <summary>
    ///     The persistent mute banners need a reconcile. The composition root wires this
    ///     to the notification manager's <c>SyncMuteBanners</c>, pairing it with this
    ///     service's scoped-microphone reader.
    /// </summary>
    internal event EventHandler MuteBannersSyncRequested;

    /// <summary>
    ///     Subscribes the device lister's event streams. Called at the exact
    ///     pre-refactor point of the startup sequence (InitializeMain).
    /// </summary>
    internal void SubscribeAudioDeviceEvents()
    {
        _audioDeviceLister.DefaultDeviceChanged.Subscribe((@event) =>
        {
            if (@event.Role == ERole.eCommunications && @event.Device.Type == EDataFlow.eCapture)
            {
                lock (_defaultMuteDeviceIdLock)
                    _defaultMuteDeviceIdValid = false;
            }

            // Refresh on every communications-input change: with a configured target that is
            // available the result is unchanged, and when it is unavailable the mute action falls
            // back to the default input, whose banners must follow the new default.
            if (_initialized && AppConfigs.Configuration.LimitMuteBannersToTarget &&
                @event.Role == ERole.eCommunications && @event.Device.Type == EDataFlow.eCapture)
                RefreshMicrophoneMuteBanners();

            DefaultDeviceChanged?.Invoke(this, new DeviceDefaultChangedEvent(@event.Device, @event.Role));
        });

        // Device arrival/removal changes which microphone the configured target resolves to
        // (and whether it resolves at all), so reconcile the persistent mute banners too.
        _audioDeviceLister.DeviceListRefreshed.Subscribe(_ =>
        {
            lock (_defaultMuteDeviceIdLock)
                _defaultMuteDeviceIdValid = false;
            if (_initialized)
                RefreshMicrophoneMuteBanners();
            // Arrival/removal can also change which microphone the mute actions target;
            // the mute-state tray icon re-reads the live state to follow it.
            MuteTargetChanged?.Invoke(this, EventArgs.Empty);
        });

        // Subscribe to volume change events for microphones
        _audioDeviceLister.DeviceVolumeChanged
            .Where(payload =>
                // Only listen for recording devices (microphones)
                payload.Device.Type == EDataFlow.eCapture &&
                // Only care about mute changes
                payload.MuteChanged)
            .Subscribe(HandleMicrophoneMuteChanged);
    }

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
        var muteTargetId = GetConfiguredOrDefaultMuteDeviceId();

        // The mute-state tray icon keys off the tracked microphone. Raise independently of
        // the banner scope below so every tracked-mic transition repaints the icon.
        if (IsTrackedMuteDevice(payload.Device.Id, muteTargetId))
        {
            DefaultMicMuteChanged?.Invoke(this, new MicrophoneMuteChangedEventArgs(payload.Device.Id, payload.Device.NameClean, payload.IsMuted));
        }

        // The "only selected input" option limits mute banners; sound and toast
        // notifications keep reporting every microphone. The scope verdict is resolved
        // here and the notification manager applies it together with its banner-mode
        // check — the same two conditions the pre-refactor guard evaluated.
        MuteNotificationPending?.Invoke(this, new MuteNotificationRequestEventArgs(
            payload.Device.Id,
            GetMicrophoneBannerName(payload.Device),
            payload.WasMuted,
            payload.IsMuted,
            IsMuteBannerInScope(payload.Device.Id, muteTargetId)));
    }

    /// <summary>
    ///     A microphone mute change to be rendered as a notification, with the
    ///     banner-scope verdict already resolved by the device service.
    /// </summary>
    internal sealed class MuteNotificationRequestEventArgs(string deviceId, string microphoneName, bool wasMuted, bool isMuted, bool isBannerInScope) : EventArgs
    {
        public string DeviceId { get; } = deviceId;
        public string MicrophoneName { get; } = microphoneName;
        public bool WasMuted { get; } = wasMuted;
        public bool IsMuted { get; } = isMuted;
        public bool IsBannerInScope { get; } = isBannerInScope;
    }

    internal static string GetMicrophoneBannerName(DeviceInfo device)
    {
        // FriendlyName is populated when NameClean parses the endpoint name.
        _ = device.NameClean;
        return device.FriendlyName;
    }

    /// <summary>
    /// Resolves the configured mute target against the currently active devices.
    /// Endpoint ids are not stable (driver updates and re-enumeration change them), so the
    /// stored device is matched by exact id, then stable instance path, then unique name.
    /// </summary>
    /// <returns>The active device, or null when no target is configured or it is unavailable.</returns>
    private DeviceFullInfo? ResolveMuteTarget()
    {
        var stored = AppConfigs.Configuration.MuteRecordingDevice;
        if (stored == null || _audioDeviceLister == null) return null;

        var resolved = DeviceResolver.Resolve(stored, _audioDeviceLister);
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

        var stored = AppConfigs.Configuration.MuteRecordingDevice;
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
        lock (_defaultMuteDeviceIdLock)
        {
            if (_defaultMuteDeviceIdValid) return _defaultMuteDeviceId;

            using var defaultInput = AudioSwitcher.Instance.GetDefaultAudioDevice(EDataFlow.eCapture, ERole.eCommunications);
            _defaultMuteDeviceId = defaultInput?.Id;
            _defaultMuteDeviceIdValid = true;
            return _defaultMuteDeviceId;
        }
    }

    /// <summary>
    /// Whether mute banners may be shown for the given recording endpoint.
    /// </summary>
    private bool IsMuteBannerInScope(string deviceId, string targetId)
    {
        if (!AppConfigs.Configuration.LimitMuteBannersToTarget) return true;

        return IsTrackedMuteDevice(deviceId, targetId);
    }

    /// <summary>
    /// Whether the given endpoint is the resolved mute target. Endpoint ids are matched
    /// case-insensitively, like every other device-id resolution.
    /// </summary>
    internal static bool IsTrackedMuteDevice(string deviceId, string targetId) =>
        !string.IsNullOrEmpty(targetId) &&
        string.Equals(deviceId, targetId, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Reconciles the persistent mute banners with the current mute target and banner scope:
    /// banners for microphones that left the scope are dismissed and in-scope microphones
    /// whose current state uses Persistent get their banner back.
    /// </summary>
    internal void RefreshMicrophoneMuteBanners()
    {
        if (!_initialized) return;

        MuteBannersSyncRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    ///     The in-scope microphones and their current mute states. Read when the UI thread
    ///     applies a banner reconcile — same reads, comment and ordering rationale as the
    ///     pre-refactor reconcile lambda.
    /// </summary>
    internal IReadOnlyCollection<(string Id, string Name, bool IsMuted)> GetInScopeMutedMicrophones()
    {
        var targetId = AppConfigs.Configuration.LimitMuteBannersToTarget ? GetConfiguredOrDefaultMuteDeviceId() : null;
        // Read the lister's cached states on the UI thread when applying the refresh.
        // Capturing them before dispatch could overwrite a newer live mute event.
        return _audioDeviceLister.GetDevices(EDataFlow.eCapture, EDeviceState.Active)
            .Where(device => IsMuteBannerInScope(device.Id, targetId))
            .Select(device => (device.Id, GetMicrophoneBannerName(device), device.IsMuted))
            .ToList();
    }

    #region Microphone control

    /// <summary>
    /// Toggles the configured mute microphone's state, or the Windows default input.
    /// </summary>
    /// <remarks>
    /// If no default microphone is found or the operation fails, the method raises the application error event and logs an error; on success it logs the new mute state.
    /// </remarks>
    public (string DeviceName, bool IsMuted)? ToggleMicrophoneMute()
    {
        var result = _microphoneMuteToggler.ToggleMute(GetConfiguredOrDefaultMuteDeviceId());
        if (result == null)
        {
            _errorEvents.Raise(new Exception("No mic found or unable to toggle mute state"));
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
            _errorEvents.Raise(new Exception("No mic found or unable to toggle mute state"));
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
            _errorEvents.Raise(new Exception("No mic found or unable to set mute state"));
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
            _errorEvents.Raise(ex);
        }

        return false;
    }

    /// <summary>
    ///     Cycles the active device to the next device. Returns true if succesfully switched (at least
    /// as far as we can tell), returns false if could not successfully switch. Throws NoDevicesException
    /// if there are no devices configured.
    /// </summary>
    public bool CycleActiveDevice(EDataFlow type)
    {
        try
        {
            return _deviceCyclerManager.CycleDevice(type);
        }
        catch (Exception exception)
        {
            _errorEvents.Raise(exception);
        }

        return false;
    }

    #endregion

    /// <summary>
    /// Marks the startup sequence complete: mute-banner reconciles that were suppressed
    /// during startup now run. Flipped at the exact pre-refactor point of InitializeMain.
    /// </summary>
    internal void MarkInitialized() => _initialized = true;

    /// <summary>
    /// Marks the model disposed: mute-banner reconciles are suppressed again, as before
    /// the refactor. Flipped by the façade's Dispose at the exact pre-refactor point.
    /// </summary>
    internal void MarkDeinitialized() => _initialized = false;

    /// <summary>
    /// Handles hotkey press events and performs the configured action for playback, recording, or mute hotkeys.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Key press event data whose <c>HotKey</c> is checked; unrelated hotkeys are ignored.</param>
    internal void HandleHotkeyPress(object sender, WindowsAPIAdapter.KeyPressedEventArgs e)
    {
        if (e.HotKey != AppConfigs.Configuration.PlaybackHotKey
            && e.HotKey != AppConfigs.Configuration.RecordingHotKey
            && e.HotKey != AppConfigs.Configuration.MuteRecordingHotKey)
        {
            Log.Debug("Not the registered Hotkeys {hotkeys}", e.HotKey);
            return;
        }

        try
        {
            if (e.HotKey == AppConfigs.Configuration.PlaybackHotKey)
            {
                TelemetryService.AddBreadcrumb("hotkey", "PlaybackHotKey pressed");
                CycleActiveDevice(EDataFlow.eRender);
                TelemetryService.TrackPlaybackSwitch("hotkey");
            }
            else if (e.HotKey == AppConfigs.Configuration.RecordingHotKey)
            {
                TelemetryService.AddBreadcrumb("hotkey", "RecordingHotKey pressed");
                CycleActiveDevice(EDataFlow.eCapture);
                TelemetryService.TrackRecordingSwitch("hotkey");
            }
            else if (e.HotKey == AppConfigs.Configuration.MuteRecordingHotKey)
            {
                Log.Information("Mute hotkey pressed: {hotkey}", e.HotKey);
                TelemetryService.AddBreadcrumb("hotkey", "MuteRecordingHotKey pressed");
                var micResult = ToggleMicrophoneMute();
                if (micResult != null)
                {
                    TelemetryService.TrackMicMute("hotkey", micResult.Value.IsMuted);
                }
            }
        }
        catch (Exception ex)
        {
            _errorEvents.Raise(ex);
        }
    }
}
