#nullable enable
using System;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using SoundSwitch.Common.Framework.Audio.Icon;

namespace SoundSwitch.Common.Framework.Audio.Device
{
    public class DeviceFullInfo : DeviceInfo, IDisposable
    {
        private readonly MMDevice? _device;
        public string IconPath { get; }
        public DeviceState State { get; }

        private bool _disposed = false;

        private int? _volume;

        [JsonIgnore]
        public System.Drawing.Icon LargeIcon => AudioDeviceIconExtractor.ExtractIconFromPath(IconPath, Type, true);

        [JsonIgnore]
        public System.Drawing.Icon SmallIcon => AudioDeviceIconExtractor.ExtractIconFromPath(IconPath, Type, false);

        [JsonIgnore]
        public int Volume
        {
            get
            {
                if (State != DeviceState.Active || _device == null)
                {
                    return 0;
                }

                if (_volume.HasValue)
                {
                    return _volume.Value;
                }

                try
                {
                    var deviceAudioEndpointVolume = _device.AudioEndpointVolume;
                    if (deviceAudioEndpointVolume == null)
                    {
                        _volume = 0;
                        return _volume.Value;
                    }

                    _volume = (int)Math.Round(deviceAudioEndpointVolume.MasterVolumeLevelScalar * 100);
                    deviceAudioEndpointVolume.OnVolumeNotification += DeviceAudioEndpointVolumeOnOnVolumeNotification;
                    return _volume.Value;
                }
                catch
                {
                    _volume = 0;
                    return _volume.Value;
                }
            }
        }

        [JsonConstructor]
        public DeviceFullInfo(string name, string id, DataFlow type, string iconPath, DeviceState state, bool isUsb) : base(name, id, type, isUsb, DateTime.UtcNow)
        {
            IconPath = iconPath;
            State = state;
        }

        public DeviceFullInfo(MMDevice device) : base(device)
        {
            _device = device;
            IconPath = device.IconPath;
            State = device.State;
        }

        private void DeviceAudioEndpointVolumeOnOnVolumeNotification(AudioVolumeNotificationData data)
        {
            _volume = (int)Math.Round(data.MasterVolume * 100F);
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
                if (_volume.HasValue && _device?.AudioEndpointVolume != null)
                {
                    _device.AudioEndpointVolume.OnVolumeNotification -= DeviceAudioEndpointVolumeOnOnVolumeNotification;
                }

                _device?.Dispose();
            }
            catch (Exception)
            {
                //ignored
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}