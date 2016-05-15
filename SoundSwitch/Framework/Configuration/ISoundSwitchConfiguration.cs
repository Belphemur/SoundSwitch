/********************************************************************
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
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.TooltipInfoManager.TootipInfo;
using SoundSwitch.Framework.Updater;

namespace SoundSwitch.Framework.Configuration
{
    public interface ISoundSwitchConfiguration : IConfiguration
    {
        /*TODO: Remove in next VERSION (3.6.6)*/
        HashSet<string> SelectedPlaybackDeviceList { get; set; }
        HashSet<string> SelectedRecordingDeviceList { get; set; }

        HashSet<string> SelectedPlaybackDeviceListId { get; }
        HashSet<string> SelectedRecordingDeviceListId { get; }
        bool FirstRun { get; set; }
        HotKeys PlaybackHotKeys { get; set; }
        HotKeys RecordingHotKeys { get; set; }
        bool ChangeCommunications { get; set; }
        uint UpdateCheckInterval { get; set; }

        /*TODO: Remove in next VERSION (3.7.0)*/
        [Obsolete("Use the NotificationSettings instead.")]
        bool DisplayNotifications { get; set; }

        bool MigratedSelectedDeviceLists { get; set; }
        NotificationTypeEnum NotificationSettings { get; set; }
        bool SubscribedBetaVersion { get; set; }
        string CustomNotificationFilePath { get; set; }
        UpdateState UpdateState { get; set; }
        TooltipInfoTypeEnum TooltipInfo { get; set; }
        DeviceCyclerTypeEnum CyclerType { get; set; }
    }
}