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

        protected bool Equals(Trigger other)
        {
            return Type == other.Type && WindowName == other.WindowName && ApplicationPath == other.ApplicationPath && Equals(HotKey, other.HotKey);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Trigger) obj);
        }
    }
}