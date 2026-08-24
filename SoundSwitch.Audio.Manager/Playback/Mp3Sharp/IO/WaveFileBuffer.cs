// /***************************************************************************
//  * WaveFileBuffer.cs
//  * Copyright (c) 2015, 2021 The Authors.
//  * 
//  * All rights reserved. This program and the accompanying materials
//  * are made available under the terms of the GNU Lesser General Public License
//  * (LGPL) version 3 which accompanies this distribution, and is available at
//  * https://www.gnu.org/licenses/lgpl-3.0.en.html
//  *
//  * This library is distributed in the hope that it will be useful,
//  * but WITHOUT ANY WARRANTY; without even the implied warranty of
//  * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//  * Lesser General Public License for more details.
//  *
//  ***************************************************************************/

using System;
using System.IO;
using MP3Sharp.Decoding;

namespace MP3Sharp.IO {
    /// <summary> Implements an Obuffer by writing the data to a file in RIFF WAVE format.</summary>
    public class WaveFileBuffer : ABuffer {
        private readonly short[] _Buffer;
        private readonly short[] _Bufferp;
        private readonly int _Channels;
        private readonly WaveFile _OutWave;

        internal WaveFileBuffer(int numberOfChannels, int freq, string fileName) {
            if (fileName == null)
                throw new NullReferenceException("FileName");

            _Buffer = new short[OBUFFERSIZE];
            _Bufferp = new short[MAXCHANNELS];
            _Channels = numberOfChannels;

            for (int i = 0; i < numberOfChannels; ++i)
                _Bufferp[i] = (short)i;

            _OutWave = new WaveFile();

            int rc = _OutWave.OpenForWrite(fileName, null, freq, 16, (short)_Channels);
        }

        internal WaveFileBuffer(int numberOfChannels, int freq, Stream stream) {
            _Buffer = new short[OBUFFERSIZE];
            _Bufferp = new short[MAXCHANNELS];
            _Channels = numberOfChannels;

            for (int i = 0; i < numberOfChannels; ++i)
                _Bufferp[i] = (short)i;

            _OutWave = new WaveFile();

            int rc = _OutWave.OpenForWrite(null, stream, freq, 16, (short)_Channels);
        }

        /// <summary>
        /// Takes a 16 Bit PCM sample.
        /// </summary>
        protected override void Append(int channel, short valueRenamed) {
            _Buffer[_Bufferp[channel]] = valueRenamed;
            _Bufferp[channel] = (short)(_Bufferp[channel] + _Channels);
        }

        internal override void WriteBuffer(int val) {
            int rc = _OutWave.WriteData(_Buffer, _Bufferp[0]);
            for (int i = 0; i < _Channels; ++i)
                _Bufferp[i] = (short)i;
        }

        internal void Close(bool justWriteLengthBytes) {
            _OutWave.Close(justWriteLengthBytes);
        }

        internal override void Close() {
            _OutWave.Close();
        }

        /// <summary>
        /// *
        /// </summary>
        internal override void ClearBuffer() { }

        /// <summary>
        /// *
        /// </summary>
        internal override void SetStopFlag() { }
    }
}