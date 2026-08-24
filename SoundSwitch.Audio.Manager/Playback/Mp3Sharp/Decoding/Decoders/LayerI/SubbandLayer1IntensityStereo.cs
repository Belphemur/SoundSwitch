// /***************************************************************************
//  * SubbandLayer1IntensityStereo.cs
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
    /// public class for layer I subbands in joint stereo mode.
    /// </summary>
    public class SubbandLayer1IntensityStereo : SubbandLayer1 {
        protected float Channel2Scalefactor;

        internal SubbandLayer1IntensityStereo(int subbandnumber)
            : base(subbandnumber) { }

        /// <summary>
        /// *
        /// </summary>
        internal override void ReadScaleFactor(Bitstream stream, Header header) {
            if (Allocation != 0) {
                Scalefactor = ScaleFactors[stream.GetBitsFromBuffer(6)];
                Channel2Scalefactor = ScaleFactors[stream.GetBitsFromBuffer(6)];
            }
        }

        /// <summary>
        /// *
        /// </summary>
        internal override bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2) {
            if (Allocation != 0) {
                Sample = Sample * Factor + Offset; // requantization
                if (channels == OutputChannels.BOTH_CHANNELS) {
                    float sample1 = Sample * Scalefactor, sample2 = Sample * Channel2Scalefactor;
                    filter1.AddSample(sample1, Subbandnumber);
                    filter2.AddSample(sample2, Subbandnumber);
                }
                else if (channels == OutputChannels.LEFT_CHANNEL) {
                    float sample1 = Sample * Scalefactor;
                    filter1.AddSample(sample1, Subbandnumber);
                }
                else {
                    float sample2 = Sample * Channel2Scalefactor;
                    filter1.AddSample(sample2, Subbandnumber);
                }
            }
            return true;
        }
    }
}