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
using System.Drawing;
using System.Threading;
using NAudio.CoreAudioApi;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Audio.Play;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Framework.Threading;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationCustom : INotification
    {
        private CancellationTokenSource _cancellationTokenSource;
        public NotificationTypeEnum TypeEnum => NotificationTypeEnum.CustomNotification;
        public string Label => SettingsStrings.notificationOptionCustomized;

        public INotificationConfiguration Configuration { get; set; }


        public void NotifyDefaultChanged(DeviceFullInfo audioDevice)
        {
            if (audioDevice.Type != DataFlow.Render)
                return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            JobScheduler.Instance.ScheduleJob(new PlaySoundJob(audioDevice.Id, Configuration.CustomSound), _cancellationTokenSource.Token);
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

        public void NotifyProfileChanged(Profile.Profile profile, Bitmap icon, uint? processId)
        {
            if (profile.Playback == null)
                return;

            try
            {
                var mmDevice = AudioSwitcher.Instance.GetDevice(profile.Playback.Id);
                var device = AudioSwitcher.Instance.InteractWithMmDevice(mmDevice, device => new DeviceFullInfo(device));
                NotifyDefaultChanged(device);
            }
            catch (Exception)
            {
                //Ignored
            }
        }

        public void NotifyMuteChanged(string microphoneName, bool newMuteState)
        {
        }
    }
}