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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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
                DataFlow.Render => new DeviceReadOnlyCollection<DeviceFullInfo>(PlaybackDevices.Where(info => state.HasFlag(info.State)), type),
                DataFlow.Capture => new DeviceReadOnlyCollection<DeviceFullInfo>(RecordingDevices.Where(info => state.HasFlag(info.State)), type),
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        private readonly DeviceState _state;
        private readonly ILogger _context;
        private uint _threadSafeRefreshing;
        private CancellationTokenSource _refreshCancellationTokenSource = new CancellationTokenSource();

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
            var logContext = _context.ForContext("TaskID", Task.CurrentId).ForContext("ThreadID", Environment.CurrentManagedThreadId);
            // Cancel the previous refresh operation, if any
            var previousCts = Interlocked.Exchange(ref _refreshCancellationTokenSource, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
            if (previousCts != null)
            {
                logContext.Information("Cancelling Previous Context");
                previousCts.Cancel();
                previousCts.Dispose();
            }

            cancellationToken = _refreshCancellationTokenSource.Token;

            var stopWatch = Stopwatch.StartNew();
            try
            {
                Refreshing = true;
                var playbackDevices = new Dictionary<string, DeviceFullInfo>();
                var recordingDevices = new Dictionary<string, DeviceFullInfo>();

                using var registration = cancellationToken.Register(_ => { logContext.Warning("Cancellation received."); }, null);

                try
                {
                    logContext.Information("Refreshing all devices");
                    var enumerator = new MMDeviceEnumerator();
                    using var _ = enumerator.DisposeOnCancellation(cancellationToken);
                    foreach (var endPoint in enumerator.EnumerateAudioEndPoints(DataFlow.All, _state))
                    {
                        var deviceContextLogger = logContext.ForContext("Device", endPoint.ID).ForContext("DeviceName", endPoint.FriendlyName).ForContext("DeviceState", endPoint.State).ForContext("DeviceType", endPoint.DataFlow);
                        deviceContextLogger.Verbose("Refreshing device");
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
                            logContext.Warning(e, "Can't get name of device {device}", endPoint.ID);
                        }
                        finally
                        {
                            deviceContextLogger.Verbose("Device refreshed");
                        }
                    }

                    foreach (var device in PlaybackDevices.Union(RecordingDevices))
                    {
                        device.Dispose();
                    }

                    PlaybackDevices = playbackDevices.Values.ToArray();
                    RecordingDevices = recordingDevices.Values.ToArray();


                    logContext.Information("Refreshed all devices in {@StopTime}. {@Recording}/rec, {@Playback}/play", stopWatch.Elapsed, recordingDevices.Count, playbackDevices.Count);
                }
                //If cancellation token is cancelled, its expected to throw null since the device enumerator has been disposed
                catch (Exception e) when (cancellationToken.IsCancellationRequested && e is NullReferenceException or InvalidComObjectException)
                {
                    logContext.Information(e, "Cancellation requested and enumerator is disposed, ignoring");
                }
                catch (Exception e) when (!cancellationToken.IsCancellationRequested)
                {
                    logContext.Error(e, "Can't refresh the devices");
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

            _refreshCancellationTokenSource.Dispose();
        }
    }
}