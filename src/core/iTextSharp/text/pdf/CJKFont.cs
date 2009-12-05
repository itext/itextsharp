using System;
using System.IO;
using System.Text;
using System.Collections;
using System.util;
using iTextSharp.text.error_messages;

/*
 * $Id: CJKFont.cs,v 1.9 2008/05/13 11:25:17 psoares33 Exp $
 * 
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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
 * Creates a CJK font compatible with the fonts in the Adobe Asian font Pack.
 *
 * @author  Paulo Soares
 */

internal class CJKFont : BaseFont {
    /** The encoding used in the PDF document for CJK fonts
     */
    internal const string CJK_ENCODING = "UNICODEBIGUNMARKED";
    private const int FIRST = 0;
    private const int BRACKET = 1;
    private const int SERIAL = 2;
    private const int V1Y = 880;
        
    internal static Properties cjkFonts = new Properties();
    internal static Properties cjkEncodings = new Properties();
    internal static Hashtable allCMaps = Hashtable.Synchronized(new Hashtable());
    internal static Hashtable allFonts = Hashtable.Synchronized(new Hashtable());
    private static bool propertiesLoaded = false;
    
    /** The font name */
    private string fontName;
    /** The style modifier */
    private string style = "";
    /** The CMap name associated with this font */
    private string CMap;
    
    private bool cidDirect = false;
    
    private char[] translationMap;
    private IntHashtable vMetrics;
    private IntHashtable hMetrics;
    private Hashtable fontDesc;
    private bool vertical = false;
    
    private static void LoadProperties() {
        if (propertiesLoaded)
            return;
        lock (allFonts) {
            if (propertiesLoaded)
                return;
            try {
                Stream isp = GetResourceStream(RESOURCE_PATH + "cjkfonts.properties");
                cjkFonts.Load(isp);
                isp.Close();
                isp = GetResourceStream(RESOURCE_PATH + "cjkencodings.properties");
                cjkEncodings.Load(isp);
                isp.Close();
            }
            catch {
                cjkFonts = new Properties();
                cjkEncodings = new Properties();
            }
            propertiesLoaded = true;
        }
    }
    
    /** Creates a CJK font.
     * @param fontName the name of the font
     * @param enc the encoding of the font
     * @param emb always <CODE>false</CODE>. CJK font and not embedded
     * @throws DocumentException on error
     * @throws IOException on error
     */
    internal CJKFont(string fontName, string enc, bool emb) {
        LoadProperties();
        this.FontType = FONT_TYPE_CJK;
        string nameBase = GetBaseName(fontName);
        if (!IsCJKFont(nameBase, enc))
            throw new DocumentException(MessageLocalization.GetComposedMessage("font.1.with.2.encoding.is.not.a.cjk.font", fontName, enc));
        if (nameBase.Length < fontName.Length) {
            style = fontName.Substring(nameBase.Length);
            fontName = nameBase;
        }
        this.fontName = fontName;
        encoding = CJK_ENCODING;
        vertical = enc.EndsWith("V");
        CMap = enc;
        if (enc.StartsWith("Identity-")) {
            cidDirect = true;
            string s = cjkFonts[fontName];
            s = s.Substring(0, s.IndexOf('_'));
            char[] c = (char[])allCMaps[s];
            if (c == null) {
                c = ReadCMap(s);
                if (c == null)
                    throw new DocumentException(MessageLocalization.GetComposedMessage("the.cmap.1.does.not.exist.as.a.resource", s));
                c[CID_NEWLINE] = '\n';
                allCMaps.Add(s, c);
            }
            translationMap = c;
        }
        else {
            char[] c = (char[])allCMaps[enc];
            if (c == null) {
                string s = cjkEncodings[enc];
                if (s == null)
                    throw new DocumentException(MessageLocalization.GetComposedMessage("the.resource.cjkencodings.properties.does.not.contain.the.encoding.1", enc));
                StringTokenizer tk = new StringTokenizer(s);
                string nt = tk.NextToken();
                c = (char[])allCMaps[nt];
                if (c == null) {
                    c = ReadCMap(nt);
                    allCMaps.Add(nt, c);
                }
                if (tk.HasMoreTokens()) {
                    string nt2 = tk.NextToken();
                    char[] m2 = ReadCMap(nt2);
                    for (int k = 0; k < 0x10000; ++k) {
                        if (m2[k] == 0)
                            m2[k] = c[k];
                    }
                    allCMaps.Add(enc, m2);
                    c = m2;
                }
            }
            translationMap = c;
        }
        fontDesc = (Hashtable)allFonts[fontName];
        if (fontDesc == null) {
            fontDesc = ReadFontProperties(fontName);
            allFonts.Add(fontName, fontDesc);
        }
        hMetrics = (IntHashtable)fontDesc["W"];
        vMetrics = (IntHashtable)fontDesc["W2"];
    }
    
    /** Checks if its a valid CJK font.
     * @param fontName the font name
     * @param enc the encoding
     * @return <CODE>true</CODE> if it is CJK font
     */
    public static bool IsCJKFont(string fontName, string enc) {
        LoadProperties();
        string encodings = cjkFonts[fontName];
        return (encodings != null && (enc.Equals("Identity-H") || enc.Equals("Identity-V") || encodings.IndexOf("_" + enc + "_") >= 0));
    }
        
    /**
     * Gets the width of a <CODE>char</CODE> in normalized 1000 units.
     * @param char1 the unicode <CODE>char</CODE> to get the width of
     * @return the width in normalized 1000 units
     */
    public override int GetWidth(int char1) {
        int c = (int)char1;
        if (!cidDirect)
            c = translationMap[c];
        int v;
        if (vertical)
            v = vMetrics[c];
        else
            v = hMetrics[c];
        if (v > 0)
            return v;
        else
            return 1000;
    }
    
    public override int GetWidth(string text) {
        int total = 0;
        for (int k = 0; k < text.Length; ++k) {
            int c = text[k];
            if (!cidDirect)
                c = translationMap[c];
            int v;
            if (vertical)
                v = vMetrics[c];
            else
                v = hMetrics[c];
            if (v > 0)
                total += v;
            else
                total += 1000;
        }
        return total;
    }
    
    internal override int GetRawWidth(int c, string name) {
        return 0;
    }
    public override int GetKerning(int char1, int char2) {
        return 0;
    }

    private PdfDictionary GetFontDescriptor() {
        PdfDictionary dic = new PdfDictionary(PdfName.FONTDESCRIPTOR);
        dic.Put(PdfName.ASCENT, new PdfLiteral((String)fontDesc["Ascent"]));
        dic.Put(PdfName.CAPHEIGHT, new PdfLiteral((String)fontDesc["CapHeight"]));
        dic.Put(PdfName.DESCENT, new PdfLiteral((String)fontDesc["Descent"]));
        dic.Put(PdfName.FLAGS, new PdfLiteral((String)fontDesc["Flags"]));
        dic.Put(PdfName.FONTBBOX, new PdfLiteral((String)fontDesc["FontBBox"]));
        dic.Put(PdfName.FONTNAME, new PdfName(fontName + style));
        dic.Put(PdfName.ITALICANGLE, new PdfLiteral((String)fontDesc["ItalicAngle"]));
        dic.Put(PdfName.STEMV, new PdfLiteral((String)fontDesc["StemV"]));
        PdfDictionary pdic = new PdfDictionary();
        pdic.Put(PdfName.PANOSE, new PdfString((String)fontDesc["Panose"], null));
        dic.Put(PdfName.STYLE, pdic);
        return dic;
    }
    
    private PdfDictionary GetCIDFont(PdfIndirectReference fontDescriptor, IntHashtable cjkTag) {
        PdfDictionary dic = new PdfDictionary(PdfName.FONT);
        dic.Put(PdfName.SUBTYPE, PdfName.CIDFONTTYPE0);
        dic.Put(PdfName.BASEFONT, new PdfName(fontName + style));
        dic.Put(PdfName.FONTDESCRIPTOR, fontDescriptor);
        int[] keys = cjkTag.ToOrderedKeys();
        string w = ConvertToHCIDMetrics(keys, hMetrics);
        if (w != null)
            dic.Put(PdfName.W, new PdfLiteral(w));
        if (vertical) {
            w = ConvertToVCIDMetrics(keys, vMetrics, hMetrics);
            if (w != null)
                dic.Put(PdfName.W2, new PdfLiteral(w));
        }
        else
            dic.Put(PdfName.DW, new PdfNumber(1000));
        PdfDictionary cdic = new PdfDictionary();
        cdic.Put(PdfName.REGISTRY, new PdfString((string)fontDesc["Registry"], null));
        cdic.Put(PdfName.ORDERING, new PdfString((string)fontDesc["Ordering"], null));
        cdic.Put(PdfName.SUPPLEMENT, new PdfLiteral((string)fontDesc["Supplement"]));
        dic.Put(PdfName.CIDSYSTEMINFO, cdic);
        return dic;
    }
    
    private PdfDictionary GetFontBaseType(PdfIndirectReference CIDFont) {
        PdfDictionary dic = new PdfDictionary(PdfName.FONT);
        dic.Put(PdfName.SUBTYPE, PdfName.TYPE0);
        string name = fontName;
        if (style.Length > 0)
            name += "-" + style.Substring(1);
        name += "-" + CMap;
        dic.Put(PdfName.BASEFONT, new PdfName(name));
        dic.Put(PdfName.ENCODING, new PdfName(CMap));
        dic.Put(PdfName.DESCENDANTFONTS, new PdfArray(CIDFont));
        return dic;
    }
    
    internal override void WriteFont(PdfWriter writer, PdfIndirectReference piref, Object[] parms) {
        IntHashtable cjkTag = (IntHashtable)parms[0];
        PdfIndirectReference ind_font = null;
        PdfObject pobj = null;
        PdfIndirectObject obj = null;
        pobj = GetFontDescriptor();
        if (pobj != null){
            obj = writer.AddToBody(pobj);
            ind_font = obj.IndirectReference;
        }
        pobj = GetCIDFont(ind_font, cjkTag);
        if (pobj != null){
            obj = writer.AddToBody(pobj);
            ind_font = obj.IndirectReference;
        }
        pobj = GetFontBaseType(ind_font);
        writer.AddToBody(pobj, piref);
    }
    
    /**
     * You can't get the FontStream of a CJK font (CJK fonts are never embedded),
     * so this method always returns null.
     * @return  null
     * @since   2.1.3
     */
    public override PdfStream GetFullFontStream() {
        return null;
    }

    private float GetDescNumber(string name) {
        return int.Parse((string)fontDesc[name]);
    }
    
    private float GetBBox(int idx) {
        string s = (string)fontDesc["FontBBox"];
        StringTokenizer tk = new StringTokenizer(s, " []\r\n\t\f");
        string ret = tk.NextToken();
        for (int k = 0; k < idx; ++k)
            ret = tk.NextToken();
        return int.Parse(ret);
    }
    
    /** Gets the font parameter identified by <CODE>key</CODE>. Valid values
     * for <CODE>key</CODE> are <CODE>ASCENT</CODE>, <CODE>CAPHEIGHT</CODE>, <CODE>DESCENT</CODE>
     * and <CODE>ITALICANGLE</CODE>.
     * @param key the parameter to be extracted
     * @param fontSize the font size in points
     * @return the parameter in points
     */
    public override float GetFontDescriptor(int key, float fontSize) {
        switch (key) {
            case AWT_ASCENT:
            case ASCENT:
                return GetDescNumber("Ascent") * fontSize / 1000;
            case CAPHEIGHT:
                return GetDescNumber("CapHeight") * fontSize / 1000;
            case AWT_DESCENT:
            case DESCENT:
                return GetDescNumber("Descent") * fontSize / 1000;
            case ITALICANGLE:
                return GetDescNumber("ItalicAngle");
            case BBOXLLX:
                return fontSize * GetBBox(0) / 1000;
            case BBOXLLY:
                return fontSize * GetBBox(1) / 1000;
            case BBOXURX:
                return fontSize * GetBBox(2) / 1000;
            case BBOXURY:
                return fontSize * GetBBox(3) / 1000;
            case AWT_LEADING:
                return 0;
            case AWT_MAXADVANCE:
                return fontSize * (GetBBox(2) - GetBBox(0)) / 1000;
        }
        return 0;
    }
    
    public override string PostscriptFontName {
        get {
            return fontName;
        }
        set {
            fontName = value;
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
            return new string[][]{new string[] {"", "", "", fontName}};
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
            return this.FullFontName;
        }
    }
    
    internal static char[] ReadCMap(string name) {
        Stream istr = null;
        try {
            name = name + ".cmap";
            istr = GetResourceStream(RESOURCE_PATH + name);
            char[] c = new char[0x10000];
            for (int k = 0; k < 0x10000; ++k)
                c[k] = (char)((istr.ReadByte() << 8) + istr.ReadByte());
            return c;
        }
        catch {
            // empty on purpose
        }
        finally {
            try{istr.Close();}catch{}
        }

        return null;
    }
    
    internal static IntHashtable CreateMetric(string s) {
        IntHashtable h = new IntHashtable();
        StringTokenizer tk = new StringTokenizer(s);
        while (tk.HasMoreTokens()) {
            int n1 = int.Parse(tk.NextToken());
            h[n1] = int.Parse(tk.NextToken());
        }
        return h;
    }
    
    internal static string ConvertToHCIDMetrics(int[] keys, IntHashtable h) {
        if (keys.Length == 0)
            return null;
        int lastCid = 0;
        int lastValue = 0;
        int start;
        for (start = 0; start < keys.Length; ++start) {
            lastCid = keys[start];
            lastValue = h[lastCid];
            if (lastValue != 0) {
                ++start;
                break;
            }
        }
        if (lastValue == 0)
            return null;
        StringBuilder buf = new StringBuilder();
        buf.Append('[');
        buf.Append(lastCid);
        int state = FIRST;
        for (int k = start; k < keys.Length; ++k) {
            int cid = keys[k];
            int value = h[cid];
            if (value == 0)
                continue;
            switch (state) {
                case FIRST: {
                    if (cid == lastCid + 1 && value == lastValue) {
                        state = SERIAL;
                    }
                    else if (cid == lastCid + 1) {
                        state = BRACKET;
                        buf.Append('[').Append(lastValue);
                    }
                    else {
                        buf.Append('[').Append(lastValue).Append(']').Append(cid);
                    }
                    break;
                }
                case BRACKET: {
                    if (cid == lastCid + 1 && value == lastValue) {
                        state = SERIAL;
                        buf.Append(']').Append(lastCid);
                    }
                    else if (cid == lastCid + 1) {
                        buf.Append(' ').Append(lastValue);
                    }
                    else {
                        state = FIRST;
                        buf.Append(' ').Append(lastValue).Append(']').Append(cid);
                    }
                    break;
                }
                case SERIAL: {
                    if (cid != lastCid + 1 || value != lastValue) {
                        buf.Append(' ').Append(lastCid).Append(' ').Append(lastValue).Append(' ').Append(cid);
                        state = FIRST;
                    }
                    break;
                }
            }
            lastValue = value;
            lastCid = cid;
        }
        switch (state) {
            case FIRST: {
                buf.Append('[').Append(lastValue).Append("]]");
                break;
            }
            case BRACKET: {
                buf.Append(' ').Append(lastValue).Append("]]");
                break;
            }
            case SERIAL: {
                buf.Append(' ').Append(lastCid).Append(' ').Append(lastValue).Append(']');
                break;
            }
        }
        return buf.ToString();
    }
    
    internal static string ConvertToVCIDMetrics(int[] keys, IntHashtable v, IntHashtable h) {
        if (keys.Length == 0)
            return null;
        int lastCid = 0;
        int lastValue = 0;
        int lastHValue = 0;
        int start;
        for (start = 0; start < keys.Length; ++start) {
            lastCid = keys[start];
            lastValue = v[lastCid];
            if (lastValue != 0) {
                ++start;
                break;
            }
            else
                lastHValue = h[lastCid];
        }
        if (lastValue == 0)
            return null;
        if (lastHValue == 0)
            lastHValue = 1000;
        StringBuilder buf = new StringBuilder();
        buf.Append('[');
        buf.Append(lastCid);
        int state = FIRST;
        for (int k = start; k < keys.Length; ++k) {
            int cid = keys[k];
            int value = v[cid];
            if (value == 0)
                continue;
            int hValue = h[lastCid];
            if (hValue == 0)
                hValue = 1000;
            switch (state) {
                case FIRST: {
                    if (cid == lastCid + 1 && value == lastValue && hValue == lastHValue) {
                        state = SERIAL;
                    }
                    else {
                        buf.Append(' ').Append(lastCid).Append(' ').Append(-lastValue).Append(' ').Append(lastHValue / 2).Append(' ').Append(V1Y).Append(' ').Append(cid);
                    }
                    break;
                }
                case SERIAL: {
                    if (cid != lastCid + 1 || value != lastValue || hValue != lastHValue) {
                        buf.Append(' ').Append(lastCid).Append(' ').Append(-lastValue).Append(' ').Append(lastHValue / 2).Append(' ').Append(V1Y).Append(' ').Append(cid);
                        state = FIRST;
                    }
                    break;
                }
            }
            lastValue = value;
            lastCid = cid;
            lastHValue = hValue;
        }
        buf.Append(' ').Append(lastCid).Append(' ').Append(-lastValue).Append(' ').Append(lastHValue / 2).Append(' ').Append(V1Y).Append(" ]");
        return buf.ToString();
    }
    
    internal static Hashtable ReadFontProperties(String name) {
        try {
            name += ".properties";
            Stream isp = GetResourceStream(RESOURCE_PATH + name);
            Properties p = new Properties();
            p.Load(isp);
            isp.Close();
            IntHashtable W = CreateMetric(p["W"]);
            p.Remove("W");
            IntHashtable W2 = CreateMetric(p["W2"]);
            p.Remove("W2");
            Hashtable map = new Hashtable();
            foreach (string key in p.Keys) {
                map[key] = p[key];
            }
            map["W"] = W;
            map["W2"] = W2;
            return map;
        }
        catch {
            // empty on purpose
        }
        return null;
    }

    public override int GetUnicodeEquivalent(int c) {
        if (cidDirect)
            return translationMap[c];
        return c;
    }
    
    public override int GetCidCode(int c) {
        if (cidDirect)
            return c;
        return translationMap[c];
    }

    public override bool HasKernPairs() {
        return false;
    }

    public override bool CharExists(int c) {
        return translationMap[c] != 0;
    }

    public override bool SetCharAdvance(int c, int advance) {
        return false;
    }

    public override bool SetKerning(int char1, int char2, int kern) {
        return false;
    }

    public override int[] GetCharBBox(int c) {
        return null;
    }

    protected override int[] GetRawCharBBox(int c, String name) {
        return null;
    }
}
}
