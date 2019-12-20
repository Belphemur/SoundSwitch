using System;
using SoundSwitch.Common.WinApi.Keyboard;
using SoundSwitch.Framework.Audio.Device;

namespace SoundSwitch.Framework.Profile
{
#nullable enable
    public class ProfileSetting : IEquatable<ProfileSetting>
    {
        public string ProfileName { get; set; } = "";
        public string? ApplicationPath { get; set; }
        public HotKeys? HotKeys { get; set; }
        public DeviceInfo? Playback { get; set; }
        public DeviceInfo? Recording { get; set; }

        public bool Equals(ProfileSetting? other)
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
            return Equals((ProfileSetting) obj);
        }

        public override int GetHashCode()
        {
            return ProfileName.GetHashCode();
        }

        public static bool operator ==(ProfileSetting? left, ProfileSetting? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ProfileSetting? left, ProfileSetting? right)
        {
            return !Equals(left, right);
        }

        public override string ToString()
        {
            return $"{nameof(ProfileName)}: {ProfileName}, {nameof(ApplicationPath)}: {ApplicationPath}, {nameof(HotKeys)}: {HotKeys}, {nameof(Playback)}: {Playback}, {nameof(Recording)}: {Recording}";
        }
    }
}