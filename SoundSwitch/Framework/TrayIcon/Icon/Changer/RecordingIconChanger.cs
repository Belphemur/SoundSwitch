using CoreAudio;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public class RecordingIconChanger : AbstractIconChanger
    {
        public override IconChangerFactory.ActionEnum TypeEnum => IconChangerFactory.ActionEnum.Recording;
        public override string Label => TrayIconStrings.iconChanger_recording;
        protected override DataFlow Flow => DataFlow.Capture;
    }
}