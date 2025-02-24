#nullable enable
using MessagePack;
using SoundSwitch.IPC.Pipe.Messages.Models;

namespace SoundSwitch.IPC.Pipe.Messages.TriggerSwitch;

[MessagePackObject(keyAsPropertyName: true)]
public class TriggerSwitchRequest : IPipeMessage
{
    public AudioType Type { get; set; }
}