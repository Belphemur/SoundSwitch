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

        private static readonly IEnumImplList<Enum, ITriggerDefinition> Impl =
            new EnumImplList<Enum, ITriggerDefinition>()
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
        /// <summary>
        /// Maximum number of occurence of this trigger in a profile
        /// </summary>
        public int MaxOccurence { get; set; }

        public string Description { get; set; }
    }

    public abstract class BaseTrigger : ITriggerDefinition
    {
        public override string ToString()
        {
            return Label;
        }

        public virtual TriggerFactory.Enum TypeEnum { get; }
        public virtual string Label { get; }
        public virtual int MaxOccurence { get; set; } = -1;
        public abstract string Description { get; set; }
    }

    public class HotKeyTrigger : BaseTrigger
    {
        public override TriggerFactory.Enum TypeEnum { get; } = TriggerFactory.Enum.HotKey;
        public override string Label => SettingsStrings.hotkeys;
        public override string Description { get; set; } = SettingsStrings.profile_trigger_hotkey_desc;
        public override int MaxOccurence { get; set; } = 1;
    }

    public class WindowTrigger : BaseTrigger
    {
        public override TriggerFactory.Enum TypeEnum { get; } = TriggerFactory.Enum.Window;
        public override string Label => SettingsStrings.profile_trigger_window;
        public override string Description { get; set; } = SettingsStrings.profile_trigger_window_desc;
    }

    public class ProcessTrigger : BaseTrigger
    {
        public override TriggerFactory.Enum TypeEnum { get; } = TriggerFactory.Enum.Process;
        public override string Label => SettingsStrings.profile_trigger_process;
        public override string Description { get; set; } = SettingsStrings.profile_trigger_process_desc;
    }

    public class SteamBigPictureTrigger : BaseTrigger
    {
        public override TriggerFactory.Enum TypeEnum { get; } = TriggerFactory.Enum.Steam;
        public override string Label => SettingsStrings.profile_trigger_steam;

        public override int MaxOccurence { get; set; } = 1;
        public override string Description { get; set; } = SettingsStrings.profile_trigger_steam_desc;
    }
}