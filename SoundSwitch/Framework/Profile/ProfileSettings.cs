using System;
using SoundSwitch.Framework.Audio.Device;

namespace SoundSwitch.Framework.Profile
{
#nullable enable
    public class ProfileSettings : IEquatable<ProfileSettings>
    {
        public string ProfileName { get; set; }
        public string? ApplicationPath { get; set; }
        public HotKeys? HotKeys { get; set; }
        public DeviceInfo? Playback { get; set; }
        public DeviceInfo? Recording { get; set; }

        public bool Equals(ProfileSettings? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ProfileName == other.ProfileName;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProfileSettings) obj);
        }

        public override int GetHashCode()
        {
            return ProfileName.GetHashCode();
        }

        public static bool operator ==(ProfileSettings? left, ProfileSettings? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProfileSettings? left, ProfileSettings? right)
        {
            return !Equals(left, right);
        }
    }
}