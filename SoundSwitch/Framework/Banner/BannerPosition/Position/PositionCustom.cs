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
using System.Drawing;
using System.Windows.Forms;

using SoundSwitch.Localization;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.Banner.BannerPosition.Position;

internal class PositionCustom : APosition, IPosition
{
    public BannerPosition TypeEnum => BannerPosition.Custom;
    public string Label => SettingsStrings.buttonCustom;

    public Point GetScreenPosition(Screen screen, int height, int width, int offset) =>
        ResolvePosition(screen, AppModel.Instance.CustomBannerPosition, AppModel.Instance.HasCustomBannerPosition, height, width);

    public Point GetPosition(Rectangle bounds, int height, int width, int offset) =>
        ResolvePosition(bounds, AppModel.Instance.CustomBannerPosition, AppModel.Instance.HasCustomBannerPosition, height, width);

    /// <summary>
    /// Resolve the saved custom position against the display the banner actually belongs to.
    /// </summary>
    /// <remarks>
    /// A custom position is expressed in desktop coordinates, so it belongs to one display. An
    /// explicit monitor selection moves it onto that display, the same as every other position
    /// strategy; otherwise the saved point keeps the banner on the display the user dropped it on.
    /// </remarks>
    internal static Point ResolvePosition(Screen screen, Point savedPosition, bool hasSavedPosition, int height, int width)
    {
        if (!hasSavedPosition)
            return new Point(PositionCenterX(screen.Bounds, width), PositionCenterY(screen.Bounds, height));

        return ResolvePosition(TargetBounds(screen, savedPosition), savedPosition, true, height, width);
    }

    /// <summary>
    /// Places the banner inside <paramref name="bounds"/>.
    /// </summary>
    /// <remarks>
    /// With no position saved yet the banner is centred. Otherwise it is clamped rather than
    /// re-centred, which keeps a position the user deliberately chose: only a display that went
    /// away or a banner that grew past the edge nudges it back.
    /// <para>
    /// The saved point must not double as the "never placed" marker: (0,0) is the default value
    /// <em>and</em> the top-left corner of the primary display, which a drag can produce.
    /// </para>
    /// </remarks>
    internal static Point ResolvePosition(Rectangle bounds, Point savedPosition, bool hasSavedPosition, int height, int width) =>
        !hasSavedPosition
            ? new Point(PositionCenterX(bounds, width), PositionCenterY(bounds, height))
            : new Point(
                Math.Clamp(savedPosition.X, bounds.Left, Math.Max(bounds.Left, bounds.Right - width)),
                Math.Clamp(savedPosition.Y, bounds.Top, Math.Max(bounds.Top, bounds.Bottom - height)));

    /// <summary>
    /// Bounds the saved position has to stay inside.
    /// </summary>
    /// <remarks>
    /// An explicit choice from the monitor selector - a pinned display or the primary screen -
    /// wins, so a custom position follows the same screen as every other position strategy.
    /// Only in "follow the cursor" mode does the saved point decide, which is what keeps a banner
    /// dragged onto a secondary display in place while the cursor is elsewhere.
    /// <see cref="Screen.FromPoint(Point)"/> falls back to the primary screen when the point is on
    /// no connected display, which keeps a stale position off any other monitor.
    /// </remarks>
    private static Rectangle TargetBounds(Screen screen, Point savedPosition) =>
        IsScreenExplicitlySelected()
            ? screen.Bounds
            : Screen.FromPoint(savedPosition).Bounds;

    private static bool IsScreenExplicitlySelected() =>
        !string.IsNullOrEmpty(AppModel.Instance.BannerMonitorDeviceName) ||
        AppModel.Instance.NotifyUsingPrimaryScreen;
}
