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
        public DeviceReadOnlyCollection<DeviceFullInfo> PlaybackDevices { get; private set; } = new(Enumerable.Empty<DeviceFullInfo>(), DataFlow.Render);

        /// <inheritdoc />
        public DeviceReadOnlyCollection<DeviceFullInfo> RecordingDevices { get; private set; } = new(Enumerable.Empty<DeviceFullInfo>(), DataFlow.Capture);

        private readonly DeviceState _state;
        private readonly SemaphoreSlim _refreshSemaphore = new(1);
        private readonly TimeSpan _refreshWaitTime = TimeSpan.FromSeconds(5);
        private readonly ILogger _context;
        private uint _threadSafeRefreshing = 0;

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
            Refreshing = true;
            _context.Verbose("Device Changed received, triggering job");
            JobScheduler.Instance.ScheduleJob(new DebounceRefreshJob(_state, this, _context), e.Token);
        }

        public void Refresh(CancellationToken cancellationToken = default)
        {
            try
            {
                var playbackDevices = new Dictionary<string, DeviceFullInfo>();
                var recordingDevices = new Dictionary<string, DeviceFullInfo>();

                if (!_refreshSemaphore.Wait(_refreshWaitTime, cancellationToken))
                {
                    _context.Error("Can't refresh the devices after {time}", _refreshWaitTime);
                    return;
                }

                using var registration = cancellationToken.Register(_ => { _context.Warning("Cancellation received."); }, null);

                try
                {
                    _context.Information("Refreshing all devices");
                    var enumerator = new MMDeviceEnumerator();
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

                    PlaybackDevices = new DeviceReadOnlyCollection<DeviceFullInfo>(playbackDevices.Values, DataFlow.Render);
                    RecordingDevices = new DeviceReadOnlyCollection<DeviceFullInfo>(recordingDevices.Values, DataFlow.Capture);


                    _context.Information("Refreshed all devices. {@Recording}/rec, {@Playback}/play", recordingDevices.Count, playbackDevices.Count);
                }
                //If cancellation token is cancelled, its expected to throw null since the device enumerator has been disposed
                catch (NullReferenceException e) when (!cancellationToken.IsCancellationRequested)
                {
                    _context.Error(e, "Can't refresh the devices");
                }
                finally
                {
                    _refreshSemaphore.Release();
                }
            }
            finally
            {
                Refreshing = false;
            }
        }

        public void Dispose()
        {
            MMNotificationClient.Instance.DevicesChanged -= DeviceChanged;
        }
    }
}