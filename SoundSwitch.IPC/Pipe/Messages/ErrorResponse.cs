#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages;

[MessagePackObject(keyAsPropertyName: true)]
public class ErrorResponse : IPipeMessage
{
    public bool NotReady { get; set; }
    public string Error { get; set; } = string.Empty;
}
