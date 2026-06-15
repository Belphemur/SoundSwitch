/************************************************************************
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
using SoundSwitch.Framework.Audio;
using SoundSwitch.Framework.Profile;
using SoundSwitch.UI.Component;
using SoundSwitch.Services;

namespace SoundSwitch.Model;

/// <summary>
/// Application infrastructure services and lifecycle.
/// </summary>
public interface IAppInfrastructure
{
    /// <summary>
    /// The tray icon of the application
    /// </summary>
    TrayIcon TrayIcon { get; }

    /// <summary>
    /// List the active audio devices
    /// </summary>
    IAudioDeviceLister AudioDeviceLister { get; }

    /// <summary>
    /// Manage the profile in the application
    /// </summary>
    ProfileManager ProfileManager { get; }

    /// <summary>
    /// Manage per-app audio routing (App Sound Lock)
    /// </summary>
    AppSoundLockManager AppSoundLockManager { get; }

    /// <summary>
    /// Initialize the Main class with Updater and Hotkeys
    /// </summary>
    void InitializeMain(IAudioDeviceLister active, bool skipUpdate = false);

    /// <summary>
    /// If an exception happened in the model
    /// </summary>
    event EventHandler<ExceptionEvent> ErrorTriggered;
}
