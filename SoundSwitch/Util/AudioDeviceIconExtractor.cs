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

using System;
using System.Collections.Generic;
using System.Drawing;
using AudioEndPointControllerWrapper;
using SoundSwitch.Framework;
using SoundSwitch.Properties;

namespace SoundSwitch.Util
{
    internal class AudioDeviceIconExtractor
    {
        private static readonly Dictionary<string, Icon> IconCache = new Dictionary<string, Icon>();

        /// <summary>
        ///     Extract the Icon out of an AudioDevice
        /// </summary>
        /// <param name="audioDevice"></param>
        /// <param name="largeIcon"></param>
        /// <returns></returns>
        public static Icon ExtractIconFromAudioDevice(IAudioDevice audioDevice, bool largeIcon)
        {
            Icon ico;
            if (IconCache.TryGetValue(audioDevice.DeviceClassIconPath, out ico))
            {
                return ico;
            }
            try
            {
                if (audioDevice.DeviceClassIconPath.EndsWith(".ico"))
                {
                    ico = Icon.ExtractAssociatedIcon(audioDevice.DeviceClassIconPath);
                }
                else
                {
                    var iconInfo = audioDevice.DeviceClassIconPath.Split(',');
                    var dllPath = iconInfo[0];
                    var iconIndex = int.Parse(iconInfo[1]);
                    ico = IconExtractor.Extract(dllPath, iconIndex, largeIcon);
                }
            }
            catch (Exception e)
            {
                AppLogger.Log.Error($"Can't extract icon from {audioDevice.DeviceClassIconPath}\n Ex: ", e);
                switch (audioDevice.Type)
                {
                    case AudioDeviceType.Playback:
                        ico = Resources.defaultSpeakers;
                        break;
                    case AudioDeviceType.Recording:
                        ico = Resources.defaultMicrophone;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            IconCache.Add(audioDevice.DeviceClassIconPath, ico);
            return ico;
        }
    }
}