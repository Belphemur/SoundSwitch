using SoundSwitch.Framework.Factory;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.Profile.Trigger
{
    public class TriggerFactory : AbstractFactory<TriggerFactory.Enum, ITriggerDefinition>
    {
        public enum Enum
        {
            HotKey,
            Window,
            Process,
            Steam
        }

        private static readonly IEnumImplList<Enum, ITriggerDefinition> Impl = new EnumImplList<Enum, ITriggerDefinition>()
        {
            new HotKeyTrigger(),
            new ProcessTrigger(),
            new WindowTrigger(),
            new SteamBigPictureTrigger()
        };

        public TriggerFactory() : base(Impl)
        {
        }
    }

    public interface ITriggerDefinition : IEnumImpl<TriggerFactory.Enum>
    {
    }

    public class HotKeyTrigger : ITriggerDefinition
    {
        public TriggerFactory.Enum TypeEnum { get; } = TriggerFactory.Enum.HotKey;
        public string              Label    => SettingsStrings.hotkeys;
    }

    public class WindowTrigger : ITriggerDefinition
    {
        public TriggerFactory.Enum TypeEnum { get; } = TriggerFactory.Enum.Window;
        public string              Label    => SettingsStrings.profile_trigger_window;
    }

    public class ProcessTrigger : ITriggerDefinition
    {
        public TriggerFactory.Enum TypeEnum { get; } = TriggerFactory.Enum.Process;
        public string              Label    => SettingsStrings.profile_trigger_process;
    }

    public class SteamBigPictureTrigger : ITriggerDefinition
    {
        public TriggerFactory.Enum TypeEnum { get; } = TriggerFactory.Enum.Steam;
        public string              Label    => SettingsStrings.profile_trigger_steam;
    }
}