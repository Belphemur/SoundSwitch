namespace SoundSwitch.Audio.Manager.Interop.Enum
{
    /// <summary>
    /// State of an audio session (AudioSessionState* in audiopolicy.h).
    /// Numerically identical to the native constants.
    /// </summary>
    public enum AudioSessionState
    {
        Inactive = 0,
        Active = 1,
        Expired = 2
    }
}
