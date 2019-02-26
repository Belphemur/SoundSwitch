using System;
using System.Runtime.InteropServices;
using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Interop.Interface.Audio
{
    [Guid(ComGuid.AUDIO_IMMDEVICE_IID),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDevice
    {
        // activationParams is a propvariant
        int __incomplete__Activate();

        int __incomplete__OpenPropertyStore(/*..*/);
        [return: MarshalAs(UnmanagedType.LPWStr)]
        string GetId();
        DeviceState GetState();
    }
}