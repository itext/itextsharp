using System;

/*
 * $Id$
 * 
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */

namespace iTextSharp.text
{
    public class SpecialSymbol {
        
    /**
    * Returns the first occurrence of a special symbol in a <CODE>String</CODE>.
    *
    * @param    string        a <CODE>String</CODE>
    * @return    an index of -1 if no special symbol was found
    */
        
        public static int Index(string str) {
            int length = str.Length;
            for (int i = 0; i < length; i++) {
                if (GetCorrespondingSymbol(str[i]) != ' ') {
                    return i;
                }
            }
            return -1;
        }
        
    /**
    * Gets a chunk with a symbol character.
    * @param c a character that has to be changed into a symbol
    * @param font Font if there is no SYMBOL character corresponding with c
    * @return a SYMBOL version of a character
    */
        
        public static Chunk Get(char c, Font font) {
            char greek = SpecialSymbol.GetCorrespondingSymbol(c);
            if (greek == ' ') {
                return new Chunk(c.ToString(), font);
            }
            Font symbol = new Font(Font.FontFamily.SYMBOL, font.Size, font.Style, font.Color);
            return new Chunk(greek.ToString(), symbol);
        }
        
    /**
    * Looks for the corresponding symbol in the font Symbol.
    *
    * @param    c    the original ASCII-char
    * @return    the corresponding symbol in font Symbol
    */
        
        public static char GetCorrespondingSymbol(char c) {
            switch (c) {
                case (char)913:
                    return 'A'; // ALFA
                case (char)914:
                    return 'B'; // BETA
                case (char)915:
                    return 'G'; // GAMMA
                case (char)916:
                    return 'D'; // DELTA
                case (char)917:
                    return 'E'; // EPSILON
                case (char)918:
                    return 'Z'; // ZETA
                case (char)919:
                    return 'H'; // ETA
                case (char)920:
                    return 'Q'; // THETA
                case (char)921:
                    return 'I'; // IOTA
                case (char)922:
                    return 'K'; // KAPPA
                case (char)923:
                    return 'L'; // LAMBDA
                case (char)924:
                    return 'M'; // MU
                case (char)925:
                    return 'N'; // NU
                case (char)926:
                    return 'X'; // XI
                case (char)927:
                    return 'O'; // OMICRON
                case (char)928:
                    return 'P'; // PI
                case (char)929:
                    return 'R'; // RHO
                case (char)931:
                    return 'S'; // SIGMA
                case (char)932:
                    return 'T'; // TAU
                case (char)933:
                    return 'U'; // UPSILON
                case (char)934:
                    return 'F'; // PHI
                case (char)935:
                    return 'C'; // CHI
                case (char)936:
                    return 'Y'; // PSI
                case (char)937:
                    return 'W'; // OMEGA
                case (char)945:
                    return 'a'; // alfa
                case (char)946:
                    return 'b'; // beta
                case (char)947:
                    return 'g'; // gamma
                case (char)948:
                    return 'd'; // delta
                case (char)949:
                    return 'e'; // epsilon
                case (char)950:
                    return 'z'; // zeta
                case (char)951:
                    return 'h'; // eta
                case (char)952:
                    return 'q'; // theta
                case (char)953:
                    return 'i'; // iota
                case (char)954:
                    return 'k'; // kappa
                case (char)955:
                    return 'l'; // lambda
                case (char)956:
                    return 'm'; // mu
                case (char)957:
                    return 'n'; // nu
                case (char)958:
                    return 'x'; // xi
                case (char)959:
                    return 'o'; // omicron
                case (char)960:
                    return 'p'; // pi
                case (char)961:
                    return 'r'; // rho
                case (char)962:
                    return 'V'; // sigma
                case (char)963:
                    return 's'; // sigma
                case (char)964:
                    return 't'; // tau
                case (char)965:
                    return 'u'; // upsilon
                case (char)966:
                    return 'f'; // phi
                case (char)967:
                    return 'c'; // chi
                case (char)968:
                    return 'y'; // psi
                case (char)969:
                    return 'w'; // omega
                default:
                        return ' ';
            }
        }
    }
}
