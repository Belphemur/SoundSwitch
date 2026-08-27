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
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Collection;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.Audio.Lister;

public class CachedAudioDeviceLister : IAudioDeviceLister
{
    // Dedicated .NET 9 lock serializing the two mutation paths (Refresh's wholesale publish and
    // ProcessDeviceUpdates' incremental edits). It is held ONLY for the immutable-dictionary
    // reference swaps (microseconds) — never during COM enumeration, event subscription, or
    // device disposal. Readers never take it: they read the published immutable snapshot, which
    // is inherently safe to enumerate.
    private readonly Lock _cacheLock = new Lock();

    /// <inheritdoc />
    private ImmutableDictionary<string, DeviceFullInfo> PlaybackDevices { get; set; } = ImmutableDictionary<string, DeviceFullInfo>.Empty;

    /// <inheritdoc />
    private ImmutableDictionary<string, DeviceFullInfo> RecordingDevices { get; set; } = ImmutableDictionary<string, DeviceFullInfo>.Empty;

    private readonly ISubject<DefaultDevicePayload> _defaultDeviceChanged = new Subject<DefaultDevicePayload>();
    public IObservable<DefaultDevicePayload> DefaultDeviceChanged => _defaultDeviceChanged.AsObservable();

    private readonly ISubject<DeviceVolumeChangedPayload> _deviceVolumeChanged = new Subject<DeviceVolumeChangedPayload>();
    public IObservable<DeviceVolumeChangedPayload> DeviceVolumeChanged => _deviceVolumeChanged.AsObservable();

    private readonly ISubject<Unit> _deviceListRefreshed = new Subject<Unit>();

    /// <summary>
    /// Observable that emits when the device list has been successfully and fully refreshed.
    /// Does not emit when a refresh is cancelled or fails.
    /// </summary>
    public IObservable<Unit> DeviceListRefreshed => _deviceListRefreshed.AsObservable();

    /// <summary>
    /// Get devices per type and state
    /// </summary>
    /// <param name="type"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public DeviceReadOnlyCollection<DeviceFullInfo> GetDevices(EDataFlow type, EDeviceState state)
    {
        // Lock-free read: the published dictionaries are immutable, so enumerating a snapshot
        // can never throw "Collection was modified" even while another thread swaps the reference.
        return type switch
        {
            EDataFlow.eRender => new DeviceReadOnlyCollection<DeviceFullInfo>(PlaybackDevices.Values.Where(info => state.HasFlag(info.State)), type),
            EDataFlow.eCapture => new DeviceReadOnlyCollection<DeviceFullInfo>(RecordingDevices.Values.Where(info => state.HasFlag(info.State)), type),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    private readonly EDeviceState _state;
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

    public CachedAudioDeviceLister(EDeviceState state)
    {
        _state = state;
        _context = Log.ForContext("State", _state);
    }

    private void SubscribeToDeviceEvents(DeviceFullInfo deviceFullInfo)
    {
        // Subscribe to volume change events for this device
        deviceFullInfo.MuteVolumeChanged += DeviceOnMuteVolumeChanged;
        // Subscribe to OS-level volume notifications (marshalled onto the ComThread internally)
        deviceFullInfo.SubscribeToVolumeNotifications();
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

        // Disposal tears down the underlying device on the ComThread internally
        deviceFullInfo.Dispose();
    }

    /// <summary>
    /// Disposes a collection of devices that has already been snapshotted, so callers can
    /// enumerate a stable copy instead of the live published dictionaries. This avoids
    /// <see cref="InvalidOperationException"/> ("Collection was modified") when another thread
    /// (e.g. <see cref="ProcessDeviceUpdates"/> handling device arrival/removal) mutates
    /// <c>PlaybackDevices</c>/<c>RecordingDevices</c> while we enumerate them.
    /// </summary>
    /// <param name="oldDevices">A materialized snapshot (array) of the devices to dispose. The array
    /// type is deliberate: it forces callers to pass an already-snapshotted collection rather than a
    /// lazy enumeration over the live dictionaries, which would reintroduce the race.</param>
    private void DisposeOldDevices(DeviceFullInfo[] oldDevices)
    {
        foreach (var device in oldDevices)
        {
            DisposeDevice(device);
        }
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

            // The lookup of the replaced device and the swap are a single atomic step under
            // _cacheLock, so we always dispose exactly the instance that was evicted and a
            // concurrent Refresh can never overwrite this edit (or vice versa).
            switch (device.Type)
            {
                case EDataFlow.eRender:
                    DeviceFullInfo oldPlaybackDevice;
                    lock (_cacheLock)
                    {
                        PlaybackDevices.TryGetValue(device.Id, out oldPlaybackDevice);
                        PlaybackDevices = PlaybackDevices.SetItem(device.Id, device);
                    }

                    if (oldPlaybackDevice != null)
                    {
                        DisposeDevice(oldPlaybackDevice);
                    }

                    SubscribeToDeviceEvents(device);
                    break;
                case EDataFlow.eCapture:
                    DeviceFullInfo oldRecordingDevice;
                    lock (_cacheLock)
                    {
                        RecordingDevices.TryGetValue(device.Id, out oldRecordingDevice);
                        RecordingDevices = RecordingDevices.SetItem(device.Id, device);
                    }

                    if (oldRecordingDevice != null)
                    {
                        DisposeDevice(oldRecordingDevice);
                    }

                    SubscribeToDeviceEvents(device);
                    break;
                case EDataFlow.eAll:
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
                        DeviceFullInfo? playbackDevice, recordingDevice;
                        lock (_cacheLock)
                        {
                            PlaybackDevices.TryGetValue(deviceChangedEvent.DeviceId, out playbackDevice);
                            PlaybackDevices = PlaybackDevices.Remove(deviceChangedEvent.DeviceId);
                            RecordingDevices.TryGetValue(deviceChangedEvent.DeviceId, out recordingDevice);
                            RecordingDevices = RecordingDevices.Remove(deviceChangedEvent.DeviceId);
                        }

                        var removed = false;
                        if (playbackDevice != null)
                        {
                            DisposeDevice(playbackDevice);
                            removed = true;
                        }

                        if (recordingDevice != null)
                        {
                            DisposeDevice(recordingDevice);
                            removed = true;
                        }

                        if (removed)
                        {
                            _deviceListRefreshed.OnNext(Unit.Default);
                        }

                        break;
                    case EventType.Added:
                    case EventType.StateChanged:
                    case EventType.PropertyChanged:
                        UpdateDeviceCache(deviceChangedEvent);
                        _deviceListRefreshed.OnNext(Unit.Default);
                        break;
                    case EventType.DefaultChanged:
                        // Read-only lookup on the published immutable snapshots: no lock needed.
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
            // Devices to dispose after publishing: those present before but absent from the new
            // enumeration. Devices newly added this refresh (for post-publish subscription).
            var toDispose = new List<DeviceFullInfo>();
            var newIds = new HashSet<string>();

            using var registration = cancellationToken.Register(_ => { logContext.Warning("Cancellation received."); }, null);

            try
            {
                logContext.Information("Refreshing all devices");
                // Materialize the enumeration up front: if cancellation (or any failure) interrupts
                // the placement loop below, the catch disposes every enumerated entry — placed or
                // not — so no DeviceFullInfo (and the AudioDevice COM reference it owns) is abandoned.
                // The lock is NOT held here: COM enumeration is the slow part and must not block
                // ProcessDeviceUpdates or hold _cacheLock across COM calls.
                var enumeratedDevices = AudioSwitcher.Instance.GetAudioEndpoints(EDataFlow.eAll, _state).ToArray();
                try
                {
                    foreach (var deviceInfo in enumeratedDevices)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // Reconcile: if this device id already exists in the cache (and is still
                        // alive), REUSE the existing DeviceFullInfo — it already holds a live COM
                        // endpoint and an active volume subscription. We only rebuild a fresh
                        // instance for genuinely new ids. This avoids tearing down and re-subscribing
                        // every device on every refresh (the full-refresh happens at startup and
                        // system resume), which is both cleaner and preserves real-time volume state.
                        ImmutableDictionary<string, DeviceFullInfo>? existingDict = deviceInfo.Type == EDataFlow.eRender ? PlaybackDevices : RecordingDevices;
                        DeviceFullInfo? existing = existingDict != null && existingDict.TryGetValue(deviceInfo.Id, out var candidate) && !candidate.IsDisposed ? candidate : null;

                        switch (deviceInfo.Type)
                        {
                            case EDataFlow.eRender:
                                if (existing != null)
                                {
                                    playbackDevices.Add(deviceInfo.Id, existing);
                                }
                                else
                                {
                                    playbackDevices.Add(deviceInfo.Id, deviceInfo);
                                    newIds.Add(deviceInfo.Id);
                                }

                                break;
                            case EDataFlow.eCapture:
                                if (existing != null)
                                {
                                    recordingDevices.Add(deviceInfo.Id, existing);
                                }
                                else
                                {
                                    recordingDevices.Add(deviceInfo.Id, deviceInfo);
                                    newIds.Add(deviceInfo.Id);
                                }

                                break;
                            case EDataFlow.eAll:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                catch
                {
                    // The new dictionaries are discarded without having been published, so every
                    // enumerated device is ours to dispose (each entry was placed at most once).
                    foreach (var device in enumeratedDevices)
                    {
                        device.Dispose();
                    }

                    throw;
                }

                // Capture the previously published devices and swap in the new cache as ONE atomic
                // step under _cacheLock: the dispose list contains exactly the old devices (not the
                // ones we are about to publish), and a concurrent ProcessDeviceUpdates edit can
                // neither be lost between snapshot and publish nor mutate the dictionaries while we
                // read them. The lock is held only for these reference swaps (microseconds).
                DeviceFullInfo[] oldDevices;
                lock (_cacheLock)
                {
                    oldDevices = PlaybackDevices.Values.Concat(RecordingDevices.Values).ToArray();
                    PlaybackDevices = playbackDevices.ToImmutableDictionary();
                    RecordingDevices = recordingDevices.ToImmutableDictionary();
                }

                // Dispose only the devices that were published before but are absent from the new
                // enumeration (genuinely removed). Reused instances are still in the new cache, so
                // they are never disposed. Disposal happens OUTSIDE the lock (COM-heavy).
                if (oldDevices.Length > 0)
                {
                    foreach (var oldDevice in oldDevices)
                    {
                        if (!newIds.Contains(oldDevice.Id))
                        {
                            toDispose.Add(oldDevice);
                        }
                    }

                    DisposeOldDevices(toDispose.ToArray());
                }

                // Now subscribe to events, but ONLY for devices that weren't already in the cache
                // (reused instances keep their existing volume subscription). The newIds set was
                // built from the freshly enumerated ids, so subscribing only those is correct and
                // avoids tearing down/re-adding the COM volume registration on every refresh.
                foreach (var device in PlaybackDevices.Values.Concat(RecordingDevices.Values))
                {
                    if (newIds.Contains(device.Id))
                    {
                        SubscribeToDeviceEvents(device);
                    }
                }


                logContext.Information("Refreshed all devices in {@StopTime}. {@Recording}/rec, {@Playback}/play", stopWatch.Elapsed, recordingDevices.Count, playbackDevices.Count);
                if (!cancellationToken.IsCancellationRequested)
                {
                    _deviceListRefreshed.OnNext(Unit.Default);
                }
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
        // Swap both caches to Empty under the lock so no concurrent mutation can reintroduce a
        // device after the snapshot, then dispose the captured devices outside the lock.
        DeviceFullInfo[] oldDevices;
        lock (_cacheLock)
        {
            oldDevices = PlaybackDevices.Values.Concat(RecordingDevices.Values).ToArray();
            PlaybackDevices = ImmutableDictionary<string, DeviceFullInfo>.Empty;
            RecordingDevices = ImmutableDictionary<string, DeviceFullInfo>.Empty;
        }

        DisposeOldDevices(oldDevices);

        // Dispose subjects and clear all subscriptions
        (_defaultDeviceChanged as Subject<DefaultDevicePayload>)?.Dispose();
        (_deviceVolumeChanged as Subject<DeviceVolumeChangedPayload>)?.Dispose();
        (_deviceListRefreshed as Subject<Unit>)?.Dispose();

        _refreshCancellationTokenSource.Dispose();
    }
}
