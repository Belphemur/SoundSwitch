#nullable enable
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Framework.Profile
{
    public class Profile : IEquatable<Profile>
    {
        internal class DeviceRoleWrapper
        {
            public DeviceInfo DeviceInfo { get; }
            public ERole      Role       { get; }

            internal DeviceRoleWrapper(DeviceInfo deviceInfo, ERole role)
            {
                DeviceInfo = deviceInfo;
                Role       = role;
            }
        }

        public DeviceInfo? Playback      { get; set; }
        public DeviceInfo? Communication { get; set; }
        public DeviceInfo? Recording     { get; set; }

        public string               Name     { get; set; } = "";
        public IList<Trigger.Trigger> Triggers { get; set; } = new List<Trigger.Trigger>();

        public bool AlsoSwitchDefaultDevice { get; set; } = true;

        [JsonIgnore]
        internal IEnumerable<DeviceRoleWrapper> Devices
        {
            get
            {
                if (Playback != null)
                    yield return new DeviceRoleWrapper(Playback, Communication == null ? ERole.ERole_enum_count : ERole.eConsole);
                if (Communication != null)
                    yield return new DeviceRoleWrapper(Communication, ERole.eCommunications);
                if (Recording != null)
                    yield return new DeviceRoleWrapper(Recording, ERole.ERole_enum_count);
            }
        }

        public bool Equals(Profile? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Profile) obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}