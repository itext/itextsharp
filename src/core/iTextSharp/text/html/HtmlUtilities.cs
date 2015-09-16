using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.util;
/*
 * $Id: Markup.java 4672 2011-01-30 10:38:33Z blowagie $
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

namespace iTextSharp.text.html {
    /**
     * A class that contains some utilities to parse HTML attributes and content.
     * @since 5.0.6 (some of these methods used to be in the Markup class)
     * @deprecated since 5.5.2
     */
    [Obsolete]
    public class HtmlUtilities {

        /**
         * a default value for font-size 
         * @since 2.1.3
         */
        public const float DEFAULT_FONT_SIZE = 12f;

        private static Dictionary<String,float> sizes = new Dictionary<String,float>();

        static HtmlUtilities() {
            sizes["xx-small"] = 4;
            sizes["x-small"] = 6;
            sizes["small"] = 8;
            sizes["medium"] = 10;
            sizes["large"] = 13;
            sizes["x-large"] = 18;
            sizes["xx-large"] = 26;
        }

        /**
         * Parses a length.
         * 
         * @param str
         *            a length in the form of an optional + or -, followed by a
         *            number and a unit.
         * @return a float
         */

        public static float ParseLength(String str) {
            return ParseLength(str, DEFAULT_FONT_SIZE);
        }

        /**
         * New method contributed by: Lubos Strapko
         * 
         * @since 2.1.3
         */
        public static float ParseLength(String str, float actualFontSize) {
            if (str == null)
                return 0f;
            if (sizes.ContainsKey(str))
                return sizes[str];
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
                return float.Parse(str, CultureInfo.InvariantCulture);
            float f = float.Parse(str.Substring(0, pos), CultureInfo.InvariantCulture);
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

        /**
         * Converts a <CODE>BaseColor</CODE> into a HTML representation of this <CODE>
         * BaseColor</CODE>.
         * 
         * @param s
         *            the <CODE>BaseColor</CODE> that has to be converted.
         * @return the HTML representation of this <COLOR>BaseColor </COLOR>
         */

        public static BaseColor DecodeColor(String s) {
            if (s == null)
                return null;
            s = s.ToLowerInvariant().Trim();
            try {
                return WebColors.GetRGBColor(s);
            }
            catch {
                return null;
            }
        }

        /**
         * This method parses a String with attributes and returns a Properties
         * object.
         * 
         * @param str
         *            a String of this form: 'key1="value1"; key2="value2";...
         *            keyN="valueN" '
         * @return a Properties object
         */
        public static Properties ParseAttributes(String str) {
            Properties result = new Properties();
            if (str == null)
                return result;
            StringTokenizer keyValuePairs = new StringTokenizer(str, ";");
            StringTokenizer keyValuePair;
            String key;
            String value;
            while (keyValuePairs.HasMoreTokens()) {
                keyValuePair = new StringTokenizer(keyValuePairs.NextToken(), ":");
                if (keyValuePair.HasMoreTokens())
                    key = keyValuePair.NextToken().Trim();
                else
                    continue;
                if (keyValuePair.HasMoreTokens())
                    value = keyValuePair.NextToken().Trim();
                else
                    continue;
                if (value.StartsWith("\""))
                    value = value.Substring(1);
                if (value.EndsWith("\""))
                    value = value.Substring(0, value.Length - 1);
                result[key.ToLowerInvariant()] = value;
            }
            return result;
        }

        /**
         * Removes the comments sections of a String.
         * 
         * @param str
         *            the original String
         * @param startComment
         *            the String that marks the start of a Comment section
         * @param endComment
         *            the String that marks the end of a Comment section.
         * @return the String stripped of its comment section
         */
        public static String RemoveComment(String str, String startComment,
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
        
        /**
         * Helper class that reduces the white space in a String
         * @param content content containing whitespace
         * @return the content without all unnecessary whitespace
         */
        public static String EliminateWhiteSpace(String content) {
            // multiple spaces are reduced to one,
            // newlines are treated as spaces,
            // tabs, carriage returns are ignored.
            StringBuilder buf = new StringBuilder();
            int len = content.Length;
            char character;
            bool newline = false;
            for (int i = 0; i < len; i++) {
                switch (character = content[i]) {
                case ' ':
                    if (!newline) {
                        buf.Append(character);
                    }
                    break;
                case '\n':
                    if (i > 0) {
                        newline = true;
                        buf.Append(' ');
                    }
                    break;
                case '\r':
                    break;
                case '\t':
                    break;
                default:
                    newline = false;
                    buf.Append(character);
                    break;
                }
            }
            return buf.ToString();
        }

        /**
         * A series of predefined font sizes.
         * @since 5.0.6 (renamed)
         */
        public static readonly int[] FONTSIZES = { 8, 10, 12, 14, 18, 24, 36 };
        
        /**
         * Picks a font size from a series of predefined font sizes.
         * @param value     the new value of a font, expressed as an index
         * @param previous  the previous value of the font size
         * @return  a new font size.
         */
        public static int GetIndexedFontSize(String value, String previous) {
            // the font is expressed as an index in a series of predefined font sizes
            int sIndex = 0;
            // the font is defined as a relative size
            if (value.StartsWith("+") || value.StartsWith("-")) {
                // fetch the previous value
                if (previous == null)
                    previous = "12";
                int c = (int)float.Parse(previous, CultureInfo.InvariantCulture);
                // look for the nearest font size in the predefined series
                for (int k = FONTSIZES.Length - 1; k >= 0; --k) {
                    if (c >= FONTSIZES[k]) {
                        sIndex = k;
                        break;
                    }
                }
                // retrieve the difference
                int diff =
                    int.Parse(value.StartsWith("+") ?
                        value.Substring(1) : value);
                // apply the difference
                sIndex += diff;
            }
            // the font is defined as an index
            else {
                try {
                    sIndex = int.Parse(value) - 1;
                } 
                catch {
                    sIndex = 0;
                }
            }
            if (sIndex < 0)
                sIndex = 0;
            else if (sIndex >= FONTSIZES.Length)
                sIndex = FONTSIZES.Length - 1;
            return FONTSIZES[sIndex];
        }

        /**
         * Translates a String value to an alignment value.
         * (written by Norman Richards, integrated into iText by Bruno)
         * @param   alignment a String (one of the ALIGN_ constants of this class)
         * @return  an alignment value (one of the ALIGN_ constants of the Element interface) 
         */
        public static int AlignmentValue(String alignment) {
            if (alignment == null) return Element.ALIGN_UNDEFINED;
            if (Util.EqualsIgnoreCase(HtmlTags.ALIGN_CENTER, alignment)) {
                return Element.ALIGN_CENTER;
            }
            if (Util.EqualsIgnoreCase(HtmlTags.ALIGN_LEFT, alignment)) {
                return Element.ALIGN_LEFT;
            }
            if (Util.EqualsIgnoreCase(HtmlTags.ALIGN_RIGHT, alignment)) {
                return Element.ALIGN_RIGHT;
            }
            if (Util.EqualsIgnoreCase(HtmlTags.ALIGN_JUSTIFY, alignment)) {
                return Element.ALIGN_JUSTIFIED;
            }
            if (Util.EqualsIgnoreCase(HtmlTags.ALIGN_JUSTIFIED_ALL, alignment)) {
                return Element.ALIGN_JUSTIFIED_ALL;
            }
            if (Util.EqualsIgnoreCase(HtmlTags.ALIGN_TOP, alignment)) {
                return Element.ALIGN_TOP;
            }
            if (Util.EqualsIgnoreCase(HtmlTags.ALIGN_MIDDLE, alignment)) {
                return Element.ALIGN_MIDDLE;
            }
            if (Util.EqualsIgnoreCase(HtmlTags.ALIGN_BOTTOM, alignment)) {
                return Element.ALIGN_BOTTOM;
            }
            if (Util.EqualsIgnoreCase(HtmlTags.ALIGN_BASELINE, alignment)) {
                return Element.ALIGN_BASELINE;
            }
            return Element.ALIGN_UNDEFINED;
        }
    }
}
