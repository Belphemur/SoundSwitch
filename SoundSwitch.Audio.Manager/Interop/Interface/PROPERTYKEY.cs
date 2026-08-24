using System;
using System.Runtime.InteropServices;

namespace SoundSwitch.Audio.Manager.Interop.Interface
{
    /// <summary>
    /// Managed view of the native PROPERTYKEY (fmtid + pid).
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PROPERTYKEY : IEquatable<PROPERTYKEY>
    {
        public Guid fmtid;
        public int pid;

        public PROPERTYKEY(Guid fmtid, int pid)
        {
            this.fmtid = fmtid;
            this.pid = pid;
        }

        public bool Equals(PROPERTYKEY other) => fmtid.Equals(other.fmtid) && pid == other.pid;

        public override bool Equals(object obj) => obj is PROPERTYKEY other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(fmtid, pid);

        public override string ToString() => $"{fmtid}:{pid}";
    }
}
