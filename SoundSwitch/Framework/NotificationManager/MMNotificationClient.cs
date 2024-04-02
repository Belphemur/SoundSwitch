using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Model;
using PropertyKeys = NAudio.CoreAudioApi.PropertyKeys;

namespace SoundSwitch.Framework.NotificationManager
{
    public class MMNotificationClient : IMMNotificationClient, IDisposable
    {
        private record struct DeviceRole(DataFlow Flow, Role Role);

        public static MMNotificationClient Instance { get; } = new();
        private MMDeviceEnumerator _enumerator;

        private readonly Dictionary<DeviceRole, string> _lastRoleDevice = new();

        private readonly ConcurrentQueue<DeviceChangedEvent> _deviceChangedEvents = new();

        public event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged;
        public event EventHandler<DeviceChangedEventBase> DevicesChanged;
        public event EventHandler<DeviceChangedEventBase> DeviceAdded;

        /// <summary>
        /// Get the last events and clear the queue of events
        /// </summary>
        /// <returns></returns>
        public IEnumerable<DeviceChangedEvent> GetLastEvents()
        {
            if (_deviceChangedEvents.IsEmpty)
                return ArraySegment<DeviceChangedEvent>.Empty;

            var events = new SortedSet<DeviceChangedEvent>();
            while (_deviceChangedEvents.TryDequeue(out var deviceChangedEvent))
            {
                events.Add(deviceChangedEvent);
            }

            return events;
        }

        /// <summary>
        /// Register the notification client in the Enumerator
        /// </summary>
        public void Register()
        {
            _enumerator = new MMDeviceEnumerator();
            _enumerator.RegisterEndpointNotificationCallback(this);
            foreach (var flow in Enum.GetValues<DataFlow>().Where(flow => flow != DataFlow.All))
            {
                foreach (var role in Enum.GetValues<Role>())
                {
                    var device = AudioSwitcher.Instance.GetDefaultAudioEndpoint((EDataFlow)flow, (ERole)role);
                    if (device == null)
                        continue;

                    _lastRoleDevice[new DeviceRole(flow, role)] = device.Id;
                }
            }
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            _deviceChangedEvents.Enqueue(new DeviceChangedEvent(EventType.StateChanged, deviceId));
        }

        public void OnDeviceAdded(string deviceId)
        {
            _deviceChangedEvents.Enqueue(new DeviceChangedEvent(EventType.Added, deviceId));
        }

        public void OnDeviceRemoved(string deviceId)
        {
            _deviceChangedEvents.Enqueue(new DeviceChangedEvent(EventType.Removed, deviceId));
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string deviceId)
        {
            if (deviceId == null)
                return;

            var deviceRole = new DeviceRole(flow, role);
            if (_lastRoleDevice.TryGetValue(deviceRole, out var oldDeviceId) && oldDeviceId == deviceId)
            {
                return;
            }

            _lastRoleDevice[deviceRole] = deviceId;
            _deviceChangedEvents.Enqueue(new DeviceChangedEvent(EventType.DefaultChanged, deviceId));
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
            if (PropertyKeys.PKEY_DeviceInterface_FriendlyName.formatId != key.formatId
                && PropertyKeys.PKEY_AudioEndpoint_GUID.formatId != key.formatId
                && PropertyKeys.PKEY_Device_IconPath.formatId != key.formatId
                && PropertyKeys.PKEY_Device_FriendlyName.formatId != key.formatId
               )
            {
                return;
            }

            _deviceChangedEvents.Enqueue(new DeviceChangedEvent(EventType.PropertyChanged, pwstrDeviceId));
        }

        public void Dispose()
        {
            _enumerator.UnregisterEndpointNotificationCallback(this);
            _enumerator?.Dispose();
        }
    }
}