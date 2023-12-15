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

        [JsonIgnore]
        public System.Drawing.Icon LargeIcon => AudioDeviceIconExtractor.ExtractIconFromPath(IconPath, Type, true);

        [JsonIgnore]
        public System.Drawing.Icon SmallIcon => AudioDeviceIconExtractor.ExtractIconFromPath(IconPath, Type, false);

        [JsonIgnore]
        public int Volume { get; private set; } = 0;

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
            try
            {
                //Can only get volume for active devices
                if (device.State == DeviceState.Active)
                {
                    var deviceAudioEndpointVolume = device.AudioEndpointVolume;
                    if (deviceAudioEndpointVolume == null)
                    {
                        Volume = 0;
                        return;
                    }

                    Volume = (int)Math.Round(deviceAudioEndpointVolume.MasterVolumeLevelScalar * 100);
                    deviceAudioEndpointVolume.OnVolumeNotification += DeviceAudioEndpointVolumeOnOnVolumeNotification;
                }
            }
            catch
            {
                //Ignored
            }
        }

        private void DeviceAudioEndpointVolumeOnOnVolumeNotification(AudioVolumeNotificationData data)
        {
            Volume = (int)Math.Round(data.MasterVolume * 100F);
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
                if (_device?.AudioEndpointVolume != null)
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