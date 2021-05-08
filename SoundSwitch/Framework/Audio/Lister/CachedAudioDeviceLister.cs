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
        public IReadOnlyCollection<DeviceFullInfo> PlaybackDevices
        {
            get
            {
                lock (_lockPlayback)
                {
                    return _playbackList;
                }
            }
        }

        /// <inheritdoc />
        public IReadOnlyCollection<DeviceFullInfo> RecordingDevices
        {
            get
            {
                lock (_lockRecording)
                {
                    return _recordingList;
                }
            }
        }

        private readonly DeviceState _state;
        private readonly DebounceDispatcher _dispatcher = new();
        private readonly object _lockRefresh = new();
        private readonly object _lockRecording = new();
        private readonly object _lockPlayback = new();
        private IReadOnlyCollection<DeviceFullInfo> _playbackList = new DeviceFullInfo[0];
        private IReadOnlyCollection<DeviceFullInfo> _recordingList = new DeviceFullInfo[0];


        public CachedAudioDeviceLister(DeviceState state)
        {
            _state = state;
            MMNotificationClient.Instance.DevicesChanged += DeviceChanged;
        }

        private void DeviceChanged(object sender, DeviceChangedEventBase e)
        {
            _dispatcher.Debounce(100, o => Refresh());
        }

        public void Refresh()
        {
            var playbackDevices = new Dictionary<string, DeviceFullInfo>();
            var recordingDevices = new Dictionary<string, DeviceFullInfo>();
            lock (_lockRefresh)
            {
                Log.Information("[{@State}] Refreshing all devices", _state);
                using var enumerator = new MMDeviceEnumerator();
                foreach (var endPoint in enumerator.EnumerateAudioEndPoints(DataFlow.All, _state))
                {
                    try
                    {
                        var deviceInfo = new DeviceFullInfo(endPoint);
                        if (string.IsNullOrEmpty(deviceInfo.Name))
                        {
                            continue;
                        }

                        switch (deviceInfo.Type)
                        {
                            case DataFlow.Render:
                                playbackDevices.Add(deviceInfo.Id, deviceInfo);
                                break;
                            case DataFlow.Capture:
                                recordingDevices.Add(deviceInfo.Id, deviceInfo);
                                break;
                            case DataFlow.All:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warning(e, "Can't get name of device {device}", endPoint.ID);
                    }
                }

                lock (_lockPlayback)
                {
                    _playbackList = playbackDevices.Values.ToArray();
                }

                lock (_lockRecording)
                {
                    _recordingList = recordingDevices.Values.ToArray();
                }

                Log.Information("[{@State}] Refreshed all devices. {@Recording}/rec, {@Playback}/play", _state, recordingDevices.Count, playbackDevices.Count);
            }
        }

        public void Dispose()
        {
            MMNotificationClient.Instance.DevicesChanged -= DeviceChanged;
        }
    }
}