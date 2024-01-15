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
        public int PositionTop(Screen screen) => screen.Bounds.X + 50;
        public int PositionLeft(Screen screen) => screen.Bounds.Y + 60;
        public int PositionBottom(Screen screen, int height) => screen.Bounds.Height - height - PositionTop(screen);
        public int PositionRight(Screen screen, int width) => screen.Bounds.Width - width - PositionLeft(screen);
        public int PositionCenter(Screen screen, int width) => (screen.Bounds.Width - width) / 2;
    }
}
