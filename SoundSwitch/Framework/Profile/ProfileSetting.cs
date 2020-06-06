using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SoundSwitch.Common.Framework.Audio.Device;
using HotKey = SoundSwitch.Framework.WinApi.Keyboard.HotKey;

namespace SoundSwitch.Framework.Profile
{
#nullable enable
    public class ProfileSetting : IEquatable<ProfileSetting>
    {
        public string ProfileName { get; set; } = "";
        public string? ApplicationPath { get; set; }
        public HotKey? HotKey { get; set; }
        public DeviceInfo? Playback { get; set; }
        public DeviceInfo? Recording { get; set; }
        public bool AlsoSwitchDefaultDevice { get; set; } = true;

        [JsonIgnore]
        public IEnumerable<DeviceInfo> Devices => new[] {Playback, Recording}.Where(info => info != null)!;

        /// <summary>
        /// Clone the current profile
        /// </summary>
        public ProfileSetting Clone() => new ProfileSetting
        {
            ProfileName             = ProfileName,
            ApplicationPath         = ApplicationPath,
            Playback                = Playback,
            Recording               = Recording,
            AlsoSwitchDefaultDevice = AlsoSwitchDefaultDevice
        };

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
            return $"{nameof(ProfileName)}: {ProfileName}, {nameof(ApplicationPath)}: {ApplicationPath}, {nameof(HotKey)}: {HotKey}, {nameof(Playback)}: {Playback}, {nameof(Recording)}: {Recording}";
        }
    }
}