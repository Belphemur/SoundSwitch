using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.TrayIcon.Icon.Changer;

namespace SoundSwitch.Framework.TrayIcon.Icon
{
    public class IconChangerFactory : AbstractFactory<IconChangerFactory.ActionEnum, IIconChanger>
    {
        public enum ActionEnum
        {
            Never,
            Recording,
            Playback,
            Always
        }

        private static readonly IEnumImplList<IconChangerFactory.ActionEnum, IIconChanger> Impl = new EnumImplList<ActionEnum, IIconChanger>()
        {
            new PlaybackIconChanger(),
            new RecordingIconChanger(),
            new NeverIconIconChanger(),
            new AlwaysIconChanger()
        };

        public IconChangerFactory() : base(Impl)
        {
        }
    }
}