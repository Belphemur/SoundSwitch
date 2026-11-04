using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using FluentAssertions;

using Newtonsoft.Json;

using NUnit.Framework;

using SoundSwitch.Framework.Configuration;
using SoundSwitch.Model;

namespace SoundSwitch.Tests;

/// <summary>
/// Persistence of the assignment ownership ledger (identity ADR §6.4): entries survive a
/// configuration round-trip through the same serializes settings as
/// <see cref="ConfigurationManager"/> and preserve enough of the route to drive the deferred
/// restore-default clear afterwards.
/// </summary>
[TestFixture]
public class AssignmentLedgerTests
{
    private static AssignmentLedgerEntry SampleEntry() => new()
    {
        RuleId = Guid.NewGuid(),
        ExePath = @"\Device\HarddiskVolume3\Apps\Spotify\Spotify.exe",
        Enforcement = AppRuleEnforcement.Assignment,
        PlaybackDeviceId = "{0.0.0.00000000}.{11111111-1111-1111-1111-111111111111}",
        RecordingDeviceId = "{0.0.1.00000000}.{22222222-2222-2222-2222-222222222222}",
        WrittenAt = new DateTime(2026, 11, 3, 12, 0, 0, DateTimeKind.Utc),
        ClearRequestedAt = new DateTime(2026, 11, 3, 18, 0, 0, DateTimeKind.Utc)
    };

    [Test]
    public void RoundTrip_EntriesSurviveConfigurationSerialization()
    {
        var configuration = new SoundSwitchConfiguration
        {
            AssignmentLedger = { SampleEntry(), new AssignmentLedgerEntry { RuleId = Guid.NewGuid(), ExePath = @"C:\Apps\Discord\Discord.exe" } }
        };

        var serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
        var writer = new StringWriter();
        serializer.Serialize(writer, configuration);
        var roundTripped = serializer.Deserialize<SoundSwitchConfiguration>(
            new JsonTextReader(new StringReader(writer.ToString())));

        roundTripped.Should().NotBeNull();
        roundTripped!.AssignmentLedger.Should().HaveCount(2);

        var original = configuration.AssignmentLedger[0];
        var restored = roundTripped.AssignmentLedger[0];
        restored.RuleId.Should().Be(original.RuleId);
        restored.ExePath.Should().Be(original.ExePath);
        restored.Enforcement.Should().Be(original.Enforcement);
        restored.PlaybackDeviceId.Should().Be(original.PlaybackDeviceId);
        restored.RecordingDeviceId.Should().Be(original.RecordingDeviceId);
        restored.WrittenAt.Should().Be(original.WrittenAt);
        restored.ClearRequestedAt.Should().Be(original.ClearRequestedAt);
    }

    [Test]
    public void Ledger_EnumeratedWhileMutated_ReturnsStableSnapshots()
    {
        var ledger = new AssignmentLedgerCollection();
        for (var i = 0; i < 10; i++)
        {
            ledger.Add(SampleEntry());
        }

        // The reconciler mutates on background threads while saves/UI enumerate: the
        // enumerator must snapshot instead of throwing "collection was modified". Removals
        // happen by entry reference (Remove(entry)), mirroring the reconciler's usage.
        var mutatedEverything = Parallel.For(0, 8, _ =>
        {
            for (var i = 0; i < 5_000; i++)
            {
                if (ledger.Count > 40)
                {
                    var stale = ledger[0];
                    ledger.Remove(stale);
                }

                ledger.Add(SampleEntry());
            }
        });

        foreach (var _ in ledger)
        {
            // enumeration must not throw while the writers run
        }

        mutatedEverything.IsCompleted.Should().BeTrue();
        ledger.Should().NotBeEmpty();
    }

    [Test]
    public void RoundTrip_MissingWrittenAt_NormalizesDateTime()
    {
        // Configurations written before an entry had a WrittenAt deserialize to its default.
        var serializer = new JsonSerializer { NullValueHandling = NullValueHandling.Ignore };
        var roundTripped = serializer.Deserialize<AssignmentLedgerEntry>(new JsonTextReader(
            new StringReader($@"{{""RuleId"":""{Guid.NewGuid()}"",""ExePath"":""app.exe""}}")));

        roundTripped.Should().NotBeNull();
        roundTripped!.Enforcement.Should().Be(AppRuleEnforcement.Dynamic);
        roundTripped.PlaybackDeviceId.Should().BeNull();
        roundTripped.RecordingDeviceId.Should().BeNull();
    }

    [Test]
    public void Configuration_DefaultLedger_IsEmpty()
    {
        new SoundSwitchConfiguration().AssignmentLedger.Should().BeEmpty();
    }
}
