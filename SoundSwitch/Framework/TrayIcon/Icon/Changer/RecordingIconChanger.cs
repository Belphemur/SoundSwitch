using NAudio.CoreAudioApi;
using SoundSwitch.Framework.Audio.Device;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public class RecordingIconChanger : IIconChanger
    {
        public IconChangerFactory.Enum TypeEnum => IconChangerFactory.Enum.Recording;
        public string Label => TrayIconStrings.iconChanger_recording;
        
        public bool ChangeIcon(DeviceInfo deviceInfo)
        {
            return deviceInfo.Type == DataFlow.Capture;
        }
        
        public void OnSelection(Util.TrayIcon trayIcon)
        {
            using var enumerator = new MMDeviceEnumerator();
            using var defaultAudio = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Console);
            
            trayIcon.ReplaceIcon(new DeviceFullInfo(defaultAudio).SmallIcon);
        }
    }
}