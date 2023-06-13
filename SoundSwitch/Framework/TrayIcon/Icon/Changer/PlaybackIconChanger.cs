using NAudio.CoreAudioApi;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public class PlaybackIconChanger : AbstractIconChanger
    {
        public override IconChangerFactory.ActionEnum TypeEnum => IconChangerFactory.ActionEnum.Playback;
        public override string Label => TrayIconStrings.iconChanger_playback;
        protected override DataFlow Flow => DataFlow.Render;
    }
}