#nullable enable
using MessagePack;
using System;

namespace SoundSwitch.IPC.Pipe;

[Union(0, typeof(OpenSettingsRequest))]
[Union(1, typeof(OpenSettingsResponse))]
[Union(2, typeof(TriggerProfileRequest))]
[Union(3, typeof(TriggerSwitchRequest))]
[Union(4, typeof(GetProfileListRequest))]
[Union(5, typeof(TriggerProfileResponse))]
[Union(6, typeof(TriggerSwitchResponse))]
[Union(7, typeof(GetProfileListResponse))]
public interface IPipeMessage
{
}

[MessagePackObject(keyAsPropertyName: true)]
public class OpenSettingsRequest : IPipeMessage
{
}

[MessagePackObject(keyAsPropertyName: true)]
public class OpenSettingsResponse : IPipeMessage
{
    public bool Success { get; set; }
}

[MessagePackObject(keyAsPropertyName: true)]
public class TriggerProfileRequest : IPipeMessage
{
    public string ProfileName { get; set; } = "";
}

[MessagePackObject(keyAsPropertyName: true)]
public class TriggerProfileResponse : IPipeMessage
{
    public bool Success { get; set; }

    public string? Error { get; set; }
}

[MessagePackObject(keyAsPropertyName: true)]
public class TriggerSwitchRequest : IPipeMessage
{
    public AudioType Type { get; set; }
}

[MessagePackObject(keyAsPropertyName: true)]
public class TriggerSwitchResponse : IPipeMessage
{
    public bool Success { get; set; }
}

[MessagePackObject(keyAsPropertyName: true)]
public class GetProfileListRequest : IPipeMessage
{
}

[MessagePackObject(keyAsPropertyName: true)]
public class GetProfileListResponse : IPipeMessage
{
    public string[] ProfileNames { get; set; } = [];
}