using System;
using System.IO;
using System.Collections.Generic;
using System.util;
using iTextSharp.text.error_messages;
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

    /** Reads a Type1 font
     *
     * @author Paulo Soares
     */
    internal class Type1Font : BaseFont {
        private object lockObject = new object();

        /** The PFB file if the input was made with a <CODE>byte</CODE> array.
         */    
        protected byte[] pfb;
        /** The Postscript font name.
         */
        private string FontName;
        /** The full name of the font.
         */
        private string FullName;
        /** The family name of the font.
         */
        private string FamilyName;
        /** The weight of the font: normal, bold, etc.
         */
        private string Weight = "";
        /** The italic angle of the font, usually 0.0 or negative.
         */
        private float ItalicAngle = 0.0f;
        /** <CODE>true</CODE> if all the characters have the same
         *  width.
         */
        private bool IsFixedPitch = false;
        /** The character set of the font.
         */
        private string CharacterSet;
        /** The llx of the FontBox.
         */
        private int llx = -50;
        /** The lly of the FontBox.
         */
        private int lly = -200;
        /** The lurx of the FontBox.
         */
        private int urx = 1000;
        /** The ury of the FontBox.
         */
        private int ury = 900;
        /** The underline position.
         */
        private int UnderlinePosition = -100;
        /** The underline thickness.
         */
        private int UnderlineThickness = 50;
        /** The font's encoding name. This encoding is 'StandardEncoding' or
         *  'AdobeStandardEncoding' for a font that can be totally encoded
         *  according to the characters names. For all other names the
         *  font is treated as symbolic.
         */
        private string EncodingScheme = "FontSpecific";
        /** A variable.
         */
        private int CapHeight = 700;
        /** A variable.
         */
        private int XHeight = 480;
        /** A variable.
         */
        private int Ascender = 800;
        /** A variable.
         */
        private int Descender = -200;
        /** A variable.
         */
        private int StdHW;
        /** A variable.
         */
        private int StdVW = 80;
    
        /** Represents the section CharMetrics in the AFM file. Each
        *  value of this array contains a <CODE>Object[4]</CODE> with an
        *  Integer, Integer, String and int[]. This is the code, width, name and char bbox.
        *  The key is the name of the char and also an Integer with the char number.
        */
        private Dictionary<object,object[]> CharMetrics = new Dictionary<object,object[]>();
        /** Represents the section KernPairs in the AFM file. The key is
         *  the name of the first character and the value is a <CODE>Object[]</CODE>
         *  with 2 elements for each kern pair. Position 0 is the name of
         *  the second character and position 1 is the kerning distance. This is
         *  repeated for all the pairs.
         */
        private Dictionary<string,object[]> KernPairs = new Dictionary<string,object[]>();
        /** The file in use.
         */
        private string fileName;
        /** <CODE>true</CODE> if this font is one of the 14 built in fonts.
         */
        private bool builtinFont = false;
        /** Types of records in a PFB file. ASCII is 1 and BINARY is 2.
         *  They have to appear in the PFB file in this sequence.
         */
        private static readonly int[] PFB_TYPES = {1, 2, 1};
    
        /** Creates a new Type1 font.
         * @param ttfAfm the AFM file if the input is made with a <CODE>byte</CODE> array
         * @param pfb the PFB file if the input is made with a <CODE>byte</CODE> array
         * @param afmFile the name of one of the 14 built-in fonts or the location of an AFM file. The file must end in '.afm'
         * @param enc the encoding to be applied to this font
         * @param emb true if the font is to be embedded in the PDF
         * @throws DocumentException the AFM file is invalid
         * @throws IOException the AFM file could not be read
         */
        internal Type1Font(string afmFile, string enc, bool emb, byte[] ttfAfm, byte[] pfb, bool forceRead) {
            if (emb && ttfAfm != null && pfb == null)
                throw new DocumentException(MessageLocalization.GetComposedMessage("two.byte.arrays.are.needed.if.the.type1.font.is.embedded"));
            if (emb && ttfAfm != null)
                this.pfb = pfb;
            encoding = enc;
            embedded = emb;
            fileName = afmFile;
            FontType = FONT_TYPE_T1;
            RandomAccessFileOrArray rf = null;
            Stream istr = null;
            if (BuiltinFonts14.ContainsKey(afmFile)) {
                embedded = false;
                builtinFont = true;
                byte[] buf = new byte[1024];
                try {
                    istr = StreamUtil.GetResourceStream(RESOURCE_PATH + afmFile + ".afm");
                    if (istr == null) {
                        string msg = MessageLocalization.GetComposedMessage("1.not.found.as.resource", afmFile);
                        Console.Error.WriteLine(msg);
                        throw new DocumentException(msg);
                    }
                    MemoryStream ostr = new MemoryStream();
                    while (true) {
                        int size = istr.Read(buf, 0, buf.Length);
                        if (size == 0)
                            break;
                        ostr.Write(buf, 0, size);
                    }
                    buf = ostr.ToArray();
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
                try {
                    rf = new RandomAccessFileOrArray(buf);
                    Process(rf);
                }
                finally {
                    if (rf != null) {
                        try {
                            rf.Close();
                        }
                        catch {
                            // empty on purpose
                        }
                    }
                }
            }
            else if (afmFile.ToLower(System.Globalization.CultureInfo.InvariantCulture).EndsWith(".afm")) {
                try {
                    if (ttfAfm == null)
                        rf = new RandomAccessFileOrArray(afmFile, forceRead);
                    else
                        rf = new RandomAccessFileOrArray(ttfAfm);
                    Process(rf);
                }
                finally {
                    if (rf != null) {
                        try {
                            rf.Close();
                        }
                        catch {
                            // empty on purpose
                        }
                    }
                }
            }
            else if (afmFile.ToLower(System.Globalization.CultureInfo.InvariantCulture).EndsWith(".pfm")) {
                try {
                    MemoryStream ba = new MemoryStream();
                    if (ttfAfm == null)
                        rf = new RandomAccessFileOrArray(afmFile, forceRead);
                    else
                        rf = new RandomAccessFileOrArray(ttfAfm);
                    Pfm2afm.Convert(rf, ba);
                    rf.Close();
                    rf = new RandomAccessFileOrArray(ba.ToArray());
                    Process(rf);
                }
                finally {
                    if (rf != null) {
                        try {
                            rf.Close();
                        }
                        catch  {
                            // empty on purpose
                        }
                    }
                }
            }
            else
                throw new DocumentException(MessageLocalization.GetComposedMessage("1.is.not.an.afm.or.pfm.font.file", afmFile));
            EncodingScheme = EncodingScheme.Trim();
            if (EncodingScheme.Equals("AdobeStandardEncoding") || EncodingScheme.Equals("StandardEncoding")) {
                fontSpecific = false;
            }
            if (!encoding.StartsWith("#"))
                PdfEncodings.ConvertToBytes(" ", enc); // check if the encoding exists
            CreateEncoding();
        }
    
        /** Gets the width from the font according to the <CODE>name</CODE> or,
         * if the <CODE>name</CODE> is null, meaning it is a symbolic font,
         * the char <CODE>c</CODE>.
         * @param c the char if the font is symbolic
         * @param name the glyph name
         * @return the width of the char
         */
        internal override int GetRawWidth(int c, string name) {
            Object[] metrics;
            if (name == null) { // font specific
                CharMetrics.TryGetValue(c, out metrics);
            }
            else {
                if (name.Equals(".notdef"))
                    return 0;
                CharMetrics.TryGetValue(name, out metrics);
            }
            if (metrics != null)
                return (int)metrics[1];
            return 0;
        }
    
        /** Gets the kerning between two Unicode characters. The characters
         * are converted to names and this names are used to find the kerning
         * pairs in the <CODE>Hashtable</CODE> <CODE>KernPairs</CODE>.
         * @param char1 the first char
         * @param char2 the second char
         * @return the kerning to be applied
         */
        public override int GetKerning(int char1, int char2) {
            string first = GlyphList.UnicodeToName(char1);
            if (first == null)
                return 0;
            string second = GlyphList.UnicodeToName(char2);
            if (second == null)
                return 0;
            Object[] obj;
            KernPairs.TryGetValue(first, out obj);
            if (obj == null)
                return 0;
            for (int k = 0; k < obj.Length; k += 2) {
                if (second.Equals(obj[k]))
                    return (int)obj[k + 1];
            }
            return 0;
        }
    
    
        /** Reads the font metrics
         * @param rf the AFM file
         * @throws DocumentException the AFM file is invalid
         * @throws IOException the AFM file could not be read
         */
        virtual public void Process(RandomAccessFileOrArray rf) {
            string line;
            bool isMetrics = false;
            while ((line = rf.ReadLine()) != null) {
                StringTokenizer tok = new StringTokenizer(line, " ,\n\r\t\f");
                if (!tok.HasMoreTokens())
                    continue;
                string ident = tok.NextToken();
                if (ident.Equals("FontName"))
                    FontName = tok.NextToken("\u00ff").Substring(1);
                else if (ident.Equals("FullName"))
                    FullName = tok.NextToken("\u00ff").Substring(1);
                else if (ident.Equals("FamilyName"))
                    FamilyName = tok.NextToken("\u00ff").Substring(1);
                else if (ident.Equals("Weight"))
                    Weight = tok.NextToken("\u00ff").Substring(1);
                else if (ident.Equals("ItalicAngle"))
                    ItalicAngle = float.Parse(tok.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                else if (ident.Equals("IsFixedPitch"))
                    IsFixedPitch = tok.NextToken().Equals("true");
                else if (ident.Equals("CharacterSet"))
                    CharacterSet = tok.NextToken("\u00ff").Substring(1);
                else if (ident.Equals("FontBBox")) {
                    llx = (int)float.Parse(tok.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                    lly = (int)float.Parse(tok.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                    urx = (int)float.Parse(tok.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                    ury = (int)float.Parse(tok.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                }
                else if (ident.Equals("UnderlinePosition"))
                    UnderlinePosition = (int)float.Parse(tok.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                else if (ident.Equals("UnderlineThickness"))
                    UnderlineThickness = (int)float.Parse(tok.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                else if (ident.Equals("EncodingScheme"))
                    EncodingScheme = tok.NextToken("\u00ff").Substring(1);
                else if (ident.Equals("CapHeight"))
                    CapHeight = (int)float.Parse(tok.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                else if (ident.Equals("XHeight"))
                    XHeight = (int)float.Parse(tok.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                else if (ident.Equals("Ascender"))
                    Ascender = (int)float.Parse(tok.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                else if (ident.Equals("Descender"))
                    Descender = (int)float.Parse(tok.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                else if (ident.Equals("StdHW"))
                    StdHW = (int)float.Parse(tok.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                else if (ident.Equals("StdVW"))
                    StdVW = (int)float.Parse(tok.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                else if (ident.Equals("StartCharMetrics")) {
                    isMetrics = true;
                    break;
                }
            }
            if (!isMetrics)
                throw new DocumentException(MessageLocalization.GetComposedMessage("missing.startcharmetrics.in.1", fileName));
            while ((line = rf.ReadLine()) != null) {
                StringTokenizer tok = new StringTokenizer(line);
                if (!tok.HasMoreTokens())
                    continue;
                string ident = tok.NextToken();
                if (ident.Equals("EndCharMetrics")) {
                    isMetrics = false;
                    break;
                }
                int C = -1;
                int WX = 250;
                string N = "";
                int[] B = null;

                tok = new StringTokenizer(line, ";");
                while (tok.HasMoreTokens())
                {
                    StringTokenizer tokc = new StringTokenizer(tok.NextToken());
                    if (!tokc.HasMoreTokens())
                        continue;
                    ident = tokc.NextToken();
                    if (ident.Equals("C"))
                        C = int.Parse(tokc.NextToken());
                    else if (ident.Equals("WX"))
                        WX = (int)float.Parse(tokc.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                    else if (ident.Equals("N"))
                        N = tokc.NextToken();
                    else if (ident.Equals("B")) {
                        B = new int[]{int.Parse(tokc.NextToken()), 
                                            int.Parse(tokc.NextToken()),
                                            int.Parse(tokc.NextToken()),
                                            int.Parse(tokc.NextToken())};
                    }
                }
                Object[] metrics = new Object[]{C, WX, N, B};
                if (C >= 0)
                    CharMetrics[C] = metrics;
                CharMetrics[N] = metrics;
            }
            if (isMetrics)
                throw new DocumentException(MessageLocalization.GetComposedMessage("missing.endcharmetrics.in.1", fileName));
            if (!CharMetrics.ContainsKey("nonbreakingspace")) {
                Object[] space;
                CharMetrics.TryGetValue("space", out space);
                if (space != null)
                    CharMetrics["nonbreakingspace"] = space;
            }
            while ((line = rf.ReadLine()) != null) {
                StringTokenizer tok = new StringTokenizer(line);
                if (!tok.HasMoreTokens())
                    continue;
                string ident = tok.NextToken();
                if (ident.Equals("EndFontMetrics"))
                    return;
                if (ident.Equals("StartKernPairs")) {
                    isMetrics = true;
                    break;
                }
            }
            if (!isMetrics)
                throw new DocumentException(MessageLocalization.GetComposedMessage("missing.endfontmetrics.in.1", fileName));
            while ((line = rf.ReadLine()) != null) {
                StringTokenizer tok = new StringTokenizer(line);
                if (!tok.HasMoreTokens())
                    continue;
                string ident = tok.NextToken();
                if (ident.Equals("KPX")) {
                    string first = tok.NextToken();
                    string second = tok.NextToken();
                    int width = (int)float.Parse(tok.NextToken(), System.Globalization.NumberFormatInfo.InvariantInfo);
                    Object[] relates;
                    KernPairs.TryGetValue(first, out relates);
                    if (relates == null)
                        KernPairs[first] = new Object[]{second, width};
                    else {
                        int n = relates.Length;
                        Object[] relates2 = new Object[n + 2];
                        Array.Copy(relates, 0, relates2, 0, n);
                        relates2[n] = second;
                        relates2[n + 1] = width;
                        KernPairs[first] = relates2;
                    }
                }
                else if (ident.Equals("EndKernPairs")) {
                    isMetrics = false;
                    break;
                }
            }
            if (isMetrics)
                throw new DocumentException(MessageLocalization.GetComposedMessage("missing.endkernpairs.in.1", fileName));
            rf.Close();
        }
    
        /** If the embedded flag is <CODE>false</CODE> or if the font is
         *  one of the 14 built in types, it returns <CODE>null</CODE>,
         * otherwise the font is read and output in a PdfStream object.
         * @return the PdfStream containing the font or <CODE>null</CODE>
         * @throws DocumentException if there is an error reading the font
         */
        public override PdfStream GetFullFontStream() {
            if (builtinFont || !embedded)
                return null;
            lock (lockObject) {
                RandomAccessFileOrArray rf = null;
                try {
                    string filePfb = fileName.Substring(0, fileName.Length - 3) + "pfb";
                    if (pfb == null)
                        rf = new RandomAccessFileOrArray(filePfb, true);
                    else
                        rf = new RandomAccessFileOrArray(pfb);
                    int fileLength = (int)rf.Length;
                    byte[] st = new byte[fileLength - 18];
                    int[] lengths = new int[3];
                    int bytePtr = 0;
                    for (int k = 0; k < 3; ++k) {
                        if (rf.Read() != 0x80)
                            throw new DocumentException(MessageLocalization.GetComposedMessage("start.marker.missing.in.1", filePfb));
                        if (rf.Read() != PFB_TYPES[k])
                            throw new DocumentException(MessageLocalization.GetComposedMessage("incorrect.segment.type.in.1", filePfb));
                        int size = rf.Read();
                        size += rf.Read() << 8;
                        size += rf.Read() << 16;
                        size += rf.Read() << 24;
                        lengths[k] = size;
                        while (size != 0) {
                            int got = rf.Read(st, bytePtr, size);
                            if (got < 0)
                                throw new DocumentException(MessageLocalization.GetComposedMessage("premature.end.in.1", filePfb));
                            bytePtr += got;
                            size -= got;
                        }
                    }
                    return new StreamFont(st, lengths, compressionLevel);
                }
                finally {
                    if (rf != null) {
                        try {
                            rf.Close();
                        }
                        catch  {
                            // empty on purpose
                        }
                    }
                }
            }
        }
    
        /** Generates the font descriptor for this font or <CODE>null</CODE> if it is
         * one of the 14 built in fonts.
         * @param fontStream the indirect reference to a PdfStream containing the font or <CODE>null</CODE>
         * @return the PdfDictionary containing the font descriptor or <CODE>null</CODE>
         */
        virtual public PdfDictionary GetFontDescriptor(PdfIndirectReference fontStream) {
            if (builtinFont)
                return null;
            PdfDictionary dic = new PdfDictionary(PdfName.FONTDESCRIPTOR);
            dic.Put(PdfName.ASCENT, new PdfNumber(Ascender));
            dic.Put(PdfName.CAPHEIGHT, new PdfNumber(CapHeight));
            dic.Put(PdfName.DESCENT, new PdfNumber(Descender));
            dic.Put(PdfName.FONTBBOX, new PdfRectangle(llx, lly, urx, ury));
            dic.Put(PdfName.FONTNAME, new PdfName(FontName));
            dic.Put(PdfName.ITALICANGLE, new PdfNumber(ItalicAngle));
            dic.Put(PdfName.STEMV, new PdfNumber(StdVW));
            if (fontStream != null)
                dic.Put(PdfName.FONTFILE, fontStream);
            int flags = 0;
            if (IsFixedPitch)
                flags |= 1;
            flags |= fontSpecific ? 4 : 32;
            if (ItalicAngle < 0)
                flags |= 64;
            if (FontName.IndexOf("Caps") >= 0 || FontName.EndsWith("SC"))
                flags |= 131072;
            if (Weight.Equals("Bold"))
                flags |= 262144;
            dic.Put(PdfName.FLAGS, new PdfNumber(flags));
            
            return dic;
        }
    
        /** Generates the font dictionary for this font.
         * @return the PdfDictionary containing the font dictionary
         * @param firstChar the first valid character
         * @param lastChar the last valid character
         * @param shortTag a 256 bytes long <CODE>byte</CODE> array where each unused byte is represented by 0
         * @param fontDescriptor the indirect reference to a PdfDictionary containing the font descriptor or <CODE>null</CODE>
         */
        private PdfDictionary GetFontBaseType(PdfIndirectReference fontDescriptor, int firstChar, int lastChar, byte[] shortTag) {
            PdfDictionary dic = new PdfDictionary(PdfName.FONT);
            dic.Put(PdfName.SUBTYPE, PdfName.TYPE1);
            dic.Put(PdfName.BASEFONT, new PdfName(FontName));
            bool stdEncoding = encoding.Equals(CP1252) || encoding.Equals(MACROMAN);
            if (!fontSpecific || specialMap != null) {
                for (int k = firstChar; k <= lastChar; ++k) {
                    if (!differences[k].Equals(notdef)) {
                        firstChar = k;
                        break;
                    }
                }
                if (stdEncoding)
                    dic.Put(PdfName.ENCODING, encoding.Equals(CP1252) ? PdfName.WIN_ANSI_ENCODING : PdfName.MAC_ROMAN_ENCODING);
                else {
                    PdfDictionary enc = new PdfDictionary(PdfName.ENCODING);
                    PdfArray dif = new PdfArray();
                    bool gap = true;                
                    for (int k = firstChar; k <= lastChar; ++k) {
                        if (shortTag[k] != 0) {
                            if (gap) {
                                dif.Add(new PdfNumber(k));
                                gap = false;
                            }
                            dif.Add(new PdfName(differences[k]));
                        }
                        else
                            gap = true;
                    }
                    enc.Put(PdfName.DIFFERENCES, dif);
                    dic.Put(PdfName.ENCODING, enc);
                }
            }
            if (specialMap != null || forceWidthsOutput || !(builtinFont && (fontSpecific || stdEncoding))) {
                dic.Put(PdfName.FIRSTCHAR, new PdfNumber(firstChar));
                dic.Put(PdfName.LASTCHAR, new PdfNumber(lastChar));
                PdfArray wd = new PdfArray();
                for (int k = firstChar; k <= lastChar; ++k) {
                    if (shortTag[k] == 0)
                        wd.Add(new PdfNumber(0));
                    else
                        wd.Add(new PdfNumber(widths[k]));
                }
                dic.Put(PdfName.WIDTHS, wd);
            }
            if (!builtinFont && fontDescriptor != null)
                dic.Put(PdfName.FONTDESCRIPTOR, fontDescriptor);
            return dic;
        }
    
        /** Outputs to the writer the font dictionaries and streams.
         * @param writer the writer for this document
         * @param ref the font indirect reference
         * @param parms several parameters that depend on the font type
         * @throws IOException on error
         * @throws DocumentException error in generating the object
         */
        internal override void WriteFont(PdfWriter writer, PdfIndirectReference piref, Object[] parms) {
            int firstChar = (int)parms[0];
            int lastChar = (int)parms[1];
            byte[] shortTag = (byte[])parms[2];
            bool subsetp = (bool)parms[3] && subset;
            if (!subsetp) {
                firstChar = 0;
                lastChar = shortTag.Length - 1;
                for (int k = 0; k < shortTag.Length; ++k)
                    shortTag[k] = 1;
            }
            PdfIndirectReference ind_font = null;
            PdfObject pobj = null;
            PdfIndirectObject obj = null;
            pobj = GetFullFontStream();
            if (pobj != null){
                obj = writer.AddToBody(pobj);
                ind_font = obj.IndirectReference;
            }
            pobj = GetFontDescriptor(ind_font);
            if (pobj != null){
                obj = writer.AddToBody(pobj);
                ind_font = obj.IndirectReference;
            }
            pobj = GetFontBaseType(ind_font, firstChar, lastChar, shortTag);
            writer.AddToBody(pobj, piref);
        }
    
        /** Gets the font parameter identified by <CODE>key</CODE>. Valid values
         * for <CODE>key</CODE> are <CODE>ASCENT</CODE>, <CODE>CAPHEIGHT</CODE>, <CODE>DESCENT</CODE>,
         * <CODE>ITALICANGLE</CODE>, <CODE>BBOXLLX</CODE>, <CODE>BBOXLLY</CODE>, <CODE>BBOXURX</CODE>
         * and <CODE>BBOXURY</CODE>.
         * @param key the parameter to be extracted
         * @param fontSize the font size in points
         * @return the parameter in points
         */    
        public override float GetFontDescriptor(int key, float fontSize) {
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
                case UNDERLINE_POSITION:
                    return UnderlinePosition * fontSize / 1000;
                case UNDERLINE_THICKNESS:
                    return UnderlineThickness * fontSize / 1000;
            }
            return 0;
        }

        /** Sets the font parameter identified by <CODE>key</CODE>. Valid values
         * for <CODE>key</CODE> are <CODE>ASCENT</CODE>, <CODE>CAPHEIGHT</CODE>, <CODE>DESCENT</CODE>,
         * <CODE>ITALICANGLE</CODE>, <CODE>BBOXLLX</CODE>, <CODE>BBOXLLY</CODE>, <CODE>BBOXURX</CODE>
         * and <CODE>BBOXURY</CODE>.
         * @param key the parameter to be updated
         * @param value the parameter value
         */
        public override void SetFontDescriptor(int key, float value) {
            switch (key) {
                case AWT_ASCENT:
                case ASCENT:
                    Ascender = (int)value;
                    break;
                case AWT_DESCENT:
                case DESCENT:
                    Descender = (int)value;
                    break;
                default:
                    break;
            }
        }
    
        /** Gets the postscript font name.
         * @return the postscript font name
         */
        public override string PostscriptFontName {
            get {
                return FontName;
            }
            set {
                FontName = value;
            }
        }
    
        /** Gets the full name of the font. If it is a True Type font
         * each array element will have {Platform ID, Platform Encoding ID,
         * Language ID, font name}. The interpretation of this values can be
         * found in the Open Type specification, chapter 2, in the 'name' table.<br>
         * For the other fonts the array has a single element with {"", "", "",
         * font name}.
         * @return the full name of the font
         */
        public override string[][] FullFontName {
            get {
                return new string[][]{new string[] {"", "", "", FullName}};
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
                return new string[][]{new string[]{"4", "", "", "", FullName}};
            }
        }
        
        /** Gets the family name of the font. If it is a True Type font
         * each array element will have {Platform ID, Platform Encoding ID,
         * Language ID, font name}. The interpretation of this values can be
         * found in the Open Type specification, chapter 2, in the 'name' table.<br>
         * For the other fonts the array has a single element with {"", "", "",
         * font name}.
         * @return the family name of the font
         */
        public override string[][] FamilyFontName {
            get {
                return new string[][]{new string[] {"", "", "", FamilyName}};
            }
        }
    
        /** Checks if the font has any kerning pairs.
        * @return <CODE>true</CODE> if the font has any kerning pairs
        */    
        public override bool HasKernPairs() {
            return KernPairs.Count > 0;
        }
        
        /**
        * Sets the kerning between two Unicode chars.
        * @param char1 the first char
        * @param char2 the second char
        * @param kern the kerning to apply in normalized 1000 units
        * @return <code>true</code> if the kerning was applied, <code>false</code> otherwise
        */
        public override bool SetKerning(int char1, int char2, int kern) {
            String first = GlyphList.UnicodeToName((int)char1);
            if (first == null)
                return false;
            String second = GlyphList.UnicodeToName((int)char2);
            if (second == null)
                return false;
            Object[] obj;
            KernPairs.TryGetValue(first, out obj);
            if (obj == null) {
                obj = new Object[]{second, kern};
                KernPairs[first] = obj;
                return true;
            }
            for (int k = 0; k < obj.Length; k += 2) {
                if (second.Equals(obj[k])) {
                    obj[k + 1] = kern;
                    return true;
                }
            }
            int size = obj.Length;
            Object[] obj2 = new Object[size + 2];
            Array.Copy(obj, 0, obj2, 0, size);
            obj2[size] = second;
            obj2[size + 1] = kern;
            KernPairs[first] = obj2;
            return true;
        }
        
        protected override int[] GetRawCharBBox(int c, String name) {
            Object[] metrics;
            if (name == null) { // font specific
                CharMetrics.TryGetValue(c, out metrics);
            }
            else {
                if (name.Equals(".notdef"))
                    return null;
                CharMetrics.TryGetValue(name, out metrics);
            }
            if (metrics != null)
                return ((int[])(metrics[3]));
            return null;
        }
    }
}
