using System.Runtime.InteropServices;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Interop.Interface
{
    /// <summary>
    /// IMMDeviceCollection (mmdeviceapi.h). Vtable order matches the Windows SDK declaration exactly.
    /// </summary>
    [ComImport]
    [Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceCollection
    {
        [PreserveSig]
        HRESULT GetCount([Out] out uint numDevices);

        [PreserveSig]
        HRESULT Item([In] uint deviceNumber, [Out] out IMMDevice device);
    }

    /// <summary>
    /// IMMEndpoint (mmdeviceapi.h). Obtained by querying an IMMDevice; reports the data flow.
    /// </summary>
    [ComImport]
    [Guid("1BE09788-6894-4089-8586-9A2A6C265AC5")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMEndpoint
    {
        [PreserveSig]
        HRESULT GetDataFlow([Out] out EDataFlow dataFlow);
    }
}
