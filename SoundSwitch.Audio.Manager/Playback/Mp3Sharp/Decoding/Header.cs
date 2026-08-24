// /***************************************************************************
//  * Header.cs
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

using System.Text;
using MP3Sharp.Support;

namespace MP3Sharp.Decoding {
    /// <summary>
    /// public class for extracting information from a frame header.
    /// TODO: move strings into resources.
    /// </summary>
    public class Header {
        /// <summary>
        /// Constant for MPEG-2 LSF version
        /// </summary>
        internal const int MPEG2_LSF = 0;

        internal const int MPEG25_LSF = 2; // SZD

        /// <summary>
        /// Constant for MPEG-1 version
        /// </summary>
        internal const int MPEG1 = 1;

        internal const int STEREO = 0;
        internal const int JOINT_STEREO = 1;
        internal const int DUAL_CHANNEL = 2;
        internal const int SINGLE_CHANNEL = 3;
        internal const int FOURTYFOUR_POINT_ONE = 0;
        internal const int FOURTYEIGHT = 1;
        internal const int THIRTYTWO = 2;

        internal static readonly int[][] Frequencies = {
            new[] {22050, 24000, 16000, 1}, new[] {44100, 48000, 32000, 1},
            new[] {11025, 12000, 8000, 1}
        };

        internal static readonly int[][][] Bitrates = {
            new[] {
                new[] {
                    0, 32000, 48000, 56000, 64000, 80000, 96000, 112000, 128000, 144000, 160000, 176000, 192000, 224000,
                    256000, 0
                },
                new[] {
                    0, 8000, 16000, 24000, 32000, 40000, 48000, 56000, 64000, 80000, 96000, 112000, 128000, 144000,
                    160000,
                    0
                },
                new[] {
                    0, 8000, 16000, 24000, 32000, 40000, 48000, 56000, 64000, 80000, 96000, 112000, 128000, 144000,
                    160000,
                    0
                }
            },
            new[] {
                new[] {
                    0, 32000, 64000, 96000, 128000, 160000, 192000, 224000, 256000, 288000, 320000, 352000, 384000,
                    416000,
                    448000, 0
                },
                new[] {
                    0, 32000, 48000, 56000, 64000, 80000, 96000, 112000, 128000, 160000, 192000, 224000, 256000, 320000,
                    384000, 0
                },
                new[] {
                    0, 32000, 40000, 48000, 56000, 64000, 80000, 96000, 112000, 128000, 160000, 192000, 224000, 256000,
                    320000, 0
                }
            },
            new[] {
                new[] {
                    0, 32000, 48000, 56000, 64000, 80000, 96000, 112000, 128000, 144000, 160000, 176000, 192000, 224000,
                    256000, 0
                },
                new[] {
                    0, 8000, 16000, 24000, 32000, 40000, 48000, 56000, 64000, 80000, 96000, 112000, 128000, 144000,
                    160000,
                    0
                },
                new[] {
                    0, 8000, 16000, 24000, 32000, 40000, 48000, 56000, 64000, 80000, 96000, 112000, 128000, 144000,
                    160000,
                    0
                }
            }
        };
        
        internal static readonly string[][][] BitrateStr = {
            new[] {
                new[] {
                    "free format", "32 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s", "80 kbit/s", "96 kbit/s",
                    "112 kbit/s", "128 kbit/s", "144 kbit/s", "160 kbit/s", "176 kbit/s", "192 kbit/s", "224 kbit/s",
                    "256 kbit/s", "forbidden"
                },
                new[] {
                    "free format", "8 kbit/s", "16 kbit/s", "24 kbit/s", "32 kbit/s", "40 kbit/s", "48 kbit/s",
                    "56 kbit/s",
                    "64 kbit/s", "80 kbit/s", "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s", "160 kbit/s",
                    "forbidden"
                },
                new[] {
                    "free format", "8 kbit/s", "16 kbit/s", "24 kbit/s", "32 kbit/s", "40 kbit/s", "48 kbit/s",
                    "56 kbit/s",
                    "64 kbit/s", "80 kbit/s", "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s", "160 kbit/s",
                    "forbidden"
                }
            },
            new[] {
                new[] {
                    "free format", "32 kbit/s", "64 kbit/s", "96 kbit/s", "128 kbit/s", "160 kbit/s", "192 kbit/s",
                    "224 kbit/s", "256 kbit/s", "288 kbit/s", "320 kbit/s", "352 kbit/s", "384 kbit/s", "416 kbit/s",
                    "448 kbit/s", "forbidden"
                },
                new[] {
                    "free format", "32 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s", "80 kbit/s", "96 kbit/s",
                    "112 kbit/s", "128 kbit/s", "160 kbit/s", "192 kbit/s", "224 kbit/s", "256 kbit/s", "320 kbit/s",
                    "384 kbit/s", "forbidden"
                },
                new[] {
                    "free format", "32 kbit/s", "40 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s", "80 kbit/s",
                    "96 kbit/s", "112 kbit/s", "128 kbit/s", "160 kbit/s", "192 kbit/s", "224 kbit/s", "256 kbit/s",
                    "320 kbit/s", "forbidden"
                }
            },
            new[] {
                new[] {
                    "free format", "32 kbit/s", "48 kbit/s", "56 kbit/s", "64 kbit/s", "80 kbit/s", "96 kbit/s",
                    "112 kbit/s", "128 kbit/s", "144 kbit/s", "160 kbit/s", "176 kbit/s", "192 kbit/s", "224 kbit/s",
                    "256 kbit/s", "forbidden"
                },
                new[] {
                    "free format", "8 kbit/s", "16 kbit/s", "24 kbit/s", "32 kbit/s", "40 kbit/s", "48 kbit/s",
                    "56 kbit/s",
                    "64 kbit/s", "80 kbit/s", "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s", "160 kbit/s",
                    "forbidden"
                },
                new[] {
                    "free format", "8 kbit/s", "16 kbit/s", "24 kbit/s", "32 kbit/s", "40 kbit/s", "48 kbit/s",
                    "56 kbit/s",
                    "64 kbit/s", "80 kbit/s", "96 kbit/s", "112 kbit/s", "128 kbit/s", "144 kbit/s", "160 kbit/s",
                    "forbidden"
                }
            }
        };

        internal short Checksum;
        internal int NSlots;

        private Crc16 _CRC;
        internal int Framesize;
        private bool _Copyright, _Original;
        private int _Headerstring = -1;
        private int _Layer, _ProtectionBit, _BitrateIndex, _PaddingBit, _ModeExtension;
        private int _Mode;
        private int _NumberOfSubbands, _IntensityStereoBound;
        private int _SampleFrequency;
        private sbyte _Syncmode;
        private int _Version;

        internal Header() {
            InitBlock();
        }

        /// <summary>
        /// Returns synchronized header.
        /// </summary>
        internal virtual int SyncHeader => _Headerstring;

        private void InitBlock() {
            _Syncmode = Bitstream.INITIAL_SYNC;
        }

        public override string ToString() {
            StringBuilder buffer = new StringBuilder(200);
            buffer.Append("Layer ");
            buffer.Append(LayerAsString());
            buffer.Append(" frame ");
            buffer.Append(ModeAsString());
            buffer.Append(' ');
            buffer.Append(VersionAsString());
            if (!IsProtection())
                buffer.Append(" no");
            buffer.Append(" checksums");
            buffer.Append(' ');
            buffer.Append(SampleFrequencyAsString());
            buffer.Append(',');
            buffer.Append(' ');
            buffer.Append(BitrateAsString());

            string s = buffer.ToString();
            return s;
        }

        /// <summary>
        /// Read a 32-bit header from the bitstream.
        /// </summary>
        internal void read_header(Bitstream stream, Crc16[] crcp) {
            int headerstring;
            int channelBitrate;

            bool sync = false;

            do {
                headerstring = stream.SyncHeader(_Syncmode);
                _Headerstring = headerstring;

                if (_Syncmode == Bitstream.INITIAL_SYNC) {
                    _Version = SupportClass.URShift(headerstring, 19) & 1;
                    if ((SupportClass.URShift(headerstring, 20) & 1) == 0)
                        // SZD: MPEG2.5 detection
                        if (_Version == MPEG2_LSF)
                            _Version = MPEG25_LSF;
                        else
                            throw stream.NewBitstreamException(BitstreamErrors.UNKNOWN_ERROR);

                    if ((_SampleFrequency = SupportClass.URShift(headerstring, 10) & 3) == 3) {
                        throw stream.NewBitstreamException(BitstreamErrors.UNKNOWN_ERROR);
                    }
                }

                _Layer = 4 - SupportClass.URShift(headerstring, 17) & 3;
                _ProtectionBit = SupportClass.URShift(headerstring, 16) & 1;
                _BitrateIndex = SupportClass.URShift(headerstring, 12) & 0xF;
                _PaddingBit = SupportClass.URShift(headerstring, 9) & 1;
                _Mode = SupportClass.URShift(headerstring, 6) & 3;
                _ModeExtension = SupportClass.URShift(headerstring, 4) & 3;
                if (_Mode == JOINT_STEREO)
                    _IntensityStereoBound = (_ModeExtension << 2) + 4;
                else
                    _IntensityStereoBound = 0;
                // should never be used
                _Copyright |= (SupportClass.URShift(headerstring, 3) & 1) == 1;
                _Original |= (SupportClass.URShift(headerstring, 2) & 1) == 1;

                // calculate number of subbands:
                if (_Layer == 1)
                    _NumberOfSubbands = 32;
                else {
                    channelBitrate = _BitrateIndex;
                    // calculate bitrate per channel:
                    if (_Mode != SINGLE_CHANNEL)
                        if (channelBitrate == 4)
                            channelBitrate = 1;
                        else
                            channelBitrate -= 4;

                    if (channelBitrate == 1 || channelBitrate == 2)
                        if (_SampleFrequency == THIRTYTWO)
                            _NumberOfSubbands = 12;
                        else
                            _NumberOfSubbands = 8;
                    else if (_SampleFrequency == FOURTYEIGHT || channelBitrate >= 3 && channelBitrate <= 5)
                        _NumberOfSubbands = 27;
                    else
                        _NumberOfSubbands = 30;
                }
                if (_IntensityStereoBound > _NumberOfSubbands)
                    _IntensityStereoBound = _NumberOfSubbands;
                // calculate framesize and nSlots
                CalculateFrameSize();

                // read framedata:
                stream.Read_frame_data(Framesize);

                if (stream.IsSyncCurrentPosition(_Syncmode)) {
                    if (_Syncmode == Bitstream.INITIAL_SYNC) {
                        _Syncmode = Bitstream.STRICT_SYNC;
                        stream.SetSyncWord(headerstring & unchecked((int)0xFFF80CC0));
                    }
                    sync = true;
                }
                else {
                    stream.UnreadFrame();
                }
            } while (!sync);

            stream.ParseFrame();

            if (_ProtectionBit == 0) {
                // frame contains a crc checksum
                Checksum = (short)stream.GetBitsFromBuffer(16);
                if (_CRC == null)
                    _CRC = new Crc16();
                _CRC.AddBits(headerstring, 16);
                crcp[0] = _CRC;
            }
            else
                crcp[0] = null;
            if (_SampleFrequency == FOURTYFOUR_POINT_ONE) {
                /*
                if (offset == null)
                {
                int max = max_number_of_frames(stream);
                offset = new int[max];
                for(int i=0; i<max; i++) offset[i] = 0;
                }
                // Bizarre, y avait ici une acollade ouvrante
                int cf = stream.current_frame();
                int lf = stream.last_frame();
                if ((cf > 0) && (cf == lf))
                {
                offset[cf] = offset[cf-1] + h_padding_bit;
                }
                else
                {
                offset[0] = h_padding_bit;
                }
                */
            }
        }

        // Functions to query header contents:
        /// <summary>
        /// Returns version.
        /// </summary>
        internal int Version() => _Version;

        /// <summary>
        /// Returns Layer ID.
        /// </summary>
        internal int Layer() => _Layer;

        /// <summary>
        /// Returns bitrate index.
        /// </summary>
        internal int bitrate_index() => _BitrateIndex;

        /// <summary>
        /// Returns Sample Frequency.
        /// </summary>
        internal int sample_frequency() => _SampleFrequency;

        /// <summary>
        /// Returns Frequency.
        /// </summary>
        internal int Frequency() => Frequencies[_Version][_SampleFrequency];

        /// <summary>
        /// Returns Mode.
        /// </summary>
        internal int Mode() => _Mode;

        /// <summary>
        /// Returns Protection bit.
        /// </summary>
        internal bool IsProtection() {
            if (_ProtectionBit == 0)
                return true;
            return false;
        }

        /// <summary>
        /// Returns Copyright.
        /// </summary>
        internal bool IsCopyright() => _Copyright;

        /// <summary>
        /// Returns Original.
        /// </summary>
        internal bool IsOriginal() => _Original;

        /// <summary>
        /// Returns Checksum flag.
        /// Compares computed checksum with stream checksum.
        /// </summary>
        internal bool IsChecksumOK() => Checksum == _CRC.Checksum();

        // Seeking and layer III stuff
        /// <summary>
        /// Returns Layer III Padding bit.
        /// </summary>
        internal bool IsPadding() {
            if (_PaddingBit == 0)
                return false;
            return true;
        }

        /// <summary>
        /// Returns Slots.
        /// </summary>
        internal int Slots() => NSlots;

        /// <summary>
        /// Returns Mode Extension.
        /// </summary>
        internal int mode_extension() => _ModeExtension;
        
        /// <summary>
        /// Calculate Frame size.
        /// Calculates framesize in bytes excluding header size.
        /// </summary>
        internal int CalculateFrameSize() {
            if (_Layer == 1) {
                Framesize = 12 * Bitrates[_Version][0][_BitrateIndex] / Frequencies[_Version][_SampleFrequency];
                if (_PaddingBit != 0)
                    Framesize++;
                Framesize <<= 2; // one slot is 4 bytes long
                NSlots = 0;
            }
            else {
                Framesize = 144 * Bitrates[_Version][_Layer - 1][_BitrateIndex] /
                            Frequencies[_Version][_SampleFrequency];
                if (_Version == MPEG2_LSF || _Version == MPEG25_LSF)
                    Framesize >>= 1;
                // SZD
                if (_PaddingBit != 0)
                    Framesize++;
                // Layer III slots
                if (_Layer == 3) {
                    if (_Version == MPEG1) {
                        NSlots = Framesize - (_Mode == SINGLE_CHANNEL ? 17 : 32) - (_ProtectionBit != 0 ? 0 : 2) -
                                 4; // header size
                    }
                    else {
                        // MPEG-2 LSF, SZD: MPEG-2.5 LSF
                        NSlots = Framesize - (_Mode == SINGLE_CHANNEL ? 9 : 17) - (_ProtectionBit != 0 ? 0 : 2) -
                                 4; // header size
                    }
                }
                else {
                    NSlots = 0;
                }
            }
            Framesize -= 4; // subtract header size
            return Framesize;
        }

        /// <summary>
        /// Returns the maximum number of frames in the stream.
        /// </summary>
        internal int MaxNumberOfFrame(int streamsize) {
            if (Framesize + 4 - _PaddingBit == 0)
                return 0;
            return streamsize / (Framesize + 4 - _PaddingBit);
        }

        /// <summary>
        /// Returns the maximum number of frames in the stream.
        /// </summary>
        internal int min_number_of_frames(int streamsize) {
            if (Framesize + 5 - _PaddingBit == 0)
                return 0;
            return streamsize / (Framesize + 5 - _PaddingBit);
        }

        /// <summary>
        /// Returns ms/frame.
        /// </summary>
        internal float MsPerFrame() {
            float[][] msPerFrameArray = {
                new[] {8.707483f, 8.0f, 12.0f}, new[] {26.12245f, 24.0f, 36.0f},
                new[] {26.12245f, 24.0f, 36.0f}
            };
            return msPerFrameArray[_Layer - 1][_SampleFrequency];
        }

        /// <summary>
        /// Returns total ms.
        /// </summary>
        internal float TotalMS(int streamsize) => MaxNumberOfFrame(streamsize) * MsPerFrame();

        // functions which return header informations as strings:
        /// <summary>
        /// Return Layer version.
        /// </summary>
        internal string LayerAsString() {
            switch (_Layer) {
                case 1:
                    return "I";

                case 2:
                    return "II";

                case 3:
                    return "III";
            }
            return null;
        }

        /// <summary>
        /// Returns Bitrate.
        /// </summary>
        internal string BitrateAsString() => BitrateStr[_Version][_Layer - 1][_BitrateIndex];

        /// <summary>
        /// Returns Frequency
        /// </summary>
        internal string SampleFrequencyAsString() {
            switch (_SampleFrequency) {
                case THIRTYTWO:
                    if (_Version == MPEG1)
                        return "32 kHz";
                    if (_Version == MPEG2_LSF)
                        return "16 kHz";
                    return "8 kHz";
                case FOURTYFOUR_POINT_ONE:
                    if (_Version == MPEG1)
                        return "44.1 kHz";
                    if (_Version == MPEG2_LSF)
                        return "22.05 kHz";
                    return "11.025 kHz";
                case FOURTYEIGHT:
                    if (_Version == MPEG1)
                        return "48 kHz";
                    if (_Version == MPEG2_LSF)
                        return "24 kHz";
                    return "12 kHz";
            }
            return null;
        }

        /// <summary>
        /// Returns Mode.
        /// </summary>
        internal string ModeAsString() {
            switch (_Mode) {
                case STEREO:
                    return "Stereo";
                case JOINT_STEREO:
                    return "Joint stereo";
                case DUAL_CHANNEL:
                    return "Dual channel";
                case SINGLE_CHANNEL:
                    return "Single channel";
            }
            return null;
        }

        /// <summary>
        /// Returns Version.
        /// </summary>
        internal string VersionAsString() {
            switch (_Version) {
                case MPEG1:
                    return "MPEG-1";

                case MPEG2_LSF:
                    return "MPEG-2 LSF";

                case MPEG25_LSF:
                    return "MPEG-2.5 LSF";
            }
            return null;
        }

        /// <summary>
        /// Returns the number of subbands in the current frame.
        /// </summary>
        internal int NumberSubbands() => _NumberOfSubbands;

        /// <summary>
        /// Returns Intensity Stereo.
        /// Layer II joint stereo only).
        /// Returns the number of subbands which are in stereo mode,
        /// subbands above that limit are in intensity stereo mode.
        /// </summary>
        internal int IntensityStereoBound() => _IntensityStereoBound;
    }
}