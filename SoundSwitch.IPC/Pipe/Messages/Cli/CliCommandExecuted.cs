#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.Cli;

[MessagePackObject(keyAsPropertyName: true)]
public class CliCommandExecuted : IPipeMessage
{
    public string Command { get; set; } = "";
}
