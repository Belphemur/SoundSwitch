using System.Drawing;
using NAudio.CoreAudioApi;
using SoundSwitch.Util;

namespace SoundSwitch.Framework.Audio.Device
{
    public class DeviceFullInfo : DeviceInfo
    {
        public string IconPath { get; }
        public DeviceState State { get; }

        public Icon LargeIcon => AudioDeviceIconExtractor.ExtractIconFromPath(IconPath, Type, true);
        public Icon SmallIcon => AudioDeviceIconExtractor.ExtractIconFromPath(IconPath, Type, false);

        public DeviceFullInfo(string name, string id, DataFlow type, string iconPath, DeviceState state) : base(name, id, type)
        {
            IconPath = iconPath;
            State = state;
        }

        public DeviceFullInfo(MMDevice device) : base(device)
        {
            IconPath = device.IconPath;
            State = device.State;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}