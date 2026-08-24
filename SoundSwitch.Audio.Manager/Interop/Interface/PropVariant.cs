using System;
using System.Runtime.InteropServices;

namespace SoundSwitch.Audio.Manager.Interop.Interface
{
    /// <summary>
    /// Minimal blittable view of the native PROPVARIANT covering the value kinds SoundSwitch reads
    /// (VT_LPWSTR strings plus scalar VT_INT/VT_BOOL).
    /// The union is given its full native 16-byte extent (DECIMAL-sized) so a native write can never
    /// overrun the buffer; <see cref="Size"/> keeps the marshaled size at the native 24 bytes on
    /// both x86 and x64.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = Size)]
    internal struct PropVariant
    {
        internal const int Size = 24;

        public ushort vt;
        public ushort wReserved1;
        public ushort wReserved2;
        public ushort wReserved3;
        public IntPtr data1;
        public IntPtr data2;

        [DllImport("Ole32.dll", ExactSpelling = true)]
        internal static extern int PropVariantClear(ref PropVariant pvar);
    }
}
