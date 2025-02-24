#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.TriggerProfile;

[MessagePackObject(keyAsPropertyName: true)]
public class TriggerProfileResponse : IPipeMessage
{
    public bool Success { get; set; }
    public string? Error { get; set; }
}