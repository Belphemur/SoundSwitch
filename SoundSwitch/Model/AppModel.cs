/********************************************************************
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

using Microsoft.WindowsAPICodePack.ApplicationServices;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Localization;
using SoundSwitch.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using SoundSwitch.Framework.Audio.Device;

namespace SoundSwitch.Model
{
    public class AppModel : IAppModel
    {
        private bool _initialized;
        private readonly NotificationManager _notificationManager;
        private IntervalUpdateChecker _updateChecker;

        private AppModel()
        {

            RegisterForRestart();
            RegisterRecovery();
            _notificationManager = new NotificationManager(this);

            _deviceCyclerManager = new DeviceCyclerManager();
        }

        public static IAppModel Instance { get; } = new AppModel();
        public TrayIcon TrayIcon { get; set; }
        private CachedSound _customNotificationCachedSound;
        private readonly DeviceCyclerManager _deviceCyclerManager;



        public CachedSound CustomNotificationSound
        {
            get =>
                _customNotificationCachedSound ??
                (_customNotificationCachedSound =
                    new CachedSound(AppConfigs.Configuration.CustomNotificationFilePath));
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
                }
                AppConfigs.Configuration.IncludeBetaVersions = value;
                AppConfigs.Configuration.Save();
            }
        }


        public ISet<DeviceInfo> SelectedDevices => AppConfigs.Configuration.SelectedDevices;

        public IReadOnlyCollection<DeviceFullInfo> AvailablePlaybackDevices =>
            ActiveAudioDeviceLister.PlaybackDevices.Intersect(SelectedDevices.Where(info => info.Type == DataFlow.Render)).Cast<DeviceFullInfo>().ToArray();


        public IReadOnlyCollection<DeviceFullInfo> AvailableRecordingDevices =>
            ActiveAudioDeviceLister.RecordingDevices.Intersect(SelectedDevices.Where(info => info.Type == DataFlow.Capture)).Cast<DeviceFullInfo>().ToArray();

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

        #region Misc settings

        /// <summary>
        ///     If the application runs at windows startup
        /// </summary>
        public bool RunAtStartup
        {
            get { return AutoStart.IsAutoStarted(); }
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

        public IAudioDeviceLister ActiveAudioDeviceLister { get; set; }
        public event EventHandler<NotificationSettingsUpdatedEvent> NotificationSettingsChanged;
        public event EventHandler<CustomSoundChangedEvent> CustomSoundChanged;

        #endregion



        /// <summary>
        ///     Initialize the Main class with Updater and Hotkeys
        /// </summary>
        public void InitializeMain()
        {
            if (ActiveAudioDeviceLister == null)
            {
                throw new InvalidOperationException("The devices lister are not configured");
            }
            if (_initialized)
            {
                throw new InvalidOperationException("Already initialized");
            }
            SetHotkeyCombination(AppConfigs.Configuration.PlaybackHotKeys, DataFlow.Render);
            SetHotkeyCombination(AppConfigs.Configuration.RecordingHotKeys, DataFlow.Capture);

            WindowsAPIAdapter.HotKeyPressed += HandleHotkeyPress;

            InitUpdateChecker();
            _notificationManager.Init();
            _initialized = true;
        }




        private void InitUpdateChecker()
        {
            if (AppConfigs.Configuration.UpdateMode == UpdateMode.Never)
            {
                return;
            }
#if DEBUG
            const string url = "https://www.aaflalo.me/api.json";
#else
            const string url = "https://api.github.com/repos/Belphemur/SoundSwitch/releases";
#endif
            _updateChecker = new IntervalUpdateChecker(new Uri(url),
                                                       AppConfigs.Configuration.UpdateCheckInterval,
                                                       AppConfigs.Configuration.IncludeBetaVersions);

            _updateChecker.UpdateAvailable += (sender, @event) => NewVersionReleased?.Invoke(this,
                                              new NewReleaseAvailableEvent(@event.Release, AppConfigs.Configuration.UpdateMode));
            _updateChecker.CheckForUpdate();
        }

        public event EventHandler<DeviceListChanged> SelectedDeviceChanged;
        public event EventHandler<ExceptionEvent> ErrorTriggered;
        public event EventHandler<NewReleaseAvailableEvent> NewVersionReleased;

        public event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged
        {
            add => MMNotificationClient.Instance.DefaultDeviceChanged += value;
            remove => MMNotificationClient.Instance.DefaultDeviceChanged -= value;
        }

        private void RegisterRecovery()
        {
            var settings = new RecoverySettings(new RecoveryData(SaveState, AppConfigs.Configuration), 0);
            ApplicationRestartRecoveryManager.RegisterForApplicationRecovery(settings);
            Log.Information("Recovery Registered");
        }

        private void RegisterForRestart()
        {
            var settings = new RestartSettings("/restart", RestartRestrictions.NotOnReboot);
            ApplicationRestartRecoveryManager.RegisterForApplicationRestart(settings);
            Log.Information("Restart Registered");
        }

        private int SaveState(object state)
        {

            Log.Error("Saving application state");
            var settings = (SoundSwitchConfiguration)state;
            var cancelled = ApplicationRestartRecoveryManager.ApplicationRecoveryInProgress();
            if (cancelled)
            {
                Log.Error("Recovery Cancelled");
                ApplicationRestartRecoveryManager.ApplicationRecoveryFinished(false);
                return 0;
            }
            settings.Save();
            ApplicationRestartRecoveryManager.ApplicationRecoveryFinished(true);
            Log.Error("Recovery Success");
            return 0;

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
                SelectedDevices.Add(device);
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
            var result = false;
            try
            {
                result = SelectedDevices.Remove(device);
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

        public bool SetHotkeyCombination(HotKeys hotkeys, DataFlow deviceType)
        {

            HotKeys confHotKeys = null;
            switch (deviceType)
            {
                 case DataFlow.Render:
                    confHotKeys = AppConfigs.Configuration.PlaybackHotKeys;
                    break;
                case DataFlow.Capture:
                    confHotKeys = AppConfigs.Configuration.RecordingHotKeys;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
            }

            Log.Information("Unregister previous hotkeys {hotkeys}", confHotKeys);
            WindowsAPIAdapter.UnRegisterHotKey(confHotKeys);
            Log.Information("Unregistered previous hotkeys {hotkeys}", confHotKeys);

            if (hotkeys.Enabled && !WindowsAPIAdapter.RegisterHotKey(hotkeys))
            {
                Log.Warning("Can't register new hotkeys {hotkeys}", hotkeys);
                ErrorTriggered?.Invoke(this,
                    new ExceptionEvent(new Exception("Impossible to register HotKey: " + hotkeys)));
                return false;
            }

            Log.Information("New Hotkeys registered {hotkeys}", hotkeys);

            switch (deviceType)
            {
                case DataFlow.Render:
                    AppConfigs.Configuration.PlaybackHotKeys = hotkeys;
                    break;
                case DataFlow.Capture:
                    AppConfigs.Configuration.RecordingHotKeys = hotkeys;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
            }
            AppConfigs.Configuration.Save();
            return true;

        }


        private void HandleHotkeyPress(object sender, WindowsAPIAdapter.KeyPressedEventArgs e)
        {

            if (e.HotKeys != AppConfigs.Configuration.PlaybackHotKeys && e.HotKeys != AppConfigs.Configuration.RecordingHotKeys)
            {
                Log.Debug("Not the registered Hotkeys {hotkeys}", e.HotKeys);
                return;
            }

            try
            {
                if (e.HotKeys == AppConfigs.Configuration.PlaybackHotKeys)
                {
                    CycleActiveDevice(DataFlow.Render);
                }
                else if (e.HotKeys == AppConfigs.Configuration.RecordingHotKeys)
                {
                    CycleActiveDevice(DataFlow.Capture);
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
        public bool SetActiveDevice(DeviceFullInfo device)
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
    }
}