#nullable enable
using System;
using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended;

public interface IAudioPolicyConfig : IDisposable
{
    /// <summary>
    /// Set the audio endpoint for the process
    /// </summary>
    /// <param name="processId"></param>
    /// <param name="flow"></param>
    /// <param name="role"></param>
    /// <param name="deviceId"></param>
    void SetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role, string deviceId);

    /// <summary>
    /// Get Audio endpoint of the process
    /// </summary>
    /// <param name="processId"></param>
    /// <param name="flow"></param>
    /// <param name="role"></param>
    string? GetPersistedDefaultAudioEndpoint(uint processId, EDataFlow flow, ERole role);

    /// <summary>
    /// Clear all set audio enpoint
    /// </summary>
    void ClearAllPersistedApplicationDefaultEndpoints();
}
