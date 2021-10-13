using NAudio.CoreAudioApi;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.Icon.Changer
{
    public class AlwaysIconChanger : AbstractIconChanger
    {
        public override IconChangerFactory.ActionEnum TypeEnum => IconChangerFactory.ActionEnum.Always;
        public override string Label => TrayIconStrings.iconChanger_both;

        protected override bool NeedsToChangeIcon(DeviceInfo deviceInfo)
        {
            return true;
        }

        protected override DataFlow Flow => DataFlow.Render;
    }
}