/********************************************************************
* Copyright (C) 2015 Jeroen Pelgrims
* Copyright (C) 2015 Antoine Aflalo
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
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using Microsoft.WindowsAPICodePack.ApplicationServices;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.NotificationManager;

namespace SoundSwitch.Model
{
    public class AppModel : IAppModel
    {
        private bool _initialized;
        private readonly NotificationManager _notificationManager;
        private IntervalUpdateChecker _updateChecker;

        private AppModel()
        {
            using (AppLogger.Log.DebugCall())
            {
                RegisterForRestart();
                RegisterRecovery();
                _notificationManager = new NotificationManager(this);
            }
        }

        public static IAppModel Instance { get; } = new AppModel();
        public NotifyIcon NotifyIcon { get; set; }
        private CachedSound _customNotificationCachedSound;
        public CachedSound CustomNotificationSound
        {
            get {
                return _customNotificationCachedSound ??
                       (_customNotificationCachedSound =
                           new CachedSound(AppConfigs.Configuration.CustomNotificationFilePath));
            }
            set
            {
                _customNotificationCachedSound = value;
                AppConfigs.Configuration.CustomNotificationFilePath = _customNotificationCachedSound.FilePath;
                AppConfigs.Configuration.Save();
            }
        }

        public NotificationTypeEnum NotificationSettings
        {
            get { return AppConfigs.Configuration.NotificationSettings; }
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
        public bool SubscribedBetaVersions
        {
            get { return AppConfigs.Configuration.SubscribedBetaVersion; }
            set
            {
                if (value != SubscribedBetaVersions && _updateChecker != null)
                {
                    _updateChecker.Beta = value;
                }
                AppConfigs.Configuration.SubscribedBetaVersion = value;
                AppConfigs.Configuration.Save();
            }
        }

        public HashSet<string> SelectedPlaybackDevicesList => AppConfigs.Configuration.SelectedPlaybackDeviceListId;


        public ICollection<IAudioDevice> AvailablePlaybackDevices
        {
            get
            {
                return
                   ActiveAudioDeviceLister.GetPlaybackDevices()
                    .Join(SelectedPlaybackDevicesList,
                    a => a.Id,
                    selected => selected,
                    (a, selected) => a)
                    .ToList();
            }
        }

        public HashSet<string> SelectedRecordingDevicesList => AppConfigs.Configuration.SelectedRecordingDeviceListId;

        public ICollection<IAudioDevice> AvailableRecordingDevices
        {
            get
            {
                return ActiveAudioDeviceLister.GetRecordingDevices()
                  .Join(SelectedRecordingDevicesList,
                  a => a.Id,
                  selected => selected,
                  (a, selected) => a)
                  .ToList();
            }
        }

        public bool SetCommunications
        {
            get { return AppConfigs.Configuration.ChangeCommunications; }
            set
            {
                AppConfigs.Configuration.ChangeCommunications = value;
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
                using (AppLogger.Log.InfoCall())
                {
                    AppLogger.Log.Info("Set AutoStart: ", value);
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
        }

        public IAudioDeviceLister ActiveAudioDeviceLister { get; set; }
        public event EventHandler<NotificationSettingsUpdatedEvent> NotificationSettingsChanged;

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
            SetHotkeyCombination(AppConfigs.Configuration.PlaybackHotKeys, AudioDeviceType.Playback);
            SetHotkeyCombination(AppConfigs.Configuration.RecordingHotKeys, AudioDeviceType.Recording);
            /*TODO: Remove in next VERSION (3.6.6)*/
            MigrateSelectedDeviceLists();
            InitUpdateChecker();
            _notificationManager.Init();
            _initialized = true;
        }
       

        private void MigrateSelectedDeviceLists()
        {
            using (AppLogger.Log.InfoCall())
            {
                if (AppConfigs.Configuration.MigratedSelectedDeviceLists)
                {
                    AppLogger.Log.Info("Already migrated the device lists");
                    return;
                }
                var audioDeviceLister = new AudioDeviceLister(DeviceState.All);
                using (AppLogger.Log.InfoCall())
                {
                    if (AppConfigs.Configuration.SelectedRecordingDeviceList != null)
                        foreach (var audioDevice in audioDeviceLister.GetRecordingDevices()
                            .Join(AppConfigs.Configuration.SelectedRecordingDeviceList,
                                a => a.FriendlyName,
                                selected => selected,
                                (a, selected) => a))
                        {
                            SelectedRecordingDevicesList.Add(audioDevice.Id);
                            AppLogger.Log.Info("Migrating Device: ", audioDevice);
                        }
                }
                using (AppLogger.Log.InfoCall())
                {
                    if (AppConfigs.Configuration.SelectedPlaybackDeviceList != null)
                        foreach (var audioDevice in audioDeviceLister.GetPlaybackDevices()
                            .Join(AppConfigs.Configuration.SelectedPlaybackDeviceList,
                                a => a.FriendlyName,
                                selected => selected,
                                (a, selected) => a))
                        {
                            SelectedPlaybackDevicesList.Add(audioDevice.Id);
                            AppLogger.Log.Info("Migrating Device: ", audioDevice);
                        }
                }
                AppConfigs.Configuration.MigratedSelectedDeviceLists = true;
                AppConfigs.Configuration.SelectedPlaybackDeviceList = null;
                AppConfigs.Configuration.SelectedRecordingDeviceList = null;
                AppConfigs.Configuration.Save();
            }
        }

        private void InitUpdateChecker()
        {
            WindowsAPIAdapter.HotKeyPressed += HandleHotkeyPress;
            _updateChecker = new IntervalUpdateChecker(
                new Uri("https://api.github.com/repos/Belphemur/SoundSwitch/releases"),
                AppConfigs.Configuration.UpdateCheckInterval, AppConfigs.Configuration.SubscribedBetaVersion);
            _updateChecker.UpdateAvailable += (sender, @event) => NewVersionReleased?.Invoke(this, @event);
            _updateChecker.CheckForUpdate();
        }

        public event EventHandler<DeviceListChanged> SelectedDeviceChanged;
        public event EventHandler<ExceptionEvent> ErrorTriggered;
        public event EventHandler<UpdateChecker.NewReleaseEvent> NewVersionReleased;

        public event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged
        {
            add { AudioController.DeviceDefaultChanged += value; }
            remove { AudioController.DeviceDefaultChanged -= value; }
        }

        public event EventHandler<DeviceStateChangedEvent> DeviceStateChanged
        {
            add { AudioController.DeviceStateChanged += value; }
            remove { AudioController.DeviceStateChanged -= value; }
        }

        public event EventHandler<DeviceRemovedEvent> DeviceRemoved
        {
            add { AudioController.DeviceRemoved += value; }
            remove { AudioController.DeviceRemoved -= value; }
        }

        public event EventHandler<DeviceAddedEvent> DeviceAdded
        {
            add { AudioController.DeviceAdded += value; }
            remove { AudioController.DeviceAdded -= value; }
        }

        private void RegisterRecovery()
        {
            var settings = new RecoverySettings(new RecoveryData(SaveState, AppConfigs.Configuration), 0);
            ApplicationRestartRecoveryManager.RegisterForApplicationRecovery(settings);
            AppLogger.Log.Info("Recovery Registered");
        }

        private void RegisterForRestart()
        {
            var settings = new RestartSettings("/restart", RestartRestrictions.NotOnReboot);
            ApplicationRestartRecoveryManager.RegisterForApplicationRestart(settings);
            AppLogger.Log.Info("Restart Registered");
        }

        private int SaveState(object state)
        {
            using (AppLogger.Log.ErrorCall())
            {
                AppLogger.Log.Error("Saving application state");
                var settings = (SoundSwitchConfiguration)state;
                var cancelled = ApplicationRestartRecoveryManager.ApplicationRecoveryInProgress();
                if (cancelled)
                {
                    AppLogger.Log.Error("Recovery Cancelled");
                    ApplicationRestartRecoveryManager.ApplicationRecoveryFinished(false);
                    return 0;
                }
                settings.Save();
                ApplicationRestartRecoveryManager.ApplicationRecoveryFinished(true);
                AppLogger.Log.Error("Recovery Success");
                return 0;
            }
        }

        #region Selected devices

        /// <summary>
        /// Add the device to the Selected device list
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public bool SelectDevice(IAudioDevice device)
        {
            var result = false;
            DeviceListChanged eventChanged = null;
            switch (device.Type)
            {
                case AudioDeviceType.Playback:
                    result = SelectedPlaybackDevicesList.Add(device.Id);
                    eventChanged = new DeviceListChanged(SelectedPlaybackDevicesList, device.Type);
                    break;
                case AudioDeviceType.Recording:
                    result = SelectedRecordingDevicesList.Add(device.Id);
                    eventChanged = new DeviceListChanged(SelectedRecordingDevicesList, device.Type);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (result)
            {
                SelectedDeviceChanged?.Invoke(this, eventChanged);
                AppConfigs.Configuration.Save();
            }
            return result;
        }

        /// <summary>
        /// Add the device to the Selected device list
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public bool UnselectDevice(IAudioDevice device)
        {
            var result = false;
            DeviceListChanged eventChanged = null;
            switch (device.Type)
            {
                case AudioDeviceType.Playback:
                    result = SelectedPlaybackDevicesList.Remove(device.Id);
                    eventChanged = new DeviceListChanged(SelectedPlaybackDevicesList, device.Type);
                    break;
                case AudioDeviceType.Recording:
                    result = SelectedRecordingDevicesList.Remove(device.Id);
                    eventChanged = new DeviceListChanged(SelectedRecordingDevicesList, device.Type);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (result)
            {
                SelectedDeviceChanged?.Invoke(this, eventChanged);
                AppConfigs.Configuration.Save();
            }
            return result;
        }

        #endregion

        #region Hot keys

        public bool SetHotkeyCombination(HotKeys hotkeys, AudioDeviceType deviceType)
        {
            using (AppLogger.Log.InfoCall())
            {
                HotKeys confHotKeys = null;
                switch (deviceType)
                {
                    case AudioDeviceType.Playback:
                        confHotKeys = AppConfigs.Configuration.PlaybackHotKeys;
                        break;
                    case AudioDeviceType.Recording:
                        confHotKeys = AppConfigs.Configuration.RecordingHotKeys;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
                }
                AppLogger.Log.Info("Unregister previous hotkeys", confHotKeys);
                WindowsAPIAdapter.UnRegisterHotKey(confHotKeys);

                if (!WindowsAPIAdapter.RegisterHotKey(hotkeys))
                {
                    AppLogger.Log.Warn("Can't register new hotkeys", hotkeys);
                    ErrorTriggered?.Invoke(this, new ExceptionEvent(new Exception("Impossible to register HotKey: " + hotkeys)));
                    return false;
                }

                AppLogger.Log.Info("New Hotkeys registered", hotkeys);
                switch (deviceType)
                {
                    case AudioDeviceType.Playback:
                        AppConfigs.Configuration.PlaybackHotKeys = hotkeys;
                        break;
                    case AudioDeviceType.Recording:
                        AppConfigs.Configuration.RecordingHotKeys = hotkeys;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
                }
                AppConfigs.Configuration.Save();
                return true;
            }
        }


        private void HandleHotkeyPress(object sender, WindowsAPIAdapter.KeyPressedEventArgs e)
        {
            using (AppLogger.Log.DebugCall())
            {
                if (e.HotKeys != AppConfigs.Configuration.PlaybackHotKeys && e.HotKeys != AppConfigs.Configuration.RecordingHotKeys)
                {
                    AppLogger.Log.Debug("Not the registered Hotkeys", e.HotKeys);
                    return;
                }

                try
                {
                    if (e.HotKeys == AppConfigs.Configuration.PlaybackHotKeys)
                    {
                        CycleActiveDevice(AudioDeviceType.Playback);
                    }
                    else if (e.HotKeys == AppConfigs.Configuration.RecordingHotKeys)
                    {
                        CycleActiveDevice(AudioDeviceType.Recording);
                    }
                }
                catch (Exception ex)
                {
                    ErrorTriggered?.Invoke(this, new ExceptionEvent(ex));
                }
            }
        }

        #endregion

        #region Active device

        /// <summary>
        ///     Attempts to set active device to the specified name
        /// </summary>
        /// <param name="device"></param>
        public bool SetActiveDevice(IAudioDevice device)
        {
            using (AppLogger.Log.InfoCall())
            {
                try
                {
                    AppLogger.Log.Info("Set Default device", device);
                    if (!SetCommunications)
                    {
                        device.SetAsDefault(Role.Console);
                        device.SetAsDefault(Role.Multimedia);
                    }
                    else
                    {
                        AppLogger.Log.Info("Set Default Communication device", device);
                        device.SetAsDefault(Role.All);
                    }
                    switch (device.Type)
                    {
                        case AudioDeviceType.Playback:
                            AppConfigs.Configuration.LastPlaybackActiveId = device.Id;
                            break;
                        case AudioDeviceType.Recording:
                            AppConfigs.Configuration.LastRecordingActiveId = device.Id;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    AppConfigs.Configuration.Save();
                    return true;
                }
                catch (Exception ex)
                {
                    ErrorTriggered?.Invoke(this, new ExceptionEvent(ex));
                }
                return false;
            }
        }

        /// <summary>
        ///     Cycles the active device to the next device. Returns true if succesfully switched (at least
        ///     as far as we can tell), returns false if could not successfully switch. Throws NoDevicesException
        ///     if there are no devices configured.
        /// </summary>
        public bool CycleActiveDevice(AudioDeviceType type)
        {
            using (AppLogger.Log.InfoCall())
            {
                ICollection<IAudioDevice> list = null;
                string lastActive = null;
                switch (type)
                {
                    case AudioDeviceType.Playback:
                        list = AvailablePlaybackDevices;
                        lastActive = AppConfigs.Configuration.LastPlaybackActiveId;
                        break;
                    case AudioDeviceType.Recording:
                        list = AvailableRecordingDevices;
                        lastActive = AppConfigs.Configuration.LastRecordingActiveId;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }

                switch (list.Count)
                {
                    case 0:
                        ErrorTriggered?.Invoke(this, new ExceptionEvent(new NoDevicesException()));
                        return false;
                    case 1:
                        return false;
                }
                AppLogger.Log.Info("Cycle Audio Devices", list);
                var defaultDev = list.FirstOrDefault(device => device.Id == lastActive) ?? list.FirstOrDefault(device => device.IsDefault(Role.Console)) ?? list.ElementAt(0);
                var next = list.SkipWhile((device, i) => !Equals(device, defaultDev)).Skip(1).FirstOrDefault() ?? list.ElementAt(0);
                AppLogger.Log.Info("Select AudioDevice", next);
                return SetActiveDevice(next);
            }
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