#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.TriggerSwitch;

[MessagePackObject(keyAsPropertyName: true)]
public class TriggerSwitchResponse : IPipeMessage
{
    public bool Success { get; set; }
}