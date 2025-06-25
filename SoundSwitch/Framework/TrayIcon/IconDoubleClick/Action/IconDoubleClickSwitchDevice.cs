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

using NAudio.CoreAudioApi;
using SoundSwitch.Localization;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.TrayIcon.IconDoubleClick.Action;

/// <summary>
/// Implementation for the "Switch Device" double-click action.
/// When the user double-clicks the system tray icon, this action cycles through the configured playback devices.
/// This is the default behavior and provides the traditional SoundSwitch functionality of device switching.
/// </summary>
internal class IconDoubleClickSwitchDevice : IIconDoubleClick
{
    /// <summary>
    /// Gets the enum type that this implementation corresponds to.
    /// </summary>
    public IconDoubleClickEnum TypeEnum => IconDoubleClickEnum.SwitchDevice;

    /// <summary>
    /// Gets the localized label for this action as displayed in the user interface.
    /// </summary>
    public string Label => SettingsStrings.switchDevice;

    /// <summary>
    /// Executes the switch device action by cycling through configured playback devices.
    /// </summary>
    /// <param name="trayIcon">The TrayIcon instance (not used for this action)</param>
    public void Execute(UI.Component.TrayIcon trayIcon)
    {
        AppModel.Instance.CycleActiveDevice(DataFlow.Render);
    }
}