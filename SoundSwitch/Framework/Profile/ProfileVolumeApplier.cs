#nullable enable

using System;

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
