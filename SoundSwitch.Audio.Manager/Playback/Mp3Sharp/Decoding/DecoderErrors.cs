// /***************************************************************************
//  * DecoderErrors.cs
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
    /// This interface provides constants describing the error
    /// codes used by the Decoder to indicate errors.
    /// </summary>
    internal struct DecoderErrors {
        internal const int UNKNOWN_ERROR = BitstreamErrors.DECODER_ERROR + 0;
        internal const int UNSUPPORTED_LAYER = BitstreamErrors.DECODER_ERROR + 1;
    }
}