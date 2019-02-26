using System;

namespace SoundSwitch.Audio.Policies.Interop
{
    [Flags]
    public enum ERole
    {
        eConsole = 0,
        eMultimedia = 1,
        eCommunications = 2,
        eAll = 3
    }
}