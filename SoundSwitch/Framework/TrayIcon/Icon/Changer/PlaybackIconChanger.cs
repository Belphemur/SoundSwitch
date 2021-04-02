using NAudio.CoreAudioApi;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Localization;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public class PlaybackIconChanger : AbstractIconChanger
    {
        public override IconChangerFactory.ActionEnum TypeEnum => IconChangerFactory.ActionEnum.Playback;
        public override string                        Label    => TrayIconStrings.iconChanger_playback;

        public override bool NeedsToChangeIcon(DeviceInfo deviceInfo)
        {
            return deviceInfo.Type == DataFlow.Render;
        }

        public override void ChangeIcon(UI.Component.TrayIcon trayIcon)
        {
            var defaultAudio = AudioSwitcher.Instance.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eConsole);
            if (defaultAudio != null)
                trayIcon.ReplaceIcon(defaultAudio.SmallIcon);
        }
    }
}