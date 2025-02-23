#nullable enable
using MessagePack;
using System;

namespace SoundSwitch.IPC.Pipe;

[Union(0, typeof(OpenSettingsRequest))]
[Union(1, typeof(TriggerProfileRequest))]
[Union(2, typeof(TriggerSwitchRequest))]
[Union(3, typeof(GetProfileListRequest))]
[Union(4, typeof(OpenSettingsResponse))]
[Union(5, typeof(TriggerProfileResponse))]
[Union(6, typeof(TriggerSwitchResponse))]
[Union(7, typeof(GetProfileListResponse))]
public interface IPipeMessage { }

[MessagePackObject(keyAsPropertyName: true)]
public class OpenSettingsRequest : IPipeMessage { }

[MessagePackObject(keyAsPropertyName: true)]
public class OpenSettingsResponse : IPipeMessage
{
    [Key(0)]
    public bool Success { get; set; }
}

[MessagePackObject(keyAsPropertyName: true)]
public class TriggerProfileRequest : IPipeMessage
{
    [Key(0)]
    public string ProfileName { get; set; } = "";
}

[MessagePackObject(keyAsPropertyName: true)]
public class TriggerProfileResponse : IPipeMessage
{
    [Key(0)]
    public bool Success { get; set; }
    
    [Key(1)]
    public string? Error { get; set; }
}

[MessagePackObject(keyAsPropertyName: true)]
public class TriggerSwitchRequest : IPipeMessage
{
    [Key(0)]
    public AudioType Type { get; set; }
}

[MessagePackObject(keyAsPropertyName: true)]
public class TriggerSwitchResponse : IPipeMessage
{
    [Key(0)]
    public bool Success { get; set; }
}

[MessagePackObject(keyAsPropertyName: true)]
public class GetProfileListRequest : IPipeMessage { }

[MessagePackObject(keyAsPropertyName: true)]
public class GetProfileListResponse : IPipeMessage
{
    [Key(0)]
    public string[] ProfileNames { get; set; } = Array.Empty<string>();
}