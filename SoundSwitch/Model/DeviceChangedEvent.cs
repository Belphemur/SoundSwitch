using System;

namespace SoundSwitch.Model;

public enum EventType
{
    Removed,
    Added,
    StateChanged,
    PropertyChanged,
    DefaultChanged
}

public record DeviceChangedEvent(EventType Action, string DeviceId) : IComparable<DeviceChangedEvent>
{
    public int CompareTo(DeviceChangedEvent other)
    {
        return Action.CompareTo(other.Action);
    }
};