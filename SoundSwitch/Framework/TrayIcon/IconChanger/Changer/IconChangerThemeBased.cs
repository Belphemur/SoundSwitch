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

using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Framework.TrayIcon;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Localization;

namespace SoundSwitch.Framework.TrayIcon.IconChanger.Changer;

public class IconChangerThemeBased : IIconChanger
{
    public IconChanger TypeEnum => IconChanger.ThemeBased;
    public string Label => TrayIconStrings.iconChanger_themeBased;

    /// <summary>
    /// Replaces the given tray icon with the theme-appropriate application icon:
    /// a light icon for dark taskbars and a dark icon for light taskbars.
    /// </summary>
    /// <param name="trayIcon">The tray icon whose visual icon will be replaced.</param>
    public void ChangeIcon(UI.Component.TrayIcon trayIcon)
    {
        using var handle = ThemeIcons.GetIcon(IconKind.Speaker, WindowsThemeHelper.IsDarkModeEnabled()).Acquire();
        trayIcon.ReplaceIcon(handle.Icon);
    }

    public void ChangeIcon(UI.Component.TrayIcon trayIcon, DeviceFullInfo deviceInfo, ERole role)
    {
        if (role == ERole.eCommunications) return;
        var kind = DeviceFormFactorDetector.From(deviceInfo);
        using var handle = ThemeIcons.GetIcon(kind, WindowsThemeHelper.IsDarkModeEnabled()).Acquire();
        trayIcon.ReplaceIcon(handle.Icon);
    }
}
