// /***************************************************************************
//  * Equalizer.cs
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
    /// The Equalizer class can be used to specify
    /// equalization settings for the MPEG audio decoder.
    /// The equalizer consists of 32 band-pass filters.
    /// Each band of the equalizer can take on a fractional value between
    /// -1.0 and +1.0.
    /// At -1.0, the input signal is attenuated by 6dB, at +1.0 the signal is
    /// amplified by 6dB.
    /// </summary>
    public class Equalizer {
        private const int BANDS = 32;

        /// <summary>
        /// Equalizer setting to denote that a given band will not be
        /// present in the output signal.
        /// </summary>
        internal const float BAND_NOT_PRESENT = float.NegativeInfinity;

        internal static readonly Equalizer PassThruEq = new Equalizer();
        private float[] _Settings;

        /// <summary>
        /// Creates a new Equalizer instance.
        /// </summary>
        internal Equalizer() {
            InitBlock();
        }

        //    private Equalizer(float b1, float b2, float b3, float b4, float b5,
        //                     float b6, float b7, float b8, float b9, float b10, float b11,
        //                     float b12, float b13, float b14, float b15, float b16,
        //                     float b17, float b18, float b19, float b20);

        internal Equalizer(float[] settings) {
            InitBlock();
            FromFloatArray = settings;
        }

        internal Equalizer(EQFunction eq) {
            InitBlock();
            FromEQFunction = eq;
        }

        internal float[] FromFloatArray {
            set {
                Reset();
                int max = value.Length > BANDS ? BANDS : value.Length;

                for (int i = 0; i < max; i++) {
                    _Settings[i] = Limit(value[i]);
                }
            }
        }

        //UPGRADE_TODO: Method 'setFrom' was converted to a set modifier. This name conflicts with another property. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1137"'
        /// <summary>
        /// Sets the bands of this equalizer to the value the bands of
        /// another equalizer. Bands that are not present in both equalizers are ignored.
        /// </summary>
        internal virtual Equalizer FromEqualizer {
            set {
                if (value != this) {
                    FromFloatArray = value._Settings;
                }
            }
        }

        //UPGRADE_TODO: Method 'setFrom' was converted to a set modifier. This name conflicts with another property. 'ms-help://MS.VSCC.2003/commoner/redir/redirect.htm?keyword="jlca1137"'
        internal EQFunction FromEQFunction {
            set {
                Reset();
                int max = BANDS;

                for (int i = 0; i < max; i++) {
                    _Settings[i] = Limit(value.GetBand(i));
                }
            }
        }

        /// <summary>
        /// Retrieves the number of bands present in this equalizer.
        /// </summary>
        internal virtual int BandCount => _Settings.Length;

        /// <summary>
        /// Retrieves an array of floats whose values represent a
        /// scaling factor that can be applied to linear samples
        /// in each band to provide the equalization represented by
        /// this instance.
        /// </summary>
        /// <returns>
        /// an array of factors that can be applied to the
        /// subbands.
        /// </returns>
        internal virtual float[] BandFactors {
            get {
                float[] factors = new float[BANDS];
                for (int i = 0; i < BANDS; i++) {
                    factors[i] = GetBandFactor(_Settings[i]);
                }

                return factors;
            }
        }

        private void InitBlock() {
            _Settings = new float[BANDS];
        }

        /// <summary>
        /// Sets all bands to 0.0
        /// </summary>
        internal void Reset() {
            for (int i = 0; i < BANDS; i++) {
                _Settings[i] = 0.0f;
            }
        }

        internal float SetBand(int band, float neweq) {
            float eq = 0.0f;

            if (band >= 0 && band < BANDS) {
                eq = _Settings[band];
                _Settings[band] = Limit(neweq);
            }

            return eq;
        }

        /// <summary>
        /// Retrieves the eq setting for a given band.
        /// </summary>
        internal float GetBand(int band) {
            float eq = 0.0f;

            if (band >= 0 && band < BANDS) {
                eq = _Settings[band];
            }

            return eq;
        }

        private float Limit(float eq) {
            if (eq == BAND_NOT_PRESENT)
                return eq;
            if (eq > 1.0f)
                return 1.0f;
            if (eq < -1.0f)
                return -1.0f;

            return eq;
        }

        /// <summary>
        /// Converts an equalizer band setting to a sample factor.
        /// The factor is determined by the function f = 2^n where
        /// n is the equalizer band setting in the range [-1.0,1.0].
        /// </summary>
        internal float GetBandFactor(float eq) {
            if (eq == BAND_NOT_PRESENT)
                return 0.0f;

            float f = (float)Math.Pow(2.0, eq);
            return f;
        }

        internal abstract class EQFunction {
            /// <summary>
            /// Returns the setting of a band in the equalizer.
            /// </summary>
            /// <param name="band">
            /// The index of the band to retrieve the setting for.
            /// </param>
            /// <returns>
            /// the setting of the specified band. This is a value between
            /// -1 and +1.
            /// </returns>
            internal virtual float GetBand(int band) => 0.0f;
        }
    }
}