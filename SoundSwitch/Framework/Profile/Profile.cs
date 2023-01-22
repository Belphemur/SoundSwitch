#nullable enable
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Common.Framework.Audio.Icon;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.Profile
{
    public class Profile : IEquatable<Profile>
    {
        private bool? _restoreDevices = null;

        internal class DeviceRoleWrapper
        {
            public DeviceInfo DeviceInfo { get; }
            public ERole Role { get; }

            internal DeviceRoleWrapper(DeviceInfo deviceInfo, ERole role)
            {
                DeviceInfo = deviceInfo;
                Role = role;
            }
        }

        public DeviceInfo? Playback { get; set; }
        public DeviceInfo? Communication { get; set; }
        public DeviceInfo? Recording { get; set; }
        public DeviceInfo? RecordingCommunication { get; set; }
        public string Name { get; set; } = "";
        public IList<Trigger.Trigger> Triggers { get; set; } = new List<Trigger.Trigger>();

        public bool AlsoSwitchDefaultDevice { get; set; } = false;

        [JsonIgnore]
        public Icon Icon
        {
            get
            {
                var device = AudioSwitcher.Instance.GetDevice(Devices.First().DeviceInfo.Id);
                if (device == null)
                {
                    return Resources.profile;
                }

                return AudioSwitcher.Instance.InteractWithMmDevice(device, mmDevice => AudioDeviceIconExtractor.ExtractIconFromPath(mmDevice.IconPath, mmDevice.DataFlow, true));
            }
        }

        /// <summary>
        /// Should the device be restore when the profile is disabled
        /// </summary>
        public bool RestoreDevices
        {
            get => AlsoSwitchDefaultDevice && (_restoreDevices ?? AlsoSwitchDefaultDevice);
            set => _restoreDevices = value;
        }

        public bool NotifyOnActivation { get; set; } = true;

        /// <summary>
        /// Deep copy the profile
        /// </summary>
        public Profile Copy()
        {
            return new()
            {
                AlsoSwitchDefaultDevice = AlsoSwitchDefaultDevice,
                Communication = Communication,
                Name = Name,
                Playback = Playback,
                Recording = Recording,
                RecordingCommunication = RecordingCommunication,
                RestoreDevices = RestoreDevices,
                Triggers = Triggers.Select(trigger => new Trigger.Trigger(trigger.Type)
                                   {
                                       HotKey = trigger.HotKey,
                                       ApplicationPath = trigger.ApplicationPath,
                                       WindowName = trigger.WindowName,
                                       ShouldRestoreDevices = trigger.ShouldRestoreDevices
                                   })
                                   .ToList()
            };
        }

        [JsonIgnore]
        internal IEnumerable<DeviceRoleWrapper> Devices
        {
            get
            {
                if (Playback != null)
                    yield return new DeviceRoleWrapper(Playback, Communication == null || !Playback.Equals(Communication) ? ERole.ERole_enum_count : ERole.eConsole | ERole.eMultimedia);
                if (Communication != null && !Communication.Equals(Playback))
                    yield return new DeviceRoleWrapper(Communication, ERole.eCommunications);
                if (Recording != null)
                    yield return new DeviceRoleWrapper(Recording, RecordingCommunication == null || !Recording.Equals(RecordingCommunication) ? ERole.ERole_enum_count : ERole.eConsole | ERole.eMultimedia);
                if (RecordingCommunication != null && !RecordingCommunication.Equals(Recording))
                    yield return new DeviceRoleWrapper(RecordingCommunication, ERole.eCommunications);
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
            return Equals((Profile)obj);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}