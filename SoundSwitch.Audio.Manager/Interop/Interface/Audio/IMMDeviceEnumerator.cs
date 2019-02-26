using System.Runtime.InteropServices;
using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Interop.Interface.Audio
{
    [Guid(ComGuid.AUDIO_IMMDEVICE_ENUMERATOR_IID),
     InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IMMDeviceEnumerator
    {
        int __incomplete__EnumAudioEndpoints(EDataFlow dataFlow, DeviceState dwStateMask);

        [return: MarshalAs(UnmanagedType.Interface)]
        IMMDevice GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role);

        [return: MarshalAs(UnmanagedType.Interface)]
        IMMDevice GetDevice([MarshalAs(UnmanagedType.LPWStr)] string pwstrId);

        void __incomplete__RegisterEndpointNotificationCallback(/*...*/);
        void __incomplete__UnregisterEndpointNotificationCallback(/*...*/);
    }
}