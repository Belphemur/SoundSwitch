/********************************************************************
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

using System.Collections.Generic;
using System.Windows.Forms;
using SoundSwitch.Util;

namespace SoundSwitch.Framework
{
    public class SoundSwitchConfiguration : IConfiguration
    {
        public SoundSwitchConfiguration()
        {
            FirstRun = true;
            RunOnStartup = false;
            SelectedDeviceList = new HashSet<string>();
            HotKeys = Keys.F11;
            HotModifierKeys = ModifierKeys.Alt | ModifierKeys.Control;
        }

        public HashSet<string> SelectedDeviceList { get; set; }
        public string LastActiveDevice { get; set; }
        public bool FirstRun { get; set; }
        public bool RunOnStartup { get; set; }
        public Keys HotKeys { get; set; }
        public ModifierKeys HotModifierKeys { get; set; }
        public string FileLocation { get; set; }

        public void Save()
        {
            ConfigurationManager.SaveConfiguration(this);
        }
    }
}