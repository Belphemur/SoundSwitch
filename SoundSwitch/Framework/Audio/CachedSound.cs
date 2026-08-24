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

using SoundSwitch.Audio.Manager.Playback;

namespace SoundSwitch.Framework.Audio;

public class CachedSound
{
    public byte[] AudioData { get; }
    public WaveFormat WaveFormat { get; }
    public string FilePath { get; }

    /// <summary>
    /// Load the audio file (WAV or MP3) into memory.
    /// </summary>
    /// <param name="audioFileName"></param>
    /// <exception cref="CachedSoundFileNotExistsException">Audio file doesn't exist</exception>
    /// <exception cref="InvalidDataException">Not a supported audio file (only PCM/IEEE float WAV and MP3 are supported)</exception>
    public CachedSound(string audioFileName)
    {
        if (!File.Exists(audioFileName))
        {
            throw new CachedSoundFileNotExistsException("The audio file doesn't exists");
        }

        FilePath = audioFileName;
        (AudioData, WaveFormat) = IsMp3(audioFileName)
            ? Mp3FileReader.ReadFile(audioFileName)
            : WaveFileReader.ReadFile(audioFileName);
    }

    /// <summary>
    /// Decode the audio (WAV or MP3) from the stream.
    /// </summary>
    /// <param name="stream">A stream containing a WAV or an MP3 file.</param>
    /// <exception cref="InvalidDataException">Not a supported audio file (only PCM/IEEE float WAV and MP3 are supported)</exception>
    public CachedSound(Stream stream)
    {
        // Buffer the stream so the format can be sniffed from the start regardless of
        // the stream's position, and so the WAV reader sees a rewound stream.
        using var buffered = new MemoryStream();
        stream.CopyTo(buffered);
        var data = buffered.ToArray();

        (AudioData, WaveFormat) = IsMp3Data(data)
            ? Mp3FileReader.Read(new MemoryStream(data, writable: false))
            : WaveFileReader.Read(new MemoryStream(data, writable: false));
    }

    private static bool IsMp3(string path)
    {
        if (path.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
            return true;

        // Not obviously an MP3 by name: sniff the header — an MP3 may carry any
        // extension, and conversely a mislabeled file should fall through to the WAV
        // reader so its (more precise) error surfaces.
        try
        {
            using var stream = File.OpenRead(path);
            var header = new byte[3];
            var read = stream.Read(header, 0, header.Length);
            return IsMp3Data(header.AsSpan(0, read));
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>
    /// Detects MP3 content: an ID3v2 tag ("ID3") or a raw MPEG audio frame sync
    /// (0xFF + frame header whose version isn't reserved and whose layer is III).
    /// </summary>
    private static bool IsMp3Data(ReadOnlySpan<byte> header)
    {
        if (header.Length >= 3 && header[0] == 0x49 && header[1] == 0x44 && header[2] == 0x33)
            return true; // "ID3"

        return header.Length >= 2
               && header[0] == 0xFF
               && (header[1] & 0xE0) == 0xE0 // frame sync
               && ((header[1] >> 3) & 0x3) != 1 // MPEG version: not reserved
               && ((header[1] >> 1) & 0x3) == 1; // layer III
    }
}
