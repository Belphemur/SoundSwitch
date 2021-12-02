using System;
using System.Runtime.InteropServices;

namespace SoundSwitch.Audio.Manager.Interop.Com.Base
{
    public sealed class AudioSes
    {
        [DllImport("AudioSes.dll", PreserveSig = false)]
        public static extern void DllGetActivationFactory([In] HSTRING iid, [Out] out IntPtr factory);
    }
}