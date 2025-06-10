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

using SoundSwitch.Framework.Factory;
using SoundSwitch.Framework.TrayIcon.IconChanger.Changer;

namespace SoundSwitch.Framework.TrayIcon.IconChanger
{
    public class IconChangerFactory() : AbstractFactory<IconChangerEnum, IIconChanger>(Impl)
    {
        private static readonly IEnumImplList<IconChangerEnum, IIconChanger> Impl = new EnumImplList<IconChangerEnum, IIconChanger>()
        {
            new NeverIconIconChanger(),
            new PlaybackIconChanger(),
            new RecordingIconChanger(),
            new AlwaysIconChanger()
        };
    }
}