#nullable enable
using MessagePack;
using SoundSwitch.IPC.Pipe.Messages.Models;

namespace SoundSwitch.IPC.Pipe.Messages.GetProfileList;

[MessagePackObject(keyAsPropertyName: true)]
public class GetProfileListResponse : IPipeMessage
{
    public ProfileInfo[] Profiles { get; set; } = [];
}