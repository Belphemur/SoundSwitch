// /***************************************************************************
//  * ScaleFactorData.cs
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
    public class ScaleFactorData {
        internal int[] L; /* [cb] */
        internal int[][] S; /* [window][cb] */

        internal ScaleFactorData() {
            L = new int[23];
            S = new int[3][];
            for (int i = 0; i < 3; i++) {
                S[i] = new int[13];
            }
        }
    }
}