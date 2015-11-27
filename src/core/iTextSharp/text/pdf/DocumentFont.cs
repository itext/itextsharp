using System;
using System.Collections.Generic;
using iTextSharp.text.pdf.fonts.cmaps;
using iTextSharp.text.io;
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
    public class DocumentFont : BaseFont {
        
        // code, [glyph, width]
        private Dictionary<int, int[]> metrics = new Dictionary<int,int[]>();
        private String fontName;
        private PRIndirectReference refFont;
        private PdfDictionary font;
        private double[] fontMatrix;
        private IntHashtable uni2byte = new IntHashtable();
        private IntHashtable byte2uni = new IntHashtable();
        private IntHashtable diffmap;
        private float Ascender = 800;
        private float CapHeight = 700;
        private float Descender = -200;
        private float ItalicAngle = 0;
        private float fontWeight = 0;
        private float llx = -50;
        private float lly = -200;
        private float urx = 100;
        private float ury = 900;
        protected bool isType0 = false;
        protected int defaultWidth = 1000;
        private IntHashtable hMetrics;
        protected internal String cjkEncoding;
        protected internal String uniMap;
        
        private BaseFont cjkMirror;
        
        private static int[] stdEnc = {
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            32,33,34,35,36,37,38,8217,40,41,42,43,44,45,46,47,
            48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,
            64,65,66,67,68,69,70,71,72,73,74,75,76,77,78,79,
            80,81,82,83,84,85,86,87,88,89,90,91,92,93,94,95,
            8216,97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,
            112,113,114,115,116,117,118,119,120,121,122,123,124,125,126,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,161,162,163,8260,165,402,167,164,39,8220,171,8249,8250,64257,64258,
            0,8211,8224,8225,183,0,182,8226,8218,8222,8221,187,8230,8240,0,191,
            0,96,180,710,732,175,728,729,168,0,730,184,0,733,731,711,
            8212,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
            0,198,0,170,0,0,0,0,321,216,338,186,0,0,0,0,
            0,230,0,0,0,305,0,0,322,248,339,223,0,0,0,0};


        /** Creates a new instance of DocumentFont */
        internal DocumentFont(PdfDictionary font) {
            this.refFont = null;
            this.font = font;
            Init();
        }

        /** Creates a new instance of DocumentFont */
        internal DocumentFont(PRIndirectReference refFont) {
            this.refFont = refFont;
            font = (PdfDictionary)PdfReader.GetPdfObject(refFont);
            Init();
        }

        /** Creates a new instance of DocumentFont */
        internal DocumentFont(PRIndirectReference refFont, PdfDictionary drEncoding) {
        this.refFont = refFont;
        font = (PdfDictionary) PdfReader.GetPdfObject(refFont);
        if (font.Get(PdfName.ENCODING) == null
                && drEncoding != null) {
            foreach (PdfName key in drEncoding.Keys) {
                font.Put(PdfName.ENCODING, drEncoding.Get(key));
            }
        }

        Init();
    }

        private void Init() {
            encoding = "";
            fontSpecific = false;
            fontType = FONT_TYPE_DOCUMENT;
            PdfName baseFont = font.GetAsName(PdfName.BASEFONT);
            fontName = baseFont != null ? PdfName.DecodeName(baseFont.ToString()) : "Unspecified Font Name";
            PdfName subType = font.GetAsName(PdfName.SUBTYPE);
            if (PdfName.TYPE1.Equals(subType) || PdfName.TRUETYPE.Equals(subType))
                DoType1TT();
            else if (PdfName.TYPE3.Equals(subType)) {
                // In case of a Type3 font, we just show the characters as is.
                // Note that this doesn't always make sense:
                // Type 3 fonts are user defined fonts where arbitrary characters are mapped to custom glyphs
                // For instance: the character a could be mapped to an image of a dog, the character b to an image of a cat
                // When parsing a document that shows a cat and a dog, you shouldn't expect seeing a cat and a dog. Instead you'll get b and a.
                FillEncoding(null);
                FillDiffMap(font.GetAsDict(PdfName.ENCODING), null);
                FillWidths();
            } else {
                PdfName encodingName = font.GetAsName(PdfName.ENCODING);
                if (encodingName != null){
                    String enc = PdfName.DecodeName(encodingName.ToString());
                    String ffontname = CJKFont.GetCompatibleFont(enc);
                    if (ffontname != null) {
                        cjkMirror = BaseFont.CreateFont(ffontname, enc, false);
                        cjkEncoding = enc;
                        uniMap = ((CJKFont)cjkMirror).UniMap;
                    }
                    if(PdfName.TYPE0.Equals(subType)) {
                        isType0 = true;
                        if(!enc.Equals("Identity-H") && cjkMirror != null) {
                            PdfArray df = (PdfArray)PdfReader.GetPdfObjectRelease(font.Get(PdfName.DESCENDANTFONTS));
                            PdfDictionary cidft = (PdfDictionary)PdfReader.GetPdfObjectRelease(df[0]);
                            PdfNumber dwo = (PdfNumber)PdfReader.GetPdfObjectRelease(cidft.Get(PdfName.DW));
                            if(dwo != null)
                                defaultWidth = dwo.IntValue;
                            hMetrics = ReadWidths((PdfArray)PdfReader.GetPdfObjectRelease(cidft.Get(PdfName.W)));

                            PdfDictionary fontDesc = (PdfDictionary)PdfReader.GetPdfObjectRelease(cidft.Get(PdfName.FONTDESCRIPTOR));
                            FillFontDesc(fontDesc);
                        }
                        else {
                            ProcessType0(font);
                        }
                    }
                }
            }
        }

        virtual public PdfDictionary FontDictionary {
            get { return font; }        
        }
        
        private void ProcessType0(PdfDictionary font) {
            PdfObject toUniObject = PdfReader.GetPdfObjectRelease(font.Get(PdfName.TOUNICODE));
            PdfArray df = (PdfArray)PdfReader.GetPdfObjectRelease(font.Get(PdfName.DESCENDANTFONTS));
            PdfDictionary cidft = (PdfDictionary)PdfReader.GetPdfObjectRelease(df[0]);
            PdfNumber dwo = (PdfNumber)PdfReader.GetPdfObjectRelease(cidft.Get(PdfName.DW));
            int dw = 1000;
            if (dwo != null)
                dw = dwo.IntValue;
            IntHashtable widths = ReadWidths((PdfArray)PdfReader.GetPdfObjectRelease(cidft.Get(PdfName.W)));
            PdfDictionary fontDesc = (PdfDictionary)PdfReader.GetPdfObjectRelease(cidft.Get(PdfName.FONTDESCRIPTOR));
            FillFontDesc(fontDesc);
            if (toUniObject is PRStream){
                FillMetrics(PdfReader.GetStreamBytes((PRStream)toUniObject), widths, dw);
            } else if (new PdfName("Identity-H").Equals(toUniObject)) {
                FillMetricsIdentity(widths, dw);
            }
        }
        
        private IntHashtable ReadWidths(PdfArray ws) {
            IntHashtable hh = new IntHashtable();
            if (ws == null)
                return hh;
            for (int k = 0; k < ws.Size; ++k) {
                int c1 = ((PdfNumber)PdfReader.GetPdfObjectRelease(ws[k])).IntValue;
                PdfObject obj = PdfReader.GetPdfObjectRelease(ws[++k]);
                if (obj.IsArray()) {
                    PdfArray a2 = (PdfArray)obj;
                    for (int j = 0; j < a2.Size; ++j) {
                        int c2 = ((PdfNumber)PdfReader.GetPdfObjectRelease(a2[j])).IntValue;
                        hh[c1++] = c2;
                    }
                }
                else {
                    int c2 = ((PdfNumber)obj).IntValue;
                    int w = ((PdfNumber)PdfReader.GetPdfObjectRelease(ws[++k])).IntValue;
                    for (; c1 <= c2; ++c1)
                        hh[c1] = w;
                }
            }
            return hh;
        }
        
        private String DecodeString(PdfString ps) {
            if (ps.IsHexWriting())
                return PdfEncodings.ConvertToString(ps.GetBytes(), "UnicodeBigUnmarked");
            else
                return ps.ToUnicodeString();
        }

        private void FillMetricsIdentity(IntHashtable widths, int dw) {
            for (int i = 0; i < 65536; i++) {
                int w = dw;
                if (widths.ContainsKey(i))
                    w = widths[i];
                metrics[i] = new int[] { i, w };
            }
        }
        
        private void FillMetrics(byte[] touni, IntHashtable widths, int dw) {
            PdfContentParser ps = new PdfContentParser(new PRTokeniser(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(touni))));
            PdfObject ob = null;
            bool notFound = true;
            int nestLevel = 0;
            int maxExc = 50;
            while ((notFound || nestLevel > 0)) {
                try {
                    ob = ps.ReadPRObject();
                }
                catch {
                    if (--maxExc < 0)
                        break;
                    continue;
                }
                if (ob == null)
                    break;
                if (ob.Type == PdfContentParser.COMMAND_TYPE) {
                    if (ob.ToString().Equals("begin")) {
                        notFound = false;
                        nestLevel++;
                    }
                    else if (ob.ToString().Equals("end")) {
                        nestLevel--;
                    }
                    else if (ob.ToString().Equals("beginbfchar")) {
                        while (true) {
                            PdfObject nx = ps.ReadPRObject();
                            if (nx.ToString().Equals("endbfchar"))
                                break;
                            String cid = DecodeString((PdfString)nx);
                            String uni = DecodeString((PdfString)ps.ReadPRObject());
                            if (uni.Length == 1) {
                                int cidc = (int)cid[0];
                                int unic = (int)uni[uni.Length - 1];
                                int w = dw;
                                if (widths.ContainsKey(cidc))
                                    w = widths[cidc];
                                metrics[unic] = new int[]{cidc, w};
                            }
                        }
                    }
                    else if (ob.ToString().Equals("beginbfrange")) {
                        while (true) {
                            PdfObject nx = ps.ReadPRObject();
                            if (nx.ToString().Equals("endbfrange"))
                                break;
                            String cid1 = DecodeString((PdfString)nx);
                            String cid2 = DecodeString((PdfString)ps.ReadPRObject());
                            int cid1c = (int)cid1[0];
                            int cid2c = (int)cid2[0];
                            PdfObject ob2 = ps.ReadPRObject();
                            if (ob2.IsString()) {
                                String uni = DecodeString((PdfString)ob2);
                                if (uni.Length == 1) {
                                    int unic = (int)uni[uni.Length - 1];
                                    for (; cid1c <= cid2c; cid1c++, unic++) {
                                        int w = dw;
                                        if (widths.ContainsKey(cid1c))
                                            w = widths[cid1c];
                                        metrics[unic] = new int[]{cid1c, w};
                                    }
                                }
                            }
                            else {
                                PdfArray a = (PdfArray)ob2;
                                for (int j = 0; j < a.Size; ++j, ++cid1c) {
                                    String uni = DecodeString(a.GetAsString(j));
                                    if (uni.Length == 1) {
                                        int unic = (int)uni[uni.Length - 1];
                                        int w = dw;
                                        if (widths.ContainsKey(cid1c))
                                            w = widths[cid1c];
                                        metrics[unic] = new int[]{cid1c, w};
                                    }
                                }
                            }
                        }                        
                    }
                }
            }
        }

        private void DoType1TT() {
            CMapToUnicode toUnicode = null;
            PdfObject enc = PdfReader.GetPdfObject(font.Get(PdfName.ENCODING));
            if (enc == null) {
                PdfName baseFont = font.GetAsName(PdfName.BASEFONT);
                if (BuiltinFonts14.ContainsKey(fontName)
                    && (PdfName.SYMBOL.Equals(baseFont) || PdfName.ZAPFDINGBATS.Equals(baseFont))) {
                    FillEncoding(baseFont);
                } else
                    FillEncoding(null);
                toUnicode = ProcessToUnicode();
                if (toUnicode != null) {
                    IDictionary<int, int> rm = toUnicode.CreateReverseMapping();
                    foreach (KeyValuePair<int, int> kv in rm) {
                        uni2byte[kv.Key] = kv.Value;
                        byte2uni[kv.Value] = kv.Key;
                    }
                }
            } else {
                if (enc.IsName())
                    FillEncoding((PdfName) enc);
                else if (enc.IsDictionary()) {
                    PdfDictionary encDic = (PdfDictionary) enc;
                    enc = PdfReader.GetPdfObject(encDic.Get(PdfName.BASEENCODING));
                    if (enc == null)
                        FillEncoding(null);
                    else
                        FillEncoding((PdfName) enc);
                    FillDiffMap(encDic, toUnicode);
                }
            }
            if (BuiltinFonts14.ContainsKey(fontName)) {
                BaseFont bf = BaseFont.CreateFont(fontName, WINANSI, false);
                int[] e = uni2byte.ToOrderedKeys();
                for (int k = 0; k < e.Length; ++k) {
                    int n = uni2byte[e[k]];
                    widths[n] = bf.GetRawWidth(n, GlyphList.UnicodeToName(e[k]));
                }
                if (diffmap != null) {
                    //widths for differences must override existing ones
                    e = diffmap.ToOrderedKeys();
                    for (int k = 0; k < e.Length; ++k) {
                        int n = diffmap[e[k]];
                        widths[n] = bf.GetRawWidth(n, GlyphList.UnicodeToName(e[k]));
                    }
                    diffmap = null;
                }
                Ascender = bf.GetFontDescriptor(ASCENT, 1000);
                CapHeight = bf.GetFontDescriptor(CAPHEIGHT, 1000);
                Descender = bf.GetFontDescriptor(DESCENT, 1000);
                ItalicAngle = bf.GetFontDescriptor(ITALICANGLE, 1000);
                fontWeight = bf.GetFontDescriptor(FONT_WEIGHT, 1000);
                llx = bf.GetFontDescriptor(BBOXLLX, 1000);
                lly = bf.GetFontDescriptor(BBOXLLY, 1000);
                urx = bf.GetFontDescriptor(BBOXURX, 1000);
                ury = bf.GetFontDescriptor(BBOXURY, 1000);
            }
            FillWidths();
            FillFontDesc(font.GetAsDict(PdfName.FONTDESCRIPTOR));
        }

        private void FillWidths() {
            PdfArray newWidths = font.GetAsArray(PdfName.WIDTHS);
            PdfNumber first = font.GetAsNumber(PdfName.FIRSTCHAR);
            PdfNumber last = font.GetAsNumber(PdfName.LASTCHAR);
            if (first != null && last != null && newWidths != null) {
                int f = first.IntValue;
                int nSize = f + newWidths.Size;
                if (widths.Length < nSize) {
                    int[] tmp = new int[nSize];
                    System.Array.Copy(widths, 0, tmp, 0, f);
                    widths = tmp;
                }
                for (int k = 0; k < newWidths.Size; ++k) {
                    widths[f + k] = newWidths.GetAsNumber(k).IntValue;
                }
            }
        }

        private void FillDiffMap(PdfDictionary encDic, CMapToUnicode toUnicode) {
            PdfArray diffs = encDic.GetAsArray(PdfName.DIFFERENCES);
            if (diffs != null) {
                diffmap = new IntHashtable();
                int currentNumber = 0;
                for (int k = 0; k < diffs.Size; ++k) {
                    PdfObject obj = diffs[k];
                    if (obj.IsNumber())
                        currentNumber = ((PdfNumber)obj).IntValue;
                    else {
                        int[] c = GlyphList.NameToUnicode(PdfName.DecodeName(((PdfName)obj).ToString()));
                        if (c != null && c.Length > 0) {
                            uni2byte[c[0]] = currentNumber;
                            byte2uni[currentNumber] = c[0];
                            diffmap[c[0]] = currentNumber;
                        }
                        else {
                            if (toUnicode == null) {
                                toUnicode = ProcessToUnicode();
                                if (toUnicode == null) {
                                    toUnicode = new CMapToUnicode();
                                }
                            }
                            string unicode = toUnicode.Lookup(new byte[]{(byte) currentNumber}, 0, 1);
                            if ((unicode != null) && (unicode.Length == 1)) {
                                this.uni2byte[unicode[0]] = currentNumber;
                                this.byte2uni[currentNumber] = unicode[0];
                                this.diffmap[unicode[0]] = currentNumber;
                            }
                        }
                        ++currentNumber;
                    }
                }
            }
        }
        
        private CMapToUnicode ProcessToUnicode() {
            CMapToUnicode cmapRet = null;
            PdfObject toUni = PdfReader.GetPdfObjectRelease(this.font.Get(PdfName.TOUNICODE));
            if (toUni is PRStream) {
                try {
                    byte[] touni = PdfReader.GetStreamBytes((PRStream)toUni);
                    CidLocationFromByte lb = new CidLocationFromByte(touni);
                    cmapRet = new CMapToUnicode();
                    CMapParserEx.ParseCid("", cmapRet, lb);
                } catch {
                    cmapRet = null;
                }
            }
            return cmapRet;
        }

        private void FillFontDesc(PdfDictionary fontDesc) {
            if (fontDesc == null)
                return;
            PdfNumber v = fontDesc.GetAsNumber(PdfName.ASCENT);
            if (v != null)
                Ascender = v.FloatValue;
            v = fontDesc.GetAsNumber(PdfName.CAPHEIGHT);
            if (v != null)
                CapHeight = v.FloatValue;
            v = fontDesc.GetAsNumber(PdfName.DESCENT);
            if (v != null)
                Descender = v.FloatValue;
            v = fontDesc.GetAsNumber(PdfName.ITALICANGLE);
            if (v != null)
                ItalicAngle = v.FloatValue;
            v = fontDesc.GetAsNumber(PdfName.FONTWEIGHT);
            if (v != null) {
                fontWeight = v.FloatValue;
            }
            PdfArray bbox = fontDesc.GetAsArray(PdfName.FONTBBOX);
            if (bbox != null) {
                llx = bbox.GetAsNumber(0).FloatValue;
                lly = bbox.GetAsNumber(1).FloatValue;
                urx = bbox.GetAsNumber(2).FloatValue;
                ury = bbox.GetAsNumber(3).FloatValue;
                if (llx > urx) {
                    float t = llx;
                    llx = urx;
                    urx = t;
                }
                if (lly > ury) {
                    float t = lly;
                    lly = ury;
                    ury = t;
                }
            }
            float maxAscent = Math.Max(ury, Ascender);
            float minDescent = Math.Min(lly, Descender);
            Ascender = maxAscent * 1000 / (maxAscent - minDescent);
            Descender = minDescent * 1000 / (maxAscent - minDescent);
        }
        
        private void FillEncoding(PdfName encoding) {
            if (encoding == null && IsSymbolic()) {
                for (int k = 0; k < 256; ++k) {
                    uni2byte[k] = k;
                    byte2uni[k] = k;
                }
            } else if (PdfName.MAC_ROMAN_ENCODING.Equals(encoding) || PdfName.WIN_ANSI_ENCODING.Equals(encoding)
                       || PdfName.SYMBOL.Equals(encoding) || PdfName.ZAPFDINGBATS.Equals(encoding)) {
                byte[] b = new byte[256];
                for (int k = 0; k < 256; ++k)
                    b[k] = (byte) k;
                String enc = WINANSI;
                if (PdfName.MAC_ROMAN_ENCODING.Equals(encoding))
                    enc = MACROMAN;
                else if (PdfName.SYMBOL.Equals(encoding))
                    enc = SYMBOL;
                else if (PdfName.ZAPFDINGBATS.Equals(encoding))
                    enc = ZAPFDINGBATS;
                String cv = PdfEncodings.ConvertToString(b, enc);
                char[] arr = cv.ToCharArray();
                for (int k = 0; k < 256; ++k) {
                    uni2byte[arr[k]] = k;
                    byte2uni[k] = arr[k];
                }
                this.encoding = enc;
            }
            else {
                for (int k = 0; k < 256; ++k) {
                    uni2byte[stdEnc[k]] = k;
                    byte2uni[k] = stdEnc[k];
                }
            }
        }
        
        /** Gets the family name of the font. If it is a True Type font
        * each array element will have {Platform ID, Platform Encoding ID,
        * Language ID, font name}. The interpretation of this values can be
        * found in the Open Type specification, chapter 2, in the 'name' table.<br>
        * For the other fonts the array has a single element with {"", "", "",
        * font name}.
        * @return the family name of the font
        *
        */
        public override string[][] FamilyFontName {
            get {
                return FullFontName;
            }
        }
        
        /** Gets the font parameter identified by <CODE>key</CODE>. Valid values
        * for <CODE>key</CODE> are <CODE>ASCENT</CODE>, <CODE>CAPHEIGHT</CODE>, <CODE>DESCENT</CODE>,
        * <CODE>ITALICANGLE</CODE>, <CODE>BBOXLLX</CODE>, <CODE>BBOXLLY</CODE>, <CODE>BBOXURX</CODE>
        * and <CODE>BBOXURY</CODE>.
        * @param key the parameter to be extracted
        * @param fontSize the font size in points
        * @return the parameter in points
        *
        */
        public override float GetFontDescriptor(int key, float fontSize) {
            if (cjkMirror != null)
                return cjkMirror.GetFontDescriptor(key, fontSize);
            switch (key) {
                case AWT_ASCENT:
                case ASCENT:
                    return Ascender * fontSize / 1000;
                case CAPHEIGHT:
                    return CapHeight * fontSize / 1000;
                case AWT_DESCENT:
                case DESCENT:
                    return Descender * fontSize / 1000;
                case ITALICANGLE:
                    return ItalicAngle;
                case BBOXLLX:
                    return llx * fontSize / 1000;
                case BBOXLLY:
                    return lly * fontSize / 1000;
                case BBOXURX:
                    return urx * fontSize / 1000;
                case BBOXURY:
                    return ury * fontSize / 1000;
                case AWT_LEADING:
                    return 0;
                case AWT_MAXADVANCE:
                    return (urx - llx) * fontSize / 1000;
                case FONT_WEIGHT:
                    return fontWeight * fontSize / 1000;
            }
            return 0;
        }
        
        /** Gets the full name of the font. If it is a True Type font
        * each array element will have {Platform ID, Platform Encoding ID,
        * Language ID, font name}. The interpretation of this values can be
        * found in the Open Type specification, chapter 2, in the 'name' table.<br>
        * For the other fonts the array has a single element with {"", "", "",
        * font name}.
        * @return the full name of the font
        *
        */
        public override string[][] FullFontName {
            get {
                return new string[][]{new string[]{"", "", "", fontName}};
            }
        }
        
        /** Gets all the entries of the names-table. If it is a True Type font
        * each array element will have {Name ID, Platform ID, Platform Encoding ID,
        * Language ID, font name}. The interpretation of this values can be
        * found in the Open Type specification, chapter 2, in the 'name' table.<br>
        * For the other fonts the array has a single element with {"4", "", "", "",
        * font name}.
        * @return the full name of the font
        */
        public override string[][] AllNameEntries {
            get {
                return new string[][]{new string[]{"4", "", "", "", fontName}};
            }
        }

        /** Gets the kerning between two Unicode chars.
        * @param char1 the first char
        * @param char2 the second char
        * @return the kerning to be applied
        *
        */
        public override int GetKerning(int char1, int char2) {
            return 0;
        }
        
        /** Gets the postscript font name.
        * @return the postscript font name
        *
        */
        public override string PostscriptFontName {
            get {
                return fontName;
            }
            set {
            }
        }
        
        /** Gets the width from the font according to the Unicode char <CODE>c</CODE>
        * or the <CODE>name</CODE>. If the <CODE>name</CODE> is null it's a symbolic font.
        * @param c the unicode char
        * @param name the glyph name
        * @return the width of the char
        *
        */
        internal override int GetRawWidth(int c, String name) {
            return 0;
        }
        
        /** Checks if the font has any kerning pairs.
        * @return <CODE>true</CODE> if the font has any kerning pairs
        *
        */
        public override bool HasKernPairs() {
            return false;
        }
        
        /** Outputs to the writer the font dictionaries and streams.
        * @param writer the writer for this document
        * @param ref the font indirect reference
        * @param params several parameters that depend on the font type
        * @throws IOException on error
        * @throws DocumentException error in generating the object
        *
        */
        internal override void WriteFont(PdfWriter writer, PdfIndirectReference refi, Object[] param) {
        }

        /**
        * Always returns null.
        * @return  null
        * @since   2.1.3
        */
        public override PdfStream GetFullFontStream() {
            return null;
        }

        /**
        * Gets the width of a <CODE>char</CODE> in normalized 1000 units.
        * @param char1 the unicode <CODE>char</CODE> to get the width of
        * @return the width in normalized 1000 units
        */
        public override int GetWidth(int char1) {
            if(isType0) {
                if(hMetrics != null && cjkMirror != null && !cjkMirror.IsVertical()) {
                    int c = cjkMirror.GetCidCode(char1);
                    int v = hMetrics[c];
                    if(v > 0)
                        return v;
                    else
                        return defaultWidth;
                }
                else {
                    int[] ws = null;
                    metrics.TryGetValue(char1, out ws);
                    if(ws != null)
                        return ws[1];
                    else
                        return 0;
                }
            }
            if(cjkMirror != null)
                return cjkMirror.GetWidth(char1);
            return base.GetWidth(char1);
        }
        
        public override int GetWidth(String text) {
            if(isType0) {
                int total = 0;
                if(hMetrics != null && cjkMirror != null && !cjkMirror.IsVertical()) {
                    if(((CJKFont)cjkMirror).IsIdentity()) {
                        for(int k = 0; k < text.Length; ++k) {
                            total += GetWidth(text[k]);
                        }
                    }
                    else {
                        for(int k = 0; k < text.Length; ++k) {
                            int val;
                            if(Utilities.IsSurrogatePair(text, k)) {
                                val = Utilities.ConvertToUtf32(text, k);
                                k++;
                            }
                            else {
                                val = text[k];
                            }
                            total += GetWidth(val);
                        }
                    }
                }
                else {
                    char[] chars = text.ToCharArray();
                    int len = chars.Length;
                    for(int k = 0; k < len; ++k) {
                        int[] ws = null;
                        metrics.TryGetValue(chars[k], out ws);
                        if(ws != null)
                            total += ws[1];
                    }
                }
                return total;
            }
            if(cjkMirror != null)
                return cjkMirror.GetWidth(text);
            return base.GetWidth(text);
        }
        
        public override byte[] ConvertToBytes(String text) {
            if (cjkMirror != null)
                return cjkMirror.ConvertToBytes(text);
            else if (isType0) {
                char[] chars = text.ToCharArray();
                int len = chars.Length;
                byte[] b = new byte[len * 2];
                int bptr = 0;
                for (int k = 0; k < len; ++k) {
                    int[] ws;
                    metrics.TryGetValue((int)chars[k], out ws);
                    if (ws != null) {
                        int g = ws[0];
                        b[bptr++] = (byte)(g / 256);
                        b[bptr++] = (byte)g;
                    }
                }
                if (bptr == b.Length)
                    return b;
                else {
                    byte[] nb = new byte[bptr];
                    System.Array.Copy(b, 0, nb, 0, bptr);
                    return nb;
                }
            }
            else {
                char[] cc = text.ToCharArray();
                byte[] b = new byte[cc.Length];
                int ptr = 0;
                for (int k = 0; k < cc.Length; ++k) {
                    if (uni2byte.ContainsKey(cc[k]))
                        b[ptr++] = (byte)uni2byte[cc[k]];
                }
                if (ptr == b.Length)
                    return b;
                else {
                    byte[] b2 = new byte[ptr];
                    System.Array.Copy(b, 0, b2, 0, ptr);
                    return b2;
                }
            }
        }
        
        internal override byte[] ConvertToBytes(int char1) {
            if (cjkMirror != null)
                return cjkMirror.ConvertToBytes(char1);
            else if (isType0) {
                int[] ws;
                metrics.TryGetValue((int)char1, out ws);
                if (ws != null) {
                    int g = ws[0];
                    return new byte[]{(byte)(g / 256), (byte)g};
                }
                else
                    return new byte[0];
            }
            else {
                if (uni2byte.ContainsKey(char1))
                    return new byte[]{(byte)uni2byte[char1]};
                else
                    return new byte[0];
            }
        }
        
        internal PdfIndirectReference IndirectReference {
            get {
                if (refFont == null)
                    throw new ArgumentException("Font reuse not allowed with direct font objects.");
                return refFont;
            }
        }
        
        public override bool CharExists(int c) {
            if (cjkMirror != null)
                return cjkMirror.CharExists(c);
            else if (isType0) {
                return metrics.ContainsKey((int)c);
            }
            else
                return base.CharExists(c);
        }


        public override double[] GetFontMatrix() {
            if (fontMatrix == null) {
                PdfArray array = font.GetAsArray(PdfName.FONTMATRIX);
                fontMatrix = array != null ? array.AsDoubleArray() : DEFAULT_FONT_MATRIX;
            }
            return fontMatrix;
        }

        public override bool SetKerning(int char1, int char2, int kern) {
            return false;
        }
        
        public override int[] GetCharBBox(int c) {
            return null;
        }

        public override bool IsVertical() {
            if (cjkMirror != null)
                return cjkMirror.IsVertical();
            else
                return base.IsVertical();
        }
        
        protected override int[] GetRawCharBBox(int c, String name) {
            return null;
        }    

        /**
        * Exposes the unicode - > CID map that is constructed from the font's encoding
        * @return the unicode to CID map
        * @since 2.1.7
        */
        internal IntHashtable Uni2Byte {
            get {
                return uni2byte;
            }
        }

        /**
         * Exposes the CID - > unicode map that is constructed from the font's encoding
         * @return the CID to unicode map
         * @since 5.4.0
         */
        internal IntHashtable Byte2Uni {
            get {
                return byte2uni;
            }
        }

        /**
         * Gets the difference map
         * @return the difference map
         * @since 5.0.5
         */
        internal IntHashtable Diffmap {
            get {
                return diffmap;
            }
        }

        bool IsSymbolic()
        {
            PdfDictionary fontDescriptor = font.GetAsDict(PdfName.FONTDESCRIPTOR);
            if (fontDescriptor == null)
                return false;
            PdfNumber flags = fontDescriptor.GetAsNumber(PdfName.FLAGS);
            if (flags == null)
                return false;
            return (flags.IntValue & 0x04) != 0;
        }

    }
}
