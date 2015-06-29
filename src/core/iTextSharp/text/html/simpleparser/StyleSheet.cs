using System;
using System.Collections.Generic;
using System.Globalization;
using System.util;
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

namespace iTextSharp.text.html.simpleparser {
    /**
     * Old class to define styles for HTMLWorker.
     * We've completely rewritten HTML to PDF functionality; see project XML Worker.
     * XML Worker is able to parse CSS files and "style" attribute values.
     * @deprecated since 5.5.2
     */
    [Obsolete]
    public class StyleSheet {
        
        /**
         * IDictionary storing tags and their corresponding styles.
         * @since 5.0.6 (changed Dictionary => IDictionary)
         */
        protected internal IDictionary<String, IDictionary<String, String>> tagMap = new Dictionary<String, IDictionary<String, String>>();
        
        /**
         * IDictionary storing possible names of the "class" attribute
         * and their corresponding styles.
         * @since 5.0.6 (changed Dictionary => IDictionary)
         */
        protected internal IDictionary<String, IDictionary<String, String>> classMap = new Dictionary<String, IDictionary<String, String>>();

        /**
         * Creates a new instance of StyleSheet
         */
        public StyleSheet() {
        }

        /**
         * Associates a IDictionary containing styles with a tag.
         * @param   tag     the name of the HTML/XML tag
         * @param   attrs   a map containing styles
         */
        virtual public void LoadTagStyle(String tag, IDictionary<String, String> attrs) {
            tagMap[tag.ToLowerInvariant()] = attrs;
        }

        /**
         * Adds an extra style key-value pair to the styles IDictionary
         * of a specific tag
         * @param   tag     the name of the HTML/XML tag
         * @param   key     the key specifying a specific style
         * @param   value   the value defining the style
         */
        virtual public void LoadTagStyle(String tag, String key, String value) {
            tag = tag.ToLowerInvariant();
            IDictionary<String, String> styles;
            tagMap.TryGetValue(tag, out styles);
            if (styles == null) {
                styles = new Dictionary<String, String>();
                tagMap[tag] = styles;
            }
            styles[key] = value;
        }

        /**
         * Associates a IDictionary containing styles with a class name.
         * @param   className   the value of the class attribute
         * @param   attrs       a map containing styles
         */
        virtual public void LoadStyle(String className, Dictionary<String, String> attrs) {
            classMap[className.ToLowerInvariant()] = attrs;
        }

        /**
         * Adds an extra style key-value pair to the styles IDictionary
         * of a specific tag
         * @param   className   the name of the HTML/XML tag
         * @param   key         the key specifying a specific style
         * @param   value       the value defining the style
         */
        virtual public void LoadStyle(String className, String key, String value) {
            className = className.ToLowerInvariant();
            IDictionary<String, String> styles;
            classMap.TryGetValue(className, out styles);
            if (styles == null) {
                styles = new Dictionary<String, String>();
                classMap[className] = styles;
            }
            styles[key] = value;
        }

        /**
         * Resolves the styles based on the tag name and the value
         * of the class attribute.
         * @param   tag     the tag that needs to be resolved
         * @param   attrs   existing style map that will be updated
         */
        virtual public void ApplyStyle(String tag, IDictionary<String, String> attrs) {
            // first fetch the styles corresponding with the tag name
            IDictionary<String, String> map;
            tagMap.TryGetValue(tag.ToLowerInvariant(), out map);
            if (map != null) {
                // create a new map with properties
                IDictionary<String, String> tempi = new Dictionary<String, String>(map);
                // override with the existing properties
                foreach (KeyValuePair<string,string> kv in attrs)
                    tempi[kv.Key] = kv.Value;
                // update the existing properties
                foreach (KeyValuePair<string,string> kv in tempi)
                    attrs[kv.Key] = kv.Value;
            }
            // look for the class attribute
            String cm;
            attrs.TryGetValue(HtmlTags.CLASS, out cm);
            if (cm == null)
                return;
            // fetch the styles corresponding with the class attribute
            classMap.TryGetValue(cm.ToLowerInvariant(), out map);
            if (map == null)
                return;
            // remove the class attribute from the properties
            attrs.Remove(HtmlTags.CLASS);
            // create a map with the styles corresponding with the class value
            IDictionary<String, String> temp = new Dictionary<String, String>(map);
            // override with the existing properties
            foreach (KeyValuePair<string,string> kv in attrs)
                temp[kv.Key] = kv.Value;
            // update the properties
            foreach (KeyValuePair<string,string> kv in temp)
                attrs[kv.Key] = kv.Value;
        }

        /**
         * Method contributed by Lubos Strapko
         * @param h
         * @param chain
         * @since 2.1.3
         */
        public static void ResolveStyleAttribute(IDictionary<String, String> h, ChainedProperties chain) {
            String style;
            h.TryGetValue(HtmlTags.STYLE, out style);
            if (style == null)
                return;
            Properties prop = HtmlUtilities.ParseAttributes(style);
            foreach (string key in prop.Keys) {
                if (key.Equals(HtmlTags.FONTFAMILY)) {
                    h[HtmlTags.FACE] = prop[key];
                } else if (key.Equals(HtmlTags.FONTSIZE)) {
                    float actualFontSize = HtmlUtilities.ParseLength(chain[HtmlTags.SIZE],
                            HtmlUtilities.DEFAULT_FONT_SIZE);
                    if (actualFontSize <= 0f)
                        actualFontSize = HtmlUtilities.DEFAULT_FONT_SIZE;
                    h[HtmlTags.SIZE] = HtmlUtilities.ParseLength(prop[key], actualFontSize).ToString(CultureInfo.InvariantCulture) + "pt";
                } else if (key.Equals(HtmlTags.FONTSTYLE)) {
                    String ss = prop[key].Trim().ToLowerInvariant();
                    if (ss.Equals(HtmlTags.ITALIC) || ss.Equals(HtmlTags.OBLIQUE))
                        h[HtmlTags.I] = null;
                } else if (key.Equals(HtmlTags.FONTWEIGHT)) {
                    String ss = prop[key].Trim().ToLowerInvariant();
                    if (ss.Equals(HtmlTags.BOLD) || ss.Equals("700") || ss.Equals("800")
                            || ss.Equals("900"))
                        h[HtmlTags.B] = null;
                } else if (key.Equals(HtmlTags.TEXTDECORATION)) {
                    String ss = prop[key].Trim().ToLowerInvariant();
                    if (ss.Equals(HtmlTags.UNDERLINE))
                        h[HtmlTags.U] = null;
                } else if (key.Equals(HtmlTags.COLOR)) {
                    BaseColor c = HtmlUtilities.DecodeColor(prop[key]);
                    if (c != null) {
                        int hh = c.ToArgb();
                        String hs = hh.ToString("x");
                        hs = "000000" + hs;
                        hs = "#" + hs.Substring(hs.Length - 6);
                        h[HtmlTags.COLOR] = hs;
                    }
                } else if (key.Equals(HtmlTags.LINEHEIGHT)) {
                    String ss = prop[key].Trim();
                    float actualFontSize = HtmlUtilities.ParseLength(chain[HtmlTags.SIZE],
                            HtmlUtilities.DEFAULT_FONT_SIZE);
                    if (actualFontSize <= 0f)
                        actualFontSize = HtmlUtilities.DEFAULT_FONT_SIZE;
                    float v = HtmlUtilities.ParseLength(prop[key],
                            actualFontSize);
                    if (ss.EndsWith("%")) {
                        h[HtmlTags.LEADING] = "0," + v / 100;
                        return;
                    }
                    if (Util.EqualsIgnoreCase(HtmlTags.NORMAL, ss)) {
                        h[HtmlTags.LEADING] = "0,1.5";
                        return;
                    }
                    h[HtmlTags.LEADING] = v + ",0";
                } else if (key.Equals(HtmlTags.TEXTALIGN)) {
                    String ss = prop[key].Trim().ToLowerInvariant();
                    h[HtmlTags.ALIGN] = ss;
                } else if (key.Equals(HtmlTags.PADDINGLEFT)) {
                    String ss = prop[key].Trim().ToLowerInvariant();
                    h[HtmlTags.INDENT] = HtmlUtilities.ParseLength(ss).ToString(CultureInfo.InvariantCulture);
                }
            }
        }
    }
}
