using System.IO;

using FluentAssertions;

using Newtonsoft.Json;

using NUnit.Framework;

using SoundSwitch.Framework.Profile;

namespace SoundSwitch.Tests;

/// <summary>
/// Pins the profile volume preset schema introduced for issue #2547 (mockup phase):
/// the new <see cref="Profile.PlaybackVolume"/>/<see cref="Profile.RecordingVolume"/>
/// fields are optional members that stay absent from saved JSON while off, so existing
/// configuration files deserialize unchanged with no migration.
/// </summary>
[TestFixture]
public class ProfileVolumeSerializationTests
{
    /// <summary>Matches the configuration serializer used by <c>ConfigurationManager</c>.</summary>
    private static readonly JsonSerializer Serializer = new() { NullValueHandling = NullValueHandling.Ignore };

    [Test]
    public void Deserialize_ExistingProfileJson_LeavesVolumeFieldsNull_NoMigration()
    {
        // A profile as saved by the previous version: devices and triggers, no volume data.
        const string legacyJson = @"{
  ""Name"": ""Work Headset"",
  ""Playback"": {
    ""Name"": ""Headset (USB Audio)"",
    ""Id"": ""{0.0.0.00000000}.{1}"",
    ""Type"": 0,
    ""IsUsb"": true,
    ""DiscoveredAt"": ""2026-01-01T00:00:00Z""
  },
  ""Recording"": {
    ""Name"": ""Microphone (USB Audio)"",
    ""Id"": ""{0.0.1.00000000}.{2}"",
    ""Type"": 1,
    ""IsUsb"": true,
    ""DiscoveredAt"": ""2026-01-01T00:00:00Z""
  },
  ""AlsoSwitchDefaultDevice"": true,
  ""NotifyOnActivation"": true
}";

        var profile = JsonConvert.DeserializeObject<Profile>(legacyJson);

        profile.Should().NotBeNull();
        profile!.PlaybackVolume.Should().BeNull();
        profile.RecordingVolume.Should().BeNull();
        profile.Name.Should().Be("Work Headset");
    }

    [Test]
    public void Serialize_VolumeFieldsAbsent_WhileFeatureOff()
    {
        var writer = new StringWriter();
        Serializer.Serialize(writer, new Profile { Name = "Work" });

        var json = writer.ToString();

        json.Should().NotContain("PlaybackVolume");
        json.Should().NotContain("RecordingVolume");
    }

    [Test]
    public void Serialize_RoundTrip_PreservesVolumePresets()
    {
        var writer = new StringWriter();
        Serializer.Serialize(writer, new Profile
        {
            Name = "Work",
            PlaybackVolume = new ProfileVolumeSetting { Enabled = true, Volume = 35f },
            RecordingVolume = new ProfileVolumeSetting { Enabled = true, Volume = 80.5f }
        });

        var restored = JsonConvert.DeserializeObject<Profile>(writer.ToString());

        restored.Should().NotBeNull();
        restored!.PlaybackVolume.Should().NotBeNull();
        restored.PlaybackVolume!.Enabled.Should().BeTrue();
        restored.PlaybackVolume.Volume.Should().Be(35f);
        restored.RecordingVolume.Should().NotBeNull();
        restored.RecordingVolume!.Enabled.Should().BeTrue();
        restored.RecordingVolume.Volume.Should().Be(80.5f);
    }

    [Test]
    public void Copy_DeepCopiesVolumePresets()
    {
        var profile = new Profile
        {
            Name = "Work",
            PlaybackVolume = new ProfileVolumeSetting { Enabled = true, Volume = 35f },
            RecordingVolume = new ProfileVolumeSetting { Enabled = false, Volume = 80f }
        };

        var copy = profile.Copy();

        copy.PlaybackVolume.Should().NotBeNull();
        copy.PlaybackVolume!.Enabled.Should().BeTrue();
        copy.PlaybackVolume.Volume.Should().Be(35f);
        copy.RecordingVolume.Should().NotBeNull();
        copy.RecordingVolume!.Enabled.Should().BeFalse();
        copy.RecordingVolume.Volume.Should().Be(80f);

        // Deep copy: mutating the copy's preset never leaks into the source profile.
        copy.PlaybackVolume.Volume = 1f;
        profile.PlaybackVolume!.Volume.Should().Be(35f);
    }

    [Test]
    public void Copy_WithoutVolumePresets_KeepsThemNull()
    {
        var copy = new Profile().Copy();

        copy.PlaybackVolume.Should().BeNull();
        copy.RecordingVolume.Should().BeNull();
    }
}
