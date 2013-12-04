using System;
using System.Collections.Generic;
/*
 * Copyright 2008 ZXing authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace iTextSharp.text.pdf.qrcode {

    /**
     * <p>Implements Reed-Solomon enbcoding, as the name implies.</p>
     *
     * @author Sean Owen
     * @author William Rucklidge
     */
    public sealed class ReedSolomonEncoder {

        private GF256 field;
        private List<GF256Poly> cachedGenerators;

        public ReedSolomonEncoder(GF256 field) {
            if (!GF256.QR_CODE_FIELD.Equals(field)) {
                throw new ArgumentException("Only QR Code is supported at this time");
            }
            this.field = field;
            this.cachedGenerators = new List<GF256Poly>();
            cachedGenerators.Add(new GF256Poly(field, new int[] { 1 }));
        }

        private GF256Poly BuildGenerator(int degree) {
            if (degree >= cachedGenerators.Count) {
                GF256Poly lastGenerator = cachedGenerators[cachedGenerators.Count - 1];
                for (int d = cachedGenerators.Count; d <= degree; d++) {
                    GF256Poly nextGenerator = lastGenerator.Multiply(new GF256Poly(field, new int[] { 1, field.Exp(d - 1) }));
                    cachedGenerators.Add(nextGenerator);
                    lastGenerator = nextGenerator;
                }
            }
            return cachedGenerators[degree];
        }

        public void Encode(int[] toEncode, int ecBytes) {
            if (ecBytes == 0) {
                throw new ArgumentException("No error correction bytes");
            }
            int dataBytes = toEncode.Length - ecBytes;
            if (dataBytes <= 0) {
                throw new ArgumentException("No data bytes provided");
            }
            GF256Poly generator = BuildGenerator(ecBytes);
            int[] infoCoefficients = new int[dataBytes];
            System.Array.Copy(toEncode, 0, infoCoefficients, 0, dataBytes);
            GF256Poly info = new GF256Poly(field, infoCoefficients);
            info = info.MultiplyByMonomial(ecBytes, 1);
            GF256Poly remainder = info.Divide(generator)[1];
            int[] coefficients = remainder.GetCoefficients();
            int numZeroCoefficients = ecBytes - coefficients.Length;
            for (int i = 0; i < numZeroCoefficients; i++) {
                toEncode[dataBytes + i] = 0;
            }
            System.Array.Copy(coefficients, 0, toEncode, dataBytes + numZeroCoefficients, coefficients.Length);
        }
    }
}
