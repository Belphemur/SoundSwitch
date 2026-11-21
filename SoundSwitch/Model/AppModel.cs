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
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

using RailSharp;

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Audio.Lister.Job;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.Banner.BannerDisplayInfo;
using SoundSwitch.Framework.Banner.BannerPosition;
using SoundSwitch.Framework.Banner.BannerPosition.Position;
using SoundSwitch.Framework.Banner.MicrophoneMute;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Framework.Telemetry;
using SoundSwitch.Framework.Theme;
using SoundSwitch.Framework.Threading;
using SoundSwitch.Framework.TrayIcon.IconDoubleClick;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Localization;
using SoundSwitch.Localization.Factory;
using SoundSwitch.UI.Component;

namespace SoundSwitch.Model;

/// <summary>
///     Thin delegating façade over the decomposed model services (ADR
///     docs/design/architecture/2026-11-21-appmodel-decomposition-di.md): the four
///     sub-interfaces are now implemented by <see cref="DeviceService" />,
///     <see cref="NotificationSettingsService" /> and <see cref="SettingsService" />,
///     with the application error event owned by <see cref="ErrorEvents" />. Exists so
///     the <see cref="Instance" /> call sites keep compiling while they migrate to
///     constructor injection; it is deleted with them.
/// </summary>
public class AppModel : IAppModel
{
    private bool _skipUpdate;
    private bool _initialized;

    private readonly ErrorEvents _errorEvents;
    private readonly SettingsService _settingsService;
    private readonly NotificationSettingsService _notificationSettingsService;
    private readonly NotificationManager _notificationManager;
    private DeviceService _deviceService;

    private AppModel()
    {
        _errorEvents = new ErrorEvents();
        _settingsService = new SettingsService(_errorEvents);
        _notificationSettingsService = new NotificationSettingsService();
        // Same construction point as the pre-refactor model: the notification manager is
        // wired to the settings service and to this façade, which forwards the device and
        // infrastructure members until InitializeMain constructs the device service and the
        // tray-adjacent managers.
        _notificationManager = new NotificationManager(_notificationSettingsService, this, this);
    }

    public static IAppModel Instance { get; } = new AppModel();

    /// <summary>
    ///     The device service; available once <see cref="InitializeMain" /> ran, exactly
    ///     like the pre-refactor device members became usable with the lister.
    /// </summary>
    private DeviceService DeviceService =>
        _deviceService ?? throw new InvalidOperationException("InitializeMain must run before the device service is available");

    #region Infrastructure state (assigned during InitializeMain, as before)

    public TrayIcon TrayIcon { get; private set; }

    public ProfileManager ProfileManager { get; private set; }

    public Services.AppSoundLockManager AppSoundLockManager { get; private set; }

    public IAudioDeviceLister AudioDeviceLister { get; private set; }

    public event EventHandler<ExceptionEvent> ErrorTriggered
    {
        add => _errorEvents.ErrorTriggered += value;
        remove => _errorEvents.ErrorTriggered -= value;
    }

    #endregion

    #region IDeviceService forwarding

    public DeviceCollection<DeviceInfo> SelectedDevices => DeviceService.SelectedDevices;

    public IEnumerable<DeviceFullInfo> AvailablePlaybackDevices => DeviceService.AvailablePlaybackDevices;

    public IEnumerable<DeviceFullInfo> AvailableRecordingDevices => DeviceService.AvailableRecordingDevices;

    public event EventHandler<DeviceListChanged> SelectedDeviceChanged
    {
        add => DeviceService.SelectedDeviceChanged += value;
        remove => DeviceService.SelectedDeviceChanged -= value;
    }

    public event EventHandler<DeviceDefaultChangedEvent> DefaultDeviceChanged
    {
        add => DeviceService.DefaultDeviceChanged += value;
        remove => DeviceService.DefaultDeviceChanged -= value;
    }

    public event EventHandler<MicrophoneMuteChangedEventArgs> DefaultMicMuteChanged
    {
        add => DeviceService.DefaultMicMuteChanged += value;
        remove => DeviceService.DefaultMicMuteChanged -= value;
    }

    public event EventHandler MuteTargetChanged
    {
        add => DeviceService.MuteTargetChanged += value;
        remove => DeviceService.MuteTargetChanged -= value;
    }

    public bool SelectDevice(DeviceFullInfo device) => DeviceService.SelectDevice(device);

    public bool UnselectDevice(DeviceFullInfo device) => DeviceService.UnselectDevice(device);

    public bool SetActiveDevice(DeviceInfo device) => DeviceService.SetActiveDevice(device);

    public bool CycleActiveDevice(EDataFlow type) => DeviceService.CycleActiveDevice(type);

    public (string DeviceName, bool IsMuted)? ToggleMicrophoneMute() => DeviceService.ToggleMicrophoneMute();

    public (string DeviceName, bool IsMuted)? GetMicrophoneMuteState() => DeviceService.GetMicrophoneMuteState();

    public (string DeviceName, bool IsMuted)? SetMicrophoneMuteState(bool muteState) => DeviceService.SetMicrophoneMuteState(muteState);

    public (string DeviceName, bool IsMuted)? SetMicrophoneMuteState(string deviceId, bool muteState) => DeviceService.SetMicrophoneMuteState(deviceId, muteState);

    #endregion

    #region INotificationSettings forwarding

    public const int BannerScaleMinimum = NotificationSettingsService.BannerScaleMinimum;
    public const int BannerScaleMaximum = NotificationSettingsService.BannerScaleMaximum;

    public int BannerOnScreenTimeSecs
    {
        get => _notificationSettingsService.BannerOnScreenTimeSecs;
        set => _notificationSettingsService.BannerOnScreenTimeSecs = value;
    }

    public TimeSpan BannerOnScreenTime
    {
        get => _notificationSettingsService.BannerOnScreenTime;
        set => _notificationSettingsService.BannerOnScreenTime = value;
    }

    public int BannerOpacityPercentage
    {
        get => _notificationSettingsService.BannerOpacityPercentage;
        set => _notificationSettingsService.BannerOpacityPercentage = value;
    }

    public int MaxNumberNotification
    {
        get => _notificationSettingsService.MaxNumberNotification;
        set => _notificationSettingsService.MaxNumberNotification = value;
    }

    public bool IsSingleNotification
    {
        get => _notificationSettingsService.IsSingleNotification;
        set => _notificationSettingsService.IsSingleNotification = value;
    }

    public CachedSound CustomNotificationSound
    {
        get => _notificationSettingsService.CustomNotificationSound;
        set => _notificationSettingsService.CustomNotificationSound = value;
    }

    public BannerDisplayInfo BannerDisplayInfo
    {
        get => _notificationSettingsService.BannerDisplayInfo;
        set => _notificationSettingsService.BannerDisplayInfo = value;
    }

    public bool NotificationAdvancedMode
    {
        get => _notificationSettingsService.NotificationAdvancedMode;
        set => _notificationSettingsService.NotificationAdvancedMode = value;
    }

    public NotificationType SwitchDeviceNotification
    {
        get => _notificationSettingsService.SwitchDeviceNotification;
        set => _notificationSettingsService.SwitchDeviceNotification = value;
    }

    public NotificationType SwitchProfileNotification
    {
        get => _notificationSettingsService.SwitchProfileNotification;
        set => _notificationSettingsService.SwitchProfileNotification = value;
    }

    public NotificationType MicrophoneMuteNotification
    {
        get => _notificationSettingsService.MicrophoneMuteNotification;
        set => _notificationSettingsService.MicrophoneMuteNotification = value;
    }

    public BannerPosition BannerPosition
    {
        get => _notificationSettingsService.BannerPosition;
        set => _notificationSettingsService.BannerPosition = value;
    }

    public Point CustomBannerPosition
    {
        get => _notificationSettingsService.CustomBannerPosition;
        set => _notificationSettingsService.CustomBannerPosition = value;
    }

    public bool HasCustomBannerPosition => _notificationSettingsService.HasCustomBannerPosition;

    public IPosition BannerPositionImpl => _notificationSettingsService.BannerPositionImpl;

    public MicrophoneMute MicrophoneMuteBanner
    {
        get => _notificationSettingsService.MicrophoneMuteBanner;
        set => _notificationSettingsService.MicrophoneMuteBanner = value;
    }

    public MicrophoneMute MicrophoneUnmuteBanner
    {
        get => _notificationSettingsService.MicrophoneUnmuteBanner;
        set => _notificationSettingsService.MicrophoneUnmuteBanner = value;
    }

    public bool NotifyUsingPrimaryScreen
    {
        get => _notificationSettingsService.NotifyUsingPrimaryScreen;
        set => _notificationSettingsService.NotifyUsingPrimaryScreen = value;
    }

    public string BannerMonitorDeviceName
    {
        get => _notificationSettingsService.BannerMonitorDeviceName;
        set => _notificationSettingsService.BannerMonitorDeviceName = value;
    }

    public Color BannerBackgroundColor
    {
        get => _notificationSettingsService.BannerBackgroundColor;
        set => _notificationSettingsService.BannerBackgroundColor = value;
    }

    public BannerStyle BannerStyle
    {
        get => _notificationSettingsService.BannerStyle;
        set => _notificationSettingsService.BannerStyle = value;
    }

    public int BannerScalePercentage
    {
        get => _notificationSettingsService.BannerScalePercentage;
        set => _notificationSettingsService.BannerScalePercentage = value;
    }

    public void SetBannerMonitor(string deviceName, bool usePrimaryScreen) =>
        _notificationSettingsService.SetBannerMonitor(deviceName, usePrimaryScreen);

    public event EventHandler<NotificationSettingsUpdatedEvent> NotificationSettingsChanged
    {
        add => _notificationSettingsService.NotificationSettingsChanged += value;
        remove => _notificationSettingsService.NotificationSettingsChanged -= value;
    }

    public event EventHandler<BannerDataChangedEvent> BannerSettingsChanged
    {
        add => _notificationSettingsService.BannerSettingsChanged += value;
        remove => _notificationSettingsService.BannerSettingsChanged -= value;
    }

    public event EventHandler<CustomSoundChangedEvent> CustomSoundChanged
    {
        add => _notificationSettingsService.CustomSoundChanged += value;
        remove => _notificationSettingsService.CustomSoundChanged -= value;
    }

    #endregion

    #region IAppSettings forwarding

    public DeviceInfo MuteRecordingDevice
    {
        get => _settingsService.MuteRecordingDevice;
        set => _settingsService.MuteRecordingDevice = value;
    }

    public bool LimitMuteBannersToTarget
    {
        get => _settingsService.LimitMuteBannersToTarget;
        set => _settingsService.LimitMuteBannersToTarget = value;
    }

    public IconDoubleClick IconDoubleClick
    {
        get => _settingsService.IconDoubleClick;
        set => _settingsService.IconDoubleClick = value;
    }

    public bool IncludeBetaVersions
    {
        get => _settingsService.IncludeBetaVersions;
        set => _settingsService.IncludeBetaVersions = value;
    }

    public bool Telemetry
    {
        get => _settingsService.Telemetry;
        set => _settingsService.Telemetry = value;
    }

    public bool QuickMenuEnabled
    {
        get => _settingsService.QuickMenuEnabled;
        set => _settingsService.QuickMenuEnabled = value;
    }

    public bool KeepVolumeEnabled
    {
        get => _settingsService.KeepVolumeEnabled;
        set => _settingsService.KeepVolumeEnabled = value;
    }

    public bool SetCommunications
    {
        get => _settingsService.SetCommunications;
        set => _settingsService.SetCommunications = value;
    }

    public UpdateMode UpdateMode
    {
        get => _settingsService.UpdateMode;
        set => _settingsService.UpdateMode = value;
    }

    public Language Language
    {
        get => _settingsService.Language;
        set => _settingsService.Language = value;
    }

    public ThemeMode ThemeMode
    {
        get => _settingsService.ThemeMode;
        set => _settingsService.ThemeMode = value;
    }

    public bool SwitchForegroundProgram
    {
        get => _settingsService.SwitchForegroundProgram;
        set => _settingsService.SwitchForegroundProgram = value;
    }

    public bool RunAtStartup
    {
        get => _settingsService.RunAtStartup;
        set => _settingsService.RunAtStartup = value;
    }

    public bool SetHotkeyCombination(HotKey hotKey, HotKeyAction action, bool force = false) =>
        _settingsService.SetHotkeyCombination(hotKey, action, force);

    public void CheckForUpdate(UpdateCheckTrigger trigger = UpdateCheckTrigger.Manual) =>
        _settingsService.CheckForUpdate(trigger);

    public event EventHandler<UpdateMode> UpdateModeChanged
    {
        add => _settingsService.UpdateModeChanged += value;
        remove => _settingsService.UpdateModeChanged -= value;
    }

    public event EventHandler<NewReleaseAvailableEvent> NewVersionReleased
    {
        add => _settingsService.NewVersionReleased += value;
        remove => _settingsService.NewVersionReleased -= value;
    }

    #endregion

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
        _deviceService = new DeviceService(active, _errorEvents, _notificationManager, _settingsService, _notificationSettingsService);
        JobScheduler.Instance.ScheduleJob(new ProcessNotificationEventsJob());
        _deviceService.SubscribeAudioDeviceEvents();

        _settingsService.RegisterConfiguredHotkeys();

        var saveConfig = false;

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

        WindowsAPIAdapter.HotKeyPressed += _deviceService.HandleHotkeyPress;
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

        // Device arrival/removal changes which persisted route resolves: reconciled assignment
        // rules keep working, disabled ones restore their default (identity ADR §6.6).
        AudioDeviceLister.DeviceListRefreshed.Subscribe(_ => AppSoundLockManager.OnDevicesChanged());

        if (!_skipUpdate)
            _settingsService.InitUpdateChecker();
        _initialized = true;
        _deviceService.MarkInitialized();

        // A mute event that arrived while the mute notification was still uninitialized
        // (NotifyMuteChanged) was skipped, and RefreshMicrophoneMuteBanners early-returned
        // until now. Reconcile once so a microphone muted during startup gets its
        // persistent banner back. Banner loss is cosmetic, so never let a reconcile
        // failure abort initialization.
        try
        {
            _deviceService.RefreshMicrophoneMuteBanners();
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to reconcile microphone mute banners during startup");
        }
    }

    public void Dispose()
    {
        _initialized = false;
        _deviceService?.MarkDeinitialized();
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
