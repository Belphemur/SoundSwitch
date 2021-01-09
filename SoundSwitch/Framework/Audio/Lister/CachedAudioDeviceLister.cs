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
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Model;
using SoundSwitch.Util.Timer;

namespace SoundSwitch.Framework.Audio.Lister
{
    public class CachedAudioDeviceLister : IAudioDeviceLister
    {
        /// <inheritdoc />
        public IReadOnlyCollection<DeviceFullInfo> PlaybackDevices { get; private set; } = new List<DeviceFullInfo>();

        /// <inheritdoc />
        public IReadOnlyCollection<DeviceFullInfo> RecordingDevices { get; private set; } = new List<DeviceFullInfo>();

        private readonly DeviceState _state;
        private readonly DebounceDispatcher _dispatcher = new();

        public CachedAudioDeviceLister(DeviceState state)
        {
            _state = state;
            MMNotificationClient.Instance.DevicesChanged += DeviceChanged;
        }

        private void DeviceChanged(object sender, DeviceChangedEventBase e)
        {
            _dispatcher.Debounce(100, o => Refresh());
        }

        private void UpdatePlayback()
        {
            Log.Information("Refreshing playback device of state {@State}", _state);
            using var enumerator = new MMDeviceEnumerator();
            PlaybackDevices = CreateDeviceList(enumerator.EnumerateAudioEndPoints(DataFlow.Render, _state));
            Log.Information("Refreshed playbacks: {@Playbacks}", PlaybackDevices.Select(info => new {info.Name, info.Id}));
        }

        private void UpdateRecording()
        {
            Log.Information("Refreshing recording device of state {@State}", _state);
            using var enumerator = new MMDeviceEnumerator();
            RecordingDevices = CreateDeviceList(enumerator.EnumerateAudioEndPoints(DataFlow.Capture, _state));
            Log.Information("Refreshed recordings: {@Recordings}", RecordingDevices.Select(info => new {info.Name, info.Id}));
        }

        public void Refresh()
        {
            Log.Information("Refreshing device of state {@State}", _state);
            var threadPlayback = new Thread(UpdatePlayback)
            {
                Name = $"Playback Refresh {_state}",
                IsBackground = true
            };
            var threadRecording = new Thread(UpdateRecording)  
            {
                Name = $"Recording Refresh {_state}",
                IsBackground = true
            };
            
            threadPlayback.Start();
            threadRecording.Start();

            threadPlayback.Join();
            threadRecording.Join();

            Log.Information("Refreshed device of state {@State}", _state);
        }

        private static IReadOnlyCollection<DeviceFullInfo> CreateDeviceList(MMDeviceCollection collection)
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

            return sortedDevices.Values.ToArray();
        }

        public void Dispose()
        {
            MMNotificationClient.Instance.DevicesChanged -= DeviceChanged;
        }
    }
}