using NAudio.CoreAudioApi;
using SoundSwitch.Framework.Audio.Device;
using SoundSwitch.Localization;
using SoundSwitch.Model;

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

        public void OnSelection(Util.TrayIcon trayIcon)
        {
            using var enumerator = new MMDeviceEnumerator();
            using var defaultAudio = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            
            trayIcon.ReplaceIcon(new DeviceFullInfo(defaultAudio).SmallIcon);
        }
    }
}