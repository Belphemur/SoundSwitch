using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.TrayIcon.Icon.Changer;

namespace SoundSwitch.Framework.TrayIcon.Icon
{
    public class IconChangerFactory : AbstractFactory<IconChangerFactory.Enum, IIconChanger>
    {
        public enum Enum
        {
            Recording,
            Playback
        }

        private static readonly IEnumImplList<IconChangerFactory.Enum, IIconChanger> Impl = new EnumImplList<Enum, IIconChanger>()
        {
            new PlaybackIconChanger(),
            new RecordingIconChanger()
        };

        public IconChangerFactory() : base(Impl)
        {
        }
    }
}