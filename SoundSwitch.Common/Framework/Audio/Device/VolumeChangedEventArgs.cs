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
        /// The previous volume level (0-100)
        /// </summary>
        public int PreviousVolume { get; }

        /// <summary>
        /// Whether the device is muted
        /// </summary>
        public bool IsMuted { get; }

        /// <summary>
        /// Whether the device was previously muted
        /// </summary>
        public bool WasMuted { get; }

        /// <summary>
        /// Whether the volume has changed
        /// </summary>
        public bool VolumeChanged => Volume != PreviousVolume;

        /// <summary>
        /// Whether the mute state has changed
        /// </summary>
        public bool MuteChanged => IsMuted != WasMuted;

        /// <summary>
        /// Initializes a new instance of the VolumeChangedEventArgs class
        /// </summary>
        /// <param name="volume">Current volume level (0-100)</param>
        /// <param name="previousVolume">Previous volume level (0-100)</param>
        /// <param name="isMuted">Whether the device is muted</param>
        /// <param name="wasMuted">Whether the device was previously muted</param>
        public VolumeChangedEventArgs(int volume, int previousVolume, bool isMuted, bool wasMuted)
        {
            Volume = volume;
            PreviousVolume = previousVolume;
            IsMuted = isMuted;
            WasMuted = wasMuted;
        }
    }
}