#nullable enable
using System;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface;

namespace SoundSwitch.Audio.Manager
{
    public sealed class DeviceStateChangedEventArgs : EventArgs
    {
        public DeviceStateChangedEventArgs(string deviceId, EDeviceState state)
        {
            DeviceId = deviceId;
            State = state;
        }

        public string DeviceId { get; }
        public EDeviceState State { get; }
    }

    public sealed class DeviceNotificationEventArgs : EventArgs
    {
        public DeviceNotificationEventArgs(string deviceId)
        {
            DeviceId = deviceId;
        }

        public string DeviceId { get; }
    }

    public sealed class DefaultDeviceChangedEventArgs : EventArgs
    {
        public DefaultDeviceChangedEventArgs(EDataFlow flow, ERole role, string? deviceId)
        {
            Flow = flow;
            Role = role;
            DeviceId = deviceId;
        }

        public EDataFlow Flow { get; }
        public ERole Role { get; }

        /// <summary>Null when Windows reports there is no longer a default device for the role.</summary>
        public string? DeviceId { get; }
    }

    public sealed class DevicePropertyChangedEventArgs : EventArgs
    {
        public DevicePropertyChangedEventArgs(string deviceId, PROPERTYKEY propertyKey)
        {
            DeviceId = deviceId;
            PropertyKey = propertyKey;
        }

        public string DeviceId { get; }
        public PROPERTYKEY PropertyKey { get; }
    }

    /// <summary>
    /// Managed implementation of <see cref="IMMNotificationClient"/> — the in-house replacement
    /// for the legacy third-party MMDeviceNotificationClient. The COM vtable is exactly the five notification
    /// callbacks (plus the three implicit IUnknown slots); SoundSwitch implements all five and
    /// nothing more.
    ///
    /// This is a pure managed object: it is safe to construct and subscribe on any thread.
    /// Register/unregister through <see cref="AudioSwitcher.RegisterNotificationClient"/> /
    /// <see cref="AudioSwitcher.UnregisterNotificationClient"/>, which marshal the operation onto
    /// the ComThread. Keep the instance rooted for as long as it is registered.
    ///
    /// The five events are raised on an audio-service thread: handlers must not block and must not
    /// touch UI state directly.
    /// </summary>
    public sealed class AudioDeviceNotificationClient : IMMNotificationClient
    {
        public event EventHandler<DeviceStateChangedEventArgs>? DeviceStateChanged;
        public event EventHandler<DeviceNotificationEventArgs>? DeviceAdded;
        public event EventHandler<DeviceNotificationEventArgs>? DeviceRemoved;
        public event EventHandler<DefaultDeviceChangedEventArgs>? DefaultDeviceChanged;
        public event EventHandler<DevicePropertyChangedEventArgs>? PropertyValueChanged;

        public void OnDeviceStateChanged(string deviceId, EDeviceState newState) =>
            DeviceStateChanged?.Invoke(this, new DeviceStateChangedEventArgs(deviceId, newState));

        public void OnDeviceAdded(string deviceId) =>
            DeviceAdded?.Invoke(this, new DeviceNotificationEventArgs(deviceId));

        public void OnDeviceRemoved(string deviceId) =>
            DeviceRemoved?.Invoke(this, new DeviceNotificationEventArgs(deviceId));

        public void OnDefaultDeviceChanged(EDataFlow flow, ERole role, string? defaultDeviceId) =>
            DefaultDeviceChanged?.Invoke(this, new DefaultDeviceChangedEventArgs(flow, role, defaultDeviceId));

        public void OnPropertyValueChanged(string deviceId, PROPERTYKEY key) =>
            PropertyValueChanged?.Invoke(this, new DevicePropertyChangedEventArgs(deviceId, key));
    }
}
