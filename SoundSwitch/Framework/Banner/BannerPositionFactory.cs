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

using SoundSwitch.Framework.Banner.Position;
using SoundSwitch.Framework.Factory;

namespace SoundSwitch.Framework.Banner
{
    public class BannerPositionFactory : AbstractFactory<BannerPositionEnum, IPosition>
    {
        private static readonly IEnumImplList<BannerPositionEnum, IPosition> Positions = new EnumImplList
            <BannerPositionEnum, IPosition>
            {
                new PositionTopLeft(),
                new PositionTopCenter(),
                new PositionTopRight(),
                new PositionBottomLeft(),
                new PositionBottomCenter(),
                new PositionBottomRight(),
                new PositionCenter()
            };

        public BannerPositionFactory() : base(Positions)
        {
        }
    }
}
