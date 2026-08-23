using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using NAudio.CoreAudioApi;

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Model;

using PropertyKeys = NAudio.CoreAudioApi.PropertyKeys;

namespace SoundSwitch.Framework.NotificationManager;

public class MMNotificationClient : IDisposable
{
    private static readonly ILogger _logger = Log.ForContext<MMNotificationClient>();

    // AUDCLNT_E_SERVICE_NOT_RUNNING: the Windows audio service is stopped/unavailable.
    private const int AudioServiceNotRunningHResult = unchecked((int)0x88890010);

    private record struct DeviceRole(DataFlow Flow, Role Role);

    public static MMNotificationClient Instance { get; } = new();
    private MMDeviceEnumerator _enumerator;
    private MMDeviceNotificationClient _client;

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
    /// Register the notification client in the Enumerator
    /// </summary>
    public void Register()
    {
        try
        {
            _enumerator = new MMDeviceEnumerator();
            _client = _enumerator.CreateNotificationClient(false);
            _client.DeviceStateChanged += OnDeviceStateChanged;
            _client.DeviceAdded += OnDeviceAdded;
            _client.DeviceRemoved += OnDeviceRemoved;
            _client.DefaultDeviceChanged += OnDefaultDeviceChanged;
            _client.PropertyValueChanged += OnPropertyValueChanged;
            foreach (var flow in Enum.GetValues<DataFlow>().Where(flow => flow != DataFlow.All))
            {
                foreach (var role in Enum.GetValues<Role>())
                {
                    var device = AudioSwitcher.Instance.GetDefaultAudioEndpoint((EDataFlow)flow, (ERole)role);
                    if (device == null)
                        continue;

                    using (_lastRoleDeviceLock.EnterScope())
                    {
                        _lastRoleDevice[new DeviceRole(flow, role)] = device.Id;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // The Windows audio service can be unavailable at startup (stopped, sleep/resume,
            // RDP disconnect, fast-user-switch). Never let that fatal-exit the application —
            // the tray app must still start; _client/_enumerator stay null and Dispose() already
            // null-checks them. Only the "service not running" HRESULT is the expected case.
            if (ex is CoreAudioException { HResult: AudioServiceNotRunningHResult })
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
        foreach (var flow in Enum.GetValues<DataFlow>().Where(flow => flow != DataFlow.All))
        {
            foreach (var role in Enum.GetValues<Role>())
            {
                using (_lastRoleDeviceLock.EnterScope())
                {
                    var device = AudioSwitcher.Instance.GetDefaultAudioEndpoint((EDataFlow)flow, (ERole)role);
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
        if (PropertyKeys.PKEY_DeviceInterface_FriendlyName.formatId != e.PropertyKey.formatId
            && PropertyKeys.PKEY_AudioEndpoint_GUID.formatId != e.PropertyKey.formatId
            && PropertyKeys.PKEY_Device_IconPath.formatId != e.PropertyKey.formatId
            && PropertyKeys.PKEY_Device_FriendlyName.formatId != e.PropertyKey.formatId
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
            _client.Dispose();
        }

        _enumerator?.Dispose();
    }
}
