// /***************************************************************************
//  * LayerIIDecoder.cs
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

using MP3Sharp.Decoding.Decoders.LayerII;

namespace MP3Sharp.Decoding.Decoders {
    /// <summary>
    /// Implements decoding of MPEG Audio Layer II frames.
    /// </summary>
    public class LayerIIDecoder : LayerIDecoder {
        protected override void CreateSubbands() {
            int i;
            switch (Mode) {
                case Header.SINGLE_CHANNEL: {
                    for (i = 0; i < NuSubbands; ++i)
                        Subbands[i] = new SubbandLayer2(i);
                    break;
                }
                case Header.JOINT_STEREO: {
                    for (i = 0; i < Header.IntensityStereoBound(); ++i)
                        Subbands[i] = new SubbandLayer2Stereo(i);
                    for (; i < NuSubbands; ++i)
                        Subbands[i] = new SubbandLayer2IntensityStereo(i);
                    break;
                }
                default: {
                    for (i = 0; i < NuSubbands; ++i)
                        Subbands[i] = new SubbandLayer2Stereo(i);
                    break;
                }
            }
        }

        protected override void ReadScaleFactorSelection() {
            for (int i = 0; i < NuSubbands; ++i)
                ((SubbandLayer2)Subbands[i]).ReadScaleFactorSelection(Stream, CRC);
        }
    }
}