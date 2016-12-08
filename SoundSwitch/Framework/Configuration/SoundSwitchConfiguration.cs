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
using System.IO;
using System.Windows.Forms;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.TooltipInfoManager.TootipInfo;
using SoundSwitch.Framework.Updater;

namespace SoundSwitch.Framework.Configuration
{
    public class SoundSwitchConfiguration : ISoundSwitchConfiguration
    {
        public SoundSwitchConfiguration()
        {
            FirstRun = true;
            ChangeCommunications = false;
            SelectedPlaybackDeviceList = null;
            SelectedRecordingDeviceList = null;
            NotificationSettings = NotificationTypeEnum.DefaultWindowsNotification;
            SelectedPlaybackDeviceListId = new HashSet<string>();
            SelectedRecordingDeviceListId = new HashSet<string>();
            PlaybackHotKeys = new HotKeys(Keys.F11, HotKeys.ModifierKeys.Alt | HotKeys.ModifierKeys.Control);
            RecordingHotKeys = new HotKeys(Keys.F7, HotKeys.ModifierKeys.Alt | HotKeys.ModifierKeys.Control);
            //12 hours
            UpdateCheckInterval = 3600 * 12;
            UpdateState = UpdateState.Steath;
            SubscribedBetaVersion = false;
            TooltipInfo = TooltipInfoTypeEnum.Playback;
            CyclerType = DeviceCyclerTypeEnum.Available;
            KeepSystrayIcon = false;

        }

        /*TODO: Remove in next VERSION (3.6.6)*/
        public HashSet<string> SelectedPlaybackDeviceList { get; set; }
        public HashSet<string> SelectedRecordingDeviceList { get; set; }
        public bool MigratedSelectedDeviceLists { get; set; }

        public HashSet<string> SelectedPlaybackDeviceListId { get; }
        public HashSet<string> SelectedRecordingDeviceListId { get; }
        public bool FirstRun { get; set; }
        public HotKeys PlaybackHotKeys { get; set; }
        public HotKeys RecordingHotKeys { get; set; }
        public bool ChangeCommunications { get; set; }
        public uint UpdateCheckInterval { get; set; }
        public UpdateState UpdateState { get; set; }
        public TooltipInfoTypeEnum TooltipInfo { get; set; }
        public DeviceCyclerTypeEnum CyclerType { get; set; }
        public NotificationTypeEnum NotificationSettings { get; set; }
        public bool SubscribedBetaVersion { get; set; }
        public string CustomNotificationFilePath { get; set; }
        public bool KeepSystrayIcon { get; set; }


        //Needed by Interface
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