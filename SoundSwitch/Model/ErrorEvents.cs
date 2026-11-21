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

namespace SoundSwitch.Model;

/// <summary>
///     Owns the application-wide error event raised by the model services.
///     <see cref="IAppInfrastructure.ErrorTriggered" /> forwards to this hub so the
///     services can raise where the exceptions happen while every subscriber keeps
///     observing a single event, exactly as the pre-refactor AppModel exposed one.
/// </summary>
public class ErrorEvents
{
    /// <summary>
    ///     The single application error event; raised through <see cref="Raise" />.
    /// </summary>
    public event EventHandler<ExceptionEvent> ErrorTriggered;

    /// <summary>
    ///     Raises <see cref="ErrorTriggered" /> for the given exception. Same raise
    ///     points as the pre-refactor AppModel members.
    /// </summary>
    public void Raise(Exception exception)
    {
        ErrorTriggered?.Invoke(this, new ExceptionEvent(exception));
    }
}
