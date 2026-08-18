#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.Cli;

[MessagePackObject(keyAsPropertyName: true)]
public class CliCommandExecutedResponse : IPipeMessage
{
    public bool Success { get; set; }
}
