using System;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using Serilog;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.NotificationManager
{
    public class MMNotificationClient : IMMNotificationClient, IDisposable
    {
        public static MMNotificationClient Instance { get; } = new();
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

        private Task StartInTask(Action action, string eventName)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Issue while processing {event}", eventName);
                }
            });
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            StartInTask(() => DevicesChanged?.Invoke(this, new DeviceChangedEventBase(deviceId)), nameof(OnDeviceStateChanged));
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
            StartInTask(() => DevicesChanged?.Invoke(this, new DeviceChangedEventBase(pwstrDeviceId)), nameof(OnDeviceAdded));
        }

        public void OnDeviceRemoved(string deviceId)
        {
            StartInTask(() => DevicesChanged?.Invoke(this, new DeviceChangedEventBase(deviceId)), nameof(OnDeviceRemoved));
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            if (defaultDeviceId == null)
                return;

            StartInTask(() =>
            {
                var device = _enumerator.GetDevice(defaultDeviceId);
                DefaultDeviceChanged?.Invoke(this, new DeviceDefaultChangedEvent(device, role));
            }, nameof(OnDefaultDeviceChanged));
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            StartInTask(() =>
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
            }, nameof(OnPropertyValueChanged));
        }

        public void Dispose()
        {
            _enumerator.UnregisterEndpointNotificationCallback(this);
            _enumerator?.Dispose();
        }
    }
}