#nullable enable
using MessagePack;

namespace SoundSwitch.Common.Framework.Pipe;

[Union(0, typeof(OpenSettingsPayload))]
[Union(1, typeof(TriggerProfilePayload))]
[Union(2, typeof(TriggerSwitchPayload))]
public interface IMessagePayload { }

[MessagePackObject]
public class OpenSettingsPayload : IMessagePayload { }

[MessagePackObject]
public class TriggerProfilePayload : IMessagePayload
{
    [Key(0)]
    public string ProfileName { get; set; } = "";
}

[MessagePackObject]
public class TriggerSwitchPayload : IMessagePayload
{
    [Key(0)]
    public AudioType Type { get; set; }
}