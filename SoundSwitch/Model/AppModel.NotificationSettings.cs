/********************************************************************
 * Copyright (C) 2015 Jeroen Pelgrims
 * Copyright (C) 2015-2024 Antoine Aflalo
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
using System.IO;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Banner.BannerDisplayInfo;
using SoundSwitch.Framework.Banner.BannerPosition;
using SoundSwitch.Framework.Banner.BannerPosition.Position;
using SoundSwitch.Framework.Banner.MicrophoneMute;
using SoundSwitch.Framework.Configuration;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.UI.Component;

namespace SoundSwitch.Model;

public partial class AppModel
{
    public int BannerOnScreenTimeSecs
    {
        get => (int)BannerOnScreenTime.TotalSeconds;
        set => BannerOnScreenTime = TimeSpan.FromSeconds(value);
    }
    public TimeSpan BannerOnScreenTime
    {
        get => AppConfigs.Configuration.BannerOnScreenTime;
        set
        {
            if (value < TimeSpan.FromSeconds(2)) return;
            if (value > TimeSpan.FromMinutes(1)) return;
            var preValue = AppConfigs.Configuration.BannerOnScreenTime;
            AppConfigs.Configuration.BannerOnScreenTime = value;
            AppConfigs.Configuration.Save();
            BannerSettingsChanged?.Invoke(this,
                new BannerDataChangedEvent(BannerPosition, BannerPosition, preValue, value,
                    BannerOpacityPercentage, BannerOpacityPercentage, BannerDisplayInfo, BannerDisplayInfo,
                    MicrophoneMuteBanner, MicrophoneMuteBanner, MicrophoneUnmuteBanner, MicrophoneUnmuteBanner));
        }
    }

    public int BannerOpacityPercentage
    {
        get => AppConfigs.Configuration.BannerOpacityPercentage;
        set
        {
            if (value is < 10 or > 100) return;
            var preValue = AppConfigs.Configuration.BannerOpacityPercentage;
            AppConfigs.Configuration.BannerOpacityPercentage = value;
            AppConfigs.Configuration.Save();
            BannerSettingsChanged?.Invoke(this,
                new BannerDataChangedEvent(BannerPosition, BannerPosition, BannerOnScreenTime, BannerOnScreenTime,
                    preValue, value, BannerDisplayInfo, BannerDisplayInfo,
                    MicrophoneMuteBanner, MicrophoneMuteBanner, MicrophoneUnmuteBanner, MicrophoneUnmuteBanner));
        }
    }

    /// <summary>
    /// How many notification to show at the same time
    /// </summary>
    public int MaxNumberNotification
    {
        get => AppConfigs.Configuration.MaxNumberNotification;
        set
        {
            if (value is < 1 or > 100) return;
            AppConfigs.Configuration.MaxNumberNotification = value;
            AppConfigs.Configuration.Save();
        }
    }

    /// <summary>
    /// Is there only 1 concurrent notification enabled ?
    /// </summary>
    public bool IsSingleNotification
    {
        get => AppConfigs.Configuration.MaxNumberNotification == 1;
        set => MaxNumberNotification = value ? 1 : 5;
    }

    public CachedSound CustomNotificationSound
    {
        get
        {
            try
            {
                return _customNotificationCachedSound ??= new CachedSound(AppConfigs.Configuration.CustomNotificationFilePath);
            }
            catch (CachedSoundFileNotExistsException)
            {
                return null;
            }
            catch (InvalidDataException)
            {
                // Custom sounds are WAV-only now: a stored path to a previously supported
                // non-WAV file (MP3/FLAC/AAC) is ignored, falling back to the default sound.
                return null;
            }
        }
        set
        {
            var oldSound = _customNotificationCachedSound;
            _customNotificationCachedSound = value;
            AppConfigs.Configuration.CustomNotificationFilePath = _customNotificationCachedSound?.FilePath;
            AppConfigs.Configuration.Save();
            CustomSoundChanged?.Invoke(this, new CustomSoundChangedEvent(oldSound, value));
        }
    }

    public BannerDisplayInfo BannerDisplayInfo
    {
        get => AppConfigs.Configuration.BannerDisplayInfo;
        set
        {
            var preValue = AppConfigs.Configuration.BannerDisplayInfo;
            AppConfigs.Configuration.BannerDisplayInfo = value;
            AppConfigs.Configuration.Save();
            BannerSettingsChanged?.Invoke(this,
                new BannerDataChangedEvent(BannerPosition, BannerPosition, BannerOnScreenTime, BannerOnScreenTime,
                    BannerOpacityPercentage, BannerOpacityPercentage, preValue, value,
                    MicrophoneMuteBanner, MicrophoneMuteBanner, MicrophoneUnmuteBanner, MicrophoneUnmuteBanner));
        }
    }

    public bool NotificationAdvancedMode
    {
        get => AppConfigs.Configuration.NotificationAdvancedMode;
        set
        {
            var previousSwitchDeviceNotification = AppConfigs.Configuration.SwitchDeviceNotification;
            var previousSwitchProfileNotification = AppConfigs.Configuration.SwitchProfileNotification;
            var previousMicrophoneMuteNotification = AppConfigs.Configuration.MicrophoneMuteNotification;

            AppConfigs.Configuration.NotificationAdvancedMode = value;

            if (!value)
            {
                AppConfigs.Configuration.SwitchProfileNotification = AppConfigs.Configuration.SwitchDeviceNotification;
                AppConfigs.Configuration.MicrophoneMuteNotification = AppConfigs.Configuration.SwitchDeviceNotification;
            }

            AppConfigs.Configuration.Save();

            NotificationSettingsChanged?.Invoke(this,
                new NotificationSettingsUpdatedEvent(previousSwitchDeviceNotification, AppConfigs.Configuration.SwitchDeviceNotification,
                    previousSwitchProfileNotification, AppConfigs.Configuration.SwitchProfileNotification,
                    previousMicrophoneMuteNotification, AppConfigs.Configuration.MicrophoneMuteNotification));
        }
    }

    public NotificationType SwitchDeviceNotification
    {
        get => AppConfigs.Configuration.SwitchDeviceNotification;
        set
        {
            var previousSwitchDeviceNotification = AppConfigs.Configuration.SwitchDeviceNotification;
            var previousSwitchProfileNotification = AppConfigs.Configuration.SwitchProfileNotification;
            var previousMicrophoneMuteNotification = AppConfigs.Configuration.MicrophoneMuteNotification;

            AppConfigs.Configuration.SwitchDeviceNotification = value;

            if (!AppConfigs.Configuration.NotificationAdvancedMode)
            {
                AppConfigs.Configuration.SwitchProfileNotification = value;
                AppConfigs.Configuration.MicrophoneMuteNotification = value;
            }

            AppConfigs.Configuration.Save();

            NotificationSettingsChanged?.Invoke(this,
                new NotificationSettingsUpdatedEvent(previousSwitchDeviceNotification, AppConfigs.Configuration.SwitchDeviceNotification,
                    previousSwitchProfileNotification, AppConfigs.Configuration.SwitchProfileNotification,
                    previousMicrophoneMuteNotification, AppConfigs.Configuration.MicrophoneMuteNotification));
        }
    }

    public NotificationType SwitchProfileNotification
    {
        get => AppConfigs.Configuration.SwitchProfileNotification;
        set
        {
            if (!AppConfigs.Configuration.NotificationAdvancedMode)
            {
                SwitchDeviceNotification = value;
                return;
            }

            var preValue = AppConfigs.Configuration.SwitchProfileNotification;
            AppConfigs.Configuration.SwitchProfileNotification = value;
            AppConfigs.Configuration.Save();
            NotificationSettingsChanged?.Invoke(this,
                new NotificationSettingsUpdatedEvent(SwitchDeviceNotification, SwitchDeviceNotification,
                    preValue, value, MicrophoneMuteNotification, MicrophoneMuteNotification));
        }
    }
    public NotificationType MicrophoneMuteNotification
    {
        get => AppConfigs.Configuration.MicrophoneMuteNotification;
        set
        {
            if (!AppConfigs.Configuration.NotificationAdvancedMode)
            {
                SwitchDeviceNotification = value;
                return;
            }

            var preValue = AppConfigs.Configuration.MicrophoneMuteNotification;
            AppConfigs.Configuration.MicrophoneMuteNotification = value;
            AppConfigs.Configuration.Save();
            NotificationSettingsChanged?.Invoke(this,
                new NotificationSettingsUpdatedEvent(SwitchDeviceNotification, SwitchDeviceNotification,
                    SwitchProfileNotification, SwitchProfileNotification, preValue, value));
        }
    }

    public BannerPosition BannerPosition
    {
        get => AppConfigs.Configuration.BannerPosition;
        set
        {
            var preValue = AppConfigs.Configuration.BannerPosition;
            AppConfigs.Configuration.BannerPosition = value;
            AppConfigs.Configuration.Save();
            BannerSettingsChanged?.Invoke(this,
                new BannerDataChangedEvent(preValue, value, BannerOnScreenTime, BannerOnScreenTime,
                    BannerOpacityPercentage, BannerOpacityPercentage, BannerDisplayInfo, BannerDisplayInfo,
                    MicrophoneMuteBanner, MicrophoneMuteBanner, MicrophoneUnmuteBanner, MicrophoneUnmuteBanner));
        }
    }

    public Point CustomBannerPosition
    {
        get => AppConfigs.Configuration.CustomBannerPosition;
        set
        {
            AppConfigs.Configuration.CustomBannerPosition = value;
            AppConfigs.Configuration.Save();
        }

    }

    /// <summary>
    /// Current banner position implementation based on the BannerPosition setting
    /// </summary>
    public IPosition BannerPositionImpl => _bannerPositionFactory.Get(BannerPosition);

    public MicrophoneMute MicrophoneMuteBanner
    {
        get => AppConfigs.Configuration.MicrophoneMuteBanner;
        set
        {
            var prevValue = AppConfigs.Configuration.MicrophoneMuteBanner;
            AppConfigs.Configuration.MicrophoneMuteBanner = value;
            AppConfigs.Configuration.Save();
            BannerSettingsChanged?.Invoke(this,
                new BannerDataChangedEvent(BannerPosition, BannerPosition, BannerOnScreenTime, BannerOnScreenTime,
                    BannerOpacityPercentage, BannerOpacityPercentage, BannerDisplayInfo, BannerDisplayInfo,
                    prevValue, value, MicrophoneUnmuteBanner, MicrophoneUnmuteBanner));
        }
    }

    public MicrophoneMute MicrophoneUnmuteBanner
    {
        get => AppConfigs.Configuration.MicrophoneUnmuteBanner;
        set
        {
            var prevValue = AppConfigs.Configuration.MicrophoneUnmuteBanner;
            AppConfigs.Configuration.MicrophoneUnmuteBanner = value;
            AppConfigs.Configuration.Save();
            BannerSettingsChanged?.Invoke(this,
                new BannerDataChangedEvent(BannerPosition, BannerPosition, BannerOnScreenTime, BannerOnScreenTime,
                    BannerOpacityPercentage, BannerOpacityPercentage, BannerDisplayInfo, BannerDisplayInfo,
                    MicrophoneMuteBanner, MicrophoneMuteBanner, prevValue, value));
        }
    }

    public bool NotifyUsingPrimaryScreen
    {
        get => AppConfigs.Configuration.NotifyUsingPrimaryScreen;
        set
        {
            AppConfigs.Configuration.NotifyUsingPrimaryScreen = value;
            AppConfigs.Configuration.Save();
        }
    }

    public event EventHandler<NotificationSettingsUpdatedEvent> NotificationSettingsChanged;
    public event EventHandler<BannerDataChangedEvent> BannerSettingsChanged;
    public event EventHandler<CustomSoundChangedEvent> CustomSoundChanged;
}
