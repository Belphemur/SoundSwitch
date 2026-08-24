// /***************************************************************************
//  * RiffFile.cs
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

using System.IO;
using MP3Sharp.Support;

namespace MP3Sharp.IO {
    /// <summary>
    /// public class to manage RIFF files
    /// </summary>
    public class RiffFile {
        protected const int DDC_SUCCESS = 0; // The operation succeded
        protected const int DDC_FAILURE = 1; // The operation failed for unspecified reasons
        protected const int DDC_OUT_OF_MEMORY = 2; // Operation failed due to running out of memory
        protected const int DDC_FILE_ERROR = 3; // Operation encountered file I/O error
        protected const int DDC_INVALID_CALL = 4; // Operation was called with invalid parameters
        protected const int DDC_USER_ABORT = 5; // Operation was aborted by the user
        protected const int DDC_INVALID_FILE = 6; // File format does not match
        protected const int RF_UNKNOWN = 0; // undefined type (can use to mean "N/A" or "not open")
        protected const int RF_WRITE = 1; // open for write
        protected const int RF_READ = 2; // open for read
        private readonly RiffChunkHeader _RiffHeader; // header for whole file
        protected int Fmode; // current file I/O mode
        private Stream _File; // I/O stream to use

        internal RiffFile() {
            _File = null;
            Fmode = RF_UNKNOWN;
            _RiffHeader = new RiffChunkHeader(this);

            _RiffHeader.CkId = FourCC("RIFF");
            _RiffHeader.CkSize = 0;
        }

        /// <summary>
        /// Return File Mode.
        /// </summary>
        internal int CurrentFileMode() => Fmode;

        /// <summary>
        /// Open a RIFF file.
        /// </summary>
        internal virtual int Open(string filename, int newMode) {
            int retcode = DDC_SUCCESS;

            if (Fmode != RF_UNKNOWN) {
                retcode = Close();
            }

            if (retcode == DDC_SUCCESS) {
                switch (newMode) {
                    case RF_WRITE:
                        try {
                            _File = RandomAccessFileStream.CreateRandomAccessFile(filename, "rw");

                            try {
                                // Write the RIFF header...
                                // We will have to come back later and patch it!
                                sbyte[] br = new sbyte[8];
                                br[0] = (sbyte)((SupportClass.URShift(_RiffHeader.CkId, 24)) & 0x000000FF);
                                br[1] = (sbyte)((SupportClass.URShift(_RiffHeader.CkId, 16)) & 0x000000FF);
                                br[2] = (sbyte)((SupportClass.URShift(_RiffHeader.CkId, 8)) & 0x000000FF);
                                br[3] = (sbyte)(_RiffHeader.CkId & 0x000000FF);

                                sbyte br4 = (sbyte)((SupportClass.URShift(_RiffHeader.CkSize, 24)) & 0x000000FF);
                                sbyte br5 = (sbyte)((SupportClass.URShift(_RiffHeader.CkSize, 16)) & 0x000000FF);
                                sbyte br6 = (sbyte)((SupportClass.URShift(_RiffHeader.CkSize, 8)) & 0x000000FF);
                                sbyte br7 = (sbyte)(_RiffHeader.CkSize & 0x000000FF);

                                br[4] = br7;
                                br[5] = br6;
                                br[6] = br5;
                                br[7] = br4;

                                _File.Write(SupportClass.ToByteArray(br), 0, 8);
                                Fmode = RF_WRITE;
                            }
                            catch {
                                _File.Close();
                                Fmode = RF_UNKNOWN;
                            }
                        }
                        catch {
                            Fmode = RF_UNKNOWN;
                            retcode = DDC_FILE_ERROR;
                        }
                        break;

                    case RF_READ:
                        try {
                            _File = RandomAccessFileStream.CreateRandomAccessFile(filename, "r");
                            try {
                                // Try to read the RIFF header...
                                sbyte[] br = new sbyte[8];
                                SupportClass.ReadInput(_File, ref br, 0, 8);
                                Fmode = RF_READ;
                                _RiffHeader.CkId = ((br[0] << 24) & (int)SupportClass.Identity(0xFF000000)) |
                                                   ((br[1] << 16) & 0x00FF0000) | ((br[2] << 8) & 0x0000FF00) |
                                                   (br[3] & 0x000000FF);
                                _RiffHeader.CkSize = ((br[4] << 24) & (int)SupportClass.Identity(0xFF000000)) |
                                                     ((br[5] << 16) & 0x00FF0000) | ((br[6] << 8) & 0x0000FF00) |
                                                     (br[7] & 0x000000FF);
                            }
                            catch {
                                _File.Close();
                                Fmode = RF_UNKNOWN;
                            }
                        }
                        catch {
                            Fmode = RF_UNKNOWN;
                            retcode = DDC_FILE_ERROR;
                        }
                        break;

                    default:
                        retcode = DDC_INVALID_CALL;
                        break;
                }
            }
            return retcode;
        }

        /// <summary>
        /// Open a RIFF STREAM.
        /// </summary>
        internal virtual int Open(Stream stream, int newMode) {
            int retcode = DDC_SUCCESS;

            if (Fmode != RF_UNKNOWN) {
                retcode = Close();
            }

            if (retcode == DDC_SUCCESS) {
                switch (newMode) {
                    case RF_WRITE:
                        try {
                            //file = SupportClass.RandomAccessFileSupport.CreateRandomAccessFile(Filename, "rw");
                            _File = stream;

                            try {
                                // Write the RIFF header...
                                // We will have to come back later and patch it!
                                sbyte[] br = new sbyte[8];
                                br[0] = (sbyte)((SupportClass.URShift(_RiffHeader.CkId, 24)) & 0x000000FF);
                                br[1] = (sbyte)((SupportClass.URShift(_RiffHeader.CkId, 16)) & 0x000000FF);
                                br[2] = (sbyte)((SupportClass.URShift(_RiffHeader.CkId, 8)) & 0x000000FF);
                                br[3] = (sbyte)(_RiffHeader.CkId & 0x000000FF);

                                sbyte br4 = (sbyte)((SupportClass.URShift(_RiffHeader.CkSize, 24)) & 0x000000FF);
                                sbyte br5 = (sbyte)((SupportClass.URShift(_RiffHeader.CkSize, 16)) & 0x000000FF);
                                sbyte br6 = (sbyte)((SupportClass.URShift(_RiffHeader.CkSize, 8)) & 0x000000FF);
                                sbyte br7 = (sbyte)(_RiffHeader.CkSize & 0x000000FF);

                                br[4] = br7;
                                br[5] = br6;
                                br[6] = br5;
                                br[7] = br4;

                                _File.Write(SupportClass.ToByteArray(br), 0, 8);
                                Fmode = RF_WRITE;
                            }
                            catch {
                                _File.Close();
                                Fmode = RF_UNKNOWN;
                            }
                        }
                        catch {
                            Fmode = RF_UNKNOWN;
                            retcode = DDC_FILE_ERROR;
                        }
                        break;

                    case RF_READ:
                        try {
                            _File = stream;
                            //file = SupportClass.RandomAccessFileSupport.CreateRandomAccessFile(Filename, "r");
                            try {
                                // Try to read the RIFF header... 
                                sbyte[] br = new sbyte[8];
                                SupportClass.ReadInput(_File, ref br, 0, 8);
                                Fmode = RF_READ;
                                _RiffHeader.CkId = ((br[0] << 24) & (int)SupportClass.Identity(0xFF000000)) |
                                                   ((br[1] << 16) & 0x00FF0000) | ((br[2] << 8) & 0x0000FF00) |
                                                   (br[3] & 0x000000FF);
                                _RiffHeader.CkSize = ((br[4] << 24) & (int)SupportClass.Identity(0xFF000000)) |
                                                     ((br[5] << 16) & 0x00FF0000) | ((br[6] << 8) & 0x0000FF00) |
                                                     (br[7] & 0x000000FF);
                            }
                            catch {
                                _File.Close();
                                Fmode = RF_UNKNOWN;
                            }
                        }
                        catch {
                            Fmode = RF_UNKNOWN;
                            retcode = DDC_FILE_ERROR;
                        }
                        break;

                    default:
                        retcode = DDC_INVALID_CALL;
                        break;
                }
            }
            return retcode;
        }

        /// <summary>
        /// Write NumBytes data.
        /// </summary>
        internal virtual int Write(sbyte[] data, int numBytes) {
            if (Fmode != RF_WRITE) {
                return DDC_INVALID_CALL;
            }
            try {
                _File.Write(SupportClass.ToByteArray(data), 0, numBytes);
                Fmode = RF_WRITE;
            }
            catch {
                return DDC_FILE_ERROR;
            }
            _RiffHeader.CkSize += numBytes;
            return DDC_SUCCESS;
        }

        /// <summary>
        /// Write NumBytes data.
        /// </summary>
        internal virtual int Write(short[] data, int numBytes) {
            sbyte[] theData = new sbyte[numBytes];
            int yc = 0;
            for (int y = 0; y < numBytes; y = y + 2) {
                theData[y] = (sbyte)(data[yc] & 0x00FF);
                theData[y + 1] = (sbyte)((SupportClass.URShift(data[yc++], 8)) & 0x00FF);
            }
            if (Fmode != RF_WRITE) {
                return DDC_INVALID_CALL;
            }
            try {
                _File.Write(SupportClass.ToByteArray(theData), 0, numBytes);
                Fmode = RF_WRITE;
            }
            catch {
                return DDC_FILE_ERROR;
            }
            _RiffHeader.CkSize += numBytes;
            return DDC_SUCCESS;
        }

        /// <summary>
        /// Write NumBytes data.
        /// </summary>
        internal virtual int Write(RiffChunkHeader riffHeader, int numBytes) {
            sbyte[] br = new sbyte[8];
            br[0] = (sbyte)((SupportClass.URShift(riffHeader.CkId, 24)) & 0x000000FF);
            br[1] = (sbyte)((SupportClass.URShift(riffHeader.CkId, 16)) & 0x000000FF);
            br[2] = (sbyte)((SupportClass.URShift(riffHeader.CkId, 8)) & 0x000000FF);
            br[3] = (sbyte)(riffHeader.CkId & 0x000000FF);

            sbyte br4 = (sbyte)((SupportClass.URShift(riffHeader.CkSize, 24)) & 0x000000FF);
            sbyte br5 = (sbyte)((SupportClass.URShift(riffHeader.CkSize, 16)) & 0x000000FF);
            sbyte br6 = (sbyte)((SupportClass.URShift(riffHeader.CkSize, 8)) & 0x000000FF);
            sbyte br7 = (sbyte)(riffHeader.CkSize & 0x000000FF);

            br[4] = br7;
            br[5] = br6;
            br[6] = br5;
            br[7] = br4;

            if (Fmode != RF_WRITE) {
                return DDC_INVALID_CALL;
            }
            try {
                _File.Write(SupportClass.ToByteArray(br), 0, numBytes);
                Fmode = RF_WRITE;
            }
            catch {
                return DDC_FILE_ERROR;
            }
            _RiffHeader.CkSize += numBytes;
            return DDC_SUCCESS;
        }

        /// <summary>
        /// Write NumBytes data.
        /// </summary>
        internal virtual int Write(short data, int numBytes) {
            short theData = data; //(short) (((SupportClass.URShift(data, 8)) & 0x00FF) | ((Data << 8) & 0xFF00));
            if (Fmode != RF_WRITE) {
                return DDC_INVALID_CALL;
            }
            try {
                BinaryWriter tempBinaryWriter = new BinaryWriter(_File);
                tempBinaryWriter.Write(theData);
                Fmode = RF_WRITE;
            }
            catch {
                return DDC_FILE_ERROR;
            }
            _RiffHeader.CkSize += numBytes;
            return DDC_SUCCESS;
        }

        /// <summary>
        /// Write NumBytes data.
        /// </summary>
        internal virtual int Write(int data, int numBytes) {
            int theData = data;
            if (Fmode != RF_WRITE) {
                return DDC_INVALID_CALL;
            }
            try {
                BinaryWriter tempBinaryWriter = new BinaryWriter(_File);
                tempBinaryWriter.Write(theData);
                Fmode = RF_WRITE;
            }
            catch {
                return DDC_FILE_ERROR;
            }
            _RiffHeader.CkSize += numBytes;
            return DDC_SUCCESS;
        }

        /// <summary>
        /// Read NumBytes data.
        /// </summary>
        internal virtual int Read(sbyte[] data, int numBytes) {
            int retcode = DDC_SUCCESS;
            try {
                SupportClass.ReadInput(_File, ref data, 0, numBytes);
            }
            catch {
                retcode = DDC_FILE_ERROR;
            }
            return retcode;
        }

        /// <summary>
        /// Expect NumBytes data.
        /// </summary>
        internal virtual int Expect(string data, int numBytes) {
            int cnt = 0;
            try {
                while ((numBytes--) != 0) {
                    sbyte target = (sbyte)_File.ReadByte();
                    if (target != data[cnt++])
                        return DDC_FILE_ERROR;
                }
            }
            catch {
                return DDC_FILE_ERROR;
            }
            return DDC_SUCCESS;
        }

        /// <summary>
        /// Close Riff File.
        /// Length is written too.
        /// </summary>
        internal virtual int Close() {
            int retcode = DDC_SUCCESS;

            switch (Fmode) {
                case RF_WRITE:
                    try {
                        _File.Seek(0, SeekOrigin.Begin);
                        try {
                            sbyte[] br = new sbyte[8];
                            br[0] = (sbyte)((SupportClass.URShift(_RiffHeader.CkId, 24)) & 0x000000FF);
                            br[1] = (sbyte)((SupportClass.URShift(_RiffHeader.CkId, 16)) & 0x000000FF);
                            br[2] = (sbyte)((SupportClass.URShift(_RiffHeader.CkId, 8)) & 0x000000FF);
                            br[3] = (sbyte)(_RiffHeader.CkId & 0x000000FF);

                            br[7] = (sbyte)((SupportClass.URShift(_RiffHeader.CkSize, 24)) & 0x000000FF);
                            br[6] = (sbyte)((SupportClass.URShift(_RiffHeader.CkSize, 16)) & 0x000000FF);
                            br[5] = (sbyte)((SupportClass.URShift(_RiffHeader.CkSize, 8)) & 0x000000FF);
                            br[4] = (sbyte)(_RiffHeader.CkSize & 0x000000FF);
                            _File.Write(SupportClass.ToByteArray(br), 0, 8);
                            _File.Close();
                        }
                        catch {
                            retcode = DDC_FILE_ERROR;
                        }
                    }
                    catch {
                        retcode = DDC_FILE_ERROR;
                    }
                    break;

                case RF_READ:
                    try {
                        _File.Close();
                    }
                    catch {
                        retcode = DDC_FILE_ERROR;
                    }
                    break;
            }
            _File = null;
            Fmode = RF_UNKNOWN;
            return retcode;
        }

        /// <summary>
        /// Return File Position.
        /// </summary>
        internal virtual long CurrentFilePosition() {
            long position;
            try {
                position = _File.Position;
            }
            catch {
                position = -1;
            }
            return position;
        }

        /// <summary>
        /// Write Data to specified offset.
        /// </summary>
        internal virtual int Backpatch(long fileOffset, RiffChunkHeader data, int numBytes) {
            if (_File == null) {
                return DDC_INVALID_CALL;
            }
            try {
                _File.Seek(fileOffset, SeekOrigin.Begin);
            }
            catch {
                return DDC_FILE_ERROR;
            }
            return Write(data, numBytes);
        }

        internal virtual int Backpatch(long fileOffset, sbyte[] data, int numBytes) {
            if (_File == null) {
                return DDC_INVALID_CALL;
            }
            try {
                _File.Seek(fileOffset, SeekOrigin.Begin);
            }
            catch {
                return DDC_FILE_ERROR;
            }
            return Write(data, numBytes);
        }

        /// <summary>
        /// Seek in the File.
        /// </summary>
        protected virtual int Seek(long offset) {
            int rc;
            try {
                _File.Seek(offset, SeekOrigin.Begin);
                rc = DDC_SUCCESS;
            }
            catch {
                rc = DDC_FILE_ERROR;
            }
            return rc;
        }

        /// <summary>
        /// Fill the header.
        /// </summary>
        internal static int FourCC(string chunkName) {
            sbyte[] p = {0x20, 0x20, 0x20, 0x20};
            SupportClass.GetSBytesFromString(chunkName, 0, 4, ref p, 0);
            int ret = (((p[0] << 24) & (int)SupportClass.Identity(0xFF000000)) | ((p[1] << 16) & 0x00FF0000) |
                       ((p[2] << 8) & 0x0000FF00) | (p[3] & 0x000000FF));
            return ret;
        }

        public class RiffChunkHeader {
            internal int CkId; // Four-character chunk ID
            internal int CkSize;

            private RiffFile _EnclosingInstance;

            // Length of data in chunk
            internal RiffChunkHeader(RiffFile enclosingInstance) {
                InitBlock(enclosingInstance);
            }

            internal RiffFile EnclosingInstance => _EnclosingInstance;

            private void InitBlock(RiffFile enclosingInstance) {
                _EnclosingInstance = enclosingInstance;
            }
        }
    }
}