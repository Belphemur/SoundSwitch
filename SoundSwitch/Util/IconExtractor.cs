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
//source : https://stackoverflow.com/questions/6872957/how-can-i-use-the-images-within-shell32-dll-in-my-c-sharp-project

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using AudioEndPointControllerWrapper;

namespace SoundSwitch.Util
{
    public static class IconExtractor
    {
        /// <summary>
        ///     Extract Icon from executable or DLL
        /// </summary>
        /// <param name="file"></param>
        /// <param name="iconIndex"></param>
        /// <param name="largeIcon"></param>
        /// <returns></returns>
        public static Icon Extract(string file, int iconIndex, bool largeIcon)
        {
            IntPtr large;
            IntPtr small;
            NativeMethods.ExtractIconEx(file, iconIndex, out large, out small, 1);
            try
            {
                return Icon.FromHandle(largeIcon ? large : small);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Extract the Icon out of an AudioDevice
        /// </summary>
        /// <param name="audioDevice"></param>
        /// <param name="largeIcon"></param>
        /// <returns></returns>
        public static Icon ExtractIconFromAudioDevice(AudioDeviceWrapper audioDevice, bool largeIcon)
        {
            var iconInfo = audioDevice.DeviceClassIconPath.Split(',');
            var dllPath = iconInfo[0];
            var iconIndex = int.Parse(iconInfo[1]);
            return Extract(dllPath, iconIndex, largeIcon);
        }

        private static class NativeMethods
        {
            [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true,
                CallingConvention = CallingConvention.StdCall)]
            public static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion,
                out IntPtr piSmallVersion, int amountIcons);
        }
    }
}