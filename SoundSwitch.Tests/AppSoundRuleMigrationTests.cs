using System;
using System.IO;

using FluentAssertions;

using Newtonsoft.Json;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Model;

#pragma warning disable CS0618 // Type or member is obsolete

namespace SoundSwitch.Tests;

/// <summary>
/// Tests the migration of <see cref="AppSoundRule"/>s from the raw device id strings
/// (<see cref="AppSoundRule.PlaybackDeviceId"/>/<see cref="AppSoundRule.RecordingDeviceId"/>)
/// to the full <see cref="DeviceInfo"/> persistence
/// (<see cref="AppSoundRule.PlaybackDevice"/>/<see cref="AppSoundRule.RecordingDevice"/>).
/// </summary>
[TestFixture]
public class AppSoundRuleMigrationTests
{
    private const string OldPlaybackId = "{0.0.0.00000000}.{11111111-1111-1111-1111-111111111111}";
    private const string OldRecordingId = "{0.0.1.00000000}.{22222222-2222-2222-2222-222222222222}";

    [Test]
    public void Migrate_OldJsonRule_CreatesDeviceInfoAndClearsObsoleteIds()
    {
        var json = $@"{{""Id"":""{Guid.NewGuid()}"",""ProcessPath"":""app.exe"",""PlaybackDeviceId"":""{OldPlaybackId}"",""RecordingDeviceId"":""{OldRecordingId}""}}";
        var rule = JsonConvert.DeserializeObject<AppSoundRule>(json);

        rule.Should().NotBeNull();
        rule!.PlaybackDevice.Should().BeNull("the old configuration never persisted the device info");
        rule.PlaybackDeviceId.Should().Be(OldPlaybackId, "the old id must deserialize for the migration to have data to work with");

        var migrated = AppSoundRuleMigrator.Migrate(new[] { rule });

        migrated.Should().BeTrue();
        rule.PlaybackDevice.Should().NotBeNull();
        rule.PlaybackDevice!.Type.Should().Be(EDataFlow.eRender);
        rule.PlaybackDevice.Id.Should().Be(OldPlaybackId);
        rule.PlaybackDevice.Name.Should().Be(OldPlaybackId, "the name was never persisted, the id is the honest best effort until self-heal");
        rule.RecordingDevice.Should().NotBeNull();
        rule.RecordingDevice!.Type.Should().Be(EDataFlow.eCapture);
        rule.RecordingDevice.Id.Should().Be(OldRecordingId);
        rule.PlaybackDeviceId.Should().BeNull();
        rule.RecordingDeviceId.Should().BeNull();
    }

    [Test]
    public void Migrate_AlreadyMigratedRule_IsIdempotent()
    {
        var rule = new AppSoundRule
        {
            PlaybackDevice = new DeviceInfo("Speakers (Realtek(R) Audio)", OldPlaybackId, EDataFlow.eRender, true, DateTime.UtcNow),
            RecordingDevice = new DeviceInfo("Microphone (Realtek(R) Audio)", OldRecordingId, EDataFlow.eCapture, false, DateTime.UtcNow)
        };

        var migrated = AppSoundRuleMigrator.Migrate(new[] { rule });

        migrated.Should().BeFalse();
        rule.PlaybackDevice!.Name.Should().Be("Speakers (Realtek(R) Audio)");
        rule.PlaybackDevice.Type.Should().Be(EDataFlow.eRender);
        rule.RecordingDevice!.Name.Should().Be("Microphone (Realtek(R) Audio)");
        rule.RecordingDevice.Type.Should().Be(EDataFlow.eCapture);
    }

    [Test]
    public void Migrate_NullOrEmptyDeviceIds_PassThroughUntouched()
    {
        var rule = new AppSoundRule();

        var migrated = AppSoundRuleMigrator.Migrate(new[] { rule });

        migrated.Should().BeFalse();
        rule.PlaybackDevice.Should().BeNull();
        rule.RecordingDevice.Should().BeNull();
    }

    [Test]
    public void Migrate_MigratedRule_DoesNotPersistObsoleteIds()
    {
        var json = $@"{{""PlaybackDeviceId"":""{OldPlaybackId}"",""RecordingDeviceId"":""{OldRecordingId}""}}";
        var rule = JsonConvert.DeserializeObject<AppSoundRule>(json)!;
        AppSoundRuleMigrator.Migrate(new[] { rule });

        // Same settings as ConfigurationManager: NullValueHandling.Ignore
        var serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
        var writer = new StringWriter();
        serializer.Serialize(writer, rule);

        var serialized = writer.ToString();
        serialized.Should().NotContain(nameof(AppSoundRule.PlaybackDeviceId));
        serialized.Should().NotContain(nameof(AppSoundRule.RecordingDeviceId));
        serialized.Should().Contain(nameof(AppSoundRule.PlaybackDevice));
        serialized.Should().Contain(nameof(AppSoundRule.RecordingDevice));
    }
}
