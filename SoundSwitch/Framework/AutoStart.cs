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

using System.Windows.Forms;
using Microsoft.Win32;

namespace SoundSwitch.Framework
{
    public static class AutoStart
    {
        private static readonly RegistryKey SstartupKey = Registry.CurrentUser.OpenSubKey
            ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        /// <summary>
        ///     Enable autostarting using registry for the current user
        /// </summary>
        public static void EnableAutoStart()
        {
            using (AppLogger.Log.DebugCall())
            {
                SstartupKey?.SetValue(Application.ProductName, Application.ExecutablePath);
            }
        }

        /// <summary>
        ///     Disable autostarting using registry for the current user
        /// </summary>
        public static void DisableAutoStart()
        {
            using (AppLogger.Log.DebugCall())
            {
                SstartupKey?.DeleteValue(Application.ProductName, false);
            }
        }

        /// <summary>
        ///     Check if the application is set to start with Windows
        /// </summary>
        /// <returns></returns>
        public static bool IsAutoStarted()
        {
            using (AppLogger.Log.DebugCall())
            {
                return SstartupKey?.GetValue(Application.ProductName)?.ToString() == Application.ExecutablePath;
            }
        }
    }
}