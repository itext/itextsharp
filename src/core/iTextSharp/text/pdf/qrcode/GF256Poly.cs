using System;
using System.Text;
/*
 * Copyright 2007 ZXing authors
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
     * <p>Represents a polynomial whose coefficients are elements of GF(256).
     * Instances of this class are immutable.</p>
     *
     * <p>Much credit is due to William Rucklidge since portions of this code are an indirect
     * port of his C++ Reed-Solomon implementation.</p>
     *
     * @author Sean Owen
     */
    internal sealed class GF256Poly {

        private GF256 field;
        private int[] coefficients;

        /**
         * @param field the {@link GF256} instance representing the field to use
         * to perform computations
         * @param coefficients coefficients as ints representing elements of GF(256), arranged
         * from most significant (highest-power term) coefficient to least significant
         * @throws IllegalArgumentException if argument is null or empty,
         * or if leading coefficient is 0 and this is not a
         * constant polynomial (that is, it is not the monomial "0")
         */
        internal GF256Poly(GF256 field, int[] coefficients) {
            if (coefficients == null || coefficients.Length == 0) {
                throw new ArgumentException();
            }
            this.field = field;
            int coefficientsLength = coefficients.Length;
            if (coefficientsLength > 1 && coefficients[0] == 0) {
                // Leading term must be non-zero for anything except the constant polynomial "0"
                int firstNonZero = 1;
                while (firstNonZero < coefficientsLength && coefficients[firstNonZero] == 0) {
                    firstNonZero++;
                }
                if (firstNonZero == coefficientsLength) {
                    this.coefficients = field.GetZero().coefficients;
                }
                else {
                    this.coefficients = new int[coefficientsLength - firstNonZero];
                    System.Array.Copy(coefficients,
                        firstNonZero,
                        this.coefficients,
                        0,
                        this.coefficients.Length);
                }
            }
            else {
                this.coefficients = coefficients;
            }
        }

        internal int[] GetCoefficients() {
            return coefficients;
        }

        /**
         * @return degree of this polynomial
         */
        internal int GetDegree() {
            return coefficients.Length - 1;
        }

        /**
         * @return true iff this polynomial is the monomial "0"
         */
        internal bool IsZero() {
            return coefficients[0] == 0;
        }

        /**
         * @return coefficient of x^degree term in this polynomial
         */
        internal int GetCoefficient(int degree) {
            return coefficients[coefficients.Length - 1 - degree];
        }

        /**
         * @return evaluation of this polynomial at a given point
         */
        internal int EvaluateAt(int a) {
            if (a == 0) {
                // Just return the x^0 coefficient
                return GetCoefficient(0);
            }
            int size = coefficients.Length;
            if (a == 1) {
                // Just the sum of the coefficients
                int result2 = 0;
                for (int i = 0; i < size; i++) {
                    result2 = GF256.AddOrSubtract(result2, coefficients[i]);
                }
                return result2;
            }
            int result = coefficients[0];
            for (int i = 1; i < size; i++) {
                result = GF256.AddOrSubtract(field.Multiply(a, result), coefficients[i]);
            }
            return result;
        }

        internal GF256Poly AddOrSubtract(GF256Poly other) {
            if (!field.Equals(other.field)) {
                throw new ArgumentException("GF256Polys do not have same GF256 field");
            }
            if (IsZero()) {
                return other;
            }
            if (other.IsZero()) {
                return this;
            }

            int[] smallerCoefficients = this.coefficients;
            int[] largerCoefficients = other.coefficients;
            if (smallerCoefficients.Length > largerCoefficients.Length) {
                int[] temp = smallerCoefficients;
                smallerCoefficients = largerCoefficients;
                largerCoefficients = temp;
            }
            int[] sumDiff = new int[largerCoefficients.Length];
            int lengthDiff = largerCoefficients.Length - smallerCoefficients.Length;
            // Copy high-order terms only found in higher-degree polynomial's coefficients
            System.Array.Copy(largerCoefficients, 0, sumDiff, 0, lengthDiff);

            for (int i = lengthDiff; i < largerCoefficients.Length; i++) {
                sumDiff[i] = GF256.AddOrSubtract(smallerCoefficients[i - lengthDiff], largerCoefficients[i]);
            }

            return new GF256Poly(field, sumDiff);
        }

        internal GF256Poly Multiply(GF256Poly other) {
            if (!field.Equals(other.field)) {
                throw new ArgumentException("GF256Polys do not have same GF256 field");
            }
            if (IsZero() || other.IsZero()) {
                return field.GetZero();
            }
            int[] aCoefficients = this.coefficients;
            int aLength = aCoefficients.Length;
            int[] bCoefficients = other.coefficients;
            int bLength = bCoefficients.Length;
            int[] product = new int[aLength + bLength - 1];
            for (int i = 0; i < aLength; i++) {
                int aCoeff = aCoefficients[i];
                for (int j = 0; j < bLength; j++) {
                    product[i + j] = GF256.AddOrSubtract(product[i + j],
                        field.Multiply(aCoeff, bCoefficients[j]));
                }
            }
            return new GF256Poly(field, product);
        }

        internal GF256Poly Multiply(int scalar) {
            if (scalar == 0) {
                return field.GetZero();
            }
            if (scalar == 1) {
                return this;
            }
            int size = coefficients.Length;
            int[] product = new int[size];
            for (int i = 0; i < size; i++) {
                product[i] = field.Multiply(coefficients[i], scalar);
            }
            return new GF256Poly(field, product);
        }

        internal GF256Poly MultiplyByMonomial(int degree, int coefficient) {
            if (degree < 0) {
                throw new ArgumentException();
            }
            if (coefficient == 0) {
                return field.GetZero();
            }
            int size = coefficients.Length;
            int[] product = new int[size + degree];
            for (int i = 0; i < size; i++) {
                product[i] = field.Multiply(coefficients[i], coefficient);
            }
            return new GF256Poly(field, product);
        }

        internal GF256Poly[] Divide(GF256Poly other) {
            if (!field.Equals(other.field)) {
                throw new ArgumentException("GF256Polys do not have same GF256 field");
            }
            if (other.IsZero()) {
                throw new DivideByZeroException("Divide by 0");
            }

            GF256Poly quotient = field.GetZero();
            GF256Poly remainder = this;

            int denominatorLeadingTerm = other.GetCoefficient(other.GetDegree());
            int inverseDenominatorLeadingTerm = field.Inverse(denominatorLeadingTerm);

            while (remainder.GetDegree() >= other.GetDegree() && !remainder.IsZero()) {
                int degreeDifference = remainder.GetDegree() - other.GetDegree();
                int scale = field.Multiply(remainder.GetCoefficient(remainder.GetDegree()), inverseDenominatorLeadingTerm);
                GF256Poly term = other.MultiplyByMonomial(degreeDifference, scale);
                GF256Poly iterationQuotient = field.BuildMonomial(degreeDifference, scale);
                quotient = quotient.AddOrSubtract(iterationQuotient);
                remainder = remainder.AddOrSubtract(term);
            }

            return new GF256Poly[] { quotient, remainder };
        }

        public override String ToString() {
            StringBuilder result = new StringBuilder(8 * GetDegree());
            for (int degree = GetDegree(); degree >= 0; degree--) {
                int coefficient = GetCoefficient(degree);
                if (coefficient != 0) {
                    if (coefficient < 0) {
                        result.Append(" - ");
                        coefficient = -coefficient;
                    }
                    else {
                        if (result.Length > 0) {
                            result.Append(" + ");
                        }
                    }
                    if (degree == 0 || coefficient != 1) {
                        int alphaPower = field.Log(coefficient);
                        if (alphaPower == 0) {
                            result.Append('1');
                        }
                        else if (alphaPower == 1) {
                            result.Append('a');
                        }
                        else {
                            result.Append("a^");
                            result.Append(alphaPower);
                        }
                    }
                    if (degree != 0) {
                        if (degree == 1) {
                            result.Append('x');
                        }
                        else {
                            result.Append("x^");
                            result.Append(degree);
                        }
                    }
                }
            }
            return result.ToString();
        }
    }
}
