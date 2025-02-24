#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.TriggerProfile;

[MessagePackObject(keyAsPropertyName: true)]
public class TriggerProfileRequest : IPipeMessage
{
    public string ProfileName { get; set; } = "";
}