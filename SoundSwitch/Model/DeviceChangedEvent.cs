using System;
using NAudio.CoreAudioApi;

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
        if (ReferenceEquals(this, other)) return 0;
        if (ReferenceEquals(null, other)) return 1;
        var actionComparison = Action.CompareTo(other.Action);
        if (actionComparison != 0) return actionComparison;
        return string.Compare(DeviceId, other.DeviceId, StringComparison.Ordinal);
    }
}

public record DefaultDeviceChangedEvent(EventType Action, string DeviceId, Role Role) : DeviceChangedEvent(Action, DeviceId), IComparable<DefaultDeviceChangedEvent>
{
    // Don't compare the role in this case, we don't want multiple times the same event for the same device with different role
    public int CompareTo(DefaultDeviceChangedEvent other)
    {
        return base.CompareTo(other);
    }
}