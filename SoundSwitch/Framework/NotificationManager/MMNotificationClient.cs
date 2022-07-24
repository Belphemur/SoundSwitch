using System;
using System.Threading;
using System.Threading.Tasks;
using Job.Scheduler.Job;
using Job.Scheduler.Job.Action;
using Job.Scheduler.Job.Exception;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Threading;
using SoundSwitch.Model;
using PropertyKeys = NAudio.CoreAudioApi.PropertyKeys;

namespace SoundSwitch.Framework.NotificationManager
{
    public class MMNotificationClient : IMMNotificationClient, IDisposable
    {
        public static MMNotificationClient Instance { get; } = new();
        private MMDeviceEnumerator _enumerator;

        public event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged;
        public event EventHandler<DeviceChangedEventBase> DevicesChanged;
        public event EventHandler<DeviceChangedEventBase> DeviceAdded;

        private readonly TaskScheduler _taskScheduler = new LimitedConcurrencyLevelTaskScheduler(1);

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
            _enumerator = new MMDeviceEnumerator();
            _enumerator.RegisterEndpointNotificationCallback(this);
        }

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
            JobScheduler.Instance.ScheduleJob(new DeviceChangedJob(this, deviceId, newState == DeviceState.Active), CancellationToken.None, _taskScheduler);
        }

        public void OnDeviceAdded(string deviceId)
        {
            JobScheduler.Instance.ScheduleJob(new DeviceChangedJob(this, deviceId, true), CancellationToken.None, _taskScheduler);
        }

        public void OnDeviceRemoved(string deviceId)
        {
            JobScheduler.Instance.ScheduleJob(new DeviceChangedJob(this, deviceId), CancellationToken.None, _taskScheduler);
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string deviceId)
        {
            if (deviceId == null)
                return;

            JobScheduler.Instance.ScheduleJob(new DefaultDeviceChangedJob(this, deviceId, role), CancellationToken.None, _taskScheduler);
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

            JobScheduler.Instance.ScheduleJob(new DeviceChangedJob(this, pwstrDeviceId), CancellationToken.None, _taskScheduler);
        }

        public void Dispose()
        {
            _enumerator.UnregisterEndpointNotificationCallback(this);
            _enumerator?.Dispose();
        }
    }
}