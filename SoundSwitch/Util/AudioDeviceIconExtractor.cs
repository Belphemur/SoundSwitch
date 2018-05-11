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

using System;
using System.Collections.Generic;
using System.Drawing;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Framework;
using SoundSwitch.Properties;

namespace SoundSwitch.Util
{
    internal class AudioDeviceIconExtractor
    {
        private static readonly Icon defaultSpeakers = Resources.defaultSpeakers;
        private static readonly Icon defaultMicrophone = Resources.defaultMicrophone;

        /// <summary>
        ///     Extract the Icon out of an AudioDevice
        /// </summary>
        /// <param name="audioDevice"></param>
        /// <param name="largeIcon"></param>
        /// <returns></returns>
        public static Icon ExtractIconFromAudioDevice(MMDevice audioDevice, bool largeIcon)
        {
            try
            {
                if (audioDevice.IconPath.EndsWith(".ico"))
                {
                    return Icon.ExtractAssociatedIcon(audioDevice.IconPath);
                }
                else
                {
                    var iconInfo = audioDevice.IconPath.Split(',');
                    var dllPath = iconInfo[0];
                    var iconIndex = int.Parse(iconInfo[1]);
                    return IconExtractor.Extract(dllPath, iconIndex, largeIcon);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Can't extract icon from {path}", audioDevice.IconPath);
                switch (audioDevice.DataFlow)
                {
                    case DataFlow.Capture:
                        return defaultSpeakers;
                    case DataFlow.Render:
                        return defaultMicrophone;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}