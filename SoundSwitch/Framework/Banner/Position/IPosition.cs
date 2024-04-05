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
using SoundSwitch.Framework.Factory;

namespace SoundSwitch.Framework.Banner.Position
{
    public interface IPosition : IEnumImpl<BannerPositionEnum>
    {
        /// <summary>
        /// Get position in the screen for the banner
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="height"></param>
        /// <param name="width"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public Point GetScreenPosition(Screen screen, int height, int width, int offset);
    }
}