// /***************************************************************************
//  * Crc16.cs
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

using MP3Sharp.Support;

namespace MP3Sharp.Decoding {
    /// <summary>
    /// 16-Bit CRC checksum
    /// </summary>
    public sealed class Crc16 {
        private static readonly short Polynomial;
        private short _CRC;

        static Crc16() {
            Polynomial = (short)SupportClass.Identity(0x8005);
        }

        internal Crc16() {
            _CRC = (short)SupportClass.Identity(0xFFFF);
        }

        /// <summary>
        /// Feed a bitstring to the crc calculation (length between 0 and 32, not inclusive).
        /// </summary>
        internal void AddBits(int bitstring, int length) {
            int bitmask = 1 << (length - 1);
            do
                if (((_CRC & 0x8000) == 0) ^ ((bitstring & bitmask) == 0)) {
                    _CRC <<= 1;
                    _CRC ^= Polynomial;
                }
                else
                    _CRC <<= 1;
            while ((bitmask = SupportClass.URShift(bitmask, 1)) != 0);
        }

        /// <summary>
        /// Return the calculated checksum.
        /// Erase it for next calls to add_bits().
        /// </summary>
        internal short Checksum() {
            short sum = _CRC;
            _CRC = (short)SupportClass.Identity(0xFFFF);
            return sum;
        }
    }
}