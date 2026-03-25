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

using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Framework.NotificationManager.Notification;
using SoundSwitch.Localization;
using SoundSwitch.Model;
using SoundSwitch.Properties;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System;

namespace SoundSwitch.Framework.NotificationManager;

public class NotificationManager(IAppModel model)
{
    private string _lastDeviceId;
    private INotification _switchDeviceNotification;
    private INotification _switchProfileNotification;
    private INotification _microphoneMuteNotification;
    private readonly NotificationFactory _notificationFactory = new();

    public void Init()
    {
        model.DefaultDeviceChanged += ModelOnDefaultDeviceChanged;
        model.NotificationSettingsChanged += ModelOnNotificationSettingsChanged;
        SetNotifications(model.SwitchDeviceNotification, model.SwitchProfileNotification, model.MicrophoneMuteNotification);
        model.CustomSoundChanged += ModelOnCustomSoundChanged;
        model.BannerSettingsChanged += ModelOnBannerSettingsChanged;
        Log.Information("Notification manager initiated");
    }

    private void ModelOnBannerSettingsChanged(object sender, BannerDataChangedEvent e)
    {
        ApplyConfigurationChanges(_switchDeviceNotification, e);
        ApplyConfigurationChanges(_switchProfileNotification, e);
        ApplyConfigurationChanges(_microphoneMuteNotification, e);
    }

    private void ModelOnCustomSoundChanged(object sender, CustomSoundChangedEvent customSoundChangedEvent)
    {
        _switchDeviceNotification.OnSoundChanged(customSoundChangedEvent.NewSound);
        _switchProfileNotification.OnSoundChanged(customSoundChangedEvent.NewSound);
        _microphoneMuteNotification.OnSoundChanged(customSoundChangedEvent.NewSound);
    }

    private void ModelOnNotificationSettingsChanged(object sender, NotificationSettingsUpdatedEvent notificationSettingsUpdatedEvent)
    {
        SetNotifications(notificationSettingsUpdatedEvent.NewSwitchDeviceSettings,
            notificationSettingsUpdatedEvent.NewSwitchProfileSettings,
            notificationSettingsUpdatedEvent.NewMicrophoneMuteSettings);
    }

    private void SetNotifications(NotificationType switchDeviceType, NotificationType switchProfileType, NotificationType microphoneMuteType)
    {
        _switchDeviceNotification = BuildNotification(switchDeviceType);
        _switchProfileNotification = BuildNotification(switchProfileType);
        _microphoneMuteNotification = BuildNotification(microphoneMuteType);
    }

    private INotification BuildNotification(NotificationType notificationType)
    {
        var notification = _notificationFactory.Get(notificationType);
        notification.Configuration = new NotificationConfiguration
        {
            Icon = model.TrayIcon.NotifyIcon,
            DefaultSound = Resources.NotificationSound,
            BannerPosition = AppModel.Instance.BannerPosition,
            Ttl = AppModel.Instance.BannerOnScreenTime,
            Opacity = AppModel.Instance.BannerOpacityPercentage,
            MicrophoneMuteBanner = AppModel.Instance.MicrophoneMuteBanner,
            MicrophoneUnmuteBanner = AppModel.Instance.MicrophoneUnmuteBanner,
        };

        try
        {
            notification.Configuration.CustomSound = AppModel.Instance.CustomNotificationSound;
        }
        catch (CachedSoundFileNotExistsException)
        {
            MessageBox.Show(string.Format(SettingsStrings.audioFileNotFound, SettingsStrings.notification_option_sound),
                SettingsStrings.audioFileNotFound_caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        return notification;
    }

    private static void ApplyConfigurationChanges(INotification notification, BannerDataChangedEvent e)
    {
        notification.Configuration.BannerPosition = e.NewBannerPosition;
        notification.Configuration.Ttl = e.NewTtl;
        notification.Configuration.Opacity = e.NewOpacity;
        notification.Configuration.MicrophoneMuteBanner = e.NewMicrophoneMuteBanner;
        notification.Configuration.MicrophoneUnmuteBanner = e.NewMicrophoneUnmuteBanner;
    }

    private void ModelOnDefaultDeviceChanged(object sender, DeviceDefaultChangedEvent deviceDefaultChangedEvent)
    {
        if (_lastDeviceId == deviceDefaultChangedEvent.DeviceId)
            return;

        _switchDeviceNotification.NotifyDefaultChanged(deviceDefaultChangedEvent.Device);
        _lastDeviceId = deviceDefaultChangedEvent.DeviceId;
    }

    private DeviceFullInfo? CheckDeviceAvailable(DeviceInfo deviceInfo)
    {
        return deviceInfo.Type switch
        {
            DataFlow.Capture => model.AvailableRecordingDevices.FirstOrDefault(info => info.Equals(deviceInfo)),
            _ => model.AvailablePlaybackDevices.FirstOrDefault(info => info.Equals(deviceInfo))
        };
    }

    /// <summary>
    /// Notify on Profile changed
    /// </summary>
    public void NotifyProfileChanged(Profile.Profile profile, uint? processId)
    {
        if (profile.NotifyOnActivation)
        {
            var icon = GetIcon(profile, processId);

            _switchProfileNotification.NotifyProfileChanged(profile, icon, processId);
        }
    }

    /// <summary>
    /// Notify on App Rule matched
    /// </summary>
    public void NotifyAppRuleMatched(AppSoundRule rule, uint processId)
    {
        if (rule.Notify)
        {
            var icon = GetIconForRule(rule, processId);
            
            var playback = model.AvailablePlaybackDevices.FirstOrDefault(d => d.Id == rule.PlaybackDeviceId);
            var recording = model.AvailableRecordingDevices.FirstOrDefault(d => d.Id == rule.RecordingDeviceId);
            
            _switchProfileNotification.NotifyAppRuleMatched(rule, playback, recording, icon, processId);
        }
    }

    private Bitmap GetIconForRule(AppSoundRule rule, uint processId)
    {
        if (!_switchProfileNotification.SupportIcon)
        {
            return null;
        }

        Bitmap icon = null;
        try
        {
            var process = Process.GetProcessById((int)processId);
            using var iconHandle = IconExtractor.Extract(process.MainModule?.FileName, 0, true);
            icon = iconHandle.ToBitmap();
        }
        catch (Exception)
        {
            // ignored
        }

        return icon ?? Resources.default_profile_image;
    }

    private Bitmap GetIcon(Profile.Profile profile, uint? processId)
    {
        if (!_switchProfileNotification.SupportIcon)
        {
            return null;
        }

        Bitmap icon = null;
        if (processId.HasValue)
        {
            try
            {
                var process = Process.GetProcessById((int)processId.Value);
                using var iconHandle = IconExtractor.Extract(process.MainModule?.FileName, 0, true);
                icon = iconHandle.ToBitmap();
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
                using var iconHandle = device.LargeIcon;
                icon = iconHandle.ToBitmap();
            }
        }

        return icon ?? Resources.default_profile_image;
    }

    public void NotifyMuteChanged(string deviceId, string microphoneName, bool newMuteState)
    {
        _microphoneMuteNotification.NotifyMicrophoneMuteChanged(deviceId, microphoneName, newMuteState);
    }

    ~NotificationManager()
    {
        model.DefaultDeviceChanged -= ModelOnDefaultDeviceChanged;
        model.NotificationSettingsChanged -= ModelOnNotificationSettingsChanged;
        model.CustomSoundChanged -= ModelOnCustomSoundChanged;
    }
}
