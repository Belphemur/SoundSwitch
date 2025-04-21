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
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.NotificationManager
{
    public class NotificationManager
    {
        private readonly IAppModel _model;
        private string _lastDeviceId;
        private INotification _notification;
        private readonly NotificationFactory _notificationFactory;

        public NotificationManager(IAppModel model)
        {
            _model = model;
            _notificationFactory = new NotificationFactory();
        }

        public void Init()
        {
            _model.DefaultDeviceChanged += ModelOnDefaultDeviceChanged;
            _model.NotificationSettingsChanged += ModelOnNotificationSettingsChanged;
            SetNotification(_model.NotificationSettings);
            _model.CustomSoundChanged += ModelOnCustomSoundChanged;
            _model.BannerSettingsChanged += ModelOnBannerSettingsChanged;
            Log.Information("Notification manager initiated");
        }

        private void ModelOnBannerSettingsChanged(object sender, BannerDataChangedEvent e)
        {
            _notification.Configuration.BannerPosition = e.NewBannerPosition;
            _notification.Configuration.Ttl = e.NewTtl;
            _notification.Configuration.MicrophoneMuteNotification = e.NewMicrophoneMuteNotification;
        }

        private void ModelOnCustomSoundChanged(object sender, CustomSoundChangedEvent customSoundChangedEvent)
        {
            _notification.OnSoundChanged(customSoundChangedEvent.NewSound);
        }

        private void ModelOnNotificationSettingsChanged(object sender, NotificationSettingsUpdatedEvent notificationSettingsUpdatedEvent)
        {
            var notificationTypeEnum = notificationSettingsUpdatedEvent.NewSettings;
            SetNotification(notificationTypeEnum);
        }

        private void SetNotification(NotificationTypeEnum notificationTypeEnum)
        {
            _notification = _notificationFactory.Get(notificationTypeEnum);
            _notification.Configuration = new NotificationConfiguration()
            {
                Icon = _model.TrayIcon.NotifyIcon,
                DefaultSound = Resources.NotificationSound,
                BannerPosition = AppModel.Instance.BannerPosition,
                Ttl = AppModel.Instance.BannerOnScreenTime,
                MicrophoneMuteNotification = AppModel.Instance.MicrophoneMuteNotification
            };
            try
            {
                _notification.Configuration.CustomSound = AppModel.Instance.CustomNotificationSound;
            }
            catch (CachedSoundFileNotExistsException)
            {
                MessageBox.Show(string.Format(SettingsStrings.audioFileNotFound, SettingsStrings.notification_option_sound),
                    SettingsStrings.audioFileNotFound_caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ModelOnDefaultDeviceChanged(object sender, DeviceDefaultChangedEvent deviceDefaultChangedEvent)
        {
            if (_lastDeviceId == deviceDefaultChangedEvent.DeviceId)
                return;

            _notification.NotifyDefaultChanged(deviceDefaultChangedEvent.Device);
            _lastDeviceId = deviceDefaultChangedEvent.DeviceId;
        }

        private DeviceFullInfo? CheckDeviceAvailable(DeviceInfo deviceInfo)
        {
            return deviceInfo.Type switch
            {
                DataFlow.Capture => _model.AvailableRecordingDevices.FirstOrDefault(info => info.Equals(deviceInfo)),
                _ => _model.AvailablePlaybackDevices.FirstOrDefault(info => info.Equals(deviceInfo))
            };
        }

        /// <summary>
        /// Notify on Profile changed
        /// </summary>
        public void NotifyProfileChanged(Profile.Profile profile, uint? processId)
        {
            if (!profile.NotifyOnActivation)
            {
                return;
            }

            var icon = GetIcon(profile, processId);

            _notification.NotifyProfileChanged(profile, icon, processId);
        }

        private Bitmap GetIcon(Profile.Profile profile, uint? processId)
        {
            if (!_notification.SupportIcon)
            {
                return null;
            }

            Bitmap icon = null;
            if (processId.HasValue)
            {
                try
                {
                    var process = Process.GetProcessById((int)processId.Value);
                    icon = IconExtractor.Extract(process.MainModule?.FileName, 0, true).ToBitmap();
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (icon == null)
            {
                var device = profile.Devices.Select(wrapper => CheckDeviceAvailable(wrapper.DeviceInfo)).FirstOrDefault(info => info != null);
                if (device != null)
                {
                    icon = device.LargeIcon.ToBitmap();
                }
            }

            return icon ?? Resources.default_profile_image;
        }

        public void NotifyMuteChanged(string deviceId, string microphoneName, bool newMuteState)
        {
            _notification.NotifyMuteChanged(deviceId, microphoneName, newMuteState);
        }

        ~NotificationManager()
        {
            _model.DefaultDeviceChanged -= ModelOnDefaultDeviceChanged;
            _model.NotificationSettingsChanged -= ModelOnNotificationSettingsChanged;
            _model.CustomSoundChanged -= ModelOnCustomSoundChanged;
        }
    }
}