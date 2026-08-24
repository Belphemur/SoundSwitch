// /***************************************************************************
//  * SubbandLayer2Stereo.cs
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
    /// public class for layer II subbands in stereo mode.
    /// </summary>
    public class SubbandLayer2Stereo : SubbandLayer2 {
        protected int Channel2Allocation;
        protected readonly float[] Channel2C = {0};
        protected readonly int[] Channel2Codelength = {0};
        protected readonly float[] Channel2D = {0};
        protected readonly float[] Channel2Factor = {0};
        protected readonly float[] Channel2Samples;
        protected float Channel2Scalefactor1, Channel2Scalefactor2, Channel2Scalefactor3;
        protected int Channel2Scfsi;

        internal SubbandLayer2Stereo(int subbandnumber)
            : base(subbandnumber) {
            Channel2Samples = new float[3];
        }

        /// <summary>
        /// *
        /// </summary>
        internal override void ReadAllocation(Bitstream stream, Header header, Crc16 crc) {
            int length = GetAllocationLength(header);
            Allocation = stream.GetBitsFromBuffer(length);
            Channel2Allocation = stream.GetBitsFromBuffer(length);
            if (crc != null) {
                crc.AddBits(Allocation, length);
                crc.AddBits(Channel2Allocation, length);
            }
        }

        /// <summary>
        /// *
        /// </summary>
        internal override void ReadScaleFactorSelection(Bitstream stream, Crc16 crc) {
            if (Allocation != 0) {
                Scfsi = stream.GetBitsFromBuffer(2);
                crc?.AddBits(Scfsi, 2);
            }
            if (Channel2Allocation != 0) {
                Channel2Scfsi = stream.GetBitsFromBuffer(2);
                crc?.AddBits(Channel2Scfsi, 2);
            }
        }

        /// <summary>
        /// *
        /// </summary>
        internal override void ReadScaleFactor(Bitstream stream, Header header) {
            base.ReadScaleFactor(stream, header);
            if (Channel2Allocation != 0) {
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
                PrepareForSampleRead(header, Channel2Allocation, 1, Channel2Factor, Channel2Codelength,
                    Channel2C, Channel2D);
            }
        }

        /// <summary>
        /// *
        /// </summary>
        internal override bool ReadSampleData(Bitstream stream) {
            bool returnvalue = base.ReadSampleData(stream);

            if (Channel2Allocation != 0)
                if (Groupingtable[1] != null) {
                    int samplecode = stream.GetBitsFromBuffer(Channel2Codelength[0]);
                    // create requantized samples:
                    samplecode += samplecode << 1;
                    /*
                    float[] target = channel2_samples;
                    float[] source = channel2_groupingtable[0];
                    int tmp = 0;
                    int temp = 0;
                    target[tmp++] = source[samplecode + temp];
                    temp++;
                    target[tmp++] = source[samplecode + temp];
                    temp++;
                    target[tmp] = source[samplecode + temp];
                    // memcpy (channel2_samples, channel2_groupingtable + samplecode, 3 * sizeof (real));
                    */
                    float[] target = Channel2Samples;
                    float[] source = Groupingtable[1];
                    int tmp = 0;
                    int temp = samplecode;
                    target[tmp] = source[temp];
                    temp++;
                    tmp++;
                    target[tmp] = source[temp];
                    temp++;
                    tmp++;
                    target[tmp] = source[temp];
                }
                else {
                    Channel2Samples[0] =
                        (float)(stream.GetBitsFromBuffer(Channel2Codelength[0]) * Channel2Factor[0] - 1.0);
                    Channel2Samples[1] =
                        (float)(stream.GetBitsFromBuffer(Channel2Codelength[0]) * Channel2Factor[0] - 1.0);
                    Channel2Samples[2] =
                        (float)(stream.GetBitsFromBuffer(Channel2Codelength[0]) * Channel2Factor[0] - 1.0);
                }
            return returnvalue;
        }

        /// <summary>
        /// *
        /// </summary>
        internal override bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2) {
            bool returnvalue = base.PutNextSample(channels, filter1, filter2);
            if (Channel2Allocation != 0 && channels != OutputChannels.LEFT_CHANNEL) {
                float sample = Channel2Samples[Samplenumber - 1];

                if (Groupingtable[1] == null)
                    sample = (sample + Channel2D[0]) * Channel2C[0];

                if (Groupnumber <= 4)
                    sample *= Channel2Scalefactor1;
                else if (Groupnumber <= 8)
                    sample *= Channel2Scalefactor2;
                else
                    sample *= Channel2Scalefactor3;
                if (channels == OutputChannels.BOTH_CHANNELS)
                    filter2.AddSample(sample, Subbandnumber);
                else
                    filter1.AddSample(sample, Subbandnumber);
            }
            return returnvalue;
        }
    }
}