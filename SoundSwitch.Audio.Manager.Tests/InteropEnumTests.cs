using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Audio.Manager.Interop.Interface;

namespace SoundSwitch.Audio.Manager.Tests;

/// <summary>
/// Pins the numeric values of the interop enums passed to (and returned by) the raw COM
/// interfaces. A silent renumber here would corrupt vtable-level calls without failing to
/// compile, so the wire contract is locked by tests instead.
/// </summary>
[TestFixture]
public sealed class InteropEnumTests
{
    [Test]
    public void ERole_UsesNativeValues()
    {
        ((uint)ERole.eConsole).Should().Be(0u);
        ((uint)ERole.eMultimedia).Should().Be(1u);
        ((uint)ERole.eCommunications).Should().Be(2u);
        ((uint)ERole.ERole_enum_count).Should().Be(3u);
    }

    [Test]
    public void ERole_CannotBeUsedAsBitFlags()
    {
        // The native values are not powers of two: OR-ing eConsole (= 0) into anything is a no-op,
        // and HasFlag(eConsole) is true for EVERY value. Consumers must match against a set of
        // individual roles instead (see Profile.Devices).
        (ERole.eConsole | ERole.eMultimedia).Should().Be(ERole.eMultimedia);
        ERole.eCommunications.HasFlag(ERole.eConsole).Should().BeTrue();
        ((ERole)99u).HasFlag(ERole.eConsole).Should().BeTrue();
    }

    [Test]
    public void EDataFlow_UsesNativeValues()
    {
        ((uint)EDataFlow.eRender).Should().Be(0u);
        ((uint)EDataFlow.eCapture).Should().Be(1u);
        ((uint)EDataFlow.eAll).Should().Be(2u);
        ((uint)EDataFlow.EDataFlow_enum_count).Should().Be(3u);
    }

    [Test]
    public void EDeviceState_IsBitmask()
    {
        ((uint)EDeviceState.Active).Should().Be(1u);
        ((uint)EDeviceState.Disabled).Should().Be(2u);
        ((uint)EDeviceState.NotPresent).Should().Be(4u);
        ((uint)EDeviceState.Unplugged).Should().Be(8u);
        ((uint)EDeviceState.All).Should().Be(0xFu);
        (EDeviceState.Active | EDeviceState.Unplugged).Should().Be((EDeviceState)9u);
    }

    [Test]
    public void AudioSessionState_UsesNativeValues()
    {
        ((int)AudioSessionState.Inactive).Should().Be(0);
        ((int)AudioSessionState.Active).Should().Be(1);
        ((int)AudioSessionState.Expired).Should().Be(2);
    }

    [Test]
    public void AudioClientEnums_KeepNativeValues()
    {
        ((int)AudioClientShareMode.Shared).Should().Be(0);
        ((int)AudioClientShareMode.Exclusive).Should().Be(1);
        ((int)AudioClientStreamFlags.None).Should().Be(0);
        ((uint)AudioClientStreamFlags.EventCallback).Should().Be(0x00040000u);
        ((int)AudioClientBufferFlags.None).Should().Be(0);
        ((uint)AudioClientBufferFlags.Silent).Should().Be(0x2u);
    }
}
