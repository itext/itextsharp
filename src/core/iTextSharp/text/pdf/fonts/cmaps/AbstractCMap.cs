using System;
using iTextSharp.text.pdf;
/*
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
namespace iTextSharp.text.pdf.fonts.cmaps {

    /**
     *
     * @author psoares
     */
    public abstract class AbstractCMap {

        private String cmapName;
        private String registry;

        private String ordering;

        private int supplement;

        virtual public int Supplement {
            get { return supplement; }
            set { supplement = value; }
        }
        
        virtual public String Name {
            get { return cmapName; }
            set { cmapName = value; }
        }

        virtual public String Ordering {
            get { return ordering; }
            set { ordering = value; }
        }
        
        virtual public String Registry {
            get { return registry; }
            set { registry = value; }
        }


        internal abstract void AddChar(PdfString mark, PdfObject code);
        
        internal void AddRange(PdfString from, PdfString to, PdfObject code) {
            byte[] a1 = DecodeStringToByte(from);
            byte[] a2 = DecodeStringToByte(to);
            if (a1.Length != a2.Length || a1.Length == 0)
                throw new ArgumentException("Invalid map.");
            byte[] sout = null;
            if (code is PdfString)
                sout = DecodeStringToByte((PdfString)code);
            int start = ByteArrayToInt(a1);
            int end = ByteArrayToInt(a2);
            for (int k = start; k <= end; ++k) {
                IntToByteArray(k, a1);
                PdfString s = new PdfString(a1);
                s.SetHexWriting(true);
                if (code is PdfArray) {
                    AddChar(s, ((PdfArray)code)[k - start]);
                }
                else if (code is PdfNumber) {
                    int nn = ((PdfNumber)code).IntValue + k - start;
                    AddChar(s, new PdfNumber(nn));
                }
                else if (code is PdfString) {
                    PdfString s1 = new PdfString(sout);
                    s1.SetHexWriting(true);
                    ++sout[sout.Length - 1];
                    AddChar(s, s1);
                }
            }
        }
        
        private static void IntToByteArray(int v, byte[] b) {
            for (int k = b.Length - 1; k >= 0; --k) {
                b[k] = (byte)v;
                v = v >> 8;
            }
        }
    
        private static int ByteArrayToInt(byte[] b) {
            int v = 0;
            for (int k = 0; k < b.Length; ++k) {
                v = v << 8;
                v |= b[k] & 0xff;
            }
            return v;
        }

        public static byte[] DecodeStringToByte(PdfString s) {
            byte[] b = s.GetBytes();
            byte[] br = new byte[b.Length];
            System.Array.Copy(b, 0, br, 0, b.Length);
            return br;
        }

        virtual public String DecodeStringToUnicode(PdfString ps) {
            if (ps.IsHexWriting())
                return PdfEncodings.ConvertToString(ps.GetBytes(), "UnicodeBigUnmarked");
            else
                return ps.ToUnicodeString();
        }
    }
}
