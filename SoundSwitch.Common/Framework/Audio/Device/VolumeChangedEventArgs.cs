#nullable enable
using System;

namespace SoundSwitch.Common.Framework.Audio.Device
{
    /// <summary>
    /// Event arguments for device volume and mute state changes
    /// </summary>
    public class VolumeChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The current volume level (0-100)
        /// </summary>
        public int Volume { get; }

        /// <summary>
        /// Whether the device is muted
        /// </summary>
        public bool IsMuted { get; }

        /// <summary>
        /// Initializes a new instance of the VolumeChangedEventArgs class
        /// </summary>
        /// <param name="volume">Current volume level (0-100)</param>
        /// <param name="isMuted">Whether the device is muted</param>
        public VolumeChangedEventArgs(int volume, bool isMuted)
        {
            Volume = volume;
            IsMuted = isMuted;
        }
    }
}