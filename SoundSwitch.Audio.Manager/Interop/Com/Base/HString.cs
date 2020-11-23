using System;
using System.Runtime.InteropServices;
using System.Security;
using WinRT;

namespace SoundSwitch.Audio.Manager.Interop.Com.Base
{
    [StructLayout(LayoutKind.Sequential), SecuritySafeCritical]
    // ReSharper disable once InconsistentNaming
    public struct HSTRING : IEquatable<HSTRING>, IDisposable
    {
        readonly IntPtr handle;

        public static HSTRING FromString(string str) => new HSTRING(MarshalString.FromManaged(str));

        public static implicit operator IntPtr(HSTRING h)
        {
            return h.handle;
        }

        public static implicit operator string(HSTRING h)
        {
            return h.ToString();
        }

        private HSTRING(IntPtr handle)
        {
            this.handle = handle;
        }


        public static HSTRING Cast(IntPtr h)
        {
           return new HSTRING(h);
        }


        public void Delete()
        {
            MarshalString.DisposeAbi(handle);
        }

        public override string ToString()
        {
            return MarshalString.FromAbi(handle);
        }

        public void Dispose()
        {
            Delete();
        }

        public bool Equals(HSTRING other)
        {
            return handle == other.handle;
        }

        public override bool Equals(object obj)
        {
            return obj is HSTRING other && Equals(other);
        }

        public override int GetHashCode()
        {
            return handle.GetHashCode();
        }
    }
}