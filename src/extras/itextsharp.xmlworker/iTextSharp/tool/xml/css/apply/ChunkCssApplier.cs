/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2017 iText Group NV
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
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.util;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.html;

namespace iTextSharp.tool.xml.css.apply {

/**
 * Applies CSS Rules to Chunks
 */

    public class ChunkCssApplier : CssApplier<Chunk> {
        /**
         * FF4 and IE8 provide normal text and bold text. All other values are translated to one of these 2 styles <br />
         * 100 - 500 and "lighter" = normal.<br />
         * 600 - 900 and "bolder" = bold.
         */
        public static IList<String> BOLD = new List<string>(new String[] {"bold", "bolder", "600", "700", "800", "900"});
        protected CssUtils utils = CssUtils.GetInstance();
        protected IFontProvider fontProvider;

        public ChunkCssApplier() : this(null)
        {
        }

        public ChunkCssApplier(IFontProvider fontProvider)
        {
            if (fontProvider != null)
            {
                this.fontProvider = fontProvider;
            }
            else
            {
                this.fontProvider = new FontFactoryImp();
            }
        }

        public virtual Chunk Apply(Chunk c, Tag t)
        {
            return (Chunk) Apply(c, t, null, null, null);
        }

        /**
         *
         * @param c the Chunk to apply CSS to.
         * @param t the tag containing the chunk data
         * @return the styled chunk
         */

        public override Chunk Apply(Chunk c, Tag t, IMarginMemory mm, IPageSizeContainable psc, HtmlPipelineContext ctx) {
            Font f = ApplyFontStyles(t);
            float size = f.Size;
            String value = null;
            IDictionary<String, String> rules = t.CSS;
            foreach (KeyValuePair<String, String> entry in rules)
            {
                String key = entry.Key;
                value = entry.Value;
                if (Util.EqualsIgnoreCase(CSS.Property.FONT_STYLE, key)) {
                    if (Util.EqualsIgnoreCase(CSS.Value.OBLIQUE, value)) {
                        c.SetSkew(0, 12);
                    }
                } else if (Util.EqualsIgnoreCase(CSS.Property.LETTER_SPACING, key)) {
                    String letterSpacing = entry.Value;
                    float letterSpacingValue = 0f;
                    if (utils.IsRelativeValue(value)) {
                        letterSpacingValue = utils.ParseRelativeValue(letterSpacing, f.Size);
                    } else if (utils.IsMetricValue(value)) {
                        letterSpacingValue = utils.ParsePxInCmMmPcToPt(letterSpacing);
                    }
                    c.SetCharacterSpacing(letterSpacingValue);
                } else if (Util.EqualsIgnoreCase(CSS.Property.XFA_FONT_HORIZONTAL_SCALE, key)) {
                    // only % allowed; need a catch block NumberFormatExc?
                    c.SetHorizontalScaling(
                        float.Parse(value.Replace("%", ""), CultureInfo.InvariantCulture) /100);
                }
            }
            // following styles are separate from the for each loop, because they are based on font settings like size.
            if (rules.TryGetValue(CSS.Property.VERTICAL_ALIGN, out value))
            {
                if (Util.EqualsIgnoreCase(CSS.Value.SUPER, value)
                    || Util.EqualsIgnoreCase(CSS.Value.TOP, value)
                    || Util.EqualsIgnoreCase(CSS.Value.TEXT_TOP, value)) {
                    c.SetTextRise((float) (size/2 + 0.5));
                } else if (Util.EqualsIgnoreCase(CSS.Value.SUB, value)
                    || Util.EqualsIgnoreCase(CSS.Value.BOTTOM, value)
                    || Util.EqualsIgnoreCase(CSS.Value.TEXT_BOTTOM, value)) {
                    c.SetTextRise(-size/2);
                } else {
                    c.SetTextRise(utils.ParsePxInCmMmPcToPt(value));
                }
            }
            String xfaVertScale;
            if (rules.TryGetValue(CSS.Property.XFA_FONT_VERTICAL_SCALE, out xfaVertScale))
            {
                if (xfaVertScale.Contains("%"))
                {
                    size *= float.Parse(xfaVertScale.Replace("%", ""), CultureInfo.InvariantCulture) /100;
                    c.SetHorizontalScaling(100/float.Parse(xfaVertScale.Replace("%", ""), CultureInfo.InvariantCulture));
                }
            }
            if (rules.TryGetValue(CSS.Property.TEXT_DECORATION, out value)) {
                String[] splitValues = new Regex(@"\s+").Split(value);
                foreach (String curValue in splitValues) {
                    if (Util.EqualsIgnoreCase(CSS.Value.UNDERLINE, curValue)) {
                        c.SetUnderline(null, 0.75f, 0, 0, -0.125f, PdfContentByte.LINE_CAP_BUTT);
                    }
                    if (Util.EqualsIgnoreCase(CSS.Value.LINE_THROUGH, curValue)) {
                        c.SetUnderline(null, 0.75f, 0, 0, 0.25f, PdfContentByte.LINE_CAP_BUTT);
                    }
                }
            }
            if (rules.TryGetValue(CSS.Property.BACKGROUND_COLOR, out value))
            {
                c.SetBackground(HtmlUtilities.DecodeColor(value));
            }
            f.Size = size;
            c.Font = f;


            float? leading = null;
            value = null;
            if (rules.TryGetValue(CSS.Property.LINE_HEIGHT, out value)) {
                if (utils.IsNumericValue(value)) {
                    leading = float.Parse(value, CultureInfo.InvariantCulture) * c.Font.Size;
                } else if (utils.IsRelativeValue(value)) {
                    leading = utils.ParseRelativeValue(value, c.Font.Size);
                } else if (utils.IsMetricValue(value)) {
                    leading = utils.ParsePxInCmMmPcToPt(value);
                }
            }

            if (leading != null) {
                c.setLineHeight((float)leading);
            }
            return c;
        }

        virtual public Font ApplyFontStyles(Tag t)
        {
            String fontName = null;
            String encoding = BaseFont.CP1252;
            float size = FontSizeTranslator.GetInstance().GetFontSize(t);
            if (size == Font.UNDEFINED)
                size = Font.DEFAULTSIZE;
            int style = Font.UNDEFINED;
            BaseColor color = null;
            IDictionary<String, String> rules = t.CSS;
            foreach (KeyValuePair< String, String > entry in rules)
            {
                String key = entry.Key;
                String value = entry.Value;
                if (Util.EqualsIgnoreCase(CSS.Property.FONT_WEIGHT, key)) {
                    if (IsBoldValue(value)) {
                        if (style == Font.ITALIC) {
                            style = Font.BOLDITALIC;
                        } else {
                            style = Font.BOLD;
                        }
                    } else {
                        if (style == Font.BOLDITALIC) {
                            style = Font.ITALIC;
                        } else if (style == Font.BOLD) {
                            style = Font.NORMAL;
                        }
                    }
                } else if (Util.EqualsIgnoreCase(CSS.Property.FONT_STYLE, key)) {
                    if (Util.EqualsIgnoreCase(CSS.Value.ITALIC, value))
                    {
                        if (style == Font.BOLD) {
                            style = Font.BOLDITALIC;
                        } else {
                            style = Font.ITALIC;
                        }
                    }
                } else if (Util.EqualsIgnoreCase(CSS.Property.FONT_FAMILY, key)) {
                    // TODO improve fontfamily parsing (what if a font family has a comma in the name ? )
                    fontName = value;
                }
                else if (Util.EqualsIgnoreCase(CSS.Property.COLOR, key)) {
                    color = HtmlUtilities.DecodeColor(value);
                }
            }
            if (fontName != null)
            {
                if (fontName.Contains(","))
                {
                    String[] fonts = fontName.Split(new Char[]{','});
                    Font firstFont = null;
                    foreach (String s in fonts)
                    {
                        String trimmedS = utils.TrimAndRemoveQuoutes(s);
                        if (fontProvider.IsRegistered(trimmedS))
                        {
                            Font f = fontProvider.GetFont(trimmedS, encoding, BaseFont.EMBEDDED, size, style, color);
                            if (f != null &&
                                (style == Font.NORMAL || style == Font.UNDEFINED || (f.Style & style) == 0))
                            {
                                return f;
                            }
                            if (firstFont == null)
                            {
                                firstFont = f;
                            }
                        }
                    }
                    if (firstFont != null)
                    {
                        return firstFont;
                    }else {
                        if (fonts.Length > 0) {
                            fontName = utils.TrimAndRemoveQuoutes(fontName.Split(new Char[] { ',' })[0]);
                        } else {
                            fontName = null;
                        }
                    }
                } else {
                    fontName = utils.TrimAndRemoveQuoutes(fontName);
                }
            }

            return fontProvider.GetFont(fontName, encoding, BaseFont.EMBEDDED, size, style, color);
        }

        protected bool IsBoldValue(String value) {
            value = value.Trim();
            return CSS.Value.BOLD.Contains(value) ||
                    (value.Length == 3 && value.EndsWith("00") && value[0] >= '6' && value[0] <= '9');
        }


        /**
         * Method used for retrieving the widest word of a chunk of text. All styles of the chunk will be taken into account when calculating the width of the words.
         *
         * @param c chunk of which the widest word is required.
         *
         * @return float containing the width of the widest word.
         */
        virtual public float GetWidestWord(Chunk c) {
            String[] words = c.Content.Split(CssUtils.whitespace);
            float widestWord = 0;
            for (int i = 0; i < words.Length; i++)
            {
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
         *
         * @param source chunk which contains the required styles.
         * @param target chunk which needs the required styles.
         */

        virtual public void CopyChunkStyles(Chunk source, Chunk target)
        {
            target.Font = source.Font;
            target.Attributes = source.Attributes;
            target.SetCharacterSpacing(source.GetCharacterSpacing());
            target.SetHorizontalScaling(source.HorizontalScaling);
            target.SetHorizontalScaling(source.HorizontalScaling);
        }

        virtual public IFontProvider FontProvider
        {
            get {
                return fontProvider;
            }
            set {
                fontProvider = value;
            }
        }
    }
}
