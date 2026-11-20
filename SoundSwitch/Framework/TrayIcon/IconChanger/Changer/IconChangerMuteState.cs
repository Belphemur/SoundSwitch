/********************************************************************
* Copyright (C) 2015-2017 Antoine Aflalo
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License; either
* version 2 of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
********************************************************************/

using Serilog;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Framework.TrayIcon;
using SoundSwitch.Framework.WinApi;
using SoundSwitch.Localization;
using SoundSwitch.Model;

namespace SoundSwitch.Framework.TrayIcon.IconChanger.Changer;

public class IconChangerMuteState : IIconChanger
{
    public IconChanger TypeEnum => IconChanger.MuteState;
    public string Label => TrayIconStrings.iconChanger_muteState;

    /// <summary>
    /// Replaces the given tray icon with the glyph matching the current mute state of the
    /// tracked microphone (the configured mute device, else the default communication input).
    /// </summary>
    /// <param name="trayIcon">The tray icon whose visual icon will be replaced.</param>
    public void ChangeIcon(UI.Component.TrayIcon trayIcon)
    {
        var state = AppModel.Instance.GetMicrophoneMuteState();
        if (state == null)
        {
            // No microphone resolved: keep the unmuted glyph; a mute event corrects the
            // icon once a microphone is tracked again.
            Log.Information("No microphone found to read the mute state, showing the unmuted tray icon");
            ChangeIcon(trayIcon, false);
            return;
        }

        ChangeIcon(trayIcon, state.Value.IsMuted);
    }

    public void ChangeIcon(UI.Component.TrayIcon trayIcon, DeviceFullInfo deviceInfo, ERole role) { }

    /// <summary>
    /// Replaces the given tray icon with the glyph matching the given mute state of the
    /// tracked microphone.
    /// </summary>
    /// <param name="trayIcon">The tray icon whose visual icon will be replaced.</param>
    /// <param name="isMuted">Whether the tracked microphone is muted.</param>
    public void ChangeIcon(UI.Component.TrayIcon trayIcon, bool isMuted)
    {
        using var handle = ThemeIcons.GetIcon(SelectIconKind(isMuted), WindowsThemeHelper.IsDarkModeEnabled()).Acquire();
        trayIcon.ReplaceIcon(handle.Icon);
    }

    /// <summary>
    /// The glyph shown for the given mute state of the tracked microphone.
    /// </summary>
    internal static IconKind SelectIconKind(bool isMuted) => isMuted ? IconKind.MicrophoneMuted : IconKind.Microphone;
}
