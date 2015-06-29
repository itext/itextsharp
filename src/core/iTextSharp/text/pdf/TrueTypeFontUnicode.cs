using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using iTextSharp.text.error_messages;

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

    /** Represents a True Type font with Unicode encoding. All the character
     * in the font can be used directly by using the encoding Identity-H or
     * Identity-V. This is the only way to represent some character sets such
     * as Thai.
     * @author  Paulo Soares
     */
    public class TrueTypeFontUnicode : TrueTypeFont, IComparer<int[]> {
    
        /** Creates a new TrueType font addressed by Unicode characters. The font
         * will always be embedded.
         * @param ttFile the location of the font on file. The file must end in '.ttf'.
         * The modifiers after the name are ignored.
         * @param enc the encoding to be applied to this font
         * @param emb true if the font is to be embedded in the PDF
         * @param ttfAfm the font as a <CODE>byte</CODE> array
         * @throws DocumentException the font is invalid
         * @throws IOException the font file could not be read
         */
        internal TrueTypeFontUnicode(string ttFile, string enc, bool emb, byte[] ttfAfm, bool forceRead) {
            string nameBase = GetBaseName(ttFile);
            string ttcName = GetTTCName(nameBase);
            if (nameBase.Length < ttFile.Length) {
                style = ttFile.Substring(nameBase.Length);
            }
            encoding = enc;
            embedded = emb;
            fileName = ttcName;
            ttcIndex = "";
            if (ttcName.Length < nameBase.Length)
                ttcIndex = nameBase.Substring(ttcName.Length + 1);
            FontType = FONT_TYPE_TTUNI;
            if ((fileName.ToLower(System.Globalization.CultureInfo.InvariantCulture).EndsWith(".ttf") || fileName.ToLower(System.Globalization.CultureInfo.InvariantCulture).EndsWith(".otf") || fileName.ToLower(System.Globalization.CultureInfo.InvariantCulture).EndsWith(".ttc")) && ((enc.Equals(IDENTITY_H) || enc.Equals(IDENTITY_V)) && emb)) {
                Process(ttfAfm, forceRead);
                if (os_2.fsType == 2)
                    throw new DocumentException(MessageLocalization.GetComposedMessage("1.cannot.be.embedded.due.to.licensing.restrictions", fileName + style));
                // Sivan
                if ((cmap31 == null && !fontSpecific) || (cmap10 == null && fontSpecific))
                    directTextToByte=true;
                    //throw new DocumentException(MessageLocalization.GetComposedMessage("1.2.does.not.contain.an.usable.cmap", fileName, style));
                if (fontSpecific) {
                    fontSpecific = false;
                    String tempEncoding = encoding;
                    encoding = "";
                    CreateEncoding();
                    encoding = tempEncoding;
                    fontSpecific = true;
                }
            }
            else
                throw new DocumentException(MessageLocalization.GetComposedMessage("1.2.is.not.a.ttf.font.file", fileName, style));
            vertical = enc.EndsWith("V");
        }
    
        /**
        * Gets the width of a <CODE>char</CODE> in normalized 1000 units.
        * @param char1 the unicode <CODE>char</CODE> to get the width of
        * @return the width in normalized 1000 units
        */
        public override int GetWidth(int char1) {
            if (vertical)
                return 1000;
            if (fontSpecific) {
                if ((char1 & 0xff00) == 0 || (char1 & 0xff00) == 0xf000)
                    return GetRawWidth(char1 & 0xff, null);
                else
                    return 0;
            }
            else {
                return GetRawWidth(char1, encoding);
            }
        }
        
        /**
         * Gets the width of a <CODE>string</CODE> in normalized 1000 units.
         * @param text the <CODE>string</CODE> to get the witdth of
         * @return the width in normalized 1000 units
         */
        public override int GetWidth(string text) {
            if (vertical)
                return text.Length * 1000;
            int total = 0;
            if (fontSpecific) {
                char[] cc = text.ToCharArray();
                int len = cc.Length;
                for (int k = 0; k < len; ++k) {
                    char c = cc[k];
                    if ((c & 0xff00) == 0 || (c & 0xff00) == 0xf000)
                        total += GetRawWidth(c & 0xff, null);
                }
            }
            else {
                int len = text.Length;
                for (int k = 0; k < len; ++k) {
                    if (Utilities.IsSurrogatePair(text, k)) {
                        total += GetRawWidth(Utilities.ConvertToUtf32(text, k), encoding);
                        ++k;
                    }
                    else
                        total += GetRawWidth(text[k], encoding);
                }
            }
            return total;
        }

        /** Creates a ToUnicode CMap to allow copy and paste from Acrobat.
         * @param metrics metrics[0] contains the glyph index and metrics[2]
         * contains the Unicode code
         * @throws DocumentException on error
         * @return the stream representing this CMap or <CODE>null</CODE>
         */    
        virtual public PdfStream GetToUnicode(Object[] metrics) {
            if (metrics.Length == 0)
                return null;
            StringBuilder buf = new StringBuilder(
                "/CIDInit /ProcSet findresource begin\n" +
                "12 dict begin\n" +
                "begincmap\n" +
                "/CIDSystemInfo\n" +
                "<< /Registry (TTX+0)\n" +
                "/Ordering (T42UV)\n" +
                "/Supplement 0\n" +
                ">> def\n" +
                "/CMapName /TTX+0 def\n" +
                "/CMapType 2 def\n" +
                "1 begincodespacerange\n" +
                "<0000><FFFF>\n" +
                "endcodespacerange\n");
            int size = 0;
            for (int k = 0; k < metrics.Length; ++k) {
                if (size == 0) {
                    if (k != 0) {
                        buf.Append("endbfrange\n");
                    }
                    size = Math.Min(100, metrics.Length - k);
                    buf.Append(size).Append(" beginbfrange\n");
                }
                --size;
                int[] metric = (int[])metrics[k];
                string fromTo = ToHex(metric[0]);
                buf.Append(fromTo).Append(fromTo).Append(ToHex(metric[2])).Append('\n');
            }
            buf.Append(
                "endbfrange\n" +
                "endcmap\n" +
                "CMapName currentdict /CMap defineresource pop\n" +
                "end end\n");
            string s = buf.ToString();
            PdfStream stream = new PdfStream(PdfEncodings.ConvertToBytes(s, null));
            stream.FlateCompress(compressionLevel);
            return stream;
        }
    
        /** Gets an hex string in the format "&lt;HHHH&gt;".
         * @param n the number
         * @return the hex string
         */    
        internal static string ToHex(int n) {
            if (n < 0x10000)
                return "<" + System.Convert.ToString(n, 16).PadLeft(4, '0') + ">";
            n -= 0x10000;
            int high = (n / 0x400) + 0xd800;
            int low = (n % 0x400) + 0xdc00;
            return "[<" + System.Convert.ToString(high, 16).PadLeft(4, '0') + System.Convert.ToString(low, 16).PadLeft(4, '0') + ">]";
        }
    
        /** Generates the CIDFontTyte2 dictionary.
         * @param fontDescriptor the indirect reference to the font descriptor
         * @param subsetPrefix the subset prefix
         * @param metrics the horizontal width metrics
         * @return a stream
         */    
        virtual public PdfDictionary GetCIDFontType2(PdfIndirectReference fontDescriptor, string subsetPrefix, Object[] metrics) {
            PdfDictionary dic = new PdfDictionary(PdfName.FONT);
            // sivan; cff
            if (cff) {
                dic.Put(PdfName.SUBTYPE, PdfName.CIDFONTTYPE0);
                dic.Put(PdfName.BASEFONT, new PdfName(subsetPrefix + fontName+"-"+encoding));
            }
            else {
                dic.Put(PdfName.SUBTYPE, PdfName.CIDFONTTYPE2);
                dic.Put(PdfName.BASEFONT, new PdfName(subsetPrefix + fontName));
            }
            dic.Put(PdfName.FONTDESCRIPTOR, fontDescriptor);
            if (!cff)
                dic.Put(PdfName.CIDTOGIDMAP,PdfName.IDENTITY);
            PdfDictionary cdic = new PdfDictionary();
            cdic.Put(PdfName.REGISTRY, new PdfString("Adobe"));
            cdic.Put(PdfName.ORDERING, new PdfString("Identity"));
            cdic.Put(PdfName.SUPPLEMENT, new PdfNumber(0));
            dic.Put(PdfName.CIDSYSTEMINFO, cdic);
            if (!vertical) {
                dic.Put(PdfName.DW, new PdfNumber(1000));
                StringBuilder buf = new StringBuilder("[");
                int lastNumber = -10;
                bool firstTime = true;
                for (int k = 0; k < metrics.Length; ++k) {
                    int[] metric = (int[])metrics[k];
                    if (metric[1] == 1000)
                        continue;
                    int m = metric[0];
                    if (m == lastNumber + 1) {
                        buf.Append(' ').Append(metric[1]);
                    }
                    else {
                        if (!firstTime) {
                            buf.Append(']');
                        }
                        firstTime = false;
                        buf.Append(m).Append('[').Append(metric[1]);
                    }
                    lastNumber = m;
                }
                if (buf.Length > 1) {
                    buf.Append("]]");
                    dic.Put(PdfName.W, new PdfLiteral(buf.ToString()));
                }
            }
            return dic;
        }
    
        /** Generates the font dictionary.
         * @param descendant the descendant dictionary
         * @param subsetPrefix the subset prefix
         * @param toUnicode the ToUnicode stream
         * @return the stream
         */    
        virtual public PdfDictionary GetFontBaseType(PdfIndirectReference descendant, string subsetPrefix, PdfIndirectReference toUnicode) {
            PdfDictionary dic = new PdfDictionary(PdfName.FONT);

            dic.Put(PdfName.SUBTYPE, PdfName.TYPE0);
            // The PDF Reference manual advises to add -encoding to CID font names
            if (cff)
                dic.Put(PdfName.BASEFONT, new PdfName(subsetPrefix + fontName+"-"+encoding));
            else
                dic.Put(PdfName.BASEFONT, new PdfName(subsetPrefix + fontName));
            dic.Put(PdfName.ENCODING, new PdfName(encoding));
            dic.Put(PdfName.DESCENDANTFONTS, new PdfArray(descendant));
            if (toUnicode != null)
                dic.Put(PdfName.TOUNICODE, toUnicode);  
            return dic;
        }

        public virtual int GetCharFromGlyphId(int gid) {
            if (glyphIdToChar == null) {
                int[] g2 = new int[maxGlyphId];
                Dictionary<int, int[]> map = null;
                if (cmapExt != null) {
                    map = cmapExt;
                } else if (cmap31 != null) {
                    map = cmap31;
                }
                if (map != null) {
                    foreach (KeyValuePair<int, int[]> entry in map) {
                        g2[entry.Value[0]] = entry.Key;
                    }
                }
                glyphIdToChar = g2;
            }
            return glyphIdToChar[gid];
        }

        /** The method used to sort the metrics array.
         * @param o1 the first element
         * @param o2 the second element
         * @return the comparisation
         */    
        virtual public int Compare(int[] o1, int[] o2) {
            int m1 = o1[0];
            int m2 = o2[0];
            if (m1 < m2)
                return -1;
            if (m1 == m2)
                return 0;
            return 1;
        }

        private static readonly byte[] rotbits = {(byte)0x80,(byte)0x40,(byte)0x20,(byte)0x10,(byte)0x08,(byte)0x04,(byte)0x02,(byte)0x01};

        /** Outputs to the writer the font dictionaries and streams.
         * @param writer the writer for this document
         * @param ref the font indirect reference
         * @param parms several parameters that depend on the font type
         * @throws IOException on error
         * @throws DocumentException error in generating the object
         */
        internal override void WriteFont(PdfWriter writer, PdfIndirectReference piref, Object[] parms) {
            writer.GetTtfUnicodeWriter().WriteFont(this, piref, parms, rotbits);
        }

        /**
        * Returns a PdfStream object with the full font program.
        * @return  a PdfStream with the font program
        * @since   2.1.3
        */
        public override PdfStream GetFullFontStream() {
            if (cff) {
                return new StreamFont(ReadCffFont(), "CIDFontType0C", compressionLevel);
            }
            return base.GetFullFontStream();
        }
        
        /** A forbidden operation. Will throw a null pointer exception.
         * @param text the text
         * @return always <CODE>null</CODE>
         */    
        public override byte[] ConvertToBytes(string text) {
            return null;
        }

        internal override byte[] ConvertToBytes(int char1) {
            return null;
        }

        /** Gets the glyph index and metrics for a character.
         * @param c the character
         * @return an <CODE>int</CODE> array with {glyph index, width}
         */    
        public override int[] GetMetricsTT(int c) {
            int[] ret;
            if (cmapExt != null) {
                cmapExt.TryGetValue(c, out ret);
                return ret;
            }
            Dictionary<int,int[]> map = null;
            if (fontSpecific)
                map = cmap10;
            else
                map = cmap31;
            if (map == null)
                return null;
            if (fontSpecific) {
                if ((c & 0xffffff00) == 0 || (c & 0xffffff00) == 0xf000) {
                    map.TryGetValue(c & 0xff, out ret);
                    return ret;
                }
                else
                    return null;
            }
            else {
                map.TryGetValue(c, out ret);
                return ret;
            }
        }

        /**
        * Checks if a character exists in this font.
        * @param c the character to check
        * @return <CODE>true</CODE> if the character has a glyph,
        * <CODE>false</CODE> otherwise
        */
        public override bool CharExists(int c) {
            return GetMetricsTT(c) != null;
        }
        
        /**
        * Sets the character advance.
        * @param c the character
        * @param advance the character advance normalized to 1000 units
        * @return <CODE>true</CODE> if the advance was set,
        * <CODE>false</CODE> otherwise
        */
        public override bool SetCharAdvance(int c, int advance) {
            int[] m = GetMetricsTT(c);
            if (m == null)
                return false;
            m[1] = advance;
            return true;
        }
        
        public override int[] GetCharBBox(int c) {
            if (bboxes == null)
                return null;
            int[] m = GetMetricsTT(c);
            if (m == null)
                return null;
            return bboxes[m[0]];
        }
    }
}
