// /***************************************************************************
//  * LayerIIIDecoder.cs
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
using MP3Sharp.Decoding.Decoders.LayerIII;
using MP3Sharp.Support;

namespace MP3Sharp.Decoding.Decoders {
    /// <summary>
    /// Implements decoding of MPEG Audio Layer 3 frames.
    /// </summary>
    internal sealed class LayerIIIDecoder : IFrameDecoder {
        private const int SSLIMIT = 18;
        private const int SBLIMIT = 32;

        private static readonly int[][] Slen = {
            new[] {0, 0, 0, 0, 3, 1, 1, 1, 2, 2, 2, 3, 3, 3, 4, 4},
            new[] {0, 1, 2, 3, 0, 1, 2, 3, 1, 2, 3, 1, 2, 3, 2, 3}
        };

        internal static readonly int[] Pretab = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 3, 3, 3, 2, 0};

        internal static readonly float[] TwoToNegativeHalfPow = {
            1.0000000000e+00f, 7.0710678119e-01f, 5.0000000000e-01f, 3.5355339059e-01f, 2.5000000000e-01f,
            1.7677669530e-01f, 1.2500000000e-01f, 8.8388347648e-02f, 6.2500000000e-02f, 4.4194173824e-02f,
            3.1250000000e-02f, 2.2097086912e-02f, 1.5625000000e-02f, 1.1048543456e-02f, 7.8125000000e-03f,
            5.5242717280e-03f, 3.9062500000e-03f, 2.7621358640e-03f, 1.9531250000e-03f, 1.3810679320e-03f,
            9.7656250000e-04f, 6.9053396600e-04f, 4.8828125000e-04f, 3.4526698300e-04f, 2.4414062500e-04f,
            1.7263349150e-04f, 1.2207031250e-04f, 8.6316745750e-05f, 6.1035156250e-05f, 4.3158372875e-05f,
            3.0517578125e-05f, 2.1579186438e-05f, 1.5258789062e-05f, 1.0789593219e-05f, 7.6293945312e-06f,
            5.3947966094e-06f, 3.8146972656e-06f, 2.6973983047e-06f, 1.9073486328e-06f, 1.3486991523e-06f,
            9.5367431641e-07f, 6.7434957617e-07f, 4.7683715820e-07f, 3.3717478809e-07f, 2.3841857910e-07f,
            1.6858739404e-07f, 1.1920928955e-07f, 8.4293697022e-08f, 5.9604644775e-08f, 4.2146848511e-08f,
            2.9802322388e-08f, 2.1073424255e-08f, 1.4901161194e-08f, 1.0536712128e-08f, 7.4505805969e-09f,
            5.2683560639e-09f, 3.7252902985e-09f, 2.6341780319e-09f, 1.8626451492e-09f, 1.3170890160e-09f,
            9.3132257462e-10f, 6.5854450798e-10f, 4.6566128731e-10f, 3.2927225399e-10f
        };

        internal static readonly float[] PowerTable;

        internal static readonly float[][] Io = {
            new[] {
                1.0000000000e+00f, 8.4089641526e-01f, 7.0710678119e-01f, 5.9460355751e-01f, 5.0000000001e-01f,
                4.2044820763e-01f, 3.5355339060e-01f, 2.9730177876e-01f, 2.5000000001e-01f, 2.1022410382e-01f,
                1.7677669530e-01f, 1.4865088938e-01f, 1.2500000000e-01f, 1.0511205191e-01f, 8.8388347652e-02f,
                7.4325444691e-02f, 6.2500000003e-02f, 5.2556025956e-02f, 4.4194173826e-02f, 3.7162722346e-02f,
                3.1250000002e-02f, 2.6278012978e-02f, 2.2097086913e-02f, 1.8581361173e-02f, 1.5625000001e-02f,
                1.3139006489e-02f, 1.1048543457e-02f, 9.2906805866e-03f, 7.8125000006e-03f, 6.5695032447e-03f,
                5.5242717285e-03f, 4.6453402934e-03f
            },
            new[] {
                1.0000000000e+00f, 7.0710678119e-01f, 5.0000000000e-01f, 3.5355339060e-01f, 2.5000000000e-01f,
                1.7677669530e-01f, 1.2500000000e-01f, 8.8388347650e-02f, 6.2500000001e-02f, 4.4194173825e-02f,
                3.1250000001e-02f, 2.2097086913e-02f, 1.5625000000e-02f, 1.1048543456e-02f, 7.8125000002e-03f,
                5.5242717282e-03f, 3.9062500001e-03f, 2.7621358641e-03f, 1.9531250001e-03f, 1.3810679321e-03f,
                9.7656250004e-04f, 6.9053396603e-04f, 4.8828125002e-04f, 3.4526698302e-04f, 2.4414062501e-04f,
                1.7263349151e-04f, 1.2207031251e-04f, 8.6316745755e-05f, 6.1035156254e-05f, 4.3158372878e-05f,
                3.0517578127e-05f, 2.1579186439e-05f
            }
        };

        internal static readonly float[] Tan12 = {
            0.0f, 0.26794919f, 0.57735027f, 1.0f, 1.73205081f, 3.73205081f, 9.9999999e10f, -3.73205081f, -1.73205081f,
            -1.0f, -0.57735027f, -0.26794919f, 0.0f, 0.26794919f, 0.57735027f, 1.0f
        };

        private static int[][] _reorderTable; // Generated on demand

        private static readonly float[] Cs = {
            0.857492925712f, 0.881741997318f, 0.949628649103f, 0.983314592492f, 0.995517816065f, 0.999160558175f,
            0.999899195243f, 0.999993155067f
        };

        private static readonly float[] Ca = {
            -0.5144957554270f, -0.4717319685650f, -0.3133774542040f, -0.1819131996110f, -0.0945741925262f,
            -0.0409655828852f, -0.0141985685725f, -0.00369997467375f
        };

        internal static readonly float[][] Win = {
            new[] {
                -1.6141214951e-02f, -5.3603178919e-02f, -1.0070713296e-01f, -1.6280817573e-01f, -4.9999999679e-01f,
                -3.8388735032e-01f, -6.2061144372e-01f, -1.1659756083e+00f, -3.8720752656e+00f, -4.2256286556e+00f,
                -1.5195289984e+00f, -9.7416483388e-01f, -7.3744074053e-01f, -1.2071067773e+00f, -5.1636156596e-01f,
                -4.5426052317e-01f, -4.0715656898e-01f, -3.6969460527e-01f, -3.3876269197e-01f, -3.1242222492e-01f,
                -2.8939587111e-01f, -2.6880081906e-01f, -5.0000000266e-01f, -2.3251417468e-01f, -2.1596714708e-01f,
                -2.0004979098e-01f, -1.8449493497e-01f, -1.6905846094e-01f, -1.5350360518e-01f, -1.3758624925e-01f,
                -1.2103922149e-01f, -2.0710679058e-01f, -8.4752577594e-02f, -6.4157525656e-02f, -4.1131172614e-02f,
                -1.4790705759e-02f
            },
            new[] {
                -1.6141214951e-02f, -5.3603178919e-02f, -1.0070713296e-01f, -1.6280817573e-01f, -4.9999999679e-01f,
                -3.8388735032e-01f, -6.2061144372e-01f, -1.1659756083e+00f, -3.8720752656e+00f, -4.2256286556e+00f,
                -1.5195289984e+00f, -9.7416483388e-01f, -7.3744074053e-01f, -1.2071067773e+00f, -5.1636156596e-01f,
                -4.5426052317e-01f, -4.0715656898e-01f, -3.6969460527e-01f, -3.3908542600e-01f, -3.1511810350e-01f,
                -2.9642226150e-01f, -2.8184548650e-01f, -5.4119610000e-01f, -2.6213228100e-01f, -2.5387916537e-01f,
                -2.3296291359e-01f, -1.9852728987e-01f, -1.5233534808e-01f, -9.6496400054e-02f, -3.3423828516e-02f,
                0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f,
                0.0000000000e+00f
            },
            new[] {
                -4.8300800645e-02f, -1.5715656932e-01f, -2.8325045177e-01f, -4.2953747763e-01f, -1.2071067795e+00f,
                -8.2426483178e-01f, -1.1451749106e+00f, -1.7695290101e+00f, -4.5470225061e+00f, -3.4890531002e+00f,
                -7.3296292804e-01f, -1.5076514758e-01f, 0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f,
                0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f,
                0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f,
                0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f,
                0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f,
                0.0000000000e+00f
            },
            new[] {
                0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f, 0.0000000000e+00f,
                0.0000000000e+00f, -1.5076513660e-01f, -7.3296291107e-01f, -3.4890530566e+00f, -4.5470224727e+00f,
                -1.7695290031e+00f, -1.1451749092e+00f, -8.3137738100e-01f, -1.3065629650e+00f, -5.4142014250e-01f,
                -4.6528974900e-01f, -4.1066990750e-01f, -3.7004680800e-01f, -3.3876269197e-01f, -3.1242222492e-01f,
                -2.8939587111e-01f, -2.6880081906e-01f, -5.0000000266e-01f, -2.3251417468e-01f, -2.1596714708e-01f,
                -2.0004979098e-01f,
                -1.8449493497e-01f, -1.6905846094e-01f, -1.5350360518e-01f, -1.3758624925e-01f, -1.2103922149e-01f,
                -2.0710679058e-01f, -8.4752577594e-02f, -6.4157525656e-02f, -4.1131172614e-02f, -1.4790705759e-02f
            }
        };

        internal static readonly int[][][] NrOfSfbBlock = {
            new[] {new[] {6, 5, 5, 5}, new[] {9, 9, 9, 9}, new[] {6, 9, 9, 9}},
            new[] {new[] {6, 5, 7, 3}, new[] {9, 9, 12, 6}, new[] {6, 9, 12, 6}},
            new[] {new[] {11, 10, 0, 0}, new[] {18, 18, 0, 0}, new[] {15, 18, 0, 0}},
            new[] {new[] {7, 7, 7, 0}, new[] {12, 12, 12, 0}, new[] {6, 15, 12, 0}},
            new[] {new[] {6, 6, 6, 3}, new[] {12, 9, 9, 6}, new[] {6, 12, 9, 6}},
            new[] {new[] {8, 8, 5, 0}, new[] {15, 12, 9, 0}, new[] {6, 18, 9, 0}}
        };

        private readonly ABuffer _Buffer;
        private readonly int _Channels;
        private readonly SynthesisFilter _Filter1;
        private readonly SynthesisFilter _Filter2;
        private readonly int _FirstChannel;
        private readonly Header _Header;
        private readonly int[] _Is1D;
        private readonly float[][] _K;
        private readonly int _LastChannel;
        private readonly float[][][] _Lr;

        private readonly int _MaxGr;
        private readonly int[] _Nonzero;
        private readonly float[] _Out1D;
        private readonly float[][] _Prevblck;
        private readonly float[][][] _Ro;
        private readonly ScaleFactorData[] _Scalefac;
        private readonly SBI[] _SfBandIndex; // Init in the ctor.
        private readonly int _Sfreq;
        private readonly Layer3SideInfo _SideInfo;
        private readonly Bitstream _Stream;
        private readonly int _WhichChannels;
        private BitReserve _BitReserve;

        private int _CheckSumHuff;
        private int _FrameStart;

        internal int[] IsPos;

        internal float[] IsRatio;

        // MDM: new_slen is fully initialized before use, no need
        // to reallocate array.
        private int[] _NewSlen;

        private int _Part2Start;
        internal float[] Rawout;

        // subband samples are buffered and passed to the
        // SynthesisFilter in one go.
        private float[] _Samples1;

        private float[] _Samples2;
        internal int[] ScalefacBuffer;
        internal ScaleFactorTable Sftable;

        // MDM: tsOutCopy and rawout do not need initializing, so the arrays
        // can be reused.
        internal float[] TsOutCopy;

        internal int[] V = {0};
        internal int[] W = {0};
        internal int[] X = {0};
        internal int[] Y = {0};

        static LayerIIIDecoder() {
            PowerTable = CreatePowerTable();
        }

        /// <summary>
        /// REVIEW: these ctor arguments should be moved to the decodeFrame() method.
        /// </summary>
        internal LayerIIIDecoder(Bitstream stream, Header header, SynthesisFilter filtera, SynthesisFilter filterb, ABuffer buffer, int whichCh) {
            Huffman.Initialize();

            InitBlock();
            _Is1D = new int[SBLIMIT * SSLIMIT + 4];
            _Ro = new float[2][][];
            for (int i = 0; i < 2; i++) {
                _Ro[i] = new float[SBLIMIT][];
                for (int i2 = 0; i2 < SBLIMIT; i2++) {
                    _Ro[i][i2] = new float[SSLIMIT];
                }
            }
            _Lr = new float[2][][];
            for (int i3 = 0; i3 < 2; i3++) {
                _Lr[i3] = new float[SBLIMIT][];
                for (int i4 = 0; i4 < SBLIMIT; i4++) {
                    _Lr[i3][i4] = new float[SSLIMIT];
                }
            }
            _Out1D = new float[SBLIMIT * SSLIMIT];
            _Prevblck = new float[2][];
            for (int i5 = 0; i5 < 2; i5++) {
                _Prevblck[i5] = new float[SBLIMIT * SSLIMIT];
            }
            _K = new float[2][];
            for (int i6 = 0; i6 < 2; i6++) {
                _K[i6] = new float[SBLIMIT * SSLIMIT];
            }
            _Nonzero = new int[2];

            //III_scalefact_t
            ScaleFactorData[] iiiScalefacT = new ScaleFactorData[2];
            iiiScalefacT[0] = new ScaleFactorData();
            iiiScalefacT[1] = new ScaleFactorData();
            _Scalefac = iiiScalefacT;
            // L3TABLE INIT

            _SfBandIndex = new SBI[9]; // SZD: MPEG2.5 +3 indices
            int[] l0 = {
                0, 6, 12, 18, 24, 30, 36, 44, 54, 66, 80, 96, 116, 140, 168, 200, 238, 284, 336, 396, 464, 522, 576
            };
            int[] s0 = {0, 4, 8, 12, 18, 24, 32, 42, 56, 74, 100, 132, 174, 192};
            int[] l1 = {
                0, 6, 12, 18, 24, 30, 36, 44, 54, 66, 80, 96, 114, 136, 162, 194, 232, 278, 330, 394, 464, 540, 576
            };
            int[] s1 = {0, 4, 8, 12, 18, 26, 36, 48, 62, 80, 104, 136, 180, 192};
            int[] l2 = {
                0, 6, 12, 18, 24, 30, 36, 44, 54, 66, 80, 96, 116, 140, 168, 200, 238, 284, 336, 396, 464, 522, 576
            };
            int[] s2 = {0, 4, 8, 12, 18, 26, 36, 48, 62, 80, 104, 134, 174, 192};

            int[] l3 = {
                0, 4, 8, 12, 16, 20, 24, 30, 36, 44, 52, 62, 74, 90, 110, 134, 162, 196, 238, 288, 342, 418, 576
            };
            int[] s3 = {0, 4, 8, 12, 16, 22, 30, 40, 52, 66, 84, 106, 136, 192};
            int[] l4 = {
                0, 4, 8, 12, 16, 20, 24, 30, 36, 42, 50, 60, 72, 88, 106, 128, 156, 190, 230, 276, 330, 384, 576
            };
            int[] s4 = {0, 4, 8, 12, 16, 22, 28, 38, 50, 64, 80, 100, 126, 192};
            int[] l5 = {
                0, 4, 8, 12, 16, 20, 24, 30, 36, 44, 54, 66, 82, 102, 126, 156, 194, 240, 296, 364, 448, 550,
                576
            };
            int[] s5 = {0, 4, 8, 12, 16, 22, 30, 42, 58, 78, 104, 138, 180, 192};
            // SZD: MPEG2.5
            int[] l6 = {
                0, 6, 12, 18, 24, 30, 36, 44, 54, 66, 80, 96, 116, 140, 168, 200, 238, 284, 336, 396, 464, 522,
                576
            };
            int[] s6 = {0, 4, 8, 12, 18, 26, 36, 48, 62, 80, 104, 134, 174, 192};
            int[] l7 = {
                0, 6, 12, 18, 24, 30, 36, 44, 54, 66, 80, 96, 116, 140, 168, 200, 238, 284, 336, 396, 464, 522,
                576
            };
            int[] s7 = {0, 4, 8, 12, 18, 26, 36, 48, 62, 80, 104, 134, 174, 192};
            int[] l8 = {
                0, 12, 24, 36, 48, 60, 72, 88, 108, 132, 160, 192, 232, 280, 336, 400, 476, 566, 568, 570, 572,
                574, 576
            };
            int[] s8 = {0, 8, 16, 24, 36, 52, 72, 96, 124, 160, 162, 164, 166, 192};

            _SfBandIndex[0] = new SBI(l0, s0);
            _SfBandIndex[1] = new SBI(l1, s1);
            _SfBandIndex[2] = new SBI(l2, s2);

            _SfBandIndex[3] = new SBI(l3, s3);
            _SfBandIndex[4] = new SBI(l4, s4);
            _SfBandIndex[5] = new SBI(l5, s5);
            //SZD: MPEG2.5
            _SfBandIndex[6] = new SBI(l6, s6);
            _SfBandIndex[7] = new SBI(l7, s7);
            _SfBandIndex[8] = new SBI(l8, s8);
            // END OF L3TABLE INIT

            if (_reorderTable == null) {
                // SZD: generate LUT
                _reorderTable = new int[9][];
                for (int i = 0; i < 9; i++)
                    _reorderTable[i] = Reorder(_SfBandIndex[i].S);
            }

            // Sftable
            int[] ll0 = {0, 6, 11, 16, 21};
            int[] ss0 = {0, 6, 12};
            Sftable = new ScaleFactorTable(this, ll0, ss0);
            // END OF Sftable

            // scalefac_buffer
            ScalefacBuffer = new int[54];
            // END OF scalefac_buffer

            _Stream = stream;
            _Header = header;
            _Filter1 = filtera;
            _Filter2 = filterb;
            _Buffer = buffer;
            _WhichChannels = whichCh;

            _FrameStart = 0;
            _Channels = _Header.Mode() == Header.SINGLE_CHANNEL ? 1 : 2;
            _MaxGr = _Header.Version() == Header.MPEG1 ? 2 : 1;

            _Sfreq = _Header.sample_frequency() +
                    (_Header.Version() == Header.MPEG1 ? 3 : _Header.Version() == Header.MPEG25_LSF ? 6 : 0); // SZD

            if (_Channels == 2) {
                switch (_WhichChannels) {
                    case (int)OutputChannelsEnum.LeftChannel:
                    case (int)OutputChannelsEnum.DownmixChannels:
                        _FirstChannel = _LastChannel = 0;
                        break;

                    case (int)OutputChannelsEnum.RightChannel:
                        _FirstChannel = _LastChannel = 1;
                        break;

                    default: // OutputChannelsEnum.BothChannels:
                        _FirstChannel = 0;
                        _LastChannel = 1;
                        break;
                }
            }
            else {
                _FirstChannel = _LastChannel = 0;
            }

            for (int ch = 0; ch < 2; ch++)
                for (int j = 0; j < 576; j++)
                    _Prevblck[ch][j] = 0.0f;

            _Nonzero[0] = _Nonzero[1] = 576;

            _BitReserve = new BitReserve();
            _SideInfo = new Layer3SideInfo();
        }

        public void DecodeFrame() {
            Decode();
        }

        private void InitBlock() {
            Rawout = new float[36];
            TsOutCopy = new float[18];
            IsRatio = new float[576];
            IsPos = new int[576];
            _NewSlen = new int[4];
            _Samples2 = new float[32];
            _Samples1 = new float[32];
        }

        /// <summary>
        /// Notify decoder that a seek is being made.
        /// </summary>
        internal void SeekNotify() {
            _FrameStart = 0;
            for (int ch = 0; ch < 2; ch++)
                for (int j = 0; j < 576; j++)
                    _Prevblck[ch][j] = 0.0f;
            _BitReserve = new BitReserve();
        }

        internal void Decode() {
            int nSlots = _Header.Slots();
            int flushMain;
            int gr, ch, ss, sb, sb18;
            int mainDataEnd;
            int bytesToDiscard;
            int i;

            ReadSideInfo();

            for (i = 0; i < nSlots; i++)
                _BitReserve.PutBuffer(_Stream.GetBitsFromBuffer(8));

            mainDataEnd = SupportClass.URShift(_BitReserve.HssTell(), 3); // of previous frame

            if ((flushMain = _BitReserve.HssTell() & 7) != 0) {
                _BitReserve.ReadBits(8 - flushMain);
                mainDataEnd++;
            }

            bytesToDiscard = _FrameStart - mainDataEnd - _SideInfo.MainDataBegin;

            _FrameStart += nSlots;

            if (bytesToDiscard < 0)
                return;

            if (mainDataEnd > 4096) {
                _FrameStart -= 4096;
                _BitReserve.RewindStreamBytes(4096);
            }

            for (; bytesToDiscard > 0; bytesToDiscard--)
                _BitReserve.ReadBits(8);

            for (gr = 0; gr < _MaxGr; gr++) {
                for (ch = 0; ch < _Channels; ch++) {
                    _Part2Start = _BitReserve.HssTell();

                    if (_Header.Version() == Header.MPEG1)
                        ReadScaleFactors(ch, gr);
                    // MPEG-2 LSF, SZD: MPEG-2.5 LSF
                    else
                        GLSFScaleFactors(ch, gr);

                    HuffmanDecode(ch, gr);
                    // System.out.println("CheckSum HuffMan = " + CheckSumHuff);
                    DequantizeSample(_Ro[ch], ch, gr);
                }

                Stereo(gr);

                if (_WhichChannels == OutputChannels.DOWNMIX_CHANNELS && _Channels > 1)
                    DoDownMix();

                for (ch = _FirstChannel; ch <= _LastChannel; ch++) {
                    Reorder(_Lr[ch], ch, gr);
                    Antialias(ch, gr);
                    //for (int hb = 0;hb<576;hb++) CheckSumOut1d = CheckSumOut1d + out_1d[hb];
                    //System.out.println("CheckSumOut1d = "+CheckSumOut1d);

                    Hybrid(ch, gr);

                    //for (int hb = 0;hb<576;hb++) CheckSumOut1d = CheckSumOut1d + out_1d[hb];
                    //System.out.println("CheckSumOut1d = "+CheckSumOut1d);

                    for (sb18 = 18; sb18 < 576; sb18 += 36)
                        // Frequency inversion
                        for (ss = 1; ss < SSLIMIT; ss += 2)
                            _Out1D[sb18 + ss] = -_Out1D[sb18 + ss];

                    if (ch == 0 || _WhichChannels == OutputChannels.RIGHT_CHANNEL) {
                        for (ss = 0; ss < SSLIMIT; ss++) {
                            // Polyphase synthesis
                            sb = 0;
                            for (sb18 = 0; sb18 < 576; sb18 += 18) {
                                _Samples1[sb] = _Out1D[sb18 + ss];
                                //filter1.input_sample(out_1d[sb18+ss], sb);
                                sb++;
                            }
                            //buffer.appendSamples(0, samples1);
                            //Console.WriteLine("Adding samples right into output buffer");
                            _Filter1.AddSamples(_Samples1);
                            _Filter1.calculate_pc_samples(_Buffer);
                        }
                    }
                    else {
                        for (ss = 0; ss < SSLIMIT; ss++) {
                            // Polyphase synthesis
                            sb = 0;
                            for (sb18 = 0; sb18 < 576; sb18 += 18) {
                                _Samples2[sb] = _Out1D[sb18 + ss];
                                //filter2.input_sample(out_1d[sb18+ss], sb);
                                sb++;
                            }
                            //buffer.appendSamples(1, samples2);
                            //Console.WriteLine("Adding samples right into output buffer");
                            _Filter2.AddSamples(_Samples2);
                            _Filter2.calculate_pc_samples(_Buffer);
                        }
                    }
                }
                // channels
            }
            // granule
            _Buffer.WriteBuffer(1);
        }

        /// <summary>
        /// Reads the side info from the stream, assuming the entire.
        /// frame has been read already.
        /// Mono   : 136 bits (= 17 bytes)
        /// Stereo : 256 bits (= 32 bytes)
        /// </summary>
        private bool ReadSideInfo() {
            int ch, gr;
            if (_Header.Version() == Header.MPEG1) {
                _SideInfo.MainDataBegin = _Stream.GetBitsFromBuffer(9);
                if (_Channels == 1)
                    _SideInfo.PrivateBits = _Stream.GetBitsFromBuffer(5);
                else
                    _SideInfo.PrivateBits = _Stream.GetBitsFromBuffer(3);

                for (ch = 0; ch < _Channels; ch++) {
                    _SideInfo.Channels[ch].ScaleFactorBits[0] = _Stream.GetBitsFromBuffer(1);
                    _SideInfo.Channels[ch].ScaleFactorBits[1] = _Stream.GetBitsFromBuffer(1);
                    _SideInfo.Channels[ch].ScaleFactorBits[2] = _Stream.GetBitsFromBuffer(1);
                    _SideInfo.Channels[ch].ScaleFactorBits[3] = _Stream.GetBitsFromBuffer(1);
                }

                for (gr = 0; gr < 2; gr++) {
                    for (ch = 0; ch < _Channels; ch++) {
                        _SideInfo.Channels[ch].Granules[gr].Part23Length = _Stream.GetBitsFromBuffer(12);
                        _SideInfo.Channels[ch].Granules[gr].BigValues = _Stream.GetBitsFromBuffer(9);
                        _SideInfo.Channels[ch].Granules[gr].GlobalGain = _Stream.GetBitsFromBuffer(8);
                        _SideInfo.Channels[ch].Granules[gr].ScaleFacCompress = _Stream.GetBitsFromBuffer(4);
                        _SideInfo.Channels[ch].Granules[gr].WindowSwitchingFlag = _Stream.GetBitsFromBuffer(1);
                        if (_SideInfo.Channels[ch].Granules[gr].WindowSwitchingFlag != 0) {
                            _SideInfo.Channels[ch].Granules[gr].BlockType = _Stream.GetBitsFromBuffer(2);
                            _SideInfo.Channels[ch].Granules[gr].MixedBlockFlag = _Stream.GetBitsFromBuffer(1);

                            _SideInfo.Channels[ch].Granules[gr].TableSelect[0] = _Stream.GetBitsFromBuffer(5);
                            _SideInfo.Channels[ch].Granules[gr].TableSelect[1] = _Stream.GetBitsFromBuffer(5);

                            _SideInfo.Channels[ch].Granules[gr].SubblockGain[0] = _Stream.GetBitsFromBuffer(3);
                            _SideInfo.Channels[ch].Granules[gr].SubblockGain[1] = _Stream.GetBitsFromBuffer(3);
                            _SideInfo.Channels[ch].Granules[gr].SubblockGain[2] = _Stream.GetBitsFromBuffer(3);

                            // Set region_count parameters since they are implicit in this case.

                            if (_SideInfo.Channels[ch].Granules[gr].BlockType == 0) {
                                // Side info bad: block_type == 0 in split block
                                return false;
                            }
                            if (_SideInfo.Channels[ch].Granules[gr].BlockType == 2 && _SideInfo.Channels[ch].Granules[gr].MixedBlockFlag == 0) {
                                _SideInfo.Channels[ch].Granules[gr].Region0Count = 8;
                            }
                            else {
                                _SideInfo.Channels[ch].Granules[gr].Region0Count = 7;
                            }
                            _SideInfo.Channels[ch].Granules[gr].Region1Count = 20 - _SideInfo.Channels[ch].Granules[gr].Region0Count;
                        }
                        else {
                            _SideInfo.Channels[ch].Granules[gr].TableSelect[0] = _Stream.GetBitsFromBuffer(5);
                            _SideInfo.Channels[ch].Granules[gr].TableSelect[1] = _Stream.GetBitsFromBuffer(5);
                            _SideInfo.Channels[ch].Granules[gr].TableSelect[2] = _Stream.GetBitsFromBuffer(5);
                            _SideInfo.Channels[ch].Granules[gr].Region0Count = _Stream.GetBitsFromBuffer(4);
                            _SideInfo.Channels[ch].Granules[gr].Region1Count = _Stream.GetBitsFromBuffer(3);
                            _SideInfo.Channels[ch].Granules[gr].BlockType = 0;
                        }
                        _SideInfo.Channels[ch].Granules[gr].Preflag = _Stream.GetBitsFromBuffer(1);
                        _SideInfo.Channels[ch].Granules[gr].ScaleFacScale = _Stream.GetBitsFromBuffer(1);
                        _SideInfo.Channels[ch].Granules[gr].Count1TableSelect = _Stream.GetBitsFromBuffer(1);
                    }
                }
            }
            else {
                // MPEG-2 LSF, SZD: MPEG-2.5 LSF

                _SideInfo.MainDataBegin = _Stream.GetBitsFromBuffer(8);
                if (_Channels == 1)
                    _SideInfo.PrivateBits = _Stream.GetBitsFromBuffer(1);
                else
                    _SideInfo.PrivateBits = _Stream.GetBitsFromBuffer(2);

                for (ch = 0; ch < _Channels; ch++) {
                    _SideInfo.Channels[ch].Granules[0].Part23Length = _Stream.GetBitsFromBuffer(12);
                    _SideInfo.Channels[ch].Granules[0].BigValues = _Stream.GetBitsFromBuffer(9);
                    _SideInfo.Channels[ch].Granules[0].GlobalGain = _Stream.GetBitsFromBuffer(8);
                    _SideInfo.Channels[ch].Granules[0].ScaleFacCompress = _Stream.GetBitsFromBuffer(9);
                    _SideInfo.Channels[ch].Granules[0].WindowSwitchingFlag = _Stream.GetBitsFromBuffer(1);

                    if (_SideInfo.Channels[ch].Granules[0].WindowSwitchingFlag != 0) {
                        _SideInfo.Channels[ch].Granules[0].BlockType = _Stream.GetBitsFromBuffer(2);
                        _SideInfo.Channels[ch].Granules[0].MixedBlockFlag = _Stream.GetBitsFromBuffer(1);
                        _SideInfo.Channels[ch].Granules[0].TableSelect[0] = _Stream.GetBitsFromBuffer(5);
                        _SideInfo.Channels[ch].Granules[0].TableSelect[1] = _Stream.GetBitsFromBuffer(5);

                        _SideInfo.Channels[ch].Granules[0].SubblockGain[0] = _Stream.GetBitsFromBuffer(3);
                        _SideInfo.Channels[ch].Granules[0].SubblockGain[1] = _Stream.GetBitsFromBuffer(3);
                        _SideInfo.Channels[ch].Granules[0].SubblockGain[2] = _Stream.GetBitsFromBuffer(3);

                        // Set region_count parameters since they are implicit in this case.

                        if (_SideInfo.Channels[ch].Granules[0].BlockType == 0) {
                            // Side info bad: block_type == 0 in split block
                            return false;
                        }
                        if (_SideInfo.Channels[ch].Granules[0].BlockType == 2 && _SideInfo.Channels[ch].Granules[0].MixedBlockFlag == 0) {
                            _SideInfo.Channels[ch].Granules[0].Region0Count = 8;
                        }
                        else {
                            _SideInfo.Channels[ch].Granules[0].Region0Count = 7;
                            _SideInfo.Channels[ch].Granules[0].Region1Count = 20 - _SideInfo.Channels[ch].Granules[0].Region0Count;
                        }
                    }
                    else {
                        _SideInfo.Channels[ch].Granules[0].TableSelect[0] = _Stream.GetBitsFromBuffer(5);
                        _SideInfo.Channels[ch].Granules[0].TableSelect[1] = _Stream.GetBitsFromBuffer(5);
                        _SideInfo.Channels[ch].Granules[0].TableSelect[2] = _Stream.GetBitsFromBuffer(5);
                        _SideInfo.Channels[ch].Granules[0].Region0Count = _Stream.GetBitsFromBuffer(4);
                        _SideInfo.Channels[ch].Granules[0].Region1Count = _Stream.GetBitsFromBuffer(3);
                        _SideInfo.Channels[ch].Granules[0].BlockType = 0;
                    }

                    _SideInfo.Channels[ch].Granules[0].ScaleFacScale = _Stream.GetBitsFromBuffer(1);
                    _SideInfo.Channels[ch].Granules[0].Count1TableSelect = _Stream.GetBitsFromBuffer(1);
                }
                // for(ch=0; ch<channels; ch++)
            }
            // if (header.version() == MPEG1)
            return true;
        }

        /// <summary>
        /// *
        /// </summary>
        private void ReadScaleFactors(int ch, int gr) {
            int sfb, window;
            GranuleInfo grInfo = _SideInfo.Channels[ch].Granules[gr];
            int scaleComp = grInfo.ScaleFacCompress;
            int length0 = Slen[0][scaleComp];
            int length1 = Slen[1][scaleComp];

            if (grInfo.WindowSwitchingFlag != 0 && grInfo.BlockType == 2) {
                if (grInfo.MixedBlockFlag != 0) {
                    // MIXED
                    for (sfb = 0; sfb < 8; sfb++)
                        _Scalefac[ch].L[sfb] = _BitReserve.ReadBits(Slen[0][grInfo.ScaleFacCompress]);
                    for (sfb = 3; sfb < 6; sfb++)
                        for (window = 0; window < 3; window++)
                            _Scalefac[ch].S[window][sfb] = _BitReserve.ReadBits(Slen[0][grInfo.ScaleFacCompress]);
                    for (sfb = 6; sfb < 12; sfb++)
                        for (window = 0; window < 3; window++)
                            _Scalefac[ch].S[window][sfb] = _BitReserve.ReadBits(Slen[1][grInfo.ScaleFacCompress]);
                    for (sfb = 12, window = 0; window < 3; window++)
                        _Scalefac[ch].S[window][sfb] = 0;
                }
                else {
                    // SHORT

                    _Scalefac[ch].S[0][0] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[1][0] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[2][0] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[0][1] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[1][1] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[2][1] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[0][2] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[1][2] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[2][2] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[0][3] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[1][3] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[2][3] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[0][4] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[1][4] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[2][4] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[0][5] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[1][5] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[2][5] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].S[0][6] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[1][6] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[2][6] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[0][7] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[1][7] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[2][7] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[0][8] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[1][8] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[2][8] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[0][9] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[1][9] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[2][9] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[0][10] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[1][10] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[2][10] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[0][11] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[1][11] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[2][11] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].S[0][12] = 0;
                    _Scalefac[ch].S[1][12] = 0;
                    _Scalefac[ch].S[2][12] = 0;
                }
                // SHORT
            }
            else {
                // LONG types 0,1,3

                if (_SideInfo.Channels[ch].ScaleFactorBits[0] == 0 || gr == 0) {
                    _Scalefac[ch].L[0] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].L[1] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].L[2] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].L[3] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].L[4] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].L[5] = _BitReserve.ReadBits(length0);
                }
                if (_SideInfo.Channels[ch].ScaleFactorBits[1] == 0 || gr == 0) {
                    _Scalefac[ch].L[6] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].L[7] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].L[8] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].L[9] = _BitReserve.ReadBits(length0);
                    _Scalefac[ch].L[10] = _BitReserve.ReadBits(length0);
                }
                if (_SideInfo.Channels[ch].ScaleFactorBits[2] == 0 || gr == 0) {
                    _Scalefac[ch].L[11] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].L[12] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].L[13] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].L[14] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].L[15] = _BitReserve.ReadBits(length1);
                }
                if (_SideInfo.Channels[ch].ScaleFactorBits[3] == 0 || gr == 0) {
                    _Scalefac[ch].L[16] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].L[17] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].L[18] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].L[19] = _BitReserve.ReadBits(length1);
                    _Scalefac[ch].L[20] = _BitReserve.ReadBits(length1);
                }

                _Scalefac[ch].L[21] = 0;
                _Scalefac[ch].L[22] = 0;
            }
        }

        private void GetLSFScaleData(int ch, int gr) {
            int scalefacComp, intScalefacComp;
            int modeExt = _Header.mode_extension();
            int m;
            int blocktypenumber;
            int blocknumber = 0;

            GranuleInfo grInfo = _SideInfo.Channels[ch].Granules[gr];

            scalefacComp = grInfo.ScaleFacCompress;

            if (grInfo.BlockType == 2) {
                if (grInfo.MixedBlockFlag == 0)
                    blocktypenumber = 1;
                else if (grInfo.MixedBlockFlag == 1)
                    blocktypenumber = 2;
                else
                    blocktypenumber = 0;
            }
            else {
                blocktypenumber = 0;
            }

            if (!((modeExt == 1 || modeExt == 3) && ch == 1)) {
                if (scalefacComp < 400) {
                    _NewSlen[0] = SupportClass.URShift(scalefacComp, 4) / 5;
                    _NewSlen[1] = SupportClass.URShift(scalefacComp, 4) % 5;
                    _NewSlen[2] = SupportClass.URShift(scalefacComp & 0xF, 2);
                    _NewSlen[3] = scalefacComp & 3;
                    _SideInfo.Channels[ch].Granules[gr].Preflag = 0;
                    blocknumber = 0;
                }
                else if (scalefacComp < 500) {
                    _NewSlen[0] = SupportClass.URShift(scalefacComp - 400, 2) / 5;
                    _NewSlen[1] = SupportClass.URShift(scalefacComp - 400, 2) % 5;
                    _NewSlen[2] = (scalefacComp - 400) & 3;
                    _NewSlen[3] = 0;
                    _SideInfo.Channels[ch].Granules[gr].Preflag = 0;
                    blocknumber = 1;
                }
                else if (scalefacComp < 512) {
                    _NewSlen[0] = (scalefacComp - 500) / 3;
                    _NewSlen[1] = (scalefacComp - 500) % 3;
                    _NewSlen[2] = 0;
                    _NewSlen[3] = 0;
                    _SideInfo.Channels[ch].Granules[gr].Preflag = 1;
                    blocknumber = 2;
                }
            }

            if ((modeExt == 1 || modeExt == 3) && ch == 1) {
                intScalefacComp = SupportClass.URShift(scalefacComp, 1);

                if (intScalefacComp < 180) {
                    _NewSlen[0] = intScalefacComp / 36;
                    _NewSlen[1] = intScalefacComp % 36 / 6;
                    _NewSlen[2] = intScalefacComp % 36 % 6;
                    _NewSlen[3] = 0;
                    _SideInfo.Channels[ch].Granules[gr].Preflag = 0;
                    blocknumber = 3;
                }
                else if (intScalefacComp < 244) {
                    _NewSlen[0] = SupportClass.URShift((intScalefacComp - 180) & 0x3F, 4);
                    _NewSlen[1] = SupportClass.URShift((intScalefacComp - 180) & 0xF, 2);
                    _NewSlen[2] = (intScalefacComp - 180) & 3;
                    _NewSlen[3] = 0;
                    _SideInfo.Channels[ch].Granules[gr].Preflag = 0;
                    blocknumber = 4;
                }
                else if (intScalefacComp < 255) {
                    _NewSlen[0] = (intScalefacComp - 244) / 3;
                    _NewSlen[1] = (intScalefacComp - 244) % 3;
                    _NewSlen[2] = 0;
                    _NewSlen[3] = 0;
                    _SideInfo.Channels[ch].Granules[gr].Preflag = 0;
                    blocknumber = 5;
                }
            }

            for (int x = 0; x < 45; x++)
                // why 45, not 54?
                ScalefacBuffer[x] = 0;

            m = 0;
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < NrOfSfbBlock[blocknumber][blocktypenumber][i]; j++) {
                    ScalefacBuffer[m] = _NewSlen[i] == 0 ? 0 : _BitReserve.ReadBits(_NewSlen[i]);
                    m++;
                }
                // for (unint32 j ...
            }
            // for (uint32 i ...
        }

        /// <summary>
        /// *
        /// </summary>
        private void GLSFScaleFactors(int ch, int gr) {
            int m = 0;
            int sfb;
            GranuleInfo grInfo = _SideInfo.Channels[ch].Granules[gr];

            GetLSFScaleData(ch, gr);

            if (grInfo.WindowSwitchingFlag != 0 && grInfo.BlockType == 2) {
                int window;
                if (grInfo.MixedBlockFlag != 0) {
                    // MIXED
                    for (sfb = 0; sfb < 8; sfb++) {
                        _Scalefac[ch].L[sfb] = ScalefacBuffer[m];
                        m++;
                    }
                    for (sfb = 3; sfb < 12; sfb++) {
                        for (window = 0; window < 3; window++) {
                            _Scalefac[ch].S[window][sfb] = ScalefacBuffer[m];
                            m++;
                        }
                    }
                    for (window = 0; window < 3; window++)
                        _Scalefac[ch].S[window][12] = 0;
                }
                else {
                    // SHORT

                    for (sfb = 0; sfb < 12; sfb++) {
                        for (window = 0; window < 3; window++) {
                            _Scalefac[ch].S[window][sfb] = ScalefacBuffer[m];
                            m++;
                        }
                    }

                    for (window = 0; window < 3; window++)
                        _Scalefac[ch].S[window][12] = 0;
                }
            }
            else {
                // LONG types 0,1,3

                for (sfb = 0; sfb < 21; sfb++) {
                    _Scalefac[ch].L[sfb] = ScalefacBuffer[m];
                    m++;
                }
                _Scalefac[ch].L[21] = 0; // Jeff
                _Scalefac[ch].L[22] = 0;
            }
        }

        private void HuffmanDecode(int ch, int gr) {
            X[0] = 0;
            Y[0] = 0;
            V[0] = 0;
            W[0] = 0;
            int part23End = _Part2Start + _SideInfo.Channels[ch].Granules[gr].Part23Length;
            int nuBits;
            int region1Start;
            int region2Start;
            int index;

            int buf, buf1;

            Huffman h;

            // Find region boundary for short block case

            if (_SideInfo.Channels[ch].Granules[gr].WindowSwitchingFlag != 0 && _SideInfo.Channels[ch].Granules[gr].BlockType == 2) {
                // Region2.
                //MS: Extrahandling for 8KHZ
                region1Start = _Sfreq == 8 ? 72 : 36; // sfb[9/3]*3=36 or in case 8KHZ = 72
                region2Start = 576; // No Region2 for short block case
            }
            else {
                // Find region boundary for long block case

                buf = _SideInfo.Channels[ch].Granules[gr].Region0Count + 1;
                buf1 = buf + _SideInfo.Channels[ch].Granules[gr].Region1Count + 1;

                if (buf1 > _SfBandIndex[_Sfreq].L.Length - 1)
                    buf1 = _SfBandIndex[_Sfreq].L.Length - 1;

                region1Start = _SfBandIndex[_Sfreq].L[buf];
                region2Start = _SfBandIndex[_Sfreq].L[buf1]; /* MI */
            }

            index = 0;
            // Read bigvalues area
            for (int i = 0; i < _SideInfo.Channels[ch].Granules[gr].BigValues << 1; i += 2) {
                if (i < region1Start)
                    h = Huffman.HuffmanTable[_SideInfo.Channels[ch].Granules[gr].TableSelect[0]];
                else if (i < region2Start)
                    h = Huffman.HuffmanTable[_SideInfo.Channels[ch].Granules[gr].TableSelect[1]];
                else
                    h = Huffman.HuffmanTable[_SideInfo.Channels[ch].Granules[gr].TableSelect[2]];

                Huffman.Decode(h, X, Y, V, W, _BitReserve);

                _Is1D[index++] = X[0];
                _Is1D[index++] = Y[0];
                _CheckSumHuff = _CheckSumHuff + X[0] + Y[0];
                // System.out.println("x = "+x[0]+" y = "+y[0]);
            }

            // Read count1 area
            h = Huffman.HuffmanTable[_SideInfo.Channels[ch].Granules[gr].Count1TableSelect + 32];
            nuBits = _BitReserve.HssTell();

            while (nuBits < part23End && index < 576) {
                Huffman.Decode(h, X, Y, V, W, _BitReserve);

                _Is1D[index++] = V[0];
                _Is1D[index++] = W[0];
                _Is1D[index++] = X[0];
                _Is1D[index++] = Y[0];
                _CheckSumHuff = _CheckSumHuff + V[0] + W[0] + X[0] + Y[0];
                // System.out.println("v = "+v[0]+" w = "+w[0]);
                // System.out.println("x = "+x[0]+" y = "+y[0]);
                nuBits = _BitReserve.HssTell();
            }

            if (nuBits > part23End) {
                _BitReserve.RewindStreamBits(nuBits - part23End);
                index -= 4;
            }

            nuBits = _BitReserve.HssTell();

            // Dismiss stuffing bits
            if (nuBits < part23End)
                _BitReserve.ReadBits(part23End - nuBits);

            // Zero out rest

            if (index < 576)
                _Nonzero[ch] = index;
            else
                _Nonzero[ch] = 576;

            if (index < 0)
                index = 0;

            // may not be necessary
            for (; index < 576; index++)
                _Is1D[index] = 0;
        }

        /// <summary>
        /// *
        /// </summary>
        private void GetKStereoValues(int isPos, int ioType, int i) {
            if (isPos == 0) {
                _K[0][i] = 1.0f;
                _K[1][i] = 1.0f;
            }
            else if ((isPos & 1) != 0) {
                _K[0][i] = Io[ioType][SupportClass.URShift(isPos + 1, 1)];
                _K[1][i] = 1.0f;
            }
            else {
                _K[0][i] = 1.0f;
                _K[1][i] = Io[ioType][SupportClass.URShift(isPos, 1)];
            }
        }

        /// <summary>
        /// *
        /// </summary>
        private void DequantizeSample(float[][] xr, int ch, int gr) {
            GranuleInfo grInfo = _SideInfo.Channels[ch].Granules[gr];
            int cb = 0;
            int nextCbBoundary;
            int cbBegin = 0;
            int cbWidth = 0;
            int index = 0;
            int j;
            float[][] xr1D = xr;

            // choose correct scalefactor band per block type, initalize boundary

            if (grInfo.WindowSwitchingFlag != 0 && grInfo.BlockType == 2) {
                if (grInfo.MixedBlockFlag != 0)
                    nextCbBoundary = _SfBandIndex[_Sfreq].L[1];
                // LONG blocks: 0,1,3
                else {
                    cbWidth = _SfBandIndex[_Sfreq].S[1];
                    nextCbBoundary = (cbWidth << 2) - cbWidth;
                    cbBegin = 0;
                }
            }
            else {
                nextCbBoundary = _SfBandIndex[_Sfreq].L[1]; // LONG blocks: 0,1,3
            }

            // Compute overall (global) scaling.

            float gGain = (float)Math.Pow(2.0, 0.25 * (grInfo.GlobalGain - 210.0));

            for (j = 0; j < _Nonzero[ch]; j++) {
                int reste = j % SSLIMIT;
                int quotien = (j - reste) / SSLIMIT;
                if (_Is1D[j] == 0)
                    xr1D[quotien][reste] = 0.0f;
                else {
                    int abv = _Is1D[j];
                    // Begin Patch
                    // This was taken from a patch to the original java file. Ported by DamianMehers
                    // Original:
                    // if (is_1d[j] > 0)
                    //     xr_1d[quotien][reste] = g_gain * t_43[abv];
                    // else
                    //     xr_1d[quotien][reste] = -g_gain * t_43[-abv];
                    const double d43 = 4.0 / 3.0;
                    if (abv < PowerTable.Length) {
                        if (_Is1D[j] > 0) {
                            xr1D[quotien][reste] = gGain * PowerTable[abv];
                        }
                        else if (-abv < PowerTable.Length) {
                            xr1D[quotien][reste] = -gGain * PowerTable[-abv];
                        }
                        else {
                            xr1D[quotien][reste] = -gGain * (float)Math.Pow(-abv, d43);
                        }
                    }
                    else if (_Is1D[j] > 0) {
                        xr1D[quotien][reste] = gGain * (float)Math.Pow(abv, d43);
                    }
                    else {
                        xr1D[quotien][reste] = -gGain * (float)Math.Pow(-abv, d43);
                    }
                    // End Patch
                }
            }

            // apply formula per block type
            for (j = 0; j < _Nonzero[ch]; j++) {
                int reste = j % SSLIMIT;
                int quotien = (j - reste) / SSLIMIT;
                if (index == nextCbBoundary) {
                    /* Adjust critical band boundary */
                    if (grInfo.WindowSwitchingFlag != 0 && grInfo.BlockType == 2) {
                        if (grInfo.MixedBlockFlag != 0) {
                            if (index == _SfBandIndex[_Sfreq].L[8]) {
                                nextCbBoundary = _SfBandIndex[_Sfreq].S[4];
                                nextCbBoundary = (nextCbBoundary << 2) - nextCbBoundary;
                                cb = 3;
                                cbWidth = _SfBandIndex[_Sfreq].S[4] - _SfBandIndex[_Sfreq].S[3];

                                cbBegin = _SfBandIndex[_Sfreq].S[3];
                                cbBegin = (cbBegin << 2) - cbBegin;
                            }
                            else if (index < _SfBandIndex[_Sfreq].L[8]) {
                                nextCbBoundary = _SfBandIndex[_Sfreq].L[++cb + 1];
                            }
                            else {
                                nextCbBoundary = _SfBandIndex[_Sfreq].S[++cb + 1];
                                nextCbBoundary = (nextCbBoundary << 2) - nextCbBoundary;

                                cbBegin = _SfBandIndex[_Sfreq].S[cb];
                                cbWidth = _SfBandIndex[_Sfreq].S[cb + 1] - cbBegin;
                                cbBegin = (cbBegin << 2) - cbBegin;
                            }
                        }
                        else {
                            nextCbBoundary = _SfBandIndex[_Sfreq].S[++cb + 1];
                            nextCbBoundary = (nextCbBoundary << 2) - nextCbBoundary;

                            cbBegin = _SfBandIndex[_Sfreq].S[cb];
                            cbWidth = _SfBandIndex[_Sfreq].S[cb + 1] - cbBegin;
                            cbBegin = (cbBegin << 2) - cbBegin;
                        }
                    }
                    else {
                        // long blocks

                        nextCbBoundary = _SfBandIndex[_Sfreq].L[++cb + 1];
                    }
                }

                // Do long/short dependent scaling operations

                if (grInfo.WindowSwitchingFlag != 0 &&
                    (grInfo.BlockType == 2 && grInfo.MixedBlockFlag == 0 ||
                     grInfo.BlockType == 2 && grInfo.MixedBlockFlag != 0 && j >= 36)) {
                    int tIndex = (index - cbBegin) / cbWidth;
                    /*            xr[sb][ss] *= pow(2.0, ((-2.0 * gr_info.subblock_gain[t_index])
                    -(0.5 * (1.0 + gr_info.scalefac_scale)
                    * scalefac[ch].s[t_index][cb]))); */
                    int idx = _Scalefac[ch].S[tIndex][cb] << grInfo.ScaleFacScale;
                    idx += grInfo.SubblockGain[tIndex] << 2;

                    xr1D[quotien][reste] *= TwoToNegativeHalfPow[idx];
                }
                else {
                    // LONG block types 0,1,3 & 1st 2 subbands of switched blocks
                    /* xr[sb][ss] *= pow(2.0, -0.5 * (1.0+gr_info.scalefac_scale)
                    * (scalefac[ch].l[cb]
                    + gr_info.preflag * pretab[cb])); */
                    int idx = _Scalefac[ch].L[cb];

                    if (grInfo.Preflag != 0)
                        idx += Pretab[cb];

                    idx = idx << grInfo.ScaleFacScale;
                    xr1D[quotien][reste] *= TwoToNegativeHalfPow[idx];
                }
                index++;
            }

            for (j = _Nonzero[ch]; j < 576; j++) {
                int reste = j % SSLIMIT;
                int quotien = (j - reste) / SSLIMIT;
                if (reste < 0)
                    reste = 0;
                if (quotien < 0)
                    quotien = 0;
                xr1D[quotien][reste] = 0.0f;
            }
        }

        /// <summary>
        /// *
        /// </summary>
        private void Reorder(float[][] xr, int ch, int gr) {
            GranuleInfo grInfo = _SideInfo.Channels[ch].Granules[gr];
            int freq, freq3;
            int index;
            int sfb, sfbStart, sfbLines;
            int srcLine, desLine;
            float[][] xr1D = xr;

            if (grInfo.WindowSwitchingFlag != 0 && grInfo.BlockType == 2) {
                for (index = 0; index < 576; index++)
                    _Out1D[index] = 0.0f;

                if (grInfo.MixedBlockFlag != 0) {
                    // NO REORDER FOR LOW 2 SUBBANDS
                    for (index = 0; index < 36; index++) {
                        int reste = index % SSLIMIT;
                        int quotien = (index - reste) / SSLIMIT;
                        _Out1D[index] = xr1D[quotien][reste];
                    }
                    // REORDERING FOR REST SWITCHED SHORT
                    for (sfb = 3, sfbStart = _SfBandIndex[_Sfreq].S[3], sfbLines = _SfBandIndex[_Sfreq].S[4] - sfbStart;
                        sfb < 13;
                        sfb++, sfbStart = _SfBandIndex[_Sfreq].S[sfb],
                        sfbLines = _SfBandIndex[_Sfreq].S[sfb + 1] - sfbStart) {
                        int sfbStart3 = (sfbStart << 2) - sfbStart;

                        for (freq = 0, freq3 = 0; freq < sfbLines; freq++, freq3 += 3) {
                            srcLine = sfbStart3 + freq;
                            desLine = sfbStart3 + freq3;
                            int reste = srcLine % SSLIMIT;
                            int quotien = (srcLine - reste) / SSLIMIT;

                            _Out1D[desLine] = xr1D[quotien][reste];
                            srcLine += sfbLines;
                            desLine++;

                            reste = srcLine % SSLIMIT;
                            quotien = (srcLine - reste) / SSLIMIT;

                            _Out1D[desLine] = xr1D[quotien][reste];
                            srcLine += sfbLines;
                            desLine++;

                            reste = srcLine % SSLIMIT;
                            quotien = (srcLine - reste) / SSLIMIT;

                            _Out1D[desLine] = xr1D[quotien][reste];
                        }
                    }
                }
                else {
                    // pure short
                    for (index = 0; index < 576; index++) {
                        int j = _reorderTable[_Sfreq][index];
                        int reste = j % SSLIMIT;
                        int quotien = (j - reste) / SSLIMIT;
                        _Out1D[index] = xr1D[quotien][reste];
                    }
                }
            }
            else {
                // long blocks
                for (index = 0; index < 576; index++) {
                    int reste = index % SSLIMIT;
                    int quotien = (index - reste) / SSLIMIT;
                    _Out1D[index] = xr1D[quotien][reste];
                }
            }
        }

        private void Stereo(int gr) {
            int sb, ss;

            if (_Channels == 1) {
                // mono , bypass xr[0][][] to lr[0][][]

                for (sb = 0; sb < SBLIMIT; sb++)
                    for (ss = 0; ss < SSLIMIT; ss += 3) {
                        _Lr[0][sb][ss] = _Ro[0][sb][ss];
                        _Lr[0][sb][ss + 1] = _Ro[0][sb][ss + 1];
                        _Lr[0][sb][ss + 2] = _Ro[0][sb][ss + 2];
                    }
            }
            else {
                GranuleInfo grInfo = _SideInfo.Channels[0].Granules[gr];
                int modeExt = _Header.mode_extension();
                int sfb;
                int i;
                int lines, temp, temp2;

                bool msStereo = _Header.Mode() == Header.JOINT_STEREO && (modeExt & 0x2) != 0;
                bool iStereo = _Header.Mode() == Header.JOINT_STEREO && (modeExt & 0x1) != 0;
                bool lsf = _Header.Version() == Header.MPEG2_LSF || _Header.Version() == Header.MPEG25_LSF; // SZD

                int ioType = grInfo.ScaleFacCompress & 1;

                // initialization

                for (i = 0; i < 576; i++) {
                    IsPos[i] = 7;

                    IsRatio[i] = 0.0f;
                }

                if (iStereo) {
                    if (grInfo.WindowSwitchingFlag != 0 && grInfo.BlockType == 2) {
                        if (grInfo.MixedBlockFlag != 0) {
                            int maxSfb = 0;

                            for (int j = 0; j < 3; j++) {
                                int sfbcnt;
                                sfbcnt = 2;
                                for (sfb = 12; sfb >= 3; sfb--) {
                                    i = _SfBandIndex[_Sfreq].S[sfb];
                                    lines = _SfBandIndex[_Sfreq].S[sfb + 1] - i;
                                    i = (i << 2) - i + (j + 1) * lines - 1;

                                    while (lines > 0) {
                                        if (_Ro[1][i / 18][i % 18] != 0.0f) {
                                            // MDM: in java, array access is very slow.
                                            // Is quicker to compute div and mod values.
                                            //if (ro[1][ss_div[i]][ss_mod[i]] != 0.0f) {
                                            sfbcnt = sfb;
                                            sfb = -10;
                                            lines = -10;
                                        }

                                        lines--;
                                        i--;
                                    } // while (lines > 0)
                                }
                                // for (sfb=12 ...
                                sfb = sfbcnt + 1;

                                if (sfb > maxSfb)
                                    maxSfb = sfb;

                                while (sfb < 12) {
                                    temp = _SfBandIndex[_Sfreq].S[sfb];
                                    sb = _SfBandIndex[_Sfreq].S[sfb + 1] - temp;
                                    i = (temp << 2) - temp + j * sb;

                                    for (; sb > 0; sb--) {
                                        IsPos[i] = _Scalefac[1].S[j][sfb];
                                        if (IsPos[i] != 7)
                                            if (lsf)
                                                GetKStereoValues(IsPos[i], ioType, i);
                                            else
                                                IsRatio[i] = Tan12[IsPos[i]];

                                        i++;
                                    }
                                    // for (; sb>0...
                                    sfb++;
                                } // while (sfb < 12)
                                sfb = _SfBandIndex[_Sfreq].S[10];
                                sb = _SfBandIndex[_Sfreq].S[11] - sfb;
                                sfb = (sfb << 2) - sfb + j * sb;
                                temp = _SfBandIndex[_Sfreq].S[11];
                                sb = _SfBandIndex[_Sfreq].S[12] - temp;
                                i = (temp << 2) - temp + j * sb;

                                for (; sb > 0; sb--) {
                                    IsPos[i] = IsPos[sfb];

                                    if (lsf) {
                                        _K[0][i] = _K[0][sfb];
                                        _K[1][i] = _K[1][sfb];
                                    }
                                    else {
                                        IsRatio[i] = IsRatio[sfb];
                                    }
                                    i++;
                                }
                                // for (; sb > 0 ...
                            }
                            if (maxSfb <= 3) {
                                i = 2;
                                ss = 17;
                                sb = -1;
                                while (i >= 0) {
                                    if (_Ro[1][i][ss] != 0.0f) {
                                        sb = (i << 4) + (i << 1) + ss;
                                        i = -1;
                                    }
                                    else {
                                        ss--;
                                        if (ss < 0) {
                                            i--;
                                            ss = 17;
                                        }
                                    }
                                    // if (ro ...
                                } // while (i>=0)
                                i = 0;
                                while (_SfBandIndex[_Sfreq].L[i] <= sb)
                                    i++;
                                sfb = i;
                                i = _SfBandIndex[_Sfreq].L[i];
                                for (; sfb < 8; sfb++) {
                                    sb = _SfBandIndex[_Sfreq].L[sfb + 1] - _SfBandIndex[_Sfreq].L[sfb];
                                    for (; sb > 0; sb--) {
                                        IsPos[i] = _Scalefac[1].L[sfb];
                                        if (IsPos[i] != 7)
                                            if (lsf)
                                                GetKStereoValues(IsPos[i], ioType, i);
                                            else
                                                IsRatio[i] = Tan12[IsPos[i]];
                                        i++;
                                    }
                                    // for (; sb>0 ...
                                }
                                // for (; sfb<8 ...
                            }
                            // for (j=0 ...
                        }
                        else {
                            // if (gr_info.mixed_block_flag)
                            for (int j = 0; j < 3; j++) {
                                int sfbcnt;
                                sfbcnt = -1;
                                for (sfb = 12; sfb >= 0; sfb--) {
                                    temp = _SfBandIndex[_Sfreq].S[sfb];
                                    lines = _SfBandIndex[_Sfreq].S[sfb + 1] - temp;
                                    i = (temp << 2) - temp + (j + 1) * lines - 1;

                                    while (lines > 0) {
                                        if (_Ro[1][i / 18][i % 18] != 0.0f) {
                                            // MDM: in java, array access is very slow.
                                            // Is quicker to compute div and mod values.
                                            //if (ro[1][ss_div[i]][ss_mod[i]] != 0.0f) {
                                            sfbcnt = sfb;
                                            sfb = -10;
                                            lines = -10;
                                        }
                                        lines--;
                                        i--;
                                    } // while (lines > 0) */
                                }
                                // for (sfb=12 ...
                                sfb = sfbcnt + 1;
                                while (sfb < 12) {
                                    temp = _SfBandIndex[_Sfreq].S[sfb];
                                    sb = _SfBandIndex[_Sfreq].S[sfb + 1] - temp;
                                    i = (temp << 2) - temp + j * sb;
                                    for (; sb > 0; sb--) {
                                        IsPos[i] = _Scalefac[1].S[j][sfb];
                                        if (IsPos[i] != 7)
                                            if (lsf)
                                                GetKStereoValues(IsPos[i], ioType, i);
                                            else
                                                IsRatio[i] = Tan12[IsPos[i]];
                                        i++;
                                    }
                                    // for (; sb>0 ...
                                    sfb++;
                                } // while (sfb<12)

                                temp = _SfBandIndex[_Sfreq].S[10];
                                temp2 = _SfBandIndex[_Sfreq].S[11];
                                sb = temp2 - temp;
                                sfb = (temp << 2) - temp + j * sb;
                                sb = _SfBandIndex[_Sfreq].S[12] - temp2;
                                i = (temp2 << 2) - temp2 + j * sb;

                                for (; sb > 0; sb--) {
                                    IsPos[i] = IsPos[sfb];

                                    if (lsf) {
                                        _K[0][i] = _K[0][sfb];
                                        _K[1][i] = _K[1][sfb];
                                    }
                                    else {
                                        IsRatio[i] = IsRatio[sfb];
                                    }
                                    i++;
                                }
                                // for (; sb>0 ...
                            }
                            // for (sfb=12
                        }
                        // for (j=0 ...
                    }
                    else {
                        // if (gr_info.window_switching_flag ...
                        i = 31;
                        ss = 17;
                        sb = 0;
                        while (i >= 0) {
                            if (_Ro[1][i][ss] != 0.0f) {
                                sb = (i << 4) + (i << 1) + ss;
                                i = -1;
                            }
                            else {
                                ss--;
                                if (ss < 0) {
                                    i--;
                                    ss = 17;
                                }
                            }
                        }
                        i = 0;
                        while (_SfBandIndex[_Sfreq].L[i] <= sb)
                            i++;

                        sfb = i;
                        i = _SfBandIndex[_Sfreq].L[i];
                        for (; sfb < 21; sfb++) {
                            sb = _SfBandIndex[_Sfreq].L[sfb + 1] - _SfBandIndex[_Sfreq].L[sfb];
                            for (; sb > 0; sb--) {
                                IsPos[i] = _Scalefac[1].L[sfb];
                                if (IsPos[i] != 7)
                                    if (lsf)
                                        GetKStereoValues(IsPos[i], ioType, i);
                                    else
                                        IsRatio[i] = Tan12[IsPos[i]];
                                i++;
                            }
                        }
                        sfb = _SfBandIndex[_Sfreq].L[20];
                        for (sb = 576 - _SfBandIndex[_Sfreq].L[21]; sb > 0 && i < 576; sb--) {
                            IsPos[i] = IsPos[sfb]; // error here : i >=576

                            if (lsf) {
                                _K[0][i] = _K[0][sfb];
                                _K[1][i] = _K[1][sfb];
                            }
                            else {
                                IsRatio[i] = IsRatio[sfb];
                            }
                            i++;
                        }
                        // if (gr_info.mixed_block_flag)
                    }
                    // if (gr_info.window_switching_flag ...
                }
                // if (i_stereo)

                i = 0;
                for (sb = 0; sb < SBLIMIT; sb++)
                    for (ss = 0; ss < SSLIMIT; ss++) {
                        if (IsPos[i] == 7) {
                            if (msStereo) {
                                _Lr[0][sb][ss] = (_Ro[0][sb][ss] + _Ro[1][sb][ss]) * 0.707106781f;
                                _Lr[1][sb][ss] = (_Ro[0][sb][ss] - _Ro[1][sb][ss]) * 0.707106781f;
                            }
                            else {
                                _Lr[0][sb][ss] = _Ro[0][sb][ss];
                                _Lr[1][sb][ss] = _Ro[1][sb][ss];
                            }
                        }
                        else if (iStereo) {
                            if (lsf) {
                                _Lr[0][sb][ss] = _Ro[0][sb][ss] * _K[0][i];
                                _Lr[1][sb][ss] = _Ro[0][sb][ss] * _K[1][i];
                            }
                            else {
                                _Lr[1][sb][ss] = _Ro[0][sb][ss] / (1 + IsRatio[i]);
                                _Lr[0][sb][ss] = _Lr[1][sb][ss] * IsRatio[i];
                            }
                        }
                        /* else {
                        System.out.println("Error in stereo processing\n");
                        } */
                        i++;
                    }
            }
            // channels == 2
        }

        /// <summary>
        /// *
        /// </summary>
        private void Antialias(int ch, int gr) {
            int sb18, ss, sb18Lim;
            GranuleInfo grInfo = _SideInfo.Channels[ch].Granules[gr];
            // 31 alias-reduction operations between each pair of sub-bands
            // with 8 butterflies between each pair

            if (grInfo.WindowSwitchingFlag != 0 && grInfo.BlockType == 2 && !(grInfo.MixedBlockFlag != 0))
                return;

            if (grInfo.WindowSwitchingFlag != 0 && grInfo.MixedBlockFlag != 0 && grInfo.BlockType == 2) {
                sb18Lim = 18;
            }
            else {
                sb18Lim = 558;
            }

            for (sb18 = 0; sb18 < sb18Lim; sb18 += 18) {
                for (ss = 0; ss < 8; ss++) {
                    int srcIdx1 = sb18 + 17 - ss;
                    int srcIdx2 = sb18 + 18 + ss;
                    float bu = _Out1D[srcIdx1];
                    float bd = _Out1D[srcIdx2];
                    _Out1D[srcIdx1] = bu * Cs[ss] - bd * Ca[ss];
                    _Out1D[srcIdx2] = bd * Cs[ss] + bu * Ca[ss];
                }
            }
        }

        private void Hybrid(int ch, int gr) {
            int bt;
            int sb18;
            GranuleInfo grInfo = _SideInfo.Channels[ch].Granules[gr];
            float[] tsOut;

            float[][] prvblk;

            for (sb18 = 0; sb18 < 576; sb18 += 18) {
                bt = grInfo.WindowSwitchingFlag != 0 && grInfo.MixedBlockFlag != 0 && sb18 < 36 ? 0 : grInfo.BlockType;

                tsOut = _Out1D;
                for (int cc = 0; cc < 18; cc++)
                    TsOutCopy[cc] = tsOut[cc + sb18];
                InverseMdct(TsOutCopy, Rawout, bt);
                for (int cc = 0; cc < 18; cc++)
                    tsOut[cc + sb18] = TsOutCopy[cc];

                // overlap addition
                prvblk = _Prevblck;

                tsOut[0 + sb18] = Rawout[0] + prvblk[ch][sb18 + 0];
                prvblk[ch][sb18 + 0] = Rawout[18];
                tsOut[1 + sb18] = Rawout[1] + prvblk[ch][sb18 + 1];
                prvblk[ch][sb18 + 1] = Rawout[19];
                tsOut[2 + sb18] = Rawout[2] + prvblk[ch][sb18 + 2];
                prvblk[ch][sb18 + 2] = Rawout[20];
                tsOut[3 + sb18] = Rawout[3] + prvblk[ch][sb18 + 3];
                prvblk[ch][sb18 + 3] = Rawout[21];
                tsOut[4 + sb18] = Rawout[4] + prvblk[ch][sb18 + 4];
                prvblk[ch][sb18 + 4] = Rawout[22];
                tsOut[5 + sb18] = Rawout[5] + prvblk[ch][sb18 + 5];
                prvblk[ch][sb18 + 5] = Rawout[23];
                tsOut[6 + sb18] = Rawout[6] + prvblk[ch][sb18 + 6];
                prvblk[ch][sb18 + 6] = Rawout[24];
                tsOut[7 + sb18] = Rawout[7] + prvblk[ch][sb18 + 7];
                prvblk[ch][sb18 + 7] = Rawout[25];
                tsOut[8 + sb18] = Rawout[8] + prvblk[ch][sb18 + 8];
                prvblk[ch][sb18 + 8] = Rawout[26];
                tsOut[9 + sb18] = Rawout[9] + prvblk[ch][sb18 + 9];
                prvblk[ch][sb18 + 9] = Rawout[27];
                tsOut[10 + sb18] = Rawout[10] + prvblk[ch][sb18 + 10];
                prvblk[ch][sb18 + 10] = Rawout[28];
                tsOut[11 + sb18] = Rawout[11] + prvblk[ch][sb18 + 11];
                prvblk[ch][sb18 + 11] = Rawout[29];
                tsOut[12 + sb18] = Rawout[12] + prvblk[ch][sb18 + 12];
                prvblk[ch][sb18 + 12] = Rawout[30];
                tsOut[13 + sb18] = Rawout[13] + prvblk[ch][sb18 + 13];
                prvblk[ch][sb18 + 13] = Rawout[31];
                tsOut[14 + sb18] = Rawout[14] + prvblk[ch][sb18 + 14];
                prvblk[ch][sb18 + 14] = Rawout[32];
                tsOut[15 + sb18] = Rawout[15] + prvblk[ch][sb18 + 15];
                prvblk[ch][sb18 + 15] = Rawout[33];
                tsOut[16 + sb18] = Rawout[16] + prvblk[ch][sb18 + 16];
                prvblk[ch][sb18 + 16] = Rawout[34];
                tsOut[17 + sb18] = Rawout[17] + prvblk[ch][sb18 + 17];
                prvblk[ch][sb18 + 17] = Rawout[35];
            }
        }

        /// <summary>
        /// *
        /// </summary>
        private void DoDownMix() {
            for (int sb = 0; sb < SSLIMIT; sb++) {
                for (int ss = 0; ss < SSLIMIT; ss += 3) {
                    _Lr[0][sb][ss] = (_Lr[0][sb][ss] + _Lr[1][sb][ss]) * 0.5f;
                    _Lr[0][sb][ss + 1] = (_Lr[0][sb][ss + 1] + _Lr[1][sb][ss + 1]) * 0.5f;
                    _Lr[0][sb][ss + 2] = (_Lr[0][sb][ss + 2] + _Lr[1][sb][ss + 2]) * 0.5f;
                }
            }
        }

        /// <summary>
        /// Fast Inverse Modified discrete cosine transform.
        /// </summary>
        internal void InverseMdct(float[] inValues, float[] outValues, int blockType) {
            float tmpf0, tmpf1, tmpf2, tmpf3, tmpf4, tmpf5, tmpf6, tmpf7, tmpf8, tmpf9;
            float tmpf10, tmpf11, tmpf12, tmpf13, tmpf14, tmpf15, tmpf16, tmpf17;

            if (blockType == 2) {
                /*
                *
                * Under MicrosoftVM 2922, This causes a GPF, or
                * At best, an ArrayIndexOutOfBoundsExceptin.
                for(int p=0;p<36;p+=9)
                {
                out[p]   = out[p+1] = out[p+2] = out[p+3] =
                out[p+4] = out[p+5] = out[p+6] = out[p+7] =
                out[p+8] = 0.0f;
                }
                */
                outValues[0] = 0.0f;
                outValues[1] = 0.0f;
                outValues[2] = 0.0f;
                outValues[3] = 0.0f;
                outValues[4] = 0.0f;
                outValues[5] = 0.0f;
                outValues[6] = 0.0f;
                outValues[7] = 0.0f;
                outValues[8] = 0.0f;
                outValues[9] = 0.0f;
                outValues[10] = 0.0f;
                outValues[11] = 0.0f;
                outValues[12] = 0.0f;
                outValues[13] = 0.0f;
                outValues[14] = 0.0f;
                outValues[15] = 0.0f;
                outValues[16] = 0.0f;
                outValues[17] = 0.0f;
                outValues[18] = 0.0f;
                outValues[19] = 0.0f;
                outValues[20] = 0.0f;
                outValues[21] = 0.0f;
                outValues[22] = 0.0f;
                outValues[23] = 0.0f;
                outValues[24] = 0.0f;
                outValues[25] = 0.0f;
                outValues[26] = 0.0f;
                outValues[27] = 0.0f;
                outValues[28] = 0.0f;
                outValues[29] = 0.0f;
                outValues[30] = 0.0f;
                outValues[31] = 0.0f;
                outValues[32] = 0.0f;
                outValues[33] = 0.0f;
                outValues[34] = 0.0f;
                outValues[35] = 0.0f;

                int sixI = 0;

                int i;
                for (i = 0; i < 3; i++) {
                    // 12 point IMDCT
                    // Begin 12 point IDCT
                    // Input aliasing for 12 pt IDCT
                    inValues[15 + i] += inValues[12 + i];
                    inValues[12 + i] += inValues[9 + i];
                    inValues[9 + i] += inValues[6 + i];
                    inValues[6 + i] += inValues[3 + i];
                    inValues[3 + i] += inValues[0 + i];

                    // Input aliasing on odd indices (for 6 point IDCT)
                    inValues[15 + i] += inValues[9 + i];
                    inValues[9 + i] += inValues[3 + i];

                    // 3 point IDCT on even indices
                    float pp1, pp2, sum;
                    pp2 = inValues[12 + i] * 0.500000000f;
                    pp1 = inValues[6 + i] * 0.866025403f;
                    sum = inValues[0 + i] + pp2;
                    tmpf1 = inValues[0 + i] - inValues[12 + i];
                    tmpf0 = sum + pp1;
                    tmpf2 = sum - pp1;

                    // End 3 point IDCT on even indices
                    // 3 point IDCT on odd indices (for 6 point IDCT)
                    pp2 = inValues[15 + i] * 0.500000000f;
                    pp1 = inValues[9 + i] * 0.866025403f;
                    sum = inValues[3 + i] + pp2;
                    tmpf4 = inValues[3 + i] - inValues[15 + i];
                    tmpf5 = sum + pp1;
                    tmpf3 = sum - pp1;
                    // End 3 point IDCT on odd indices
                    // Twiddle factors on odd indices (for 6 point IDCT)

                    tmpf3 *= 1.931851653f;
                    tmpf4 *= 0.707106781f;
                    tmpf5 *= 0.517638090f;

                    // Output butterflies on 2 3 point IDCT's (for 6 point IDCT)
                    float save = tmpf0;
                    tmpf0 += tmpf5;
                    tmpf5 = save - tmpf5;
                    save = tmpf1;
                    tmpf1 += tmpf4;
                    tmpf4 = save - tmpf4;
                    save = tmpf2;
                    tmpf2 += tmpf3;
                    tmpf3 = save - tmpf3;

                    // End 6 point IDCT
                    // Twiddle factors on indices (for 12 point IDCT)

                    tmpf0 *= 0.504314480f;
                    tmpf1 *= 0.541196100f;
                    tmpf2 *= 0.630236207f;
                    tmpf3 *= 0.821339815f;
                    tmpf4 *= 1.306562965f;
                    tmpf5 *= 3.830648788f;

                    // End 12 point IDCT

                    // Shift to 12 point modified IDCT, multiply by window type 2
                    tmpf8 = -tmpf0 * 0.793353340f;
                    tmpf9 = -tmpf0 * 0.608761429f;
                    tmpf7 = -tmpf1 * 0.923879532f;
                    tmpf10 = -tmpf1 * 0.382683432f;
                    tmpf6 = -tmpf2 * 0.991444861f;
                    tmpf11 = -tmpf2 * 0.130526192f;

                    tmpf0 = tmpf3;
                    tmpf1 = tmpf4 * 0.382683432f;
                    tmpf2 = tmpf5 * 0.608761429f;

                    tmpf3 = -tmpf5 * 0.793353340f;
                    tmpf4 = -tmpf4 * 0.923879532f;
                    tmpf5 = -tmpf0 * 0.991444861f;

                    tmpf0 *= 0.130526192f;

                    outValues[sixI + 6] += tmpf0;
                    outValues[sixI + 7] += tmpf1;
                    outValues[sixI + 8] += tmpf2;
                    outValues[sixI + 9] += tmpf3;
                    outValues[sixI + 10] += tmpf4;
                    outValues[sixI + 11] += tmpf5;
                    outValues[sixI + 12] += tmpf6;
                    outValues[sixI + 13] += tmpf7;
                    outValues[sixI + 14] += tmpf8;
                    outValues[sixI + 15] += tmpf9;
                    outValues[sixI + 16] += tmpf10;
                    outValues[sixI + 17] += tmpf11;

                    sixI += 6;
                }
            }
            else {
                // 36 point IDCT
                // input aliasing for 36 point IDCT
                inValues[17] += inValues[16];
                inValues[16] += inValues[15];
                inValues[15] += inValues[14];
                inValues[14] += inValues[13];
                inValues[13] += inValues[12];
                inValues[12] += inValues[11];
                inValues[11] += inValues[10];
                inValues[10] += inValues[9];
                inValues[9] += inValues[8];
                inValues[8] += inValues[7];
                inValues[7] += inValues[6];
                inValues[6] += inValues[5];
                inValues[5] += inValues[4];
                inValues[4] += inValues[3];
                inValues[3] += inValues[2];
                inValues[2] += inValues[1];
                inValues[1] += inValues[0];

                // 18 point IDCT for odd indices
                // input aliasing for 18 point IDCT
                inValues[17] += inValues[15];
                inValues[15] += inValues[13];
                inValues[13] += inValues[11];
                inValues[11] += inValues[9];
                inValues[9] += inValues[7];
                inValues[7] += inValues[5];
                inValues[5] += inValues[3];
                inValues[3] += inValues[1];

                float tmp0, tmp1, tmp2, tmp3, tmp4, tmp0X, tmp1X, tmp2X, tmp3X;
                float tmp0O, tmp1O, tmp2O, tmp3O, tmp4O, tmp0Xo, tmp1Xo, tmp2Xo, tmp3Xo;

                // Fast 9 Point Inverse Discrete Cosine Transform
                //
                // By  Francois-Raymond Boyer
                //         mailto:boyerf@iro.umontreal.ca
                //         http://www.iro.umontreal.ca/~boyerf
                //
                // The code has been optimized for Intel processors
                //  (takes a lot of time to convert float to and from iternal FPU representation)
                //
                // It is a simple "factorization" of the IDCT matrix.

                // 9 point IDCT on even indices

                // 5 points on odd indices (not realy an IDCT)
                float i00 = inValues[0] + inValues[0];
                float iip12 = i00 + inValues[12];

                tmp0 = iip12 + inValues[4] * 1.8793852415718f + inValues[8] * 1.532088886238f +
                       inValues[16] * 0.34729635533386f;
                tmp1 = i00 + inValues[4] - inValues[8] - inValues[12] - inValues[12] - inValues[16];
                tmp2 = iip12 - inValues[4] * 0.34729635533386f - inValues[8] * 1.8793852415718f +
                       inValues[16] * 1.532088886238f;
                tmp3 = iip12 - inValues[4] * 1.532088886238f + inValues[8] * 0.34729635533386f -
                       inValues[16] * 1.8793852415718f;
                tmp4 = inValues[0] - inValues[4] + inValues[8] - inValues[12] + inValues[16];

                // 4 points on even indices
                float i66 = inValues[6] * 1.732050808f; // Sqrt[3]

                tmp0X = inValues[2] * 1.9696155060244f + i66 + inValues[10] * 1.2855752193731f +
                        inValues[14] * 0.68404028665134f;
                tmp1X = (inValues[2] - inValues[10] - inValues[14]) * 1.732050808f;
                tmp2X = inValues[2] * 1.2855752193731f - i66 - inValues[10] * 0.68404028665134f +
                        inValues[14] * 1.9696155060244f;
                tmp3X = inValues[2] * 0.68404028665134f - i66 + inValues[10] * 1.9696155060244f -
                        inValues[14] * 1.2855752193731f;

                // 9 point IDCT on odd indices
                // 5 points on odd indices (not realy an IDCT)
                float i0 = inValues[0 + 1] + inValues[0 + 1];
                float i0P12 = i0 + inValues[12 + 1];

                tmp0O = i0P12 + inValues[4 + 1] * 1.8793852415718f + inValues[8 + 1] * 1.532088886238f +
                        inValues[16 + 1] * 0.34729635533386f;
                tmp1O = i0 + inValues[4 + 1] - inValues[8 + 1] - inValues[12 + 1] - inValues[12 + 1] -
                        inValues[16 + 1];
                tmp2O = i0P12 - inValues[4 + 1] * 0.34729635533386f - inValues[8 + 1] * 1.8793852415718f +
                        inValues[16 + 1] * 1.532088886238f;
                tmp3O = i0P12 - inValues[4 + 1] * 1.532088886238f + inValues[8 + 1] * 0.34729635533386f -
                        inValues[16 + 1] * 1.8793852415718f;
                tmp4O = (inValues[0 + 1] - inValues[4 + 1] + inValues[8 + 1] - inValues[12 + 1] +
                         inValues[16 + 1]) * 0.707106781f; // Twiddled

                // 4 points on even indices
                float i6 = inValues[6 + 1] * 1.732050808f; // Sqrt[3]

                tmp0Xo = inValues[2 + 1] * 1.9696155060244f + i6 + inValues[10 + 1] * 1.2855752193731f +
                         inValues[14 + 1] * 0.68404028665134f;
                tmp1Xo = (inValues[2 + 1] - inValues[10 + 1] - inValues[14 + 1]) * 1.732050808f;
                tmp2Xo = inValues[2 + 1] * 1.2855752193731f - i6 - inValues[10 + 1] * 0.68404028665134f +
                         inValues[14 + 1] * 1.9696155060244f;
                tmp3Xo = inValues[2 + 1] * 0.68404028665134f - i6 + inValues[10 + 1] * 1.9696155060244f -
                         inValues[14 + 1] * 1.2855752193731f;

                // Twiddle factors on odd indices
                // and
                // Butterflies on 9 point IDCT's
                // and
                // twiddle factors for 36 point IDCT

                float e, o;
                e = tmp0 + tmp0X;
                o = (tmp0O + tmp0Xo) * 0.501909918f;
                tmpf0 = e + o;
                tmpf17 = e - o;
                e = tmp1 + tmp1X;
                o = (tmp1O + tmp1Xo) * 0.517638090f;
                tmpf1 = e + o;
                tmpf16 = e - o;
                e = tmp2 + tmp2X;
                o = (tmp2O + tmp2Xo) * 0.551688959f;
                tmpf2 = e + o;
                tmpf15 = e - o;
                e = tmp3 + tmp3X;
                o = (tmp3O + tmp3Xo) * 0.610387294f;
                tmpf3 = e + o;
                tmpf14 = e - o;
                tmpf4 = tmp4 + tmp4O;
                tmpf13 = tmp4 - tmp4O;
                e = tmp3 - tmp3X;
                o = (tmp3O - tmp3Xo) * 0.871723397f;
                tmpf5 = e + o;
                tmpf12 = e - o;
                e = tmp2 - tmp2X;
                o = (tmp2O - tmp2Xo) * 1.183100792f;
                tmpf6 = e + o;
                tmpf11 = e - o;
                e = tmp1 - tmp1X;
                o = (tmp1O - tmp1Xo) * 1.931851653f;
                tmpf7 = e + o;
                tmpf10 = e - o;
                e = tmp0 - tmp0X;
                o = (tmp0O - tmp0Xo) * 5.736856623f;
                tmpf8 = e + o;
                tmpf9 = e - o;

                // end 36 point IDCT */
                // shift to modified IDCT
                float[] winBt = Win[blockType];

                outValues[0] = -tmpf9 * winBt[0];
                outValues[1] = -tmpf10 * winBt[1];
                outValues[2] = -tmpf11 * winBt[2];
                outValues[3] = -tmpf12 * winBt[3];
                outValues[4] = -tmpf13 * winBt[4];
                outValues[5] = -tmpf14 * winBt[5];
                outValues[6] = -tmpf15 * winBt[6];
                outValues[7] = -tmpf16 * winBt[7];
                outValues[8] = -tmpf17 * winBt[8];
                outValues[9] = tmpf17 * winBt[9];
                outValues[10] = tmpf16 * winBt[10];
                outValues[11] = tmpf15 * winBt[11];
                outValues[12] = tmpf14 * winBt[12];
                outValues[13] = tmpf13 * winBt[13];
                outValues[14] = tmpf12 * winBt[14];
                outValues[15] = tmpf11 * winBt[15];
                outValues[16] = tmpf10 * winBt[16];
                outValues[17] = tmpf9 * winBt[17];
                outValues[18] = tmpf8 * winBt[18];
                outValues[19] = tmpf7 * winBt[19];
                outValues[20] = tmpf6 * winBt[20];
                outValues[21] = tmpf5 * winBt[21];
                outValues[22] = tmpf4 * winBt[22];
                outValues[23] = tmpf3 * winBt[23];
                outValues[24] = tmpf2 * winBt[24];
                outValues[25] = tmpf1 * winBt[25];
                outValues[26] = tmpf0 * winBt[26];
                outValues[27] = tmpf0 * winBt[27];
                outValues[28] = tmpf1 * winBt[28];
                outValues[29] = tmpf2 * winBt[29];
                outValues[30] = tmpf3 * winBt[30];
                outValues[31] = tmpf4 * winBt[31];
                outValues[32] = tmpf5 * winBt[32];
                outValues[33] = tmpf6 * winBt[33];
                outValues[34] = tmpf7 * winBt[34];
                outValues[35] = tmpf8 * winBt[35];
            }
        }

        private static float[] CreatePowerTable() {
            float[] powerTable = new float[8192];
            double d43 = 4.0 / 3.0;
            for (int i = 0; i < 8192; i++) {
                powerTable[i] = (float)Math.Pow(i, d43);
            }
            return powerTable;
        }

        internal static int[] Reorder(int[] scalefacBand) {
            // SZD: converted from LAME
            int j = 0;
            int[] ix = new int[576];
            for (int sfb = 0; sfb < 13; sfb++) {
                int start = scalefacBand[sfb];
                int end = scalefacBand[sfb + 1];
                for (int window = 0; window < 3; window++)
                    for (int i = start; i < end; i++)
                        ix[3 * i + window] = j++;
            }
            return ix;
        }
    }
}
