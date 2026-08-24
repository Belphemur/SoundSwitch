// /***************************************************************************
//  * SoundFormat.cs
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

namespace MP3Sharp {
    /// <summary>
    /// Describes sound formats that can be produced by the Mp3Stream class.
    /// </summary>
    public enum SoundFormat {
        /// <summary>
        /// PCM encoded, 16-bit Mono sound format.
        /// </summary>
        Pcm16BitMono,

        /// <summary>
        /// PCM encoded, 16-bit Stereo sound format.
        /// </summary>
        Pcm16BitStereo
    }
}
