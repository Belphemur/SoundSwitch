#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.WinApi.Keyboard;

namespace SoundSwitch.Model
{
    public class AppSoundRule : IEquatable<AppSoundRule>
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public bool CaseSensitive { get; set; } = false;
        public string ProcessPath { get; set; } = string.Empty;
        public string WindowName { get; set; } = string.Empty;

        /// <summary>
        /// How the rule is applied: automatically on process/foreground match, or
        /// manually through <see cref="HotKey"/>.
        /// </summary>
        public AppRuleMode Mode { get; set; } = AppRuleMode.Automatic;

        /// <summary>
        /// Ordered cycle steps. <see cref="AppRuleMode.Automatic"/> uses the first
        /// pair; <see cref="AppRuleMode.Hotkey"/> cycles through all of them.
        /// </summary>
        public List<AppSoundRuleDevicePair> DevicePairs { get; set; } = new();

        /// <summary>
        /// Optional global hotkey that applies the next device pair to the matching
        /// running process(es). Only meaningful in <see cref="AppRuleMode.Hotkey"/>.
        /// </summary>
        public HotKey? HotKey { get; set; }

        [Obsolete("Migrated to " + nameof(DevicePairs))]
        public DeviceInfo? PlaybackDevice { get; set; }

        [Obsolete("Migrated to " + nameof(DevicePairs))]
        public DeviceInfo? RecordingDevice { get; set; }

        [Obsolete("Migrated to " + nameof(DevicePairs))]
        public string? PlaybackDeviceId { get; set; }

        [Obsolete("Migrated to " + nameof(DevicePairs))]
        public string? RecordingDeviceId { get; set; }

        public bool Enabled { get; set; } = true;
        public bool Notify { get; set; } = true;

        public AppSoundRule Copy()
        {
            return new AppSoundRule
            {
                Id = Id,
                CaseSensitive = CaseSensitive,
                ProcessPath = ProcessPath,
                WindowName = WindowName,
                Mode = Mode,
                DevicePairs = DevicePairs?.Where(pair => pair != null).Select(pair => pair.Copy()).ToList() ?? new List<AppSoundRuleDevicePair>(),
                HotKey = HotKey,
                Enabled = Enabled,
                Notify = Notify
            };
        }

        public bool Equals(AppSoundRule? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Id.Equals(other.Id);
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((AppSoundRule)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(AppSoundRule? left, AppSoundRule? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AppSoundRule? left, AppSoundRule? right)
        {
            return !Equals(left, right);
        }
    }
}
