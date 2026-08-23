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

    private readonly ConcurrentQueue<DeviceChangedEvent> _deviceChangedEvents = new();

    /// <summary>
    /// Get the last events and clear the queue of events
    /// </summary>
    /// <returns></returns>
    public IEnumerable<DeviceChangedEvent> GetLastEvents()
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
                using (_lastRoleDeviceLock.EnterScope())
                {
                    using var device = AudioSwitcher.Instance.GetDefaultAudioDevice(flow, role);
                    if (device == null)
                        continue;

                    var deviceRole = new DeviceRole(flow, role);
                    if (_lastRoleDevice.TryGetValue(deviceRole, out var oldDeviceId) && oldDeviceId == device.Id)
                    {
                        continue;
                    }

                    _lastRoleDevice[deviceRole] = device.Id;
                    _deviceChangedEvents.Enqueue(new DefaultDeviceChangedEvent(EventType.DefaultChanged, device.Id, role));
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
            if (_lastRoleDevice.TryGetValue(deviceRole, out var oldDeviceId) && oldDeviceId == e.DeviceId)
            {
                return;
            }

            _lastRoleDevice[deviceRole] = e.DeviceId;
            _deviceChangedEvents.Enqueue(new DefaultDeviceChangedEvent(EventType.DefaultChanged, e.DeviceId, e.Role));
        }
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
