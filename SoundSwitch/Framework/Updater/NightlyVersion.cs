#if NIGHTLY
using System;

namespace SoundSwitch.Framework.Updater;

/// <summary>
/// A four-part nightly version (major.minor.build.revision) where all four numeric
/// parts participate in ordering and comparison. Unlike the stable/beta channel, the
/// nightly feed uses a full revision component that must be compared directly.
/// </summary>
public readonly struct NightlyVersion : IComparable<NightlyVersion>, IEquatable<NightlyVersion>
{
    public int Major { get; }
    public int Minor { get; }
    public int Build { get; }
    public int Revision { get; }

    public NightlyVersion(int major, int minor, int build, int revision)
    {
        Major = major;
        Minor = minor;
        Build = build;
        Revision = revision;
    }

    public static bool TryParse(string value, out NightlyVersion version)
    {
        version = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var parts = value.Split('.');
        if (parts.Length != 4)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out var major) ||
            !int.TryParse(parts[1], out var minor) ||
            !int.TryParse(parts[2], out var build) ||
            !int.TryParse(parts[3], out var revision))
        {
            return false;
        }

        version = new NightlyVersion(major, minor, build, revision);
        return true;
    }

    public static NightlyVersion Parse(string value)
    {
        if (!TryParse(value, out var version))
        {
            throw new FormatException($"Unable to parse nightly version '{value}'.");
        }

        return version;
    }

    public int CompareTo(NightlyVersion other)
    {
        var majorComparison = Major.CompareTo(other.Major);
        if (majorComparison != 0)
        {
            return majorComparison;
        }

        var minorComparison = Minor.CompareTo(other.Minor);
        if (minorComparison != 0)
        {
            return minorComparison;
        }

        var buildComparison = Build.CompareTo(other.Build);
        if (buildComparison != 0)
        {
            return buildComparison;
        }

        return Revision.CompareTo(other.Revision);
    }

    public bool Equals(NightlyVersion other) => CompareTo(other) == 0;

    public override bool Equals(object obj) => obj is NightlyVersion other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Major, Minor, Build, Revision);

    public override string ToString() => $"{Major}.{Minor}.{Build}.{Revision}";

    public static bool operator >(NightlyVersion left, NightlyVersion right) => left.CompareTo(right) > 0;
    public static bool operator <(NightlyVersion left, NightlyVersion right) => left.CompareTo(right) < 0;
    public static bool operator >=(NightlyVersion left, NightlyVersion right) => left.CompareTo(right) >= 0;
    public static bool operator <=(NightlyVersion left, NightlyVersion right) => left.CompareTo(right) <= 0;
    public static bool operator ==(NightlyVersion left, NightlyVersion right) => left.Equals(right);
    public static bool operator !=(NightlyVersion left, NightlyVersion right) => !left.Equals(right);
}
#endif
