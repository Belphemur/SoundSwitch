// /***************************************************************************
//  * SampleBuffer.cs
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

namespace MP3Sharp.Decoding {
    /// <summary>
    /// The SampleBuffer class implements an output buffer
    /// that provides storage for a fixed size block of samples.
    /// </summary>
    public class SampleBuffer : ABuffer {
        private readonly short[] _Buffer;
        private readonly int[] _Bufferp;
        private readonly int _Channels;
        private readonly int _Frequency;

        internal SampleBuffer(int sampleFrequency, int numberOfChannels) {
            _Buffer = new short[OBUFFERSIZE];
            _Bufferp = new int[MAXCHANNELS];
            _Channels = numberOfChannels;
            _Frequency = sampleFrequency;

            for (int i = 0; i < numberOfChannels; ++i)
                _Bufferp[i] = (short)i;
        }

        internal virtual int ChannelCount => _Channels;

        internal virtual int SampleFrequency => _Frequency;

        internal virtual short[] Buffer => _Buffer;

        internal virtual int BufferLength => _Bufferp[0];

        /// <summary>
        /// Takes a 16 Bit PCM sample.
        /// </summary>
        protected override void Append(int channel, short valueRenamed) {
            _Buffer[_Bufferp[channel]] = valueRenamed;
            _Bufferp[channel] += _Channels;
        }

        internal override void AppendSamples(int channel, float[] samples) {
            int pos = _Bufferp[channel];

            short s;
            float fs;
            for (int i = 0; i < 32;) {
                fs = samples[i++];
                fs = fs > 32767.0f ? 32767.0f : fs < -32767.0f ? -32767.0f : fs;

                //UPGRADE_WARNING: Narrowing conversions may produce unexpected results in C#. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1042"'
                s = (short)fs;
                _Buffer[pos] = s;
                pos += _Channels;
            }

            _Bufferp[channel] = pos;
        }

        /// <summary>
        /// Write the samples to the file (Random Acces).
        /// </summary>
        internal override void WriteBuffer(int val) {
            // for (int i = 0; i < channels; ++i) 
            // bufferp[i] = (short)i;
        }

        internal override void Close() { }

        /// <summary>
        /// *
        /// </summary>
        internal override void ClearBuffer() {
            for (int i = 0; i < _Channels; ++i)
                _Bufferp[i] = (short)i;
        }

        /// <summary>
        /// *
        /// </summary>
        internal override void SetStopFlag() { }
    }
}