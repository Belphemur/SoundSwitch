﻿/********************************************************************
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

namespace SoundSwitch.Framework.TrayIcon.IconDoubleClick.Action;

/// <summary>
/// Defines the contract for system tray icon double-click action implementations.
/// Each implementation represents a specific action that can be performed when the user double-clicks the SoundSwitch system tray icon.
/// </summary>
public interface IIconDoubleClick : IEnumImpl<IconDoubleClickEnum>
{
    /// <summary>
    /// Executes the double-click action.
    /// This method is called when the user double-clicks the system tray icon and this action is configured.
    /// </summary>
    /// <param name="trayIcon">The TrayIcon instance that can be used to access UI functionality</param>
    void Execute(UI.Component.TrayIcon trayIcon);
}