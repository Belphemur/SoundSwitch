#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.GetActiveDevices;

[MessagePackObject(keyAsPropertyName: true)]
public class GetActiveDevicesResponse : IPipeMessage
{
    public string? ActiveProfile { get; set; }
    public string PlaybackDevice { get; set; } = "";
    public string RecordingDevice { get; set; } = "";
    public string PlaybackCommunicationDevice { get; set; } = "";
    public string RecordingCommunicationDevice { get; set; } = "";
}
