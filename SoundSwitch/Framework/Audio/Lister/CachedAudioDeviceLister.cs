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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Framework.Audio.Device;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Model;
using SoundSwitch.Util.Timer;

namespace SoundSwitch.Framework.Audio.Lister
{
    public class CachedAudioDeviceLister : IAudioDeviceLister
    {
        /// <inheritdoc />
        public ICollection<DeviceFullInfo> PlaybackDevices { get; private set; }

        /// <inheritdoc />
        public ICollection<DeviceFullInfo> RecordingDevices { get; private set; }

        private readonly DeviceState _state;
        private readonly DebounceDispatcher _dispatcher = new DebounceDispatcher();

        public CachedAudioDeviceLister(DeviceState state)
        {
            _state = state;
            MMNotificationClient.Instance.DevicesChanged += DeviceChanged;
        }

        private void DeviceChanged(object sender, DeviceChangedEventBase e)
        {
            _dispatcher.Debounce(300, async (o) => await Refresh());
        }

        public async Task Refresh()
        {
            Log.Information("Refreshing device of state {@State}", _state);
            var playbackTask = Task<ICollection<DeviceFullInfo>>.Factory.StartNew((() =>
            {
                Log.Information("Refreshing playback device of state {@State}", _state);
                using var enumerator = new MMDeviceEnumerator();
                var playback = CreateDeviceList(enumerator.EnumerateAudioEndPoints(DataFlow.Render, _state));
                Log.Information("Refreshed playbacks: {@Playbacks}", playback.Select(info => new {Name = info.Name, Id = info.Id}));
                return playback;
            }));
            var recordingTask = Task<ICollection<DeviceFullInfo>>.Factory.StartNew((() =>
            {
                Log.Information("Refreshing recording device of state {@State}", _state);
                using var enumerator = new MMDeviceEnumerator();
                var recordings = CreateDeviceList(enumerator.EnumerateAudioEndPoints(DataFlow.Capture, _state));
                Log.Information("Refreshed recordings: {@Recordings}", recordings.Select(info => new {Name = info.Name, Id = info.Id}));
                return recordings;
            }));
            PlaybackDevices = await playbackTask;
            RecordingDevices = await recordingTask;

            Log.Information("Refreshed device of state {@State}", _state);
        }

        private static ICollection<DeviceFullInfo> CreateDeviceList(MMDeviceCollection collection)
        {
            var sortedDevices = new SortedList<string, DeviceFullInfo>();
            foreach (var device in collection)
            {
                try
                {
                    var deviceInfo = new DeviceFullInfo(device);
                    if (string.IsNullOrEmpty(deviceInfo.Name))
                    {
                        continue;
                    }

                    sortedDevices.Add(device.ID, deviceInfo);
                }
                catch (Exception)
                {
                    Log.Warning("Can't get name of device {device}", device.ID);
                }
            }

            return sortedDevices.Values;
        }

        public void Dispose()
        {
            MMNotificationClient.Instance.DevicesChanged -= DeviceChanged;
        }
    }
}