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

using System;
using System.Collections.Generic;
using System.util.collections;

namespace iTextSharp.text.pdf{

    public class TtfUnicodeWriter
    {

        protected PdfWriter writer = null;

        public TtfUnicodeWriter(PdfWriter writer)
        {
            this.writer = writer;
        }

        internal protected virtual void WriteFont(TrueTypeFontUnicode font, PdfIndirectReference refer, Object[] parms, byte[] rotbits)
        {
            Dictionary<int, int[]> longTag = (Dictionary<int, int[]>)parms[0];
            font.AddRangeUni(longTag, true, font.Subset);
            int[][] metrics = new int[longTag.Count][];
            longTag.Values.CopyTo(metrics, 0);
            Array.Sort(metrics, font);
            PdfIndirectReference ind_font = null;
            PdfObject pobj = null;
            PdfIndirectObject obj = null;
            // sivan: cff
            if (font.Cff)
            {
                byte[] b = font.ReadCffFont();
                if (font.Subset || font.SubsetRanges != null)
                {
                    CFFFontSubset cff = new CFFFontSubset(new RandomAccessFileOrArray(b), longTag);
                    try
                    {
                        b = cff.Process(cff.GetNames()[0]);
                    }
                    catch (Exception e)
                    {
                        // temporary fix for cff subset failure
                        font.Subset = false;
                        font.AddRangeUni(longTag, true, font.Subset);
                        metrics = new int[longTag.Count][];
                        longTag.Values.CopyTo(metrics, 0);
                        Array.Sort(metrics, font);
                    }
                }
                pobj = new BaseFont.StreamFont(b, "CIDFontType0C", font.CompressionLevel);
                obj = writer.AddToBody(pobj);
                ind_font = obj.IndirectReference;
            }
            else
            {
                byte[] b;
                if (font.Subset || font.DirectoryOffset != 0)
                {
                    b = font.GetSubSet(new HashSet2<int>(longTag.Keys), true);
                }
                else
                {
                    b = font.GetFullFont();
                }
                int[] lengths = new int[] { b.Length };
                pobj = new BaseFont.StreamFont(b, lengths, font.CompressionLevel);
                obj = writer.AddToBody(pobj);
                ind_font = obj.IndirectReference;
            }
            String subsetPrefix = "";
            if (font.Subset)
                subsetPrefix = TrueTypeFontUnicode.CreateSubsetPrefix();
            PdfDictionary dic = font.GetFontDescriptor(ind_font, subsetPrefix, null);
            obj = writer.AddToBody(dic);
            ind_font = obj.IndirectReference;

            pobj = font.GetCIDFontType2(ind_font, subsetPrefix, metrics);
            obj = writer.AddToBody(pobj);
            ind_font = obj.IndirectReference;

            pobj = font.GetToUnicode(metrics);
            PdfIndirectReference toUnicodeRef = null;

            if (pobj != null)
            {
                obj = writer.AddToBody(pobj);
                toUnicodeRef = obj.IndirectReference;
            }

            pobj = font.GetFontBaseType(ind_font, subsetPrefix, toUnicodeRef);
            writer.AddToBody(pobj, refer);
        }
    }
}
