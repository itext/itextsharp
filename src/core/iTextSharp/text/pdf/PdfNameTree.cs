using System;
using System.Collections.Generic;
/*
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

namespace iTextSharp.text.pdf {
    /**
    * Creates a name tree.
    * @author Paulo Soares
    */
    public class PdfNameTree {
        
        private const int leafSize = 64;
        
        /**
        * Creates a name tree.
        * @param items the item of the name tree. The key is a <CODE>String</CODE>
        * and the value is a <CODE>PdfObject</CODE>. Note that although the
        * keys are strings only the lower byte is used and no check is made for chars
        * with the same lower byte and different upper byte. This will generate a wrong
        * tree name.
        * @param writer the writer
        * @throws IOException on error
        * @return the dictionary with the name tree. This dictionary is the one
        * generally pointed to by the key /Dests, for example
        */    
        public static PdfDictionary WriteTree<T>(Dictionary<String, T> items, PdfWriter writer) where T : PdfObject {
            if (items.Count == 0)
                return null;
            String[] names = new String[items.Count];
            items.Keys.CopyTo(names, 0);
            Array.Sort(names, new CompareSrt());
            if (names.Length <= leafSize) {
                PdfDictionary dic = new PdfDictionary();
                PdfArray ar = new PdfArray();
                for (int k = 0; k < names.Length; ++k) {
                    ar.Add(new PdfString(names[k], null));
                    ar.Add(items[names[k]]);
                }
                dic.Put(PdfName.NAMES, ar);
                return dic;
            }
            int skip = leafSize;
            PdfIndirectReference[] kids = new PdfIndirectReference[(names.Length + leafSize - 1) / leafSize];
            for (int k = 0; k < kids.Length; ++k) {
                int offset = k * leafSize;
                int end = Math.Min(offset + leafSize, names.Length);
                PdfDictionary dic = new PdfDictionary();
                PdfArray arr = new PdfArray();
                arr.Add(new PdfString(names[offset], null));
                arr.Add(new PdfString(names[end - 1], null));
                dic.Put(PdfName.LIMITS, arr);
                arr = new PdfArray();
                for (; offset < end; ++offset) {
                    arr.Add(new PdfString(names[offset], null));
                    arr.Add(items[names[offset]]);
                }
                dic.Put(PdfName.NAMES, arr);
                kids[k] = writer.AddToBody(dic).IndirectReference;
            }
            int top = kids.Length;
            while (true) {
                if (top <= leafSize) {
                    PdfArray arr = new PdfArray();
                    for (int k = 0; k < top; ++k)
                        arr.Add(kids[k]);
                    PdfDictionary dic = new PdfDictionary();
                    dic.Put(PdfName.KIDS, arr);
                    return dic;
                }
                skip *= leafSize;
                int tt = (names.Length + skip - 1 )/ skip;
                for (int k = 0; k < tt; ++k) {
                    int offset = k * leafSize;
                    int end = Math.Min(offset + leafSize, top);
                    PdfDictionary dic = new PdfDictionary();
                    PdfArray arr = new PdfArray();
                    arr.Add(new PdfString(names[k * skip], null));
                    arr.Add(new PdfString(names[Math.Min((k + 1) * skip, names.Length) - 1], null));
                    dic.Put(PdfName.LIMITS, arr);
                    arr = new PdfArray();
                    for (; offset < end; ++offset) {
                        arr.Add(kids[offset]);
                    }
                    dic.Put(PdfName.KIDS, arr);
                    kids[k] = writer.AddToBody(dic).IndirectReference;
                }
                top = tt;
            }
        }

        private static PdfString IterateItems(PdfDictionary dic, Dictionary<string, PdfObject> items, PdfString leftOverString) {
            PdfArray nn = (PdfArray)PdfReader.GetPdfObjectRelease(dic.Get(PdfName.NAMES));
            if (nn != null) {
                for (int k = 0; k < nn.Size; ++k) {
                    PdfString s;
                    if (leftOverString == null)
                        s = (PdfString)PdfReader.GetPdfObjectRelease(nn.GetPdfObject(k++));
                    else {
                        // this is the leftover string from the previous loop
                        s = leftOverString;
                        leftOverString = null;
                    }
                    if (k < nn.Size) // could have a mistake int the pdf file
                        items[PdfEncodings.ConvertToString(s.GetBytes(), null)] = nn.GetPdfObject(k);
                    else
                        return s;
                }
            } else if ((nn = (PdfArray)PdfReader.GetPdfObjectRelease(dic.Get(PdfName.KIDS))) != null) {
                for (int k = 0; k < nn.Size; ++k) {
                    PdfDictionary kid = (PdfDictionary)PdfReader.GetPdfObjectRelease(nn.GetPdfObject(k));
                    leftOverString = IterateItems(kid, items, leftOverString);
                }
            }
            return null;
        }
        
        public static Dictionary<string, PdfObject> ReadTree(PdfDictionary dic) {
            Dictionary<string, PdfObject> items = new Dictionary<string,PdfObject>();
            if (dic != null)
                IterateItems(dic, items, null);
            return items;
        }

        internal class CompareSrt : IComparer<string>  {
            virtual public int Compare(string x, string y) {
                char[] a = x.ToCharArray();
                char[] b = y.ToCharArray();
                int m = Math.Min(a.Length, b.Length);
                for (int k = 0; k < m; ++k) {
                    if (a[k] < b[k])
                        return -1;
                    if (a[k] > b[k])
                        return 1;
                }
                if (a.Length < b.Length)
                    return -1;
                if (a.Length > b.Length)
                    return 1;
                return 0;
            }
        }
    }
}
