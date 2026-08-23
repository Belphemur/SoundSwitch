/*******************************************************************
 * Copyright (C) 2015 Jeroen Pelgrims
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

using System;
using SoundSwitch.Framework.Telemetry;
using SoundSwitch.Framework.TrayIcon.IconDoubleClick;
using SoundSwitch.Framework.Updater;
using SoundSwitch.Framework.WinApi.Keyboard;
using SoundSwitch.Localization.Factory;

namespace SoundSwitch.Model;

/// <summary>
/// General application settings: startup, language, updates, hotkeys.
/// </summary>
public interface IAppSettings
{
    /// <summary>
    /// If the application runs at windows startup.
    /// </summary>
    bool RunAtStartup { get; set; }

    /// <summary>
    /// If the Playback device need also to be set for Communications.
    /// </summary>
    bool SetCommunications { get; set; }

    /// <summary>
    /// Switch also the foreground program
    /// </summary>
    bool SwitchForegroundProgram { get; set; }

    /// <summary>
    /// The language of the application.
    /// </summary>
    Language Language { get; set; }

    bool Telemetry { get; set; }
    bool QuickMenuEnabled { get; set; }
    bool KeepVolumeEnabled { get; set; }

    /// <summary>
    /// Select the action when double-clicking the tray icon
    /// </summary>
    IconDoubleClick IconDoubleClick { get; set; }

    /// <summary>
    /// Beta or Stable channel.
    /// </summary>
    bool IncludeBetaVersions { get; set; }

    /// <summary>
    /// Specifies how the application searches for updates and installs them.
    /// </summary>
    UpdateMode UpdateMode { get; set; }

    /// <summary>
    /// Sets the hotkey combination
    /// </summary>
    bool SetHotkeyCombination(HotKey hotKey, HotKeyAction action, bool force = false);

    /// <summary>
    /// For the app to check for update
    /// </summary>
    /// <param name="trigger">What initiated the check (manual tray click or a settings change).</param>
    void CheckForUpdate(UpdateCheckTrigger trigger = UpdateCheckTrigger.Manual);

    /// <summary>
    /// Triggered when the update mode has been changed
    /// </summary>
    event EventHandler<UpdateMode> UpdateModeChanged;

    /// <summary>
    /// The update checker found a newer release than the installed version.
    /// </summary>
    event EventHandler<NewReleaseAvailableEvent> NewVersionReleased;
}