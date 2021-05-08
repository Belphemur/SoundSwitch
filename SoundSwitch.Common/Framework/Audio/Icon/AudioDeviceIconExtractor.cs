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
using Microsoft.Extensions.Caching.Memory;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Common.Properties;

namespace SoundSwitch.Common.Framework.Audio.Icon
{
    public class AudioDeviceIconExtractor
    {
        private static readonly System.Drawing.Icon DefaultSpeakers   = Resources.defaultSpeakers;
        private static readonly System.Drawing.Icon DefaultMicrophone = Resources.defaultMicrophone;

        private static readonly IMemoryCache IconCache = new MemoryCache(new MemoryCacheOptions());

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
            var key = $"{path}-${largeIcon}";

            return IconCache.GetOrCreate(key, entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromMinutes(30);
                entry.RegisterPostEvictionCallback((o, value, reason, state) =>
                {
                    if (!(value is IDisposable disposable)) return;
                    //Don't dispose the default values
                    if (value == DefaultMicrophone || value == DefaultSpeakers) return;
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (ObjectDisposedException)
                    {
                    }
                });
                try
                {
                    if (path.EndsWith(".ico"))
                    {
                        return System.Drawing.Icon.ExtractAssociatedIcon(path);
                    }

                    var iconInfo  = path.Split(',');
                    var dllPath   = iconInfo[0];
                    var iconIndex = int.Parse(iconInfo[1]);
                    return IconExtractor.Extract(dllPath, iconIndex, largeIcon);
                }
                catch (Exception e)
                {
                    Log.Warning(e, "Can't extract icon from {path}", path);
                    return dataFlow switch
                    {
                        DataFlow.Capture => DefaultMicrophone,
                        DataFlow.Render  => DefaultSpeakers,
                        _                => throw new ArgumentOutOfRangeException()
                    };
                }
            });
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