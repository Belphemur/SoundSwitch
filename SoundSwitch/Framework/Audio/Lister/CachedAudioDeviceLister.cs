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
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.Audio.Lister
{
    public class CachedAudioDeviceLister : IAudioDeviceLister
    {
        /// <inheritdoc />
        private Dictionary<string, DeviceFullInfo> PlaybackDevices { get; set; } = new();

        /// <inheritdoc />
        private Dictionary<string, DeviceFullInfo> RecordingDevices { get; set; } = new();

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
                DataFlow.Render => new DeviceReadOnlyCollection<DeviceFullInfo>(PlaybackDevices.Values.Where(info => state.HasFlag(info.State)), type),
                DataFlow.Capture => new DeviceReadOnlyCollection<DeviceFullInfo>(RecordingDevices.Values.Where(info => state.HasFlag(info.State)), type),
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
            _context = Log.ForContext("State", _state);
        }

        private void DisposeDevice(DeviceFullInfo deviceFullInfo)
        {
            _ = AudioSwitcher.Instance.InteractWithDevice(deviceFullInfo, device =>
            {
                device.Dispose();
                return device;
            });
        }

        /// <summary>
        /// Process device updates
        /// </summary>
        /// <param name="deviceChangedEvents"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void ProcessDeviceUpdates(IEnumerable<DeviceChangedEvent> deviceChangedEvents)
        {
            void UpdateDeviceCache(DeviceChangedEvent deviceChangedEvent)
            {
                var device = AudioSwitcher.Instance.GetAudioEndpoint(deviceChangedEvent.DeviceId);
                if (device == null)
                {
                    _context.Warning("Can't get device {deviceId}", deviceChangedEvent.DeviceId);
                    return;
                }

                switch (device.Type)
                {
                    case DataFlow.Render:
                        if (PlaybackDevices.TryGetValue(device.Id, out var oldPlaybackDevice))
                        {
                            DisposeDevice(oldPlaybackDevice);
                        }

                        PlaybackDevices[device.Id] = device;
                        break;
                    case DataFlow.Capture:
                        if (RecordingDevices.TryGetValue(device.Id, out var oldRecordingDevice))
                        {
                            DisposeDevice(oldRecordingDevice);
                        }

                        RecordingDevices[device.Id] = device;
                        break;
                    case DataFlow.All:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _context.Information("Updated device {deviceId} in cache", device.Id);
            }

            foreach (var deviceChangedEvent in deviceChangedEvents)
            {
                try
                {
                    switch (deviceChangedEvent.Action)
                    {
                        case EventType.Removed:
                            if (PlaybackDevices.Remove(deviceChangedEvent.DeviceId, out var playbackDevice))
                            {
                                DisposeDevice(playbackDevice);
                            }

                            if (RecordingDevices.Remove(deviceChangedEvent.DeviceId, out var recordingDevice))
                            {
                                DisposeDevice(recordingDevice);
                            }

                            break;
                        case EventType.Added:
                        case EventType.StateChanged:
                        case EventType.PropertyChanged:
                            UpdateDeviceCache(deviceChangedEvent);
                            break;
                        case EventType.DefaultChanged:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception e)
                {
                    _context.Warning(e, "Couldn't process event: {event} for device {deviceId}", deviceChangedEvent.Action, deviceChangedEvent.DeviceId);
                }
            }
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
                    foreach (var deviceInfo in AudioSwitcher.Instance.GetAudioEndpoints((EDataFlow)DataFlow.All, (EDeviceState)_state))
                    {
                        cancellationToken.ThrowIfCancellationRequested();
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

                    foreach (var device in PlaybackDevices.Union(RecordingDevices))
                    {
                        DisposeDevice(device.Value);
                    }

                    PlaybackDevices = playbackDevices;
                    RecordingDevices = recordingDevices;


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
            foreach (var device in PlaybackDevices.Union(RecordingDevices))
            {
                DisposeDevice(device.Value);
            }

            _refreshCancellationTokenSource.Dispose();
        }
    }
}