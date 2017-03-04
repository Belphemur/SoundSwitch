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
using NAudio.Wave;

namespace SoundSwitch.Framework.Audio
{
    public class CachedSoundWaveStream : WaveStream
    {
        private readonly CachedSound _cachedSound;

        public CachedSoundWaveStream(CachedSound cachedSound)
        {
            this._cachedSound = cachedSound;
        }

        public override WaveFormat WaveFormat => _cachedSound.WaveFormat;
        public override int Read(byte[] buffer, int offset, int count)
        {
            var availableSamples = _cachedSound.AudioData.Length - Position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(_cachedSound.AudioData, Position, buffer, offset, samplesToCopy);
            Position += samplesToCopy;
            return (int)samplesToCopy;
        }

        public override long Length => _cachedSound.AudioData.Length;
        public override long Position { get; set; }
    }
}