using System.Runtime.InteropServices;

namespace SoundSwitch.Audio.Manager.Interop.Client.Extended;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Guid
{
    public int Data1;
    public ushort Data2;
    public ushort Data3;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Data4;
}