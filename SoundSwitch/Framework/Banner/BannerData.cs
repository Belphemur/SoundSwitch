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
using SoundSwitch.Framework.Banner.BannerPosition.Position;

// Alias required: inside namespace SoundSwitch.Framework.Banner the simple name
// BannerDisplayInfo resolves to the nested namespace, shadowing the enum.
using BannerDisplayInfoEnum = SoundSwitch.Framework.Banner.BannerDisplayInfo.BannerDisplayInfo;

namespace SoundSwitch.Framework.Banner;

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

    /// <summary>
    /// Opacity of the banner
    /// </summary>
    public int Opacity { get; internal set; } = 100;

    /// <summary>
    /// Which elements of the banner to display: icon, text, or both
    /// </summary>
    public BannerDisplayInfoEnum DisplayInfo { get; internal set; } = BannerDisplayInfoEnum.FullDisplay;

    /// <summary>
    /// The display mode that can actually be rendered for this banner.
    /// An icon-only banner without an image would be empty, so it falls back to full display.
    /// </summary>
    public BannerDisplayInfoEnum EffectiveDisplayInfo =>
        DisplayInfo == BannerDisplayInfoEnum.IconOnly && Image == null
            ? BannerDisplayInfoEnum.FullDisplay
            : DisplayInfo;

    /// <summary>
    /// When enabled, displays the banner in compact mode (half the normal size)
    /// </summary>
    public bool CompactMode { get; internal set; }

    /// <summary>
    /// When enabled. sets the banner in custom position mode
    /// </summary>
    public bool CustomPositionMode { get; internal set; }
        
    /// <summary>
    /// Callback that is triggered when the banner is clicked
    /// </summary>
    [AllowNull]
    public EventHandler OnClick { get; internal set; }

    /// <summary>
    /// Key used to deduplicate concurrently shown banners. Banners sharing the
    /// same key (same device + same title) update the existing banner instead of
    /// stacking a new one. This is what makes the "Only one banner" setting hold
    /// when several notifications for the same device are raised in quick succession
    /// (e.g. a default-device switch with "Switch default communication device" on).
    /// </summary>
    public string DedupKey => $"{CurrentDeviceId ?? string.Empty}|{Title ?? string.Empty}";
}
