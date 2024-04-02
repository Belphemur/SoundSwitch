using System;

namespace SoundSwitch.Audio.Manager.Interop.Enum;

/// <summary>Device State</summary>
[Flags]
public enum EDeviceState
{
    /// <summary>DEVICE_STATE_ACTIVE</summary>
    Active = 1,
    /// <summary>DEVICE_STATE_DISABLED</summary>
    Disabled = 2,
    /// <summary>DEVICE_STATE_NOTPRESENT</summary>
    NotPresent = 4,
    /// <summary>DEVICE_STATE_UNPLUGGED</summary>
    Unplugged = 8,
    /// <summary>DEVICE_STATEMASK_ALL</summary>
    All = Unplugged | NotPresent | Disabled | Active, // 0x0000000F
}