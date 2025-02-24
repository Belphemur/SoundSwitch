#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.Models;

[MessagePackObject(keyAsPropertyName: true)]
public class ProfileInfo
{
    public string Name { get; set; } = "";
    public string PlaybackDevice { get; set; } = "";
    public string RecordingDevice { get; set; } = "";
    public string PlaybackCommunicationDevice { get; set; } = "";
    public string RecordingCommunicationDevice { get; set; } = "";
}