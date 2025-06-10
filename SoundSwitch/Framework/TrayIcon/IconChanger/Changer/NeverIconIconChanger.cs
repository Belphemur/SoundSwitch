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
using SoundSwitch.Localization;
using SoundSwitch.Properties;

namespace SoundSwitch.Framework.TrayIcon.IconChanger.Changer
{
    public class NeverIconIconChanger : IIconChanger
    {
        public IconChangerEnum TypeEnum => IconChangerEnum.Never;
        public string Label => TrayIconStrings.iconChanger_none;

        public void ChangeIcon(UI.Component.TrayIcon trayIcon) => trayIcon.ReplaceIcon(Resources.Switch_SoundWave);

        public void ChangeIcon(UI.Component.TrayIcon trayIcon, DeviceFullInfo deviceInfo, ERole role) { }
    }
}