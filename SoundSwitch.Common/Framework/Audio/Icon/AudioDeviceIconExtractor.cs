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

#nullable enable
using System;
using NAudio.CoreAudioApi;
using Serilog;
using SoundSwitch.Common.Framework.Icon;
using SoundSwitch.Common.Properties;

namespace SoundSwitch.Common.Framework.Audio.Icon
{
    /// <summary>
    /// Extracts icons for audio devices with DataFlow-specific fallback defaults.
    /// Delegates caching and GDI handle management to <see cref="IconExtractor"/>.
    /// </summary>
    public class AudioDeviceIconExtractor
    {
        private static readonly System.Drawing.Icon DefaultSpeakers = Resources.defaultSpeakers;
        private static readonly System.Drawing.Icon DefaultMicrophone = Resources.defaultMicrophone;

        /// <summary>
        ///     Extract an icon from an audio device icon path, falling back to a DataFlow-specific
        ///     default icon on failure.
        ///     The returned icon is owned by the cache and must not be disposed by the caller.
        /// </summary>
        /// <param name="path">Audio device icon path (a <c>.ico</c> file or <c>dllPath,iconIndex</c>).</param>
        /// <param name="dataFlow">Data flow of the device, used to select the fallback icon.</param>
        /// <param name="largeIcon">When true, extract a large icon; otherwise a small icon.</param>
        /// <returns>The device icon, or a default speaker/microphone icon on failure.</returns>
        public static System.Drawing.Icon ExtractIconFromPath(string path, DataFlow dataFlow, bool largeIcon)
        {
            try
            {
                return IconExtractor.ExtractFromPath(path, largeIcon);
            }
            catch (Exception e)
            {
                Log.Warning(e, "Can't extract icon from {path}", path);
                return dataFlow switch
                {
                    DataFlow.Capture => DefaultMicrophone,
                    DataFlow.Render => DefaultSpeakers,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        /// <summary>
        ///     Extract the Icon out of an AudioDevice.
        ///     The returned icon is owned by the cache and must not be disposed by the caller.
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
