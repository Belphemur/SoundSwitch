// /***************************************************************************
//  * GranuleInfo.cs
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

namespace MP3Sharp.Decoding.Decoders.LayerIII {
    public class GranuleInfo {
        internal int BigValues;
        internal int BlockType;
        internal int Count1TableSelect;
        internal int GlobalGain;
        internal int MixedBlockFlag;
        internal int Part23Length;
        internal int Preflag;
        internal int Region0Count;
        internal int Region1Count;
        internal int ScaleFacCompress;
        internal int ScaleFacScale;
        internal int[] SubblockGain;
        internal int[] TableSelect;
        internal int WindowSwitchingFlag;

        internal GranuleInfo() {
            TableSelect = new int[3];
            SubblockGain = new int[3];
        }
    }
}