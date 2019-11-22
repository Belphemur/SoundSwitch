using NAudio.CoreAudioApi;
using SoundSwitch.Framework.Audio.Device;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public class RecordingIconChanger : IIconChanger
    {
        public IconChangerFactory.Enum TypeEnum => IconChangerFactory.Enum.Recording;
        public string Label => "Recording Changed";
        
        public bool ChangeIcon(DeviceInfo deviceInfo)
        {
            return deviceInfo.Type == DataFlow.Capture;
        }
    }
}