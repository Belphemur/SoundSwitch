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

namespace SoundSwitch.Framework.Profile;

public class Profile : IEquatable<Profile>, IDisposable
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

    public bool SwitchForegroundApp { get; set; } = false;

    public bool NotifyOnActivation { get; set; } = true;

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

            return AudioSwitcher.Instance.InteractWithDevice(device, mmDevice => AudioDeviceIconExtractor.ExtractIconFromPath(mmDevice.IconPath, mmDevice.DataFlow, true));
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
            NotifyOnActivation = NotifyOnActivation,
            SwitchForegroundApp = SwitchForegroundApp,
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
                yield return new DeviceRoleWrapper(Playback, ERole.eConsole | ERole.eMultimedia);
            if (Communication != null)
                yield return new DeviceRoleWrapper(Communication, ERole.eCommunications);
            if (Recording != null)
                yield return new DeviceRoleWrapper(Recording, ERole.eConsole | ERole.eMultimedia);
            if (RecordingCommunication != null)
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
        if (obj.GetType() != GetType()) return false;
        return Equals((Profile)obj);
    }

    public override string ToString()
    {
        return $"{nameof(Name)}: {Name}, {nameof(Devices)}: [{string.Join(", ", Devices)}]";
    }

    public void Dispose()
    {
        foreach (var device in Devices.Select(wrapper => wrapper.DeviceInfo).Where(info => info is DeviceFullInfo).Cast<DeviceFullInfo>())
        {
            device.Dispose();
        }
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}