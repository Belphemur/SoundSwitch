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
using SoundSwitch.Framework.TrayIcon.IconDoubleClick.Action;

namespace SoundSwitch.Framework.TrayIcon.IconDoubleClick;

/// <summary>
/// Factory class for creating and managing system tray icon double-click action implementations.
/// Provides access to all available double-click actions that users can configure for the SoundSwitch system tray icon.
/// </summary>
public class IconDoubleClickFactory() : AbstractFactory<IconDoubleClickEnum, IIconDoubleClick>(DoubleClick)
{
    /// <summary>
    /// Collection of all available double-click action implementations.
    /// Each implementation corresponds to a specific action users can perform by double-clicking the system tray icon.
    /// </summary>
    private static readonly IEnumImplList<IconDoubleClickEnum, IIconDoubleClick> DoubleClick = new EnumImplList
        <IconDoubleClickEnum, IIconDoubleClick>
        {
            new IconDoubleClickSwitchDevice(),
            new IconDoubleClickSwitchProfile(),
            new IconDoubleClickOpenSettings()
        };
}