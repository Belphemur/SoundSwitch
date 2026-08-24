// /***************************************************************************
//  * CircularByteBuffer.cs
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

namespace MP3Sharp.Decoding {
    [Serializable]
    internal class CircularByteBuffer {
        private byte[] _Buffer;
        private int _Index;
        private int _Length;
        private int _NumValid;

        internal CircularByteBuffer(int size) {
            _Buffer = new byte[size];
            _Length = size;
        }

        /// <summary>
        /// Initialize by copying the CircularByteBuffer passed in
        /// </summary>
        internal CircularByteBuffer(CircularByteBuffer cdb) {
            lock (cdb) {
                _Length = cdb._Length;
                _NumValid = cdb._NumValid;
                _Index = cdb._Index;
                _Buffer = new byte[_Length];
                for (int c = 0; c < _Length; c++) {
                    _Buffer[c] = cdb._Buffer[c];
                }
            }
        }

        /// <summary>
        /// The physical size of the Buffer (read/write)
        /// </summary>
        internal int BufferSize {
            get => _Length;
            set {
                byte[] newDataArray = new byte[value];

                int minLength = _Length > value ? value : _Length;
                for (int i = 0; i < minLength; i++) {
                    newDataArray[i] = InternalGet(i - _Length + 1);
                }
                _Buffer = newDataArray;
                _Index = minLength - 1;
                _Length = value;
            }
        }

        /// <summary>
        /// e.g. Offset[0] is the current value
        /// </summary>
        internal byte this[int index] {
            get => InternalGet(-1 - index);
            set => InternalSet(-1 - index, value);
        }

        /// <summary>
        /// How far back it is safe to look (read/write).  Write only to reduce NumValid.
        /// </summary>
        internal int NumValid {
            get => _NumValid;
            set {
                if (value > _NumValid)
                    throw new Exception("Can't set NumValid to " + value +
                                        " which is greater than the current numValid value of " + _NumValid);
                _NumValid = value;
            }
        }

        internal CircularByteBuffer Copy() => new CircularByteBuffer(this);

        internal void Reset() {
            _Index = 0;
            _NumValid = 0;
        }

        /// <summary>
        /// Push a byte into the buffer.  Returns the value of whatever comes off.
        /// </summary>
        internal byte Push(byte newValue) {
            byte ret;
            lock (this) {
                ret = InternalGet(_Length);
                _Buffer[_Index] = newValue;
                _NumValid++;
                if (_NumValid > _Length) _NumValid = _Length;
                _Index++;
                _Index %= _Length;
            }
            return ret;
        }

        /// <summary>
        /// Pop an integer off the start of the buffer. Throws an exception if the buffer is empty (NumValid == 0)
        /// </summary>
        internal byte Pop() {
            lock (this) {
                if (_NumValid == 0) throw new Exception("Can't pop off an empty CircularByteBuffer");
                _NumValid--;
                return this[_NumValid];
            }
        }

        /// <summary>
        /// Returns what would fall out of the buffer on a Push.  NOT the same as what you'd get with a Pop().
        /// </summary>
        internal byte Peek() {
            lock (this) {
                return InternalGet(_Length);
            }
        }

        private byte InternalGet(int offset) {
            int ind = _Index + offset;

            // Do thin modulo (should just drop through)
            for (; ind >= _Length; ind -= _Length) { }
            for (; ind < 0; ind += _Length) { }
            // Set value
            return _Buffer[ind];
        }

        private void InternalSet(int offset, byte valueToSet) {
            int ind = _Index + offset;

            // Do thin modulo (should just drop through)
            for (; ind > _Length; ind -= _Length) { }

            for (; ind < 0; ind += _Length) { }
            // Set value
            _Buffer[ind] = valueToSet;
        }

        /// <summary>
        /// Returns a range (in terms of Offsets) in an int array in chronological (oldest-to-newest) order. e.g. (3, 0)
        /// returns the last four ints pushed, with result[3] being the most recent.
        /// </summary>
        internal byte[] GetRange(int str, int stp) {
            byte[] outByte = new byte[str - stp + 1];

            for (int i = str, j = 0; i >= stp; i--, j++) {
                outByte[j] = this[i];
            }

            return outByte;
        }

        public override string ToString() {
            string ret = "";
            for (int i = 0; i < _Buffer.Length; i++) {
                ret += _Buffer[i] + " ";
            }
            ret += "\n index = " + _Index + " numValid = " + NumValid;
            return ret;
        }
    }
}