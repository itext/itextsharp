using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.util;
using iTextSharp.text.io;

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

namespace iTextSharp.text.pdf {

    public class GlyphList {
        private static Dictionary<int,string> unicode2names = new Dictionary<int,string>();
        private static Dictionary<string,int[]> names2unicode = new Dictionary<string,int[]>();
    
        static GlyphList() {
            Stream istr = null;
            try {
                istr = StreamUtil.GetResourceStream(BaseFont.RESOURCE_PATH + "glyphlist.txt");
                if (istr == null) {
                    String msg = "glyphlist.txt not found as resource.";
                    throw new Exception(msg);
                }
                byte[] buf = new byte[1024];
                MemoryStream outp = new MemoryStream();
                while (true) {
                    int size = istr.Read(buf, 0, buf.Length);
                    if (size == 0)
                        break;
                    outp.Write(buf, 0, size);
                }
                istr.Close();
                istr = null;
                String s = PdfEncodings.ConvertToString(outp.ToArray(), null);
                StringTokenizer tk = new StringTokenizer(s, "\r\n");
                while (tk.HasMoreTokens()) {
                    String line = tk.NextToken();
                    if (line.StartsWith("#"))
                        continue;
                    StringTokenizer t2 = new StringTokenizer(line, " ;\r\n\t\f");
                    String name = null;
                    String hex = null;
                    if (!t2.HasMoreTokens())
                        continue;
                    name = t2.NextToken();
                    if (!t2.HasMoreTokens())
                        continue;
                    hex = t2.NextToken();
                    int num = int.Parse(hex, NumberStyles.HexNumber);
                    unicode2names[num] = name;
                    names2unicode[name] = new int[]{num};
                }
            }
            catch (Exception e) {
                Console.Error.WriteLine("glyphlist.txt loading error: " + e.Message);
            }
            finally {
                if (istr != null) {
                    try {
                        istr.Close();
                    }
                    catch {
                        // empty on purpose
                    }
                }
            }
        }
    
        public static int[] NameToUnicode(string name) {
            int[] v;
            names2unicode.TryGetValue(name, out v);
            if (v == null && name.Length == 7 && name.ToLowerInvariant().StartsWith("uni")) {
                try {
                    return new int[]{int.Parse(name.Substring(3), NumberStyles.HexNumber)};
                }
                catch {
                }
            }
            return v;
        }
    
        public static string UnicodeToName(int num) {
            string a;
            unicode2names.TryGetValue(num, out a);
            return a;
        }
    }
}
