using System;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.NotificationManager
{
    public class MMNotificationClient : IMMNotificationClient, IDisposable
    {
        public static MMNotificationClient Instance { get; } = new ();
        private       MMDeviceEnumerator   _enumerator;

        public event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged;
        public event EventHandler<DeviceChangedEventBase>    DevicesChanged;

        /// <summary>
        /// Register the notification client in the Enumerator
        /// </summary>
        public void Register()
        {
            _enumerator = new MMDeviceEnumerator();
            _enumerator.RegisterEndpointNotificationCallback(this);
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            Task.Factory.StartNew(() => { DevicesChanged?.Invoke(this, new DeviceChangedEventBase(deviceId)); });
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
            Task.Factory.StartNew(() => { DevicesChanged?.Invoke(this, new DeviceChangedEventBase(pwstrDeviceId)); });
        }

        public void OnDeviceRemoved(string deviceId)
        {
            Task.Factory.StartNew(() => { DevicesChanged?.Invoke(this, new DeviceChangedEventBase(deviceId)); });
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            if (defaultDeviceId == null)
                return;

            Task.Factory.StartNew(() =>
            {
                var device = _enumerator.GetDevice(defaultDeviceId);
                DefaultDeviceChanged?.Invoke(this, new DeviceDefaultChangedEvent(device, role));
            });
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            Task.Factory.StartNew(() =>
            {
                if (PropertyKeys.PKEY_DeviceInterface_FriendlyName.formatId != key.formatId
                    && PropertyKeys.PKEY_AudioEndpoint_GUID.formatId != key.formatId
                    && PropertyKeys.PKEY_Device_IconPath.formatId != key.formatId
                    && PropertyKeys.PKEY_Device_FriendlyName.formatId != key.formatId
                )
                {
                    return;
                }

                DevicesChanged?.Invoke(this, new DeviceChangedEventBase(pwstrDeviceId));
            });
        }

        public void Dispose()
        {
            _enumerator.UnregisterEndpointNotificationCallback(this);
            _enumerator?.Dispose();
        }
    }
}