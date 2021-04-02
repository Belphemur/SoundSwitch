using NAudio.CoreAudioApi;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public class RecordingIconChanger : AbstractIconChanger
    {
        public override IconChangerFactory.ActionEnum TypeEnum => IconChangerFactory.ActionEnum.Recording;
        public override string                        Label    => TrayIconStrings.iconChanger_recording;

        public override bool NeedsToChangeIcon(DeviceInfo deviceInfo)
        {
            return deviceInfo.Type == DataFlow.Capture;
        }

        public override void ChangeIcon(UI.Component.TrayIcon trayIcon)
        {
            var defaultAudio = AudioSwitcher.Instance.GetDefaultAudioEndpoint(EDataFlow.eCapture, ERole.eConsole);
            if (defaultAudio != null)
                trayIcon.ReplaceIcon(defaultAudio.SmallIcon);
        }
    }
}