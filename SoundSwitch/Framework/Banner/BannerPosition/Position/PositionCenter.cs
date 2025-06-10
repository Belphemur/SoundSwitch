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
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.Banner.BannerPosition.Position;

internal class PositionCenter : APosition, IPosition
{
    public BannerPositionEnum TypeEnum => BannerPositionEnum.Center;
    public string Label => SettingsStrings.position_option_center;

    public Point GetScreenPosition(Screen screen, int height, int width, int offset)
    {
        return new Point(
            PositionCenterH(screen, width),
            PositionCenterV(screen, height)
        );
    }
}