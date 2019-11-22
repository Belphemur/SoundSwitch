using NAudio.CoreAudioApi;
using SoundSwitch.Framework.Audio.Device;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public class PlaybackIconChanger : IIconChanger
    {
        public IconChangerFactory.Enum TypeEnum => IconChangerFactory.Enum.Playback;
        public string Label => TrayIconStrings.iconChanger_playback;

        public bool ChangeIcon(DeviceInfo deviceInfo)
        {
            return deviceInfo.Type == DataFlow.Render;
        }
    }
}