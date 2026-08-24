// /***************************************************************************
//  * LayerIDecoder.cs
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

using MP3Sharp.Decoding.Decoders.LayerI;

namespace MP3Sharp.Decoding.Decoders {
    /// <summary>
    /// Implements decoding of MPEG Audio Layer I frames.
    /// </summary>
    public class LayerIDecoder : IFrameDecoder {
        protected ABuffer Buffer;
        protected readonly Crc16 CRC;
        protected SynthesisFilter Filter1, Filter2;
        protected Header Header;
        protected int Mode;
        protected int NuSubbands;
        protected Bitstream Stream;
        protected ASubband[] Subbands;

        protected int WhichChannels;
        // new Crc16[1] to enable CRC checking.

        internal LayerIDecoder() {
            CRC = new Crc16();
        }

        public virtual void DecodeFrame() {
            NuSubbands = Header.NumberSubbands();
            Subbands = new ASubband[32];
            Mode = Header.Mode();

            CreateSubbands();

            ReadAllocation();
            ReadScaleFactorSelection();

            if (CRC != null || Header.IsChecksumOK()) {
                ReadScaleFactors();

                ReadSampleData();
            }
        }

        internal virtual void Create(Bitstream stream0, Header header0, SynthesisFilter filtera, SynthesisFilter filterb,
            ABuffer buffer0, int whichCh0) {
            Stream = stream0;
            Header = header0;
            Filter1 = filtera;
            Filter2 = filterb;
            Buffer = buffer0;
            WhichChannels = whichCh0;
        }

        protected virtual void CreateSubbands() {
            int i;
            if (Mode == Header.SINGLE_CHANNEL)
                for (i = 0; i < NuSubbands; ++i)
                    Subbands[i] = new SubbandLayer1(i);
            else if (Mode == Header.JOINT_STEREO) {
                for (i = 0; i < Header.IntensityStereoBound(); ++i)
                    Subbands[i] = new SubbandLayer1Stereo(i);
                for (; i < NuSubbands; ++i)
                    Subbands[i] = new SubbandLayer1IntensityStereo(i);
            }
            else {
                for (i = 0; i < NuSubbands; ++i)
                    Subbands[i] = new SubbandLayer1Stereo(i);
            }
        }

        protected virtual void ReadAllocation() {
            // start to read audio data:
            for (int i = 0; i < NuSubbands; ++i)
                Subbands[i].ReadAllocation(Stream, Header, CRC);
        }

        protected virtual void ReadScaleFactorSelection() {
            // scale factor selection not present for layer I. 
        }

        protected virtual void ReadScaleFactors() {
            for (int i = 0; i < NuSubbands; ++i)
                Subbands[i].ReadScaleFactor(Stream, Header);
        }

        protected virtual void ReadSampleData() {
            bool readReady = false;
            bool writeReady = false;
            int hdrMode = Header.Mode();
            do {
                int i;
                for (i = 0; i < NuSubbands; ++i)
                    readReady = Subbands[i].ReadSampleData(Stream);
                do {
                    for (i = 0; i < NuSubbands; ++i)
                        writeReady = Subbands[i].PutNextSample(WhichChannels, Filter1, Filter2);

                    Filter1.calculate_pc_samples(Buffer);
                    if (WhichChannels == OutputChannels.BOTH_CHANNELS && hdrMode != Header.SINGLE_CHANNEL)
                        Filter2.calculate_pc_samples(Buffer);
                } while (!writeReady);
            } while (!readReady);
        }
    }
}