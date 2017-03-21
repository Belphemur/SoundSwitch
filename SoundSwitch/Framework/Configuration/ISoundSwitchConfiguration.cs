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

using System.Collections.Generic;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.TooltipInfoManager.TootipInfo;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.Configuration
{
    public interface ISoundSwitchConfiguration : IConfiguration
    {
        HashSet<string> SelectedPlaybackDeviceListId { get; }
        HashSet<string> SelectedRecordingDeviceListId { get; }
        bool FirstRun { get; set; }
        HotKeys PlaybackHotKeys { get; set; }
        HotKeys RecordingHotKeys { get; set; }
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
    }
}