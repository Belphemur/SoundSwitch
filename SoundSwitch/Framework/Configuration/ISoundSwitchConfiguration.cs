/********************************************************************
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
using System.Drawing;

using JetBrains.Annotations;

using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Banner.BannerDisplayInfo;
using SoundSwitch.Framework.Banner.BannerPosition;
using SoundSwitch.Framework.Banner.MicrophoneMute;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.TrayIcon.IconChanger;
using SoundSwitch.Framework.TrayIcon.IconDoubleClick;
using SoundSwitch.Framework.TrayIcon.TooltipInfoManager;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.Updater.Remind;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Localization.Factory;

namespace SoundSwitch.Framework.Configuration;

public interface ISoundSwitchConfiguration : IConfiguration
{

    [Obsolete]
    HashSet<string> SelectedPlaybackDeviceListId { get; }
    [Obsolete]
    HashSet<string> SelectedRecordingDeviceListId { get; }

    HashSet<DeviceInfo> SelectedDevices { get; set; }

    bool FirstRun { get; set; }
    HotKey PlaybackHotKey { get; set; }
    HotKey RecordingHotKey { get; set; }
    bool ChangeCommunications { get; set; }
    uint UpdateCheckInterval { get; set; }

    NotificationType SwitchDeviceNotification { get; set; }
    NotificationType SwitchProfileNotification { get; set; }
    NotificationType MicrophoneMuteNotification { get; set; }
    BannerPosition BannerPosition { get; set; }
    Point CustomBannerPosition { get; set; }

    /// <summary>
    /// Show a banner when microphone is muted
    /// </summary>
    MicrophoneMute MicrophoneMuteBanner { get; set; }
    MicrophoneMute MicrophoneUnmuteBanner { get; set; }
    bool IncludeBetaVersions { get; set; }
    string CustomNotificationFilePath { get; set; }
    UpdateMode UpdateMode { get; set; }
    Language Language { get; set; }
    TooltipInfoType TooltipInfo { get; set; }
    DeviceCyclerType CyclerType { get; set; }
    bool KeepSystrayIcon { get; set; }

    bool SwitchForegroundProgram { get; set; }
    bool NotifyUsingPrimaryScreen { get; set; }

    bool AutoAddNewConnectedDevices { get; set; }

    /// <summary>
    /// What to do with the TrayIcon when changing default device
    /// </summary>
    IconChanger SwitchIcon { get; set; }

    IconDoubleClick IconDoubleClick { get; set; }

    [Obsolete]
    HashSet<ProfileSetting> ProfileSettings { get; set; }
    HashSet<Profile.Profile> Profiles { get; set; }
    DateTime LastDonationNagTime { get; set; }
    TimeSpan TimeBetweenDonateNag { get; set; }
    HotKey MuteRecordingHotKey { get; set; }

    /// <summary>
    /// Unique ID assigned at installation
    /// </summary>
    Guid UniqueInstallationId { get; set; }

    [CanBeNull]
    ReleasePostponed Postponed { get; set; }

    /// <summary>
    /// Fields of the config that got migrated
    /// </summary>
    HashSet<string> MigratedFields { get; }

    /// <summary>
    /// Is telemetry enabled
    /// </summary>
    bool Telemetry { get; set; }

    /// <summary>
    /// Is the quick menu showed when using a hotkey
    /// </summary>
    bool QuickMenuEnabled { get; set; }

    /// <summary>
    /// Is keep volume enabled
    /// </summary>
    bool KeepVolumeEnabled { get; set; }

    /// <summary>
    /// How many banner notification can be displayed at the same time on the screen
    /// </summary>
    int MaxNumberNotification { get; set; }

    TimeSpan BannerOnScreenTime { get; set; }
    int BannerOpacityPercentage { get; set; }
    bool NotificationAdvancedMode { get; set; }
    BannerDisplayInfo BannerDisplayInfo { get; set; }

    /// <summary>
    /// Rules for per-app audio routing (App Sound Lock)
    /// </summary>
    HashSet<Model.AppSoundRule> AppSoundRules { get; set; }
}
