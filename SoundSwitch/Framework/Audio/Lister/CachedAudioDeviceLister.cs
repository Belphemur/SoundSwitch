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

    // Volatile backing fields give the lock-free readers (GetDevices, DefaultChanged lookup) acquire
    // semantics on every read, so a reference published inside Refresh's/ProcessDeviceUpdates' lock
    // is always seen as the latest snapshot. The properties are thin wrappers over these fields.
    private volatile ImmutableDictionary<string, DeviceFullInfo> _playbackDevices = ImmutableDictionary<string, DeviceFullInfo>.Empty;
    private volatile ImmutableDictionary<string, DeviceFullInfo> _recordingDevices = ImmutableDictionary<string, DeviceFullInfo>.Empty;

    /// <inheritdoc />
    private ImmutableDictionary<string, DeviceFullInfo> PlaybackDevices
    {
        get => _playbackDevices;
        set => _playbackDevices = value;
    }

    /// <inheritdoc />
    private ImmutableDictionary<string, DeviceFullInfo> RecordingDevices
    {
        get => _recordingDevices;
        set => _recordingDevices = value;
    }

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
                    DeviceFullInfo? oldPlaybackDevice;
                    lock (_cacheLock)
                    {
                        PlaybackDevices.TryGetValue(device.Id, out oldPlaybackDevice);
                        PlaybackDevices = PlaybackDevices.SetItem(device.Id, device);
                    }

                    // Dispose the previously-cached instance only if it isn't the one we just
                    // published (guards against a no-op update of the same object).
                    if (oldPlaybackDevice != null && !ReferenceEquals(oldPlaybackDevice, device))
                    {
                        DisposeDevice(oldPlaybackDevice);
                    }

                    SubscribeToDeviceEvents(device);
                    break;
                case EDataFlow.eCapture:
                    DeviceFullInfo? oldRecordingDevice;
                    lock (_cacheLock)
                    {
                        RecordingDevices.TryGetValue(device.Id, out oldRecordingDevice);
                        RecordingDevices = RecordingDevices.SetItem(device.Id, device);
                    }

                    if (oldRecordingDevice != null && !ReferenceEquals(oldRecordingDevice, device))
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
            // Ids of devices newly enumerated this refresh (used to subscribe only the new ones
            // after publishing, and to detect fresh instances rejected in favour of a still-alive
            // existing instance).
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

                        // Build the candidate (new) cache from the freshly enumerated endpoints.
                        // We do NOT peek at the published caches here; the reconcile decision
                        // (reuse existing vs. keep fresh) is made atomically under _cacheLock below,
                        // so a concurrent ProcessDeviceUpdates removal cannot leave a device that we
                        // reused and then disposed (use-after-free), and a freshly enumerated device
                        // that loses the race is still disposed (no COM AudioDevice leak).
                        switch (deviceInfo.Type)
                        {
                            case EDataFlow.eRender:
                                playbackDevices.Add(deviceInfo.Id, deviceInfo);
                                newIds.Add(deviceInfo.Id);
                                break;
                            case EDataFlow.eCapture:
                                recordingDevices.Add(deviceInfo.Id, deviceInfo);
                                newIds.Add(deviceInfo.Id);
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

                // Reconcile atomically under _cacheLock (this is the only place that reads the
                // published caches for the reuse decision, so it is immune to concurrent mutation):
                //  - for each id, keep the EXISTING instance if still alive (preserves its COM
                //    endpoint + volume subscription); otherwise keep the freshly enumerated one;
                //  - a freshly enumerated instance that is NOT chosen (existing won), is disposed
                //    here so its AudioDevice COM reference is released;
                //  - an old instance absent from the new enumeration is disposed (genuinely removed).
                // The dispose lists are materialized and disposal itself happens AFTER the swap,
                // outside the lock (COM-heavy). Readers see the fresh cache during disposal.
                DeviceFullInfo[] oldDevices;
                List<DeviceFullInfo> rejectedFresh;
                lock (_cacheLock)
                {
                    oldDevices = PlaybackDevices.Values.Concat(RecordingDevices.Values).ToArray();

                    // Choose the surviving instance per id (reuse existing when alive).
                    var mergedPlayback = new Dictionary<string, DeviceFullInfo>(playbackDevices.Count);
                    var mergedRecording = new Dictionary<string, DeviceFullInfo>(recordingDevices.Count);
                    rejectedFresh = new List<DeviceFullInfo>();

                    foreach (var kvp in playbackDevices)
                    {
                        if (PlaybackDevices.TryGetValue(kvp.Key, out var existing) && !existing.IsDisposed)
                        {
                            mergedPlayback[kvp.Key] = existing;
                        }
                        else
                        {
                            mergedPlayback[kvp.Key] = kvp.Value; // fresh wins
                        }

                        if (!newIds.Contains(kvp.Key)) continue; // not fresh, no reject possible
                        var chosen = mergedPlayback[kvp.Key];
                        if (!ReferenceEquals(chosen, kvp.Value))
                        {
                            rejectedFresh.Add(kvp.Value); // fresh instance lost the race
                        }
                    }

                    foreach (var kvp in recordingDevices)
                    {
                        if (RecordingDevices.TryGetValue(kvp.Key, out var existing) && !existing.IsDisposed)
                        {
                            mergedRecording[kvp.Key] = existing;
                        }
                        else
                        {
                            mergedRecording[kvp.Key] = kvp.Value; // fresh wins
                        }

                        if (!newIds.Contains(kvp.Key)) continue;
                        var chosen = mergedRecording[kvp.Key];
                        if (!ReferenceEquals(chosen, kvp.Value))
                        {
                            rejectedFresh.Add(kvp.Value); // fresh instance lost the race
                        }
                    }

                    PlaybackDevices = mergedPlayback.ToImmutableDictionary();
                    RecordingDevices = mergedRecording.ToImmutableDictionary();
                }

                // Dispose freshly enumerated instances that were not published (rejected in favour
                // of a still-alive existing instance), then dispose old instances absent from the
                // new enumeration. Both happen outside the lock.
                if (rejectedFresh.Count > 0)
                {
                    DisposeOldDevices(rejectedFresh.ToArray());
                }

                // Devices to dispose: those published before the refresh but absent from the
                // NEWLY PUBLISHED merged caches (which include both reused existing instances and
                // freshly enumerated ones). Deriving the retained set from the published caches —
                // not from the freshly enumerated dictionaries — is essential: reused instances are
                // NOT in the fresh-enumeration keys, so using those would schedule still-published
                // (reused) devices for disposal. Disposal happens outside the lock.
                var retainedIds = new HashSet<string>(
                    PlaybackDevices.Keys.Concat(RecordingDevices.Keys));
                var removedOld = oldDevices.Where(d => !retainedIds.Contains(d.Id)).ToArray();
                if (removedOld.Length > 0)
                {
                    DisposeOldDevices(removedOld);
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
