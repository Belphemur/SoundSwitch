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

using System.Drawing;
using System.Windows.Forms;

namespace SoundSwitch.Framework.Banner.BannerPosition.Position;

/// <summary>
/// Anchor points used by the banner position strategies.
/// <para>
/// Every helper resolves against the edges of the given bounds rather than against the screen
/// size, so a display that does not start at the desktop origin (Left/Top negative or non-zero)
/// places the banner inside that display instead of offset from it.
/// </para>
/// <para>
/// The <see cref="Screen"/> overloads delegate to the <see cref="Rectangle"/> ones so the geometry
/// can be exercised with an arbitrary bounds rectangle, without a second physical display.
/// </para>
/// </summary>
internal class APosition
{
    public static int PositionTop(Screen screen, int offset) => PositionTop(screen.Bounds, offset);
    public static int PositionTop(Rectangle bounds, int offset) => bounds.Top + 60 + offset;

    public static int PositionLeft(Screen screen) => PositionLeft(screen.Bounds);
    public static int PositionLeft(Rectangle bounds) => bounds.Left + 50;

    public static int PositionBottom(Screen screen, int height, int offset) => PositionBottom(screen.Bounds, height, offset);
    public static int PositionBottom(Rectangle bounds, int height, int offset) => bounds.Bottom - height - 60 - offset;

    public static int PositionRight(Screen screen, int width) => PositionRight(screen.Bounds, width);
    public static int PositionRight(Rectangle bounds, int width) => bounds.Right - width - 50;

    public static int PositionCenterX(Screen screen, int width) => PositionCenterX(screen.Bounds, width);
    public static int PositionCenterX(Rectangle bounds, int width) => bounds.Left + (bounds.Width - width) / 2;

    public static int PositionCenterY(Screen screen, int height) => PositionCenterY(screen.Bounds, height);
    public static int PositionCenterY(Rectangle bounds, int height) => bounds.Top + (bounds.Height - height) / 2;
}
