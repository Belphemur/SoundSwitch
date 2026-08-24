// /***************************************************************************
//  * BitstreamErrors.cs
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

namespace MP3Sharp.Decoding {
    /// <summary>
    /// This struct describes all error codes that can be thrown
    /// in BistreamExceptions.
    /// </summary>
    internal struct BitstreamErrors {
        internal const int UNKNOWN_ERROR = BITSTREAM_ERROR + 0;
        internal const int UNKNOWN_SAMPLE_RATE = BITSTREAM_ERROR + 1;
        internal const int STREA_ERROR = BITSTREAM_ERROR + 2;
        internal const int UNEXPECTED_EOF = BITSTREAM_ERROR + 3;
        internal const int STREAM_EOF = BITSTREAM_ERROR + 4;
        internal const int BITSTREAM_LAST = 0x1ff;

        internal const int BITSTREAM_ERROR = 0x100;
        internal const int DECODER_ERROR = 0x200;
    }
}