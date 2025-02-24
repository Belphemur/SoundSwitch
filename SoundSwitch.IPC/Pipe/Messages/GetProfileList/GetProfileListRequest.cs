#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.GetProfileList;

[MessagePackObject(keyAsPropertyName: true)]
public class GetProfileListRequest : IPipeMessage
{
}