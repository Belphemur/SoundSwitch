#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.Mute;

[MessagePackObject(keyAsPropertyName: true)]
public class MuteRequest : IPipeMessage
{
    public bool Mute { get; set; }
}