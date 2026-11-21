using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using NUnit.Framework;

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.Profile;

namespace SoundSwitch.Tests;

/// <summary>
/// Pins the volume-application decision logic of <see cref="ProfileVolumeApplier"/> (ADR
/// docs/design/profile/2026-11-21-profile-volume-settings.md, issue #2547): which preset
/// a device gets by flow, that null or disabled presets are no-ops, the percent → scalar
/// conversion at the COM boundary, and that a volume failure is skipped without ever
/// throwing at the profile switch. The COM device write/read is substituted at the
/// applier's internal seam, so no real audio endpoint is touched.
/// </summary>
[TestFixture]
public class ProfileVolumeApplierTests
{
    private static DeviceInfo Device(string name, EDataFlow flow) =>
        new(name, $"{name}-id", flow, false, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    /// <summary>An applier whose COM interactions are captured instead of executed.</summary>
    private static (ProfileVolumeApplier Applier, List<(string Id, float Scalar)> Writes) RecordingApplier(Func<string, float, bool>? result = null)
    {
        var writes = new List<(string Id, float Scalar)>();
        var applier = new ProfileVolumeApplier(null!)
        {
            DeviceVolumeWrite = (id, scalar) =>
            {
                writes.Add((id, scalar));
                return result?.Invoke(id, scalar) ?? true;
            }
        };
        return (applier, writes);
    }

    [Test]
    public void NullPresets_AreNoOps()
    {
        var (applier, writes) = RecordingApplier();
        var profile = new Profile { Name = "Work" };

        applier.Apply(profile, Device("speakers", EDataFlow.eRender)).Should().BeFalse();
        applier.Apply(profile, Device("microphone", EDataFlow.eCapture)).Should().BeFalse();

        writes.Should().BeEmpty();
    }

    [Test]
    public void DisabledPresets_AreNoOps()
    {
        var (applier, writes) = RecordingApplier();
        var profile = new Profile
        {
            PlaybackVolume = new ProfileVolumeSetting { Enabled = false, Volume = 35f },
            RecordingVolume = new ProfileVolumeSetting { Enabled = false, Volume = 80f }
        };

        applier.Apply(profile, Device("speakers", EDataFlow.eRender)).Should().BeFalse();
        applier.Apply(profile, Device("microphone", EDataFlow.eCapture)).Should().BeFalse();

        writes.Should().BeEmpty();
    }

    [Test]
    public void EnabledPlaybackPreset_AppliesToRenderDevices_AsScalar()
    {
        var (applier, writes) = RecordingApplier();
        var profile = new Profile
        {
            Name = "Work",
            PlaybackVolume = new ProfileVolumeSetting { Enabled = true, Volume = 35f }
        };

        applier.Apply(profile, Device("speakers", EDataFlow.eRender)).Should().BeTrue();

        writes.Should().HaveCount(1);
        writes[0].Id.Should().Be("speakers-id");
        writes[0].Scalar.Should().BeApproximately(0.35f, 0.0001f);
    }

    [Test]
    public void EnabledRecordingPreset_AppliesToCaptureDevices_AsScalar()
    {
        var (applier, writes) = RecordingApplier();
        var profile = new Profile
        {
            Name = "Work",
            RecordingVolume = new ProfileVolumeSetting { Enabled = true, Volume = 80f }
        };

        applier.Apply(profile, Device("microphone", EDataFlow.eCapture)).Should().BeTrue();

        writes.Should().HaveCount(1);
        writes[0].Id.Should().Be("microphone-id");
        writes[0].Scalar.Should().BeApproximately(0.8f, 0.0001f);
    }

    [Test]
    public void PresetsOnlyApplyToTheirOwnFlow()
    {
        var (applier, writes) = RecordingApplier();
        var profile = new Profile
        {
            Name = "Work",
            PlaybackVolume = new ProfileVolumeSetting { Enabled = true, Volume = 35f },
            RecordingVolume = new ProfileVolumeSetting { Enabled = true, Volume = 80f }
        };

        applier.Apply(profile, Device("speakers", EDataFlow.eRender)).Should().BeTrue();
        applier.Apply(profile, Device("microphone", EDataFlow.eCapture)).Should().BeTrue();

        // Exactly one write per device, each with its own flow's preset.
        writes.Should().HaveCount(2);
        writes.Should().Contain(w => w.Id == "speakers-id" && Math.Abs(w.Scalar - 0.35f) < 0.0001f);
        writes.Should().Contain(w => w.Id == "microphone-id" && Math.Abs(w.Scalar - 0.8f) < 0.0001f);
    }

    [Test]
    public void RecordingPreset_IgnoresRenderDevices_AndViceVersa()
    {
        var recordingOnly = new Profile { RecordingVolume = new ProfileVolumeSetting { Enabled = true, Volume = 80f } };
        var playbackOnly = new Profile { PlaybackVolume = new ProfileVolumeSetting { Enabled = true, Volume = 35f } };
        var (applier, writes) = RecordingApplier();

        applier.Apply(recordingOnly, Device("speakers", EDataFlow.eRender)).Should().BeFalse();
        applier.Apply(playbackOnly, Device("microphone", EDataFlow.eCapture)).Should().BeFalse();

        writes.Should().BeEmpty();
    }

    [Test]
    public void SliderBoundaries_AreLegalScalars()
    {
        var (applier, writes) = RecordingApplier();
        var silent = new Profile { PlaybackVolume = new ProfileVolumeSetting { Enabled = true, Volume = 0f } };
        var full = new Profile { PlaybackVolume = new ProfileVolumeSetting { Enabled = true, Volume = 100f } };

        // ADR Decision 5: 0 is a legal preset (intentionally silent), so it is a real
        // write of scalar 0 — not a no-op like a null or disabled preset.
        applier.Apply(silent, Device("speakers", EDataFlow.eRender)).Should().BeTrue();
        writes[0].Scalar.Should().Be(0f);

        applier.Apply(full, Device("speakers", EDataFlow.eRender)).Should().BeTrue();
        writes[1].Scalar.Should().Be(1f);
    }

    [Test]
    public void WriteFailure_IsSkippedAndNeverThrows()
    {
        var applier = new ProfileVolumeApplier(null!)
        {
            DeviceVolumeWrite = (_, _) => throw new InvalidOperationException("COM endpoint volume failure")
        };
        var profile = new Profile { PlaybackVolume = new ProfileVolumeSetting { Enabled = true, Volume = 35f } };

        var applied = applier.Apply(profile, Device("speakers", EDataFlow.eRender));

        applied.Should().BeFalse("a volume failure must be skipped, the profile switch itself is unaffected");
    }

    [Test]
    public void UnresolvedDevice_IsSkipped()
    {
        // The write seam reports "not applied" (production: GetDevice found nothing).
        var (applier, writes) = RecordingApplier(result: (_, _) => false);
        var profile = new Profile { PlaybackVolume = new ProfileVolumeSetting { Enabled = true, Volume = 35f } };

        applier.Apply(profile, Device("speakers", EDataFlow.eRender)).Should().BeFalse();
        writes.Should().HaveCount(1);
    }

    [Test]
    public void HasActivePreset_RequiresAnEnabledPreset()
    {
        ProfileVolumeApplier.HasActivePreset(new Profile()).Should().BeFalse();
        ProfileVolumeApplier.HasActivePreset(new Profile
        {
            PlaybackVolume = new ProfileVolumeSetting { Enabled = false, Volume = 35f },
            RecordingVolume = new ProfileVolumeSetting { Enabled = false, Volume = 80f }
        }).Should().BeFalse();
        ProfileVolumeApplier.HasActivePreset(new Profile
        {
            PlaybackVolume = new ProfileVolumeSetting { Enabled = true, Volume = 35f }
        }).Should().BeTrue();
        ProfileVolumeApplier.HasActivePreset(new Profile
        {
            RecordingVolume = new ProfileVolumeSetting { Enabled = true, Volume = 80f }
        }).Should().BeTrue();
    }

    [Test]
    public void Restore_WritesTheSnapshottedScalarBackOnTheDevice()
    {
        var (applier, writes) = RecordingApplier();
        var device = Device("speakers", EDataFlow.eRender);

        applier.Restore(device, 0.42f).Should().BeTrue();

        writes.Should().HaveCount(1);
        writes[0].Id.Should().Be("speakers-id");
        writes[0].Scalar.Should().BeApproximately(0.42f, 0.0001f);
    }

    [Test]
    public void RestoreFailure_IsSkippedAndNeverThrows()
    {
        var applier = new ProfileVolumeApplier(null!)
        {
            DeviceVolumeWrite = (_, _) => throw new InvalidOperationException("COM endpoint volume failure")
        };

        applier.Restore(Device("speakers", EDataFlow.eRender), 0.42f).Should().BeFalse();
    }

    [Test]
    public void SnapshotVolumes_ReadsEveryNonNullDevice()
    {
        var reads = new Dictionary<string, float?>
        {
            ["communication-id"] = 0.11f,
            ["playback-id"] = 0.22f,
            ["recording-id"] = null, // unreadable device → skipped
            ["recordingCommunication-id"] = 0.44f
        };
        var applier = new ProfileVolumeApplier(null!)
        {
            DeviceVolumeRead = id => reads.TryGetValue(id, out var scalar) ? scalar : throw new InvalidOperationException("no such device")
        };

        var snapshot = applier.SnapshotVolumes(new DeviceInfo?[]
        {
            Device("communication", EDataFlow.eRender),
            Device("playback", EDataFlow.eRender),
            Device("recording", EDataFlow.eCapture),
            Device("recordingCommunication", EDataFlow.eCapture),
            null // no default device in that slot
        });

        snapshot.Should().HaveCount(3);
        snapshot.Should().Contain(s => s.Device.Id == "communication-id" && Math.Abs(s.VolumeScalar - 0.11f) < 0.0001f);
        snapshot.Should().Contain(s => s.Device.Id == "playback-id" && Math.Abs(s.VolumeScalar - 0.22f) < 0.0001f);
        snapshot.Should().Contain(s => s.Device.Id == "recordingCommunication-id" && Math.Abs(s.VolumeScalar - 0.44f) < 0.0001f);
        snapshot.Should().NotContain(s => s.Device.Id == "recording-id");
    }

    [Test]
    public void SnapshotReadFailure_IsSkippedForThatDeviceOnly()
    {
        var applier = new ProfileVolumeApplier(null!)
        {
            DeviceVolumeRead = id => id == "broken-id" ? throw new InvalidOperationException("COM read failure") : 0.5f
        };

        var snapshot = applier.SnapshotVolumes(new DeviceInfo?[]
        {
            Device("broken", EDataFlow.eRender),
            Device("healthy", EDataFlow.eCapture)
        });

        snapshot.Should().ContainSingle().Which.Device.Id.Should().Be("healthy-id");
    }

    [Test]
    public void SnapshotOfOnlyNullSlots_IsEmpty()
    {
        var applier = new ProfileVolumeApplier(null!) { DeviceVolumeRead = _ => throw new InvalidOperationException("must not be called") };

        applier.SnapshotVolumes(new DeviceInfo?[] { null, null }).Should().BeEmpty();
    }

    [Test]
    public void DevicesToSnapshot_UnionsDefaultEndpointsAndPresetTargets()
    {
        var defaults = new DeviceInfo?[] { Device("speakers", EDataFlow.eRender), Device("microphone", EDataFlow.eCapture), null };
        var targets = new[] { Device("headset", EDataFlow.eRender) };

        var devices = ProfileVolumeApplier.DevicesToSnapshot(defaults, targets).ToArray();

        // The profile target differs from the current default endpoints and must be
        // snapshotted alongside them, or its volume would never be restored.
        devices.Should().HaveCount(3);
        devices.Select(device => device.Id).Should().BeEquivalentTo("speakers-id", "microphone-id", "headset-id");
    }

    [Test]
    public void DevicesToSnapshot_DeduplicatesByEndpointId()
    {
        var speakers = Device("speakers", EDataFlow.eRender);
        var defaults = new DeviceInfo?[] { Device("microphone", EDataFlow.eCapture), speakers };
        var targets = new[] { speakers, Device("headset", EDataFlow.eRender) };

        var devices = ProfileVolumeApplier.DevicesToSnapshot(defaults, targets).ToArray();

        devices.Should().HaveCount(3);
        devices.Count(device => device.Id == "speakers-id").Should().Be(1, "one snapshot per endpoint id, whatever slot named it");
    }

    [Test]
    public void SnapshotAndRestore_CoversAPresetTargetThatIsNotTheDefault()
    {
        var reads = new Dictionary<string, float?>
        {
            ["speakers-id"] = 0.3f,
            ["headset-id"] = 0.9f
        };
        var writes = new List<(string Id, float Scalar)>();
        var applier = new ProfileVolumeApplier(null!)
        {
            DeviceVolumeRead = id => reads.TryGetValue(id, out var scalar) ? scalar : throw new InvalidOperationException("no such device"),
            DeviceVolumeWrite = (id, scalar) =>
            {
                writes.Add((id, scalar));
                return true;
            }
        };

        // The review regression (trigger profile targeting a device other than the
        // current default): deactivation must restore the target's own volume, not
        // only the default endpoints'.
        var snapshot = applier.SnapshotVolumes(ProfileVolumeApplier.DevicesToSnapshot(
            new DeviceInfo?[] { Device("speakers", EDataFlow.eRender) },
            new[] { Device("headset", EDataFlow.eRender) }));

        foreach (var entry in snapshot)
        {
            applier.Restore(entry.Device, entry.VolumeScalar);
        }

        writes.Should().HaveCount(2);
        writes.Should().Contain(write => write.Id == "speakers-id" && Math.Abs(write.Scalar - 0.3f) < 0.0001f);
        writes.Should().Contain(write => write.Id == "headset-id" && Math.Abs(write.Scalar - 0.9f) < 0.0001f);
    }

    [Test]
    public void HeldVolumes_KeepsTheEarliestRecordPerEndpoint()
    {
        var olderState = (Sequence: 1, Volumes: new[] { (Device("speakers", EDataFlow.eRender), 0.3f) });
        var newerState = (Sequence: 2, Volumes: new[]
        {
            (Device("speakers", EDataFlow.eRender), 0.5f),
            (Device("headset", EDataFlow.eRender), 0.7f)
        });

        // Enumeration order of the states must not matter: the sequence decides.
        var held = ProfileVolumeApplier.HeldVolumes(new[] { newerState, olderState });

        held["speakers-id"].Should().Be(0.3f, "the earliest active record is the endpoint's true pre-trigger volume");
        held["headset-id"].Should().Be(0.7f);
        held.Count.Should().Be(2);
    }

    [Test]
    public void InheritHeldVolumes_OverlaysTheHeldOriginals()
    {
        var held = new Dictionary<string, float> { ["speakers-id"] = 0.3f };
        var freshlyRead = new[]
        {
            (Device("speakers", EDataFlow.eRender), 0.5f),   // currently overridden by the active trigger
            (Device("headset", EDataFlow.eRender), 0.9f)      // untouched by it
        };

        var inherited = ProfileVolumeApplier.InheritHeldVolumes(freshlyRead, held);

        inherited.Should().HaveCount(2);
        inherited.Single(entry => entry.Device.Id == "speakers-id").VolumeScalar.Should().Be(0.3f);
        inherited.Single(entry => entry.Device.Id == "headset-id").VolumeScalar.Should().Be(0.9f);
    }

    [Test]
    public void OverlappingTriggers_RestoreTheOriginalNotAnIntermediateValue()
    {
        // Review round 1 (qodo finding 6): two trigger windows apply presets to the
        // same endpoint and close in the opposite order of their activation.
        var speakers = Device("speakers", EDataFlow.eRender);

        // Window 1 activates while the endpoint sits at its original 30%.
        var firstState = (Sequence: 1, Volumes: new[] { (speakers, 0.3f) });
        var heldAfterFirst = ProfileVolumeApplier.HeldVolumes(new[] { firstState });

        // Window 2 activates and reads the endpoint at the first trigger's preset (50%):
        // it must inherit the first trigger's original instead.
        var secondRead = new[] { (speakers, 0.5f) };
        var secondVolumes = ProfileVolumeApplier.InheritHeldVolumes(secondRead, heldAfterFirst);
        var secondState = (Sequence: 2, Volumes: secondVolumes);

        // Window 1 closes while window 2 is still active: the endpoint is held, so its
        // restore is skipped and window 2's preset keeps governing the volume.
        var heldWhenFirstCloses = ProfileVolumeApplier.HeldVolumes(new[] { secondState });
        heldWhenFirstCloses.ContainsKey("speakers-id").Should().BeTrue();

        // Window 2 closes last: nothing else holds the endpoint, and its inherited
        // snapshot is the true original, not the intermediate preset.
        var heldWhenSecondCloses = ProfileVolumeApplier.HeldVolumes(Array.Empty<(int, (DeviceInfo Device, float VolumeScalar)[])>());
        heldWhenSecondCloses.ContainsKey("speakers-id").Should().BeFalse();
        secondVolumes.Single().VolumeScalar.Should().Be(0.3f);
    }
}
