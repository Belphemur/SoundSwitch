using FluentAssertions;

using NAudio.CoreAudioApi;

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
    [TestCase(DataFlow.Render)]
    [TestCase(DataFlow.Capture)]
    public void ExtractIconFromPath_WhenPathIsInvalid_ReturnsFallbackIcon(DataFlow dataFlow)
    {
        using var iconHandle = AudioDeviceIconExtractor.ExtractIconFromPath("invalid-icon-path", dataFlow, false);

        iconHandle.Should().NotBeNull();
        iconHandle.Icon.Should().NotBeNull();
    }
}
