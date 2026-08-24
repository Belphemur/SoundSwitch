// /***************************************************************************
//  * DecoderException.cs
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
using System.Runtime.Serialization;

namespace MP3Sharp.Decoding {
    /// <summary>
    /// The DecoderException represents the class of
    /// errors that can occur when decoding MPEG audio.
    /// </summary>
    [Serializable]
    public class DecoderException : MP3SharpException {
        private int _ErrorCode;

        internal DecoderException(string message, Exception inner) : base(message, inner) {
            InitBlock();
        }

        internal DecoderException(int errorcode, Exception inner) : this(GetErrorString(errorcode), inner) {
            InitBlock();
            _ErrorCode = errorcode;
        }

        protected DecoderException(SerializationInfo info, StreamingContext context) : base(info, context) {
            _ErrorCode = info.GetInt32("ErrorCode");
        }

        internal virtual int ErrorCode => _ErrorCode;

        public override void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null) {
                throw new ArgumentNullException(nameof(info));
            }

            info.AddValue("ErrorCode", _ErrorCode);
            base.GetObjectData(info, context);
        }

        private void InitBlock() {
            _ErrorCode = DecoderErrors.UNKNOWN_ERROR;
        }

        internal static string GetErrorString(int errorcode) =>
            // REVIEW: use resource file to map error codes
            // to locale-sensitive strings. 
            "Decoder errorcode " + Convert.ToString(errorcode, 16);
    }
}