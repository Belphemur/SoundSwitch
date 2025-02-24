#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.Microphone;

[MessagePackObject(keyAsPropertyName: true)]
public class MicrophoneStateResponse : IPipeMessage
{
    public bool Success { get; set; }
    public bool IsMuted { get; set; }
    public string DeviceName { get; set; } = "";
}