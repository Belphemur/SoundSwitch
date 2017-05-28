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

using System.Threading;
using System.Threading.Tasks;
using AudioEndPointControllerWrapper;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationCustom : INotification
    {
        private MMDeviceEnumerator _deviceEnumerator;

        public NotificationTypeEnum TypeEnum => NotificationTypeEnum.CustomNotification;
        public string Label => SettingsStrings.notificationOptionCustomized;

        public INotificationConfiguration Configuration { get; set; }

        private MMDeviceEnumerator GetEnumerator()
        {
            if (_deviceEnumerator != null)
                return _deviceEnumerator;

            return _deviceEnumerator = new MMDeviceEnumerator();
        }

        public void NotifyDefaultChanged(IAudioDevice audioDevice)
        {
            if (audioDevice.Type != AudioDeviceType.Playback)
                return;
            var task = new Task(() =>
            {
                var device = GetEnumerator().GetDevice(audioDevice.Id);
                using (var output = new WasapiOut(device, AudioClientShareMode.Shared, true, 10))
                using (var waveStream = new CachedSoundWaveStream(Configuration.CustomSound))
                {
                    output.Init(waveStream);
                    output.Play();
                    while (output.PlaybackState == PlaybackState.Playing)
                    {
                        Thread.Sleep(500);
                    }
                }
            });
            task.Start();
        }

        public void OnSoundChanged(CachedSound newSound)
        {
            Configuration.CustomSound = newSound;
        }

        public NotificationCustomSoundEnum SupportCustomSound() => NotificationCustomSoundEnum.Required;

        public bool NeedCustomSound()
        {
            return true;
        }

        public bool IsAvailable()
        {
            return true;
        }
    }
}