/*
 * $Id$
 *
 * This file is part of the iText (R) project.
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

namespace iTextSharp.text.pdf {
    public class StringUtils {

        private static readonly byte[] r = DocWriter.GetISOBytes("\\r");
        private static readonly byte[] n = DocWriter.GetISOBytes("\\n");
        private static readonly byte[] t = DocWriter.GetISOBytes("\\t");
        private static readonly byte[] b = DocWriter.GetISOBytes("\\b");
        private static readonly byte[] f = DocWriter.GetISOBytes("\\f");

        private StringUtils() {
            
        }

        /**
         * Escapes a <CODE>byte</CODE> array according to the PDF conventions.
         *
         * @param b the <CODE>byte</CODE> array to escape
         * @return an escaped <CODE>byte</CODE> array
         */
        public static byte[] EscapeString(byte[] b) {
            ByteBuffer content = new ByteBuffer();
            EscapeString(b, content);
            return content.ToByteArray();
        }

        /**
         * Escapes a <CODE>byte</CODE> array according to the PDF conventions.
         *
         * @param b the <CODE>byte</CODE> array to escape
         */
        public static void EscapeString(byte[] bytes, ByteBuffer content) {
            content.Append_i('(');
            for (int k = 0; k < bytes.Length; ++k) {
                byte c = bytes[k];
                switch ((int) c) {
                    case '\r':
                        content.Append(r);
                        break;
                    case '\n':
                        content.Append(n);
                        break;
                    case '\t':
                        content.Append(t);
                        break;
                    case '\b':
                        content.Append(b);
                        break;
                    case '\f':
                        content.Append(f);
                        break;
                    case '(':
                    case ')':
                    case '\\':
                        content.Append_i('\\').Append_i(c);
                        break;
                    default:
                        content.Append_i(c);
                        break;
                }
            }
            content.Append(')');
        }

         /**
         * Converts an array of unsigned 16bit numbers to an array of bytes.
         * The input values are presented as chars for convenience.
         * 
         * @param chars the array of 16bit numbers that should be converted
         * @return the resulting byte array, twice as large as the input
         */
        public static byte[] ConvertCharsToBytes(char[] chars) {
            byte[] result = new byte[chars.Length * 2];
            for (int i = 0; i < chars.Length; i++) {
                result[2 * i] = (byte)(chars[i] / 256);
                result[2 * i + 1] = (byte)(chars[i] % 256);
            }
            return result;
        }

    }
}
