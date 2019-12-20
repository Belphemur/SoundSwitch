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
using System.Runtime.Caching;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Common.Properties;

namespace SoundSwitch.Common.Framework.Audio.Icon
{
    public class AudioDeviceIconExtractor
    {

        private static readonly System.Drawing.Icon DefaultSpeakers = Resources.defaultSpeakers;
        private static readonly System.Drawing.Icon DefaultMicrophone = Resources.defaultMicrophone;

        private static readonly MemoryCache IconCache = new MemoryCache("_iconCache");
        private static readonly CacheItemPolicy CacheItemPolicy = new CacheItemPolicy
        {
            RemovedCallback = CleanupIcon,
            SlidingExpiration = TimeSpan.FromMinutes(30)
        };

        private static void CleanupIcon(CacheEntryRemovedArguments arg)
        {
            if (!(arg.CacheItem.Value is IDisposable item)) return;
            try
            {
                item.Dispose();
            }
            catch (ObjectDisposedException)
            {

            }
        }

        private static string GetKey(MMDevice audioDevice, bool largeIcon)
        {
            return $"{audioDevice.IconPath}-${largeIcon}";
        }

        /// <summary>
        /// Extract an Icon form a given path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dataFlow"></param>
        /// <param name="largeIcon"></param>
        /// <returns></returns>
        public static System.Drawing.Icon ExtractIconFromPath(string path, DataFlow dataFlow, bool largeIcon)
        {
            System.Drawing.Icon ico;
            var key = $"{path}-${largeIcon}";

            if (IconCache.Contains(key))
            {
                return (System.Drawing.Icon)IconCache.Get(key);
            }
            try
            {
                if (path.EndsWith(".ico"))
                {
                    ico = System.Drawing.Icon.ExtractAssociatedIcon(path);
                }
                else
                {
                    var iconInfo = path.Split(',');
                    var dllPath = iconInfo[0];
                    var iconIndex = int.Parse(iconInfo[1]);
                    ico = IconExtractor.Extract(dllPath, iconIndex, largeIcon);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "Can't extract icon from {path}", path);
                switch (dataFlow)
                {
                    case DataFlow.Capture:
                        return DefaultMicrophone;
                    case DataFlow.Render:
                        return DefaultSpeakers;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            IconCache.Add(key, ico, CacheItemPolicy);
            return ico;
        }

        /// <summary>
        ///     Extract the Icon out of an AudioDevice
        /// </summary>
        /// <param name="audioDevice"></param>
        /// <param name="largeIcon"></param>
        /// <returns></returns>
        public static System.Drawing.Icon ExtractIconFromAudioDevice(MMDevice audioDevice, bool largeIcon)
        {
            return ExtractIconFromPath(audioDevice.IconPath, audioDevice.DataFlow, largeIcon);
        }
    }
}