﻿using System;
using System.Text.RegularExpressions;
using NAudio.CoreAudioApi;
using Newtonsoft.Json;
#pragma warning disable CS0618 // Type or member is obsolete

namespace SoundSwitch.Common.Framework.Audio.Device
{
    public partial class DeviceInfo : IEquatable<DeviceInfo>
    {
        private static readonly Regex NameSplitterRegex = NameSplitterRegexCompiled();

        private static readonly Regex NameCleanerRegex = NameCleanerRegexCompiled();

        private string _nameClean;

        [Obsolete("Use " + nameof(NameClean))]
        public string Name { get; }
        
        [JsonIgnore]
        public string FriendlyName { get; private set; }
        
        [JsonIgnore]
        public string DeviceName { get; private set; }

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
                    FriendlyName = Name;
                    DeviceName = string.Empty;
                    return _nameClean = Name;
                }

                FriendlyName = match.Groups["friendlyName"].Value;
                DeviceName = match.Groups["deviceName"].Value;
                return _nameClean = $"{NameCleanerRegex.Replace(FriendlyName, "")} ({DeviceName})";
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

        [GeneratedRegex(@"(?<friendlyName>.+)\s\([\d\s\-|]*(?<deviceName>.+)\)", RegexOptions.Compiled)]
        private static partial Regex NameSplitterRegexCompiled();
        [GeneratedRegex(@"\s?\(\d\)|^\d+\s?-\s?", RegexOptions.Compiled | RegexOptions.Singleline)]
        private static partial Regex NameCleanerRegexCompiled();
    }
}