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
using Serilog;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.NotificationManager.Notification.Configuration;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.NotificationManager.Notification
{
    public class NotificationSound : INotification
    {
        private CancellationTokenSource _cancellationTokenSource;
        public NotificationTypeEnum TypeEnum => NotificationTypeEnum.SoundNotification;
        public string Label => SettingsStrings.notificationOptionSound;

        public INotificationConfiguration Configuration { get; set; }

        public void NotifyDefaultChanged(DeviceFullInfo audioDevice)
        {
            if (audioDevice.Type != DataFlow.Render)
                return;

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(async () =>
            {
                try
                {
                    using var enumerator = new MMDeviceEnumerator();
                    using var semaphore = new SemaphoreSlim(0);
                    using var player = new WasapiOut(enumerator.GetDevice(audioDevice.Id), AudioClientShareMode.Shared, true, 200);
                    await using var memoryStreamedSound = GetStreamCopy();
                    await using var waveStream = new WaveFileReader(memoryStreamedSound);
                    player.Init(waveStream);

                    player.PlaybackStopped += (_, _) =>
                    {
                        if (_cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            return;
                        }

                        semaphore.Release();
                    };
                    player.Play();
                    await semaphore.WaitAsync(_cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    //Ignored
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Issue while playing default sound");
                }
            }, _cancellationTokenSource.Token);
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