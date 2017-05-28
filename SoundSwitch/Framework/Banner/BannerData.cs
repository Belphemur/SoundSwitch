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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public string SoundFilePath { get; internal set; }
    }
}
