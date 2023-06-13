using System;
using System.Text.RegularExpressions;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;

namespace SoundSwitch.Common.Framework.Audio.Device
{
    public class DeviceInfo : IEquatable<DeviceInfo>
    {
        private static readonly Regex NameSplitterRegex = new(@"(?<friendlyName>.+)\s\([\d\s\-|]*(?<deviceName>.+)\)", RegexOptions.Compiled);

        private static readonly Regex NameCleanerRegex = new(@"\s?\(\d\)|^\d+\s?-\s?", RegexOptions.Compiled | RegexOptions.Singleline);

        private string _nameClean;

        [Obsolete("Use " + nameof(NameClean))]
        public string Name { get; }

        public string Id { get; }
        public DataFlow Type { get; }

        public bool IsUsb { get; } = true;

        public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;

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

        public DeviceInfo(MMDevice device)
        {
            Name = device.FriendlyName;
            Id = device.ID;
            Type = device.DataFlow;
            var deviceProperties = device.Properties;
            var enumerator = deviceProperties.Contains(PropertyKeys.DEVPKEY_Device_EnumeratorName) ? (string)deviceProperties[PropertyKeys.DEVPKEY_Device_EnumeratorName].Value : "";
            IsUsb = enumerator == "USB";
        }

        [JsonConstructor]
        public DeviceInfo(string name, string id, DataFlow type, bool isUsb, DateTime discoveredAt)
        {
            Name = name;
            Id = id;
            Type = type;
            IsUsb = isUsb;
            DiscoveredAt = discoveredAt;
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
            return $"[{Type}]{NameClean}";
        }


        public bool Equals(DeviceInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (Type != other.Type) return false;
            //Same Id, it's the same device
            if (Id == other.Id) return true;
            //Always match on NameClean when Id is different
            //help recognizing the same device
            return NameClean == other.NameClean;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is not DeviceInfo info)
            {
                return false;
            }

            return Equals(info);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type, NameClean, Id);
        }
    }
}