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

using Microsoft.Win32;

namespace SoundSwitch.Framework.WinApi;

/// <summary>
/// Helper that reports whether Windows is currently using a light or dark application mode.
/// </summary>
public static class WindowsThemeHelper
{
    private const string PersonalizeKey = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    // AppsUseLightTheme (not SystemUsesLightTheme) — matches what Application.SetColorMode
    // and Application.IsDarkModeEnabled resolve internally, so the tray icon path and the
    // custom-paint path stay consistent. When the user sets Windows mode and App mode
    // independently (Win11 Personalization → 'Choose your default app mode'), this helper
    // follows the App mode so our colour choices match the rest of WinForms.
    private const string AppsUseLightThemeValue = "AppsUseLightTheme";

    /// <summary>
    /// Returns true when Windows is currently using a dark application mode
    /// (the default for dark mode; this is the registry value WinForms reads via
    /// Application.SetColorMode / Application.IsDarkModeEnabled).
    /// </summary>
    public static bool IsDarkModeEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(PersonalizeKey);
        if (key?.GetValue(AppsUseLightThemeValue) is int value)
        {
            return value == 0;
        }

        // Default to dark mode if the value is missing, matching Windows' default dark apps.
        return true;
    }
}