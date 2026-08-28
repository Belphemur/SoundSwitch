using System;
using System.Runtime.InteropServices;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Interop.Com.Base
{
    /// <summary>
    /// Ole32 interop for the cross-apartment COM marshalling used by the playback path. The
    /// notification renderer resolves an <c>IMMDevice</c> on the shared ComThread and then
    /// unmarshals it on its own dedicated STA thread, so the raw interface pointer must cross an
    /// apartment boundary via COM's standard inter-thread marshalling primitive rather than being
    /// wrapped directly.
    /// </summary>
    internal static class Ole32
    {
        /// <summary>
        /// Marshals an interface pointer into a stream (<c>IStream</c>) so it can be unmarshalled
        /// in another COM apartment. The returned stream owns the marshalled reference; the caller
        /// releases its own reference once the marshal has captured it.
        /// </summary>
        [DllImport("Ole32.dll", PreserveSig = true)]
        internal static extern HRESULT CoMarshalInterThreadInterfaceInStream([In] ref Guid riid, [In, MarshalAs(UnmanagedType.IUnknown)] object pUnk, out IntPtr ppStm);

        /// <summary>
        /// Unmarshals an interface pointer previously written by
        /// <see cref="CoMarshalInterThreadInterfaceInStream"/>; consumes and releases the stream.
        /// On success the returned pointer owns one reference and must be released by the caller.
        /// </summary>
        [DllImport("Ole32.dll", PreserveSig = true)]
        internal static extern HRESULT CoGetInterfaceAndReleaseStream(IntPtr pStm, [In] ref Guid riid, out IntPtr ppv);
    }
}
