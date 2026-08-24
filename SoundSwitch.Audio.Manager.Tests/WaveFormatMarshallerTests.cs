using System;
using System.IO;
using System.Runtime.InteropServices;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Interface;
using SoundSwitch.Audio.Manager.Playback;

namespace SoundSwitch.Audio.Manager.Tests;

/// <summary>
/// Round-trips the native WAVEFORMATEX marshalling used to hand formats to WASAPI: the packed
/// 18-byte WAVEFORMATEX field layout and the WAVE_FORMAT_EXTENSIBLE sub-type tail read from
/// engine-provided mix formats.
/// </summary>
[TestFixture]
public sealed class WaveFormatMarshallerTests
{
    [Test]
    public void ToUnmanaged_WritesNativeWaveFormatExFields()
    {
        var format = new WaveFormat(WaveFormatEncoding.Pcm, 44100, 16, 2);

        var pointer = WaveFormatMarshaller.ToUnmanaged(format);
        try
        {
            var native = Marshal.PtrToStructure<WaveFormatEx>(pointer);

            native.wFormatTag.Should().Be((ushort)1);
            native.nChannels.Should().Be((ushort)2);
            native.nSamplesPerSec.Should().Be((uint)44100);
            native.nAvgBytesPerSec.Should().Be((uint)176400);
            native.nBlockAlign.Should().Be((ushort)4);
            native.wBitsPerSample.Should().Be((ushort)16);
            native.cbSize.Should().Be((ushort)0);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Test]
    public void RoundTrip_PcmAndIeeeFloat_PreservesFormat()
    {
        var formats = new[]
        {
            new WaveFormat(WaveFormatEncoding.Pcm, 44100, 16, 2),
            new WaveFormat(WaveFormatEncoding.IeeeFloat, 48000, 32, 2),
            new WaveFormat(WaveFormatEncoding.Pcm, 8000, 24, 1),
        };

        foreach (var format in formats)
        {
            var pointer = WaveFormatMarshaller.ToUnmanaged(format);
            try
            {
                WaveFormatMarshaller.FromPointer(pointer).Should().Be(format);
            }
            finally
            {
                Marshal.FreeHGlobal(pointer);
            }
        }
    }

    [Test]
    public void FromPointer_ExtensiblePcm_MapsToPcm()
    {
        ReadExtensible(new Guid("00000001-0000-0010-8000-00aa00389b71"))
            .Encoding.Should().Be(WaveFormatEncoding.Pcm);
    }

    [Test]
    public void FromPointer_ExtensibleFloat_MapsToIeeeFloat()
    {
        ReadExtensible(new Guid("00000003-0000-0010-8000-00aa00389b71"))
            .Encoding.Should().Be(WaveFormatEncoding.IeeeFloat);
    }

    [Test]
    public void FromPointer_ExtensibleUnknownSubType_Throws()
    {
        var act = () => ReadExtensible(Guid.Empty);

        act.Should().Throw<InvalidDataException>();
    }

    [Test]
    public void FromPointer_UnknownFormatTag_Throws()
    {
        var pointer = BuildNativePointer(tag: 0x0055, cbSize: 0, tailLength: 0);
        try
        {
            var act = () => WaveFormatMarshaller.FromPointer(pointer);

            act.Should().Throw<InvalidDataException>();
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    private static WaveFormat ReadExtensible(Guid subType)
    {
        var pointer = BuildNativePointer(tag: 0xFFFE, cbSize: 22, tailLength: 22, subType: subType);
        try
        {
            return WaveFormatMarshaller.FromPointer(pointer);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    /// <summary>Allocates an unmanaged 18-byte WAVEFORMATEX (Pack=2 layout) plus an optional tail.</summary>
    private static IntPtr BuildNativePointer(ushort tag, ushort cbSize, int tailLength, Guid? subType = null)
    {
        var blob = new byte[18 + tailLength];
        void Write(int offset, byte[] bytes) => Buffer.BlockCopy(bytes, 0, blob, offset, bytes.Length);

        Write(0, BitConverter.GetBytes(tag));
        Write(2, BitConverter.GetBytes((ushort)2)); // channels
        Write(4, BitConverter.GetBytes((uint)48000)); // samples per second
        Write(8, BitConverter.GetBytes((uint)48000 * 8)); // avg bytes per second
        Write(12, BitConverter.GetBytes((ushort)8)); // block align
        Write(14, BitConverter.GetBytes((ushort)32)); // bits per sample
        Write(16, BitConverter.GetBytes(cbSize));
        if (subType is { } guid)
            Write(24, guid.ToByteArray()); // SubFormat follows validBits(+18) and channelMask(+20)

        var pointer = Marshal.AllocHGlobal(blob.Length);
        Marshal.Copy(blob, 0, pointer, blob.Length);
        return pointer;
    }
}
