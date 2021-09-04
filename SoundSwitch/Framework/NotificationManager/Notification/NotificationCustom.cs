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
using System.Threading;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Serilog;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationCustom : INotification
    {
        private CancellationTokenSource _cancellationTokenSource;
        public NotificationTypeEnum TypeEnum => NotificationTypeEnum.CustomNotification;
        public string Label => SettingsStrings.notificationOptionCustomized;

        public INotificationConfiguration Configuration { get; set; }


        public void NotifyDefaultChanged(MMDevice audioDevice)
        {
            if (audioDevice.DataFlow != DataFlow.Render)
                return;
            
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token);
                    using var player = new WasapiOut(audioDevice, AudioClientShareMode.Shared, true, 200);
                    await using var waveStream = new CachedSoundWaveStream(Configuration.CustomSound);
                    player.Init(waveStream);

                    player.PlaybackStopped += (_, _) => cts.CancelAfter(TimeSpan.FromMilliseconds(750));
                    player.Play();
                    await Task.Delay(-1, cts.Token);
                }
                catch (TaskCanceledException)
                {
                    //Ignored
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Issue while playing {sound}", Configuration.CustomSound.FilePath);
                }
            }, _cancellationTokenSource.Token);
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
    }
}