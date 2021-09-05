using System;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using SoundSwitch.Common.Framework.Audio.Icon;

namespace SoundSwitch.Common.Framework.Audio.Device
{
    public class DeviceFullInfo : DeviceInfo
    {
        public string IconPath { get; }
        public DeviceState State { get; }

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
            IconPath = device.IconPath;
            State = device.State;
            try
            {
                //Can only get volume for active devices
                if (device.State == DeviceState.Active)
                {
                    var deviceAudioEndpointVolume = device.AudioEndpointVolume;
                    Volume = (int)(deviceAudioEndpointVolume.MasterVolumeLevelScalar * 100);
                    deviceAudioEndpointVolume.OnVolumeNotification += data => Volume = (int)data.MasterVolume * 100;
                }
            }
            catch
            {
                //Ignored
            }
        }
    }
}