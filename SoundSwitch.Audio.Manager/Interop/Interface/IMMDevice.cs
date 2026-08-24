using System;
using System.Runtime.InteropServices;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Interop.Interface
{
    /// <summary>
    /// IMMDevice (mmdeviceapi.h). Vtable order matches the Windows SDK declaration exactly.
    /// </summary>
    [ComImport]
    [Guid(ComGuid.AUDIO_IMMDEVICE_IID)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        /// <summary>
        /// Activates a device-specific COM interface (e.g. IAudioEndpointVolume, IAudioSessionManager2, IAudioClient).
        /// The returned pointer owns one reference; the caller turns it into an RCW and releases the raw pointer.
        /// </summary>
        [PreserveSig]
        HRESULT Activate([In] ref Guid iid, [In] uint dwClsCtx, [In] IntPtr pActivationParams, [Out] out IntPtr ppInterface);

        [PreserveSig]
        HRESULT OpenPropertyStore([In] uint stgmAccess, [Out] out IPropertyStore propertyStore);

        /// <summary>
        /// Returns a caller-owned LPWSTR (COM task allocator). The caller must free it with CoTaskMemFree.
        /// </summary>
        [PreserveSig]
        HRESULT GetId([Out] out IntPtr id);

        [PreserveSig]
        HRESULT GetState([Out] out EDeviceState state);
    }
}
