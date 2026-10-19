#nullable enable
using System;
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
        public DeviceInfo? PlaybackDevice { get; set; }
        public DeviceInfo? RecordingDevice { get; set; }

        /// <summary>
        /// Optional global hotkey that re-applies this rule's devices to the matching
        /// running process(es) on demand. Only the app's audio is switched; the default
        /// device is never touched.
        /// </summary>
        public HotKey? HotKey { get; set; }

        [Obsolete("Migrated to " + nameof(PlaybackDevice))]
        public string? PlaybackDeviceId { get; set; }

        [Obsolete("Migrated to " + nameof(RecordingDevice))]
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
                PlaybackDevice = PlaybackDevice,
                RecordingDevice = RecordingDevice,
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
