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
using SoundSwitch.Model;

namespace SoundSwitch.Framework.TrayIcon.IconDoubleClick;

/// <summary>
/// Factory class for creating and managing system tray icon double-click action implementations.
/// Provides access to all available double-click actions that users can configure for the SoundSwitch system tray icon.
/// </summary>
public class IconDoubleClickFactory(IDeviceService deviceService, IAppInfrastructure infrastructure) : AbstractFactory<IconDoubleClick, IIconDoubleClick>(DoubleClick(deviceService, infrastructure))
{
    /// <summary>
    /// Collection of all available double-click action implementations.
    /// Each implementation corresponds to a specific action users can perform by double-clicking the system tray icon.
    /// </summary>
    private static IEnumImplList<IconDoubleClick, IIconDoubleClick> DoubleClick(IDeviceService deviceService, IAppInfrastructure infrastructure) => new EnumImplList
        <IconDoubleClick, IIconDoubleClick>
        {
            new IconDoubleClickOpenSettings(),
            new IconDoubleClickSwitchProfile(infrastructure),
            new IconDoubleClickSwitchPlaybackDevice(deviceService),
            new IconDoubleClickSwitchRecordingDevice(deviceService),
            new IconDoubleClickToggleMicrophoneMute(deviceService),
            new IconDoubleClickNone(),
        };

    /// <summary>
    /// Executes the specified double-click action.
    /// This method eliminates the need for switch statements in the calling code.
    /// </summary>
    /// <param name="actionType">The type of action to execute</param>
    /// <param name="trayIcon">The TrayIcon instance to pass to the action</param>
    public void ExecuteAction(IconDoubleClick actionType, UI.Component.TrayIcon trayIcon)
    {
        var action = Get(actionType);
        action.Execute(trayIcon);
    }
}
