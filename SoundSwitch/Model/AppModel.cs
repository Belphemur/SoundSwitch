/********************************************************************
 * Copyright (C) 2015 Jeroen Pelgrims
 * Copyright (C) 2015-2024 Antoine Aflalo
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
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using NAudio.CoreAudioApi;

using RailSharp;

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Audio.Lister.Job;
using SoundSwitch.Framework.Audio.Microphone;
using SoundSwitch.Framework.Banner.BannerDisplayInfo;
using SoundSwitch.Framework.Banner.BannerPosition;
using SoundSwitch.Framework.Banner.BannerPosition.Position;
using SoundSwitch.Framework.Banner.MicrophoneMute;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Framework.Threading;
using SoundSwitch.Framework.TrayIcon.IconDoubleClick;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.Updater.Job;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Localization;
using SoundSwitch.Localization.Factory;
using SoundSwitch.UI.Component;

namespace SoundSwitch.Model;

public partial class AppModel : IAppModel
{
    private bool _skipUpdate;
    private bool _initialized;
    private readonly NotificationManager _notificationManager;
    private UpdateChecker _updateChecker;
    private DeviceCollection<DeviceInfo> _selectedDevices;
    private readonly BannerPositionFactory _bannerPositionFactory = new();

    private AppModel()
    {
        _notificationManager = new NotificationManager(this, this, this);

        _deviceCyclerManager = new DeviceCyclerManager();
        _selectedDevices = null;
        _microphoneMuteToggler = new MicrophoneMuteToggler(AudioSwitcher.Instance);
        _updateScheduler = new LimitedConcurrencyLevelTaskScheduler(1);
    }

    public static IAppModel Instance { get; } = new AppModel();
    public TrayIcon TrayIcon { get; private set; }
    private CachedSound _customNotificationCachedSound;
    private readonly DeviceCyclerManager _deviceCyclerManager;
    private readonly MicrophoneMuteToggler _microphoneMuteToggler;
    private readonly LimitedConcurrencyLevelTaskScheduler _updateScheduler;

    public ProfileManager ProfileManager { get; private set; }

    public Services.AppSoundLockManager AppSoundLockManager { get; private set; }

    /// <summary>
    ///     Initialize the Main class with Updater and Hotkeys
    /// </summary>
    /// <param name="active"></param>
    public void InitializeMain(IAudioDeviceLister active, bool skipUpdate = false)
    {
        _skipUpdate = skipUpdate;
        if (_initialized)
        {
            Log.Fatal("AppModel already initialized");
            throw new InvalidOperationException("Already initialized");
        }

        AudioDeviceLister = active;
        JobScheduler.Instance.ScheduleJob(new ProcessNotificationEventsJob());
        AudioDeviceLister.DefaultDeviceChanged.Subscribe((@event) =>
            { DefaultDeviceChanged?.Invoke(this, new DeviceDefaultChangedEvent(@event.Device, @event.Role)); });

        // Subscribe to volume change events for microphones
        AudioDeviceLister.DeviceVolumeChanged
            .Where(payload =>
                // Only listen for recording devices (microphones)
                payload.Device.Type == DataFlow.Capture &&
                // Only care about mute changes
                payload.MuteChanged)
            .Subscribe(HandleMicrophoneMuteChanged);

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
            AppConfigs.Configuration.Save();

        WindowsAPIAdapter.HotKeyPressed += HandleHotkeyPress;
        WindowsAPIAdapter.SystemResumed += OnSystemResumed;

        TrayIcon = new TrayIcon();
        _notificationManager.Init();
        ProfileManager = new ProfileManager(new WindowMonitor(), AudioSwitcher.Instance, AudioDeviceLister, TrayIcon.ShowError, new TriggerFactory(), _notificationManager);

        ProfileManager
            .Init()
            .Catch(profileErrors =>
            {
                foreach (var (profile, error) in profileErrors)
                    TrayIcon.ShowError($"{profile.Name}: {error}", SettingsStrings.profile_error_title);

                return Result.Success();
            });

        AppSoundLockManager = new Services.AppSoundLockManager(AppConfigs.Configuration, AudioSwitcher.Instance, new WindowMonitor(), new ProcessMonitor(), _notificationManager);
        AppSoundLockManager.Start();

        if (!_skipUpdate)
            InitUpdateChecker();
        _initialized = true;
    }

    public IAudioDeviceLister AudioDeviceLister { get; private set; }
    public event EventHandler<ExceptionEvent> ErrorTriggered;
    public event EventHandler<NewReleaseAvailableEvent> NewVersionReleased;

    public void Dispose()
    {
        _initialized = false;
        _notificationManager?.Dispose();
        TrayIcon?.Dispose();
        AudioDeviceLister?.Dispose();
        AppSoundLockManager?.Dispose();
        WindowsAPIAdapter.SystemResumed -= OnSystemResumed;
    }

    private void OnSystemResumed(object sender, EventArgs e)
    {
        if (!_initialized || AudioDeviceLister == null)
            return;

        Task.Run(() =>
        {
            try
            {
                Log.Information("System resumed from sleep, refreshing audio devices and reconciling default device");
                AudioDeviceLister.Refresh(CancellationToken.None);
                MMNotificationClient.Instance.ReconcileDefaultDevices();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to refresh audio devices after system resume");
            }
        });
    }
}
