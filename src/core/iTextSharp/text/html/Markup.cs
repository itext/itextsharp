using System;
using System.util;
using System.IO;
using System.Text;
using System.Collections;
using System.Globalization;
using iTextSharp.text;

/*
 * $Id: Markup.cs,v 1.2 2008/05/13 11:25:16 psoares33 Exp $
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

namespace iTextSharp.text.html {
    /// <summary>
    /// A class that contains all the possible tagnames and their attributes.
    /// </summary>
    public class Markup {
        // iText specific
        
        /** the key for any tag */
        public const string ITEXT_TAG = "tag";

        // HTML tags

        /** the markup for the body part of a file */
        public const string HTML_TAG_BODY = "body";
        
        /** The DIV tag. */
        public const string HTML_TAG_DIV = "div";

        /** This is a possible HTML-tag. */
        public const string HTML_TAG_LINK = "link";

        /** The SPAN tag. */
        public const string HTML_TAG_SPAN = "span";

        // HTML attributes

        /** the height attribute. */
        public const string HTML_ATTR_HEIGHT = "height";

        /** the hyperlink reference attribute. */
        public const string HTML_ATTR_HREF = "href";

        /** This is a possible HTML attribute for the LINK tag. */
        public const string HTML_ATTR_REL = "rel";

        /** This is used for inline css style information */
        public const string HTML_ATTR_STYLE = "style";

        /** This is a possible HTML attribute for the LINK tag. */
        public const string HTML_ATTR_TYPE = "type";

        /** This is a possible HTML attribute. */
        public const string HTML_ATTR_STYLESHEET = "stylesheet";

        /** the width attribute. */
        public const string HTML_ATTR_WIDTH = "width";

        /** attribute for specifying externally defined CSS class */
        public const string HTML_ATTR_CSS_CLASS = "class";

        /** The ID attribute. */
        public const string HTML_ATTR_CSS_ID = "id";

        // HTML values
        
        /** This is a possible value for the language attribute (SCRIPT tag). */
        public const string HTML_VALUE_JAVASCRIPT = "text/javascript";
        
        /** This is a possible HTML attribute for the LINK tag. */
        public const string HTML_VALUE_CSS = "text/css";

        // CSS keys

        /** the CSS tag for background color */
        public const string CSS_KEY_BGCOLOR = "background-color";

        /** the CSS tag for text color */
        public const string CSS_KEY_COLOR = "color";

        /** CSS key that indicate the way something has to be displayed */
        public const string CSS_KEY_DISPLAY = "display";

        /** the CSS tag for the font family */
        public const string CSS_KEY_FONTFAMILY = "font-family";

        /** the CSS tag for the font size */
        public const string CSS_KEY_FONTSIZE = "font-size";

        /** the CSS tag for the font style */
        public const string CSS_KEY_FONTSTYLE = "font-style";

        /** the CSS tag for the font weight */
        public const string CSS_KEY_FONTWEIGHT = "font-weight";

        /** the CSS tag for text decorations */
        public const string CSS_KEY_LINEHEIGHT = "line-height";

        /** the CSS tag for the margin of an object */
        public const string CSS_KEY_MARGIN = "margin";

        /** the CSS tag for the margin of an object */
        public const string CSS_KEY_MARGINLEFT = "margin-left";

        /** the CSS tag for the margin of an object */
        public const string CSS_KEY_MARGINRIGHT = "margin-right";

        /** the CSS tag for the margin of an object */
        public const string CSS_KEY_MARGINTOP = "margin-top";

        /** the CSS tag for the margin of an object */
        public const string CSS_KEY_MARGINBOTTOM = "margin-bottom";

        /** the CSS tag for the margin of an object */
        public const String CSS_KEY_PADDING = "padding";

        /** the CSS tag for the margin of an object */
        public const String CSS_KEY_PADDINGLEFT = "padding-left";

        /** the CSS tag for the margin of an object */
        public const String CSS_KEY_PADDINGRIGHT = "padding-right";

        /** the CSS tag for the margin of an object */
        public const String CSS_KEY_PADDINGTOP = "padding-top";

        /** the CSS tag for the margin of an object */
        public const String CSS_KEY_PADDINGBOTTOM = "padding-bottom";

        /** the CSS tag for the margin of an object */
        public const String CSS_KEY_BORDERCOLOR = "border-color";

        /** the CSS tag for the margin of an object */
        public const String CSS_KEY_BORDERWIDTH = "border-width";

        /** the CSS tag for the margin of an object */
        public const String CSS_KEY_BORDERWIDTHLEFT = "border-left-width";

        /** the CSS tag for the margin of an object */
        public const String CSS_KEY_BORDERWIDTHRIGHT = "border-right-width";

        /** the CSS tag for the margin of an object */
        public const String CSS_KEY_BORDERWIDTHTOP = "border-top-width";

        /** the CSS tag for the margin of an object */
        public const String CSS_KEY_BORDERWIDTHBOTTOM = "border-bottom-width";

        /** the CSS tag for adding a page break when the document is printed */
        public const string CSS_KEY_PAGE_BREAK_AFTER = "page-break-after";

        /** the CSS tag for adding a page break when the document is printed */
        public const string CSS_KEY_PAGE_BREAK_BEFORE = "page-break-before";

        /** the CSS tag for the horizontal alignment of an object */
        public const string CSS_KEY_TEXTALIGN = "text-align";

        /** the CSS tag for text decorations */
        public const string CSS_KEY_TEXTDECORATION = "text-decoration";

        /** the CSS tag for text decorations */
        public const string CSS_KEY_VERTICALALIGN = "vertical-align";

        /** the CSS tag for the visibility of objects */
        public const string CSS_KEY_VISIBILITY = "visibility";

        // CSS values

        /** value for the CSS tag for adding a page break when the document is printed */
        public const string CSS_VALUE_ALWAYS = "always";

        /** A possible value for the DISPLAY key */
        public const string CSS_VALUE_BLOCK = "block";

        /** a CSS value for text font weight */
        public const string CSS_VALUE_BOLD = "bold";

        /** the value if you want to hide objects. */
        public const string CSS_VALUE_HIDDEN = "hidden";

        /** A possible value for the DISPLAY key */
        public const string CSS_VALUE_INLINE = "inline";
        
        /** a CSS value for text font style */
        public const string CSS_VALUE_ITALIC = "italic";

        /** a CSS value for text decoration */
        public const string CSS_VALUE_LINETHROUGH = "line-through";

        /** A possible value for the DISPLAY key */
        public const string CSS_VALUE_LISTITEM = "list-item";
        
        /** a CSS value */
        public const string CSS_VALUE_NONE = "none";

        /** a CSS value */
        public const string CSS_VALUE_NORMAL = "normal";

        /** a CSS value for text font style */
        public const string CSS_VALUE_OBLIQUE = "oblique";

        /** A possible value for the DISPLAY key */
        public const string CSS_VALUE_TABLE = "table";

        /** A possible value for the DISPLAY key */
        public const string CSS_VALUE_TABLEROW = "table-row";

        /** A possible value for the DISPLAY key */
        public const string CSS_VALUE_TABLECELL = "table-cell";

        /** the CSS value for a horizontal alignment of an object */
        public const string CSS_VALUE_TEXTALIGNLEFT = "left";

        /** the CSS value for a horizontal alignment of an object */
        public const string CSS_VALUE_TEXTALIGNRIGHT = "right";

        /** the CSS value for a horizontal alignment of an object */
        public const string CSS_VALUE_TEXTALIGNCENTER = "center";

        /** the CSS value for a horizontal alignment of an object */
        public const string CSS_VALUE_TEXTALIGNJUSTIFY = "justify";

        /** a CSS value for text decoration */
        public const string CSS_VALUE_UNDERLINE = "underline";

        /** a default value for font-size 
         * @since 2.1.3
         */
        public const float DEFAULT_FONT_SIZE = 12f;

        /// <summary>
        /// Parses a length.
        /// </summary>
        /// <param name="str">a length in the form of an optional + or -, followed by a number and a unit.</param>
        /// <returns>a float</returns>
        public static float ParseLength(string str) {
            // TODO: Evaluate the effect of this.
            // It may change the default behavour of the methd if this is changed.
            // return ParseLength(string, Markup.DEFAULT_FONT_SIZE);
            int pos = 0;
            int length = str.Length;
            bool ok = true;
            while (ok && pos < length) {
                switch (str[pos]) {
                case '+':
                case '-':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case '.':
                    pos++;
                    break;
                default:
                    ok = false;
                    break;
                }
            }
            if (pos == 0)
                return 0f;
            if (pos == length)
                return float.Parse(str, System.Globalization.NumberFormatInfo.InvariantInfo);
            float f = float.Parse(str.Substring(0, pos), System.Globalization.NumberFormatInfo.InvariantInfo);
            str = str.Substring(pos);
            // inches
            if (str.StartsWith("in")) {
                return f * 72f;
            }
            // centimeters
            if (str.StartsWith("cm")) {
                return (f / 2.54f) * 72f;
            }
            // millimeters
            if (str.StartsWith("mm")) {
                return (f / 25.4f) * 72f;
            }
            // picas
            if (str.StartsWith("pc")) {
                return f * 12f;
            }
            // default: we assume the length was measured in points
            return f;
        }

        /**
        * New method contributed by: Lubos Strapko
        * 
        * @since 2.1.3
        */
        public static float ParseLength(String str, float actualFontSize) {
            if (str == null)
                return 0f;
            int pos = 0;
            int length = str.Length;
            bool ok = true;
            while (ok && pos < length) {
                switch (str[pos]) {
                    case '+':
                    case '-':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '.':
                        pos++;
                        break;
                    default:
                        ok = false;
                        break;
                }
            }
            if (pos == 0) return 0f;
            if (pos == length) return float.Parse(str, System.Globalization.NumberFormatInfo.InvariantInfo);
            float f = float.Parse(str.Substring(0, pos), System.Globalization.NumberFormatInfo.InvariantInfo);
            str = str.Substring(pos);
            // inches
            if (str.StartsWith("in")) {
                return f * 72f;
            }
            // centimeters
            if (str.StartsWith("cm")) {
                return (f / 2.54f) * 72f;
            }
            // millimeters
            if (str.StartsWith("mm")) {
                return (f / 25.4f) * 72f;
            }
            // picas
            if (str.StartsWith("pc")) {
                return f * 12f;
            }
            // 1em is equal to the current font size
            if (str.StartsWith("em")) {
                return f * actualFontSize;
            }
            // one ex is the x-height of a font (x-height is usually about half the
            // font-size)
            if (str.StartsWith("ex")) {
                return f * actualFontSize / 2;
            }
            // default: we assume the length was measured in points
            return f;
        }
    
        /// <summary>
        /// Converts a <CODE>Color</CODE> into a HTML representation of this <CODE>Color</CODE>.
        /// </summary>
        /// <param name="color">the <CODE>Color</CODE> that has to be converted.</param>
        /// <returns>the HTML representation of this <CODE>Color</CODE></returns>
        public static Color DecodeColor(String s) {
            if (s == null)
                return null;
            s = s.ToLower(CultureInfo.InvariantCulture).Trim();
            try {
                return WebColors.GetRGBColor(s);
            }
            catch {
            }
            return null;
        }

        /// <summary>
        /// This method parses a string with attributes and returns a Properties object.
        /// </summary>
        /// <param name="str">a string of this form: 'key1="value1"; key2="value2";... keyN="valueN" '</param>
        /// <returns>a Properties object</returns>
        public static Properties ParseAttributes(string str) {
            Properties result = new Properties();
            if (str == null) return result;
            StringTokenizer keyValuePairs = new StringTokenizer(str, ";");
            StringTokenizer keyValuePair;
            string key;
            string value;
            while (keyValuePairs.HasMoreTokens()) {
                keyValuePair = new StringTokenizer(keyValuePairs.NextToken(), ":");
                if (keyValuePair.HasMoreTokens()) key = keyValuePair.NextToken().Trim().Trim();
                else continue;
                if (keyValuePair.HasMoreTokens()) value = keyValuePair.NextToken().Trim();
                else continue;
                if (value.StartsWith("\"")) value = value.Substring(1);
                if (value.EndsWith("\"")) value = value.Substring(0, value.Length - 1);
                result.Add(key.ToLower(CultureInfo.InvariantCulture), value);
            }
            return result;
        }

        /**
        * Removes the comments sections of a String.
        * 
        * @param string
        *            the original String
        * @param startComment
        *            the String that marks the start of a Comment section
        * @param endComment
        *            the String that marks the end of a Comment section.
        * @return the String stripped of its comment section
        */
        public static string RemoveComment(String str, String startComment,
                String endComment) {
            StringBuilder result = new StringBuilder();
            int pos = 0;
            int end = endComment.Length;
            int start = str.IndexOf(startComment, pos);
            while (start > -1) {
                result.Append(str.Substring(pos, start - pos));
                pos = str.IndexOf(endComment, start) + end;
                start = str.IndexOf(startComment, pos);
            }
            result.Append(str.Substring(pos));
            return result.ToString();
        }
    }
}
