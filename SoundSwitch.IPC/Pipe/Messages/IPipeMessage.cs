#nullable enable
using MessagePack;
using SoundSwitch.IPC.Pipe.Messages.GetProfileList;
using SoundSwitch.IPC.Pipe.Messages.Microphone;
using SoundSwitch.IPC.Pipe.Messages.Mute;
using SoundSwitch.IPC.Pipe.Messages.OpenSettings;
using SoundSwitch.IPC.Pipe.Messages.TriggerProfile;
using SoundSwitch.IPC.Pipe.Messages.TriggerSwitch;

namespace SoundSwitch.IPC.Pipe.Messages;

[Union(0, typeof(OpenSettingsRequest))]
[Union(1, typeof(OpenSettingsResponse))]
[Union(2, typeof(TriggerProfileRequest))]
[Union(3, typeof(TriggerSwitchRequest))]
[Union(4, typeof(GetProfileListRequest))]
[Union(5, typeof(TriggerProfileResponse))]
[Union(6, typeof(TriggerSwitchResponse))]
[Union(7, typeof(GetProfileListResponse))]
[Union(8, typeof(MuteRequest))]
[Union(9, typeof(MicrophoneStateResponse))]
[Union(10, typeof(MicrophoneStateRequest))]
public interface IPipeMessage
{
}