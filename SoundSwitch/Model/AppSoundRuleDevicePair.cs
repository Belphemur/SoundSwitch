#nullable enable
using SoundSwitch.Common.Framework.Audio.Device;

namespace SoundSwitch.Model
{
    /// <summary>
    /// One step in an <see cref="AppSoundRule"/>'s device cycle.
    /// Either side may be null, meaning "keep the current device" for that side.
    /// </summary>
    public sealed class AppSoundRuleDevicePair
    {
        public DeviceInfo? Playback { get; set; }
        public DeviceInfo? Recording { get; set; }

        /// <summary>
        /// A pair must set at least one side to be usable.
        /// </summary>
        public bool HasAnyDevice => Playback != null || Recording != null;

        public AppSoundRuleDevicePair Copy() => new()
        {
            Playback = Playback,
            Recording = Recording
        };
    }
}
