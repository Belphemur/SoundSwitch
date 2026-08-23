using System.Runtime.InteropServices;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Interop.Interface
{
    /// <summary>
    /// IMMDeviceEnumerator (mmdeviceapi.h). Vtable order matches the Windows SDK declaration exactly.
    /// </summary>
    [ComImport]
    [Guid(ComGuid.AUDIO_IMMDEVICE_ENUMERATOR_IID)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        [PreserveSig]
        HRESULT EnumAudioEndpoints([In] EDataFlow dataFlow, [In] EDeviceState stateMask, [Out] out IMMDeviceCollection devices);

        [PreserveSig]
        HRESULT GetDefaultAudioEndpoint([In] EDataFlow dataFlow, [In] ERole role, [Out] out IMMDevice endpoint);

        [PreserveSig]
        HRESULT GetDevice([In][MarshalAs(UnmanagedType.LPWStr)] string id, [Out] out IMMDevice device);

        [PreserveSig]
        HRESULT RegisterEndpointNotificationCallback([In] IMMNotificationClient client);

        [PreserveSig]
        HRESULT UnregisterEndpointNotificationCallback([In] IMMNotificationClient client);
    }
}
