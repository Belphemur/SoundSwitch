// /***************************************************************************
//  * Decoder.cs
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
using MP3Sharp.Decoding.Decoders;

namespace MP3Sharp.Decoding {
    /// <summary>
    /// Encapsulates the details of decoding an MPEG audio frame.
    /// </summary>
    public class Decoder {
        private static readonly Params DecoderDefaultParams = new Params();
        private Equalizer _Equalizer;

        private SynthesisFilter _LeftChannelFilter;
        private SynthesisFilter _RightChannelFilter;

        private bool _IsInitialized;
        private LayerIDecoder _L1Decoder;
        private LayerIIDecoder _L2Decoder;
        private LayerIIIDecoder _L3Decoder;

        private ABuffer _Output;

        private int _OutputChannels;
        private int _OutputFrequency;

        /// <summary>
        /// Creates a new Decoder instance with default parameters.
        /// </summary>
        internal Decoder() : this(null) {
            InitBlock();
        }

        /// <summary>
        /// Creates a new Decoder instance with custom parameters.
        /// </summary>
        internal Decoder(Params params0) {
            InitBlock();
            if (params0 == null) {
                params0 = DecoderDefaultParams;
            }
            Equalizer eq = params0.InitialEqualizerSettings;
            if (eq != null) {
                _Equalizer.FromEqualizer = eq;
            }
        }

        internal static Params DefaultParams => (Params)DecoderDefaultParams.Clone();

        internal virtual Equalizer Equalizer {
            set {
                if (value == null) {
                    value = Equalizer.PassThruEq;
                }
                _Equalizer.FromEqualizer = value;
                float[] factors = _Equalizer.BandFactors;
                if (_LeftChannelFilter != null)
                    _LeftChannelFilter.Eq = factors;

                if (_RightChannelFilter != null)
                    _RightChannelFilter.Eq = factors;
            }
        }

        /// <summary>
        /// Changes the output buffer. This will take effect the next time
        /// decodeFrame() is called.
        /// </summary>
        internal virtual ABuffer OutputBuffer {
            set => _Output = value;
        }

        /// <summary>
        /// Retrieves the sample frequency of the PCM samples output
        /// by this decoder. This typically corresponds to the sample
        /// rate encoded in the MPEG audio stream.
        /// </summary>
        internal virtual int OutputFrequency => _OutputFrequency;

        /// <summary>
        /// Retrieves the number of channels of PCM samples output by
        /// this decoder. This usually corresponds to the number of
        /// channels in the MPEG audio stream.
        /// </summary>
        internal virtual int OutputChannels => _OutputChannels;

        /// <summary>
        /// Retrieves the maximum number of samples that will be written to
        /// the output buffer when one frame is decoded. This can be used to
        /// help calculate the size of other buffers whose size is based upon
        /// the number of samples written to the output buffer. NB: this is
        /// an upper bound and fewer samples may actually be written, depending
        /// upon the sample rate and number of channels.
        /// </summary>
        internal virtual int OutputBlockSize => ABuffer.OBUFFERSIZE;

        private void InitBlock() {
            _Equalizer = new Equalizer();
        }

        /// <summary>
        /// Decodes one frame from an MPEG audio bitstream.
        /// </summary>
        /// <param name="header">
        /// Header describing the frame to decode.
        /// </param>
        /// <param name="stream">
        /// Bistream that provides the bits for the body of the frame.
        /// </param>
        /// <returns>
        /// A SampleBuffer containing the decoded samples.
        /// </returns>
        internal virtual ABuffer DecodeFrame(Header header, Bitstream stream) {
            if (!_IsInitialized) {
                Initialize(header);
            }
            int layer = header.Layer();
            _Output.ClearBuffer();
            IFrameDecoder decoder = RetrieveDecoder(header, stream, layer);
            decoder.DecodeFrame();
            _Output.WriteBuffer(1);
            return _Output;
        }

        protected virtual DecoderException NewDecoderException(int errorcode) => new DecoderException(errorcode, null);

        protected virtual DecoderException NewDecoderException(int errorcode, Exception throwable) => new DecoderException(errorcode, throwable);

        protected virtual IFrameDecoder RetrieveDecoder(Header header, Bitstream stream, int layer) {
            IFrameDecoder decoder = null;

            // REVIEW: allow channel output selection type
            // (LEFT, RIGHT, BOTH, DOWNMIX)
            switch (layer) {
                case 3:
                    if (_L3Decoder == null) {
                        _L3Decoder = new LayerIIIDecoder(stream, header, _LeftChannelFilter, _RightChannelFilter, _Output,
                            (int)OutputChannelsEnum.BothChannels);
                    }

                    decoder = _L3Decoder;
                    break;

                case 2:
                    if (_L2Decoder == null) {
                        _L2Decoder = new LayerIIDecoder();
                        _L2Decoder.Create(stream, header, _LeftChannelFilter, _RightChannelFilter, _Output,
                            (int)OutputChannelsEnum.BothChannels);
                    }
                    decoder = _L2Decoder;
                    break;

                case 1:
                    if (_L1Decoder == null) {
                        _L1Decoder = new LayerIDecoder();
                        _L1Decoder.Create(stream, header, _LeftChannelFilter, _RightChannelFilter, _Output,
                            (int)OutputChannelsEnum.BothChannels);
                    }
                    decoder = _L1Decoder;
                    break;
            }

            if (decoder == null) {
                throw NewDecoderException(DecoderErrors.UNSUPPORTED_LAYER, null);
            }

            return decoder;
        }

        private void Initialize(Header header) {
            // REVIEW: allow customizable scale factor
            const float scalefactor = 32700.0f;
            int channels = header.Mode() == Header.SINGLE_CHANNEL ? 1 : 2;

            // set up output buffer if not set up by client.
            if (_Output == null)
                _Output = new SampleBuffer(header.Frequency(), channels);

            float[] factors = _Equalizer.BandFactors;
            _LeftChannelFilter = new SynthesisFilter(0, scalefactor, factors);
            if (channels == 2)
                _RightChannelFilter = new SynthesisFilter(1, scalefactor, factors);

            _OutputChannels = channels;
            _OutputFrequency = header.Frequency();

            _IsInitialized = true;
        }

        /// <summary>
        /// The Params class presents the customizable
        /// aspects of the decoder. Instances of this class are not thread safe.
        /// </summary>
        public class Params : ICloneable {
            private OutputChannels _OutputChannels;

            internal virtual OutputChannels OutputChannels {
                get => _OutputChannels;

                set => _OutputChannels = value ?? throw new NullReferenceException("out");
            }

            /// <summary>
            /// Retrieves the equalizer settings that the decoder's equalizer
            /// will be initialized from.
            /// The Equalizer instance returned
            /// cannot be changed in real time to affect the
            /// decoder output as it is used only to initialize the decoders
            /// EQ settings. To affect the decoder's output in realtime,
            /// use the Equalizer returned from the getEqualizer() method on
            /// the decoder.
            /// </summary>
            /// <returns>
            /// The Equalizer used to initialize the
            /// EQ settings of the decoder.
            /// </returns>
            private readonly Equalizer _Equalizer = null;

            internal virtual Equalizer InitialEqualizerSettings => _Equalizer;

            public object Clone() {
                try {
                    return MemberwiseClone();
                }
                catch (Exception ex) {
                    throw new ApplicationException(this + ": " + ex);
                }
            }
        }
    }
}