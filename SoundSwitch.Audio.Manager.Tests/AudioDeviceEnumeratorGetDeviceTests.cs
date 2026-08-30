using System;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;

namespace SoundSwitch.Audio.Manager.Tests;

/// <summary>
/// Regression tests for issue #2404: resolving a device by id must survive the window where
/// the OS fires a device lifecycle event before the endpoint is fully registered. The
/// round-trip exercises the real COM path (direct lookup + enumeration fallback); the bogus
/// id covers the fallback-then-null path. Real COM is expected here: these tests only run
/// where the Windows audio stack exists.
/// </summary>
[TestFixture]
public sealed class AudioDeviceEnumeratorGetDeviceTests
{
    private const string NonExistentDeviceId = "does-not-exist-id";

    [Test]
    [Platform("Win")]
    public void GetDevice_ReturnsDevice_WhenResolvingAnEnumeratedEndpoint()
    {
        var switcher = SoundSwitch.Audio.Manager.AudioSwitcher.Instance;
        var devices = switcher.GetAudioDevices(EDataFlow.eAll, EDeviceState.Active);

        // The round-trip is only meaningful when the machine exposes at least one endpoint.
        Assume.That(devices, Is.Not.Null.And.Not.Empty);

        var expectedId = devices![0].Id;

        try
        {
            using var resolved = switcher.GetDevice(expectedId);

            resolved.Should().NotBeNull("a registered endpoint must be resolvable by id");
            resolved!.Id.Should().Be(expectedId);
        }
        finally
        {
            foreach (var device in devices!) device.Dispose();
        }
    }

    [Test]
    [Platform("Win")]
    public void GetDevice_ReturnsNull_WhenIdDoesNotExist()
    {
        var switcher = SoundSwitch.Audio.Manager.AudioSwitcher.Instance;

        AudioDevice? resolved = null;
        Action act = () => resolved = switcher.GetDevice(NonExistentDeviceId);

        act.Should().NotThrow("an unknown id must resolve to null instead of propagating a COM failure");
        resolved.Should().BeNull("the enumeration fallback must not fabricate a device");
    }
}
