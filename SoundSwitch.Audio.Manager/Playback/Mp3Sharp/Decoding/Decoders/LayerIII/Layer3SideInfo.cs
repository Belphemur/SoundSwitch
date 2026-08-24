// /***************************************************************************
//  * Layer3SideInfo.cs
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
    public class Layer3SideInfo {
        internal ChannelData[] Channels;
        internal int MainDataBegin;
        internal int PrivateBits;

        internal Layer3SideInfo() {
            Channels = new ChannelData[2];
            Channels[0] = new ChannelData();
            Channels[1] = new ChannelData();
        }
    }
}