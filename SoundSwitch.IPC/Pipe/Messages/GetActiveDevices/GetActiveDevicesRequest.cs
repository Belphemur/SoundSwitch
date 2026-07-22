#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.GetActiveDevices;

[MessagePackObject(keyAsPropertyName: true)]
public class GetActiveDevicesRequest : IPipeMessage
{
}
