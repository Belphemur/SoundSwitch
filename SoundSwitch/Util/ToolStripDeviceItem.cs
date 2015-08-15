/********************************************************************
* Copyright (C) 2015 Jeroen Pelgrims
* Copyright (C) 2015 Antoine Aflalo
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

namespace SoundSwitch.Util
{
    internal class ToolStripDeviceItem : ToolStripMenuItem
    {
        public ToolStripDeviceItem(EventHandler onClick, AudioDeviceWrapper audioDevice)
            : base(audioDevice.FriendlyName, null, onClick)
        {
            AudioDevice = audioDevice;
        }

        public AudioDeviceWrapper AudioDevice { get; set; }
    }
}