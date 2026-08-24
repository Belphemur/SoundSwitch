// /***************************************************************************
//  * BitReserve.cs
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
    /// Implementation of Bit Reservoir for Layer III.
    /// The implementation stores single bits as a word in the buffer. If
    /// a bit is set, the corresponding word in the buffer will be non-zero.
    /// If a bit is clear, the corresponding word is zero. Although this
    /// may seem waseful, this can be a factor of two quicker than
    /// packing 8 bits to a byte and extracting.
    /// </summary>

    // REVIEW: there is no range checking, so buffer underflow or overflow
    // can silently occur.
    internal sealed class BitReserve {
        /// <summary>
        /// Size of the internal buffer to store the reserved bits.
        /// Must be a power of 2. And x8, as each bit is stored as a single
        /// entry.
        /// </summary>
        private const int BUFSIZE = 4096 * 8;

        /// <summary>
        /// Mask that can be used to quickly implement the
        /// modulus operation on BUFSIZE.
        /// </summary>
        private const int BUFSIZE_MASK = BUFSIZE - 1;

        private int[] _Buffer;
        private int _Offset, _Totbit, _BufByteIdx;

        internal BitReserve() {
            InitBlock();

            _Offset = 0;
            _Totbit = 0;
            _BufByteIdx = 0;
        }

        private void InitBlock() {
            _Buffer = new int[BUFSIZE];
        }

        /// <summary>
        /// Return totbit Field.
        /// </summary>
        internal int HssTell() => _Totbit;

        /// <summary>
        /// Read a number bits from the bit stream.
        /// </summary>
        internal int ReadBits(int n) {
            _Totbit += n;

            int val = 0;

            int pos = _BufByteIdx;
            if (pos + n < BUFSIZE) {
                while (n-- > 0) {
                    val <<= 1;
                    val |= _Buffer[pos++] != 0 ? 1 : 0;
                }
            }
            else {
                while (n-- > 0) {
                    val <<= 1;
                    val |= _Buffer[pos] != 0 ? 1 : 0;
                    pos = (pos + 1) & BUFSIZE_MASK;
                }
            }
            _BufByteIdx = pos;
            return val;
        }

        /// <summary>
        /// Read 1 bit from the bit stream.
        /// </summary>
        internal int ReadOneBit() {
            _Totbit++;
            int val = _Buffer[_BufByteIdx];
            _BufByteIdx = (_BufByteIdx + 1) & BUFSIZE_MASK;
            return val;
        }

        /// <summary>
        /// Write 8 bits into the bit stream.
        /// </summary>
        internal void PutBuffer(int val) {
            int ofs = _Offset;
            _Buffer[ofs++] = val & 0x80;
            _Buffer[ofs++] = val & 0x40;
            _Buffer[ofs++] = val & 0x20;
            _Buffer[ofs++] = val & 0x10;
            _Buffer[ofs++] = val & 0x08;
            _Buffer[ofs++] = val & 0x04;
            _Buffer[ofs++] = val & 0x02;
            _Buffer[ofs++] = val & 0x01;
            if (ofs == BUFSIZE)
                _Offset = 0;
            else
                _Offset = ofs;
        }

        /// <summary>
        /// Rewind n bits in Stream.
        /// </summary>
        internal void RewindStreamBits(int bitCount) {
            _Totbit -= bitCount;
            _BufByteIdx -= bitCount;
            if (_BufByteIdx < 0)
                _BufByteIdx += BUFSIZE;
        }

        /// <summary>
        /// Rewind n bytes in Stream.
        /// </summary>
        internal void RewindStreamBytes(int byteCount) {
            int bits = byteCount << 3;
            _Totbit -= bits;
            _BufByteIdx -= bits;
            if (_BufByteIdx < 0)
                _BufByteIdx += BUFSIZE;
        }
    }
}