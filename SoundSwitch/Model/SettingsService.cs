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

using Serilog;

using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.Telemetry;
using SoundSwitch.Framework.Theme;
using SoundSwitch.Framework.Threading;
using SoundSwitch.Framework.TrayIcon.IconDoubleClick;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.Updater.Job;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Localization;
using SoundSwitch.Localization.Factory;

namespace SoundSwitch.Model;

/// <summary>
///     Persisted application settings: a thin façade over <see cref="AppConfigs.Configuration" />
///     (the single settings store), plus the hotkey registration and the update checker.
///     Extracted from the AppModel.AppSettings partial.
/// </summary>
public class SettingsService : IAppSettings
{
    private readonly ErrorEvents _errorEvents;
    private readonly HotkeyRegistry _hotkeyRegistry;
    private IUpdateChecker _updateChecker;
    private readonly LimitedConcurrencyLevelTaskScheduler _updateScheduler;

    public SettingsService(ErrorEvents errorEvents, HotkeyRegistry hotkeyRegistry)
    {
        _errorEvents = errorEvents;
        _hotkeyRegistry = hotkeyRegistry;
        _updateScheduler = new LimitedConcurrencyLevelTaskScheduler(1);
    }

    public DeviceInfo MuteRecordingDevice
    {
        get => AppConfigs.Configuration.MuteRecordingDevice;
        set
        {
            var current = AppConfigs.Configuration.MuteRecordingDevice;
            if (current == null && value == null) return;
            if (current != null && value != null &&
                string.Equals(current.Id, value.Id, StringComparison.OrdinalIgnoreCase)) return;

            AppConfigs.Configuration.MuteRecordingDevice = value;
            AppConfigs.Configuration.Save();
            // The device service owns the mute-banner reconcile and the mute-target
            // notification; the pre-refactor setter invoked them directly, in this order.
            MuteRecordingDeviceChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool LimitMuteBannersToTarget
    {
        get => AppConfigs.Configuration.LimitMuteBannersToTarget;
        set
        {
            if (AppConfigs.Configuration.LimitMuteBannersToTarget == value) return;
            AppConfigs.Configuration.LimitMuteBannersToTarget = value;
            AppConfigs.Configuration.Save();
            LimitMuteBannersToTargetChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public IconDoubleClick IconDoubleClick
    {
        get => AppConfigs.Configuration.IconDoubleClick;
        set
        {
            AppConfigs.Configuration.IconDoubleClick = value;
            AppConfigs.Configuration.Save();
        }
    }

    /// <summary>
    ///     Beta or Stable channel.
    /// </summary>
    public bool IncludeBetaVersions
    {
        get => AppConfigs.Configuration.IncludeBetaVersions;
        set
        {
            if (value != IncludeBetaVersions && _updateChecker != null)
            {
                _updateChecker.Beta = value;
                CheckForUpdate(UpdateCheckTrigger.SettingChange);
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

    public bool KeepVolumeEnabled
    {
        get => AppConfigs.Configuration.KeepVolumeEnabled;
        set
        {
            AppConfigs.Configuration.KeepVolumeEnabled = value;
            AppConfigs.Configuration.Save();
        }
    }

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
                    CheckForUpdate(UpdateCheckTrigger.SettingChange);

                TelemetryService.TrackUpdateMode(value);
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

    public ThemeMode ThemeMode
    {
        get => AppConfigs.Configuration.ThemeMode;
        set
        {
            if (AppConfigs.Configuration.ThemeMode == value) return;
            AppConfigs.Configuration.ThemeMode = value;
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
        get => AutoStart.IsAutoStarted();
        set
        {
            Log.Information("Set AutoStart: {autostart}", value);
            if (value)
                AutoStart.EnableAutoStart();
            else
                AutoStart.DisableAutoStart();
        }
    }

    public event EventHandler<UpdateMode> UpdateModeChanged;

    /// <summary>
    ///     The update checker found a newer release than the installed version.
    /// </summary>
    public event EventHandler<NewReleaseAvailableEvent> NewVersionReleased;

    #endregion

    /// <summary>
    ///     The configured mute target changed. The device service reacts by reconciling
    ///     the persistent mute banners and notifying the mute-target followers.
    /// </summary>
    internal event EventHandler MuteRecordingDeviceChanged;

    /// <summary>
    ///     The banner-scope setting changed. The device service reacts by reconciling
    ///     the persistent mute banners.
    /// </summary>
    internal event EventHandler LimitMuteBannersToTargetChanged;

    /// <summary>
    ///     Creates and schedules the update checker. Called at the exact
    ///     pre-refactor point in the startup sequence (InitializeMain).
    /// </summary>
    internal void InitUpdateChecker()
    {
#if NIGHTLY
        const string url = "https://nightly.soundswitch.aaflalo.me/nightly/version.json";
        _updateChecker = new NightlyUpdateChecker(new Uri(url));
#else
#if DEBUG
        const string url = "https://www.aaflalo.me/api.json";
#else
        const string url = "https://api.github.com/repos/Belphemur/SoundSwitch/releases";
#endif
        _updateChecker = new UpdateChecker(new Uri(url), AppConfigs.Configuration.IncludeBetaVersions);
#endif

        _updateChecker.UpdateAvailable += (sender, @event) => NewVersionReleased?.Invoke(this,
            new NewReleaseAvailableEvent(@event.AppRelease, AppConfigs.Configuration.UpdateMode));


        JobScheduler.Instance.ScheduleJob(new CheckForUpdateRecurringJob(_updateChecker), CancellationToken.None, _updateScheduler);
        Log.Information("Update checker initiated");
    }

    /// <summary>
    /// For the app to check for update
    /// </summary>
    public void CheckForUpdate(UpdateCheckTrigger trigger = UpdateCheckTrigger.Manual)
    {
        TelemetryService.TrackUpdateCheck(trigger.ToString());
        JobScheduler.Instance.ScheduleJob(new CheckForUpdateOnceJob(_updateChecker), CancellationToken.None, _updateScheduler);
    }

    #region Hot keys

    /// <summary>
    ///     Registers the hotkeys configured at startup. Registration failure is
    ///     tolerated: the warning is logged and startup continues, exactly as the
    ///     pre-refactor InitializeMain did.
    /// </summary>
    internal void RegisterConfiguredHotkeys()
    {
        RegisterHotKey(AppConfigs.Configuration.PlaybackHotKey, HotkeyOwnerKind.PlaybackSwitch);
        if (!RegisterHotKey(AppConfigs.Configuration.RecordingHotKey, HotkeyOwnerKind.RecordingSwitch))
        {
            Log.Warning("Recording hotkey could not be registered: {hotkey}", AppConfigs.Configuration.RecordingHotKey);
        }

        if (!RegisterHotKey(AppConfigs.Configuration.MuteRecordingHotKey, HotkeyOwnerKind.Mute))
        {
            Log.Warning("Mute hotkey could not be registered: {hotkey}", AppConfigs.Configuration.MuteRecordingHotKey);
        }
    }

    public bool SetHotkeyCombination(HotKey hotKey, HotKeyAction action, bool force = false)
    {
        var confHotKey = action switch
        {
            HotKeyAction.Playback => AppConfigs.Configuration.PlaybackHotKey,
            HotKeyAction.Recording => AppConfigs.Configuration.RecordingHotKey,
            HotKeyAction.Mute => AppConfigs.Configuration.MuteRecordingHotKey,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };

        // Nothing to change: same combination and same enabled state (Equality ignores
        // Enabled, so compare it explicitly).
        if (confHotKey == hotKey && confHotKey.Enabled == hotKey.Enabled)
        {
            Log.Information("HotKey {action} already set {hotkeys}", action, confHotKey);
            return true;
        }

        Log.Information("Register new hotkeys {hotkeys}", hotKey);
        var ownerKind = action switch
        {
            HotKeyAction.Playback => HotkeyOwnerKind.PlaybackSwitch,
            HotKeyAction.Recording => HotkeyOwnerKind.RecordingSwitch,
            HotKeyAction.Mute => HotkeyOwnerKind.Mute,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
        };

        if (!RegisterHotKey(hotKey, ownerKind))
        {
            // The previous binding was never touched, so it stays in effect.
            return false;
        }

        // Unbind only the previous binding of THIS action: a combination that
        // belongs to another owner (a profile, an app rule, or another global
        // hotkey that took it first) stays registered, and one that never
        // registered (rejected at startup) unbinds nothing without failing.
        if (confHotKey.Enabled)
        {
            Log.Information("Unregister previous hotkeys {hotkeys}", confHotKey);
            if (!_hotkeyRegistry.Unregister(confHotKey, new HotkeyOwner(ownerKind)))
            {
                Log.Warning("Couldn't unregister previous hotkeys {hotkeys}, rolling back {newHotkeys}", confHotKey, hotKey);
                if (hotKey.Enabled && !_hotkeyRegistry.Unregister(hotKey, new HotkeyOwner(ownerKind)))
                    Log.Error("Couldn't roll back newly registered hotkeys {hotkeys}", hotKey);
                return false;
            }

            Log.Information("Unregistered previous hotkeys {hotkeys}", confHotKey);
        }

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

    private bool RegisterHotKey(HotKey hotkeys, HotkeyOwnerKind ownerKind)
    {
        if (!hotkeys.Enabled)
            return true;

        var conflict = _hotkeyRegistry.TryRegister(hotkeys, new HotkeyOwner(ownerKind));
        if (conflict == null)
            return true;

        Log.Warning("Can't register new hotkeys {hotkeys}: {HotkeyConflict}", hotkeys, conflict);
        _errorEvents.Raise(new Exception(conflict));
        return false;
    }

    #endregion
}
