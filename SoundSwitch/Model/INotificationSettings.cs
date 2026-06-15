/********************************************************************
 * Copyright (C) 2015 Jeroen Pelgrims
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
using System.Drawing;

using SoundSwitch.Framework.Banner.BannerDisplayInfo;
using SoundSwitch.Framework.Banner.BannerPosition;
using SoundSwitch.Framework.Banner.BannerPosition.Position;
using SoundSwitch.Framework.Banner.MicrophoneMute;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.UI.Component;
using SoundSwitch.Framework.Audio;

namespace SoundSwitch.Model;

/// <summary>
/// Notification and banner display settings.
/// </summary>
public interface INotificationSettings
{
    /// <summary>
    /// What did the user want as Notification of device changed
    /// </summary>
    NotificationType SwitchDeviceNotification { get; set; }
    NotificationType SwitchProfileNotification { get; set; }
    NotificationType MicrophoneMuteNotification { get; set; }
    bool NotificationAdvancedMode { get; set; }

    /// <summary>
    /// The sound to be played for a Custom notification.
    /// </summary>
    CachedSound CustomNotificationSound { get; set; }

    /// <summary>
    /// What did the user want as Banner Position of device changed
    /// </summary>
    BannerPosition BannerPosition { get; set; }

    Point CustomBannerPosition { get; set; }

    TimeSpan BannerOnScreenTime { get; set; }
    int BannerOnScreenTimeSecs { get; set; }
    int BannerOpacityPercentage { get; set; }

    /// <summary>
    /// Current banner position implementation based on the BannerPosition setting
    /// </summary>
    IPosition BannerPositionImpl { get; }

    BannerDisplayInfo BannerDisplayInfo { get; set; }

    /// <summary>
    /// Show a banner when microphone is muted
    /// </summary>
    MicrophoneMute MicrophoneMuteBanner { get; set; }
    /// <summary>
    /// Show a banner when microphone is unmuted
    /// </summary>
    MicrophoneMute MicrophoneUnmuteBanner { get; set; }

    /// <summary>
    /// How many notification to show at the same time
    /// </summary>
    int MaxNumberNotification { get; set; }

    /// <summary>
    /// Is there only 1 concurrent notification enabled ?
    /// </summary>
    bool IsSingleNotification { get; set; }

    /// <summary>
    /// Always show banner on primary screen instead of active screen
    /// </summary>
    bool NotifyUsingPrimaryScreen { get; set; }

    /// <summary>
    /// If the NotificationSettings has been modified
    /// </summary>
    event EventHandler<NotificationSettingsUpdatedEvent> NotificationSettingsChanged;

    /// <summary>
    /// If the BannerPosition has been modified
    /// </summary>
    event EventHandler<BannerDataChangedEvent> BannerSettingsChanged;

    /// <summary>
    /// When the custom sound is changed
    /// </summary>
    event EventHandler<CustomSoundChangedEvent> CustomSoundChanged;
}