using FluentAssertions;

using SoundSwitch.Audio.Manager.Interop.Enum;

using NUnit.Framework;

using SoundSwitch.Common.Framework.Audio.Icon;

namespace SoundSwitch.Tests;

/// <summary>
/// Tests for the <see cref="AudioDeviceIconExtractor"/> class, verifying icon extraction
/// behavior for audio devices including fallback handling for invalid icon paths.
/// </summary>
[TestFixture]
public class AudioDeviceIconExtractorTests
{
    [TestCase(EDataFlow.eRender)]
    [TestCase(EDataFlow.eCapture)]
    public void ExtractIconFromPath_WhenPathIsInvalid_ReturnsFallbackIcon(EDataFlow dataFlow)
    {
        using var iconHandle = AudioDeviceIconExtractor.ExtractIconFromPath("invalid-icon-path", dataFlow, false);

        iconHandle.Should().NotBeNull();
        iconHandle.Icon.Should().NotBeNull();
    }
}
