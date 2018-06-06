using System;

namespace SoundSwitch.Framework.Configuration.Device
{
    public class DeviceInfo : IComparable<DeviceInfo>, IEquatable<DeviceInfo>
    {
        public string Name { get; }
        public string Id { get; }

        public DeviceInfo(string name, string id)
        {
            Name = name;
            Id = id;
        }

        public int CompareTo(DeviceInfo other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var nameComparison = string.Compare(Name, other.Name, StringComparison.Ordinal);
            if (nameComparison != 0) return nameComparison;
            return string.Compare(Id, other.Id, StringComparison.Ordinal);
        }

        public bool Equals(DeviceInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) || string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DeviceInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Id != null ? Id.GetHashCode() : 0);
            }
        }

        public static bool operator ==(DeviceInfo left, DeviceInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DeviceInfo left, DeviceInfo right)
        {
            return !Equals(left, right);
        }
    }
}