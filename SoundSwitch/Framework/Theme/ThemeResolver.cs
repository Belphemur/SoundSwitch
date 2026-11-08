/********************************************************************
 * Copyright (C) 2015-2026 Antoine Aflalo
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
using System.Windows.Forms;

using SoundSwitch.Framework.Configuration;

namespace SoundSwitch.Framework.Theme;

/// <summary>
/// Single decision point between a user-selected <see cref="ThemeMode"/> and the
/// concrete theme rendering. Both consumers — Application.SetColorMode at startup
/// (Program.cs) and the custom-paint axis (WindowsThemeHelper.IsDarkModeEnabled) —
/// go through this class, so the framework palette and the custom paint can never
/// disagree (see docs/design/ui/2026-11-06-app-theme-setting.md and issue #2567).
/// </summary>
public static class ThemeResolver
{
    /// <summary>
    /// Where the user-selected mode comes from. Test seam; production reads the
    /// persisted configuration (defaulting to System for existing config files).
    /// </summary>
    internal static Func<ThemeMode> ThemeModeProvider { get; set; } = () => AppConfigs.Configuration.ThemeMode;

    /// <summary>
    /// The theme selection this process was started with, captured on first use
    /// (at startup, when <see cref="System.Windows.Forms.Application.SetColorMode"/>
    /// resolves the colour mode). The written setting only takes effect after the
    /// user accepts a restart, so a later configuration change must not flip
    /// custom paint while the WinForms palette keeps the startup one — that would
    /// recreate the mixed rendering of issue #2567. The System selection
    /// intentionally keeps following the live Windows app mode.
    /// Test seam below clears the capture; production captures once per process.
    /// </summary>
    internal static ThemeMode EffectiveThemeMode => _startupThemeMode ??= ThemeModeProvider();

    private static ThemeMode? _startupThemeMode;

    /// <summary>
    /// Clears the captured startup theme. Test-only; production captures once per
    /// process at startup and keeps it until the process exits (which is a restart).
    /// </summary>
    internal static void ResetEffectiveThemeModeForTests() => _startupThemeMode = null;

    /// <summary>
    /// WinForms renders dark mode correctly only on Windows 11 (build 22000+).
    /// Microsoft docs state: "the dark color mode is only available from Windows 11
    /// on or later versions". On an incomplete host everything resolves to light so
    /// the app never renders the half-broken mixed palette (issue #2567).
    /// Test seam; production checks the running Windows build.
    /// </summary>
    internal static Func<bool> DarkModeSupported { get; set; } =
        () => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);

    /// <summary>
    /// One decision: does the theme the user selected end up dark here? The OS dark
    /// app-mode is only consulted by the System mode, which keeps following Windows.
    /// </summary>
    internal static bool ResolveIsDarkMode(ThemeMode mode, bool osDarkMode, bool darkModeSupported)
    {
        if (!darkModeSupported)
            return false;

        return mode switch
        {
            ThemeMode.Light => false,
            ThemeMode.Dark => true,
            _ => osDarkMode,
        };
    }

    /// <summary>
    /// The colour mode to hand to <see cref="Application.SetColorMode"/> before any
    /// form is created, honouring the user-selected <see cref="ThemeMode"/>.
    /// On hosts where WinForms dark mode is incomplete (Windows 10) the palette is
    /// coherent light instead (issue #2567).
    /// </summary>
    public static SystemColorMode ResolveColorMode()
    {
        if (!DarkModeSupported())
            return SystemColorMode.Classic;

        return EffectiveThemeMode switch
        {
            ThemeMode.Light => SystemColorMode.Classic,
            ThemeMode.Dark => SystemColorMode.Dark,
            _ => SystemColorMode.System,
        };
    }
}
