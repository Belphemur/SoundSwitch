using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager
{
    /// <summary>
    /// In-house volume-notification payload (replacing the legacy third-party one):
    /// the endpoint's master volume scalar and mute state.
    /// </summary>
    public sealed record AudioVolumeNotificationData(float MasterVolume, bool Muted);

    /// <summary>
    /// Snapshot of one audio session on a device: owning process id and session state.
    /// </summary>
    public readonly record struct AudioSessionInfo(uint ProcessId, AudioSessionState State);
}
