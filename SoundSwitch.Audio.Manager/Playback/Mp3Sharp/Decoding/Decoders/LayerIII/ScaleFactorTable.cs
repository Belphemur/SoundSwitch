// /***************************************************************************
//  * ScaleFactorTable.cs
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
    public class ScaleFactorTable {
        internal int[] L;
        internal int[] S;

        private LayerIIIDecoder _EnclosingInstance;

        internal ScaleFactorTable(LayerIIIDecoder enclosingInstance) {
            InitBlock(enclosingInstance);
            L = new int[5];
            S = new int[3];
        }

        internal ScaleFactorTable(LayerIIIDecoder enclosingInstance, int[] thel, int[] thes) {
            InitBlock(enclosingInstance);
            L = thel;
            S = thes;
        }

        internal LayerIIIDecoder EnclosingInstance => _EnclosingInstance;

        private void InitBlock(LayerIIIDecoder enclosingInstance) {
            _EnclosingInstance = enclosingInstance;
        }
    }
}