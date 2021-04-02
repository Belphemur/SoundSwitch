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
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationSound : INotification
    {
        public NotificationTypeEnum TypeEnum => NotificationTypeEnum.SoundNotification;
        public string               Label    => SettingsStrings.notificationOptionSound;

        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(MMDevice audioDevice)
        {
            if (audioDevice.DataFlow != DataFlow.Render)
                return;

            Task.Factory.StartNew(() =>
            {
                using var memoryStreamedSound = GetStreamCopy();
                using var output              = new WasapiOut(audioDevice, AudioClientShareMode.Shared, true, 10);
                output.Init(new WaveFileReader(memoryStreamedSound));
                output.Play();
                while (output.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(500);
                }
            });
        }

        public void OnSoundChanged(CachedSound newSound)
        {
        }

        public NotificationCustomSoundEnum SupportCustomSound() => NotificationCustomSoundEnum.NotSupported;

        public bool NeedCustomSound()
        {
            return false;
        }

        public bool IsAvailable()
        {
            return true;
        }

        public void NotifyProfileChanged(Profile.Profile profile, uint? processId)
        {
            if (profile.Playback == null)
                return;

            using var enumerator = new MMDeviceEnumerator();
            try
            {
                var device = enumerator.GetDevice(profile.Playback.Id);
                NotifyDefaultChanged(device);
            }
            catch (Exception)
            {
                //Ignored
            }
        }

        public void NotifyMuteChanged(string microphoneName, bool newState)
        {
            
        }

        private Stream GetStreamCopy()
        {
            lock (this)
            {
                Configuration.DefaultSound.Position = 0;
                var memoryStreamedSound = new MemoryStream();
                Configuration.DefaultSound.CopyTo(memoryStreamedSound);
                memoryStreamedSound.Position = 0;
                return memoryStreamedSound;
            }
        }
    }
}