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
using SoundSwitch.Framework.Banner.BannerPosition;
using SoundSwitch.Framework.Banner.MicrophoneMute;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.Updater.Releases;

namespace SoundSwitch.Model;

public class ExceptionEvent(Exception exception) : EventArgs
{
    public Exception Exception { get; private set; } = exception;
}

public class DeviceListChanged(IEnumerable<DeviceInfo> seletedDevicesList, DataFlow type) : EventArgs
{
    public IEnumerable<DeviceInfo> SeletedDevicesList { get; private set; } = seletedDevicesList;
    public DataFlow Type { get; private set; } = type;
}

public class NotificationSettingsUpdatedEvent(NotificationTypeEnum prevSettings, NotificationTypeEnum newSettings)
    : EventArgs
{
    public NotificationTypeEnum PrevSettings { get; } = prevSettings;
    public NotificationTypeEnum NewSettings { get; } = newSettings;
}

public class BannerDataChangedEvent(
    BannerPositionEnum prevBannerPosition,
    BannerPositionEnum newBannerPosition,
    TimeSpan prevTtl,
    TimeSpan newTtl,
    MicrophoneMuteEnum prevMicrophoneMuteNotification,
    MicrophoneMuteEnum newMicrophoneMuteNotification)
    : EventArgs
{
    public BannerPositionEnum PrevBannerPosition { get; } = prevBannerPosition;
    public BannerPositionEnum NewBannerPosition { get; } = newBannerPosition;
    public TimeSpan PrevTtl { get; } = prevTtl;
    public TimeSpan NewTtl { get; } = newTtl;
    public MicrophoneMuteEnum PrevMicrophoneMuteNotification { get; } = prevMicrophoneMuteNotification;
    public MicrophoneMuteEnum NewMicrophoneMuteNotification { get; } = newMicrophoneMuteNotification;
}

public class CustomSoundChangedEvent(CachedSound prevSound, CachedSound newSound) : EventArgs
{
    public CachedSound PrevSound { get; } = prevSound;
    public CachedSound NewSound { get; } = newSound;
}

public class NewReleaseAvailableEvent(AppRelease appRelease, UpdateMode updateMode)
    : UpdateChecker.NewReleaseEvent(appRelease)
{
    public UpdateMode UpdateMode { get; } = updateMode;
}

public class DeviceDefaultChangedEvent(DeviceFullInfo device, Role role)
{
    public string DeviceId => Device.Id;
    public Role Role { get; } = role;
    public DeviceFullInfo Device { get; } = device;
}