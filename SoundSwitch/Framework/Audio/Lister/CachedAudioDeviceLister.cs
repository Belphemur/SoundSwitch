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
using System.Reactive.Linq;
using System.Reactive.Subjects;
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

        private readonly ISubject<DefaultDevicePayload> _defaultDeviceChanged = new Subject<DefaultDevicePayload>();
        public IObservable<DefaultDevicePayload> DefaultDeviceChanged => _defaultDeviceChanged.AsObservable();

        private readonly ISubject<DeviceVolumeChangedPayload> _deviceVolumeChanged = new Subject<DeviceVolumeChangedPayload>();
        public IObservable<DeviceVolumeChangedPayload> DeviceVolumeChanged => _deviceVolumeChanged.AsObservable();

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

        private void SubscribeToDeviceEvents(DeviceFullInfo deviceFullInfo)
        {
            // Subscribe to volume change events for this device
            deviceFullInfo.MuteVolumeChanged += DeviceOnMuteVolumeChanged;
            // Attempt to subscribe to OS-level volume notifications
            AudioSwitcher.Instance.InteractWithDevice(deviceFullInfo, device =>
            {
                // Subscribe to OS-level volume notifications
                device.SubscribeToVolumeNotifications();
                return device;
            });
        }

        private void UnsubscribeFromDeviceEvents(DeviceFullInfo deviceFullInfo)
        {
            // Unsubscribe from volume change events
            deviceFullInfo.MuteVolumeChanged -= DeviceOnMuteVolumeChanged;
            // Note: OS-level unsubscription happens within deviceFullInfo.Dispose()
        }

        private void DeviceOnMuteVolumeChanged(object sender, VolumeChangedEventArgs e)
        {
            if (sender is DeviceFullInfo device)
            {
                // Create and emit volume change payload through the subject
                _deviceVolumeChanged.OnNext(new DeviceVolumeChangedPayload(device, e));
            }
        }

        private void DisposeDevice(DeviceFullInfo deviceFullInfo)
        {
            UnsubscribeFromDeviceEvents(deviceFullInfo);

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
            bool GetDevice(DeviceChangedEvent deviceChangedEvent, out DeviceFullInfo device)
            {
                device = AudioSwitcher.Instance.GetAudioEndpoint(deviceChangedEvent.DeviceId);
                if (device == null)
                {
                    _context.Warning("Can't get device {deviceId}", deviceChangedEvent.DeviceId);
                    return true;
                }

                return false;
            }

            void UpdateDeviceCache(DeviceChangedEvent deviceChangedEvent)
            {
                if (GetDevice(deviceChangedEvent, out var device)) return;

                switch (device.Type)
                {
                    case DataFlow.Render:
                        if (PlaybackDevices.TryGetValue(device.Id, out var oldPlaybackDevice))
                        {
                            DisposeDevice(oldPlaybackDevice);
                        }

                        PlaybackDevices[device.Id] = device;
                        SubscribeToDeviceEvents(device);
                        break;
                    case DataFlow.Capture:
                        if (RecordingDevices.TryGetValue(device.Id, out var oldRecordingDevice))
                        {
                            DisposeDevice(oldRecordingDevice);
                        }

                        RecordingDevices[device.Id] = device;
                        SubscribeToDeviceEvents(device);
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
                            if (!PlaybackDevices.TryGetValue(deviceChangedEvent.DeviceId, out var device) && !RecordingDevices.TryGetValue(deviceChangedEvent.DeviceId, out device))
                            {
                                _context.Warning("Can't get device {deviceId}", deviceChangedEvent.DeviceId);
                                break;
                            }

                            _defaultDeviceChanged.OnNext(new DefaultDevicePayload(device, ((DefaultDeviceChangedEvent)deviceChangedEvent).Role));
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

                        // Subscription is now handled after adding to the dictionary
                        // SubscribeToDeviceEvents(deviceInfo);

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

                    // Dispose old devices first
                    foreach (var device in PlaybackDevices.Union(RecordingDevices))
                    {
                        DisposeDevice(device.Value);
                    }

                    // Update caches
                    PlaybackDevices = playbackDevices;
                    RecordingDevices = recordingDevices;

                    // Now subscribe to events for the new devices in the cache
                    foreach (var device in PlaybackDevices.Values.Concat(RecordingDevices.Values))
                    {
                        SubscribeToDeviceEvents(device);
                    }


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

            // Dispose subjects and clear all subscriptions
            (_defaultDeviceChanged as Subject<DefaultDevicePayload>)?.Dispose();
            (_deviceVolumeChanged as Subject<DeviceVolumeChangedPayload>)?.Dispose();

            _refreshCancellationTokenSource.Dispose();
        }
    }
}