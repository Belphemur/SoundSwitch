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
using SoundSwitch.Framework.Factory;

namespace SoundSwitch.Framework.TrayIcon.IconChanger
{
    public interface IIconChanger : IEnumImpl<IconChangerEnum>
    {

        /// <summary>
        /// Change the icon to the current default device
        /// </summary>
        /// <param name="trayIcon"></param>
        void ChangeIcon(UI.Component.TrayIcon trayIcon);
        
        /// <summary>
        ///  Change icon to selected device if match the condition
        /// </summary>
        void ChangeIcon(UI.Component.TrayIcon trayIcon, DeviceFullInfo deviceInfo, ERole role);
    }
}