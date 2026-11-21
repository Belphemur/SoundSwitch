#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

using Serilog;

using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Framework.Profile;

/// <summary>
/// Applies a profile's volume presets to a device resolved for activation, and writes
/// snapshotted volumes back on trigger deactivation (ADR:
/// docs/design/profile/2026-11-21-profile-volume-settings.md, issue #2547).
///
/// Volume is best-effort per device: any failure is logged (Serilog) and skipped, and
/// never fails the profile switch. The raw percent preset converts to a 0-1 scalar only
/// at this COM boundary, keeping the configuration human-readable (ADR Decision 5).
/// </summary>
internal sealed class ProfileVolumeApplier
{
    private readonly AudioSwitcher _audioSwitcher;
    private readonly ILogger _logger;

    /// <summary>
    /// Test seam: when set, replaces the COM device write. Production leaves it null and
    /// writes through <see cref="AudioSwitcher"/> on its COM thread.
    /// </summary>
    internal Func<string, float, bool>? DeviceVolumeWrite { private get; init; }

    /// <summary>
    /// Test seam: when set, replaces the COM device read used by
    /// <see cref="SnapshotVolumes"/>. Production leaves it null.
    /// </summary>
    internal Func<string, float?>? DeviceVolumeRead { private get; init; }

    public ProfileVolumeApplier(AudioSwitcher audioSwitcher, ILogger? logger = null)
    {
        _audioSwitcher = audioSwitcher;
        _logger = logger ?? Log.ForContext<ProfileVolumeApplier>();
    }

    /// <summary>
    /// Whether the profile carries a preset that would be applied to a matching device
    /// (a null or disabled preset is skipped everywhere, so it never needs a snapshot
    /// or a telemetry count).
    /// </summary>
    public static bool HasActivePreset(Profile profile) =>
        profile.PlaybackVolume is { Enabled: true } || profile.RecordingVolume is { Enabled: true };

    /// <summary>
    /// The devices whose volumes a trigger activation may override and deactivation must
    /// restore (ADR Q3): the pre-switch default endpoints (the switch moves them away)
    /// plus the profile's resolved preset targets (the presets write them — they are not
    /// necessarily the defaults). One entry per endpoint id: the same device can sit in
    /// both sets.
    /// </summary>
    public static IEnumerable<DeviceInfo> DevicesToSnapshot(IEnumerable<DeviceInfo?> defaultEndpoints, IEnumerable<DeviceInfo> presetTargets) =>
        defaultEndpoints.Concat<DeviceInfo?>(presetTargets)
            .OfType<DeviceInfo>()
            .GroupBy(device => device.Id, StringComparer.Ordinal)
            .Select(group => group.First());

    /// <summary>
    /// Applies the preset matching the device's flow (render → playback, capture →
    /// recording, ADR Decision 1). Returns whether the volume was actually written;
    /// a null or disabled preset is a plain no-op.
    /// </summary>
    public bool Apply(Profile profile, DeviceInfo device)
    {
        var preset = device.Type switch
        {
            EDataFlow.eRender => profile.PlaybackVolume,
            EDataFlow.eCapture => profile.RecordingVolume,
            _ => null
        };
        if (preset is not { Enabled: true })
        {
            return false;
        }

        return SetDeviceScalar(device, preset.Volume / 100f, $"applying the preset of profile '{profile.Name}'");
    }

    /// <summary>
    /// Writes a snapshotted 0-1 scalar back onto a device when a trigger deactivates
    /// (ADR Q3). No preset is involved: the value is the pre-trigger volume as read by
    /// <see cref="SnapshotVolumes"/>.
    /// </summary>
    public bool Restore(DeviceInfo device, float volumeScalar) =>
        SetDeviceScalar(device, volumeScalar, "restoring the pre-trigger volume");

    /// <summary>
    /// Reads each device's current master volume scalar (0-1) for a later
    /// <see cref="Restore"/> when a trigger deactivates (ADR Q3). Null entries in the
    /// input (no default device in that slot) are ignored; an unresolvable or
    /// unreadable device is logged and skipped — the snapshot stays best-effort, like
    /// the apply.
    /// </summary>
    public (DeviceInfo Device, float VolumeScalar)[] SnapshotVolumes(IEnumerable<DeviceInfo?> devices)
    {
        var snapshots = new List<(DeviceInfo Device, float VolumeScalar)>();
        foreach (var device in devices)
        {
            if (device == null)
            {
                continue;
            }

            try
            {
                float? scalar;
                if (DeviceVolumeRead != null)
                {
                    scalar = DeviceVolumeRead(device.Id);
                }
                else
                {
                    using var audioDevice = _audioSwitcher.GetDevice(device.Id);
                    if (audioDevice == null)
                    {
                        _logger.Warning("Device {Device} couldn't be resolved while snapshotting the pre-trigger volumes; skipped", device.NameClean);
                        continue;
                    }

                    scalar = _audioSwitcher.InteractWithDevice(audioDevice, d => d.EndpointVolume?.MasterVolumeLevelScalar);
                }

                if (scalar.HasValue)
                {
                    snapshots.Add((device, scalar.Value));
                }
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Couldn't read the master volume of {Device} while snapshotting the pre-trigger volumes; skipped", device.NameClean);
            }
        }

        return snapshots.ToArray();
    }

    private bool SetDeviceScalar(DeviceInfo device, float scalar, string action)
    {
        try
        {
            if (DeviceVolumeWrite != null)
            {
                return DeviceVolumeWrite(device.Id, scalar);
            }

            using var audioDevice = _audioSwitcher.GetDevice(device.Id);
            if (audioDevice == null)
            {
                _logger.Warning("Device {Device} couldn't be resolved while {Action}; volume skipped", device.NameClean, action);
                return false;
            }

            return _audioSwitcher.InteractWithDevice(audioDevice, d =>
            {
                var endpointVolume = d.EndpointVolume;
                if (endpointVolume == null)
                {
                    _logger.Warning("Device {Device} has no endpoint volume while {Action}; volume skipped", device.NameClean, action);
                    return false;
                }

                endpointVolume.MasterVolumeLevelScalar = scalar;
                return true;
            });
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Couldn't set the master volume of {Device} while {Action}; the profile switch itself is unaffected", device.NameClean, action);
            return false;
        }
    }
}
