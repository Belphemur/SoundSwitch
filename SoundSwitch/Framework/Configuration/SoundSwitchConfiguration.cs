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

namespace SoundSwitch.Framework.Configuration
{
    public class SoundSwitchConfiguration : IConfiguration
    {
        public SoundSwitchConfiguration()
        {
            FirstRun = true;
            ChangeCommunications = false;
            SelectedDeviceList = new HashSet<string>();
            HotKeysCombinaison = new HotKeys(Keys.F11, HotKeys.ModifierKeys.Alt | HotKeys.ModifierKeys.Control);
        }

        public HashSet<string> SelectedDeviceList { get; set; }
        public string LastActiveDevice { get; set; }
        public bool FirstRun { get; set; }
        public HotKeys HotKeysCombinaison { get; set; }
        public bool ChangeCommunications { get; set; }
        //Needed by Interface
        public string FileLocation { get; set; }

        public void Save()
        {
            using (AppLogger.Log.DebugCall())
            {
                AppLogger.Log.Debug("Saving configuration ", this);
                ConfigurationManager.SaveConfiguration(this);
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name}({FileLocation})";
        }
    }
}