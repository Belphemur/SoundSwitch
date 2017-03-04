/********************************************************************
* Copyright (C) 2015 Jeroen Pelgrims
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
using System.Windows.Forms;
using AudioEndPointControllerWrapper;
using SoundSwitch.Properties;

namespace SoundSwitch.Util
{
    internal class ToolStripDeviceItem : ToolStripMenuItem
    {
        public ToolStripDeviceItem(EventHandler onClick, IAudioDevice audioDevice)
            : base(audioDevice.FriendlyName, audioDevice.IsDefault(Role.Console) ? Resources.Check : null, onClick)
        {
            AudioDevice = audioDevice;
        }

        public IAudioDevice AudioDevice { get; set; }
    }
}