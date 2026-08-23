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
using System.Linq;
using System.Windows.Forms;

using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Framework.Telemetry;
using SoundSwitch.Framework.Toast;
using SoundSwitch.Localization;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.NotificationManager.Notification;

internal class NotificationWindows : INotification
{
    public NotificationType TypeEnum => NotificationType.DefaultWindowsNotification;
    public string Label => SettingsStrings.notification_option_windowsDefault;

    public INotificationConfiguration Configuration { get; set; }

    public void NotifyDefaultChanged(DeviceFullInfo audioDevice)
    {
        TelemetryService.TrackNotificationWindows();
        var data = NotificationContentBuilder.BuildDefaultChanged(audioDevice, NotificationContentBuilder.DeviceChangeWording.WindowsNotification);
        ShowToastOrBalloon(data);
    }

    public void NotifyProfileChanged(Profile.Profile profile, Bitmap icon, uint? processId)
    {
        var data = NotificationContentBuilder.BuildProfileChanged(profile, icon);
        ShowToastOrBalloon(data);
    }

    public void NotifyAppRuleMatched(AppSoundRule rule, DeviceFullInfo playback, DeviceFullInfo recording, Bitmap icon, uint processId)
    {
        var data = NotificationContentBuilder.BuildAppRuleMatched(playback, recording, icon);
        ShowToastOrBalloon(data);
    }

    public void NotifyMicrophoneMuteChanged(string deviceId, string microphoneName, bool newMuteState)
    {
        var data = NotificationContentBuilder.BuildMicrophoneMuteChanged(microphoneName, newMuteState);
        ShowToastOrBalloon(data);
    }

    private void ShowToastOrBalloon(BannerData data)
    {
        data.Ttl = Configuration.Ttl;

        if (OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763) && ToastNotificationRenderer.Show(data))
        {
            return;
        }

        ShowBalloon(data);
    }

    private void ShowBalloon(BannerData data)
    {
        Configuration.Icon.ShowBalloonTip(1000, data.Title, data.Text, ToolTipIcon.Info);
    }

    public void OnSoundChanged(CachedSound newSound) { }

    public bool SupportCustomSound() => false;

    public bool IsAvailable() => true;
}
