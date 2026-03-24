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
    /// Delegates caching and GDI reference counting to <see cref="IconExtractor"/>.
    /// </summary>
    public class AudioDeviceIconExtractor
    {
        private static readonly IconHandle DefaultSpeakersHandle = IconExtractor.CreatePermanent(Resources.defaultSpeakers);
        private static readonly IconHandle DefaultMicrophoneHandle = IconExtractor.CreatePermanent(Resources.defaultMicrophone);

        /// <summary>
        /// Extract an icon from an audio device icon path, falling back to a DataFlow-specific
        /// default icon on failure.
        /// </summary>
        /// <param name="path">Audio device icon path (a <c>.ico</c> file or <c>dllPath,iconIndex</c>).</param>
        /// <param name="dataFlow">Data flow of the device, used to select the fallback icon.</param>
        /// <param name="largeIcon">When <see langword="true"/>, extract a 32×32 icon; otherwise 16×16.</param>
        /// <returns>
        /// An <see cref="IconHandle"/> the caller <strong>must dispose</strong> when done.
        /// </returns>
        public static IconHandle ExtractIconFromPath(string path, DataFlow dataFlow, bool largeIcon)
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
                    DataFlow.Capture => DefaultMicrophoneHandle.Acquire(),
                    DataFlow.Render => DefaultSpeakersHandle.Acquire(),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        /// <summary>
        /// Extract the icon out of an <see cref="MMDevice"/>.
        /// </summary>
        /// <param name="audioDevice"></param>
        /// <param name="largeIcon"></param>
        /// <returns>
        /// An <see cref="IconHandle"/> the caller <strong>must dispose</strong> when done.
        /// </returns>
        public static IconHandle ExtractIconFromAudioDevice(MMDevice audioDevice, bool largeIcon)
        {
            return ExtractIconFromPath(audioDevice.IconPath, audioDevice.DataFlow, largeIcon);
        }
    }
}
