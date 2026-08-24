// /***************************************************************************
//  * SubbandLayer2IntensityStereo.cs
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

namespace MP3Sharp.Decoding.Decoders.LayerII {
    /// <summary>
    /// public class for layer II subbands in joint stereo mode.
    /// </summary>
    public class SubbandLayer2IntensityStereo : SubbandLayer2 {
        protected float Channel2Scalefactor1, Channel2Scalefactor2, Channel2Scalefactor3;
        protected int Channel2Scfsi;

        internal SubbandLayer2IntensityStereo(int subbandnumber)
            : base(subbandnumber) { }

        /// <summary>
        /// *
        /// </summary>
        internal override void ReadScaleFactorSelection(Bitstream stream, Crc16 crc) {
            if (Allocation != 0) {
                Scfsi = stream.GetBitsFromBuffer(2);
                Channel2Scfsi = stream.GetBitsFromBuffer(2);
                if (crc != null) {
                    crc.AddBits(Scfsi, 2);
                    crc.AddBits(Channel2Scfsi, 2);
                }
            }
        }

        /// <summary>
        /// *
        /// </summary>
        internal override void ReadScaleFactor(Bitstream stream, Header header) {
            if (Allocation != 0) {
                base.ReadScaleFactor(stream, header);
                switch (Channel2Scfsi) {
                    case 0:
                        Channel2Scalefactor1 = ScaleFactors[stream.GetBitsFromBuffer(6)];
                        Channel2Scalefactor2 = ScaleFactors[stream.GetBitsFromBuffer(6)];
                        Channel2Scalefactor3 = ScaleFactors[stream.GetBitsFromBuffer(6)];
                        break;

                    case 1:
                        Channel2Scalefactor1 = Channel2Scalefactor2 = ScaleFactors[stream.GetBitsFromBuffer(6)];
                        Channel2Scalefactor3 = ScaleFactors[stream.GetBitsFromBuffer(6)];
                        break;

                    case 2:
                        Channel2Scalefactor1 =
                            Channel2Scalefactor2 = Channel2Scalefactor3 = ScaleFactors[stream.GetBitsFromBuffer(6)];
                        break;

                    case 3:
                        Channel2Scalefactor1 = ScaleFactors[stream.GetBitsFromBuffer(6)];
                        Channel2Scalefactor2 = Channel2Scalefactor3 = ScaleFactors[stream.GetBitsFromBuffer(6)];
                        break;
                }
            }
        }

        /// <summary>
        /// *
        /// </summary>
        internal override bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2) {
            if (Allocation != 0) {
                float sample = Samples[Samplenumber];

                if (Groupingtable[0] == null)
                    sample = (sample + D[0]) * CFactor[0];
                if (channels == OutputChannels.BOTH_CHANNELS) {
                    float sample2 = sample;
                    if (Groupnumber <= 4) {
                        sample *= Scalefactor1;
                        sample2 *= Channel2Scalefactor1;
                    }
                    else if (Groupnumber <= 8) {
                        sample *= Scalefactor2;
                        sample2 *= Channel2Scalefactor2;
                    }
                    else {
                        sample *= Scalefactor3;
                        sample2 *= Channel2Scalefactor3;
                    }
                    filter1.AddSample(sample, Subbandnumber);
                    filter2.AddSample(sample2, Subbandnumber);
                }
                else if (channels == OutputChannels.LEFT_CHANNEL) {
                    if (Groupnumber <= 4)
                        sample *= Scalefactor1;
                    else if (Groupnumber <= 8)
                        sample *= Scalefactor2;
                    else
                        sample *= Scalefactor3;
                    filter1.AddSample(sample, Subbandnumber);
                }
                else {
                    if (Groupnumber <= 4)
                        sample *= Channel2Scalefactor1;
                    else if (Groupnumber <= 8)
                        sample *= Channel2Scalefactor2;
                    else
                        sample *= Channel2Scalefactor3;
                    filter1.AddSample(sample, Subbandnumber);
                }
            }

            if (++Samplenumber == 3)
                return true;
            return false;
        }
    }
}