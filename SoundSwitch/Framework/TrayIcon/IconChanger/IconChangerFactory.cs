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
using SoundSwitch.Model;

namespace SoundSwitch.Framework.TrayIcon.IconChanger;

public class IconChangerFactory(IDeviceService deviceService) : AbstractFactory<IconChanger, IIconChanger>(Impl(deviceService))
{
    private static IEnumImplList<IconChanger, IIconChanger> Impl(IDeviceService deviceService) => new EnumImplList<IconChanger, IIconChanger>()
    {
        new IconChangerNone(),
        new IconChangerPlayback(),
        new IconChangerRecording(),
        new IconChangerAlways(),
        new IconChangerThemeBased(),
        new IconChangerMuteState(deviceService)
    };
}
