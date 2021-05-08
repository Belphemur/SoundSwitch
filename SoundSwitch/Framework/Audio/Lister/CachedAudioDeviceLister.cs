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
using System.Collections.Concurrent;
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
        public IReadOnlyCollection<DeviceFullInfo> PlaybackDevices => (IReadOnlyCollection<DeviceFullInfo>) _playbackDevices.Values;

        /// <inheritdoc />
        public IReadOnlyCollection<DeviceFullInfo> RecordingDevices => (IReadOnlyCollection<DeviceFullInfo>) _recordingDevices.Values;

        private readonly DeviceState _state;
        private readonly DebounceDispatcher _dispatcher = new();
        private readonly object _lock = new();
        private readonly ConcurrentDictionary<string, DeviceFullInfo> _playbackDevices = new();
        private readonly ConcurrentDictionary<string,DeviceFullInfo> _recordingDevices = new();

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
                var ids = new HashSet<string>();
                Log.Information("[{@State}] Refreshing all devices", _state);
                using var enumerator = new MMDeviceEnumerator();
                foreach (var endPoint in enumerator.EnumerateAudioEndPoints(DataFlow.All, _state))
                {
                    ids.Add(endPoint.ID);
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
                                _playbackDevices.AddOrUpdate(deviceInfo.Id, deviceInfo, (_, _) => deviceInfo);
                                break;
                            case DataFlow.Capture:
                                _recordingDevices.AddOrUpdate(deviceInfo.Id, deviceInfo, (_, _) => deviceInfo);
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

                foreach (var deviceId in _playbackDevices.Keys.Except(ids))
                {
                    _playbackDevices.TryRemove(deviceId, out _);
                }

                foreach (var deviceId in _recordingDevices.Keys.Except(ids))
                {
                    _recordingDevices.TryRemove(deviceId, out _);
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