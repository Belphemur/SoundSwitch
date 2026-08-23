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
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Framework;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.Telemetry;
using SoundSwitch.Framework.Threading;
using SoundSwitch.Framework.TrayIcon.IconDoubleClick;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.Updater.Job;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Localization;
using SoundSwitch.Localization.Factory;

namespace SoundSwitch.Model;

public partial class AppModel
{
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
                    CheckForUpdate();

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

    #endregion

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
        TelemetryService.TrackUpdateCheck("manual");
        JobScheduler.Instance.ScheduleJob(new CheckForUpdateOnceJob(_updateChecker), CancellationToken.None, _updateScheduler);
    }

    #region Hot keys

    public bool SetHotkeyCombination(HotKey hotKey, HotKeyAction action, bool force = false)
    {
        var confHotKey = action switch
        {
            HotKeyAction.Playback => AppConfigs.Configuration.PlaybackHotKey,
            HotKeyAction.Recording => AppConfigs.Configuration.RecordingHotKey,
            HotKeyAction.Mute => AppConfigs.Configuration.MuteRecordingHotKey,
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
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
            return true;

        Log.Warning("Can't register new hotkeys {hotkeys}", hotkeys);
        ErrorTriggered?.Invoke(this,
            new ExceptionEvent(new Exception("Impossible to register HotKey: " + hotkeys)));
        return false;
    }


    /// <summary>
    /// Handles hotkey press events and performs the configured action for playback, recording, or mute hotkeys.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">Key press event data whose <c>HotKey</c> is checked; unrelated hotkeys are ignored.</param>
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
                TelemetryService.AddBreadcrumb("hotkey", "PlaybackHotKey pressed");
                CycleActiveDevice(DataFlow.Render);
                TelemetryService.TrackPlaybackSwitch("hotkey");
            }
            else if (e.HotKey == AppConfigs.Configuration.RecordingHotKey)
            {
                TelemetryService.AddBreadcrumb("hotkey", "RecordingHotKey pressed");
                CycleActiveDevice(DataFlow.Capture);
                TelemetryService.TrackRecordingSwitch("hotkey");
            }
            else if (e.HotKey == AppConfigs.Configuration.MuteRecordingHotKey)
            {
                TelemetryService.AddBreadcrumb("hotkey", "MuteRecordingHotKey pressed");
                var micResult = ToggleMicrophoneMute();
                if (micResult != null)
                {
                    TelemetryService.TrackMicMute("hotkey", micResult.Value.IsMuted);
                }
            }
        }
        catch (Exception ex)
        {
            ErrorTriggered?.Invoke(this, new ExceptionEvent(ex));
        }
    }

    #endregion
}
