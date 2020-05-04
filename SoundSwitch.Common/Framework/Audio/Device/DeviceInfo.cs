using System;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;

namespace SoundSwitch.Common.Framework.Audio.Device
{
    public class DeviceInfo : IEquatable<DeviceInfo>, IComparable<DeviceInfo>
    {
        public string Name { get;  }
        public string Id { get; }
        public DataFlow Type { get; }

        [JsonConstructor]
        public DeviceInfo(string name, string id, DataFlow type)
        {
            Name = name;
            Id = id;
            Type = type;
        }

        public DeviceInfo(MMDevice device)
        {
            Name = device.FriendlyName;
            Id = device.ID;
            Type = device.DataFlow;
        }

        public bool Equals(DeviceInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return (Id == other.Id || Name == other.Name) && Type == other.Type;
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
                return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (int) Type;
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


        public int CompareTo(DeviceInfo other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var idComparison = string.Compare(Id, other.Id, StringComparison.Ordinal);
            if (idComparison != 0) return idComparison;
            return Type.CompareTo(other.Type);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}