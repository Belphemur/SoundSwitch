using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.NotificationManager;

/// <summary>
/// App-side hub that owns the <see cref="AudioDeviceNotificationClient"/> registration with the
/// device enumerator (register/unregister are marshalled onto the ComThread via
/// <see cref="AudioSwitcher"/>) and translates the native default-device, device-lifecycle and
/// property notifications into queued <see cref="DeviceChangedEvent"/>s for the app to consume.
/// </summary>
public class MMNotificationClient : IDisposable
{
    private static readonly ILogger _logger = Log.ForContext<MMNotificationClient>();

    private record struct DeviceRole(EDataFlow Flow, ERole Role);

    public static MMNotificationClient Instance { get; } = new();
    private AudioDeviceNotificationClient _client;

    private readonly Dictionary<DeviceRole, string> _lastRoleDevice = new();
    private readonly Lock _lastRoleDeviceLock = new();

    // Optimistic-concurrency guard for ReconcileDefaultDevices. The OS snapshot is taken
    // OUTSIDE _lastRoleDeviceLock (a COM-thread call or AudioDevice disposal under the lock
    // deadlocks on resume — see ReconcileDefaultDevices), so a notification can commit a newer
    // default device between the snapshot and the locked compare-and-set. Reconcile captures
    // the role's revision before the lookup and commits only if it is unchanged; a real
    // notification commit bumps the revision, superseding the stale snapshot. Entries are
    // implicitly 0 and this map is accessed ONLY under _lastRoleDeviceLock, which keeps
    // OnDefaultDeviceChanged nonblocking (no COM calls, no extra waits).
    private readonly Dictionary<DeviceRole, int> _roleRevision = new();

    private readonly ConcurrentQueue<DeviceChangedEvent> _deviceChangedEvents = new();

    /// <summary>
    /// Get the last events and clear the queue of events
    /// </summary>
    /// <returns></returns>
    public IReadOnlyCollection<DeviceChangedEvent> GetLastEvents()
    {
        if (_deviceChangedEvents.IsEmpty)
            return ArraySegment<DeviceChangedEvent>.Empty;

        var events = new SortedSet<DeviceChangedEvent>();
        while (_deviceChangedEvents.TryDequeue(out var deviceChangedEvent))
        {
            events.Add(deviceChangedEvent);
        }

        return events;
    }

    /// <summary>
    /// Register the notification client in the Enumerator.
    /// The enumerator access and registration are marshalled onto the ComThread inside
    /// <see cref="AudioSwitcher.RegisterNotificationClient"/> — this method itself can be called
    /// from any thread.
    /// </summary>
    public void Register()
    {
        // Use locals during setup so a failure leaves no partial state on the instance.
        // Only assign the field once the whole registration (client + events + default-device
        // snapshot) has succeeded; Dispose() already null-checks it.
        try
        {
            var client = new AudioDeviceNotificationClient();
            client.DeviceStateChanged += OnDeviceStateChanged;
            client.DeviceAdded += OnDeviceAdded;
            client.DeviceRemoved += OnDeviceRemoved;
            client.DefaultDeviceChanged += OnDefaultDeviceChanged;
            client.PropertyValueChanged += OnPropertyValueChanged;

            // Construction and RegisterEndpointNotificationCallback happen on the ComThread here.
            if (!AudioSwitcher.Instance.RegisterNotificationClient(client))
            {
                // Registration failed (already logged in AudioSwitcher): do not publish a dead
                // client that would never fire — leave _client null so Dispose is a no-op.
                return;
            }

            // The interop enums are [Flags] and carry *_enum_count sentinels (value 3) that have
            // no native meaning — filter them out along with eAll when seeding the defaults.
            foreach (var flow in Enum.GetValues<EDataFlow>().Where(flow => flow is not (EDataFlow.eAll or EDataFlow.EDataFlow_enum_count)))
            {
                foreach (var role in Enum.GetValues<ERole>().Where(role => role != ERole.ERole_enum_count))
                {
                    using var device = AudioSwitcher.Instance.GetDefaultAudioDevice(flow, role);
                    if (device == null)
                        continue;

                    using (_lastRoleDeviceLock.EnterScope())
                    {
                        _lastRoleDevice[new DeviceRole(flow, role)] = device.Id;
                    }
                }
            }

            // All setup succeeded — publish the fully-initialized object.
            _client = client;
        }
        catch (Exception ex)
        {
            // The Windows audio service can be unavailable at startup (stopped, sleep/resume,
            // RDP disconnect, fast-user-switch). Never let that fatal-exit the application —
            // the tray app must still start. Only the "service not running" HRESULT is the
            // expected case; everything else is an unexpected failure worth a Warning.
            if (AudioDeviceException.IsAudioServiceNotRunning(ex))
            {
                _logger.Information(ex, "MMNotificationClient registration skipped: Windows audio service not running.");
            }
            else
            {
                _logger.Warning(ex, "MMNotificationClient registration failed; device notifications will be unavailable until restart.");
            }
        }
    }

    private void OnDeviceStateChanged(object sender, DeviceStateChangedEventArgs e) => _deviceChangedEvents.Enqueue(new DeviceChangedEvent(EventType.StateChanged, e.DeviceId));

    private void OnDeviceAdded(object sender, DeviceNotificationEventArgs e) => _deviceChangedEvents.Enqueue(new DeviceChangedEvent(EventType.Added, e.DeviceId));

    private void OnDeviceRemoved(object sender, DeviceNotificationEventArgs e) => _deviceChangedEvents.Enqueue(new DeviceChangedEvent(EventType.Removed, e.DeviceId));

    /// <summary>
    /// Reconcile the cached default device per (flow, role) against the real OS default.
    /// Enqueues a <see cref="DefaultDeviceChangedEvent"/> whenever the cached value diverges,
    /// mirroring <see cref="OnDefaultDeviceChanged"/>. Intended to be called on resume from sleep.
    /// </summary>
    public void ReconcileDefaultDevices()
    {
        foreach (var flow in Enum.GetValues<EDataFlow>().Where(flow => flow is not (EDataFlow.eAll or EDataFlow.EDataFlow_enum_count)))
        {
            foreach (var role in Enum.GetValues<ERole>().Where(role => role != ERole.ERole_enum_count))
            {
                var deviceRole = new DeviceRole(flow, role);

                // Capture the role's revision before the COM lookup, in its own short lock
                // scope — never held across the COM call (deadlock, see above).
                int revision;
                using (_lastRoleDeviceLock.EnterScope())
                {
                    revision = _roleRevision.TryGetValue(deviceRole, out var current) ? current : 0;
                }

                // The COM-thread call and the AudioDevice disposal must happen OUTSIDE the lock:
                // both wait on the ComThread, which can in turn wait for a notification callback
                // blocked in OnDefaultDeviceChanged for this same lock — holding it here would
                // deadlock on resume.
                using var device = AudioSwitcher.Instance.GetDefaultAudioDevice(flow, role);
                if (device == null)
                    continue;

                var deviceId = device.Id;
                using (_lastRoleDeviceLock.EnterScope())
                {
                    // A notification committed for this role since the revision capture: its
                    // state is newer than this snapshot, so discard the snapshot entirely —
                    // no write, no stale event.
                    if (_roleRevision.TryGetValue(deviceRole, out var current) && current != revision)
                    {
                        continue;
                    }

                    TryCommitDefaultDevice(deviceRole, deviceId, revision);
                }
            }
        }
    }

    private void OnDefaultDeviceChanged(object sender, DefaultDeviceChangedEventArgs e)
    {
        if (e.DeviceId == null)
            return;

        var deviceRole = new DeviceRole(e.Flow, e.Role);
        using (_lastRoleDeviceLock.EnterScope())
        {
            // A no-op notification (cached id already equal) commits nothing and must not
            // invalidate an in-flight reconcile — only real writes bump the revision.
            var revision = _roleRevision.TryGetValue(deviceRole, out var current) ? current : 0;
            TryCommitDefaultDevice(deviceRole, e.DeviceId, revision);
        }
    }

    /// <summary>
    /// Commit a default-device observation to the cache and enqueue the change event.
    /// Must be called while holding <see cref="_lastRoleDeviceLock"/>. Returns false when the
    /// cached id already equals <paramref name="deviceId"/> (dedupe, no revision bump); true
    /// after storing the id, bumping <see cref="_roleRevision"/> and enqueuing the event.
    /// </summary>
    private bool TryCommitDefaultDevice(DeviceRole deviceRole, string deviceId, int revision)
    {
        if (_lastRoleDevice.TryGetValue(deviceRole, out var oldDeviceId) && oldDeviceId == deviceId)
            return false;

        _lastRoleDevice[deviceRole] = deviceId;
        _roleRevision[deviceRole] = revision + 1;
        _deviceChangedEvents.Enqueue(new DefaultDeviceChangedEvent(EventType.DefaultChanged, deviceId, deviceRole.Role));
        return true;
    }

    private void OnPropertyValueChanged(object sender, DevicePropertyChangedEventArgs e)
    {
        if (PropertyKeys.PKEY_DeviceInterface_FriendlyName.fmtid != e.PropertyKey.fmtid
            && PropertyKeys.PKEY_AudioEndpoint_GUID.fmtid != e.PropertyKey.fmtid
            && PropertyKeys.PKEY_Device_IconPath.fmtid != e.PropertyKey.fmtid
            && PropertyKeys.PKEY_Device_FriendlyName.fmtid != e.PropertyKey.fmtid
           )
        {
            return;
        }

        _deviceChangedEvents.Enqueue(new DeviceChangedEvent(EventType.PropertyChanged, e.DeviceId));
    }

    public void Dispose()
    {
        if (_client != null)
        {
            _client.DeviceStateChanged -= OnDeviceStateChanged;
            _client.DeviceAdded -= OnDeviceAdded;
            _client.DeviceRemoved -= OnDeviceRemoved;
            _client.DefaultDeviceChanged -= OnDefaultDeviceChanged;
            _client.PropertyValueChanged -= OnPropertyValueChanged;
            // Unregister marshalled onto the ComThread; the client itself is a pure managed object.
            AudioSwitcher.Instance.UnregisterNotificationClient(_client);
        }
    }
}
