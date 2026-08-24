#nullable enable
using System;
using System.Runtime.InteropServices;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Playback
{
    /// <summary>
    /// Media Foundation interop surface needed to decode MP3 files — the in-house
    /// replacement for the legacy third-party MP3 reader dependency. Only the subset
    /// used by <see cref="Mp3FileReader"/> is declared: source reader creation, media
    /// type negotiation, and sample extraction.
    ///
    /// Vtable orders match the Windows SDK (mfobjects.h / mfreadwrite.h) exactly.
    /// </summary>
    internal static class MediaFoundationInterop
    {
        // mfapi.h — MF_VERSION for WINVER >= 0x0601.
        public const uint MfVersion = 0x20070;

        // MFSTARTUP_LITE: MFStartup without sockets.
        public const uint MfStartupLite = 0x1;

        // mfreadwrite.h — stream selectors.
        public const uint SourceReaderAllStreams = 0xFFFFFFFE;
        public const uint SourceReaderFirstAudioStream = 0xFFFFFFFD;

        /// <summary>MF_SOURCE_READERF_ENDOFSTREAM — ReadSample returns a null sample at EOS.</summary>
        public const uint SourceReaderFlagEndOfStream = 0x2;

        // mfapi.h — attribute keys.
        public static readonly Guid MtMajorType = new("48eba18e-f8c9-4687-bf11-0a74c9f96a8f");
        public static readonly Guid MtSubType = new("f7e34c9a-42e8-4714-b74b-cb29d72c35e5");
        public static readonly Guid MtAudioNumChannels = new("37e48bf5-645e-4c5b-89de-ada9e29b696a");
        public static readonly Guid MtAudioSamplesPerSecond = new("5faeeae7-0290-4c31-9e8a-c534f68d9dba");
        public static readonly Guid MtAudioBitsPerSample = new("f2deb57f-40fa-4764-aa33-ed4f2d1ff669");

        // mfapi.h — major type / audio subtype GUIDs.
        public static readonly Guid MediaTypeAudio = new("73647561-0000-0010-8000-00aa00389b71");
        public static readonly Guid AudioFormatPcm = new("00000001-0000-0010-8000-00aa00389b71");
        public static readonly Guid AudioFormatFloat = new("00000003-0000-0010-8000-00aa00389b71");
        public static readonly Guid AudioFormatMp3 = new("00000055-0000-0010-8000-00aa00389b71");

        [DllImport("mfplat.dll", ExactSpelling = true)]
        public static extern HRESULT MFStartup(uint version, uint flags);

        [DllImport("mfplat.dll", ExactSpelling = true)]
        public static extern HRESULT MFShutdown();

        [DllImport("mfplat.dll", ExactSpelling = true)]
        public static extern HRESULT MFCreateMediaType(out IMFMediaType mediaType);

        [DllImport("mfreadwrite.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern HRESULT MFCreateSourceReaderFromURL(string url, IntPtr attributes, out IMFSourceReader reader);
    }

    /// <summary>
    /// IMFAttributes (mfobjects.h). Full vtable — the methods used here sit behind the
    /// attribute getters, so every slot must be declared in order.
    /// </summary>
    [ComImport]
    [Guid("2cd2d921-c447-44a7-a13c-4adabfc247e3")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMFAttributes
    {
        [PreserveSig] HRESULT GetItem(ref Guid key, IntPtr value);
        [PreserveSig] HRESULT GetItemType(ref Guid key, IntPtr type);
        [PreserveSig] HRESULT CompareItem(ref Guid key, IntPtr value, out int result);
        [PreserveSig] HRESULT Compare(IntPtr attributes, int matchType, out int result);
        [PreserveSig] HRESULT GetUINT32(ref Guid key, out uint value);
        [PreserveSig] HRESULT GetUINT64(ref Guid key, out ulong value);
        [PreserveSig] HRESULT GetDouble(ref Guid key, out double value);
        [PreserveSig] HRESULT GetGUID(ref Guid key, out Guid value);
        [PreserveSig] HRESULT GetStringLength(ref Guid key, out uint length);
        [PreserveSig] HRESULT GetString(ref Guid key, IntPtr value, uint capacity, IntPtr length);
        [PreserveSig] HRESULT GetAllocatedString(ref Guid key, out IntPtr value, out uint length);
        [PreserveSig] HRESULT GetBlobSize(ref Guid key, out uint size);
        [PreserveSig] HRESULT GetBlob(ref Guid key, IntPtr buffer, uint size, IntPtr blobSize);
        [PreserveSig] HRESULT GetAllocatedBlob(ref Guid key, out IntPtr buffer, out uint size);
        [PreserveSig] HRESULT GetUnknown(ref Guid key, ref Guid iid, out IntPtr unknown);
        [PreserveSig] HRESULT SetItem(ref Guid key, IntPtr value);
        [PreserveSig] HRESULT DeleteItem(ref Guid key);
        [PreserveSig] HRESULT DeleteAllItems();
        [PreserveSig] HRESULT SetUINT32(ref Guid key, uint value);
        [PreserveSig] HRESULT SetUINT64(ref Guid key, ulong value);
        [PreserveSig] HRESULT SetDouble(ref Guid key, double value);
        [PreserveSig] HRESULT SetGUID(ref Guid key, ref Guid value);
        [PreserveSig] HRESULT SetString(ref Guid key, string value);
        [PreserveSig] HRESULT SetBlob(ref Guid key, IntPtr buffer, uint size);
        [PreserveSig] HRESULT SetUnknown(ref Guid key, IntPtr unknown);
        [PreserveSig] HRESULT LockStore();
        [PreserveSig] HRESULT UnlockStore();
        [PreserveSig] HRESULT GetCount(out uint count);
        [PreserveSig] HRESULT GetItemByIndex(uint index, out Guid key, IntPtr value);
        [PreserveSig] HRESULT CopyAllItems(IntPtr attributes);
    }

    /// <summary>IMFMediaType (mfobjects.h) — IMFAttributes plus five format methods.</summary>
    [ComImport]
    [Guid("44ae0fa8-ea31-4109-8d2e-4cae4997c555")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMFMediaType : IMFAttributes
    {
        [PreserveSig] HRESULT GetMajorType(out Guid majorType);
        [PreserveSig] HRESULT IsCompressedFormat([MarshalAs(UnmanagedType.Bool)] out bool compressed);
        [PreserveSig] HRESULT IsEqual(IntPtr mediaType, out uint flags);
        [PreserveSig] HRESULT GetRepresentation(ref Guid guidRepresentation, out IntPtr representation);
        [PreserveSig] HRESULT FreeRepresentation(ref Guid guidRepresentation, IntPtr representation);
    }

    /// <summary>IMFSample (mfobjects.h) — IMFAttributes plus fourteen sample methods.</summary>
    [ComImport]
    [Guid("c40a00f2-b93a-4d80-ae8c-5a1c634f58e4")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMFSample : IMFAttributes
    {
        [PreserveSig] HRESULT GetSampleFlags(out uint flags);
        [PreserveSig] HRESULT SetSampleFlags(uint flags);
        [PreserveSig] HRESULT GetSampleTime(out long sampleTime);
        [PreserveSig] HRESULT SetSampleTime(long sampleTime);
        [PreserveSig] HRESULT GetSampleDuration(out long duration);
        [PreserveSig] HRESULT SetSampleDuration(long duration);
        [PreserveSig] HRESULT GetBufferCount(out uint bufferCount);
        [PreserveSig] HRESULT GetBufferByIndex(uint index, out IMFMediaBuffer buffer);
        [PreserveSig] HRESULT ConvertToContiguousBuffer(out IMFMediaBuffer buffer);
        [PreserveSig] HRESULT AddBuffer(IMFMediaBuffer buffer);
        [PreserveSig] HRESULT RemoveBufferByIndex(uint index);
        [PreserveSig] HRESULT RemoveAllBuffers();
        [PreserveSig] HRESULT GetTotalLength(out uint totalLength);
        [PreserveSig] HRESULT CopyToBuffer(IMFMediaBuffer buffer);
    }

    /// <summary>IMFMediaBuffer (mfobjects.h).</summary>
    [ComImport]
    [Guid("045fa593-8799-42b8-bc8d-8968c6453507")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMFMediaBuffer
    {
        [PreserveSig] HRESULT Lock(out IntPtr buffer, out uint maxLength, out uint currentLength);
        [PreserveSig] HRESULT Unlock();
        [PreserveSig] HRESULT GetCurrentLength(out uint currentLength);
        [PreserveSig] HRESULT SetCurrentLength(uint currentLength);
        [PreserveSig] HRESULT GetMaxLength(out uint maxLength);
    }

    /// <summary>IMFSourceReader (mfreadwrite.h).</summary>
    [ComImport]
    [Guid("70ae66f2-c809-4e4f-8915-bdcb406b7993")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMFSourceReader
    {
        [PreserveSig] HRESULT GetStreamSelection(uint streamIndex, [MarshalAs(UnmanagedType.Bool)] out bool selected);
        [PreserveSig] HRESULT SetStreamSelection(uint streamIndex, [MarshalAs(UnmanagedType.Bool)] bool selected);
        [PreserveSig] HRESULT GetNativeMediaType(uint streamIndex, uint mediaTypeIndex, out IMFMediaType mediaType);
        [PreserveSig] HRESULT GetCurrentMediaType(uint streamIndex, out IMFMediaType mediaType);
        [PreserveSig] HRESULT SetCurrentMediaType(uint streamIndex, IntPtr reserved, IMFMediaType mediaType);
        [PreserveSig] HRESULT SetCurrentPosition(ref Guid timeFormat, IntPtr position);
        [PreserveSig] HRESULT ReadSample(uint streamIndex, uint controlFlags, out uint actualStreamIndex, out uint streamFlags, out long timestamp, out IMFSample sample);
        [PreserveSig] HRESULT Flush(uint streamIndex);
        [PreserveSig] HRESULT GetServiceForStream(uint streamIndex, ref Guid serviceId, ref Guid iid, out IntPtr service);
        [PreserveSig] HRESULT GetPresentationAttribute(uint streamIndex, ref Guid attribute, IntPtr value);
    }
}
