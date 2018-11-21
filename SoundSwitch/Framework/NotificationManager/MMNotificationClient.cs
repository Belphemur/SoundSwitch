using System;
using AudioDefaultSwitcherWrapper;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.NotificationManager
{
    public class MMNotificationClient : IMMNotificationClient
    {
        public static MMNotificationClient Instance { get; } = new MMNotificationClient();
        private MMDeviceEnumerator _enumerator;

        public event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged;
        public event EventHandler<DeviceChangedEventBase> DevicesChanged;

        /// <summary>
        /// Register the notification client in the Enumerator
        /// </summary>
        public void Register()
        {
            _enumerator = new MMDeviceEnumerator();
            _enumerator.RegisterEndpointNotificationCallback(this);
        }

        /// <summary>
        /// Unregister the notification client in the Enumerator
        /// </summary>
        public void UnRegister()
        {
            using (_enumerator)
            {
                _enumerator.UnregisterEndpointNotificationCallback(this);
            }
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            DevicesChanged?.Invoke(this, new DeviceChangedEventBase(deviceId));
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
            DevicesChanged?.Invoke(this, new DeviceChangedEventBase(pwstrDeviceId));
        }

        public void OnDeviceRemoved(string deviceId)
        {
            DevicesChanged?.Invoke(this, new DeviceChangedEventBase(deviceId));
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            var device = _enumerator.GetDevice(defaultDeviceId);
            DefaultDeviceChanged?.Invoke(this, new DeviceDefaultChangedEvent(device, (DeviceRole) role));
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            DevicesChanged?.Invoke(this, new DeviceChangedEventBase(pwstrDeviceId));
        }
    }
}