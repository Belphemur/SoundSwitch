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
using System.Linq;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.DeviceCyclerManager;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Profile;
using SoundSwitch.Framework.Profile.Trigger;
using SoundSwitch.Framework.TrayIcon.Icon;
using SoundSwitch.Framework.TrayIcon.TooltipInfoManager.TootipInfo;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.Updater.Remind;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Localization.Factory;

namespace SoundSwitch.Framework.Configuration
{
    public class SoundSwitchConfiguration : ISoundSwitchConfiguration
    {
        public SoundSwitchConfiguration()
        {
            // Basic Settings
            FirstRun = true;
            SwitchForegroundProgram = false;

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
            Language = new LanguageFactory().GetWindowsLanguage();
            SelectedPlaybackDeviceListId = new HashSet<string>();
            SelectedRecordingDeviceListId = new HashSet<string>();
            PlaybackHotKey = new HotKey(Keys.F11, HotKey.ModifierKeys.Alt | HotKey.ModifierKeys.Control);
            RecordingHotKey = new HotKey(Keys.F7, HotKey.ModifierKeys.Alt | HotKey.ModifierKeys.Control);
            MuteRecordingHotKey = new HotKey(Keys.M, HotKey.ModifierKeys.Control | HotKey.ModifierKeys.Alt);

            AutoAddNewConnectedDevices = false;

            SelectedDevices = new HashSet<DeviceInfo>();
            SwitchIcon = IconChangerFactory.ActionEnum.Never;
            MigratedFields = new HashSet<string>();
        }


        public HashSet<string> SelectedPlaybackDeviceListId { get; }
        public HashSet<string> SelectedRecordingDeviceListId { get; }
        public HashSet<DeviceInfo> SelectedDevices { get; set; }
        public bool FirstRun { get; set; }
        public HotKey PlaybackHotKey { get; set; }
        public HotKey RecordingHotKey { get; set; }

        public HotKey MuteRecordingHotKey { get; set; }
        public bool ChangeCommunications { get; set; }
        public uint UpdateCheckInterval { get; set; }
        public UpdateMode UpdateMode { get; set; }
        public TooltipInfoTypeEnum TooltipInfo { get; set; }
        public DeviceCyclerTypeEnum CyclerType { get; set; }
        public NotificationTypeEnum NotificationSettings { get; set; }
        public Language Language { get; set; }
        public bool IncludeBetaVersions { get; set; }
        public string CustomNotificationFilePath { get; set; }
        public bool NotifyUsingPrimaryScreen { get; set; }

        public bool AutoAddNewConnectedDevices { get; set; }


        public DateTime LastDonationNagTime { get; set; }

        [JsonIgnore]
        public TimeSpan TimeBetweenDonateNag { get; set; } = TimeSpan.FromDays(90);

        [Obsolete]
        public bool KeepSystrayIcon { get; set; }

        public bool SwitchForegroundProgram { get; set; }
        public IconChangerFactory.ActionEnum SwitchIcon { get; set; }

        [Obsolete]
        public HashSet<ProfileSetting> ProfileSettings { get; set; } = new();

        public HashSet<Profile.Profile> Profiles { get; set; } = new();

        public ReleasePostponed Postponed { get; set; }

        /// <summary>
        /// Fields of the config that got migrated
        /// </summary>
        public HashSet<string> MigratedFields { get; }

        public Guid UniqueInstallationId { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Is telemetry enabled
        /// </summary>
        public bool Telemetry { get; set; } = true;

        /// <summary>
        /// Is the quick menu showed when using a hotkey
        /// </summary>
        public bool QuickMenuEnabled { get; set; } = false;

        /// <summary>
        /// Is keep volume enabled
        /// </summary>
        public bool KeepVolumeEnabled { get; set; } = false;

        // Needed by Interface
        [JsonIgnore]
        public string FileLocation { get; set; }

        /// <summary>
        /// Migrate configuration to a new schema
        /// </summary>
        public bool Migrate()
        {
            var migrated = false;
            if (SelectedPlaybackDeviceListId.Count > 0)
            {
                SelectedDevices.UnionWith(
                    SelectedPlaybackDeviceListId.Select((s => new DeviceInfo("", s, DataFlow.Render, false, DateTime.UtcNow))));
                SelectedPlaybackDeviceListId.Clear();
                migrated = true;
            }

            if (SelectedRecordingDeviceListId.Count > 0)
            {
                SelectedDevices.UnionWith(
                    SelectedRecordingDeviceListId.Select((s => new DeviceInfo("", s, DataFlow.Capture, false, DateTime.UtcNow))));
                SelectedRecordingDeviceListId.Clear();
                migrated = true;
            }

            if (NotificationSettings == NotificationTypeEnum.ToastNotification)
            {
                NotificationSettings = NotificationTypeEnum.BannerNotification;
            }

#pragma warning disable 612
            if (!MigratedFields.Contains(nameof(KeepSystrayIcon)))
            {
                SwitchIcon = KeepSystrayIcon ? IconChangerFactory.ActionEnum.Never : IconChangerFactory.ActionEnum.Always;
                MigratedFields.Add(nameof(KeepSystrayIcon));
                migrated = true;
            }

            if (!MigratedFields.Contains(nameof(ProfileSettings) + "_final"))
            {
                Profiles = ProfileSettings
                           .Select(setting =>
                           {
                               var profile = new Profile.Profile
                               {
                                   AlsoSwitchDefaultDevice = setting.AlsoSwitchDefaultDevice,
                                   Communication = null,
                                   Playback = setting.Playback,
                                   Name = setting.ProfileName,
                                   Recording = setting.Recording
                               };
                               if (setting.HotKey != null)
                               {
                                   profile.Triggers.Add(new Trigger(TriggerFactory.Enum.HotKey)
                                   {
                                       HotKey = setting.HotKey
                                   });
                               }

                               if (!string.IsNullOrEmpty(setting.ApplicationPath))
                               {
                                   profile.Triggers.Add(new Trigger(TriggerFactory.Enum.Process)
                                   {
                                       ApplicationPath = setting.ApplicationPath
                                   });
                               }

                               return profile;
                           })
                           .ToHashSet();
                MigratedFields.Add(nameof(ProfileSettings) + "_final");
                migrated = true;
            }

            if (!MigratedFields.Contains(nameof(LastDonationNagTime)))
            {
                LastDonationNagTime = DateTime.UtcNow - TimeSpan.FromDays(10);
                MigratedFields.Add(nameof(LastDonationNagTime));
                migrated = true;
            }

            if (!MigratedFields.Contains($"{nameof(SwitchForegroundProgram)}_force_off"))
            {
                SwitchForegroundProgram = false;
                MigratedFields.Add($"{nameof(SwitchForegroundProgram)}_force_off");
                migrated = true;
            }

            if (!MigratedFields.Contains("CleanupSelectedDevices"))
            {
                SelectedDevices = SelectedDevices.DistinctBy(info => info.NameClean).ToHashSet();
                MigratedFields.Add("CleanupSelectedDevices");
                migrated = true;
            }

            if (!MigratedFields.Contains("CleanupProfilesForeground"))
            {
                foreach (var profile in Profiles)
                {
                    profile.SwitchForegroundApp = false;
                }

                MigratedFields.Add("CleanupProfilesForeground");
                migrated = true;
            }

            if (Environment.OSVersion.Version.Major < 10 && !MigratedFields.Contains("ProfileWin7"))
            {
                Profiles = Profiles.Select(profile =>
                                   {
                                       profile.SwitchForegroundApp = false;
                                       return profile;
                                   })
                                   .ToHashSet();
                MigratedFields.Add("ProfileWin7");
                migrated = true;
            }

            var switchForegroundFix = $"{nameof(SwitchForegroundProgram)}_fix";
            if (!MigratedFields.Contains(switchForegroundFix) && (SwitchForegroundProgram || Profiles.Any(profile => profile.SwitchForegroundApp)))
            {
                AudioSwitcher.Instance.ResetProcessDeviceConfiguration();
                MigratedFields.Add(switchForegroundFix);
                migrated = true;
            }

            return migrated;
#pragma warning restore 612
        }

        public void Save()
        {
            Log.Debug("Saving configuration {configuration}", this);
            ConfigurationManager.SaveConfiguration(this);
        }

        public override string ToString()
        {
            return $"{GetType().Name}({FileLocation})";
        }
    }
}