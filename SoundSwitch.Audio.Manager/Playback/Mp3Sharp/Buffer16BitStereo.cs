// /***************************************************************************
//  * Buffer16BitStereo.cs
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
using MP3Sharp.Decoding;

namespace MP3Sharp {
    /// <summary>
    /// public class used to queue samples that are being obtained from an Mp3 stream. 
    /// This class handles stereo 16-bit output, and can double 16-bit mono to stereo.
    /// </summary>
    public class Buffer16BitStereo : ABuffer {
        internal bool DoubleMonoToStereo = false;

        private const int OUTPUT_CHANNELS = 2;

        // Write offset used in append_bytes
        private readonly byte[] _Buffer = new byte[OBUFFERSIZE * 2]; // all channels interleaved
        private readonly int[] _BufferChannelOffsets = new int[MAXCHANNELS]; // contains write offset for each channel.

        // end marker, one past end of array. Same as bufferp[0], but
        // without the array bounds check.
        private int _End;

        // Read offset used to read from the stream, in bytes.
        private int _Offset;

        internal Buffer16BitStereo() {
            // Initialize the buffer pointers
            ClearBuffer();
        }

        /// <summary>
        /// Gets the number of bytes remaining from the current position on the buffer.
        /// </summary>
        internal int BytesLeft => _End - _Offset;

        /// <summary>
        /// Reads a sequence of bytes from the buffer and advances the position of the 
        /// buffer by the number of bytes read.
        /// </summary>
        /// <returns>
        /// The total number of bytes read in to the buffer. This can be less than the
        /// number of bytes requested if that many bytes are not currently available, or
        /// zero if th eend of the buffer has been reached.
        /// </returns>
        internal int Read(byte[] bufferOut, int offset, int count) {
            if (bufferOut == null) {
                throw new ArgumentNullException(nameof(bufferOut));
            }
            if ((count + offset) > bufferOut.Length) {
                throw new ArgumentException("The sum of offset and count is larger than the buffer length");
            }
            int remaining = BytesLeft;
            int copySize;
            if (count > remaining) {
                copySize = remaining;
            }
            else {
                // Copy an even number of sample frames
                int remainder = count % (2 * OUTPUT_CHANNELS);
                copySize = count - remainder;
            }
            Array.Copy(_Buffer, _Offset, bufferOut, offset, copySize);
            _Offset += copySize;
            return copySize;
        }

        /// <summary>
        /// Writes a single sample value to the buffer.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="sampleValue">The sample value.</param>
        protected override void Append(int channel, short sampleValue) {
            _Buffer[_BufferChannelOffsets[channel]] = (byte)(sampleValue & 0xff);
            _Buffer[_BufferChannelOffsets[channel] + 1] = (byte)(sampleValue >> 8);
            _BufferChannelOffsets[channel] += OUTPUT_CHANNELS * 2;
        }

        /// <summary>
        /// Writes 32 samples to the buffer.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <param name="samples">An array of sample values.</param>
        /// <remarks>
        /// The <paramref name="samples"/> parameter must have a length equal to
        /// or greater than 32.
        /// </remarks>
        internal override void AppendSamples(int channel, float[] samples) {
            if (samples == null) {
                // samples is required.
                throw new ArgumentNullException(nameof(samples));
            }
            if (samples.Length < 32) {
                throw new ArgumentException("samples must have 32 values");
            }
            if (_BufferChannelOffsets == null || channel >= _BufferChannelOffsets.Length) {
                throw new Exception("Song is closing...");
            }
            int pos = _BufferChannelOffsets[channel];
            // Always, 32 samples are appended
            for (int i = 0; i < 32; i++) {
                float fs = samples[i];
                // clamp values
                if (fs > 32767.0f) {
                    fs = 32767.0f;
                }
                else if (fs < -32767.0f) {
                    fs = -32767.0f;
                }
                int sample = (int)fs;
                _Buffer[pos] = (byte)(sample & 0xff);
                _Buffer[pos + 1] = (byte)(sample >> 8);
                if (DoubleMonoToStereo) {
                    _Buffer[pos + 2] = (byte)(sample & 0xff);
                    _Buffer[pos + 3] = (byte)(sample >> 8);
                }
                pos += OUTPUT_CHANNELS * 2;
            }
            _BufferChannelOffsets[channel] = pos;
        }

        /// <summary>
        /// This implementation does not clear the buffer.
        /// </summary>
        internal sealed override void ClearBuffer() {
            _Offset = 0;
            _End = 0;
            for (int i = 0; i < OUTPUT_CHANNELS; i++)
                _BufferChannelOffsets[i] = i * 2; // two bytes per channel
        }

        internal override void SetStopFlag() { }

        internal override void WriteBuffer(int val) {
            _Offset = 0;
            // speed optimization - save end marker, and avoid
            // array access at read time. Can you believe this saves
            // like 1-2% of the cpu on a PIII? I guess allocating
            // that temporary "new int(0)" is expensive, too.
            _End = _BufferChannelOffsets[0];
        }

        internal override void Close() { }
    }
}