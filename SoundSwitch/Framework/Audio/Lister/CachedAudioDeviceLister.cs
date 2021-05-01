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
        public IReadOnlyCollection<DeviceFullInfo> PlaybackDevices => _playbackDevices;

        /// <inheritdoc />
        public IReadOnlyCollection<DeviceFullInfo> RecordingDevices => _recordingDevices;

        private readonly DeviceState _state;
        private readonly DebounceDispatcher _dispatcher = new();
        private readonly object _lock = new();
        private readonly HashSet<DeviceFullInfo> _playbackDevices = new();
        private readonly HashSet<DeviceFullInfo> _recordingDevices = new();

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
            lock (_lock)
            {
                _recordingDevices.Clear();
                _playbackDevices.Clear();
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
                                _playbackDevices.Add(deviceInfo);
                                break;
                            case DataFlow.Capture:
                                _recordingDevices.Add(deviceInfo);
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

                Log.Information("[{@State}] Refreshed all devices. {@Recording}/rec, {@Playback}/play", _state, _recordingDevices.Count, _playbackDevices.Count);
            }
        }

        public void Dispose()
        {
            MMNotificationClient.Instance.DevicesChanged -= DeviceChanged;
        }
    }
}