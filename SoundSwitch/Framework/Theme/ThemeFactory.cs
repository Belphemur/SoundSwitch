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

using SoundSwitch.Framework.Factory;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.Theme;

public class ThemeFactory() : AbstractFactory<ThemeMode, IThemeMode>(Impl)
{
    private static readonly IEnumImplList<ThemeMode, IThemeMode> Impl = new EnumImplList<ThemeMode, IThemeMode>()
    {
        new ThemeModeSystem(),
        new ThemeModeLight(),
        new ThemeModeDark()
    };
}

public class ThemeModeSystem : IThemeMode
{
    public ThemeMode TypeEnum => ThemeMode.System;
    public string Label => SettingsStrings.themeSystem;
}

public class ThemeModeLight : IThemeMode
{
    public ThemeMode TypeEnum => ThemeMode.Light;
    public string Label => SettingsStrings.themeLight;
}

public class ThemeModeDark : IThemeMode
{
    public ThemeMode TypeEnum => ThemeMode.Dark;
    public string Label => SettingsStrings.themeDark;
}
