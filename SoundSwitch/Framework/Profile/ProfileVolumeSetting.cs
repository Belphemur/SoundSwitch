#nullable enable

namespace SoundSwitch.Framework.Profile;

/// <summary>
/// Optional volume preset carried by a <see cref="Profile"/> for one flow (playback or
/// recording). The value is the raw user-facing percent (0-100); it is converted to a
/// 0-1 master-volume scalar only at the audio boundary, keeping the configuration
/// human-readable and the slider binding trivial.
/// </summary>
public sealed class ProfileVolumeSetting
{
    /// <summary>
    /// Whether the preset is applied when the profile is activated. The owning
    /// <see cref="Profile"/> field being null (absent in JSON) also means "off" —
    /// the explicit flag keeps a disabled preset without losing its value.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>Target master volume in percent (0-100).</summary>
    public float Volume { get; set; }

    /// <summary>Deep copy, so a copied profile never shares its preset with the source.</summary>
    public ProfileVolumeSetting Copy() => new()
    {
        Enabled = Enabled,
        Volume = Volume
    };
}
