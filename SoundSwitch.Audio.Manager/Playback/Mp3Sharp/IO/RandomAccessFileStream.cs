// /***************************************************************************
//  * RandomAccessFileStream.cs
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

namespace MP3Sharp.IO {
    public class RandomAccessFileStream {
        internal static FileStream CreateRandomAccessFile(string fileName, string mode) {
            FileStream newFile;

            if (string.Compare(mode, "rw", StringComparison.Ordinal) == 0)
                newFile = new FileStream(fileName, FileMode.OpenOrCreate,
                    FileAccess.ReadWrite);
            else if (string.Compare(mode, "r", StringComparison.Ordinal) == 0)
                newFile = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            else
                throw new ArgumentException();

            return newFile;
        }
    }
}