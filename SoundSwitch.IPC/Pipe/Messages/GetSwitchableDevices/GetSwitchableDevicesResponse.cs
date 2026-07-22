#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.GetSwitchableDevices;

[MessagePackObject(keyAsPropertyName: true)]
public class GetSwitchableDevicesResponse : IPipeMessage
{
    public string[] PlaybackDevices { get; set; } = [];
    public string[] RecordingDevices { get; set; } = [];
    public bool Success { get; set; }
    public string? Error { get; set; }
}
