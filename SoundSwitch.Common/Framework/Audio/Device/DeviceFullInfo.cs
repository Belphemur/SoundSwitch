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

        public System.Drawing.Icon LargeIcon => AudioDeviceIconExtractor.ExtractIconFromPath(IconPath, Type, true);
        public System.Drawing.Icon SmallIcon => AudioDeviceIconExtractor.ExtractIconFromPath(IconPath, Type, false);

        public int Volume { get; } = 0;

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
                    Volume = (int)(device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
                }
            }
            catch
            {
                //Ignored
            }
        }
    }
}