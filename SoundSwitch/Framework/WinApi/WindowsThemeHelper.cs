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

using System;
using Microsoft.Win32;

using SoundSwitch.Framework.Theme;

namespace SoundSwitch.Framework.WinApi;

/// <summary>
/// Helper that reports whether the application is currently rendering in a light or
/// darkmode, honouring the user-selected theme (General tab) on top of the Windows
/// app mode.
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
    /// Returns true when the application currently renders dark: either the user
    /// forced Dark in the theme setting (General tab), or the setting is System and
    /// Windows is using a dark application mode. The decision — including the clamp
    /// to light on Windows 10 where WinForms dark mode is incomplete (issue #2567) —
    /// is made by <see cref="ThemeResolver.ResolveIsDarkMode"/>, shared with
    /// Application.SetColorMode so the two never disagree.
    /// (This is the registry value WinForms reads via Application.SetColorMode /
    /// Application.IsDarkModeEnabled.)
    /// </summary>
    public static bool IsDarkModeEnabled() => _darkModeProvider();

    /// <summary>
    /// Test-only seam for deterministic dark-mode behaviour. Production code never sets
    /// this; assigning null restores the normal registry lookup.
    /// </summary>
    internal static Func<bool> DarkModeProvider
    {
        get => _darkModeProvider;
        set => _darkModeProvider = value ?? ReadDarkModeEnabled;
    }

    private static Func<bool> _darkModeProvider = ReadDarkModeEnabled;

    private static bool ReadDarkModeEnabled()
    {
        return ThemeResolver.ResolveIsDarkMode(
            ThemeResolver.ThemeModeProvider(),
            ReadOsAppDarkMode(),
            ThemeResolver.DarkModeSupported());
    }

    private static bool ReadOsAppDarkMode()
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