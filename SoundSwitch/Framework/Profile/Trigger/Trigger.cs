using SoundSwitch.Framework.WinApi.Keyboard;

namespace SoundSwitch.Framework.Profile.Trigger
{
    public class Trigger
    {
        public Trigger(TriggerFactory.Enum type)
        {
            Type = type;
        }

        public TriggerFactory.Enum Type            { get; }
        public string              WindowName      { get; set; }
        public string              ApplicationPath { get; set; }
        public HotKey              HotKey          { get; set; }

        public override string ToString()
        {
            return new TriggerFactory().Get(Type).Label;
        }
    }
}