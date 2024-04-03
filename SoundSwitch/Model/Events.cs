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
using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Banner;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.Updater.Releases;

namespace SoundSwitch.Model
{
    public class ExceptionEvent : EventArgs
    {
        public ExceptionEvent(Exception exception)
        {
            Exception = exception;
        }

        public Exception Exception { get; private set; }
    }

    public class DeviceListChanged : EventArgs
    {
        public DeviceListChanged(IEnumerable<DeviceInfo> seletedDevicesList, DataFlow type)
        {
            SeletedDevicesList = seletedDevicesList;
            Type = type;
        }

        public IEnumerable<DeviceInfo> SeletedDevicesList { get; private set; }
        public DataFlow Type { get; private set; }
    }

    public class NotificationSettingsUpdatedEvent : EventArgs
    {
        public NotificationTypeEnum PrevSettings { get; }
        public NotificationTypeEnum NewSettings { get; }

        public NotificationSettingsUpdatedEvent(NotificationTypeEnum prevSettings, NotificationTypeEnum newSettings)
        {
            PrevSettings = prevSettings;
            NewSettings = newSettings;
        }
    }

    public class BannerPositionUpdatedEvent : EventArgs
    {
        public BannerPositionEnum PrevBannerPosition { get; }
        public BannerPositionEnum NewBannerPosition { get; }
        public BannerPositionUpdatedEvent(BannerPositionEnum prevBannerPosition, BannerPositionEnum newBannerPosition)
        {
            PrevBannerPosition = prevBannerPosition;
            NewBannerPosition = newBannerPosition;
        }
    }

    public class CustomSoundChangedEvent : EventArgs
    {
        public CachedSound PrevSound { get; }
        public CachedSound NewSound { get; }

        public CustomSoundChangedEvent(CachedSound prevSound, CachedSound newSound)
        {
            PrevSound = prevSound;
            NewSound = newSound;
        }
    }

    public class NewReleaseAvailableEvent : UpdateChecker.NewReleaseEvent
    {
        public UpdateMode UpdateMode { get; }

        public NewReleaseAvailableEvent(AppRelease appRelease, UpdateMode updateMode) : base(appRelease)
        {
            UpdateMode = updateMode;
        }
    }
    

    public class DeviceChangedEventBase : EventArgs
    {
        public string DeviceId { get; }


        public DeviceChangedEventBase(string deviceId)
        {
            DeviceId = deviceId;
        }
    }

    public class DeviceDefaultChangedEvent : DeviceChangedEventBase
    {
        public Role Role { get; }
        public DeviceFullInfo Device { get; }

        public DeviceDefaultChangedEvent(DeviceFullInfo device, Role role) : base(device.Id)
        {
            Device = device;
            Role = role;
        }
    }
}