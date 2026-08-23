/********************************************************************
 * Copyright (C) 2024 Antoine Aflalo
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

namespace SoundSwitch.Framework.Telemetry;

/// <summary>
/// Why an update check was triggered. Reported as the <c>trigger</c> attribute value on
/// <c>soundswitch.update.check</c>.
/// </summary>
public enum UpdateCheckTrigger
{
    /// <summary>A user-initiated check (tray menu "Check for update").</summary>
    Manual,

    /// <summary>A check kicked off automatically by a settings change (beta channel / update mode).</summary>
    SettingChange
}
