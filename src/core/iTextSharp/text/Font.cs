using System;
using System.util;

using iTextSharp.text.pdf;
using iTextSharp.text.html;

/*
 * $Id: Font.cs,v 1.11 2008/05/13 11:25:10 psoares33 Exp $
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

namespace iTextSharp.text {
    /// <summary>
    /// Contains all the specifications of a font: fontfamily, size, style and color.
    /// </summary>
    /// <example>
    /// <code>
    /// Paragraph p = new Paragraph("This is a paragraph",
    ///               <strong>new Font(Font.HELVETICA, 18, Font.BOLDITALIC, new Color(0, 0, 255))</strong>);
    /// </code>
    /// </example>
    public class Font : IComparable {
    
        // static membervariables for the different families
    
        /// <summary> a possible value of a font family. </summary>
        public const int COURIER = 0;
    
        /// <summary> a possible value of a font family. </summary>
        public const int HELVETICA = 1;
    
        /// <summary> a possible value of a font family. </summary>
        public const int TIMES_ROMAN = 2;
    
        /// <summary> a possible value of a font family. </summary>
        public const int SYMBOL = 3;
    
        /// <summary> a possible value of a font family. </summary>
        public const int ZAPFDINGBATS = 4;
    
        // static membervariables for the different styles
    
        /// <summary> this is a possible style. </summary>
        public const int NORMAL        = 0;
    
        /// <summary> this is a possible style. </summary>
        public const int BOLD        = 1;
    
        /// <summary> this is a possible style. </summary>
        public const int ITALIC        = 2;
    
        /// <summary> this is a possible style. </summary>
        public const int UNDERLINE    = 4;
    
        /// <summary> this is a possible style. </summary>
        public const int STRIKETHRU    = 8;
    
        /// <summary> this is a possible style. </summary>
        public const int BOLDITALIC    = BOLD | ITALIC;
    
        // static membervariables
    
        /// <summary> the value of an undefined attribute. </summary>
        public const int UNDEFINED = -1;
    
        /// <summary> the value of the default size. </summary>
        public const int DEFAULTSIZE = 12;
    
        // membervariables
    
        /// <summary> the value of the fontfamily. </summary>
        private int family = UNDEFINED;
    
        /// <summary> the value of the fontsize. </summary>
        private float size = UNDEFINED;
    
        /// <summary> the value of the style. </summary>
        private int style = UNDEFINED;
    
        /// <summary> the value of the color. </summary>
        private Color color;
    
        /// <summary> the external font </summary>
        private BaseFont baseFont = null;
    
        // constructors
    
        /**
        * Copy constructor of a Font
        * @param other the font that has to be copied
        */
        public Font(Font other) {
            this.color = other.color;
            this.family = other.family;
            this.size = other.size;
            this.style = other.style;
            this.baseFont = other.baseFont;
        }
        
        /// <summary>
        /// Constructs a Font.
        /// </summary>
        /// <param name="family">the family to which this font belongs</param>
        /// <param name="size">the size of this font</param>
        /// <param name="style">the style of this font</param>
        /// <param name="color">the Color of this font.</param>
        public Font(int family, float size, int style, Color color) {
            this.family = family;
            this.size = size;
            this.style = style;
            this.color = color;
        }
    
        /// <summary>
        /// Constructs a Font.
        /// </summary>
        /// <param name="bf">the external font</param>
        /// <param name="size">the size of this font</param>
        /// <param name="style">the style of this font</param>
        /// <param name="color">the Color of this font.</param>
        public Font(BaseFont bf, float size, int style, Color color) {
            this.baseFont = bf;
            this.size = size;
            this.style = style;
            this.color = color;
        }
    
        /// <summary>
        /// Constructs a Font.
        /// </summary>
        /// <param name="bf">the external font</param>
        /// <param name="size">the size of this font</param>
        /// <param name="style">the style of this font</param>
        public Font(BaseFont bf, float size, int style) : this(bf, size, style, null) {}
    
        /// <summary>
        /// Constructs a Font.
        /// </summary>
        /// <param name="bf">the external font</param>
        /// <param name="size">the size of this font</param>
        public Font(BaseFont bf, float size) : this(bf, size, UNDEFINED, null) {}
    
        /// <summary>
        /// Constructs a Font.
        /// </summary>
        /// <param name="bf">the external font</param>
        public Font(BaseFont bf) : this(bf, UNDEFINED, UNDEFINED, null) {}
    
        /// <summary>
        /// Constructs a Font.
        /// </summary>
        /// <param name="family">the family to which this font belongs</param>
        /// <param name="size">the size of this font</param>
        /// <param name="style">the style of this font</param>
        public Font(int family, float size, int style) : this(family, size, style, null) {}
    
        /// <summary>
        /// Constructs a Font.
        /// </summary>
        /// <param name="family">the family to which this font belongs</param>
        /// <param name="size">the size of this font</param>
        public Font(int family, float size) : this(family, size, UNDEFINED, null) {}
    
        /// <summary>
        /// Constructs a Font.
        /// </summary>
        /// <param name="family">the family to which this font belongs</param>
        public Font(int family) : this(family, UNDEFINED, UNDEFINED, null) {}
    
        /// <summary>
        /// Constructs a Font.
        /// </summary>
        /// <overloads>
        /// Has nine overloads.
        /// </overloads>
        public Font() : this(UNDEFINED, UNDEFINED, UNDEFINED, null) {}
    
        // implementation of the Comparable interface
    
        /// <summary>
        /// Compares this Font with another
        /// </summary>
        /// <param name="obj">the other Font</param>
        /// <returns>a value</returns>
        public virtual int CompareTo(Object obj) {
            if (obj == null) {
                return -1;
            }
            Font font;
            try {
                font = (Font) obj;
                if (baseFont != null && !baseFont.Equals(font.BaseFont)) {
                    return -2;
                }
                if (this.family != font.Family) {
                    return 1;
                }
                if (this.size != font.Size) {
                    return 2;
                }
                if (this.style != font.Style) {
                    return 3;
                }
                if (this.color == null) {
                    if (font.Color == null) {
                        return 0;
                    }
                    return 4;
                }
                if (font.Color == null) {
                    return 4;
                }
                if (((Color)this.color).Equals(font.Color)) {
                    return 0;
                }
                return 4;
            }
            catch {
                return -3;
            }
        }
    
        // FAMILY

        /// <summary>
        /// Gets the family of this font.
        /// </summary>
        /// <value>the value of the family</value>
        public int Family {
            get {
                return family;
            }
        }
    
        /// <summary>
        /// Gets the familyname as a string.
        /// </summary>
        /// <value>the familyname</value>
        public virtual string Familyname {
            get {
                string tmp = "unknown";
                switch (this.Family) {
                    case Font.COURIER:
                        return FontFactory.COURIER;
                    case Font.HELVETICA:
                        return FontFactory.HELVETICA;
                    case Font.TIMES_ROMAN:
                        return FontFactory.TIMES_ROMAN;
                    case Font.SYMBOL:
                        return FontFactory.SYMBOL;
                    case Font.ZAPFDINGBATS:
                        return FontFactory.ZAPFDINGBATS;
                    default:
                        if (baseFont != null) {
                            string[][] names = baseFont.FamilyFontName;
                            for (int i = 0; i < names.Length; i++) {
                                if ("0".Equals(names[i][2])) {
                                    return names[i][3];
                                }
                                if ("1033".Equals(names[i][2])) {
                                    tmp = names[i][3];
                                }
                                if ("".Equals(names[i][2])) {
                                    tmp = names[i][3];
                                }
                            }
                        }
                        break;
                }
                return tmp;
            }
        }
    
        /// <summary>
        /// Sets the family using a String ("Courier",
        /// "Helvetica", "Times New Roman", "Symbol" or "ZapfDingbats").
        /// </summary>
        /// <param name="family">A String representing a certain font-family.</param>
        public virtual void SetFamily(String family) {
            this.family = GetFamilyIndex(family);
        }

        /// <summary>
        /// Translates a string-value of a certain family
        /// into the index that is used for this family in this class.
        /// </summary>
        /// <param name="family">A string representing a certain font-family</param>
        /// <returns>the corresponding index</returns>
        public static int GetFamilyIndex(string family) {
            if (Util.EqualsIgnoreCase(family, FontFactory.COURIER)) {
                return COURIER;
            }
            if (Util.EqualsIgnoreCase(family, FontFactory.HELVETICA)) {
                return HELVETICA;
            }
            if (Util.EqualsIgnoreCase(family, FontFactory.TIMES_ROMAN)) {
                return TIMES_ROMAN;
            }
            if (Util.EqualsIgnoreCase(family, FontFactory.SYMBOL)) {
                return SYMBOL;
            }
            if (Util.EqualsIgnoreCase(family, FontFactory.ZAPFDINGBATS)) {
                return ZAPFDINGBATS;
            }
            return UNDEFINED;
        }
    
    	// SIZE
	
        /// <summary>
        /// Get/set the size of this font.
        /// </summary>
        /// <value>the size of this font</value>
        public virtual float Size {
            get {
                return size;
            }
            set {
                this.size = value;
            }
        }
    
        /** Gets the size that can be used with the calculated <CODE>BaseFont</CODE>.
        * @return the size that can be used with the calculated <CODE>BaseFont</CODE>
        */    
        public float CalculatedSize {
            get {
                float s = this.size;
                if (s == UNDEFINED) {
                    s = DEFAULTSIZE;
                }
                return s;
            }
        }

        /**
        * Gets the leading that can be used with this font.
        * 
        * @param linespacing
        *            a certain linespacing
        * @return the height of a line
        */
        public float GetCalculatedLeading(float linespacing) {
            return linespacing * CalculatedSize;
        }

        // STYLE

        /// <summary>
        /// Gets the style of this font.
        /// </summary>
        /// <value>the style of this font</value>
        public int Style {
            get {
                return style;
            }
        }
    
        /** Gets the style that can be used with the calculated <CODE>BaseFont</CODE>.
        * @return the style that can be used with the calculated <CODE>BaseFont</CODE>
        */    
        public int CalculatedStyle {
            get {
                int style = this.style;
                if (style == UNDEFINED) {
                    style = NORMAL;
                }
                if (baseFont != null)
                    return style;
                if (family == SYMBOL || family == ZAPFDINGBATS)
                    return style;
                else
                    return style & (~BOLDITALIC);
            }
        }
        
        /// <summary>
        /// checks if this font is Bold.
        /// </summary>
        /// <returns>a boolean</returns>
        public bool IsBold() {
            if (style == UNDEFINED) {
                return false;
            }
            return (style &    BOLD) == BOLD;
        }
    
        /// <summary>
        /// checks if this font is Bold.
        /// </summary>
        /// <returns>a boolean</returns>
        public bool IsItalic() {
            if (style == UNDEFINED) {
                return false;
            }
            return (style &    ITALIC) == ITALIC;
        }
    
        /// <summary>
        /// checks if this font is underlined.
        /// </summary>
        /// <returns>a boolean</returns>
        public bool IsUnderlined() {
            if (style == UNDEFINED) {
                return false;
            }
            return (style &    UNDERLINE) == UNDERLINE;
        }
    
        /// <summary>
        /// checks if the style of this font is STRIKETHRU.
        /// </summary>
        /// <returns>a boolean</returns>
        public bool IsStrikethru() {
            if (style == UNDEFINED) {
                return false;
            }
            return (style &    STRIKETHRU) == STRIKETHRU;
        }
    
        /// <summary>
        /// Sets the style using a String containing one of
        /// more of the following values: normal, bold, italic, underline, strike.
        /// </summary>
        /// <param name="style">A String representing a certain style.</param>
        public virtual void SetStyle(String style) {
            if (this.style == UNDEFINED) this.style = NORMAL;
            this.style |= GetStyleValue(style);
        }

        /**
        * Sets the style.
        * @param    style    the style.
        */
            
        public virtual void SetStyle(int style) {
            this.style = style;
        }
            
        /// <summary>
        /// Translates a string-value of a certain style
        /// into the index value is used for this style in this class.
        /// </summary>
        /// <param name="style">a string</param>
        /// <returns>the corresponding value</returns>
        public static int GetStyleValue(string style) {
            int s = 0;
            if (style.IndexOf(Markup.CSS_VALUE_NORMAL) != -1) {
                s |= NORMAL;
            }
            if (style.IndexOf(Markup.CSS_VALUE_BOLD) != -1) {
                s |= BOLD;
            }
            if (style.IndexOf(Markup.CSS_VALUE_ITALIC) != -1) {
                s |= ITALIC;
            }
            if (style.IndexOf(Markup.CSS_VALUE_OBLIQUE) != -1) {
                s |= ITALIC;
            }
            if (style.IndexOf(Markup.CSS_VALUE_UNDERLINE) != -1) {
                s |= UNDERLINE;
            }
            if (style.IndexOf(Markup.CSS_VALUE_LINETHROUGH) != -1) {
                s |= STRIKETHRU;
            }
            return s;
        }
    
    
        // COLOR

        /// <summary>
        /// Get/set the color of this font.
        /// </summary>
        /// <value>the color of this font</value>
        public virtual Color Color {
            get {
                return color;
            }
            set {
                this.color = value;
            }
        }
    
        /// <summary>
        /// Sets the color.
        /// </summary>
        /// <param name="red">the red-value of the new color</param>
        /// <param name="green">the green-value of the new color</param>
        /// <param name="blue">the blue-value of the new color</param>
        public virtual void SetColor(int red, int green, int blue) {
            this.color = new Color(red, green, blue);
        }
    
        // BASEFONT

        /// <summary>
        /// Gets the BaseFont inside this object.
        /// </summary>
        /// <value>the BaseFont</value>
        public BaseFont BaseFont {
            get {
                return baseFont;
            }
        }

        /** Gets the <CODE>BaseFont</CODE> this class represents.
        * For the built-in fonts a <CODE>BaseFont</CODE> is calculated.
        * @param specialEncoding <CODE>true</CODE> to use the special encoding for Symbol and ZapfDingbats,
        * <CODE>false</CODE> to always use <CODE>Cp1252</CODE>
        * @return the <CODE>BaseFont</CODE> this class represents
        */    
        public BaseFont GetCalculatedBaseFont(bool specialEncoding) {
            if (baseFont != null)
                return baseFont;
            int style = this.style;
            if (style == UNDEFINED) {
                style = NORMAL;
            }
            String fontName = BaseFont.HELVETICA;
            String encoding = BaseFont.WINANSI;
            BaseFont cfont = null;
            switch (family) {
                case COURIER:
                    switch (style & BOLDITALIC) {
                        case BOLD:
                            fontName = BaseFont.COURIER_BOLD;
                            break;
                        case ITALIC:
                            fontName = BaseFont.COURIER_OBLIQUE;
                            break;
                        case BOLDITALIC:
                            fontName = BaseFont.COURIER_BOLDOBLIQUE;
                            break;
                        default:
                        //case NORMAL:
                            fontName = BaseFont.COURIER;
                            break;
                    }
                    break;
                case TIMES_ROMAN:
                    switch (style & BOLDITALIC) {
                        case BOLD:
                            fontName = BaseFont.TIMES_BOLD;
                            break;
                        case ITALIC:
                            fontName = BaseFont.TIMES_ITALIC;
                            break;
                        case BOLDITALIC:
                            fontName = BaseFont.TIMES_BOLDITALIC;
                            break;
                        default:
                        //case NORMAL:
                            fontName = BaseFont.TIMES_ROMAN;
                            break;
                    }
                    break;
                case SYMBOL:
                    fontName = BaseFont.SYMBOL;
                    if (specialEncoding)
                        encoding = BaseFont.SYMBOL;
                    break;
                case ZAPFDINGBATS:
                    fontName = BaseFont.ZAPFDINGBATS;
                    if (specialEncoding)
                        encoding = BaseFont.ZAPFDINGBATS;
                    break;
                default:
                //case Font.HELVETICA:
                    switch (style & BOLDITALIC) {
                        case BOLD:
                            fontName = BaseFont.HELVETICA_BOLD;
                            break;
                        case ITALIC:
                            fontName = BaseFont.HELVETICA_OBLIQUE;
                            break;
                        case BOLDITALIC:
                            fontName = BaseFont.HELVETICA_BOLDOBLIQUE;
                            break;
                        default:
                        //case NORMAL:
                            fontName = BaseFont.HELVETICA;
                            break;
                    }
                    break;
            }
            cfont = BaseFont.CreateFont(fontName, encoding, false);
            return cfont;
        }

        // Helper methods

        /// <summary>
        /// Checks if the properties of this font are undefined or null.
        /// <p/>
        /// If so, the standard should be used.
        /// </summary>
        /// <returns>a boolean</returns>
        public virtual bool IsStandardFont() {
            return (family == UNDEFINED
                && size == UNDEFINED
                && style == UNDEFINED
                && color == null
                && baseFont == null);
        }
    
        /// <summary>
        /// Replaces the attributes that are equal to null with
        /// the attributes of a given font.
        /// </summary>
        /// <param name="font">the font of a bigger element class</param>
        /// <returns>a Font</returns>
        public virtual Font Difference(Font font) {
            if (font == null) return this;
            // size
            float dSize = font.size;
            if (dSize == UNDEFINED) {
                dSize = this.size;
            }
            // style
            int dStyle = UNDEFINED;
            int style1 = this.Style;
            int style2 = font.Style;
            if (style1 != UNDEFINED || style2 != UNDEFINED) {
                if (style1 == UNDEFINED) style1 = 0;
                if (style2 == UNDEFINED) style2 = 0;
                dStyle = style1 | style2;
            }
            // color
            object dColor = (Color)font.Color;
            if (dColor == null) {
                dColor = this.Color;
            }
            // family
            if (font.baseFont != null) {
                return new Font(font.BaseFont, dSize, dStyle, (Color)dColor);
            }
            if (font.Family != UNDEFINED) {
                return new Font(font.Family, dSize, dStyle, (Color)dColor);
            }
            if (this.baseFont != null) {
                if (dStyle == style1) {
                    return new Font(this.BaseFont, dSize, dStyle, (Color)dColor);
                }
                else {
                    return FontFactory.GetFont(this.Familyname, dSize, dStyle, (Color)dColor);
                }
            }
            return new Font(this.Family, dSize, dStyle, (Color)dColor);
        }
    }
}
