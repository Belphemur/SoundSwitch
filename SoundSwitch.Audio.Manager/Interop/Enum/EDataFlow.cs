using System;

namespace SoundSwitch.Audio.Manager.Interop.Enum
{
    [Flags]
    public enum EDataFlow : uint
    {
        eRender = 0,
        eCapture = (eRender + 1),
        eAll = (eCapture + 1),
        EDataFlow_enum_count = (eAll + 1)
    }
}