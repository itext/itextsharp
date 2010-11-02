using System;
using System.Collections.Generic;
using System.Globalization;
using System.util;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
/*
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

namespace iTextSharp.text.html.simpleparser {

    /**
    *
    * @author  psoares
    */
    public class FactoryProperties {
        
        /**
        * @since    iText 5.0   This used to be a FontFactoryImp
        */
        private IFontProvider fontImp = FontFactory.FontImp;

        /** Creates a new instance of FactoryProperties */
        public FactoryProperties() {
        }
        
        public Chunk CreateChunk(String text, ChainedProperties props) {
            Font font = GetFont(props);
            float size = font.Size;
            size /= 2;
            Chunk ck = new Chunk(text, font);
            if (props.HasProperty("sub"))
                ck.SetTextRise(-size);
            else if (props.HasProperty("sup"))
                ck.SetTextRise(size);
            ck.SetHyphenation(GetHyphenation(props));
            return ck;
        }
        
        private static void SetParagraphLeading(Paragraph p, String leading) {
            if (leading == null) {
                p.SetLeading(0, 1.5f);
                return;
            }
            try {
                StringTokenizer tk = new StringTokenizer(leading, " ,");
                String v = tk.NextToken();
                float v1 = float.Parse(v, System.Globalization.NumberFormatInfo.InvariantInfo);
                if (!tk.HasMoreTokens()) {
                    p.SetLeading(v1, 0);
                    return;
                }
                v = tk.NextToken();
                float v2 = float.Parse(v, System.Globalization.NumberFormatInfo.InvariantInfo);
                p.SetLeading(v1, v2);
            }
            catch {
                p.SetLeading(0, 1.5f);
            }

        }

        public static void CreateParagraph(Paragraph p, ChainedProperties props) {
            String value = props["align"];
            if (value != null) {
                if (Util.EqualsIgnoreCase(value, "center"))
                    p.Alignment = Element.ALIGN_CENTER;
                else if (Util.EqualsIgnoreCase(value, "right"))
                    p.Alignment = Element.ALIGN_RIGHT;
                else if (Util.EqualsIgnoreCase(value, "justify"))
                    p.Alignment = Element.ALIGN_JUSTIFIED;
            }
            p.Hyphenation = GetHyphenation(props);
            SetParagraphLeading(p, props["leading"]);
            value = props["before"];
            if (value != null) {
                try {
                    p.SpacingBefore = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
                }
                catch {}
            }
            value = props["after"];
            if (value != null) {
                try {
                    p.SpacingAfter = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
                }
                catch {}
            }
            value = props["extraparaspace"];
            if (value != null) {
                try {
                    p.ExtraParagraphSpace = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
                }
                catch {}
            }
        }

        public static Paragraph CreateParagraph(ChainedProperties props) {
            Paragraph p = new Paragraph();
            CreateParagraph(p, props);
            return p;
        }

        public static ListItem CreateListItem(ChainedProperties props) {
            ListItem p = new ListItem();
            CreateParagraph(p, props);
            return p;
        }

        public Font GetFont(ChainedProperties props) {
            String face = props[ElementTags.FACE];
            // try again, under the CSS key.  
            //ISSUE: If both are present, we always go with face, even if font-family was  
            //  defined more recently in our ChainedProperties.  One solution would go like this: 
            //    Map all our supported style attributes to the 'normal' tag name, so we could   
            //    look everything up under that one tag, retrieving the most current value.
            if (face == null || face.Trim().Length == 0) {
                face = props[Markup.CSS_KEY_FONTFAMILY];
            }
            if (face != null) {
                StringTokenizer tok = new StringTokenizer(face, ",");
                while (tok.HasMoreTokens()) {
                    face = tok.NextToken().Trim();
                    if (face.StartsWith("\""))
                        face = face.Substring(1);
                    if (face.EndsWith("\""))
                        face = face.Substring(0, face.Length - 1);
                    if (fontImp.IsRegistered(face))
                        break;
                }
            }
            int style = 0;
            String textDec = props[Markup.CSS_KEY_TEXTDECORATION];
            if (textDec != null && textDec.Trim().Length != 0) {
                if (Markup.CSS_VALUE_UNDERLINE.Equals(textDec)) {
                    style |= Font.UNDERLINE;
                }
                else if (Markup.CSS_VALUE_LINETHROUGH.Equals(textDec)) {
                    style |= Font.STRIKETHRU;
                }
            }
            if (props.HasProperty(HtmlTags.I))
                style |= Font.ITALIC;
            if (props.HasProperty(HtmlTags.B))
                style |= Font.BOLD;
            if (props.HasProperty(HtmlTags.U))
                style |= Font.UNDERLINE;
            if (props.HasProperty(HtmlTags.S))
                style |= Font.STRIKETHRU ;

            String value = props[ElementTags.SIZE];
            float size = 12;
            if (value != null)
                size = float.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
            BaseColor color = Markup.DecodeColor(props["color"]);
            String encoding = props["encoding"];
            if (encoding == null)
                encoding = BaseFont.WINANSI;
            return fontImp.GetFont(face, encoding, true, size, style, color);
        }
        
        /**
        * Gets a HyphenationEvent based on the hyphenation entry in ChainedProperties.
        * @param    props   ChainedProperties
        * @return   a HyphenationEvent
        * @since    2.1.2
        */
        public static IHyphenationEvent GetHyphenation(ChainedProperties props) {
            return GetHyphenation(props["hyphenation"]);
        }

        /**
        * Gets a HyphenationEvent based on the hyphenation entry in a HashMap.
        * @param    props   a HashMap with properties
        * @return   a HyphenationEvent
        * @since    2.1.2
        */
        public static IHyphenationEvent GetHyphenation(Dictionary<string,string> props) {
            if (props.ContainsKey("hyphenation"))
                return GetHyphenation(props["hyphenation"]);
            else
                return null;
        }
        
        /**
        * Gets a HyphenationEvent based on a String.
        * For instance "en_UK,3,2" returns new HyphenationAuto("en", "UK", 3, 2);
        * @param    a String, for instance "en_UK,2,2"
        * @return   a HyphenationEvent
        * @since    2.1.2
        */
        public static IHyphenationEvent GetHyphenation(String s) {
            if (s == null || s.Length == 0) {
                return null;
            }
            String lang = s;
            String country = null;
            int leftMin = 2;
            int rightMin = 2;
            
            int pos = s.IndexOf('_');
            if (pos == -1) {
                return new HyphenationAuto(lang, country, leftMin, rightMin);
            }
            lang = s.Substring(0, pos);
            country = s.Substring(pos + 1);
            pos = country.IndexOf(',');
            if (pos == -1) {
                return new HyphenationAuto(lang, country, leftMin, rightMin);
            }
            s = country.Substring(pos + 1);
            country = country.Substring(0, pos);
            pos = s.IndexOf(',');
            if (pos == -1) {
                leftMin = int.Parse(s);
            }
            else {
                leftMin = int.Parse(s.Substring(0, pos));
                rightMin = int.Parse(s.Substring(pos + 1));
            }   
            return new HyphenationAuto(lang, country, leftMin, rightMin);
        }
        
	    /**
	    * This method isn't used by iText, but you can use it to analyze
	    * the value of a style attribute inside a HashMap.
	    * The different elements of the style attribute are added to the
	    * HashMap as key-value pairs.
	    * @param	h	a HashMap that should have at least a key named
	    * style. After this method is invoked, more keys could be added.
	    */
        public static void InsertStyle(Dictionary<string,string> h) {
            String style;
            if (!h.TryGetValue("style", out style))
                return;
            Properties prop = Markup.ParseAttributes(style);
            foreach (String key in prop.Keys) {
                if (key.Equals(Markup.CSS_KEY_FONTFAMILY)) {
                    h["face"] = prop[key];
                }
                else if (key.Equals(Markup.CSS_KEY_FONTSIZE)) {
                    h["size"] = Markup.ParseLength(prop[key]).ToString(NumberFormatInfo.InvariantInfo) + "pt";
                }
                else if (key.Equals(Markup.CSS_KEY_FONTSTYLE)) {
                    String ss = prop[key].Trim().ToLower(CultureInfo.InvariantCulture);
                    if (ss.Equals("italic") || ss.Equals("oblique"))
                        h["i"] = null;
                }
                else if (key.Equals(Markup.CSS_KEY_FONTWEIGHT)) {
                    String ss = prop[key].Trim().ToLower(CultureInfo.InvariantCulture);
                    if (ss.Equals("bold") || ss.Equals("700") || ss.Equals("800") || ss.Equals("900"))
                        h["b"] = null;
                }
                else if (key.Equals(Markup.CSS_KEY_TEXTDECORATION)) {
                    String ss = prop[key].Trim().ToLower(CultureInfo.InvariantCulture);
                    if (ss.Equals(Markup.CSS_VALUE_UNDERLINE))
                        h["u"] = null;
                }
                else if (key.Equals(Markup.CSS_KEY_COLOR)) {
                    BaseColor c = Markup.DecodeColor(prop[key]);
                    if (c != null) {
                        int hh = c.ToArgb() & 0xffffff;
                        String hs = "#" + hh.ToString("X06", NumberFormatInfo.InvariantInfo);
                        h["color"] = hs;
                    }
                }
                else if (key.Equals(Markup.CSS_KEY_LINEHEIGHT)) {
                    String ss = prop[key].Trim();
                    float v = Markup.ParseLength(prop[key]);
                    if (ss.EndsWith("%")) {
                        v /= 100;
                        h["leading"] = "0," + v.ToString(NumberFormatInfo.InvariantInfo);
                    } 
                    else if (Util.EqualsIgnoreCase("normal", ss)) {
                        h["leading"] = "0,1.5";
                    }
                    else {
                        h["leading"] = v.ToString(NumberFormatInfo.InvariantInfo) + ",0";
                    }
                }
                else if (key.Equals(Markup.CSS_KEY_TEXTALIGN)) {
                    String ss = prop[key].Trim().ToLower(System.Globalization.CultureInfo.InvariantCulture);
                    h["align"] = ss;
                }
            }
        }
        
	    /**
	    * New method contributed by Lubos Strapko
	    * @param h
	    * @param cprops
	    * @since 2.1.3
	    */
        public static void InsertStyle(Dictionary<string,string> h, ChainedProperties cprops) {
            String style;
            if (!h.TryGetValue("style", out style))
                return;
            Properties prop = Markup.ParseAttributes(style);
            foreach (String key in prop.Keys) {
                if (key.Equals(Markup.CSS_KEY_FONTFAMILY)) {
                    h["face"] = prop[key];
                }
                else if (key.Equals(Markup.CSS_KEY_FONTSIZE)) {
                    float actualFontSize = Markup.ParseLength(cprops[ElementTags.SIZE], Markup.DEFAULT_FONT_SIZE);
                    if (actualFontSize <= 0f)
                        actualFontSize = Markup.DEFAULT_FONT_SIZE;
                    h[ElementTags.SIZE] = Markup.ParseLength(prop[key], actualFontSize).ToString(NumberFormatInfo.InvariantInfo) + "pt";
                }
                else if (key.Equals(Markup.CSS_KEY_FONTSTYLE)) {
                    String ss = prop[key].Trim().ToLower(CultureInfo.InvariantCulture);
                    if (ss.Equals("italic") || ss.Equals("oblique"))
                        h["i"] = null;
                }
                else if (key.Equals(Markup.CSS_KEY_FONTWEIGHT)) {
                    String ss = prop[key].Trim().ToLower(CultureInfo.InvariantCulture);
                    if (ss.Equals("bold") || ss.Equals("700") || ss.Equals("800") || ss.Equals("900"))
                        h["b"] = null;
                }
                else if (key.Equals(Markup.CSS_KEY_TEXTDECORATION)) {
                    String ss = prop[key].Trim().ToLower(CultureInfo.InvariantCulture);
                    if (ss.Equals(Markup.CSS_VALUE_UNDERLINE))
                        h["u"] = null;
                }
                else if (key.Equals(Markup.CSS_KEY_COLOR)) {
                    BaseColor c = Markup.DecodeColor(prop[key]);
                    if (c != null) {
                        int hh = c.ToArgb() & 0xffffff;
                        String hs = "#" + hh.ToString("X06", NumberFormatInfo.InvariantInfo);
                        h["color"] = hs;
                    }
                }
                else if (key.Equals(Markup.CSS_KEY_LINEHEIGHT)) {
                    String ss = prop[key].Trim();
                    float actualFontSize = Markup.ParseLength(cprops[ElementTags.SIZE], Markup.DEFAULT_FONT_SIZE);
                    if (actualFontSize <= 0f)
                        actualFontSize = Markup.DEFAULT_FONT_SIZE;
                    float v = Markup.ParseLength(prop[key], actualFontSize);
                    if (ss.EndsWith("%")) {
                        v /= 100;
                        h["leading"] = "0," + v.ToString(NumberFormatInfo.InvariantInfo);
                    } 
                    else if (Util.EqualsIgnoreCase("normal", ss)) {
                        h["leading"] = "0,1.5";
                    }
                    else {
                        h["leading"] = v.ToString(NumberFormatInfo.InvariantInfo) + ",0";
                    }
                }
                else if (key.Equals(Markup.CSS_KEY_TEXTALIGN)) {
                    String ss = prop[key].Trim().ToLower(System.Globalization.CultureInfo.InvariantCulture);
                    h["align"] = ss;
                } else if (key.Equals(Markup.CSS_KEY_PADDINGLEFT)) {
                    String ss = prop[key].Trim().ToLower(System.Globalization.CultureInfo.InvariantCulture);
                    h["indent"] = ss;
                }
            }
        }
        
        public IFontProvider FontImp {
            get {
                return fontImp;
            }
            set {
                fontImp = value;
            }
        }

        public static Dictionary<string,string> followTags = new Dictionary<string,string>();

        static FactoryProperties() {
            followTags["i"] = "i";
            followTags["b"] = "b";
            followTags["u"] = "u";
            followTags["sub"] = "sub";
            followTags["sup"] = "sup";
            followTags["em"] = "i";
            followTags["strong"] = "b";
            followTags["s"] = "s";
            followTags["strike"] = "s";
        }
    }
}
