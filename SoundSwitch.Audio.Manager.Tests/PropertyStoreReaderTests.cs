using System;
using System.Runtime.InteropServices;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface;

namespace SoundSwitch.Audio.Manager.Tests;

/// <summary>
/// Pins the native-cleanup / missing-property contract of the property-store reader:
/// S_OK + VT_EMPTY means "property absent" (not an error), and string-valued VT_LPWSTR
/// PROPVARIANTs must be releasable with PropVariantClear after the value is copied out.
/// </summary>
[TestFixture]
public sealed class PropertyStoreReaderTests
{
    [Test]
    public void TryReadString_SOkWithVtEmpty_IsAbsentNotError()
    {
        var propVariant = new PropVariant { vt = (ushort)VarEnum.VT_EMPTY };

        var found = PropertyStoreReader.TryReadString(HRESULT.S_OK, ref propVariant, out var value);

        found.Should().BeFalse();
        value.Should().BeEmpty();
    }

    [Test]
    public void TryReadString_FailedHResult_IsAbsent()
    {
        var propVariant = new PropVariant { vt = (ushort)VarEnum.VT_LPWSTR };

        var found = PropertyStoreReader.TryReadString(HRESULT.ERROR_NOT_FOUND, ref propVariant, out var value);

        found.Should().BeFalse();
        value.Should().BeEmpty();
    }

    [Test]
    public void TryReadString_LpString_CopiesValueAndVariantClears()
    {
        // Allocate the string with the COM task allocator, exactly like a native property store
        // would, so the PropVariantClear path frees a real heap allocation.
        var nativeString = Marshal.StringToCoTaskMemUni("Realtek USB Audio");
        var propVariant = new PropVariant
        {
            vt = (ushort)VarEnum.VT_LPWSTR,
            data1 = nativeString
        };

        try
        {
            var found = PropertyStoreReader.TryReadString(HRESULT.S_OK, ref propVariant, out var value);

            found.Should().BeTrue();
            value.Should().Be("Realtek USB Audio");
        }
        finally
        {
            // The contract: the reader copies the value out; the caller clears the variant, which
            // frees the pwszVal heap allocation (and resets vt to VT_EMPTY).
            PropVariant.PropVariantClear(ref propVariant).Should().Be(0);
            propVariant.vt.Should().Be((ushort)VarEnum.VT_EMPTY);
        }
    }

    [Test]
    public void TryReadString_WrongVariantType_IsAbsent()
    {
        var propVariant = new PropVariant { vt = (ushort)VarEnum.VT_I4, data1 = new IntPtr(42) };

        var found = PropertyStoreReader.TryReadString(HRESULT.S_OK, ref propVariant, out var value);

        found.Should().BeFalse();
        value.Should().BeEmpty();
    }
}
