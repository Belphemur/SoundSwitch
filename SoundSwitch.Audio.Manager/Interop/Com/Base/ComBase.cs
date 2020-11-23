using System;
using System.Runtime.InteropServices;

namespace SoundSwitch.Audio.Manager.Interop.Com.Base
{
    internal sealed class ComBase
    {
        [DllImport("combase.dll", PreserveSig = false)]
        public static extern void RoGetActivationFactory(
            HSTRING activatableClassId,
            [In] ref Guid iid,
            [Out, MarshalAs(UnmanagedType.IUnknown)] out Object factory);
    }
}