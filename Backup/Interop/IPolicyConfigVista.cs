using System;
using System.Runtime.InteropServices;

namespace SoundSwitch.Audio.Policies.Interop
{
    [Guid("568b9108-44bf-40b4-9006-86afe5b5a620")]
    [InterfaceType(ComInterfaceType.InterfaceIsIInspectable)]
    public interface IPolicyConfigVista
    {
        [PreserveSig]
        HRESULT SetDefaultEndpoint(
            [In] string wszDeviceId,
            [In] ERole eRole
        );
    }
}