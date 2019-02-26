using System;

namespace SoundSwitch.Audio.Manager.Interop
{
    [Flags]
    public enum ERole : uint
    {
        eConsole = 0,
        eMultimedia = (eConsole + 1),
        eCommunications = (eMultimedia + 1),
        ERole_enum_count = (eCommunications + 1)
    }
}