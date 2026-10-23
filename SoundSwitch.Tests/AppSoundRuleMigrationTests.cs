using System;
using System.IO;
using System.Linq;

using FluentAssertions;

using Newtonsoft.Json;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Model;

#pragma warning disable CS0618 // Type or member is obsolete

namespace SoundSwitch.Tests;

/// <summary>
/// Tests the schema migrations of <see cref="AppSoundRule"/>:
/// raw device ids → <see cref="DeviceInfo"/> → ordered device pairs, plus the
/// short-lived PR #2522 hotkey schema → <see cref="AppRuleMode.Hotkey"/>.
/// </summary>
[TestFixture]
public class AppSoundRuleMigrationTests
{
    private const string OldPlaybackId = "{0.0.0.00000000}.{11111111-1111-1111-1111-111111111111}";
    private const string OldRecordingId = "{0.0.1.00000000}.{22222222-2222-2222-2222-222222222222}";
    private static readonly HotKey RuleHotKey = new(System.Windows.Forms.Keys.F1, HotKey.ModifierKeys.Control | HotKey.ModifierKeys.Shift);

    private static DeviceInfo Playback(string name = "Speakers (Realtek(R) Audio)") =>
        new(name, OldPlaybackId, EDataFlow.eRender, true, DateTime.UtcNow);

    private static DeviceInfo Recording(string name = "Microphone (Realtek(R) Audio)") =>
        new(name, OldRecordingId, EDataFlow.eCapture, false, DateTime.UtcNow);

    [Test]
    public void Migrate_OldJsonRule_CreatesSinglePairAndClearsObsoleteIds()
    {
        var json = $@"{{""Id"":""{Guid.NewGuid()}"",""ProcessPath"":""app.exe"",""PlaybackDeviceId"":""{OldPlaybackId}"",""RecordingDeviceId"":""{OldRecordingId}""}}";
        var rule = JsonConvert.DeserializeObject<AppSoundRule>(json);

        rule.Should().NotBeNull();
        rule!.DevicePairs.Should().BeEmpty("the old configuration never persisted a pair list");

        var migrated = AppSoundRuleMigrator.Migrate(new[] { rule });

        migrated.Should().BeTrue();
        rule.Mode.Should().Be(AppRuleMode.Automatic);
        rule.DevicePairs.Should().ContainSingle();
        rule.DevicePairs[0].Playback.Should().NotBeNull();
        rule.DevicePairs[0].Playback!.Id.Should().Be(OldPlaybackId);
        rule.DevicePairs[0].Playback.Type.Should().Be(EDataFlow.eRender);
        rule.DevicePairs[0].Playback.Name.Should().Be(OldPlaybackId, "the name was never persisted; the id is the best effort until self-heal");
        rule.DevicePairs[0].Recording.Should().NotBeNull();
        rule.DevicePairs[0].Recording!.Id.Should().Be(OldRecordingId);
        rule.DevicePairs[0].Recording.Type.Should().Be(EDataFlow.eCapture);
        rule.PlaybackDevice.Should().BeNull();
        rule.RecordingDevice.Should().BeNull();
        rule.PlaybackDeviceId.Should().BeNull();
        rule.RecordingDeviceId.Should().BeNull();
    }

    [Test]
    public void Migrate_LegacyDevices_CreatesSinglePairAndClearsLegacyProperties()
    {
        var playback = Playback();
        var recording = Recording();
        var rule = new AppSoundRule
        {
            PlaybackDevice = playback,
            RecordingDevice = recording
        };

        var migrated = AppSoundRuleMigrator.Migrate(new[] { rule });

        migrated.Should().BeTrue();
        rule.Mode.Should().Be(AppRuleMode.Automatic);
        rule.DevicePairs.Should().ContainSingle();
        rule.DevicePairs[0].Playback.Should().BeSameAs(playback);
        rule.DevicePairs[0].Recording.Should().BeSameAs(recording);
        rule.PlaybackDevice.Should().BeNull();
        rule.RecordingDevice.Should().BeNull();
    }

    [Test]
    public void Migrate_PlaybackOnly_CreatesPairWithNullRecording()
    {
        var rule = new AppSoundRule { PlaybackDevice = Playback() };

        AppSoundRuleMigrator.Migrate(new[] { rule });

        rule.DevicePairs.Should().ContainSingle();
        rule.DevicePairs[0].Playback.Should().NotBeNull();
        rule.DevicePairs[0].Recording.Should().BeNull();
    }

    [Test]
    public void Migrate_RecordingOnly_CreatesPairWithNullPlayback()
    {
        var rule = new AppSoundRule { RecordingDevice = Recording() };

        AppSoundRuleMigrator.Migrate(new[] { rule });

        rule.DevicePairs.Should().ContainSingle();
        rule.DevicePairs[0].Playback.Should().BeNull();
        rule.DevicePairs[0].Recording.Should().NotBeNull();
    }

    [Test]
    public void Migrate_NullDevicePairs_CreatesEmptyList()
    {
        var rule = new AppSoundRule { DevicePairs = null! };

        var migrated = AppSoundRuleMigrator.Migrate(new[] { rule });

        migrated.Should().BeTrue();
        rule.DevicePairs.Should().NotBeNull();
        rule.DevicePairs.Should().BeEmpty();
    }

    [Test]
    public void Migrate_NullAndEmptyPairEntries_AreRemoved()
    {
        var valid = new AppSoundRuleDevicePair { Playback = Playback() };
        var rule = new AppSoundRule
        {
            DevicePairs = { null!, new AppSoundRuleDevicePair(), valid }
        };

        var migrated = AppSoundRuleMigrator.Migrate(new[] { rule });

        migrated.Should().BeTrue();
        rule.DevicePairs.Should().ContainSingle().Which.Should().BeSameAs(valid);
    }

    [Test]
    public void Migrate_NoDevices_LeavesEmptyPairList()
    {
        var rule = new AppSoundRule();

        var migrated = AppSoundRuleMigrator.Migrate(new[] { rule });

        migrated.Should().BeFalse();
        rule.DevicePairs.Should().BeEmpty();
    }

    [Test]
    public void Migrate_HotkeyRuleWithoutMode_BecomesHotkey()
    {
        // The schema shipped by PR #2522: a single device pair plus a hotkey,
        // but no Mode property at all.
        var json = $@"{{""ProcessPath"":""app.exe"",""HotKey"":{{""Keys"":{ (int)System.Windows.Forms.Keys.F1 },""Modifier"":{(int)(HotKey.ModifierKeys.Control | HotKey.ModifierKeys.Shift)},""Enabled"":true}},""PlaybackDevice"":{{""Name"":""Speakers (Realtek(R) Audio)"",""Id"":""{OldPlaybackId}"",""Type"":0,""IsUsb"":true,""DiscoveredAt"":""2026-01-01T00:00:00Z""}}}}";
        var rule = JsonConvert.DeserializeObject<AppSoundRule>(json);

        rule.Should().NotBeNull();
        rule!.Mode.Should().Be(AppRuleMode.Automatic, "the missing Mode deserializes to the enum default");

        AppSoundRuleMigrator.Migrate(new[] { rule });

        rule.Mode.Should().Be(AppRuleMode.Hotkey);
        rule.HotKey.Should().NotBeNull();
        rule.DevicePairs.Should().ContainSingle();
        rule.DevicePairs[0].Playback!.Id.Should().Be(OldPlaybackId);
    }

    [Test]
    public void Migrate_ExplicitAutomaticRuleWithPairsAndHotkey_KeepsAutomaticMode()
    {
        var rule = new AppSoundRule
        {
            Mode = AppRuleMode.Automatic,
            HotKey = RuleHotKey,
            DevicePairs =
            {
                new AppSoundRuleDevicePair { Playback = Playback() },
                new AppSoundRuleDevicePair { Recording = Recording() }
            }
        };

        AppSoundRuleMigrator.Migrate(new[] { rule });

        rule.Mode.Should().Be(AppRuleMode.Automatic);
        rule.DevicePairs.Should().HaveCount(2);
    }

    [Test]
    public void Migrate_InvalidMode_DefaultsToAutomatic()
    {
        var rule = new AppSoundRule { Mode = (AppRuleMode)99 };

        var migrated = AppSoundRuleMigrator.Migrate(new[] { rule });

        migrated.Should().BeTrue();
        rule.Mode.Should().Be(AppRuleMode.Automatic);
    }

    [Test]
    public void Migrate_IsIdempotent()
    {
        var rule = new AppSoundRule
        {
            PlaybackDevice = Playback(),
            RecordingDevice = Recording()
        };

        AppSoundRuleMigrator.Migrate(new[] { rule }).Should().BeTrue();
        var pair = rule.DevicePairs[0];

        var secondRun = AppSoundRuleMigrator.Migrate(new[] { rule });

        secondRun.Should().BeFalse();
        rule.DevicePairs.Should().ContainSingle();
        rule.DevicePairs[0].Should().BeSameAs(pair);
        rule.Mode.Should().Be(AppRuleMode.Automatic);
    }

    [Test]
    public void Migrate_MigratedRule_DoesNotPersistLegacyProperties()
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
        serialized.Should().NotContain(nameof(AppSoundRule.PlaybackDevice));
        serialized.Should().NotContain(nameof(AppSoundRule.RecordingDevice));
        serialized.Should().Contain(nameof(AppSoundRule.DevicePairs));
        serialized.Should().Contain(nameof(AppSoundRule.Mode));
    }

    [Test]
    public void Migrate_LegacyIdsWithExistingPair_KeepsPairAndClearsLegacy()
    {
        var existing = new AppSoundRuleDevicePair { Playback = Playback("Headset (USB Audio)") };
        var rule = new AppSoundRule
        {
            DevicePairs = { existing },
            PlaybackDeviceId = OldPlaybackId,
            RecordingDeviceId = OldRecordingId
        };

        AppSoundRuleMigrator.Migrate(new[] { rule });

        rule.DevicePairs.Should().ContainSingle();
        rule.DevicePairs[0].Should().BeSameAs(existing);
        rule.PlaybackDeviceId.Should().BeNull();
        rule.RecordingDeviceId.Should().BeNull();
    }

    [Test]
    public void Migrate_NewSchemaRuleWithPairs_DoesNotChangeAnything()
    {
        var rule = new AppSoundRule
        {
            Mode = AppRuleMode.Hotkey,
            HotKey = RuleHotKey,
            DevicePairs =
            {
                new AppSoundRuleDevicePair { Playback = Playback() },
                new AppSoundRuleDevicePair { Recording = Recording() }
            }
        };

        var migrated = AppSoundRuleMigrator.Migrate(new[] { rule });

        migrated.Should().BeFalse();
        rule.Mode.Should().Be(AppRuleMode.Hotkey);
        rule.DevicePairs.Should().HaveCount(2);
        rule.DevicePairs.Select(pair => pair.Playback?.Id).Should().Equal(OldPlaybackId, null);
        rule.DevicePairs.Select(pair => pair.Recording?.Id).Should().Equal(null, OldRecordingId);
    }
}
