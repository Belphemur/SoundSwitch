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

using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.IconDoubleClick.Action;

/// <summary>
/// Implementation for the "Disabled" double-click action.
/// When the user double-clicks the system tray icon, this action performs no operation,
/// so single-click can show the device selection menu immediately.
/// </summary>
internal class IconDoubleClickNone : IIconDoubleClick
{
    /// <summary>
    /// Gets the enum type that this implementation corresponds to.
    /// </summary>
    public IconDoubleClick TypeEnum => IconDoubleClick.None;

    /// <summary>
    /// Gets the localized label for this action as displayed in the user interface.
    /// </summary>
    public string Label => SettingsStrings.disableDoubleClickAction;

    /// <summary>
    /// Executes no operation. Double-click detection is disabled, so the single-click
    /// device selection menu is shown immediately without a double-click triggering anything.
    /// </summary>
    /// <param name="trayIcon">The TrayIcon instance (unused).</param>
    public void Execute(UI.Component.TrayIcon trayIcon)
    {
    }
}
