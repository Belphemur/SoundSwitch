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
    // Backing fields are kept separate from the properties so the CompareExchange
    // swaps below can pass them by `ref` (auto-properties cannot be passed by ref — CS0206).
    private ImmutableDictionary<string, DeviceFullInfo> _playbackDevices = ImmutableDictionary<string, DeviceFullInfo>.Empty;
    private ImmutableDictionary<string, DeviceFullInfo> _recordingDevices = ImmutableDictionary<string, DeviceFullInfo>.Empty;

    // Guards only the immutable-dictionary reference swaps below. The swap itself is a single
    // atomic reference assignment, but Refresh publishes a freshly rebuilt cache while
    // ProcessDeviceUpdates swaps individual entries in (via SwapReplace/SwapRemove). Serializing
    // just the swaps — not the COM-heavy enumeration/subscribe/dispose work — keeps the two
    // publications mutually exclusive so a concurrent update can neither be overwritten/lost nor
    // left undisposed (Sentry SOUNDSWITCH-49X-adjacent race flagged in PR #2393 review).
    // Static because the swap helpers are static (they take the backing field by ref).
    private static readonly object _cacheLock = new();

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
    /// Read-modify-write that replaces (or inserts) <paramref name="device"/> under
    /// <paramref name="id"/> in the published immutable dictionary. The swap is guarded by
    /// <c>_cacheLock</c> so it is mutually exclusive with <see cref="Refresh"/> publishing a rebuilt
    /// cache (and with <see cref="SwapRemove"/>), preventing a concurrent update from being
    /// overwritten/lost or left undisposed.
    /// </summary>
    private static ImmutableDictionary<string, DeviceFullInfo> SwapReplace(
        ref ImmutableDictionary<string, DeviceFullInfo> field,
        string id, DeviceFullInfo device)
    {
        ImmutableDictionary<string, DeviceFullInfo> current, updated;
        lock (_cacheLock)
        {
            do
            {
                current = field;
                updated = current.SetItem(id, device);
            } while (Interlocked.CompareExchange(ref field, updated, current) != current);
        }
        return updated;
    }

    /// <summary>
    /// Read-modify-write that removes <paramref name="id"/> from the published immutable
    /// dictionary. Guarded by <c>_cacheLock</c> (see <see cref="SwapReplace"/>) so it cannot race
    /// with <see cref="Refresh"/>'s publication. Returns the (possibly unchanged) dictionary;
    /// <paramref name="removed"/> is the evicted device or <c>null</c> when the id wasn't present.
    /// </summary>
    private static ImmutableDictionary<string, DeviceFullInfo> SwapRemove(
        ref ImmutableDictionary<string, DeviceFullInfo> field,
        string id, out DeviceFullInfo? removed)
    {
        ImmutableDictionary<string, DeviceFullInfo> current, updated;
        lock (_cacheLock)
        {
            do
            {
                current = field;
                if (!current.TryGetValue(id, out removed))
                {
                    removed = null;
                    return current; // nothing to remove
                }

                updated = current.Remove(id);
            } while (Interlocked.CompareExchange(ref field, updated, current) != current);
        }
        return updated;
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
                case EDataFlow.eRender:
                    if (PlaybackDevices.TryGetValue(device.Id, out var oldPlaybackDevice))
                    {
                        DisposeDevice(oldPlaybackDevice);
                    }

                    SwapReplace(ref _playbackDevices, device.Id, device);
                    SubscribeToDeviceEvents(device);
                    break;
                case EDataFlow.eCapture:
                    if (RecordingDevices.TryGetValue(device.Id, out var oldRecordingDevice))
                    {
                        DisposeDevice(oldRecordingDevice);
                    }

                    SwapReplace(ref _recordingDevices, device.Id, device);
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
                        var removed = false;
                        SwapRemove(ref _playbackDevices, deviceChangedEvent.DeviceId, out var playbackDevice);
                        if (playbackDevice != null)
                        {
                            DisposeDevice(playbackDevice);
                            removed = true;
                        }

                        SwapRemove(ref _recordingDevices, deviceChangedEvent.DeviceId, out var recordingDevice);
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
                // Materialize the enumeration up front: if cancellation (or any failure) interrupts
                // the placement loop below, the catch disposes every enumerated entry — placed or
                // not — so no DeviceFullInfo (and the AudioDevice COM reference it owns) is abandoned.
                var enumeratedDevices = AudioSwitcher.Instance.GetAudioEndpoints(EDataFlow.eAll, _state).ToArray();
                try
                {
                    foreach (var deviceInfo in enumeratedDevices)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        // Subscription is now handled after adding to the dictionary
                        // SubscribeToDeviceEvents(deviceInfo);

                        switch (deviceInfo.Type)
                        {
                            case EDataFlow.eRender:
                                playbackDevices.Add(deviceInfo.Id, deviceInfo);
                                break;
                            case EDataFlow.eCapture:
                                recordingDevices.Add(deviceInfo.Id, deviceInfo);
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

                // Capture the previously published devices BEFORE swapping in the new cache, so the
                // dispose list contains exactly the old devices (not the ones we are about to publish).
                var oldDevices = PlaybackDevices.Values.Concat(RecordingDevices.Values).ToArray();

                // Publish the new cache so any reader that starts during disposal sees the fresh,
                // valid devices (not the ones we are about to tear down). The publish is guarded by
                // _cacheLock (same as SwapReplace/SwapRemove) so a concurrent ProcessDeviceUpdates
                // swap cannot land between our snapshot and our publish and get overwritten/lost.
                lock (_cacheLock)
                {
                    PlaybackDevices = playbackDevices.ToImmutableDictionary();
                    RecordingDevices = recordingDevices.ToImmutableDictionary();
                }

                // Dispose the captured old devices outside the live dictionaries, so a concurrent
                // ProcessDeviceUpdates (device arrival/removal on another thread) cannot mutate the
                // collection while we enumerate it. The Union over the live dictionaries used to be
                // lazy and threw "Collection was modified" under concurrent mutation (Sentry SOUNDSWITCH-49X).
                if (oldDevices.Length > 0)
                {
                    DisposeOldDevices(oldDevices);
                }

                // Now subscribe to events for the new devices in the cache
                foreach (var device in PlaybackDevices.Values.Concat(RecordingDevices.Values))
                {
                    SubscribeToDeviceEvents(device);
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
        DisposeOldDevices(PlaybackDevices.Values.Concat(RecordingDevices.Values).ToArray());

        // Dispose subjects and clear all subscriptions
        (_defaultDeviceChanged as Subject<DefaultDevicePayload>)?.Dispose();
        (_deviceVolumeChanged as Subject<DeviceVolumeChangedPayload>)?.Dispose();
        (_deviceListRefreshed as Subject<Unit>)?.Dispose();

        _refreshCancellationTokenSource.Dispose();
    }
}
