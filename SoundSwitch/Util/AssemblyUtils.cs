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

using System.Linq;
using System.Reflection;

namespace SoundSwitch.Util
{
    public static class AssemblyUtils
    {
        public enum ReleaseState
        {
            Stable,
            Beta
        }

        /// <summary>
        /// Get Current Assembly configuration
        /// </summary>
        /// <returns></returns>
        public static AssemblyConfigurationAttribute GetAssemblyConfigurationAttribute()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var attributes = assembly.GetCustomAttributes(true);
            var config = attributes.OfType<AssemblyConfigurationAttribute>().FirstOrDefault();
            return config;
        }

        /// <summary>
        /// Get the current state of the application
        /// </summary>
        /// <returns></returns>
        public static ReleaseState GetReleaseState()
        {
            return GetAssemblyConfigurationAttribute().Configuration == "Beta" ? ReleaseState.Beta : ReleaseState.Stable;
        }
    }
}