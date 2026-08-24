// /***************************************************************************
//  * SubbandLayer1.cs
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

namespace MP3Sharp.Decoding.Decoders.LayerI {
    /// <summary>
    /// public class for layer I subbands in single channel mode.
    /// Used for single channel mode
    /// and in derived class for intensity stereo mode
    /// </summary>
    public class SubbandLayer1 : ASubband {
        // Factors and offsets for sample requantization
        internal static readonly float[] TableFactor = {
            0.0f, 1.0f / 2.0f * (4.0f / 3.0f), 1.0f / 4.0f * (8.0f / 7.0f), 1.0f / 8.0f * (16.0f / 15.0f),
            1.0f / 16.0f * (32.0f / 31.0f), 1.0f / 32.0f * (64.0f / 63.0f), 1.0f / 64.0f * (128.0f / 127.0f),
            1.0f / 128.0f * (256.0f / 255.0f), 1.0f / 256.0f * (512.0f / 511.0f), 1.0f / 512.0f * (1024.0f / 1023.0f),
            1.0f / 1024.0f * (2048.0f / 2047.0f), 1.0f / 2048.0f * (4096.0f / 4095.0f), 1.0f / 4096.0f * (8192.0f / 8191.0f),
            1.0f / 8192.0f * (16384.0f / 16383.0f), 1.0f / 16384.0f * (32768.0f / 32767.0f)
        };

        internal static readonly float[] TableOffset = {
            0.0f, (1.0f / 2.0f - 1.0f) * (4.0f / 3.0f), (1.0f / 4.0f - 1.0f) * (8.0f / 7.0f),
            (1.0f / 8.0f - 1.0f) * (16.0f / 15.0f), (1.0f / 16.0f - 1.0f) * (32.0f / 31.0f),
            (1.0f / 32.0f - 1.0f) * (64.0f / 63.0f), (1.0f / 64.0f - 1.0f) * (128.0f / 127.0f),
            (1.0f / 128.0f - 1.0f) * (256.0f / 255.0f), (1.0f / 256.0f - 1.0f) * (512.0f / 511.0f),
            (1.0f / 512.0f - 1.0f) * (1024.0f / 1023.0f), (1.0f / 1024.0f - 1.0f) * (2048.0f / 2047.0f),
            (1.0f / 2048.0f - 1.0f) * (4096.0f / 4095.0f), (1.0f / 4096.0f - 1.0f) * (8192.0f / 8191.0f),
            (1.0f / 8192.0f - 1.0f) * (16384.0f / 16383.0f), (1.0f / 16384.0f - 1.0f) * (32768.0f / 32767.0f)
        };

        protected int Allocation;
        protected float Factor, Offset;
        protected float Sample;
        protected int Samplelength;
        protected int Samplenumber;
        protected float Scalefactor;
        protected readonly int Subbandnumber;

        /// <summary>
        /// Construtor.
        /// </summary>
        internal SubbandLayer1(int subbandnumber) {
            Subbandnumber = subbandnumber;
            Samplenumber = 0;
        }

        /// <summary>
        /// *
        /// </summary>
        internal override void ReadAllocation(Bitstream stream, Header header, Crc16 crc) {
            if ((Allocation = stream.GetBitsFromBuffer(4)) == 15) { }
            // cerr << "WARNING: stream contains an illegal allocation!\n";
            // MPEG-stream is corrupted!
            crc?.AddBits(Allocation, 4);
            if (Allocation != 0) {
                Samplelength = Allocation + 1;
                Factor = TableFactor[Allocation];
                Offset = TableOffset[Allocation];
            }
        }

        /// <summary>
        /// *
        /// </summary>
        internal override void ReadScaleFactor(Bitstream stream, Header header) {
            if (Allocation != 0)
                Scalefactor = ScaleFactors[stream.GetBitsFromBuffer(6)];
        }

        /// <summary>
        /// *
        /// </summary>
        internal override bool ReadSampleData(Bitstream stream) {
            if (Allocation != 0) {
                Sample = stream.GetBitsFromBuffer(Samplelength);
            }
            if (++Samplenumber == 12) {
                Samplenumber = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// *
        /// </summary>
        internal override bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2) {
            if (Allocation != 0 && channels != OutputChannels.RIGHT_CHANNEL) {
                float scaledSample = (Sample * Factor + Offset) * Scalefactor;
                filter1.AddSample(scaledSample, Subbandnumber);
            }
            return true;
        }
    }
}