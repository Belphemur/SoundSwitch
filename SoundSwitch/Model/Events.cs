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
using AudioDefaultSwitcherWrapper;
using NAudio.CoreAudioApi;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Updater;

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
        public DeviceListChanged(IEnumerable<string> seletedDevicesList, DeviceType type)
        {
            SeletedDevicesList = seletedDevicesList;
            Type = type;
        }

        public IEnumerable<string> SeletedDevicesList { get; private set; }
        public DeviceType Type { get; private set; }
    }

    public class NotificationSettingsUpdatedEvent : EventArgs
    {
        public NotificationTypeEnum PrevSettings { get;}
        public NotificationTypeEnum NewSettings { get; }

        public NotificationSettingsUpdatedEvent(NotificationTypeEnum prevSettings, NotificationTypeEnum newSettings)
        {
            PrevSettings = prevSettings;
            NewSettings = newSettings;
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

        public NewReleaseAvailableEvent(Release release, UpdateMode updateMode) : base(release)
        {
            UpdateMode = updateMode;
        }
    }

    public class DeviceDefaultChangedEvent : EventArgs
    {
        public DeviceDefaultChangedEvent(string deviceId, MMDevice device)
        {
            DeviceId = deviceId;
            Device = device;
        }

        public string DeviceId { get; }
        public MMDevice Device { get; }
    }
}