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

using System.Windows.Forms;

namespace SoundSwitch.Framework.Banner.Position
{
    internal class APosition
    {
        public int PositionTop(Screen screen, int offset) => screen.Bounds.Y + 60 + offset;
        public int PositionLeft(Screen screen) => screen.Bounds.X + 50;
        public int PositionBottom(Screen screen, int height, int offset) => screen.Bounds.Height - height - PositionTop(screen, 0) - offset;
        public int PositionRight(Screen screen, int width) => screen.Bounds.Width - width - PositionLeft(screen);
        public int PositionCenterH(Screen screen, int width) => (screen.Bounds.Width - width) / 2;
        public int PositionCenterV(Screen screen, int height) => (screen.Bounds.Height - height) / 2;
    }
}
