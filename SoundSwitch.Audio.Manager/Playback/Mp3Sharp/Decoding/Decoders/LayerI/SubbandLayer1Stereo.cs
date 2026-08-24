// /***************************************************************************
//  * SubbandLayer1Stereo.cs
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
    /// public class for layer I subbands in stereo mode.
    /// </summary>
    public class SubbandLayer1Stereo : SubbandLayer1 {
        protected int Channel2Allocation;
        protected float Channel2Factor, Channel2Offset;
        protected float Channel2Sample;
        protected int Channel2Samplelength;
        protected float Channel2Scalefactor;

        internal SubbandLayer1Stereo(int subbandnumber)
            : base(subbandnumber) { }

        /// <summary>
        /// *
        /// </summary>
        internal override void ReadAllocation(Bitstream stream, Header header, Crc16 crc) {
            Allocation = stream.GetBitsFromBuffer(4);
            if (Allocation > 14) {
                return;
            }
            Channel2Allocation = stream.GetBitsFromBuffer(4);
            if (crc != null) {
                crc.AddBits(Allocation, 4);
                crc.AddBits(Channel2Allocation, 4);
            }
            if (Allocation != 0) {
                Samplelength = Allocation + 1;
                Factor = TableFactor[Allocation];
                Offset = TableOffset[Allocation];
            }
            if (Channel2Allocation != 0) {
                Channel2Samplelength = Channel2Allocation + 1;
                Channel2Factor = TableFactor[Channel2Allocation];
                Channel2Offset = TableOffset[Channel2Allocation];
            }
        }

        /// <summary>
        /// *
        /// </summary>
        internal override void ReadScaleFactor(Bitstream stream, Header header) {
            if (Allocation != 0)
                Scalefactor = ScaleFactors[stream.GetBitsFromBuffer(6)];
            if (Channel2Allocation != 0)
                Channel2Scalefactor = ScaleFactors[stream.GetBitsFromBuffer(6)];
        }

        /// <summary>
        /// *
        /// </summary>
        internal override bool ReadSampleData(Bitstream stream) {
            bool returnvalue = base.ReadSampleData(stream);
            if (Channel2Allocation != 0) {
                Channel2Sample = stream.GetBitsFromBuffer(Channel2Samplelength);
            }
            return returnvalue;
        }

        /// <summary>
        /// *
        /// </summary>
        internal override bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2) {
            base.PutNextSample(channels, filter1, filter2);
            if (Channel2Allocation != 0 && channels != OutputChannels.LEFT_CHANNEL) {
                float sample2 = (Channel2Sample * Channel2Factor + Channel2Offset) * Channel2Scalefactor;
                if (channels == OutputChannels.BOTH_CHANNELS)
                    filter2.AddSample(sample2, Subbandnumber);
                else
                    filter1.AddSample(sample2, Subbandnumber);
            }
            return true;
        }
    }
}