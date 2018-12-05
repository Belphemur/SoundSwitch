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
using System.Threading;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Framework.Configuration.Device;
using SoundSwitch.Framework.NotificationManager;

namespace SoundSwitch.Model
{
    public class CachedAudioDeviceLister : IAudioDeviceLister
    {
        private readonly DeviceState _state;
        private ICollection<DeviceFullInfo> _playbackCollection;
        private ICollection<DeviceFullInfo> _recordingCollection;

        public CachedAudioDeviceLister(DeviceState state)
        {
            _state = state;
            Refresh();
            MMNotificationClient.Instance.DevicesChanged += DeviceChanged;
        }

        private void DeviceChanged(object sender, DeviceChangedEventBase e)
        {
            Refresh();
        }

        private void Refresh()
        {
            if (!Monitor.TryEnter(this, 500))
            {
                return;
            }
            try
            {

                var playbackTask = Task<ICollection<DeviceFullInfo>>.Factory.StartNew((() =>
                {
                    using (var enumerator = new MMDeviceEnumerator())
                    {
                        return CreateDeviceList(enumerator.EnumerateAudioEndPoints(DataFlow.Render, _state));
                    }
                }));
                var recordingTask = Task<ICollection<DeviceFullInfo>>.Factory.StartNew((() =>
                {
                    using (var enumerator = new MMDeviceEnumerator())
                    {
                        return CreateDeviceList(enumerator.EnumerateAudioEndPoints(DataFlow.Capture, _state));
                    }
                }));
                _playbackCollection = playbackTask.Result;
                _recordingCollection = recordingTask.Result;
            }
            finally
            {
                Monitor.Exit(this);
            }

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
                    sortedDevices.Add(device.FriendlyName, deviceInfo);
                }
                catch (Exception)
                {
                    Log.Warning("Can't get name of device {device}", device.ID);
                }

            }

            return sortedDevices.Values;
        }

        /// <summary>
        /// Get the playback device in the set state
        /// </summary>
        /// <returns></returns>
        public ICollection<DeviceFullInfo> GetPlaybackDevices()
        {
            return _playbackCollection;
        }


        /// <summary>
        /// Get the recording device in the set state
        /// </summary>
        /// <returns></returns>
        public ICollection<DeviceFullInfo> GetRecordingDevices()
        {
            return _recordingCollection;
        }

        public void Dispose()
        {
            MMNotificationClient.Instance.DevicesChanged -= DeviceChanged;
        }
    }
}