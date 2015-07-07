using System;
using System.Collections.Generic;
using System.Text;
using System.util;
using System.Xml.Schema;
using iTextSharp.text;

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

    /** Each font in the document will have an instance of this class
     * where the characters used will be represented.
     *
     * @author  Paulo Soares
     */
    public class FontDetails {
    
        /** The indirect reference to this font
         */    
        PdfIndirectReference indirectReference;
        /** The font name that appears in the document body stream
         */    
        PdfName fontName;
        /** The font
         */    
        BaseFont baseFont;
        /** The font if its an instance of <CODE>TrueTypeFontUnicode</CODE>
         */    
        TrueTypeFontUnicode ttu;

        CJKFont cjkFont;
        /** The array used with single byte encodings
         */    
        byte[] shortTag;
        /** The map used with double byte encodings. The key is Int(glyph) and the
         * value is int[]{glyph, width, Unicode code}
         */    
        Dictionary<int, int[]> longTag;
    
        IntHashtable cjkTag;
        /** The font type
         */    
        int fontType;
        /** <CODE>true</CODE> if the font is symbolic
         */    
        bool symbolic;
        /** Indicates if all the glyphs and widths for that particular
         * encoding should be included in the document.
         */
        protected bool subset = true;
        /** Each font used in a document has an instance of this class.
         * This class stores the characters used in the document and other
         * specifics unique to the current working document.
         * @param fontName the font name
         * @param indirectReference the indirect reference to the font
         * @param baseFont the <CODE>BaseFont</CODE>
         */
        internal FontDetails(PdfName fontName, PdfIndirectReference indirectReference, BaseFont baseFont) {
            this.fontName = fontName;
            this.indirectReference = indirectReference;
            this.baseFont = baseFont;
            fontType = baseFont.FontType;
            switch (fontType) {
                case BaseFont.FONT_TYPE_T1:
                case BaseFont.FONT_TYPE_TT:
                    shortTag = new byte[256];
                    break;
                case BaseFont.FONT_TYPE_CJK:
                    cjkTag = new IntHashtable();
                    cjkFont = (CJKFont)baseFont;
                    break;
                case BaseFont.FONT_TYPE_TTUNI:
                    longTag = new Dictionary<int,int[]>();
                    ttu = (TrueTypeFontUnicode)baseFont;
                    symbolic = baseFont.IsFontSpecific();
                    break;
            }
        }
    
        /** Gets the indirect reference to this font.
         * @return the indirect reference to this font
         */    
        internal PdfIndirectReference IndirectReference {
            get {
                return indirectReference;
            }
        }
    
        /** Gets the font name as it appears in the document body.
         * @return the font name
         */    
        internal PdfName FontName {
            get {
                return fontName;
            }
        }
    
        /** Gets the <CODE>BaseFont</CODE> of this font.
         * @return the <CODE>BaseFont</CODE> of this font
         */    
        internal BaseFont BaseFont {
            get {
                return baseFont;
            }
        }

        internal virtual Object[] ConvertToBytesGid(String gids) {
            if (fontType != BaseFont.FONT_TYPE_TTUNI)
                throw new ArgumentException("GID require TT Unicode");
            try {
                StringBuilder sb = new StringBuilder();
                int totalWidth = 0;
                foreach (char gid in gids.ToCharArray()) {
                    int width = ttu.GetGlyphWidth(gid);
                    totalWidth += width;
                    int vchar = ttu.GetCharFromGlyphId(gid);
                    if (vchar != 0) {
                        sb.Append(Utilities.ConvertFromUtf32(vchar));
                    }
                    int gl = gid;
                    if (!longTag.ContainsKey(gl))
                        longTag[gl] = new int[] {gid, width, vchar};
                }
                return new Object[] {Encoding.GetEncoding(CJKFont.CJK_ENCODING).GetBytes(gids), sb.ToString(), totalWidth};
            } catch (Exception e) {
                throw;
            }
        }
    
        /** Converts the text into bytes to be placed in the document.
         * The conversion is done according to the font and the encoding and the characters
         * used are stored.
         * @param text the text to convert
         * @return the conversion
         */    
        internal byte[] ConvertToBytes(string text) {
            byte[] b = null;
            switch (fontType) {
                case BaseFont.FONT_TYPE_T3:
                    return baseFont.ConvertToBytes(text);
                case BaseFont.FONT_TYPE_T1:
                case BaseFont.FONT_TYPE_TT: {
                    b = baseFont.ConvertToBytes(text);
                    int len = b.Length;
                    for (int k = 0; k < len; ++k)
                        shortTag[((int)b[k]) & 0xff] = 1;
                    break;
                }
                case BaseFont.FONT_TYPE_CJK: {
                    int len = text.Length;
                    if (cjkFont.IsIdentity()) {
                        foreach (char c in text) {
                            cjkTag[c] = 0;
                        }
                    }
                    else {
                        for (int k = 0; k < len; ++k) {
                            int val;
                            if (Utilities.IsSurrogatePair(text, k)) {
                                val = Utilities.ConvertToUtf32(text, k);
                                k++;
                            }
                            else {
                                val = text[k];
                            }
                            cjkTag[cjkFont.GetCidCode(val)] = 0;
                        }
                    }
                    b = cjkFont.ConvertToBytes(text);
                    break;
                }
                case BaseFont.FONT_TYPE_DOCUMENT: {
                    b = baseFont.ConvertToBytes(text);
                    break;
                }
                case BaseFont.FONT_TYPE_TTUNI: {
                    int len = text.Length;
                    int[] metrics = null;
                    char[] glyph = new char[len];
                    int i = 0;
                    if (symbolic) {
                        b = PdfEncodings.ConvertToBytes(text, "symboltt");
                        len = b.Length;
                        for (int k = 0; k < len; ++k) {
                            metrics = ttu.GetMetricsTT(b[k] & 0xff);
                            if (metrics == null)
                                continue;
                            longTag[metrics[0]] = new int[]{metrics[0], metrics[1], ttu.GetUnicodeDifferences(b[k] & 0xff)};
                            glyph[i++] = (char)metrics[0];
                        }
                    }
                    else {
                        for (int k = 0; k < len; ++k) {
                            int val;
                            if (Utilities.IsSurrogatePair(text, k)) {
                                val = Utilities.ConvertToUtf32(text, k);
                                k++;
                            }
                            else {
                                val = (int)text[k];
                            }
                            metrics = ttu.GetMetricsTT(val);
                            if (metrics == null)
                                continue;
                            int m0 = metrics[0];
                            int gl = m0;
                            if (!longTag.ContainsKey(gl))
                                longTag[gl] = new int[]{m0, metrics[1], val};
                            glyph[i++] = (char)m0;
                        }
                    }
                    char[] copyGlyph = new char[i];
                    Array.Copy(glyph, copyGlyph, i);
                    b = StringUtils.ConvertCharsToBytes(copyGlyph);
                    break;
                }
            }
            return b;
        }
    
        /** Writes the font definition to the document.
         * @param writer the <CODE>PdfWriter</CODE> of this document
         */    
        virtual public void WriteFont(PdfWriter writer) {
            switch (fontType) {
                case BaseFont.FONT_TYPE_T3:
                    baseFont.WriteFont(writer, indirectReference, null);
                    break;
                case BaseFont.FONT_TYPE_T1:
                case BaseFont.FONT_TYPE_TT: {
                    int firstChar;
                    int lastChar;
                    for (firstChar = 0; firstChar < 256; ++firstChar) {
                        if (shortTag[firstChar] != 0)
                            break;
                    }
                    for (lastChar = 255; lastChar >= firstChar; --lastChar) {
                        if (shortTag[lastChar] != 0)
                            break;
                    }
                    if (firstChar > 255) {
                        firstChar = 255;
                        lastChar = 255;
                    }
                    baseFont.WriteFont(writer, indirectReference, new Object[]{firstChar, lastChar, shortTag, subset});
                    break;
                }
                case BaseFont.FONT_TYPE_CJK:
                    baseFont.WriteFont(writer, indirectReference, new Object[]{cjkTag});
                    break;
                case BaseFont.FONT_TYPE_TTUNI:
                    baseFont.WriteFont(writer, indirectReference, new Object[]{longTag, subset});
                    break;
            }
        }
    
        /** Indicates if all the glyphs and widths for that particular
         * encoding should be included in the document. Set to <CODE>false</CODE>
         * to include all.
         * @param subset new value of property subset
         */
        virtual public bool Subset {
            set {
                this.subset = value;
            }
            get {
                return subset;
            }
        }
    }
}
