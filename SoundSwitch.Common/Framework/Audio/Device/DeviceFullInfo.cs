#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Icon;
using SoundSwitch.Common.Framework.Icon;

namespace SoundSwitch.Common.Framework.Audio.Device
{
    public class DeviceFullInfo : DeviceInfo, IDisposable
    {
        private readonly AudioDevice? _device;
        private readonly ILogger _logger;
        public string IconPath { get; }
        public EDeviceState State { get; }

        private int _disposed; // 0 = not disposed, 1 = disposed (Interlocked)
        [JsonIgnore]
        public bool IsDisposed => _disposed != 0;
        private bool _isVolumeHandlerSubscribed = false;

        [JsonIgnore]
        public IconHandle LargeIcon => AudioDeviceIconExtractor.ExtractIconFromPath(IconPath, Type, true);

        [JsonIgnore]
        public IconHandle SmallIcon => AudioDeviceIconExtractor.ExtractIconFromPath(IconPath, Type, false);

        [JsonIgnore]
        public int Volume { get; private set; } = 0;

        [JsonIgnore]
        public bool IsMuted { get; private set; }

        /// <summary>
        /// Event raised when the device's volume or mute state changes
        /// </summary>
        public event EventHandler<VolumeChangedEventArgs>? MuteVolumeChanged;

        [JsonConstructor]
        public DeviceFullInfo(string name, string id, EDataFlow type, string iconPath, EDeviceState state, bool isUsb) : base(name, id, type, isUsb, DateTime.UtcNow)
        {
            _logger = Log.ForContext<DeviceFullInfo>().ForContext("DeviceID", id);
            IconPath = iconPath;
            State = state;
        }

        /// <summary>
        /// Build the DTO from a device snapshot and take ownership of the <see cref="AudioDevice"/>
        /// (disposed with this instance). The metadata reads come from the snapshot captured at
        /// device creation, so this constructor cannot fail on transient audio-service conditions.
        /// </summary>
        public DeviceFullInfo(AudioDevice device) : base(device)
        {
            // Build the logger without touching live COM state — the AudioDevice properties are an
            // immutable snapshot and are safe to read from any thread.
            _logger = Log.ForContext<DeviceFullInfo>();
            _device = device;
            IconPath = device.IconPath;
            State = device.State;
            // Initial volume/mute state retrieval and subscription moved to SubscribeToVolumeNotifications
        }

        /// <summary>
        /// Subscribes to the volume notification events for the device and retrieves initial state.
        /// The underlying COM calls are marshalled onto the ComThread that owns the device.
        /// </summary>
        public void SubscribeToVolumeNotifications()
        {
            // Precondition checks: Use guard clauses to avoid nesting
            if (_disposed != 0)
            {
                _logger.Debug("Skipping volume subscription for {DeviceNameClean}: Device is disposed.", NameClean);
                return;
            }

            if (_isVolumeHandlerSubscribed)
            {
                _logger.Information("Skipping volume subscription for {DeviceNameClean}: Already subscribed.", NameClean);
                return;
            }

            if (_device == null)
            {
                _logger.Warning("Skipping volume subscription for {DeviceNameClean}: AudioDevice is null.", NameClean);
                return;
            }

            // Attempt subscription and initial state retrieval for active devices.
            // The entire active-device path (state read + endpoint volume retrieval +
            // subscription) is wrapped so that any audio-service failure — e.g. the Windows
            // audio service stopping between enumeration and this call — is swallowed
            // instead of crashing the caller (CachedAudioDeviceLister, TooltipInfo*, etc.).
            // Runs on the ComThread so failures surface here (not swallowed by the marshalling).
            AudioSwitcher.Instance.InteractWithDevice(_device, device =>
            {
                try
                {
                    // Only active devices can have a usable audio endpoint volume
                    if (State != EDeviceState.Active)
                    {
                        _logger.Information("Device {DeviceNameClean} is not active ({State}), skipping volume subscription and initial state retrieval.", NameClean, State);
                        Volume = 0;
                        IsMuted = false;
                        return device;
                    }

                    var deviceAudioEndpointVolume = device.EndpointVolume;
                    if (deviceAudioEndpointVolume == null)
                    {
                        _logger.Warning("Cannot subscribe or get initial state for active device {DeviceNameClean}: AudioEndpointVolume is null.", NameClean);
                        Volume = 0;
                        IsMuted = false;
                        return device;
                    }

                    // Get initial volume and mute state
                    try
                    {
                        Volume = (int)Math.Round(deviceAudioEndpointVolume.MasterVolumeLevelScalar * 100);
                        IsMuted = deviceAudioEndpointVolume.Mute;
                        _logger.Information("Retrieved initial volume ({Volume}) and mute state ({IsMuted}) for {DeviceNameClean}", Volume, IsMuted, NameClean);
                    }
                    catch (Exception ex)
                    {
                        // A service-not-running failure here is the same transient condition — log at
                        // Information. Any other failure (unexpected) stays at Warning.
                        if (AudioDeviceException.IsAudioServiceNotRunning(ex))
                        {
                            _logger.Information(ex, "Audio service not running; using default volume/mute state for {DeviceNameClean}.", NameClean);
                        }
                        else
                        {
                            _logger.Warning(ex, "Failed to get initial volume/mute state for active device {DeviceNameClean}", NameClean);
                        }
                        Volume = 0; // Set defaults if retrieval fails
                        IsMuted = false;
                        // Continue to attempt subscription even if initial state retrieval failed
                    }

                    // Subscribe to notifications
                    deviceAudioEndpointVolume.VolumeNotification += DeviceOnVolumeNotification;
                    _isVolumeHandlerSubscribed = true;
                    _logger.Information("Successfully subscribed to volume notifications for active device {DeviceNameClean}", NameClean);
                }
                catch (Exception ex)
                {
                    // A failure here is almost always "The Windows audio service is not
                    // running" — a transient condition when the service stops between device
                    // enumeration and this call (sleep/resume, RDP disconnect, fast-user-switch,
                    // service restart). It is expected, not a crash, so log it at Information to
                    // avoid generating a Sentry issue alert. Unexpected failures stay at Warning.
                    if (AudioDeviceException.IsAudioServiceNotRunning(ex))
                    {
                        // "The Windows audio service is not running" is a transient condition when
                        // the service stops between device enumeration and this call (sleep/resume,
                        // RDP disconnect, fast-user-switch, service restart). It is expected, not a
                        // crash, so log at Information to avoid generating a Sentry issue alert.
                        _logger.Information(ex, "Skipping volume subscription for device {DeviceNameClean}: audio endpoint unavailable (audio service not running?).", NameClean);
                    }
                    else
                    {
                        // Any other audio failure (e.g. device invalidation) is unexpected and
                        // must stay at Warning so it remains visible.
                        _logger.Warning(ex, "Failed during volume notification subscription or initial state retrieval for device {DeviceNameClean}", NameClean);
                    }
                    Volume = 0; // Ensure defaults are set on error
                    IsMuted = false;
                    // Ensure we don't incorrectly flag as subscribed if subscription failed
                    _isVolumeHandlerSubscribed = false;
                }

                return device;
            });
        }

        private void DeviceOnVolumeNotification(object? sender, AudioVolumeNotificationData data)
        {
            // Store previous values before updating
            var previousVolume = Volume;
            var wasMuted = IsMuted;

            // Update current values
            Volume = (int)Math.Round(data.MasterVolume * 100F);
            IsMuted = data.Muted;

            // Only raise event if there's an actual change
            if (previousVolume != Volume || wasMuted != IsMuted)
            {
                Task.Run(() =>
                {
                    // Trigger the event with our custom event args that includes previous values
                    MuteVolumeChanged?.Invoke(this, new VolumeChangedEventArgs(Volume, previousVolume, IsMuted, wasMuted));
                });
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DeviceFullInfo()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0) return;

            try
            {
                if (disposing && _device != null)
                {
                    // Unsubscribe only if we successfully subscribed — marshalled onto the
                    // ComThread that owns the device (no-op pass-through when already on it).
                    AudioSwitcher.Instance.InteractWithDevice(_device, device =>
                    {
                        if (_isVolumeHandlerSubscribed && device.EndpointVolume != null)
                        {
                            device.EndpointVolume.VolumeNotification -= DeviceOnVolumeNotification;
                            _isVolumeHandlerSubscribed = false; // Mark as unsubscribed
                            _logger.Debug("Unsubscribed from volume notifications for device {DeviceNameClean}", NameClean);
                        }

                        return device;
                    });

                    _device.Dispose();
                }

                if (disposing)
                {
                    // Clean up event subscribers to prevent memory leaks
                    if (MuteVolumeChanged != null)
                        foreach (var subscriber in MuteVolumeChanged.GetInvocationList())
                            MuteVolumeChanged -= (EventHandler<VolumeChangedEventArgs>)subscriber;
                }

                // Finalizer path: do not marshal to the ComThread (it may be gone during process
                // teardown). The AudioDevice's own finalizer releases the native reference, which
                // also drops any outstanding notification registration.
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Exception during disposal for device {DeviceNameClean}", NameClean);
                //ignored
            }
            finally
            {
                _disposed = 1;
            }
        }
    }
}
