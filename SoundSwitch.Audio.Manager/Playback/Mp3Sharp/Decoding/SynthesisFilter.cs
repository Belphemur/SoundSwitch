// /***************************************************************************
//  * SynthesisFilter.cs
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
    /// A class for the synthesis filter bank.
    /// This class does a fast downsampling from 32, 44.1 or 48 kHz to 8 kHz, if ULAW is defined.
    /// Frequencies above 4 kHz are removed by ignoring higher subbands.
    /// </summary>
    public class SynthesisFilter {
        private const double MY_PI = 3.14159265358979323846;

        // Note: These values are not in the same order
        // as in Annex 3-B.3 of the ISO/IEC DIS 11172-3 
        private static readonly float Cos164 = (float)(1.0 / (2.0 * Math.Cos(MY_PI / 64.0)));
        private static readonly float Cos364 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 3.0 / 64.0)));
        private static readonly float Cos564 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 5.0 / 64.0)));
        private static readonly float Cos764 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 7.0 / 64.0)));
        private static readonly float Cos964 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 9.0 / 64.0)));
        private static readonly float Cos1164 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 11.0 / 64.0)));
        private static readonly float Cos1364 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 13.0 / 64.0)));
        private static readonly float Cos1564 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 15.0 / 64.0)));
        private static readonly float Cos1764 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 17.0 / 64.0)));
        private static readonly float Cos1964 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 19.0 / 64.0)));
        private static readonly float Cos2164 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 21.0 / 64.0)));
        private static readonly float Cos2364 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 23.0 / 64.0)));
        private static readonly float Cos2564 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 25.0 / 64.0)));
        private static readonly float Cos2764 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 27.0 / 64.0)));
        private static readonly float Cos2964 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 29.0 / 64.0)));
        private static readonly float Cos3164 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 31.0 / 64.0)));
        private static readonly float Cos132 = (float)(1.0 / (2.0 * Math.Cos(MY_PI / 32.0)));
        private static readonly float Cos332 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 3.0 / 32.0)));
        private static readonly float Cos532 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 5.0 / 32.0)));
        private static readonly float Cos732 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 7.0 / 32.0)));
        private static readonly float Cos932 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 9.0 / 32.0)));
        private static readonly float Cos1132 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 11.0 / 32.0)));
        private static readonly float Cos1332 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 13.0 / 32.0)));
        private static readonly float Cos1532 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 15.0 / 32.0)));
        private static readonly float Cos116 = (float)(1.0 / (2.0 * Math.Cos(MY_PI / 16.0)));
        private static readonly float Cos316 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 3.0 / 16.0)));
        private static readonly float Cos516 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 5.0 / 16.0)));
        private static readonly float Cos716 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 7.0 / 16.0)));
        private static readonly float Cos18 = (float)(1.0 / (2.0 * Math.Cos(MY_PI / 8.0)));
        private static readonly float Cos38 = (float)(1.0 / (2.0 * Math.Cos(MY_PI * 3.0 / 8.0)));
        private static readonly float Cos14 = (float)(1.0 / (2.0 * Math.Cos(MY_PI / 4.0)));

        private static float[] _d;

        /// d[] split into subarrays of length 16. This provides for
        /// more faster access by allowing a block of 16 to be addressed
        /// with constant offset.
        private static float[][] _d16;

        // The original data for d[]. This data (was) loaded from a file
        // to reduce the overall package size and to improve performance. 
        private static readonly float[] DData = {
            0.000000000f, -0.000442505f, 0.003250122f, -0.007003784f,
            0.031082153f, -0.078628540f, 0.100311279f, -0.572036743f,
            1.144989014f, 0.572036743f, 0.100311279f, 0.078628540f,
            0.031082153f, 0.007003784f, 0.003250122f, 0.000442505f,
            -0.000015259f, -0.000473022f, 0.003326416f, -0.007919312f,
            0.030517578f, -0.084182739f, 0.090927124f, -0.600219727f,
            1.144287109f, 0.543823242f, 0.108856201f, 0.073059082f,
            0.031478882f, 0.006118774f, 0.003173828f, 0.000396729f,
            -0.000015259f, -0.000534058f, 0.003387451f, -0.008865356f,
            0.029785156f, -0.089706421f, 0.080688477f, -0.628295898f,
            1.142211914f, 0.515609741f, 0.116577148f, 0.067520142f,
            0.031738281f, 0.005294800f, 0.003082275f, 0.000366211f,
            -0.000015259f, -0.000579834f, 0.003433228f, -0.009841919f,
            0.028884888f, -0.095169067f, 0.069595337f, -0.656219482f,
            1.138763428f, 0.487472534f, 0.123474121f, 0.061996460f,
            0.031845093f, 0.004486084f, 0.002990723f, 0.000320435f,
            -0.000015259f, -0.000625610f, 0.003463745f, -0.010848999f,
            0.027801514f, -0.100540161f, 0.057617188f, -0.683914185f,
            1.133926392f, 0.459472656f, 0.129577637f, 0.056533813f,
            0.031814575f, 0.003723145f, 0.002899170f, 0.000289917f,
            -0.000015259f, -0.000686646f, 0.003479004f, -0.011886597f,
            0.026535034f, -0.105819702f, 0.044784546f, -0.711318970f,
            1.127746582f, 0.431655884f, 0.134887695f, 0.051132202f,
            0.031661987f, 0.003005981f, 0.002792358f, 0.000259399f,
            -0.000015259f, -0.000747681f, 0.003479004f, -0.012939453f,
            0.025085449f, -0.110946655f, 0.031082153f, -0.738372803f,
            1.120223999f, 0.404083252f, 0.139450073f, 0.045837402f,
            0.031387329f, 0.002334595f, 0.002685547f, 0.000244141f,
            -0.000030518f, -0.000808716f, 0.003463745f, -0.014022827f,
            0.023422241f, -0.115921021f, 0.016510010f, -0.765029907f,
            1.111373901f, 0.376800537f, 0.143264771f, 0.040634155f,
            0.031005859f, 0.001693726f, 0.002578735f, 0.000213623f,
            -0.000030518f, -0.000885010f, 0.003417969f, -0.015121460f,
            0.021575928f, -0.120697021f, 0.001068115f, -0.791213989f,
            1.101211548f, 0.349868774f, 0.146362305f, 0.035552979f,
            0.030532837f, 0.001098633f, 0.002456665f, 0.000198364f,
            -0.000030518f, -0.000961304f, 0.003372192f, -0.016235352f,
            0.019531250f, -0.125259399f, -0.015228271f, -0.816864014f,
            1.089782715f, 0.323318481f, 0.148773193f, 0.030609131f,
            0.029937744f, 0.000549316f, 0.002349854f, 0.000167847f,
            -0.000030518f, -0.001037598f, 0.003280640f, -0.017349243f,
            0.017257690f, -0.129562378f, -0.032379150f, -0.841949463f,
            1.077117920f, 0.297210693f, 0.150497437f, 0.025817871f,
            0.029281616f, 0.000030518f, 0.002243042f, 0.000152588f,
            -0.000045776f, -0.001113892f, 0.003173828f, -0.018463135f,
            0.014801025f, -0.133590698f, -0.050354004f, -0.866363525f,
            1.063217163f, 0.271591187f, 0.151596069f, 0.021179199f,
            0.028533936f, -0.000442505f, 0.002120972f, 0.000137329f,
            -0.000045776f, -0.001205444f, 0.003051758f, -0.019577026f,
            0.012115479f, -0.137298584f, -0.069168091f, -0.890090942f,
            1.048156738f, 0.246505737f, 0.152069092f, 0.016708374f,
            0.027725220f, -0.000869751f, 0.002014160f, 0.000122070f,
            -0.000061035f, -0.001296997f, 0.002883911f, -0.020690918f,
            0.009231567f, -0.140670776f, -0.088775635f, -0.913055420f,
            1.031936646f, 0.221984863f, 0.151962280f, 0.012420654f,
            0.026840210f, -0.001266479f, 0.001907349f, 0.000106812f,
            -0.000061035f, -0.001388550f, 0.002700806f, -0.021789551f,
            0.006134033f, -0.143676758f, -0.109161377f, -0.935195923f,
            1.014617920f, 0.198059082f, 0.151306152f, 0.008316040f,
            0.025909424f, -0.001617432f, 0.001785278f, 0.000106812f,
            -0.000076294f, -0.001480103f, 0.002487183f, -0.022857666f,
            0.002822876f, -0.146255493f, -0.130310059f, -0.956481934f,
            0.996246338f, 0.174789429f, 0.150115967f, 0.004394531f,
            0.024932861f, -0.001937866f, 0.001693726f, 0.000091553f,
            -0.000076294f, -0.001586914f, 0.002227783f, -0.023910522f,
            -0.000686646f, -0.148422241f, -0.152206421f, -0.976852417f,
            0.976852417f, 0.152206421f, 0.148422241f, 0.000686646f,
            0.023910522f, -0.002227783f, 0.001586914f, 0.000076294f,
            -0.000091553f, -0.001693726f, 0.001937866f, -0.024932861f,
            -0.004394531f, -0.150115967f, -0.174789429f, -0.996246338f,
            0.956481934f, 0.130310059f, 0.146255493f, -0.002822876f,
            0.022857666f, -0.002487183f, 0.001480103f, 0.000076294f,
            -0.000106812f, -0.001785278f, 0.001617432f, -0.025909424f,
            -0.008316040f, -0.151306152f, -0.198059082f, -1.014617920f,
            0.935195923f, 0.109161377f, 0.143676758f, -0.006134033f,
            0.021789551f, -0.002700806f, 0.001388550f, 0.000061035f,
            -0.000106812f, -0.001907349f, 0.001266479f, -0.026840210f,
            -0.012420654f, -0.151962280f, -0.221984863f, -1.031936646f,
            0.913055420f, 0.088775635f, 0.140670776f, -0.009231567f,
            0.020690918f, -0.002883911f, 0.001296997f, 0.000061035f,
            -0.000122070f, -0.002014160f, 0.000869751f, -0.027725220f,
            -0.016708374f, -0.152069092f, -0.246505737f, -1.048156738f,
            0.890090942f, 0.069168091f, 0.137298584f, -0.012115479f,
            0.019577026f, -0.003051758f, 0.001205444f, 0.000045776f,
            -0.000137329f, -0.002120972f, 0.000442505f, -0.028533936f,
            -0.021179199f, -0.151596069f, -0.271591187f, -1.063217163f,
            0.866363525f, 0.050354004f, 0.133590698f, -0.014801025f,
            0.018463135f, -0.003173828f, 0.001113892f, 0.000045776f,
            -0.000152588f, -0.002243042f, -0.000030518f, -0.029281616f,
            -0.025817871f, -0.150497437f, -0.297210693f, -1.077117920f,
            0.841949463f, 0.032379150f, 0.129562378f, -0.017257690f,
            0.017349243f, -0.003280640f, 0.001037598f, 0.000030518f,
            -0.000167847f, -0.002349854f, -0.000549316f, -0.029937744f,
            -0.030609131f, -0.148773193f, -0.323318481f, -1.089782715f,
            0.816864014f, 0.015228271f, 0.125259399f, -0.019531250f,
            0.016235352f, -0.003372192f, 0.000961304f, 0.000030518f,
            -0.000198364f, -0.002456665f, -0.001098633f, -0.030532837f,
            -0.035552979f, -0.146362305f, -0.349868774f, -1.101211548f,
            0.791213989f, -0.001068115f, 0.120697021f, -0.021575928f,
            0.015121460f, -0.003417969f, 0.000885010f, 0.000030518f,
            -0.000213623f, -0.002578735f, -0.001693726f, -0.031005859f,
            -0.040634155f, -0.143264771f, -0.376800537f, -1.111373901f,
            0.765029907f, -0.016510010f, 0.115921021f, -0.023422241f,
            0.014022827f, -0.003463745f, 0.000808716f, 0.000030518f,
            -0.000244141f, -0.002685547f, -0.002334595f, -0.031387329f,
            -0.045837402f, -0.139450073f, -0.404083252f, -1.120223999f,
            0.738372803f, -0.031082153f, 0.110946655f, -0.025085449f,
            0.012939453f, -0.003479004f, 0.000747681f, 0.000015259f,
            -0.000259399f, -0.002792358f, -0.003005981f, -0.031661987f,
            -0.051132202f, -0.134887695f, -0.431655884f, -1.127746582f,
            0.711318970f, -0.044784546f, 0.105819702f, -0.026535034f,
            0.011886597f, -0.003479004f, 0.000686646f, 0.000015259f,
            -0.000289917f, -0.002899170f, -0.003723145f, -0.031814575f,
            -0.056533813f, -0.129577637f, -0.459472656f, -1.133926392f,
            0.683914185f, -0.057617188f, 0.100540161f, -0.027801514f,
            0.010848999f, -0.003463745f, 0.000625610f, 0.000015259f,
            -0.000320435f, -0.002990723f, -0.004486084f, -0.031845093f,
            -0.061996460f, -0.123474121f, -0.487472534f, -1.138763428f,
            0.656219482f, -0.069595337f, 0.095169067f, -0.028884888f,
            0.009841919f, -0.003433228f, 0.000579834f, 0.000015259f,
            -0.000366211f, -0.003082275f, -0.005294800f, -0.031738281f,
            -0.067520142f, -0.116577148f, -0.515609741f, -1.142211914f,
            0.628295898f, -0.080688477f, 0.089706421f, -0.029785156f,
            0.008865356f, -0.003387451f, 0.000534058f, 0.000015259f,
            -0.000396729f, -0.003173828f, -0.006118774f, -0.031478882f,
            -0.073059082f, -0.108856201f, -0.543823242f, -1.144287109f,
            0.600219727f, -0.090927124f, 0.084182739f, -0.030517578f,
            0.007919312f, -0.003326416f, 0.000473022f, 0.000015259f
        };

        private readonly int _Channel;
        private readonly float[] _Samples; // 32 new subband samples
        private readonly float _Scalefactor;
        private readonly float[] _V1;
        private readonly float[] _V2;

        /// <summary>
        /// Compute PCM Samples.
        /// </summary>
        private float[] _TmpOut;

        private float[] _ActualV; // v1 or v2
        private int _ActualWritePos; // 0-15
        private float[] _Eq;

        /// <summary>
        /// Quality value for controlling CPU usage/quality tradeoff.
        /// </summary>
        /// <summary>
        /// Contructor.
        /// The scalefactor scales the calculated float pcm samples to short values
        /// (raw pcm samples are in [-1.0, 1.0], if no violations occur).
        /// </summary>
        internal SynthesisFilter(int channelnumber, float factor, float[] eq0) {
            InitBlock();
            if (_d == null) {
                _d = DData; // load_d();
                _d16 = SplitArray(_d, 16);
            }

            _V1 = new float[512];
            _V2 = new float[512];
            _Samples = new float[32];
            _Channel = channelnumber;
            _Scalefactor = factor;
            Eq = _Eq;

            Reset();
        }

        internal float[] Eq {
            set {
                _Eq = value;

                if (_Eq == null) {
                    _Eq = new float[32];
                    for (int i = 0; i < 32; i++)
                        _Eq[i] = 1.0f;
                }
                if (_Eq.Length < 32) {
                    throw new ArgumentException("eq0");
                }
            }
        }

        private void InitBlock() {
            _TmpOut = new float[32];
        }

        /// <summary>
        /// Reset the synthesis filter.
        /// </summary>
        internal void Reset() {
            // initialize v1[] and v2[]:
            for (int p = 0; p < 512; p++)
                _V1[p] = _V2[p] = 0.0f;

            // initialize samples[]:
            for (int p2 = 0; p2 < 32; p2++)
                _Samples[p2] = 0.0f;

            _ActualV = _V1;
            _ActualWritePos = 15;
        }

        internal void AddSample(float sample, int subbandnumber) {
            _Samples[subbandnumber] = _Eq[subbandnumber] * sample;
        }

        internal void AddSamples(float[] s) {
            for (int i = 31; i >= 0; i--) {
                _Samples[i] = s[i] * _Eq[i];
            }
        }

        /// <summary>
        /// Compute new values via a fast cosine transform.
        /// </summary>
        private void ComputeNewValues() {
            float newV0, newV1, newV2, newV3, newV4, newV5, newV6, newV7, newV8, newV9;
            float newV10, newV11, newV12, newV13, newV14, newV15, newV16, newV17, newV18, newV19;
            float newV20, newV21, newV22, newV23, newV24, newV25, newV26, newV27, newV28, newV29;
            float newV30, newV31;

            float[] s = _Samples;

            float s0 = s[0];
            float s1 = s[1];
            float s2 = s[2];
            float s3 = s[3];
            float s4 = s[4];
            float s5 = s[5];
            float s6 = s[6];
            float s7 = s[7];
            float s8 = s[8];
            float s9 = s[9];
            float s10 = s[10];
            float s11 = s[11];
            float s12 = s[12];
            float s13 = s[13];
            float s14 = s[14];
            float s15 = s[15];
            float s16 = s[16];
            float s17 = s[17];
            float s18 = s[18];
            float s19 = s[19];
            float s20 = s[20];
            float s21 = s[21];
            float s22 = s[22];
            float s23 = s[23];
            float s24 = s[24];
            float s25 = s[25];
            float s26 = s[26];
            float s27 = s[27];
            float s28 = s[28];
            float s29 = s[29];
            float s30 = s[30];
            float s31 = s[31];

            float p0 = s0 + s31;
            float p1 = s1 + s30;
            float p2 = s2 + s29;
            float p3 = s3 + s28;
            float p4 = s4 + s27;
            float p5 = s5 + s26;
            float p6 = s6 + s25;
            float p7 = s7 + s24;
            float p8 = s8 + s23;
            float p9 = s9 + s22;
            float p10 = s10 + s21;
            float p11 = s11 + s20;
            float p12 = s12 + s19;
            float p13 = s13 + s18;
            float p14 = s14 + s17;
            float p15 = s15 + s16;

            float pp0 = p0 + p15;
            float pp1 = p1 + p14;
            float pp2 = p2 + p13;
            float pp3 = p3 + p12;
            float pp4 = p4 + p11;
            float pp5 = p5 + p10;
            float pp6 = p6 + p9;
            float pp7 = p7 + p8;
            float pp8 = (p0 - p15) * Cos132;
            float pp9 = (p1 - p14) * Cos332;
            float pp10 = (p2 - p13) * Cos532;
            float pp11 = (p3 - p12) * Cos732;
            float pp12 = (p4 - p11) * Cos932;
            float pp13 = (p5 - p10) * Cos1132;
            float pp14 = (p6 - p9) * Cos1332;
            float pp15 = (p7 - p8) * Cos1532;

            p0 = pp0 + pp7;
            p1 = pp1 + pp6;
            p2 = pp2 + pp5;
            p3 = pp3 + pp4;
            p4 = (pp0 - pp7) * Cos116;
            p5 = (pp1 - pp6) * Cos316;
            p6 = (pp2 - pp5) * Cos516;
            p7 = (pp3 - pp4) * Cos716;
            p8 = pp8 + pp15;
            p9 = pp9 + pp14;
            p10 = pp10 + pp13;
            p11 = pp11 + pp12;
            p12 = (pp8 - pp15) * Cos116;
            p13 = (pp9 - pp14) * Cos316;
            p14 = (pp10 - pp13) * Cos516;
            p15 = (pp11 - pp12) * Cos716;

            pp0 = p0 + p3;
            pp1 = p1 + p2;
            pp2 = (p0 - p3) * Cos18;
            pp3 = (p1 - p2) * Cos38;
            pp4 = p4 + p7;
            pp5 = p5 + p6;
            pp6 = (p4 - p7) * Cos18;
            pp7 = (p5 - p6) * Cos38;
            pp8 = p8 + p11;
            pp9 = p9 + p10;
            pp10 = (p8 - p11) * Cos18;
            pp11 = (p9 - p10) * Cos38;
            pp12 = p12 + p15;
            pp13 = p13 + p14;
            pp14 = (p12 - p15) * Cos18;
            pp15 = (p13 - p14) * Cos38;

            p0 = pp0 + pp1;
            p1 = (pp0 - pp1) * Cos14;
            p2 = pp2 + pp3;
            p3 = (pp2 - pp3) * Cos14;
            p4 = pp4 + pp5;
            p5 = (pp4 - pp5) * Cos14;
            p6 = pp6 + pp7;
            p7 = (pp6 - pp7) * Cos14;
            p8 = pp8 + pp9;
            p9 = (pp8 - pp9) * Cos14;
            p10 = pp10 + pp11;
            p11 = (pp10 - pp11) * Cos14;
            p12 = pp12 + pp13;
            p13 = (pp12 - pp13) * Cos14;
            p14 = pp14 + pp15;
            p15 = (pp14 - pp15) * Cos14;

            // this is pretty insane coding
            float tmp1;
            newV19 = -(newV4 = (newV12 = p7) + p5) - p6;
            newV27 = -p6 - p7 - p4;
            newV6 = (newV10 = (newV14 = p15) + p11) + p13;
            newV17 = -(newV2 = p15 + p13 + p9) - p14;
            newV21 = (tmp1 = -p14 - p15 - p10 - p11) - p13;
            newV29 = -p14 - p15 - p12 - p8;
            newV25 = tmp1 - p12;
            newV31 = -p0;
            newV0 = p1;
            newV23 = -(newV8 = p3) - p2;

            p0 = (s0 - s31) * Cos164;
            p1 = (s1 - s30) * Cos364;
            p2 = (s2 - s29) * Cos564;
            p3 = (s3 - s28) * Cos764;
            p4 = (s4 - s27) * Cos964;
            p5 = (s5 - s26) * Cos1164;
            p6 = (s6 - s25) * Cos1364;
            p7 = (s7 - s24) * Cos1564;
            p8 = (s8 - s23) * Cos1764;
            p9 = (s9 - s22) * Cos1964;
            p10 = (s10 - s21) * Cos2164;
            p11 = (s11 - s20) * Cos2364;
            p12 = (s12 - s19) * Cos2564;
            p13 = (s13 - s18) * Cos2764;
            p14 = (s14 - s17) * Cos2964;
            p15 = (s15 - s16) * Cos3164;

            pp0 = p0 + p15;
            pp1 = p1 + p14;
            pp2 = p2 + p13;
            pp3 = p3 + p12;
            pp4 = p4 + p11;
            pp5 = p5 + p10;
            pp6 = p6 + p9;
            pp7 = p7 + p8;
            pp8 = (p0 - p15) * Cos132;
            pp9 = (p1 - p14) * Cos332;
            pp10 = (p2 - p13) * Cos532;
            pp11 = (p3 - p12) * Cos732;
            pp12 = (p4 - p11) * Cos932;
            pp13 = (p5 - p10) * Cos1132;
            pp14 = (p6 - p9) * Cos1332;
            pp15 = (p7 - p8) * Cos1532;

            p0 = pp0 + pp7;
            p1 = pp1 + pp6;
            p2 = pp2 + pp5;
            p3 = pp3 + pp4;
            p4 = (pp0 - pp7) * Cos116;
            p5 = (pp1 - pp6) * Cos316;
            p6 = (pp2 - pp5) * Cos516;
            p7 = (pp3 - pp4) * Cos716;
            p8 = pp8 + pp15;
            p9 = pp9 + pp14;
            p10 = pp10 + pp13;
            p11 = pp11 + pp12;
            p12 = (pp8 - pp15) * Cos116;
            p13 = (pp9 - pp14) * Cos316;
            p14 = (pp10 - pp13) * Cos516;
            p15 = (pp11 - pp12) * Cos716;

            pp0 = p0 + p3;
            pp1 = p1 + p2;
            pp2 = (p0 - p3) * Cos18;
            pp3 = (p1 - p2) * Cos38;
            pp4 = p4 + p7;
            pp5 = p5 + p6;
            pp6 = (p4 - p7) * Cos18;
            pp7 = (p5 - p6) * Cos38;
            pp8 = p8 + p11;
            pp9 = p9 + p10;
            pp10 = (p8 - p11) * Cos18;
            pp11 = (p9 - p10) * Cos38;
            pp12 = p12 + p15;
            pp13 = p13 + p14;
            pp14 = (p12 - p15) * Cos18;
            pp15 = (p13 - p14) * Cos38;

            p0 = pp0 + pp1;
            p1 = (pp0 - pp1) * Cos14;
            p2 = pp2 + pp3;
            p3 = (pp2 - pp3) * Cos14;
            p4 = pp4 + pp5;
            p5 = (pp4 - pp5) * Cos14;
            p6 = pp6 + pp7;
            p7 = (pp6 - pp7) * Cos14;
            p8 = pp8 + pp9;
            p9 = (pp8 - pp9) * Cos14;
            p10 = pp10 + pp11;
            p11 = (pp10 - pp11) * Cos14;
            p12 = pp12 + pp13;
            p13 = (pp12 - pp13) * Cos14;
            p14 = pp14 + pp15;
            p15 = (pp14 - pp15) * Cos14;

            // manually doing something that a compiler should handle sucks
            // coding like this is hard to read
            float tmp2;
            newV5 = (newV11 = (newV13 = (newV15 = p15) + p7) + p11) + p5 + p13;
            newV7 = (newV9 = p15 + p11 + p3) + p13;
            newV16 = -(newV1 = (tmp1 = p13 + p15 + p9) + p1) - p14;
            newV18 = -(newV3 = tmp1 + p5 + p7) - p6 - p14;

            newV22 = (tmp1 = -p10 - p11 - p14 - p15) - p13 - p2 - p3;
            newV20 = tmp1 - p13 - p5 - p6 - p7;
            newV24 = tmp1 - p12 - p2 - p3;
            newV26 = tmp1 - p12 - (tmp2 = p4 + p6 + p7);
            newV30 = (tmp1 = -p8 - p12 - p14 - p15) - p0;
            newV28 = tmp1 - tmp2;

            // insert V[0-15] (== new_v[0-15]) into actual v:
            // float[] x2 = actual_v + actual_write_pos;
            float[] dest = _ActualV;

            int pos = _ActualWritePos;

            dest[0 + pos] = newV0;
            dest[16 + pos] = newV1;
            dest[32 + pos] = newV2;
            dest[48 + pos] = newV3;
            dest[64 + pos] = newV4;
            dest[80 + pos] = newV5;
            dest[96 + pos] = newV6;
            dest[112 + pos] = newV7;
            dest[128 + pos] = newV8;
            dest[144 + pos] = newV9;
            dest[160 + pos] = newV10;
            dest[176 + pos] = newV11;
            dest[192 + pos] = newV12;
            dest[208 + pos] = newV13;
            dest[224 + pos] = newV14;
            dest[240 + pos] = newV15;

            // V[16] is always 0.0:
            dest[256 + pos] = 0.0f;

            // insert V[17-31] (== -new_v[15-1]) into actual v:
            dest[272 + pos] = -newV15;
            dest[288 + pos] = -newV14;
            dest[304 + pos] = -newV13;
            dest[320 + pos] = -newV12;
            dest[336 + pos] = -newV11;
            dest[352 + pos] = -newV10;
            dest[368 + pos] = -newV9;
            dest[384 + pos] = -newV8;
            dest[400 + pos] = -newV7;
            dest[416 + pos] = -newV6;
            dest[432 + pos] = -newV5;
            dest[448 + pos] = -newV4;
            dest[464 + pos] = -newV3;
            dest[480 + pos] = -newV2;
            dest[496 + pos] = -newV1;

            // insert V[32] (== -new_v[0]) into other v:
            dest = _ActualV == _V1 ? _V2 : _V1;

            dest[0 + pos] = -newV0;
            // insert V[33-48] (== new_v[16-31]) into other v:
            dest[16 + pos] = newV16;
            dest[32 + pos] = newV17;
            dest[48 + pos] = newV18;
            dest[64 + pos] = newV19;
            dest[80 + pos] = newV20;
            dest[96 + pos] = newV21;
            dest[112 + pos] = newV22;
            dest[128 + pos] = newV23;
            dest[144 + pos] = newV24;
            dest[160 + pos] = newV25;
            dest[176 + pos] = newV26;
            dest[192 + pos] = newV27;
            dest[208 + pos] = newV28;
            dest[224 + pos] = newV29;
            dest[240 + pos] = newV30;
            dest[256 + pos] = newV31;

            // insert V[49-63] (== new_v[30-16]) into other v:
            dest[272 + pos] = newV30;
            dest[288 + pos] = newV29;
            dest[304 + pos] = newV28;
            dest[320 + pos] = newV27;
            dest[336 + pos] = newV26;
            dest[352 + pos] = newV25;
            dest[368 + pos] = newV24;
            dest[384 + pos] = newV23;
            dest[400 + pos] = newV22;
            dest[416 + pos] = newV21;
            dest[432 + pos] = newV20;
            dest[448 + pos] = newV19;
            dest[464 + pos] = newV18;
            dest[480 + pos] = newV17;
            dest[496 + pos] = newV16;
        }

        private void compute_pc_samples0(ABuffer buffer) {
            float[] vp = _ActualV;
            //int inc = v_inc;
            float[] tmpOut = _TmpOut;
            int dvp = 0;

            // fat chance of having this loop unroll
            for (int i = 0; i < 32; i++) {
                float pcSample;
                float[] dp = _d16[i];
                pcSample =
                    (vp[0 + dvp] * dp[0] + vp[15 + dvp] * dp[1] + vp[14 + dvp] * dp[2] + vp[13 + dvp] * dp[3] +
                     vp[12 + dvp] * dp[4] + vp[11 + dvp] * dp[5] + vp[10 + dvp] * dp[6] + vp[9 + dvp] * dp[7] +
                     vp[8 + dvp] * dp[8] + vp[7 + dvp] * dp[9] + vp[6 + dvp] * dp[10] + vp[5 + dvp] * dp[11] +
                     vp[4 + dvp] * dp[12] + vp[3 + dvp] * dp[13] + vp[2 + dvp] * dp[14] + vp[1 + dvp] * dp[15]) *
                    _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
            // for
        }

        private void compute_pc_samples1(ABuffer buffer) {
            float[] vp = _ActualV;
            //int inc = v_inc;
            float[] tmpOut = _TmpOut;
            int dvp = 0;

            // fat chance of having this loop unroll
            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample;

                pcSample =
                    (vp[1 + dvp] * dp[0] + vp[0 + dvp] * dp[1] + vp[15 + dvp] * dp[2] + vp[14 + dvp] * dp[3] +
                     vp[13 + dvp] * dp[4] + vp[12 + dvp] * dp[5] + vp[11 + dvp] * dp[6] + vp[10 + dvp] * dp[7] +
                     vp[9 + dvp] * dp[8] + vp[8 + dvp] * dp[9] + vp[7 + dvp] * dp[10] + vp[6 + dvp] * dp[11] +
                     vp[5 + dvp] * dp[12] + vp[4 + dvp] * dp[13] + vp[3 + dvp] * dp[14] + vp[2 + dvp] * dp[15]) *
                    _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
            // for
        }

        private void compute_pc_samples2(ABuffer buffer) {
            float[] vp = _ActualV;

            //int inc = v_inc;
            float[] tmpOut = _TmpOut;
            int dvp = 0;

            // fat chance of having this loop unroll
            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample;

                pcSample =
                    (vp[2 + dvp] * dp[0] + vp[1 + dvp] * dp[1] + vp[0 + dvp] * dp[2] + vp[15 + dvp] * dp[3] +
                     vp[14 + dvp] * dp[4] + vp[13 + dvp] * dp[5] + vp[12 + dvp] * dp[6] + vp[11 + dvp] * dp[7] +
                     vp[10 + dvp] * dp[8] + vp[9 + dvp] * dp[9] + vp[8 + dvp] * dp[10] + vp[7 + dvp] * dp[11] +
                     vp[6 + dvp] * dp[12] + vp[5 + dvp] * dp[13] + vp[4 + dvp] * dp[14] + vp[3 + dvp] * dp[15]) *
                    _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
            // for
        }

        private void compute_pc_samples3(ABuffer buffer) {
            float[] vp = _ActualV;

            float[] tmpOut = _TmpOut;
            int dvp = 0;

            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample = (vp[3 + dvp] * dp[0] + vp[2 + dvp] * dp[1] + vp[1 + dvp] * dp[2] + vp[0 + dvp] * dp[3] +
                                   vp[15 + dvp] * dp[4] + vp[14 + dvp] * dp[5] + vp[13 + dvp] * dp[6] + vp[12 + dvp] * dp[7] +
                                   vp[11 + dvp] * dp[8] + vp[10 + dvp] * dp[9] + vp[9 + dvp] * dp[10] + vp[8 + dvp] * dp[11] +
                                   vp[7 + dvp] * dp[12] + vp[6 + dvp] * dp[13] + vp[5 + dvp] * dp[14] + vp[4 + dvp] * dp[15]) *
                                  _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
        }

        private void compute_pc_samples4(ABuffer buffer) {
            float[] vp = _ActualV;

            float[] tmpOut = _TmpOut;
            int dvp = 0;

            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample = (vp[4 + dvp] * dp[0] + vp[3 + dvp] * dp[1] + vp[2 + dvp] * dp[2] + vp[1 + dvp] * dp[3] +
                                   vp[0 + dvp] * dp[4] + vp[15 + dvp] * dp[5] + vp[14 + dvp] * dp[6] + vp[13 + dvp] * dp[7] +
                                   vp[12 + dvp] * dp[8] + vp[11 + dvp] * dp[9] + vp[10 + dvp] * dp[10] + vp[9 + dvp] * dp[11] +
                                   vp[8 + dvp] * dp[12] + vp[7 + dvp] * dp[13] + vp[6 + dvp] * dp[14] + vp[5 + dvp] * dp[15]) *
                                  _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
            // for
        }

        private void compute_pc_samples5(ABuffer buffer) {
            float[] vp = _ActualV;

            float[] tmpOut = _TmpOut;
            int dvp = 0;

            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample = (vp[5 + dvp] * dp[0] + vp[4 + dvp] * dp[1] + vp[3 + dvp] * dp[2] + vp[2 + dvp] * dp[3] +
                                   vp[1 + dvp] * dp[4] + vp[0 + dvp] * dp[5] + vp[15 + dvp] * dp[6] + vp[14 + dvp] * dp[7] +
                                   vp[13 + dvp] * dp[8] + vp[12 + dvp] * dp[9] + vp[11 + dvp] * dp[10] + vp[10 + dvp] * dp[11] +
                                   vp[9 + dvp] * dp[12] + vp[8 + dvp] * dp[13] + vp[7 + dvp] * dp[14] + vp[6 + dvp] * dp[15]) *
                                  _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
            // for
        }

        private void compute_pc_samples6(ABuffer buffer) {
            float[] vp = _ActualV;
            float[] tmpOut = _TmpOut;
            int dvp = 0;

            // fat chance of having this loop unroll
            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample = (vp[6 + dvp] * dp[0] + vp[5 + dvp] * dp[1] + vp[4 + dvp] * dp[2] + vp[3 + dvp] * dp[3] +
                                   vp[2 + dvp] * dp[4] + vp[1 + dvp] * dp[5] + vp[0 + dvp] * dp[6] + vp[15 + dvp] * dp[7] +
                                   vp[14 + dvp] * dp[8] + vp[13 + dvp] * dp[9] + vp[12 + dvp] * dp[10] + vp[11 + dvp] * dp[11] +
                                   vp[10 + dvp] * dp[12] + vp[9 + dvp] * dp[13] + vp[8 + dvp] * dp[14] + vp[7 + dvp] * dp[15]) *
                                  _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
            // for
        }

        private void compute_pc_samples7(ABuffer buffer) {
            float[] vp = _ActualV;

            //int inc = v_inc;
            float[] tmpOut = _TmpOut;
            int dvp = 0;

            // fat chance of having this loop unroll
            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample;

                pcSample =
                    (vp[7 + dvp] * dp[0] + vp[6 + dvp] * dp[1] + vp[5 + dvp] * dp[2] + vp[4 + dvp] * dp[3] +
                     vp[3 + dvp] * dp[4] + vp[2 + dvp] * dp[5] + vp[1 + dvp] * dp[6] + vp[0 + dvp] * dp[7] +
                     vp[15 + dvp] * dp[8] + vp[14 + dvp] * dp[9] + vp[13 + dvp] * dp[10] + vp[12 + dvp] * dp[11] +
                     vp[11 + dvp] * dp[12] + vp[10 + dvp] * dp[13] + vp[9 + dvp] * dp[14] + vp[8 + dvp] * dp[15]) *
                    _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
            // for
        }

        private void compute_pc_samples8(ABuffer buffer) {
            float[] vp = _ActualV;

            //int inc = v_inc;
            float[] tmpOut = _TmpOut;
            int dvp = 0;

            // fat chance of having this loop unroll
            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample;

                pcSample =
                    (vp[8 + dvp] * dp[0] + vp[7 + dvp] * dp[1] + vp[6 + dvp] * dp[2] + vp[5 + dvp] * dp[3] +
                     vp[4 + dvp] * dp[4] + vp[3 + dvp] * dp[5] + vp[2 + dvp] * dp[6] + vp[1 + dvp] * dp[7] +
                     vp[0 + dvp] * dp[8] + vp[15 + dvp] * dp[9] + vp[14 + dvp] * dp[10] + vp[13 + dvp] * dp[11] +
                     vp[12 + dvp] * dp[12] + vp[11 + dvp] * dp[13] + vp[10 + dvp] * dp[14] + vp[9 + dvp] * dp[15]) *
                    _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
            // for
        }

        private void compute_pc_samples9(ABuffer buffer) {
            float[] vp = _ActualV;

            //int inc = v_inc;
            float[] tmpOut = _TmpOut;
            int dvp = 0;

            // fat chance of having this loop unroll
            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample;

                pcSample =
                    (vp[9 + dvp] * dp[0] + vp[8 + dvp] * dp[1] + vp[7 + dvp] * dp[2] + vp[6 + dvp] * dp[3] +
                     vp[5 + dvp] * dp[4] + vp[4 + dvp] * dp[5] + vp[3 + dvp] * dp[6] + vp[2 + dvp] * dp[7] +
                     vp[1 + dvp] * dp[8] + vp[0 + dvp] * dp[9] + vp[15 + dvp] * dp[10] + vp[14 + dvp] * dp[11] +
                     vp[13 + dvp] * dp[12] + vp[12 + dvp] * dp[13] + vp[11 + dvp] * dp[14] + vp[10 + dvp] * dp[15]) *
                    _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
            // for
        }

        private void compute_pc_samples10(ABuffer buffer) {
            float[] vp = _ActualV;
            //int inc = v_inc;
            float[] tmpOut = _TmpOut;
            int dvp = 0;

            // fat chance of having this loop unroll
            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample;

                pcSample =
                    (vp[10 + dvp] * dp[0] + vp[9 + dvp] * dp[1] + vp[8 + dvp] * dp[2] + vp[7 + dvp] * dp[3] +
                     vp[6 + dvp] * dp[4] + vp[5 + dvp] * dp[5] + vp[4 + dvp] * dp[6] + vp[3 + dvp] * dp[7] +
                     vp[2 + dvp] * dp[8] + vp[1 + dvp] * dp[9] + vp[0 + dvp] * dp[10] + vp[15 + dvp] * dp[11] +
                     vp[14 + dvp] * dp[12] + vp[13 + dvp] * dp[13] + vp[12 + dvp] * dp[14] + vp[11 + dvp] * dp[15]) *
                    _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
            // for
        }

        private void compute_pc_samples11(ABuffer buffer) {
            float[] vp = _ActualV;

            //int inc = v_inc;
            float[] tmpOut = _TmpOut;
            int dvp = 0;

            // fat chance of having this loop unroll
            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample;

                pcSample =
                    (vp[11 + dvp] * dp[0] + vp[10 + dvp] * dp[1] + vp[9 + dvp] * dp[2] + vp[8 + dvp] * dp[3] +
                     vp[7 + dvp] * dp[4] + vp[6 + dvp] * dp[5] + vp[5 + dvp] * dp[6] + vp[4 + dvp] * dp[7] +
                     vp[3 + dvp] * dp[8] + vp[2 + dvp] * dp[9] + vp[1 + dvp] * dp[10] + vp[0 + dvp] * dp[11] +
                     vp[15 + dvp] * dp[12] + vp[14 + dvp] * dp[13] + vp[13 + dvp] * dp[14] + vp[12 + dvp] * dp[15]) *
                    _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
            // for
        }

        private void compute_pc_samples12(ABuffer buffer) {
            float[] vp = _ActualV;
            //int inc = v_inc;
            float[] tmpOut = _TmpOut;
            int dvp = 0;

            // fat chance of having this loop unroll
            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample;

                pcSample =
                    (vp[12 + dvp] * dp[0] + vp[11 + dvp] * dp[1] + vp[10 + dvp] * dp[2] + vp[9 + dvp] * dp[3] +
                     vp[8 + dvp] * dp[4] + vp[7 + dvp] * dp[5] + vp[6 + dvp] * dp[6] + vp[5 + dvp] * dp[7] +
                     vp[4 + dvp] * dp[8] + vp[3 + dvp] * dp[9] + vp[2 + dvp] * dp[10] + vp[1 + dvp] * dp[11] +
                     vp[0 + dvp] * dp[12] + vp[15 + dvp] * dp[13] + vp[14 + dvp] * dp[14] + vp[13 + dvp] * dp[15]) *
                    _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
            // for
        }

        private void compute_pc_samples13(ABuffer buffer) {
            float[] vp = _ActualV;

            //int inc = v_inc;
            float[] tmpOut = _TmpOut;
            int dvp = 0;

            // fat chance of having this loop unroll
            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample;

                pcSample =
                    (vp[13 + dvp] * dp[0] + vp[12 + dvp] * dp[1] + vp[11 + dvp] * dp[2] + vp[10 + dvp] * dp[3] +
                     vp[9 + dvp] * dp[4] + vp[8 + dvp] * dp[5] + vp[7 + dvp] * dp[6] + vp[6 + dvp] * dp[7] +
                     vp[5 + dvp] * dp[8] + vp[4 + dvp] * dp[9] + vp[3 + dvp] * dp[10] + vp[2 + dvp] * dp[11] +
                     vp[1 + dvp] * dp[12] + vp[0 + dvp] * dp[13] + vp[15 + dvp] * dp[14] + vp[14 + dvp] * dp[15]) *
                    _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
            // for
        }

        private void compute_pc_samples14(ABuffer buffer) {
            float[] vp = _ActualV;

            //int inc = v_inc;
            float[] tmpOut = _TmpOut;
            int dvp = 0;

            // fat chance of having this loop unroll
            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample;

                pcSample =
                    (vp[14 + dvp] * dp[0] + vp[13 + dvp] * dp[1] + vp[12 + dvp] * dp[2] + vp[11 + dvp] * dp[3] +
                     vp[10 + dvp] * dp[4] + vp[9 + dvp] * dp[5] + vp[8 + dvp] * dp[6] + vp[7 + dvp] * dp[7] +
                     vp[6 + dvp] * dp[8] + vp[5 + dvp] * dp[9] + vp[4 + dvp] * dp[10] + vp[3 + dvp] * dp[11] +
                     vp[2 + dvp] * dp[12] + vp[1 + dvp] * dp[13] + vp[0 + dvp] * dp[14] + vp[15 + dvp] * dp[15]) *
                    _Scalefactor;

                tmpOut[i] = pcSample;

                dvp += 16;
            }
            // for
        }

        private void Compute_pc_samples15(ABuffer buffer) {
            float[] vp = _ActualV;

            //int inc = v_inc;
            float[] tmpOut = _TmpOut;
            int dvp = 0;

            // fat chance of having this loop unroll
            for (int i = 0; i < 32; i++) {
                float[] dp = _d16[i];
                float pcSample = (vp[15 + dvp] * dp[0] + vp[14 + dvp] * dp[1] + vp[13 + dvp] * dp[2] + vp[12 + dvp] * dp[3] +
                                  vp[11 + dvp] * dp[4] + vp[10 + dvp] * dp[5] + vp[9 + dvp] * dp[6] + vp[8 + dvp] * dp[7] +
                                  vp[7 + dvp] * dp[8] + vp[6 + dvp] * dp[9] + vp[5 + dvp] * dp[10] + vp[4 + dvp] * dp[11] +
                                  vp[3 + dvp] * dp[12] + vp[2 + dvp] * dp[13] + vp[1 + dvp] * dp[14] + vp[0 + dvp] * dp[15]) *
                                 _Scalefactor;

                tmpOut[i] = pcSample;
                dvp += 16;
            }
            // for
        }

        private void compute_pc_samples(ABuffer buffer) {
            switch (_ActualWritePos) {
                case 0:
                    compute_pc_samples0(buffer);
                    break;

                case 1:
                    compute_pc_samples1(buffer);
                    break;

                case 2:
                    compute_pc_samples2(buffer);
                    break;

                case 3:
                    compute_pc_samples3(buffer);
                    break;

                case 4:
                    compute_pc_samples4(buffer);
                    break;

                case 5:
                    compute_pc_samples5(buffer);
                    break;

                case 6:
                    compute_pc_samples6(buffer);
                    break;

                case 7:
                    compute_pc_samples7(buffer);
                    break;

                case 8:
                    compute_pc_samples8(buffer);
                    break;

                case 9:
                    compute_pc_samples9(buffer);
                    break;

                case 10:
                    compute_pc_samples10(buffer);
                    break;

                case 11:
                    compute_pc_samples11(buffer);
                    break;

                case 12:
                    compute_pc_samples12(buffer);
                    break;

                case 13:
                    compute_pc_samples13(buffer);
                    break;

                case 14:
                    compute_pc_samples14(buffer);
                    break;

                case 15:
                    Compute_pc_samples15(buffer);
                    break;
            }

            buffer?.AppendSamples(_Channel, _TmpOut);
        }

        /// <summary>
        /// Calculate 32 PCM samples and put the into the Obuffer-object.
        /// </summary>
        internal void calculate_pc_samples(ABuffer buffer) {
            ComputeNewValues();
            compute_pc_samples(buffer);

            _ActualWritePos = (_ActualWritePos + 1) & 0xf;
            _ActualV = _ActualV == _V1 ? _V2 : _V1;

            // initialize samples[]:
            //for (register float *floatp = samples + 32; floatp > samples; )
            // *--floatp = 0.0f;  

            // MDM: this may not be necessary. The Layer III decoder always
            // outputs 32 subband samples, but I haven't checked layer I & II.
            for (int p = 0; p < 32; p++)
                _Samples[p] = 0.0f;
        }

        /// <summary>
        /// Converts a 1D array into a number of smaller arrays. This is used
        /// to achieve offset + constant indexing into an array. Each sub-array
        /// represents a block of values of the original array.
        /// </summary>
        /// <param name="array">
        /// The array to split up into blocks.
        /// </param>
        /// <param name="blockSize">
        /// The size of the blocks to split the array
        /// into. This must be an exact divisor of
        /// the length of the array, or some data
        /// will be lost from the main array.
        /// </param>
        /// <returns>
        /// An array of arrays in which each element in the returned
        /// array will be of length blockSize.
        /// </returns>
        private static float[][] SplitArray(float[] array, int blockSize) {
            int size = array.Length / blockSize;
            float[][] split = new float[size][];
            for (int i = 0; i < size; i++) {
                split[i] = SubArray(array, i * blockSize, blockSize);
            }
            return split;
        }

        private static float[] SubArray(float[] array, int offs, int len) {
            if (offs + len > array.Length) {
                len = array.Length - offs;
            }

            if (len < 0)
                len = 0;

            float[] subarray = new float[len];
            for (int i = 0; i < len; i++) {
                subarray[i] = array[offs + i];
            }

            return subarray;
        }
    }
}