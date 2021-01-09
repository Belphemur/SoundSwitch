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
using RailSharp;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Localization.Factory;
using SoundSwitch.UI.Component;
using SoundSwitch.Util.Timer;
using HotKey = SoundSwitch.Framework.WinApi.Keyboard.HotKey;
using WindowsAPIAdapter = SoundSwitch.Framework.WinApi.WindowsAPIAdapter;

namespace SoundSwitch.Model
{
    public class AppModel : IAppModel
    {
        private bool _initialized;
        private readonly NotificationManager _notificationManager;
        private IntervalUpdateChecker _updateChecker;
        private readonly DebounceDispatcher _dispatcher = new DebounceDispatcher();

        private AppModel()
        {
            _notificationManager = new NotificationManager(this);

            _deviceCyclerManager                               =  new DeviceCyclerManager();
            MMNotificationClient.Instance.DefaultDeviceChanged += (sender, @event) => { _dispatcher.Debounce(250, o => { DefaultDeviceChanged?.Invoke(sender, @event); }); };
        }

        public static IAppModel Instance { get; } = new AppModel();
        public TrayIcon TrayIcon { get; set; }
        private CachedSound _customNotificationCachedSound;
        private readonly DeviceCyclerManager _deviceCyclerManager;

        public ProfileManager ProfileManager { get; private set; }

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
            ActiveAudioDeviceLister.PlaybackDevices.Where(device => SelectedDevices.FirstOrDefault(device.Equals) != null).ToArray();


        public IReadOnlyCollection<DeviceFullInfo> AvailableRecordingDevices =>
            ActiveAudioDeviceLister.RecordingDevices.Where(device => SelectedDevices.FirstOrDefault(device.Equals) != null).ToArray();

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

        public bool NotifyUsingPrimaryScreen
        {
            get => AppConfigs.Configuration.NotifyUsingPrimaryScreen;
            set
            {
                AppConfigs.Configuration.NotifyUsingPrimaryScreen = value;
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
        
        public IAudioDeviceLister ActiveUnpluggedAudioLister { get; set; }
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
            ProfileManager = new ProfileManager(new WindowMonitor(), AudioSwitcher.Instance, ActiveAudioDeviceLister, TrayIcon.ShowError, new TriggerFactory());
            RegisterHotKey(AppConfigs.Configuration.PlaybackHotKey);
            RegisterHotKey(AppConfigs.Configuration.RecordingHotKey);

            WindowsAPIAdapter.HotKeyPressed += HandleHotkeyPress;

            ProfileManager
                .Init()
                .Catch<Profile[]>(settings =>
                {
                    var profileNames = string.Join(", ", settings.Select((setting) => setting.Name));
                    TrayIcon.ShowError(string.Format(SettingsStrings.profile_error_registerHotkeys, profileNames), SettingsStrings.profile_error_registerHotkeys_title);
                    return Result.Success();
                });

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
            bool result;
            try
            {
                result = SelectedDevices.Where(device.Equals).Aggregate(true, (current, deviceInfo) => current & SelectedDevices.Remove(deviceInfo));
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

        public bool SetHotkeyCombination(HotKey hotKey, DataFlow deviceType, bool force)
        {

            HotKey confHotKey;
            switch (deviceType)
            {
                 case DataFlow.Render:
                    confHotKey = AppConfigs.Configuration.PlaybackHotKey;
                    break;
                case DataFlow.Capture:
                    confHotKey = AppConfigs.Configuration.RecordingHotKey;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
            }

            if (!force && confHotKey == hotKey)
            {
                Log.Information("HotKey already set {hotkeys}", confHotKey);
                return true;
            }

            Log.Information("Unregister previous hotkeys {hotkeys}", confHotKey);
            WindowsAPIAdapter.UnRegisterHotKey(confHotKey);
            Log.Information("Unregistered previous hotkeys {hotkeys}", confHotKey);

            if (!RegisterHotKey(hotKey)) return false;

            Log.Information("New Hotkeys registered {hotkeys}", hotKey);

            switch (deviceType)
            {
                case DataFlow.Render:
                    AppConfigs.Configuration.PlaybackHotKey = hotKey;
                    break;
                case DataFlow.Capture:
                    AppConfigs.Configuration.RecordingHotKey = hotKey;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
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

            if (e.HotKey != AppConfigs.Configuration.PlaybackHotKey && e.HotKey != AppConfigs.Configuration.RecordingHotKey)
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