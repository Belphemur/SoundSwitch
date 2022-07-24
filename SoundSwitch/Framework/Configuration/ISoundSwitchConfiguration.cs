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
using JetBrains.Annotations;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.TrayIcon.Icon;
using SoundSwitch.Framework.TrayIcon.TooltipInfoManager.TootipInfo;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.Updater.Remind;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Localization.Factory;

namespace SoundSwitch.Framework.Configuration
{
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

        NotificationTypeEnum NotificationSettings { get; set; }
        bool IncludeBetaVersions { get; set; }
        string CustomNotificationFilePath { get; set; }
        UpdateMode UpdateMode { get; set; }
        Language Language { get; set; }
        TooltipInfoTypeEnum TooltipInfo { get; set; }
        DeviceCyclerTypeEnum CyclerType { get; set; }
        bool KeepSystrayIcon { get; set; }

        bool SwitchForegroundProgram { get; set; }
        bool NotifyUsingPrimaryScreen { get; set; }
        
        bool AutoAddNewConnectedDevices { get; set; }

        /// <summary>
        /// What to do with the TrayIcon when changing default device
        /// </summary>
        IconChangerFactory.ActionEnum SwitchIcon { get; set; }

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
    }
}