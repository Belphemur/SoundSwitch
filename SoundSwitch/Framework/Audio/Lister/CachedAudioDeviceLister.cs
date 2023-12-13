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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Common.Framework.Dispose;
using SoundSwitch.Framework.Audio.Lister.Job;
using SoundSwitch.Framework.NotificationManager;
using SoundSwitch.Framework.Threading;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.Audio.Lister
{
    public class CachedAudioDeviceLister : IAudioDeviceLister
    {
        /// <inheritdoc />
        private DeviceFullInfo[] PlaybackDevices { get; set; } = Array.Empty<DeviceFullInfo>();

        /// <inheritdoc />
        private DeviceFullInfo[] RecordingDevices { get; set; } = Array.Empty<DeviceFullInfo>();

        /// <summary>
        /// Get devices per type and state
        /// </summary>
        /// <param name="type"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public DeviceReadOnlyCollection<DeviceFullInfo> GetDevices(DataFlow type, DeviceState state)
        {
            return type switch
            {
                DataFlow.Render  => new DeviceReadOnlyCollection<DeviceFullInfo>(PlaybackDevices.Where(info => state.HasFlag(info.State)), type),
                DataFlow.Capture => new DeviceReadOnlyCollection<DeviceFullInfo>(RecordingDevices.Where(info => state.HasFlag(info.State)), type),
                _                => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private readonly DeviceState _state;
        private readonly ILogger _context;
        private uint _threadSafeRefreshing;
        private DateTime _lastRefresh = DateTime.UtcNow;

        public bool Refreshing
        {
            get => Interlocked.CompareExchange(ref _threadSafeRefreshing, 1, 1) == 1;
            private set
            {
                if (value)
                {
                    Interlocked.CompareExchange(ref _threadSafeRefreshing, 1, 0);
                }
                else
                {
                    Interlocked.CompareExchange(ref _threadSafeRefreshing, 0, 1);
                }
            }
        }

        public CachedAudioDeviceLister(DeviceState state)
        {
            _state = state;
            MMNotificationClient.Instance.DevicesChanged += DeviceChanged;
            _context = Log.ForContext("State", _state);
        }

        private void DeviceChanged(object sender, DeviceChangedEventBase e)
        {
            _context.Verbose("Device Changed received, triggering job");
            JobScheduler.Instance.ScheduleJob(new DebounceRefreshJob(_state, this, _context), e.Token);
        }

        public void Refresh(CancellationToken cancellationToken = default)
        {
            if (Refreshing)
            {
                _context.Warning("Can't refresh, already refreshing since {LastRefresh}", _lastRefresh);
                //We want to be sure we get the latest refresh, it's not because we are refreshing that we'll get the latest info
                JobScheduler.Instance.ScheduleJob(new DebounceRefreshJob(_state, this, _context));

                return;
            }

            var stopWatch = Stopwatch.StartNew();
            try
            {
                Refreshing = true;
                _lastRefresh = DateTime.UtcNow;
                var playbackDevices = new Dictionary<string, DeviceFullInfo>();
                var recordingDevices = new Dictionary<string, DeviceFullInfo>();

                using var registration = cancellationToken.Register(_ => { _context.Warning("Cancellation received."); }, null);

                try
                {
                    _context.Information("Refreshing all devices");
                    using var enumerator = new MMDeviceEnumerator();
                    using var _ = enumerator.DisposeOnCancellation(cancellationToken);
                    foreach (var endPoint in enumerator.EnumerateAudioEndPoints(DataFlow.All, _state))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
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
                            _context.Warning(e, "Can't get name of device {device}", endPoint.ID);
                        }
                    }

                    foreach (var device in PlaybackDevices.Union(RecordingDevices))
                    {
                        device.Dispose();
                    }

                    PlaybackDevices = playbackDevices.Values.ToArray();
                    RecordingDevices = recordingDevices.Values.ToArray();


                    _context.Information("Refreshed all devices in {@StopTime}. {@Recording}/rec, {@Playback}/play", stopWatch.Elapsed, recordingDevices.Count, playbackDevices.Count);
                }
                //If cancellation token is cancelled, its expected to throw null since the device enumerator has been disposed
                catch (NullReferenceException e) when (!cancellationToken.IsCancellationRequested)
                {
                    _context.Error(e, "Can't refresh the devices");
                }
            }
            finally
            {
                Refreshing = false;
                stopWatch.Stop();
            }
        }

        public void Dispose()
        {
            MMNotificationClient.Instance.DevicesChanged -= DeviceChanged;

            foreach (var device in PlaybackDevices.Union(RecordingDevices))
            {
                device.Dispose();
            }
        }
    }
}