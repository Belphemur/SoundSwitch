#nullable enable
using MessagePack;

namespace SoundSwitch.IPC.Pipe.Messages.OpenSettings;

[MessagePackObject(keyAsPropertyName: true)]
public class OpenSettingsRequest : IPipeMessage
{
}