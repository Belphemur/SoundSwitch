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

namespace SoundSwitch.Framework.Updater;

public enum UpdateMode
{
    /// <summary>
    /// Updates are installed in the background automatically without asking the user anything
    /// </summary>
    Silent,

    /// <summary>
    /// If an update exists, the user will be notified
    /// </summary>
    Notify,

    /// <summary>
    /// Update mechanism is disabled
    /// </summary>
    Never
}