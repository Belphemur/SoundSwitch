// /***************************************************************************
//  * ASubband.cs
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

namespace MP3Sharp.Decoding.Decoders {
    /// <summary>
    /// Abstract base class for subband classes of layer I and II
    /// </summary>
    public abstract class ASubband {
        /*
        *  Changes from version 1.1 to 1.2:
        *    - array size increased by one, although a scalefactor with index 63
        *      is illegal (to prevent segmentation faults)
        */
        // Scalefactors for layer I and II, Annex 3-B.1 in ISO/IEC DIS 11172:
        internal static readonly float[] ScaleFactors = {
            2.00000000000000f, 1.58740105196820f, 1.25992104989487f, 1.00000000000000f, 0.79370052598410f,
            0.62996052494744f, 0.50000000000000f, 0.39685026299205f, 0.31498026247372f, 0.25000000000000f,
            0.19842513149602f, 0.15749013123686f, 0.12500000000000f, 0.09921256574801f, 0.07874506561843f,
            0.06250000000000f, 0.04960628287401f, 0.03937253280921f, 0.03125000000000f, 0.02480314143700f,
            0.01968626640461f, 0.01562500000000f, 0.01240157071850f, 0.00984313320230f, 0.00781250000000f,
            0.00620078535925f, 0.00492156660115f, 0.00390625000000f, 0.00310039267963f, 0.00246078330058f,
            0.00195312500000f, 0.00155019633981f, 0.00123039165029f, 0.00097656250000f, 0.00077509816991f,
            0.00061519582514f, 0.00048828125000f, 0.00038754908495f, 0.00030759791257f, 0.00024414062500f,
            0.00019377454248f, 0.00015379895629f, 0.00012207031250f, 0.00009688727124f, 0.00007689947814f,
            0.00006103515625f, 0.00004844363562f, 0.00003844973907f, 0.00003051757813f, 0.00002422181781f,
            0.00001922486954f, 0.00001525878906f, 0.00001211090890f, 0.00000961243477f, 0.00000762939453f,
            0.00000605545445f, 0.00000480621738f, 0.00000381469727f, 0.00000302772723f, 0.00000240310869f,
            0.00000190734863f, 0.00000151386361f, 0.00000120155435f, 0.00000000000000f
        };

        internal abstract void ReadAllocation(Bitstream stream, Header header, Crc16 crc);

        internal abstract void ReadScaleFactor(Bitstream stream, Header header);

        internal abstract bool ReadSampleData(Bitstream stream);

        internal abstract bool PutNextSample(int channels, SynthesisFilter filter1, SynthesisFilter filter2);
    }
}