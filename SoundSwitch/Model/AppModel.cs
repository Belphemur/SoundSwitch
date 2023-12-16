﻿/********************************************************************
* Copyright (C) 2015 Jeroen Pelgrims
* Copyright (C) 2015-2017 Antoine Aflalo
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
using System.Threading;
using System.Threading.Tasks;
using Job.Scheduler.Job;
using Job.Scheduler.Job.Action;
using Job.Scheduler.Job.Exception;
using NAudio.CoreAudioApi;
using RailSharp;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Audio.Microphone;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Framework.Threading;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.Updater.Job;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Localization;
using SoundSwitch.Localization.Factory;
using SoundSwitch.Model.Job;
using SoundSwitch.UI.Component;

namespace SoundSwitch.Model
{
    public class AppModel : IAppModel
    {
        private bool _initialized;
        private readonly NotificationManager _notificationManager;
        private UpdateChecker _updateChecker;
        private DeviceCollection<DeviceInfo> _selectedDevices;

        private class DefaultDeviceChangedJob : IDebounceJob
        {
            private readonly AppModel _appModel;
            private readonly DeviceFullInfo _device;
            private readonly Role _role;

            public DefaultDeviceChangedJob(AppModel appModel, DeviceFullInfo device, Role role)
            {
                _appModel = appModel;
                _device = device;
                _role = role;
                Key = $"default-device-changed-{device.Type}-{role}";
            }

            public Task ExecuteAsync(CancellationToken cancellationToken)
            {
                Log.Information(@"[WINAPI] Default device changed to {device}:{role}", _device, _role);
                _appModel.DefaultDeviceChanged?.Invoke(_appModel, new DeviceDefaultChangedEvent(_device, _role, cancellationToken));
                _device.Dispose();
                return Task.CompletedTask;
            }

            public Task OnFailure(JobException exception)
            {
                return Task.CompletedTask;
            }

            public IRetryAction FailRule { get; } = new NoRetry();
            public TimeSpan? MaxRuntime { get; }
            public string Key { get; }
            public TimeSpan DebounceTime { get; } = TimeSpan.FromMilliseconds(200);
        }

        private AppModel()
        {
            _notificationManager = new NotificationManager(this);

            _deviceCyclerManager = new DeviceCyclerManager();
            _selectedDevices = null;
            MMNotificationClient.Instance.DefaultDeviceChanged += (sender, @event) => { JobScheduler.Instance.ScheduleJob(new DefaultDeviceChangedJob(this, @event.Device, @event.Role), @event.Token); };
            MMNotificationClient.Instance.DeviceAdded += (sender, @event) =>
            {
                if (!AutoAddNewDevice)
                {
                    return;
                }

                JobScheduler.Instance.ScheduleJob(new DeviceAddedJob(this, @event.DeviceId), @event.Token);
            };
            _microphoneMuteToggler = new MicrophoneMuteToggler(AudioSwitcher.Instance, _notificationManager);
            _updateScheduler = new LimitedConcurrencyLevelTaskScheduler(1);
        }

        public static IAppModel Instance { get; } = new AppModel();
        public TrayIcon TrayIcon { get; private set; }
        private CachedSound _customNotificationCachedSound;
        private readonly DeviceCyclerManager _deviceCyclerManager;
        private readonly MicrophoneMuteToggler _microphoneMuteToggler;
        private readonly LimitedConcurrencyLevelTaskScheduler _updateScheduler;

        public ProfileManager ProfileManager { get; private set; }

        public CachedSound CustomNotificationSound
        {
            get => _customNotificationCachedSound ??= new CachedSound(AppConfigs.Configuration.CustomNotificationFilePath);
            set
            {
                var oldSound = _customNotificationCachedSound;
                _customNotificationCachedSound = value;
                AppConfigs.Configuration.CustomNotificationFilePath = _customNotificationCachedSound?.FilePath;
                AppConfigs.Configuration.Save();
                CustomSoundChanged?.Invoke(this, new CustomSoundChangedEvent(oldSound, value));
            }
        }

        public NotificationTypeEnum NotificationSettings
        {
            get => AppConfigs.Configuration.NotificationSettings;
            set
            {
                AppConfigs.Configuration.NotificationSettings = value;
                AppConfigs.Configuration.Save();
                NotificationSettingsChanged?.Invoke(this,
                    new NotificationSettingsUpdatedEvent(NotificationSettings, value));
            }
        }


        /// <summary>
        /// Beta or Stable channel.
        /// </summary>
        public bool IncludeBetaVersions
        {
            get => AppConfigs.Configuration.IncludeBetaVersions;
            set
            {
                if (value != IncludeBetaVersions && _updateChecker != null)
                {
                    _updateChecker.Beta = value;
                    CheckForUpdate();
                }

                AppConfigs.Configuration.IncludeBetaVersions = value;
                AppConfigs.Configuration.Save();
            }
        }

        public bool Telemetry
        {
            get => AppConfigs.Configuration.Telemetry;
            set
            {
                AppConfigs.Configuration.Telemetry = value;
                AppConfigs.Configuration.Save();
            }
        }

        public bool QuickMenuEnabled
        {
            get => AppConfigs.Configuration.QuickMenuEnabled;
            set
            {
                AppConfigs.Configuration.QuickMenuEnabled = value;
                AppConfigs.Configuration.Save();
            }
        }


        public DeviceCollection<DeviceInfo> SelectedDevices
        {
            get
            {
                if (_selectedDevices != null)
                {
                    return _selectedDevices;
                }

                return _selectedDevices = new DeviceCollection<DeviceInfo>(AppConfigs.Configuration.SelectedDevices.OrderBy(info => info.DiscoveredAt));
            }
        }

        public IEnumerable<DeviceFullInfo> AvailablePlaybackDevices => AudioDeviceLister.GetDevices(DataFlow.Render, DeviceState.Active).IntersectWith(SelectedDevices);

        public IEnumerable<DeviceFullInfo> AvailableRecordingDevices => AudioDeviceLister.GetDevices(DataFlow.Capture, DeviceState.Active).IntersectWith(SelectedDevices);

        public bool SetCommunications
        {
            get => AppConfigs.Configuration.ChangeCommunications;
            set
            {
                AppConfigs.Configuration.ChangeCommunications = value;
                AppConfigs.Configuration.Save();
            }
        }

        public UpdateMode UpdateMode
        {
            get => AppConfigs.Configuration.UpdateMode;
            set
            {
                if (value != AppConfigs.Configuration.UpdateMode)
                {
                    if (value != UpdateMode.Never)
                    {
                        CheckForUpdate();
                    }

                    UpdateModeChanged?.Invoke(this, value);
                }

                AppConfigs.Configuration.UpdateMode = value;
                AppConfigs.Configuration.Save();
            }
        }

        public Language Language
        {
            get => AppConfigs.Configuration.Language;
            set
            {
                AppConfigs.Configuration.Language = value;
                AppConfigs.Configuration.Save();
            }
        }

        public bool SwitchForegroundProgram
        {
            get => AppConfigs.Configuration.SwitchForegroundProgram;
            set
            {
                AppConfigs.Configuration.SwitchForegroundProgram = value;
                AppConfigs.Configuration.Save();
            }
        }

        public bool NotifyUsingPrimaryScreen
        {
            get => AppConfigs.Configuration.NotifyUsingPrimaryScreen;
            set
            {
                AppConfigs.Configuration.NotifyUsingPrimaryScreen = value;
                AppConfigs.Configuration.Save();
            }
        }


        public bool AutoAddNewDevice
        {
            get => AppConfigs.Configuration.AutoAddNewConnectedDevices;
            set
            {
                AppConfigs.Configuration.AutoAddNewConnectedDevices = value;
                AppConfigs.Configuration.Save();
            }
        }

        public bool ForceSelectedProfile
        {
            get => AppConfigs.Configuration.ForceSelectedProfile;
            set
            {
                AppConfigs.Configuration.ForceSelectedProfile = value;
                AppConfigs.Configuration.Save();
            }
        }

        #region Misc settings

        /// <summary>
        ///     If the application runs at windows startup
        /// </summary>
        public bool RunAtStartup
        {
            get => AutoStart.IsAutoStarted();
            set
            {
                Log.Information("Set AutoStart: {autostart}", value);
                if (value)
                {
                    AutoStart.EnableAutoStart();
                }
                else
                {
                    AutoStart.DisableAutoStart();
                }
            }
        }

        public IAudioDeviceLister AudioDeviceLister { get; private set; }
        public event EventHandler<NotificationSettingsUpdatedEvent> NotificationSettingsChanged;
        public event EventHandler<CustomSoundChangedEvent> CustomSoundChanged;
        public event EventHandler<UpdateMode> UpdateModeChanged;

        #endregion


        /// <summary>
        ///     Initialize the Main class with Updater and Hotkeys
        /// </summary>
        /// <param name="active"></param>
        public void InitializeMain(IAudioDeviceLister active)
        {
            AudioDeviceLister = active;
            if (_initialized)
            {
                Log.Fatal("AppModel already initialized");
                throw new InvalidOperationException("Already initialized");
            }


            RegisterHotKey(AppConfigs.Configuration.PlaybackHotKey);
            var saveConfig = false;
            if (!RegisterHotKey(AppConfigs.Configuration.RecordingHotKey))
            {
                Log.Information("Disabling Recording hotkey: {hotkey}", AppConfigs.Configuration.RecordingHotKey);
                AppConfigs.Configuration.RecordingHotKey.Enabled = false;
                saveConfig = true;
            }

            if (!RegisterHotKey(AppConfigs.Configuration.MuteRecordingHotKey))
            {
                Log.Information("Disabling Mute hotkey: {hotkey}", AppConfigs.Configuration.MuteRecordingHotKey);
                AppConfigs.Configuration.MuteRecordingHotKey.Enabled = false;
                saveConfig = true;
            }

            if (!AppConfigs.Configuration.MigratedFields.Contains($"{nameof(SwitchForegroundProgram)}_cleanup")
                && AppConfigs.Configuration.MigratedFields.Contains($"{nameof(SwitchForegroundProgram)}_force_off") &&
                !AppConfigs.Configuration.SwitchForegroundProgram)
            {
                AppConfigs.Configuration.MigratedFields.Add($"{nameof(SwitchForegroundProgram)}_cleanup");
                try
                {
                    AudioSwitcher.Instance.ResetProcessDeviceConfiguration();
                }
                catch (Exception e)
                {
                    Log.Error(e, "Trying disable ProcessDevice configuration for migration");
                }

                saveConfig = true;
            }

            if (saveConfig)
            {
                AppConfigs.Configuration.Save();
            }

            WindowsAPIAdapter.HotKeyPressed += HandleHotkeyPress;

            TrayIcon = new TrayIcon();
            _notificationManager.Init();
            ProfileManager = new ProfileManager(new WindowMonitor(), AudioSwitcher.Instance, AudioDeviceLister, TrayIcon.ShowError, new TriggerFactory(), _notificationManager);

            ProfileManager
                .Init()
                .Catch(profileErrors =>
                {
                    foreach (var (profile, error) in profileErrors)
                    {
                        TrayIcon.ShowError($"{profile.Name}: {error}", SettingsStrings.profile_error_title);
                    }

                    return Result.Success();
                });

            InitUpdateChecker();
            _initialized = true;
        }


        private void InitUpdateChecker()
        {
#if DEBUG
            const string url = "https://www.aaflalo.me/api.json";
#else
            const string url = "https://api.github.com/repos/Belphemur/SoundSwitch/releases";
#endif
            _updateChecker = new UpdateChecker(new Uri(url), AppConfigs.Configuration.IncludeBetaVersions);

            _updateChecker.UpdateAvailable += (sender, @event) => NewVersionReleased?.Invoke(this,
                new NewReleaseAvailableEvent(@event.AppRelease, AppConfigs.Configuration.UpdateMode));


            JobScheduler.Instance.ScheduleJob(new CheckForUpdateRecurringJob(_updateChecker), CancellationToken.None, _updateScheduler);
            Log.Information("Update checker initiated");
        }

        /// <summary>
        /// For the app to check for update
        /// </summary>
        public void CheckForUpdate()
        {
            JobScheduler.Instance.ScheduleJob(new CheckForUpdateOnceJob(_updateChecker), CancellationToken.None, _updateScheduler);
        }

        public event EventHandler<DeviceListChanged> SelectedDeviceChanged;
        public event EventHandler<ExceptionEvent> ErrorTriggered;
        public event EventHandler<NewReleaseAvailableEvent> NewVersionReleased;

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
        /// Add the device to the Selected device list
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

            return true;
        }

        #endregion

        #region Hot keys

        public bool SetHotkeyCombination(HotKey hotKey, HotKeyAction action, bool force = false)
        {
            var confHotKey = action switch
            {
                HotKeyAction.Playback  => AppConfigs.Configuration.PlaybackHotKey,
                HotKeyAction.Recording => AppConfigs.Configuration.RecordingHotKey,
                HotKeyAction.Mute      => AppConfigs.Configuration.MuteRecordingHotKey,
                _                      => throw new ArgumentOutOfRangeException(nameof(action), action, null)
            };

            if (!force && confHotKey == hotKey)
            {
                Log.Information("HotKey {action} already set {hotkeys}", action, confHotKey);
                return true;
            }

            Log.Information("Unregister previous hotkeys {hotkeys}", confHotKey);
            WindowsAPIAdapter.UnRegisterHotKey(confHotKey);
            Log.Information("Unregistered previous hotkeys {hotkeys}", confHotKey);

            if (!RegisterHotKey(hotKey)) return false;

            Log.Information("New Hotkeys registered {hotkeys}", hotKey);

            switch (action)
            {
                case HotKeyAction.Playback:
                    AppConfigs.Configuration.PlaybackHotKey = hotKey;
                    break;
                case HotKeyAction.Recording:
                    AppConfigs.Configuration.RecordingHotKey = hotKey;
                    break;
                case HotKeyAction.Mute:
                    AppConfigs.Configuration.MuteRecordingHotKey = hotKey;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action), action, null);
            }

            AppConfigs.Configuration.Save();
            return true;
        }

        private bool RegisterHotKey(HotKey hotkeys)
        {
            if (!hotkeys.Enabled || WindowsAPIAdapter.RegisterHotKey(hotkeys))
            {
                return true;
            }

            Log.Warning("Can't register new hotkeys {hotkeys}", hotkeys);
            ErrorTriggered?.Invoke(this,
                new ExceptionEvent(new Exception("Impossible to register HotKey: " + hotkeys)));
            return false;
        }


        private void HandleHotkeyPress(object sender, WindowsAPIAdapter.KeyPressedEventArgs e)
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
                    CycleActiveDevice(DataFlow.Render);
                }
                else if (e.HotKey == AppConfigs.Configuration.RecordingHotKey)
                {
                    CycleActiveDevice(DataFlow.Capture);
                }
                else if (e.HotKey == AppConfigs.Configuration.MuteRecordingHotKey)
                {
                    if (_microphoneMuteToggler.ToggleDefaultMute() == null)
                    {
                        ErrorTriggered?.Invoke(this, new ExceptionEvent(new Exception("No mic found")));
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorTriggered?.Invoke(this, new ExceptionEvent(ex));
            }
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
        public bool CycleActiveDevice(DataFlow type)
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

        [Serializable]
        public class NoDevicesException : InvalidOperationException
        {
            public NoDevicesException() : base("No devices to select")
            {
            }
        }

        #endregion

        public void Dispose()
        {
            TrayIcon?.Dispose();
            AudioDeviceLister?.Dispose();
        }
    }
}