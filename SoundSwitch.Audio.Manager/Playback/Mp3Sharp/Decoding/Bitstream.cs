// /***************************************************************************
//  * Bitstream.cs
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
using System.IO;
using MP3Sharp.Support;

namespace MP3Sharp.Decoding {
    /// <summary>
    /// The Bistream class is responsible for parsing an MPEG audio bitstream.
    /// REVIEW: much of the parsing currently occurs in the various decoders.
    /// This should be moved into this class and associated inner classes.
    /// </summary>
    public sealed class Bitstream {
        /// <summary>
        /// Maximum size of the frame buffer:
        /// 1730 bytes per frame: 144 * 384kbit/s / 32000 Hz + 2 Bytes CRC
        /// </summary>
        private const int BUFFER_INT_SIZE = 433;

        /// <summary>
        /// Synchronization control constant for the initial
        /// synchronization to the start of a frame.
        /// </summary>
        internal const sbyte INITIAL_SYNC = 0;

        /// <summary>
        /// Synchronization control constant for non-inital frame
        /// synchronizations.
        /// </summary>
        internal const sbyte STRICT_SYNC = 1;

        private readonly int[] _Bitmask = {
            0x00000000, 0x00000001, 0x00000003, 0x00000007, 0x0000000F, 0x0000001F, 0x0000003F, 0x0000007F,
            0x000000FF, 0x000001FF, 0x000003FF, 0x000007FF, 0x00000FFF, 0x00001FFF, 0x00003FFF, 0x00007FFF,
            0x0000FFFF, 0x0001FFFF
        };

        private readonly PushbackStream _SourceStream;

        /// <summary>
        /// Number (0-31, from MSB to LSB) of next bit for get_bits()
        /// </summary>
        private int _BitIndex;

        private Crc16[] _CRC;

        /// <summary>
        /// The bytes read from the stream.
        /// </summary>
        private sbyte[] _FrameBytes;

        /// <summary>
        /// The frame buffer that holds the data for the current frame.
        /// </summary>
        private int[] _FrameBuffer;

        /// <summary>
        /// Number of valid bytes in the frame buffer.
        /// </summary>
        private int _FrameSize;

        private Header _Header;

        private bool _SingleChMode;

        private sbyte[] _SyncBuffer;

        /// <summary>
        /// The current specified syncword
        /// </summary>
        private int _SyncWord;

        /// <summary>
        /// Index into framebuffer where the next bits are retrieved.
        /// </summary>
        private int _WordPointer;

        /// <summary>
        /// Create a IBitstream that reads data from a given InputStream.
        /// </summary>
        internal Bitstream(PushbackStream stream) {
            InitBlock();
            _SourceStream = stream ?? throw new NullReferenceException("in stream is null");
            CloseFrame();
        }

        private void InitBlock() {
            _CRC = new Crc16[1];
            _SyncBuffer = new sbyte[4];
            _FrameBytes = new sbyte[BUFFER_INT_SIZE * 4];
            _FrameBuffer = new int[BUFFER_INT_SIZE];
            _Header = new Header();
        }

        internal void Close() {
            try {
                _SourceStream.Close();
            }
            catch (IOException ex) {
                throw NewBitstreamException(BitstreamErrors.STREA_ERROR, ex);
            }
        }

        /// <summary>
        /// Reads and parses the next frame from the input source.
        /// </summary>
        /// <returns>
        /// The Header describing details of the frame read,
        /// or null if the end of the stream has been reached.
        /// </returns>
        internal Header ReadFrame() {
            Header result = null;
            try {
                result = ReadNextFrame();
            }
            catch (BitstreamException ex) {
                if (ex.ErrorCode != BitstreamErrors.STREAM_EOF) {
                    // wrap original exception so stack trace is maintained.
                    throw NewBitstreamException(ex.ErrorCode, ex);
                }
            }
            return result;
        }

        private Header ReadNextFrame() {
            if (_FrameSize == -1) {
                // entire frame is read by the header class.
                _Header.read_header(this, _CRC);
            }

            return _Header;
        }

        /// <summary>
        /// Unreads the bytes read from the frame.
        /// Throws BitstreamException.
        /// REVIEW: add new error codes for this.
        /// </summary>
        internal void UnreadFrame() {
            if (_WordPointer == -1 && _BitIndex == -1 && _FrameSize > 0) {
                try {
                    _SourceStream.UnRead(_FrameSize);
                }
                catch {
                    throw NewBitstreamException(BitstreamErrors.STREA_ERROR);
                }
            }
        }

        internal void CloseFrame() {
            _FrameSize = -1;
            _WordPointer = -1;
            _BitIndex = -1;
        }

        /// <summary>
        /// Determines if the next 4 bytes of the stream represent a frame header.
        /// </summary>
        internal bool IsSyncCurrentPosition(int syncmode) {
            int read = ReadBytes(_SyncBuffer, 0, 4);
            int headerstring = ((_SyncBuffer[0] << 24) & (int)SupportClass.Identity(0xFF000000)) |
                               ((_SyncBuffer[1] << 16) & 0x00FF0000) | ((_SyncBuffer[2] << 8) & 0x0000FF00) |
                               ((_SyncBuffer[3] << 0) & 0x000000FF);

            try {
                _SourceStream.UnRead(read);
            }
            catch (Exception e) {
                throw new MP3SharpException("Could not restore file after reading frame header.", e);
            }

            bool sync = false;
            switch (read) {
                case 0:
                    sync = true;
                    break;

                case 4:
                    sync = IsSyncMark(headerstring, syncmode, _SyncWord);
                    break;
            }

            return sync;
        }

        // REVIEW: this class should provide inner classes to
        // parse the frame contents. Eventually, readBits will
        // be removed.
        internal int ReadBits(int n) => GetBitsFromBuffer(n);

        // REVIEW: implement CRC check.
        internal int ReadCheckedBits(int n) => GetBitsFromBuffer(n);

        internal BitstreamException NewBitstreamException(int errorcode) => new BitstreamException(errorcode, null);

        internal BitstreamException NewBitstreamException(int errorcode, Exception throwable) => new BitstreamException(errorcode, throwable);

        /// <summary>
        /// Get next 32 bits from bitstream.
        /// They are stored in the headerstring.
        /// syncmod allows Synchro flag ID
        /// The returned value is False at the end of stream.
        /// </summary>
        internal int SyncHeader(sbyte syncmode) {
            bool sync = false;
            // read additional 2 bytes
            int bytesRead = ReadBytes(_SyncBuffer, 0, 3);
            if (bytesRead != 3) {
                throw NewBitstreamException(BitstreamErrors.STREAM_EOF, null);
            }

            int headerstring = ((_SyncBuffer[0] << 16) & 0x00FF0000) | ((_SyncBuffer[1] << 8) & 0x0000FF00) |
                           ((_SyncBuffer[2] << 0) & 0x000000FF);

            do {
                headerstring <<= 8;
                if (ReadBytes(_SyncBuffer, 3, 1) != 1) {
                    throw NewBitstreamException(BitstreamErrors.STREAM_EOF, null);
                }
                headerstring |= _SyncBuffer[3] & 0x000000FF;
                if (CheckAndSkipId3Tag(headerstring)) {
                    bytesRead = ReadBytes(_SyncBuffer, 0, 3);
                    if (bytesRead != 3) {
                        throw NewBitstreamException(BitstreamErrors.STREAM_EOF, null);
                    }
                    headerstring = ((_SyncBuffer[0] << 16) & 0x00FF0000) | ((_SyncBuffer[1] << 8) & 0x0000FF00) |
                                   ((_SyncBuffer[2] << 0) & 0x000000FF);
                    continue;
                }
                sync = IsSyncMark(headerstring, syncmode, _SyncWord);
            } while (!sync);

            return headerstring;
        }
        /// <summary>
        /// check and skip the id3v2 tag.
        /// mp3 frame sync inside id3 tag may led false decodeing.
        /// id3 tag do have a flag for "unsynchronisation", indicate there are no
        /// frame sync inside tags, scence decoder don't care about tags, we just
        /// skip all tags.
        /// </summary>
        internal bool CheckAndSkipId3Tag(int headerstring) {
            bool id3 = (headerstring & 0xFFFFFF00) == 0x49443300;

            if (id3) {
                sbyte[] id3_header = new sbyte[6];

                if (ReadBytes(id3_header, 0, 6) != 6) {
                    throw NewBitstreamException(BitstreamErrors.STREAM_EOF, null);
                }
                // id3 header uses 4 bytes to store the size of all tags,
                // but only the low 7 bits of each byte is used, to avoid
                // mp3 frame sync.
                int id3_tag_size = 0;
                id3_tag_size |= id3_header[2] & 0x0000007F; id3_tag_size <<= 7;
                id3_tag_size |= id3_header[3] & 0x0000007F; id3_tag_size <<= 7;
                id3_tag_size |= id3_header[4] & 0x0000007F; id3_tag_size <<= 7;
                id3_tag_size |= id3_header[5] & 0x0000007F;

                sbyte[] id3_tag = new sbyte[id3_tag_size];

                if (ReadBytes(id3_tag, 0, id3_tag_size) != id3_tag_size) {
                    throw NewBitstreamException(BitstreamErrors.STREAM_EOF, null);
                }
            }
            return id3;
        }

        internal bool IsSyncMark(int headerstring, int syncmode, int word) {
            bool sync;
            if (syncmode == INITIAL_SYNC) {
                //sync =  ((headerstring & 0xFFF00000) == 0xFFF00000);
                sync = (headerstring & 0xFFE00000) == 0xFFE00000; // SZD: MPEG 2.5
            }
            else {
                //sync = ((headerstring & 0xFFF80C00) == word) 
                sync = (headerstring & 0xFFE00000) == 0xFFE00000 // ROB -- THIS IS PROBABLY WRONG. A WEAKER CHECK.
                       && (headerstring & 0x000000C0) == 0x000000C0 == _SingleChMode;
            }

            // filter out invalid sample rate
            if (sync) {
                sync = (SupportClass.URShift(headerstring, 10) & 3) != 3;
                // if (!sync) Trace.WriteLine("INVALID SAMPLE RATE DETECTED", "Bitstream");
            }
            // filter out invalid layer
            if (sync) {
                sync = (SupportClass.URShift(headerstring, 17) & 3) != 0;
                // if (!sync) Trace.WriteLine("INVALID LAYER DETECTED", "Bitstream");
            }
            // filter out invalid version
            if (sync) {
                sync = (SupportClass.URShift(headerstring, 19) & 3) != 1;
                // if (!sync) Console.WriteLine("INVALID VERSION DETECTED");
            }
            return sync;
        }

        /// <summary>
        /// Reads the data for the next frame. The frame is not parsed
        /// until parse frame is called.
        /// </summary>
        internal void Read_frame_data(int bytesize) {
            ReadFully(_FrameBytes, 0, bytesize);
            _FrameSize = bytesize;
            _WordPointer = -1;
            _BitIndex = -1;
        }

        /// <summary>
        /// Parses the data previously read with read_frame_data().
        /// </summary>
        internal void ParseFrame() {
            // Convert Bytes read to int
            int b = 0;
            sbyte[] byteread = _FrameBytes;
            int bytesize = _FrameSize;

            for (int k = 0; k < bytesize; k = k + 4) {
                sbyte b0 = byteread[k];
                sbyte b1 = 0;
                sbyte b2 = 0;
                sbyte b3 = 0;
                if (k + 1 < bytesize)
                    b1 = byteread[k + 1];
                if (k + 2 < bytesize)
                    b2 = byteread[k + 2];
                if (k + 3 < bytesize)
                    b3 = byteread[k + 3];
                _FrameBuffer[b++] = ((b0 << 24) & (int)SupportClass.Identity(0xFF000000)) | ((b1 << 16) & 0x00FF0000) |
                                    ((b2 << 8) & 0x0000FF00) | (b3 & 0x000000FF);
            }

            _WordPointer = 0;
            _BitIndex = 0;
        }

        /// <summary>
        /// Read bits from buffer into the lower bits of an unsigned int.
        /// The LSB contains the latest read bit of the stream.
        /// (between 1 and 16, inclusive).
        /// </summary>
        internal int GetBitsFromBuffer(int countBits) {
            int returnvalue;
            int sum = _BitIndex + countBits;
            if (_WordPointer < 0) {
                _WordPointer = 0;
            }
            if (sum <= 32) {
                // all bits contained in *wordpointer
                returnvalue = SupportClass.URShift(_FrameBuffer[_WordPointer], 32 - sum) & _Bitmask[countBits];
                if ((_BitIndex += countBits) == 32) {
                    _BitIndex = 0;
                    _WordPointer++;
                }
                return returnvalue;
            }
            int right = _FrameBuffer[_WordPointer] & 0x0000FFFF;
            _WordPointer++;
            int left = _FrameBuffer[_WordPointer] & (int)SupportClass.Identity(0xFFFF0000);
            returnvalue = ((right << 16) & (int)SupportClass.Identity(0xFFFF0000)) | (SupportClass.URShift(left, 16) & 0x0000FFFF);
            returnvalue = SupportClass.URShift(returnvalue, 48 - sum);
            returnvalue &= _Bitmask[countBits];
            _BitIndex = sum - 32;
            return returnvalue;
        }

        /// <summary>
        /// Set the word we want to sync the header to.
        /// In Big-Endian byte order
        /// </summary>
        internal void SetSyncWord(int syncword0) {
            _SyncWord = syncword0 & unchecked((int)0xFFFFFF3F);
            _SingleChMode = (syncword0 & 0x000000C0) == 0x000000C0;
        }

        /// <summary>
        /// Reads the exact number of bytes from the source input stream into a byte array.
        /// </summary>
        private void ReadFully(sbyte[] b, int offs, int len) {
            try {
                while (len > 0) {
                    int bytesread = _SourceStream.Read(b, offs, len);
                    if (bytesread == -1 || bytesread == 0) // t/DD -- .NET returns 0 at end-of-stream!
                    {
                        // t/DD: this really SHOULD throw an exception here...
                        // Trace.WriteLine("readFully -- returning success at EOF? (" + bytesread + ")", "Bitstream");
                        while (len-- > 0) {
                            b[offs++] = 0;
                        }
                        break;
                        //throw newBitstreamException(UNEXPECTED_EOF, new EOFException());
                    }

                    offs += bytesread;
                    len -= bytesread;
                }
            }
            catch (IOException ex) {
                throw NewBitstreamException(BitstreamErrors.STREA_ERROR, ex);
            }
        }

        /// <summary>
        /// Simlar to readFully, but doesn't throw exception when EOF is reached.
        /// </summary>
        private int ReadBytes(sbyte[] b, int offs, int len) {
            int totalBytesRead = 0;
            try {
                while (len > 0) {
                    int bytesread = _SourceStream.Read(b, offs, len);
                    // for (int i = 0; i < len; i++) b[i] = (sbyte)Temp[i];
                    if (bytesread == -1 || bytesread == 0) {
                        break;
                    }
                    totalBytesRead += bytesread;
                    offs += bytesread;
                    len -= bytesread;
                }
            }
            catch (IOException ex) {
                throw NewBitstreamException(BitstreamErrors.STREA_ERROR, ex);
            }
            return totalBytesRead;
        }
    }
}
