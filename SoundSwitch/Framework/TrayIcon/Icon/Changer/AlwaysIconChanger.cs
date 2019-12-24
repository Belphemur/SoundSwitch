using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public class AlwaysIconChanger : AbstractIconChanger
    {
        public override IconChangerFactory.ActionEnum TypeEnum => IconChangerFactory.ActionEnum.Always;
        public override string Label => TrayIconStrings.iconChanger_both;
        public override bool NeedsToChangeIcon(DeviceInfo deviceInfo)
        {
            return true;
        }

        public override void ChangeIcon(Util.TrayIcon trayIcon)
        {
            using var enumerator = new MMDeviceEnumerator();
            using var defaultAudio = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            
            trayIcon.ReplaceIcon(new DeviceFullInfo(defaultAudio).SmallIcon);
        }
    }
}