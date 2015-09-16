using System;
using System.Collections.Generic;
using System.IO;
using iTextSharp.text.error_messages;
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
    public class CMapParserEx {
        
        private static readonly PdfName CMAPNAME = new PdfName("CMapName");
        private const String DEF = "def";
        private const String ENDCIDRANGE = "endcidrange";
        private const String ENDCIDCHAR = "endcidchar";
        private const String ENDBFRANGE = "endbfrange";
        private const String ENDBFCHAR = "endbfchar";
        private const String USECMAP = "usecmap";
        private const int MAXLEVEL = 10;
        
        public static void ParseCid(String cmapName, AbstractCMap cmap, ICidLocation location) {
            ParseCid(cmapName, cmap, location, 0);
        }
        
        private static void ParseCid(String cmapName, AbstractCMap cmap, ICidLocation location, int level) {
            if (level >= MAXLEVEL)
                return;
            PRTokeniser inp = location.GetLocation(cmapName);
            try {
                List<PdfObject> list = new List<PdfObject>();
                PdfContentParser cp = new PdfContentParser(inp);
                int maxExc = 50;
                while (true) {
                    try {
                        cp.Parse(list);
                    }
                    catch {
                        if (--maxExc < 0)
                            break;
                        continue;
                    }
                    if (list.Count == 0)
                        break;
                    String last = list[list.Count - 1].ToString();
                    if (level == 0 && list.Count == 3 && last.Equals(DEF)) {
                        PdfObject key = list[0];
                        if (PdfName.REGISTRY.Equals(key))
                            cmap.Registry = list[1].ToString();
                        else if (PdfName.ORDERING.Equals(key))
                            cmap.Ordering = list[1].ToString();
                        else if (CMAPNAME.Equals(key))
                            cmap.Name = list[1].ToString();
                        else if (PdfName.SUPPLEMENT.Equals(key)) {
                            try {
                                cmap.Supplement = ((PdfNumber)list[1]).IntValue;
                            }
                            catch {}
                        }
                    }
                    else if ((last.Equals(ENDCIDCHAR) || last.Equals(ENDBFCHAR)) && list.Count >= 3) {
                        int lmax = list.Count - 2;
                        for (int k = 0; k < lmax; k += 2) {
                            if (list[k] is PdfString) {
                                cmap.AddChar((PdfString)list[k], list[k + 1]);
                            }
                        }
                    }
                    else if ((last.Equals(ENDCIDRANGE) || last.Equals(ENDBFRANGE)) && list.Count >= 4) {
                        int lmax = list.Count - 3;
                        for (int k = 0; k < lmax; k += 3) {
                            if (list[k] is PdfString && list[k + 1] is PdfString) {
                                cmap.AddRange((PdfString)list[k], (PdfString)list[k + 1], list[k + 2]);
                            }
                        }
                    }
                    else if (last.Equals(USECMAP) && list.Count == 2 && list[0] is PdfName) {
                        ParseCid(PdfName.DecodeName(list[0].ToString()), cmap, location, level + 1);
                    }
                }
            }
            finally {
                inp.Close();
            }
        }
    }
}
