using System;
using System.Collections.Generic;
using System.Globalization;
using System.util;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
/*
 * $Id: ChunkCssApplier.java 108 2011-05-26 12:11:05Z emielackermann $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: Balder Van Camp, Emiel Ackermann, et al.
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
namespace iTextSharp.tool.xml.css.apply {

    /**
     * Applies CSS Rules to Chunks
     *
     */
    public class ChunkCssApplier {
        /**
         * FF4 and IE8 provide normal text and bold text. All other values are translated to one of these 2 styles <br />
         * 100 - 500 and "lighter" = normal.<br />
         * 600 - 900 and "bolder" = bold.
         */
        public static readonly IList<String> BOLD = new List<string>(new String[] { "bold", "bolder", "600", "700", "800", "900" });
        private CssUtils utils = CssUtils.GetInstance();

         /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.css.CssApplier#apply(com.itextpdf.text.Element,
         * com.itextpdf.tool.xml.Tag)
         */
        public Chunk Apply(Chunk c, Tag t) {
            String fontName = null;
            String encoding = BaseFont.CP1252;
            float size = new FontSizeTranslator().GetFontSize(t);
            int style = Font.UNDEFINED;
            BaseColor color = null;
            IDictionary<String, String> rules = t.CSS;
            foreach (KeyValuePair<String, String> entry in rules) {
                String key = entry.Key;
                String value = entry.Value;
                if (Util.EqualsIgnoreCase(CSS.Property.FONT_WEIGHT, key)) {
                    if (CSS.Value.BOLD.Contains(value)) {
                        if (style == Font.ITALIC) {
                            style = Font.BOLDITALIC;
                        }
                        else {
                            style = Font.BOLD;
                        }
                    }
                    else {
                        if (style == Font.BOLDITALIC) {
                            style = Font.ITALIC;
                        } else {
                            style = Font.NORMAL;
                        }
                    }
                } else if (Util.EqualsIgnoreCase(CSS.Property.FONT_STYLE, key)) {
                    if (Util.EqualsIgnoreCase(value, CSS.Value.ITALIC)) {
                        if (style == Font.BOLD)
                            style = Font.BOLDITALIC;
                        else
                            style = Font.ITALIC;
                    }
                    if (Util.EqualsIgnoreCase(value, CSS.Value.OBLIQUE)) {
                        c.SetSkew(0, 12);
                    }
                } else if (Util.EqualsIgnoreCase(CSS.Property.FONT_FAMILY, key)) {
                    if (value.Contains(",")){
                        String[] fonts = value.Split(',');
                        foreach (String s in fonts) {
                            string s2 = s.Trim();
                            if (!Util.EqualsIgnoreCase(FontFactory.GetFont(s2).Familyname, "unknown")){
                                fontName = s2;
                                break;
                            }
                        }
                    } else {
                        fontName = value;
                    }
                } else if (Util.EqualsIgnoreCase(CSS.Property.COLOR, key)) {
                    color = HtmlUtilities.DecodeColor(value);
                } else if (Util.EqualsIgnoreCase(CSS.Property.LETTER_SPACING, key)) {
                    c.SetCharacterSpacing(utils.ParsePxInCmMmPcToPt(value));
                } else if (rules.ContainsKey(CSS.Property.XFA_FONT_HORIZONTAL_SCALE)) { // only % allowed; need a catch block NumberFormatExc?
                    c.SetHorizontalScaling(float.Parse(rules[CSS.Property.XFA_FONT_HORIZONTAL_SCALE].Replace("%", ""), CultureInfo.InvariantCulture)/100f);
                }
            }
            // following styles are separate from the for each loop, because they are based on font settings like size.
            if (rules.ContainsKey(CSS.Property.VERTICAL_ALIGN)) {
                String value = rules[CSS.Property.VERTICAL_ALIGN];
                if (Util.EqualsIgnoreCase(value, CSS.Value.SUPER)||Util.EqualsIgnoreCase(value, CSS.Value.TOP)||Util.EqualsIgnoreCase(value, CSS.Value.TEXT_TOP)) {
                    c.SetTextRise((float) (size / 2 + 0.5));
                } else if (Util.EqualsIgnoreCase(value, CSS.Value.SUB)||Util.EqualsIgnoreCase(value, CSS.Value.BOTTOM)||Util.EqualsIgnoreCase(value, CSS.Value.TEXT_BOTTOM)) {
                    c.SetTextRise(-size / 2);
                } else {
                    c.SetTextRise(utils.ParsePxInCmMmPcToPt(value));
                }
            }
            String xfaVertScale;
            rules.TryGetValue(CSS.Property.XFA_FONT_VERTICAL_SCALE, out xfaVertScale);
            if (null != xfaVertScale) { // only % allowed; need a catch block NumberFormatExc?
                if (xfaVertScale.Contains("%")) {
                    size *= float.Parse(xfaVertScale.Replace("%", ""), CultureInfo.InvariantCulture)/100;
                    c.SetHorizontalScaling(100/float.Parse(xfaVertScale.Replace("%", ""), CultureInfo.InvariantCulture));
                }
            }
            if (rules.ContainsKey(CSS.Property.TEXT_DECORATION)) { // Restriction? In html a underline and a line-through is possible on one piece of text. A Chunk can set an underline only once.
                String value = rules[CSS.Property.TEXT_DECORATION];
                if (Util.EqualsIgnoreCase(CSS.Value.UNDERLINE, value)) {
                    c.SetUnderline(0.75f, -size/8f);
                }
                if (Util.EqualsIgnoreCase(CSS.Value.LINE_THROUGH, value)) {
                    c.SetUnderline(0.75f, size/4f);
                }
            }
            if (rules.ContainsKey(CSS.Property.BACKGROUND_COLOR)) {
                c.SetBackground(HtmlUtilities.DecodeColor(rules[CSS.Property.BACKGROUND_COLOR]));
            }
            Font f  = FontFactory.GetFont(fontName, encoding, BaseFont.EMBEDDED, size, style, color);
            c.Font = f;
            return c;
        }
        /**
         * Method used for retrieving the widest word of a chunk of text. All styles of the chunk will be taken into account when calculating the width of the words.
         * @param c chunk of which the widest word is required.
         * @return float containing the width of the widest word.
         */
        public float GetWidestWord(Chunk c) {
            String[] words = c.Content.Split(CssUtils.whitespace);
            float widestWord = 0;
            for (int i = 0; i<words.Length ; i++) {
                Chunk word = new Chunk(words[i]);
                CopyChunkStyles(c, word);
                if (word.GetWidthPoint() > widestWord) {
                    widestWord = word.GetWidthPoint();
                }
            }
            return widestWord;
        }
        /**
         * Method used for copying styles from one chunk to another. Could be deprecated if the content of a chunk can be overwritten.
         * @param source chunk which contains the required styles.
         * @param target chunk which needs the required styles.
         */
        public void CopyChunkStyles(Chunk source, Chunk target) {
            target.Font = source.Font;
            target.Attributes = source.Attributes;
            target.SetCharacterSpacing(source.GetCharacterSpacing());
            target.SetHorizontalScaling(source.HorizontalScaling);
        }
    }
}