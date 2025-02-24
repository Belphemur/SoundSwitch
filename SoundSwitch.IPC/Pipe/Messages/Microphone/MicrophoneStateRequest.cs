#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.Microphone;

[MessagePackObject(keyAsPropertyName: true)]
public class MicrophoneStateRequest : IPipeMessage
{
}