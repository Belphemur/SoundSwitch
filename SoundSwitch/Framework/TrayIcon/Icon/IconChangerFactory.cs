using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.TrayIcon.Icon.Changer;

namespace SoundSwitch.Framework.TrayIcon.Icon
{
    public class IconChangerFactory : AbstractFactory<IconChangerFactory.ActionEnum, IIconChanger>
    {
        public enum ActionEnum
        {
            Nothing,
            Recording,
            Playback
        }

        private static readonly IEnumImplList<IconChangerFactory.ActionEnum, IIconChanger> Impl = new EnumImplList<ActionEnum, IIconChanger>()
        {
            new PlaybackIconChanger(),
            new RecordingIconChanger(),
            new NoneIconChanger()
        };

        public IconChangerFactory() : base(Impl)
        {
        }
    }
}