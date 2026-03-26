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
/// Implementation for the "Switch Recording Device" double-click action.
/// When the user double-clicks the system tray icon, this action cycles through the configured recording devices.
/// </summary>
internal class IconDoubleClickSwitchRecordingDevice : IIconDoubleClick
{
    /// <summary>
    /// Gets the enum type that this implementation corresponds to.
    /// </summary>
    public IconDoubleClick TypeEnum => IconDoubleClick.SwitchRecordingDevice;

    /// <summary>
    /// Gets the localized label for this action as displayed in the user interface.
    /// </summary>
    public string Label => SettingsStrings.switchRecordingDevice;

    /// <summary>
    /// Executes the switch recording device action by cycling through configured recording devices.
    /// </summary>
    /// <param name="trayIcon">The TrayIcon instance (not used for this action)</param>
    public void Execute(UI.Component.TrayIcon trayIcon)
    {
        AppModel.Instance.CycleActiveDevice(DataFlow.Capture);
    }
}
