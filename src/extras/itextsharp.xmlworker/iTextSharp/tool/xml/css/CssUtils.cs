/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

    This program is free software; you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License version 3
    as published by the Free Software Foundation with the addition of the
    following permission added to Section 15 as permitted in Section 7(a):
    FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
    ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
    OF THIRD PARTY RIGHTS
    
    This program is distributed in the hope that it will be useful, but
    WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
    or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU Affero General Public License for more details.
    You should have received a copy of the GNU Affero General Public License
    along with this program; if not, see http://www.gnu.org/licenses or write to
    the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
    Boston, MA, 02110-1301 USA, or download the license from the following URL:
    http://itextpdf.com/terms-of-use/
    
    The interactive user interfaces in modified source and object code versions
    of this program must display Appropriate Legal Notices, as required under
    Section 5 of the GNU Affero General Public License.
    
    In accordance with Section 7(b) of the GNU Affero General Public License,
    a covered work must retain the producer line in every PDF that is created
    or manipulated using iText.
    
    You can be released from the requirements of the license by purchasing
    a commercial license. Buying such a license is mandatory as soon as you
    develop commercial activities involving the iText software without
    disclosing the source code of your own applications.
    These activities include: offering paid services to customers as an ASP,
    serving PDFs on the fly in a web application, shipping iText with a closed
    source product.
    
    For more information, please contact iText Software Corp. at this
    address: sales@itextpdf.com
 */
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Globalization;
using System.util;
using iTextSharp.text.html;
using iTextSharp.tool.xml.css.apply;
using iTextSharp.tool.xml.exceptions;

namespace iTextSharp.tool.xml.css {

    /**
     * @author redlab_b
     *
     */
    public class CssUtils {

        private const string COLOR = "-color";
        private const string STYLE = "-style";
        private const string WIDTH = "-width";
        private const string BORDER2 = "border-";
        private const string _0_LEFT_1 = "{0}left{1}";
        private const string _0_RIGHT_1 = "{0}right{1}";
        private const string _0_BOTTOM_1 = "{0}bottom{1}";
        private const string _0_TOP_1 = "{0}top{1}";
        private static CssUtils instance = new CssUtils();
        private static object syncroot = new object();

        /**
         * Default font size if none is set.
         */
        public const int DEFAULT_FONT_SIZE_PT = 12;


        /**
         * @return Singleton instance of CssUtils.
         */
        public static CssUtils GetInstance()
        {
            return instance;    
        }

        /**
         *
         */
        private CssUtils() {
        }
        /**
         * Returns the top, bottom, left, right version for the given box. the keys
         * will be the pre value concatenated with either top, bottom, right or left
         * and the post value. <strong>Note:</strong> Does not work when double
         * spaces are in the boxes value. (<strong>Tip:</strong> Use
         * {@link CssUtils#stripDoubleSpacesAndTrim(String)})
         *
         * @param box
         *            the value to parse
         * @param pre
         *            the pre key part
         * @param post
         *            the post key part
         * @return a map with the parsed properties
         */
        public virtual IDictionary<String, String> ParseBoxValues(String box,
                                                                  String pre, String post) {
            return ParseBoxValues(box, pre, post, null);
        }

        virtual public IDictionary<String, String> ParseBoxValues(String box,
                                                                  String pre, String post, String preKey) {
            String[] props = box.Split(' ');
            int length = props.Length;
            IDictionary<String, String> map = new Dictionary<String, String>(4);
            if (length == 1) {
                String value = props[0];

                if (preKey == null) {
                    map[string.Format(_0_TOP_1, pre, post)] = value;
                    map[string.Format(_0_BOTTOM_1, pre, post)] = value;
                    map[string.Format(_0_RIGHT_1, pre, post)] = value;
                    map[string.Format(_0_LEFT_1, pre, post)] = value;
                } else {
                    map[string.Format(preKey + "{0}", post)] = value;
                }
            } else if (length == 2) {
                if (preKey == null) {
                    map[string.Format(_0_TOP_1, pre, post)] = props[0];
                    map[string.Format(_0_BOTTOM_1, pre, post)] = props[0];
                    map[string.Format(_0_RIGHT_1, pre, post)] = props[1];
                    map[string.Format(_0_LEFT_1, pre, post)] = props[1];
                } else {
                    map[string.Format(preKey + "{0}", post)] = props[0];
                }
            } else if (length == 3) {
                if (preKey == null) {
                    map[string.Format(_0_TOP_1, pre, post)] = props[0];
                    map[string.Format(_0_BOTTOM_1, pre, post)] = props[2];
                    map[string.Format(_0_RIGHT_1, pre, post)] = props[1];
                    map[string.Format(_0_LEFT_1, pre, post)] = props[1];
                } else {
                    map[string.Format(preKey + "{0}", post)] = props[0];
                }
            } else if (length == 4) {
                if (preKey == null) {
                    map[string.Format(_0_TOP_1, pre, post)] = props[0];
                    map[string.Format(_0_BOTTOM_1, pre, post)] = props[2];
                    map[string.Format(_0_RIGHT_1, pre, post)] = props[1];
                    map[string.Format(_0_LEFT_1, pre, post)] = props[3];
                } else {
                    map[string.Format(preKey + "{0}", post)] = props[0];
                }
            }
            return map;
        }

        private static IDictionary<String,object> borderwidth = new Dictionary<String,object>();
        private static string[] bwc = { CSS.Value.THIN, CSS.Value.MEDIUM, CSS.Value.THICK }; //  thin = 1px, medium = 3px, thick = 5px
        private static IDictionary<String,object> borderstyle = new Dictionary<String,object>();
        private static string[] bsc = { CSS.Value.NONE, CSS.Value.HIDDEN, CSS.Value.DOTTED, CSS.Value.DASHED, CSS.Value.SOLID, CSS.Value.DOUBLE, CSS.Value.GROOVE, CSS.Value.RIDGE, CSS.Value.INSET, CSS.Value.OUTSET};
        private static IDictionary<String,object> backgroundPositions = new Dictionary<String,object>();
        private static string[] bgc = { CSS.Value.LEFT, CSS.Value.CENTER, CSS.Value.BOTTOM, CSS.Value.TOP, CSS.Value.RIGHT };
        
        static CssUtils() {
            foreach (string s in bwc) {
                borderwidth[s] = null;
            }
            foreach (string s in bsc) {
                borderstyle[s] = null;
            }
            foreach (string s in bgc) {
                backgroundPositions[s] = null;
            }
        }

        public static char[] whitespace = {' ','\t','\n','\f','\v','\r'};

        public static void MapPutAll(IDictionary<String, String> dest, IDictionary<String, String> src) {
            foreach (KeyValuePair<String, String> kv in src) {
                dest[kv.Key] = kv.Value;
            }
        }

        public static void MapPutAll(IDictionary<String, object> dest, IDictionary<String, object> src) {
            foreach (KeyValuePair<String, object> kv in src) {
                dest[kv.Key] = kv.Value;
            }
        }

        /**
         * @param border
         *            the border property
         * @return a map of the border property parsed to each property (width,
         *         style, color).
         */

        public virtual IDictionary<String, String> ParseBorder(String border) {
            return ParseBorder(border, null);
        }

        public virtual IDictionary<String, String> ParseBorder(String border, String borderKey) {
            Dictionary<String, String> map = new Dictionary<String, String>(0);
            String[] split = SplitComplexCssStyle(border);
            int length = split.Length;
            if (length == 1) {
                if (borderwidth.ContainsKey(split[0]) || IsNumericValue(split[0]) || IsMetricValue(split[0])) {
                    MapPutAll(map, ParseBoxValues(split[0], BORDER2, WIDTH, borderKey));
                } else {
                    MapPutAll(map, ParseBoxValues(split[0], BORDER2, STYLE, borderKey));
                }
            } else {
                for (int i = 0 ; i<length ; i++) {
                    String value = split[i];
                    if (borderwidth.ContainsKey(value) || IsNumericValue(value) || IsMetricValue(value)) {
                        MapPutAll(map, ParseBoxValues(value, BORDER2, WIDTH, borderKey));
                    } else if (borderstyle.ContainsKey(value)){
                        MapPutAll(map, ParseBoxValues(value, BORDER2, STYLE, borderKey));
                    } else if (value.Contains("rgb(") || value.Contains("#") || WebColors.NAMES.ContainsKey(value.ToLowerInvariant())){
                        MapPutAll(map, ParseBoxValues(value, BORDER2, COLOR, borderKey));
                    }
                }
            }
            return map;
        }

        /**
         * Trims and Strips double spaces from the given string.
         *
         * @param str
         *            the string to strip
         * @return the string without double spaces
         */
        virtual public String StripDoubleSpacesAndTrim(String str) {
            char[] charArray = str.ToCharArray();
            if (str.Contains("  ")) {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < charArray.Length; i++) {
                    char c = charArray[i];
                    if (c != ' ') {
                        builder.Append(c);
                    } else {
                        if (i + 1 < charArray.Length && charArray[i + 1] != ' ') {
                            builder.Append(' ');
                        }
                    }
                }
                return builder.ToString().Trim();
            } else {
                return str.Trim();
            }
        }

        virtual public String StripDoubleSpacesTrimAndToLowerCase(String str) {
            return StripDoubleSpacesAndTrim(str).ToLower();
        }

        /**
         * Preparation before implementing the background style in iText. Splits the
         * given background style and its attributes into background-color,
         * background-image, background-repeat, background-attachment,
         * background-position and css styles.
         *
         * @param background
         *            the string containing the font style value.
         * @return a map with the values of font parsed into each css property.
         */
        virtual public IDictionary<String, String> ProcessBackground(String background) {
            IDictionary<String, String> rules = new Dictionary<String, String>();
            String[] styles = SplitComplexCssStyle(background);
            foreach (String style in styles) {
                if (style.Contains("url(")) {
                    rules[CSS.Property.BACKGROUND_IMAGE] = style;
                } else if (Util.EqualsIgnoreCase(style, CSS.Value.REPEAT)
                        || Util.EqualsIgnoreCase(style, CSS.Value.NO_REPEAT)
                        || Util.EqualsIgnoreCase(style, CSS.Value.REPEAT_X)
                        || Util.EqualsIgnoreCase(style, CSS.Value.REPEAT_Y)) {
                    rules[CSS.Property.BACKGROUND_REPEAT] = style;
                } else if (Util.EqualsIgnoreCase(style, CSS.Value.FIXED) || Util.EqualsIgnoreCase(style, CSS.Value.SCROLL)) {
                    rules[CSS.Property.BACKGROUND_ATTACHMENT] = style;
                } else if (backgroundPositions.ContainsKey(style)) {
                    if (!rules.ContainsKey(CSS.Property.BACKGROUND_POSITION)) {
                        rules[CSS.Property.BACKGROUND_POSITION] = style;
                    } else {
                        string style2 = style+" "+rules[CSS.Property.BACKGROUND_POSITION];
                        rules[CSS.Property.BACKGROUND_POSITION] = style2;
                    }
                } else if (IsNumericValue(style) || IsMetricValue(style) || IsRelativeValue(style)) {
                    if (!rules.ContainsKey(CSS.Property.BACKGROUND_POSITION)) {
                        rules[CSS.Property.BACKGROUND_POSITION] = style;
                    } else {
                        string style2 = style+" "+rules[CSS.Property.BACKGROUND_POSITION];
                        rules[CSS.Property.BACKGROUND_POSITION] = style2;
                    }
                } else if (style.Contains("rgb(") || style.Contains("rgba(")|| style.Contains("#") || WebColors.NAMES.ContainsKey(style.ToLowerInvariant())) {
                    rules[CSS.Property.BACKGROUND_COLOR] = style;
                }
            }
            return rules;
        }
        /**
         * Preparation before implementing the list style in iText. Splits the given
         * list style and its attributes into list-style-type, list-style-position and list-style-image.
         *
         * @param listStyle the string containing the list style value.
         * @return a map with the values of the parsed list style into each css property.
         */
        virtual public IDictionary<String, String> ProcessListStyle(String listStyle) {
            IDictionary<String, String> rules = new Dictionary<String, String>();
            String[] styles = SplitComplexCssStyle(listStyle);
            foreach (String style in styles) {
                if (Util.EqualsIgnoreCase(style, CSS.Value.DISC)
                        || Util.EqualsIgnoreCase(style, CSS.Value.SQUARE)
                        || Util.EqualsIgnoreCase(style, CSS.Value.CIRCLE)
                        || Util.EqualsIgnoreCase(style, CSS.Value.LOWER_ROMAN)
                        || Util.EqualsIgnoreCase(style, CSS.Value.UPPER_ROMAN)
                        || Util.EqualsIgnoreCase(style, CSS.Value.LOWER_GREEK)
                        || Util.EqualsIgnoreCase(style, CSS.Value.UPPER_GREEK)
                        || Util.EqualsIgnoreCase(style, CSS.Value.LOWER_ALPHA)
                        || Util.EqualsIgnoreCase(style, CSS.Value.UPPER_ALPHA)
                        || Util.EqualsIgnoreCase(style, CSS.Value.LOWER_LATIN)
                        || Util.EqualsIgnoreCase(style, CSS.Value.UPPER_LATIN)) {
                    rules[CSS.Property.LIST_STYLE_TYPE] = style;
                } else if (Util.EqualsIgnoreCase(style, CSS.Value.INSIDE) || Util.EqualsIgnoreCase(style, CSS.Value.OUTSIDE)) {
                    rules[CSS.Property.LIST_STYLE_POSITION] = style;
                } else if (style.Contains("url(")) {
                    rules[CSS.Property.LIST_STYLE_IMAGE] = style;
                }
            }
            return rules;
        }
        /**
         * Preparation before implementing the font style in iText. Splits the given
         * font style and its attributes into font-size, line-height,
         * font-weight, font-style, font-variant and font-family css styles.
         *
         * @param font the string containing the font style value.
         * @return a map with the values of the parsed font into each css property.
         */
        virtual public IDictionary<String, String> ProcessFont(String font) {
            IDictionary<String, String> rules = new Dictionary<String, String>();
            String[] styleAndRest = font.Split(whitespace);

            for (int i = 0; i < styleAndRest.Length; i++)
            {
                String style = styleAndRest[i];
                if (Util.EqualsIgnoreCase(style, HtmlTags.ITALIC) || Util.EqualsIgnoreCase(style, HtmlTags.OBLIQUE))
                {
                    rules[HtmlTags.FONTSTYLE] = style;
                } 
                else if (Util.EqualsIgnoreCase(style, "small-caps"))
                {
                    rules["font-variant"] = style;
                }
                else if (Util.EqualsIgnoreCase(style, HtmlTags.BOLD))
                {
                    rules[HtmlTags.FONTWEIGHT] = style;
                }
                else if (IsMetricValue(style) || IsNumericValue(style))
                {
                    if (style.Contains("/"))
                    {
                        String[] sizeAndLineHeight = style.Split('/');
                        style = sizeAndLineHeight[0]; // assuming font-size always is the first parameter
                        rules[HtmlTags.LINEHEIGHT] = sizeAndLineHeight[1];
                    }
                    rules[HtmlTags.FONTSIZE] = style;
                    if (i != styleAndRest.Length - 1)
                    {
                        string rest = styleAndRest[i + 1];
                        rest = rest.Replace("\"", "");
                        rest = rest.Replace("'", "");
                        rules[HtmlTags.FONTFAMILY] = rest;
                    }
                }
            }
            return rules;
        }

        /**
         * Use only if value of style is a metric value ({@link CssUtils#isMetricValue(String)}) or a numeric value in pixels ({@link CssUtils#isNumericValue(String)}).<br />
         * Checks if the style is present in the css of the tag, then parses it to pt. and returns the parsed value.
         * @param t the tag which needs to be checked.
         * @param style the style which needs to be checked.
         * @return float the parsed value of the style or 0f if the value was invalid.
         */
        virtual public float CheckMetricStyle(Tag t, String style) {
            float? metricValue = CheckMetricStyle(t.CSS, style);
            if (metricValue != null) {
                return (float)metricValue;
            } else {
                return 0f;
            }
        }
        /**
         * Use only if value of style is a metric value ({@link CssUtils#isMetricValue(String)}) or a numeric value in pixels ({@link CssUtils#isNumericValue(String)}).<br />
         * Checks if the style is present in the css of the tag, then parses it to pt. and returns the parsed value.
         * @param css the map of css styles which needs to be checked.
         * @param style the style which needs to be checked.
         * @return float the parsed value of the style or 0f if the value was invalid.
         */
        virtual public float? CheckMetricStyle(IDictionary<String,String> css, String style) {
            String value;
            css.TryGetValue(style, out value);
            if (value != null && (IsMetricValue(value) || IsNumericValue(value))) {
                return ParsePxInCmMmPcToPt(value);
            }
            return null;
        }

        /**
         * Checks whether a string contains an allowed metric unit in HTML/CSS; px, in, cm, mm, pc or pt.
         * @param value the string that needs to be checked.
         * @return bool true if value contains an allowed metric value.
         */
        virtual public bool IsMetricValue(String value) {
            return value.Contains(CSS.Value.PX) || value.Contains(CSS.Value.IN) || value.Contains(CSS.Value.CM)
                || value.Contains(CSS.Value.MM) || value.Contains(CSS.Value.PC) || value.Contains(CSS.Value.PT);

        }
        /**
         * Checks whether a string contains an allowed value relative to previously set value.
         * @param value the string that needs to be checked.
         * @return bool true if value contains an allowed metric value.
         */
        virtual public bool IsRelativeValue(String value) {
            return value.Contains(CSS.Value.PERCENTAGE) || value.Contains(CSS.Value.EM) || value.Contains(CSS.Value.EX);

        }

        private static Regex numerics1 = new Regex(@"^-?\d+(\.\d*)?$", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private static Regex numerics2 = new Regex(@"^-?\.\d+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /**
         * Checks whether a string matches a numeric value (e.g. 123, 1.23, .123). All these metric values are allowed in HTML/CSS.
         * @param value the string that needs to be checked.
         * @return bool true if value contains an allowed metric value.
         */
        virtual public bool IsNumericValue(String value) {
            return numerics1.IsMatch(value) || numerics2.IsMatch(value);

        }
        /**
         * Convenience method for parsing a value to pt if a value can contain: <br />
         * <ul>
         *  <li>a numeric value in pixels (e.g. 123, 1.23, .123),</li>
         *  <li>a value with a metric unit (px, in, cm, mm, pc or pt) attached to it,</li>
         *  <li>or a value with a relative value (%, em, ex).</li>
         * </ul>
         * <b>Note:</b> baseValue must be in pt.<br /><br />
         * @param value the string containing the value to be parsed.
         * @param baseValue float needed for the calculation of the relative value.
         * @return parsedValue float containing the parsed value in pt.
         */
        virtual public float ParseValueToPt(String value, float baseValue) {
            float parsedValue = 0;
            if (IsMetricValue(value) || IsNumericValue(value)) {
                parsedValue = ParsePxInCmMmPcToPt(value);
            } else if (IsRelativeValue(value)) {
                parsedValue = ParseRelativeValue(value, baseValue);
            }
            return parsedValue;
        }
        /**
         * Parses an relative value based on the base value that was given, in the metric unit of the base value. <br />
         * (e.g. margin=10% should be based on the page width, so if an A4 is used, the margin = 0.10*595.0 = 59.5f)
         * @param relativeValue in %, em or ex.
         * @param baseValue the value the returned float is based on.
         * @return the parsed float in the metric unit of the base value.
         */
        virtual public float ParseRelativeValue(String relativeValue, float baseValue) {
            int pos = DeterminePositionBetweenValueAndUnit(relativeValue);
            if (pos == 0)
                return 0f;
            float f = float.Parse(relativeValue.Substring(0, pos), CultureInfo.InvariantCulture);
            String unit = relativeValue.Substring(pos);
            if (unit.StartsWith("%")) {
                f = baseValue * f / 100;
            } else if (unit.StartsWith("em")) {
                f = baseValue * f;
            } else if (unit.Contains("ex")) {
                f = baseValue * f / 2;
            }
            return f;
        }

        /**
         * Parses a length with an allowed metric unit (px, pt, in, cm, mm, pc, em or ex) or numeric value (e.g. 123, 1.23, .123) to pt.<br />
         * A numeric value (without px, pt, etc in the given length string) is considered to be in the default metric that was given.
         * @param length the string containing the length.
         * @param defaultMetric the string containing the metric if it is possible that the length string does not contain one. If null the length is considered to be in px as is default in HTML/CSS.
         * @return
         */
        virtual public float ParsePxInCmMmPcToPt(String length, String defaultMetric) {
            int pos = DeterminePositionBetweenValueAndUnit(length);
            if (pos == 0)
                return 0f;
            float f = float.Parse(length.Substring(0, pos), CultureInfo.InvariantCulture);
            String unit = length.Substring(pos);
            // inches
            if (unit.StartsWith(CSS.Value.IN) || (unit.Equals("") && defaultMetric.Equals(CSS.Value.IN))) {
                f *= 72f;
            }
            // centimeters
            else if (unit.StartsWith(CSS.Value.CM) || (unit.Equals("") && defaultMetric.Equals(CSS.Value.CM))) {
                f = (f / 2.54f) * 72f;
            }
            // millimeters
            else if (unit.StartsWith(CSS.Value.MM) || (unit.Equals("") && defaultMetric.Equals(CSS.Value.MM))) {
                f = (f / 25.4f) * 72f;
            }
            // picas
            else if (unit.StartsWith(CSS.Value.PC) || (unit.Equals("") && defaultMetric.Equals(CSS.Value.PC))) {
                f *= 12f;
            }
            // pixels (1px = 0.75pt).
            else if (unit.StartsWith(CSS.Value.PX) || (unit.Equals("") && defaultMetric.Equals(CSS.Value.PX))) {
                f *= 0.75f;
            }
            return f;
        }

        /**
         * Parses a length with an allowed metric unit (px, pt, in, cm, mm, pc, em or ex) or numeric value (e.g. 123, 1.23, .123) to pt.<br />
         * A numeric value is considered to be in px as is default in HTML/CSS.
         * @param length the string containing the length.
         * @return float the parsed length in pt.
         */
        virtual public float ParsePxInCmMmPcToPt(String length) {
            return ParsePxInCmMmPcToPt(length, CSS.Value.PX);
        }

        /**
         * Method used in preparation of splitting a string containing a numeric value with a metric unit (e.g. 18px, 9pt, 6cm, etc).<br /><br />
         * Determines the position between digits and affiliated characters ('+','-','0-9' and '.') and all other characters.<br />
         * e.g. string "16px" will return 2, string "0.5em" will return 3 and string '-8.5mm' will return 4.
         *
         * @param string containing a numeric value with a metric unit
         * @return int position between the numeric value and unit or 0 if string is null or string started with a non-numeric value.
         */
        virtual public int DeterminePositionBetweenValueAndUnit(String str) {
            if (str == null)
                return 0;
            int pos = 0;
            bool ok = true;
            while (ok && pos < str.Length) {
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
            return pos;
        }
        /**
         * Returns the sum of the left and right margin of a tag.
         * @param t the tag of which the total horizontal margin is needed.
         * @param pageWidth the page width
         * @return float the total horizontal margin.
         */
        virtual public float GetLeftAndRightMargin(Tag t, float pageWidth) {
            float horizontalMargin = 0;
            String value;
            t.CSS.TryGetValue(CSS.Property.MARGIN_LEFT, out value);
            if (value != null) {
                horizontalMargin += ParseValueToPt(value, pageWidth);
            }
            t.CSS.TryGetValue(CSS.Property.MARGIN_RIGHT, out value);
            if (value != null) {
                horizontalMargin += ParseValueToPt(value, pageWidth);
            }
            return horizontalMargin;
        }

        /**
         * Parses <code>url("file.jpg")</code> to <code>file.jpg</code>.
         * @param url the url attribute to parse
         * @return the parsed url. Or original url if not wrappend in Url()
         */
        virtual public String ExtractUrl(String url) {
            String str = null;
            if (url.StartsWith("url")) {
                String urlString = url.Substring(3).Trim().Replace("(", "").Replace(")", "").Trim();
                if (urlString.StartsWith("'") && urlString.EndsWith("'")) {
                    int st = urlString.IndexOf("'")+1;
                    str = urlString.Substring(st, urlString.LastIndexOf("'") - st);
                } else if (urlString.StartsWith("\"") && urlString.EndsWith("\"")) {
                    int st = urlString.IndexOf('"') + 1;
                    str = urlString.Substring(st, urlString.LastIndexOf('"') - st);
                } else {
                    str = urlString;
                }
            } else {
                // assume it's an url without url
                str = url;
            }
            return str;
        }

        /**
         * Validates a given textHeight based on the content of a tag against the css styles "min-height" and "max-height" of the tag if present.
         *
         * @param css the styles of a tag
         * @param textHeight the current textHeight based on the content of a tag
         * @return the text height of an element.
         */
        virtual public float ValidateTextHeight(IDictionary<String, String> css,
                float textHeight) {
            if (css.ContainsKey("min-height") && textHeight < new CssUtils().ParsePxInCmMmPcToPt(css["min-height"])) {
                textHeight = new CssUtils().ParsePxInCmMmPcToPt(css["min-height"]);
            } else if (css.ContainsKey("max-height") && textHeight > new CssUtils().ParsePxInCmMmPcToPt(css["max-height"])) {
                textHeight = new CssUtils().ParsePxInCmMmPcToPt(css["max-height"]);
            }
            return textHeight;
        }

        /**
         * Calculates the margin top or spacingBefore based on the given value and the last margin bottom.
         * <br /><br />
         * In HTML the margin-bottom of a tag overlaps with the margin-top of a following tag.
         * This method simulates this behavior by subtracting the margin-top value of the given tag from the margin-bottom of the previous tag. The remaining value is returned or if the margin-bottom value is the largest, 0 is returned
         * @param value the margin-top value of the given tag.
         * @param largestFont used if a relative value was given to calculate margin.
         * @param configuration XmlWorkerConfig containing the last margin bottom.
         * @return an offset
         */
        virtual public float CalculateMarginTop(String value, float largestFont, IMarginMemory configuration) {
            return CalculateMarginTop(ParseValueToPt(value, largestFont), configuration);
        }

        /**
         * Calculates the margin top or spacingBefore based on the given value and the last margin bottom.
         * <br /><br />
         * In HTML the margin-bottom of a tag overlaps with the margin-top of a following tag.
         * This method simulates this behavior by subtracting the margin-top value of the given tag from the margin-bottom of the previous tag. The remaining value is returned or if the margin-bottom value is the largest, 0 is returned
         * @param value float containing the margin-top value.
         * @param configuration XmlWorkerConfig containing the last margin bottom.
         * @return an offset
         */
        virtual public float CalculateMarginTop(float value, IMarginMemory configuration) {
            float marginTop = value;
            try {
                float marginBottom = configuration.LastMarginBottom;
                marginTop = (marginTop>marginBottom)?marginTop-marginBottom:0;
            } catch (NoDataException) {
            }
            return marginTop;
        }

        /**
         * Trims a string and removes surrounding " or '.
         *
         * @param s the string
         * @return trimmed and unquoted string
         */

        virtual public String TrimAndRemoveQuoutes(String s) {
            s = s.Trim();
            if ((s.StartsWith("\"") || s.StartsWith("'")) && s.EndsWith("\"") || s.EndsWith("'")) {
                s = s.Substring(1, s.Length - 2);
            }
            return s;
        }

        virtual public String[] SplitComplexCssStyle(String s) {
            s = Regex.Replace(s, "\\s*,\\s*", ",");
            return Regex.Split(s, "\\s");
        }
    }
}
