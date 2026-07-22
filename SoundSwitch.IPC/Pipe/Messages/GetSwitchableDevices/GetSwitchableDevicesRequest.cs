#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.GetSwitchableDevices;

[MessagePackObject(keyAsPropertyName: true)]
public class GetSwitchableDevicesRequest : IPipeMessage
{
}
