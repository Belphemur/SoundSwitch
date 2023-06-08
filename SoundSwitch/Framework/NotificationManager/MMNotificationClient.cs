using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CoreAudio;
using Job.Scheduler.Job;
using Job.Scheduler.Job.Action;
using Job.Scheduler.Job.Exception;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Threading;
using SoundSwitch.Model;
using PropertyKeys = NAudio.CoreAudioApi.PropertyKeys;

namespace SoundSwitch.Framework.NotificationManager
{
    public class MMNotificationClient : IDisposable
    {
        private record struct DeviceRole(DataFlow Flow, Role Role);

        public static MMNotificationClient Instance { get; } = new();

        private readonly Dictionary<DeviceRole, string> _lastRoleDevice = new();

        public event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged;
        public event EventHandler<DeviceChangedEventBase> DevicesChanged;
        public event EventHandler<DeviceChangedEventBase> DeviceAdded;

        private readonly TaskScheduler _taskScheduler = new LimitedConcurrencyLevelTaskScheduler(1);

        private CoreAudio.MMNotificationClient _notificationClient;
        private MMDeviceEnumerator _mmDeviceEnumerator;

        private class DeviceChangedJob : IJob
        {
            private readonly MMNotificationClient _notificationClient;
            private readonly string _deviceId;
            private readonly bool _deviceAdded;

            public DeviceChangedJob(MMNotificationClient notificationClient, string deviceId, bool deviceAdded = false)
            {
                _notificationClient = notificationClient;
                _deviceId = deviceId;
                _deviceAdded = deviceAdded;
            }

            public Task ExecuteAsync(CancellationToken cancellationToken)
            {
                _notificationClient.DevicesChanged?.Invoke(_notificationClient, new DeviceChangedEventBase(_deviceId, cancellationToken));

                if (_deviceAdded)
                {
                    _notificationClient.DeviceAdded?.Invoke(_notificationClient, new DeviceChangedEventBase(_deviceId, cancellationToken));
                }

                return Task.CompletedTask;
            }

            public Task OnFailure(JobException exception)
            {
                Log.ForContext<DeviceChangedJob>().Warning(exception, "Can't notify about {device} changing", _deviceId);
                return Task.CompletedTask;
            }

            public IRetryAction FailRule { get; } = new NoRetry();
            public TimeSpan? MaxRuntime => null;
        }

        private class DefaultDeviceChangedJob : IJob
        {
            private readonly MMNotificationClient _notificationClient;
            private readonly string _deviceId;
            private readonly Role _role;

            public DefaultDeviceChangedJob(MMNotificationClient notificationClient, string deviceId, Role role)
            {
                _notificationClient = notificationClient;
                _deviceId = deviceId;
                _role = role;
            }

            public Task ExecuteAsync(CancellationToken cancellationToken)
            {
                var mmDevice = AudioSwitcher.Instance.GetDevice(_deviceId);
                var device = AudioSwitcher.Instance.InteractWithMmDevice(mmDevice, device => new DeviceFullInfo(device));
                _notificationClient.DefaultDeviceChanged?.Invoke(_notificationClient, new DeviceDefaultChangedEvent(device, _role, cancellationToken));
                return Task.CompletedTask;
            }

            public Task OnFailure(JobException exception)
            {
                Log.ForContext<DeviceChangedJob>().Warning(exception, "Can't notify about {device} changing", _deviceId);
                return Task.CompletedTask;
            }

            public IRetryAction FailRule { get; } = new NoRetry();
            public TimeSpan? MaxRuntime => null;
        }

        /// <summary>
        /// Register the notification client in the Enumerator
        /// </summary>
        public void Register()
        {
            _mmDeviceEnumerator = new MMDeviceEnumerator(Guid.NewGuid());
            _notificationClient = new CoreAudio.MMNotificationClient(_mmDeviceEnumerator);
            _notificationClient.DeviceAdded += OnDeviceAdded;
            _notificationClient.DeviceRemoved += OnDeviceRemoved;
            _notificationClient.DevicePropertyChanged += OnPropertyValueChanged;
            _notificationClient.DeviceStateChanged += OnDeviceStateChanged;
            _notificationClient.DefaultDeviceChanged += OnDefaultDeviceChanged;
            foreach (var flow in Enum.GetValues<DataFlow>().Where(flow => flow != DataFlow.All))
            {
                foreach (var role in Enum.GetValues<Role>().Where(role => role != Role.EnumCount))
                {
                    using var device = AudioSwitcher.Instance.GetDefaultAudioEndpoint((EDataFlow)flow, (ERole)role);
                    if (device == null)
                    {
                        continue;
                    }
                    _lastRoleDevice[new DeviceRole(flow, role)] = device.Id;
                }
            }
        }

        public void OnDeviceStateChanged(object sender, DeviceStateChangedEventArgs deviceStateChangedEventArgs)
        {
            JobScheduler.Instance.ScheduleJob(new DeviceChangedJob(this, deviceStateChangedEventArgs.DeviceId), CancellationToken.None, _taskScheduler);
        }

        public void OnDeviceAdded(object sender, DeviceNotificationEventArgs deviceNotificationEventArgs)
        {
            JobScheduler.Instance.ScheduleJob(new DeviceChangedJob(this, deviceNotificationEventArgs.DeviceId, true), CancellationToken.None, _taskScheduler);
        }

        public void OnDeviceRemoved(object sender, DeviceNotificationEventArgs deviceNotificationEventArgs)
        {
            JobScheduler.Instance.ScheduleJob(new DeviceChangedJob(this, deviceNotificationEventArgs.DeviceId), CancellationToken.None, _taskScheduler);
        }

        public void OnDefaultDeviceChanged(object sender, DefaultDeviceChangedEventArgs defaultDeviceChangedEventArgs)
        {
            var deviceId = defaultDeviceChangedEventArgs.DeviceId;
            var flow = defaultDeviceChangedEventArgs.DataFlow;
            var role = defaultDeviceChangedEventArgs.Role;

            var deviceRole = new DeviceRole(flow, role);
            if (_lastRoleDevice.TryGetValue(deviceRole, out var oldDeviceId) && oldDeviceId == deviceId)
            {
                return;
            }

            _lastRoleDevice[deviceRole] = deviceId;
            JobScheduler.Instance.ScheduleJob(new DefaultDeviceChangedJob(this, deviceId, role), CancellationToken.None, _taskScheduler);
        }

        public void OnPropertyValueChanged(object sender, DevicePropertyChangedEventArgs devicePropertyChangedEventArgs)
        {
            var key = devicePropertyChangedEventArgs.PropertyKey;
            if (PropertyKeys.PKEY_DeviceInterface_FriendlyName.formatId != key.fmtId
                && PropertyKeys.PKEY_AudioEndpoint_GUID.formatId != key.fmtId
                && PropertyKeys.PKEY_Device_IconPath.formatId != key.fmtId
                && PropertyKeys.PKEY_Device_FriendlyName.formatId != key.fmtId
               )
            {
                return;
            }

            JobScheduler.Instance.ScheduleJob(new DeviceChangedJob(this, devicePropertyChangedEventArgs.DeviceId), CancellationToken.None, _taskScheduler);
        }

        public void Dispose()
        {
            _notificationClient.DeviceAdded -= OnDeviceAdded;
            _notificationClient.DeviceRemoved -= OnDeviceRemoved;
            _notificationClient.DevicePropertyChanged -= OnPropertyValueChanged;
            _notificationClient.DeviceStateChanged -= OnDeviceStateChanged;
            _notificationClient.DefaultDeviceChanged -= OnDefaultDeviceChanged;
            _notificationClient = null;
            _mmDeviceEnumerator = null;
        }
    }
}