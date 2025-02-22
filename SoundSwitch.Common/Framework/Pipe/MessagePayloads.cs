#nullable enable
using MessagePack;

namespace SoundSwitch.Common.Framework.Pipe;

[Union(0, typeof(OpenSettingsPayload))]
[Union(1, typeof(TriggerProfilePayload))]
[Union(2, typeof(TriggerSwitchPayload))]
public interface IPipeMessage { }

[MessagePackObject]
public class OpenSettingsPayload : IPipeMessage { }

[MessagePackObject]
public class TriggerProfilePayload : IPipeMessage
{
    [Key(0)]
    public string ProfileName { get; set; } = "";
}

[MessagePackObject]
public class TriggerSwitchPayload : IPipeMessage
{
    [Key(0)]
    public AudioType Type { get; set; }
}