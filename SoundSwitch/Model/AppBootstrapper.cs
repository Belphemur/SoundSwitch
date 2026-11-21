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
using System.Threading;
using System.Threading.Tasks;

using RailSharp;

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Audio.Lister;
using SoundSwitch.Framework.Audio.Lister.Job;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.Banner.MicrophoneMute;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Framework.Threading;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Localization;
using SoundSwitch.Services;
using SoundSwitch.UI.Component;
using SoundSwitch.UI.Menu;

namespace SoundSwitch.Model;

/// <summary>
///     Owns the application's ordered startup sequence (ADR
///     docs/design/architecture/2026-11-21-appmodel-decomposition-di.md §C): the steps
///     the pre-refactor InitializeMain and SoundSwitchApplicationContext constructor
///     encoded as comments, enforced here by statement order. The tray-adjacent
///     managers are resolved at the exact statements where they used to be
///     constructed, so the construction timing is unchanged.
/// </summary>
public sealed class AppBootstrapper : IDisposable
{
    private readonly IAudioDeviceLister _audioDeviceLister;
    private readonly DeviceService _deviceService;
    private readonly SettingsService _settingsService;
    private readonly NotificationManager _notificationManager;
    private readonly IAppInfrastructure _infrastructure;
    private readonly Lazy<TrayIcon> _trayIcon;
    private readonly Lazy<ProfileManager> _profileManager;
    private readonly Lazy<AppSoundLockManager> _appSoundLockManager;

    private bool _initialized;

    public AppBootstrapper(IAudioDeviceLister audioDeviceLister,
        DeviceService deviceService,
        SettingsService settingsService,
        NotificationManager notificationManager,
        IAppInfrastructure infrastructure,
        Lazy<TrayIcon> trayIcon,
        Lazy<ProfileManager> profileManager,
        Lazy<AppSoundLockManager> appSoundLockManager)
    {
        _audioDeviceLister = audioDeviceLister;
        _deviceService = deviceService;
        _settingsService = settingsService;
        _notificationManager = notificationManager;
        _infrastructure = infrastructure;
        _trayIcon = trayIcon;
        _profileManager = profileManager;
        _appSoundLockManager = appSoundLockManager;

        // Composition wiring for the mute-notification path: the device service resolves
        // the banner scope, the notification manager applies its banner-mode and readiness
        // guards. Same guard-then-log-then-notify order as the pre-refactor direct calls,
        // on the same thread (synchronous event dispatch), and — because this runs before
        // the device lister is subscribed — a mute event arriving during startup still
        // hits the notification manager's not-ready-yet guard and is skipped with its
        // warning, as before.
        _deviceService.MuteNotificationPending += (_, e) =>
        {
            if (_notificationManager.MicrophoneMuteNotificationIsBanner && !e.IsBannerInScope)
                return;

            Log.Information("Microphone {DeviceName} mute state changed from {WasMuted} to {IsMuted}",
                e.MicrophoneName, e.WasMuted, e.IsMuted);

            // Notify about the mute state change
            _notificationManager.NotifyMuteChanged(e.DeviceId, e.MicrophoneName, e.IsMuted);
        };
        _deviceService.MuteBannersSyncRequested += (_, _) =>
            _notificationManager.SyncMuteBanners(_deviceService.GetInScopeMutedMicrophones);
    }

    /// <summary>
    ///     Runs the ordered startup sequence and returns the application context to
    ///     hand to <c>Application.Run</c>.
    /// </summary>
    /// <param name="skipUpdate">Skips starting the update checker (CLI/portable runs).</param>
    public SoundSwitchApplicationContext Run(bool skipUpdate)
    {
        if (_initialized)
        {
            Log.Fatal("Application startup sequence already initialized");
            throw new InvalidOperationException("Already initialized");
        }

        // Banner and quick-menu managers capture the UI-thread synchronization context;
        // this runs on the UI thread right before anything can display a banner,
        // including the startup mute-banner reconcile at the end of this sequence.
        BannerManager.Setup();
        MicrophoneMuteBannerManager.Setup();
        QuickMenuManager<DeviceFullInfo>.Instance.Setup();
        QuickMenuManager<Profile>.Instance.Setup();

        // The cached lister must see real devices before the notification client
        // registers, so the first reconcile observes the live set.
        _audioDeviceLister.Refresh(CancellationToken.None);
        MMNotificationClient.Instance.Register();

        // === The former InitializeMain, same statement order ===
        JobScheduler.Instance.ScheduleJob(new ProcessNotificationEventsJob(_audioDeviceLister));
        _deviceService.SubscribeAudioDeviceEvents();

        _settingsService.RegisterConfiguredHotkeys();

        var saveConfig = false;

        if (!AppConfigs.Configuration.MigratedFields.Contains($"{nameof(SettingsService.SwitchForegroundProgram)}_cleanup")
            && AppConfigs.Configuration.MigratedFields.Contains($"{nameof(SettingsService.SwitchForegroundProgram)}_force_off") &&
            !AppConfigs.Configuration.SwitchForegroundProgram)
        {
            AppConfigs.Configuration.MigratedFields.Add($"{nameof(SettingsService.SwitchForegroundProgram)}_cleanup");
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

        var trayIcon = _trayIcon.Value;
        _notificationManager.Init();
        var profileManager = _profileManager.Value;

        profileManager
            .Init()
            .Catch(profileErrors =>
            {
                foreach (var (profile, error) in profileErrors)
                    trayIcon.ShowError($"{profile.Name}: {error}", SettingsStrings.profile_error_title);

                return Result.Success();
            });

        var appSoundLockManager = _appSoundLockManager.Value;
        appSoundLockManager.Start();

        // Device arrival/removal changes which persisted route resolves: reconciled assignment
        // rules keep working, disabled ones restore their default (identity ADR §6.6).
        _audioDeviceLister.DeviceListRefreshed.Subscribe(_ => appSoundLockManager.OnDevicesChanged());

        if (!skipUpdate)
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

        return new SoundSwitchApplicationContext(_deviceService, _settingsService, _infrastructure);
    }

    public void Dispose()
    {
        _deviceService?.MarkDeinitialized();
        WindowsAPIAdapter.SystemResumed -= OnSystemResumed;
    }

    private void OnSystemResumed(object sender, EventArgs e)
    {
        if (!_initialized)
            return;

        Task.Run(() =>
        {
            try
            {
                Log.Information("System resumed from sleep, refreshing audio devices and reconciling default device");
                _audioDeviceLister.Refresh(CancellationToken.None);
                MMNotificationClient.Instance.ReconcileDefaultDevices();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to refresh audio devices after system resume");
            }
        });
    }
}
