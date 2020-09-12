using System;
using System.Text.RegularExpressions;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;

namespace SoundSwitch.Common.Framework.Audio.Device
{
    public class DeviceInfo : IEquatable<DeviceInfo>, IComparable<DeviceInfo>
    {
        private static readonly Regex NameSplitterRegex = new Regex(@"(?<friendlyName>[\w\s-_\.\/\\]+)\s\([\d\s\-|]*(?<deviceName>.+)\)", RegexOptions.Compiled);

        private static readonly Regex NameCleanerRegex = new Regex(@"^\d+\s?-\s?", RegexOptions.Compiled | RegexOptions.Singleline);

        private string   _nameClean;
        [Obsolete("Use " + nameof(NameClean))]
        public  string   Name { get; }
        public  string   Id       { get; }
        public  DataFlow Type     { get; }

        public string NameClean
        {
            get
            {
                if (_nameClean != null)
                {
                    return _nameClean;
                }

                var match = NameSplitterRegex.Match(Name);
                //Old naming, can't do anything about this
                if (!match.Success)
                {
                    return _nameClean = Name;
                }
                var friendlyName = match.Groups["friendlyName"].Value;
                var deviceName = match.Groups["deviceName"].Value;
                return _nameClean = $"{NameCleanerRegex.Replace(friendlyName, "")} ({deviceName})";
            }
        }

        [JsonConstructor]
        public DeviceInfo(string name, string id, DataFlow type)
        {
            Name = name;
            Id       = id;
            Type     = type;
        }

        public DeviceInfo(MMDevice device)
        {
            Name = device.FriendlyName;
            Id       = device.ID;
            Type     = device.DataFlow;
        }


        public static bool operator ==(DeviceInfo left, DeviceInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DeviceInfo left, DeviceInfo right)
        {
            return !Equals(left, right);
        }


        public override string ToString()
        {
            return NameClean;
        }

        public int CompareTo(DeviceInfo other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var nameComparison = string.Compare(NameClean, other.NameClean, StringComparison.Ordinal);
            if (nameComparison != 0) return nameComparison;
            var idComparison = string.Compare(Id, other.Id, StringComparison.Ordinal);
            if (idComparison != 0) return idComparison;
            return Type.CompareTo(other.Type);
        }

        public bool Equals(DeviceInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return (Id == other.Id || NameClean == other.NameClean) && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is DeviceInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = NameClean != null ? NameClean.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Type;
                return hashCode;
            }
        }
    }
}