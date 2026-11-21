/********************************************************************
 * Copyright (C) 2015 Jeroen Pelgrims
 * Copyright (C) 2015-2024 Antoine Aflalo
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

using SoundSwitch.Framework.Profile;
using SoundSwitch.Services;
using SoundSwitch.UI.Component;

namespace SoundSwitch.Model;

/// <summary>
///     <see cref="IAppInfrastructure" /> façade: exposes the tray-adjacent managers the
///     interface promises. The managers are resolved lazily because they are constructed
///     by the startup sequence (AppBootstrapper) after this façade — the notification
///     manager, which is built earlier, reads <see cref="TrayIcon" /> only once the
///     tray icon exists, exactly as the pre-refactor property timing allowed.
/// </summary>
public class AppInfrastructure : IAppInfrastructure
{
    private readonly IAudioDeviceLister _audioDeviceLister;
    private readonly ErrorEvents _errorEvents;
    private readonly Lazy<TrayIcon> _trayIcon;
    private readonly Lazy<ProfileManager> _profileManager;
    private readonly Lazy<AppSoundLockManager> _appSoundLockManager;

    public AppInfrastructure(IAudioDeviceLister audioDeviceLister,
        ErrorEvents errorEvents,
        Lazy<TrayIcon> trayIcon,
        Lazy<ProfileManager> profileManager,
        Lazy<AppSoundLockManager> appSoundLockManager)
    {
        _audioDeviceLister = audioDeviceLister;
        _errorEvents = errorEvents;
        _trayIcon = trayIcon;
        _profileManager = profileManager;
        _appSoundLockManager = appSoundLockManager;
    }

    public TrayIcon TrayIcon => _trayIcon.Value;

    public IAudioDeviceLister AudioDeviceLister => _audioDeviceLister;

    public ProfileManager ProfileManager => _profileManager.Value;

    public AppSoundLockManager AppSoundLockManager => _appSoundLockManager.Value;

    /// <summary>
    ///     The application error event raised by the model services; the same single
    ///     event the pre-refactor AppModel exposed.
    /// </summary>
    public event EventHandler<ExceptionEvent> ErrorTriggered
    {
        add => _errorEvents.ErrorTriggered += value;
        remove => _errorEvents.ErrorTriggered -= value;
    }
}
