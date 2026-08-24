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

using System.IO;

using SoundSwitch.Audio.Manager.Playback;

namespace SoundSwitch.Framework.Audio;

public class CachedSound
{
    public byte[] AudioData { get; }
    public WaveFormat WaveFormat { get; }
    public string FilePath { get; }

    /// <summary>
    /// Load the WAV file into memory.
    /// </summary>
    /// <param name="audioFileName"></param>
    /// <exception cref="CachedSoundFileNotExistsException">Audio file doesn't exist</exception>
    /// <exception cref="InvalidDataException">Not a supported WAV file (only PCM and IEEE float WAV are supported)</exception>
    public CachedSound(string audioFileName)
    {
        if (!File.Exists(audioFileName))
        {
            throw new CachedSoundFileNotExistsException("The audio file doesn't exists");
        }

        FilePath = audioFileName;
        (AudioData, WaveFormat) = WaveFileReader.ReadFile(audioFileName);
    }

    /// <summary>
    /// Decode the WAV from the stream.
    /// </summary>
    /// <param name="stream">A stream containing a WAV file.</param>
    /// <exception cref="InvalidDataException">Not a supported WAV file (only PCM and IEEE float WAV are supported)</exception>
    public CachedSound(Stream stream)
    {
        (AudioData, WaveFormat) = WaveFileReader.Read(stream);
    }
}
