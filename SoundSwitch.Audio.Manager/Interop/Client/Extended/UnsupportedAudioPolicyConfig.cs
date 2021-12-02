#nullable enable
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;

namespace SoundSwitch.Audio.Manager.Interop.Client.Extended;

public class UnsupportedAudioPolicyConfig : IAudioPolicyConfig
{
    public void SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, string deviceId)
    {
    }

    public string? GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role)
    {
        return null;
    }

    public void ClearAllPersistedApplicationDefaultEndpoints()
    {
    }

    public void Dispose()
    {
    }
}