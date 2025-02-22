#nullable enable
using MessagePack;

namespace SoundSwitch.Common.Framework.Pipe;

[MessagePackObject]
public class PipeMessage
{
    [Key(0)]
    public MessageType Type { get; set; }

    [Key(1)]
    public IMessagePayload? Payload { get; set; }

    public enum MessageType
    {
        OpenSettings,
        TriggerProfile,
        TriggerSwitch
    }

    public static PipeMessage CreateOpenSettings()
    {
        return new PipeMessage
        {
            Type = MessageType.OpenSettings,
            Payload = new OpenSettingsPayload()
        };
    }

    public static PipeMessage CreateTriggerProfile(string profileName)
    {
        return new PipeMessage
        {
            Type = MessageType.TriggerProfile,
            Payload = new TriggerProfilePayload { ProfileName = profileName }
        };
    }

    public static PipeMessage CreateTriggerSwitch(AudioType type)
    {
        return new PipeMessage
        {
            Type = MessageType.TriggerSwitch,
            Payload = new TriggerSwitchPayload { Type = type }
        };
    }
}