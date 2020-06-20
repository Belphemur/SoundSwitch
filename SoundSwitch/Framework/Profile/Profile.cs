#nullable enable
using System.Collections.Generic;
using Newtonsoft.Json;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Framework.Profile
{
    public class Profile
    {
        internal class DeviceWrapper
        {
            public DeviceInfo DeviceInfo { get; }
            public ERole      Role       { get; }

            internal DeviceWrapper(DeviceInfo deviceInfo, ERole role)
            {
                DeviceInfo = deviceInfo;
                Role       = role;
            }
        }

        public DeviceInfo? Playback      { get; set; }
        public DeviceInfo? Communication { get; set; }
        public DeviceInfo? Recording     { get; set; }

        public string               Name     { get; set; } = "";
        public IEnumerable<Trigger.Trigger> Triggers { get; set; } = new List<Trigger.Trigger>();

        public bool AlsoSwitchDefaultDevice { get; set; } = true;

        [JsonIgnore]
        internal IEnumerable<DeviceWrapper> Devices
        {
            get
            {
                if (Playback != null)
                    yield return new DeviceWrapper(Playback, Communication == null ? ERole.ERole_enum_count : ERole.eConsole);
                if (Communication != null)
                    yield return new DeviceWrapper(Communication, ERole.eCommunications);
                if (Recording != null)
                    yield return new DeviceWrapper(Recording, ERole.ERole_enum_count);
            }
        }
    }
}