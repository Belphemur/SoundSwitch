// /***************************************************************************
//  * OutputChannels.cs
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
    /// <summary>
    /// A Type-safe representation of the the supported output channel
    /// constants. This class is immutable and, hence, is thread safe.
    /// </summary>
    /// <author>
    /// Mat McGowan
    /// </author>
    public class OutputChannels {
        /// <summary>
        /// Flag to indicate output should include both channels.
        /// </summary>
        internal const int BOTH_CHANNELS = 0;

        /// <summary>
        /// Flag to indicate output should include the left channel only.
        /// </summary>
        internal const int LEFT_CHANNEL = 1;

        /// <summary>
        /// Flag to indicate output should include the right channel only.
        /// </summary>
        internal const int RIGHT_CHANNEL = 2;

        /// <summary>
        /// Flag to indicate output is mono.
        /// </summary>
        internal const int DOWNMIX_CHANNELS = 3;

        internal static readonly OutputChannels Left = new OutputChannels(LEFT_CHANNEL);
        internal static readonly OutputChannels Right = new OutputChannels(RIGHT_CHANNEL);
        internal static readonly OutputChannels Both = new OutputChannels(BOTH_CHANNELS);
        internal static readonly OutputChannels DownMix = new OutputChannels(DOWNMIX_CHANNELS);
        private readonly int _OutputChannels;

        private OutputChannels(int channels) {
            _OutputChannels = channels;

            if (channels < 0 || channels > 3) {
                throw new ArgumentException("channels");
            }
        }

        /// <summary>
        /// Retrieves the code representing the desired output channels.
        /// Will be one of LEFT_CHANNEL, RIGHT_CHANNEL, BOTH_CHANNELS
        /// or DOWNMIX_CHANNELS.
        /// </summary>
        /// <returns>
        /// the channel code represented by this instance.
        /// </returns>
        internal virtual int ChannelsOutputCode => _OutputChannels;

        /// <summary>
        /// Retrieves the number of output channels represented
        /// by this channel output type.
        /// </summary>
        /// <returns>
        /// The number of output channels for this channel output
        /// type. This will be 2 for BOTH_CHANNELS only, and 1
        /// for all other types.
        /// </returns>
        internal virtual int ChannelCount {
            get {
                int count = _OutputChannels == BOTH_CHANNELS ? 2 : 1;
                return count;
            }
        }

        /// <summary>
        /// Creates an OutputChannels instance
        /// corresponding to the given channel code.
        /// </summary>
        /// <param name="code">
        /// one of the OutputChannels channel code constants.
        /// @throws IllegalArgumentException if code is not a valid
        /// channel code.
        /// </param>
        internal static OutputChannels FromInt(int code) {
            switch (code) {
                case (int)OutputChannelsEnum.LeftChannel:
                    return Left;

                case (int)OutputChannelsEnum.RightChannel:
                    return Right;

                case (int)OutputChannelsEnum.BothChannels:
                    return Both;

                case (int)OutputChannelsEnum.DownmixChannels:
                    return DownMix;

                default:
                    throw new ArgumentException("Invalid channel code: " + code);
            }
        }

        public override bool Equals(object obj) {
            bool equals = false;

            if (obj is OutputChannels oc) {
                equals = oc._OutputChannels == _OutputChannels;
            }

            return equals;
        }

        public override int GetHashCode() => _OutputChannels;
    }
}