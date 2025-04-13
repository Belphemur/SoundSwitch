#nullable enable
using System;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using Serilog;
using SoundSwitch.Common.Framework.Audio.Icon;

namespace SoundSwitch.Common.Framework.Audio.Device
{
    public class DeviceFullInfo : DeviceInfo, IDisposable
    {
        private readonly MMDevice? _device;
        private readonly ILogger _logger;
        public string IconPath { get; }
        public DeviceState State { get; }

        private bool _disposed = false;
        private bool _isVolumeHandlerSubscribed = false;

        [JsonIgnore]
        public System.Drawing.Icon LargeIcon => AudioDeviceIconExtractor.ExtractIconFromPath(IconPath, Type, true);

        [JsonIgnore]
        public System.Drawing.Icon SmallIcon => AudioDeviceIconExtractor.ExtractIconFromPath(IconPath, Type, false);

        [JsonIgnore]
        public int Volume { get; private set; } = 0;

        [JsonIgnore]
        public bool IsMuted { get; private set; }

        /// <summary>
        /// Event raised when the device's volume or mute state changes
        /// </summary>
        public event EventHandler<VolumeChangedEventArgs>? MuteVolumeChanged;

        [JsonConstructor]
        public DeviceFullInfo(string name, string id, DataFlow type, string iconPath, DeviceState state, bool isUsb) : base(name, id, type, isUsb, DateTime.UtcNow)
        {
            _logger = Log.ForContext<DeviceFullInfo>().ForContext("DeviceID", id);
            IconPath = iconPath;
            State = state;
        }

        public DeviceFullInfo(MMDevice device) : base(device)
        {
            _logger = Log.ForContext<DeviceFullInfo>().ForContext("DeviceID", device.ID);
            _device = device;
            IconPath = device.IconPath;
            State = device.State;
            // Initial volume/mute state retrieval and subscription moved to SubscribeToVolumeNotifications
        }

        /// <summary>
        /// Subscribes to the volume notification events for the device and retrieves initial state.
        /// </summary>
        public void SubscribeToVolumeNotifications()
        {
            // Precondition checks: Use guard clauses to avoid nesting
            if (_disposed)
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
                _logger.Warning("Skipping volume subscription for {DeviceNameClean}: MMDevice is null.", NameClean);
                return;
            }

            // Check device state
            if (_device.State != DeviceState.Active)
            {
                _logger.Information("Device {DeviceNameClean} is not active ({State}), skipping volume subscription and initial state retrieval.", NameClean, _device.State);
                Volume = 0;
                IsMuted = false;
                return;
            }

            // Attempt subscription and initial state retrieval for active devices
            try
            {
                var deviceAudioEndpointVolume = _device.AudioEndpointVolume;
                if (deviceAudioEndpointVolume == null)
                {
                    _logger.Warning("Cannot subscribe or get initial state for active device {DeviceNameClean}: AudioEndpointVolume is null.", NameClean);
                    Volume = 0;
                    IsMuted = false;
                    return;
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
                    _logger.Warning(ex, "Failed to get initial volume/mute state for active device {DeviceNameClean}", NameClean);
                    Volume = 0; // Set defaults if retrieval fails
                    IsMuted = false;
                    // Continue to attempt subscription even if initial state retrieval failed
                }

                // Subscribe to notifications
                deviceAudioEndpointVolume.OnVolumeNotification += DeviceAudioEndpointVolumeOnOnVolumeNotification;
                _isVolumeHandlerSubscribed = true;
                _logger.Information("Successfully subscribed to volume notifications for active device {DeviceNameClean}", NameClean);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed during volume notification subscription or initial state retrieval for device {DeviceNameClean}", NameClean);
                Volume = 0; // Ensure defaults are set on error
                IsMuted = false;
                // Ensure we don't incorrectly flag as subscribed if subscription failed
                _isVolumeHandlerSubscribed = false;
            }
        }

        private void DeviceAudioEndpointVolumeOnOnVolumeNotification(AudioVolumeNotificationData data)
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
            if (_disposed)
            {
                return;
            }

            try
            {
                // Unsubscribe only if we successfully subscribed
                if (_isVolumeHandlerSubscribed && _device?.AudioEndpointVolume != null)
                {
                    _device.AudioEndpointVolume.OnVolumeNotification -= DeviceAudioEndpointVolumeOnOnVolumeNotification;
                    _isVolumeHandlerSubscribed = false; // Mark as unsubscribed
                    _logger.Debug("Unsubscribed from volume notifications for device {DeviceNameClean}", NameClean);
                }

                // Clean up event subscribers to prevent memory leaks
                if (MuteVolumeChanged != null)
                {
                    foreach (var subscriber in MuteVolumeChanged.GetInvocationList())
                    {
                        MuteVolumeChanged -= (EventHandler<VolumeChangedEventArgs>)subscriber;
                    }
                }

                _device?.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Exception during disposal for device {DeviceNameClean}", NameClean);
                //ignored
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}