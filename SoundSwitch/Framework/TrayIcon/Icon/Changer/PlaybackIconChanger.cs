using NAudio.CoreAudioApi;
using SoundSwitch.Framework.Audio.Device;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public class PlaybackIconChanger : IIconChanger
    {
        public IconChangerFactory.Enum TypeEnum => IconChangerFactory.Enum.Playback;
        public string Label => "Playback Changed";

        public bool ChangeIcon(DeviceInfo deviceInfo)
        {
            return deviceInfo.Type == DataFlow.Render;
        }
    }
}