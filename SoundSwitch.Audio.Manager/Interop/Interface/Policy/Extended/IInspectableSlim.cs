using SoundSwitch.Audio.Manager.Interop.Enum;
using System.Runtime.InteropServices;
using System;

namespace SoundSwitch.Audio.Manager.Interop.Interface.Policy.Extended
{
    [Guid("00000000-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IInspectableSlim
    {
        [PreserveSig]
        HRESULT GetIids(IntPtr count, ref IntPtr iids);
    }
}
