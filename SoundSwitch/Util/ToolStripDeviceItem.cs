﻿/********************************************************************
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
using System.Drawing;
using System.Windows.Forms;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Properties;

namespace SoundSwitch.Util;

internal class ToolStripDeviceItem(EventHandler onClick, DeviceInfo audioDevice, bool isDefault)
    : ToolStripMenuItem(audioDevice.NameClean, null, onClick)
{
    public override Image Image => isDefault ? Resources.Check : null;

    public DeviceInfo AudioDevice { get; } = audioDevice;
}