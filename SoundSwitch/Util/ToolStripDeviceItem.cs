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
using NAudio.CoreAudioApi;
using SoundSwitch.Audio.Manager;
using SoundSwitch.Audio.Manager.Interop.Enum;
using SoundSwitch.Common.Framework.Audio.Device;
using SoundSwitch.Properties;

namespace SoundSwitch.Util
{
    internal class ToolStripDeviceItem : ToolStripMenuItem
    {
        private static readonly Bitmap check = Resources.Check;

        public ToolStripDeviceItem(EventHandler onClick, DeviceFullInfo audioDevice)
            : base(audioDevice.NameClean, null, onClick)
        {
            AudioDevice = audioDevice;
        }

        public override Image Image
        {
            get
            {
                if (AudioDevice != null &&
                    AudioSwitcher.Instance.IsDefault(AudioDevice.Id, (EDataFlow)AudioDevice.Type, ERole.eConsole))
                    return check;

                return null;
            }
        }

        public DeviceFullInfo AudioDevice { get; }
    }
}