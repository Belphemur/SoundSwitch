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

        public event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged;
        /// <summary>
        /// Register the notification client in the Enumerator
        /// </summary>
        public void Register()
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                enumerator.RegisterEndpointNotificationCallback(this);
            }
        }

        /// <summary>
        /// Unregister the notification client in the Enumerator
        /// </summary>
        public void UnRegister()
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                enumerator.UnregisterEndpointNotificationCallback(this);
            }
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            //ignored
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
            //ignored
        }

        public void OnDeviceRemoved(string deviceId)
        {
            //ignored
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            using (var enumerator = new MMDeviceEnumerator())
            {
                var device = enumerator.GetDevice(defaultDeviceId);
                DefaultDeviceChanged?.Invoke(this, new DeviceDefaultChangedEvent(device, (DeviceRole)role));
            }
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            //ignored
        }
    }
}