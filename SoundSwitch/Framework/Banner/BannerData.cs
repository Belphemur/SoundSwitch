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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Banner.Position;

namespace SoundSwitch.Framework.Banner
{
    /// <summary>
    /// Contains configuration data for the banner form.
    /// </summary>
    public class BannerData
    {
        /// <summary>
        /// Gets/sets the title of the banner
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Gets/sets the text of the banner
        /// </summary>
        public string Text { get; internal set; }

        /// <summary>
        /// Gets/sets the path for an image, this is optional.
        /// </summary>
        public Image Image { get; internal set; }

        /// <summary>
        /// Gets/sets the path for a wav sound to be playedc during the notification, this is optional.
        /// </summary>
        [AllowNull]
        public CachedSound SoundFile { get; internal set; }

        /// <summary>
        /// On what device to play the <see cref="CachedSound"/>
        /// </summary>
        [AllowNull]
        public string CurrentDeviceId { get; internal set; }

        /// <summary>
        /// Position of the banner
        /// </summary>
        public IPosition Position { get; internal set; }

        /// <summary>
        /// Set the priority of the notification
        /// If a notification is being shown a higher priority comes, it will replace it, if a lower, nothing will happens.
        /// </summary>
        public int Priority { get; set; } = -1;
        
        /// <summary>
        /// How long to keep the banner on the screen
        /// </summary>
        public TimeSpan Ttl { get; internal set; }
    }
}