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
using System.Globalization;
using System.Windows.Forms;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.TooltipInfoManager.TootipInfo;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.Configuration
{
    public class SoundSwitchConfiguration : ISoundSwitchConfiguration
    {
        public SoundSwitchConfiguration()
        {
            // Basic Settings
            FirstRun = true;
            KeepSystrayIcon = false;

            // Audio Settings
            ChangeCommunications = false;
            NotificationSettings = NotificationTypeEnum.BannerNotification;
            TooltipInfo = TooltipInfoTypeEnum.Playback;
            CyclerType = DeviceCyclerTypeEnum.Available;

            // Update Settings
            UpdateCheckInterval = 3600 * 24; // 24 hours
            UpdateMode = UpdateMode.Notify;
            IncludeBetaVersions = false;

            // Language Settings
            Language = LanguageParser.ParseLanguage(CultureInfo.InstalledUICulture);
            SelectedPlaybackDeviceListId = new HashSet<string>();
            SelectedRecordingDeviceListId = new HashSet<string>();
            PlaybackHotKeys = new HotKeys(Keys.F11, HotKeys.ModifierKeys.Alt | HotKeys.ModifierKeys.Control);
            RecordingHotKeys = new HotKeys(Keys.F7, HotKeys.ModifierKeys.Alt | HotKeys.ModifierKeys.Control);
        }

        /*TODO: Remove in next VERSION (3.12.8)*/
        public UpdateState UpdateState
        {
            set {
                UpdateMode = value == UpdateState.Steath ? UpdateMode.Silent : UpdateMode.Notify;
            }
        }

        public HashSet<string> SelectedPlaybackDeviceListId { get; }
        public HashSet<string> SelectedRecordingDeviceListId { get; }
        public bool FirstRun { get; set; }
        public HotKeys PlaybackHotKeys { get; set; }
        public HotKeys RecordingHotKeys { get; set; }
        public bool ChangeCommunications { get; set; }
        public uint UpdateCheckInterval { get; set; }
        public UpdateMode UpdateMode { get; set; }
        public TooltipInfoTypeEnum TooltipInfo { get; set; }
        public DeviceCyclerTypeEnum CyclerType { get; set; }
        public NotificationTypeEnum NotificationSettings { get; set; }
        public Language Language { get; set; }
        public bool IncludeBetaVersions { get; set; }
        public string CustomNotificationFilePath { get; set; }
        public bool KeepSystrayIcon { get; set; }


        // Needed by Interface
        public string FileLocation { get; set; }

        public void Save()
        {
            using (AppLogger.Log.DebugCall())
            {
                AppLogger.Log.Debug("Saving configuration ", this);
                ConfigurationManager.SaveConfiguration(this);
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name}({FileLocation})";
        }
    }
}