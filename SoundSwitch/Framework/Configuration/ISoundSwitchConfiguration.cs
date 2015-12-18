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

namespace SoundSwitch.Framework.Configuration
{
    public interface ISoundSwitchConfiguration : IConfiguration
    {
        HashSet<string> SelectedPlaybackDeviceList { get; set; }
        HashSet<string> SelectedRecordingDeviceList { get; set; }
        string LastPlaybackActiveId { get; set; }
        string LastRecordingActiveId { get; set; }
        bool FirstRun { get; set; }
        HotKeys PlaybackHotKeys { get; set; }
        HotKeys RecordingHotKeys { get; set; }
        bool ChangeCommunications { get; set; }
        uint UpdateCheckInterval { get; set; }
        bool DisplayNotifications { get; set; }
    }
}