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

namespace SoundSwitch.Framework.TrayIcon.IconDoubleClick;

/// <summary>
/// Defines the available actions that can be performed when double-clicking the SoundSwitch system tray icon.
/// These actions are configurable by the user in the application settings.
/// </summary>
public enum IconDoubleClickEnum
{
    /// <summary>
    /// Cycles through the configured playback devices.
    /// This is the default behavior and matches the traditional SoundSwitch functionality.
    /// </summary>
    SwitchDevice = 0,

    /// <summary>
    /// Opens the SoundSwitch settings window.
    /// Provides quick access to application configuration.
    /// </summary>
    OpenSettings = 1,

    /// <summary>
    /// Cycles through the configured audio profiles.
    /// Allows users to quickly switch between different audio device configurations.
    /// </summary>
    SwitchProfile = 2,
}