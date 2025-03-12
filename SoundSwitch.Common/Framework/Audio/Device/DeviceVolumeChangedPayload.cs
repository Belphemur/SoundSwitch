/********************************************************************
 * Copyright (C) 2015-2017 Antoine Aflalo
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 ********************************************************************/

using System;

namespace SoundSwitch.Common.Framework.Audio.Device
{
    /// <summary>
    /// Payload for device volume change events
    /// </summary>
    public class DeviceVolumeChangedPayload
    {
        /// <summary>
        /// The device that had its volume/mute state changed
        /// </summary>
        public DeviceFullInfo Device { get; }

        /// <summary>
        /// The current volume level (0-100)
        /// </summary>
        public int Volume { get; }

        /// <summary>
        /// The previous volume level (0-100)
        /// </summary>
        public int PreviousVolume { get; }

        /// <summary>
        /// Whether the device is currently muted
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
        /// When the change occurred
        /// </summary>
        public DateTimeOffset Timestamp { get; }

        /// <summary>
        /// Creates a new DeviceVolumeChangedPayload
        /// </summary>
        /// <param name="device">The device that changed</param>
        /// <param name="args">The volume change event arguments</param>
        public DeviceVolumeChangedPayload(DeviceFullInfo device, VolumeChangedEventArgs args)
        {
            Device = device;
            Volume = args.Volume;
            PreviousVolume = args.PreviousVolume;
            IsMuted = args.IsMuted;
            WasMuted = args.WasMuted;
            Timestamp = DateTimeOffset.Now;
        }
    }
}