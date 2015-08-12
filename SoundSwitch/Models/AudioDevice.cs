//Source: https://github.com/danielsunnerberg/AudioDevice-Quickswitcher
using System.Xml.Serialization;

namespace SoundSwitch.Models
{
    /// <summary>
    /// Represents an audio device in Microsoft Windows 7+.
    /// Since the class will usually be created through a serializer the class cannot be immmutable.
    /// </summary>
    public class AudioDevice
    {
        
        /// <summary>
        /// Windows internal index of the device.
        /// Is likely to change when disconnected.
        /// </summary>
        [XmlElement("index")]
        public int Index { get; set; }

        [XmlElement("friendlyName")]
        public string FriendlyName { get; set; }

        [XmlElement("state")]
        public int State { get; set; }

        /// <summary>
        /// Flag for whether the device is the system default or not.
        /// </summary>
        [XmlElement("default")]
        public bool IsDefault { get; set; }

        [XmlElement("description")]
        public string Description { get; set; }

        [XmlElement("interfaceFriendlyName")]
        public string InterfaceFriendlyName { get; set; }

        [XmlElement("deviceId")]
        public string DeviceId { get; set; }

        protected bool Equals(AudioDevice other)
        {
            return string.Equals(DeviceId, other.DeviceId);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AudioDevice) obj);
        }

        public override int GetHashCode()
        {
            return (DeviceId != null ? DeviceId.GetHashCode() : 0);
        }

    }
}
