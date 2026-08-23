using System;
using System.Runtime.InteropServices;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Interop.Interface
{
    /// <summary>
    /// IAudioClient (audioclient.h). The vtable is the complete, strict Windows SDK declaration
    /// order — after the three IUnknown slots, all 12 slots in audioclient.h order. A COM
    /// interface dispatches by slot position: omitting or reordering a slot would shift every
    /// subsequent method to the wrong slot, so nothing here may be removed or rearranged.
    /// Declared in Phase 2; consumed by the playback path in Phase 4.
    ///
    /// Output-pointer / COM task-allocator contract:
    /// - GetMixFormat returns a WAVEFORMATEX* allocated with CoTaskMemAlloc → the caller frees it
    ///   with CoTaskMemFree.
    /// - IsFormatSupported's ppClosestMatch is likewise task-allocated: when the call returns
    ///   S_FALSE (a closest match was found), the caller owns *ppClosestMatch and must free it
    ///   with CoTaskMemFree. Initialize output pointers to NULL before the call and free every
    ///   non-null buffer in a finally block.
    /// </summary>
    [ComImport]
    [Guid("1CB9AD4C-DBFA-4C32-B178-C2F568A703B2")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioClient
    {
        [PreserveSig]
        HRESULT Initialize([In] AudioClientShareMode shareMode, [In] AudioClientStreamFlags streamFlags, [In] long hnsBufferDuration, [In] long hnsPeriodicity, [In] IntPtr format, [In] ref Guid audioSessionGuid);

        [PreserveSig]
        HRESULT GetBufferSize([Out] out uint bufferSize);

        [PreserveSig]
        HRESULT GetStreamLatency([Out] out long latency);

        [PreserveSig]
        HRESULT GetCurrentPadding([Out] out uint currentPadding);

        [PreserveSig]
        HRESULT IsFormatSupported([In] AudioClientShareMode shareMode, [In] IntPtr format, [Out] out IntPtr closestMatchFormat);

        [PreserveSig]
        HRESULT GetMixFormat([Out] out IntPtr deviceFormatPointer);

        [PreserveSig]
        HRESULT GetDevicePeriod([Out] out long defaultDevicePeriod, [Out] out long minimumDevicePeriod);

        [PreserveSig]
        HRESULT Start();

        [PreserveSig]
        HRESULT Stop();

        [PreserveSig]
        HRESULT Reset();

        [PreserveSig]
        HRESULT SetEventHandle([In] IntPtr eventHandle);

        [PreserveSig]
        HRESULT GetService([In] ref Guid interfaceId, [Out] out IntPtr interfacePointer);
    }

    /// <summary>
    /// IAudioRenderClient (audioclient.h). Complete vtable (both methods). Declared in Phase 2;
    /// consumed by the playback path in Phase 4.
    /// </summary>
    [ComImport]
    [Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioRenderClient
    {
        [PreserveSig]
        HRESULT GetBuffer([In] uint numFramesRequested, [Out] out IntPtr dataBufferPointer);

        [PreserveSig]
        HRESULT ReleaseBuffer([In] uint numFramesWritten, [In] AudioClientBufferFlags bufferFlags);
    }

    [Flags]
    internal enum AudioClientShareMode : uint
    {
        Shared = 0,
        Exclusive = 1
    }

    [Flags]
    internal enum AudioClientStreamFlags : uint
    {
        None = 0,
        EventCallback = 0x00040000
    }

    [Flags]
    internal enum AudioClientBufferFlags : uint
    {
        None = 0,
        Silent = 0x2
    }

    /// <summary>
    /// Minimal managed view of WAVEFORMATEX for mix-format negotiation (Phase 4).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    internal struct WaveFormatEx
    {
        public ushort wFormatTag;
        public ushort nChannels;
        public uint nSamplesPerSec;
        public uint nAvgBytesPerSec;
        public ushort nBlockAlign;
        public ushort wBitsPerSample;
        public ushort cbSize;
    }
}
