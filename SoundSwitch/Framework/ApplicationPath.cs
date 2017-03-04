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
using System.IO;
using System.Windows.Forms;

namespace SoundSwitch.Framework
{
    public static class ApplicationPath
    {
        /// <summary>
        /// Application data directory
        /// </summary>
        public static string AppData { get; } = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData);
        /// <summary>
        /// Path where the application store it's file like the configuration.
        /// </summary>
        public static string Default { get; } = Path.Combine(AppData, Application.ProductName);
        /// <summary>
        /// Path wher ethe application store the logs
        /// </summary>
        public static string Logs { get; } = Path.Combine(Default, "Logs");
        /// <summary>
        /// Where the application is installed
        /// </summary>
        public static string InstallDirectory { get; } = Path.GetDirectoryName(Application.ExecutablePath);
        /// <summary>
        /// Where is the image to be used for toast
        /// </summary>
        public static string DefaultImagePath { get; } = Path.Combine(InstallDirectory, "img", "soundSwitched.png");
    }
}