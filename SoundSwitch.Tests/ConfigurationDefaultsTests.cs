using FluentAssertions;
using NUnit.Framework;

using SoundSwitch.Framework.Configuration;

namespace SoundSwitch.Tests;

[TestFixture]
public class ConfigurationDefaultsTests
{
    /// <summary>
    /// Issue #2612: a fresh install must get communication-device switching enabled by
    /// default. Only fresh configurations use the property initializer — existing installs
    /// deserialize their explicit "ChangeCommunications" value from disk, so this default
    /// is new-install-only by design (docs/design/communications-default/2026-11-19-new-install-comms-default.md).
    /// </summary>
    [Test]
    public void FreshConfiguration_SwitchDefaultCommunicationDevice_IsEnabledByDefault()
    {
        new SoundSwitchConfiguration().ChangeCommunications.Should().BeTrue();
    }
}
