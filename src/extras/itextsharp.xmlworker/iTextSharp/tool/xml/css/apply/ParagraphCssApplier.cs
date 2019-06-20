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
using System.Collections.Generic;
using System.Globalization;
using System.util;
using iTextSharp.text;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.html;

namespace iTextSharp.tool.xml.css.apply {


    /**
     * Applies CSS on a {@link Paragraph}
     *
     * @author itextpdf.com
     */

    public class ParagraphCssApplier : CssApplier<Paragraph> {

        private CssAppliers appliers;

        /**
         * Construct a ParagraphCssApplier.
         *
         */

        public ParagraphCssApplier(CssAppliers appliers) {
            this.appliers = appliers;
        }

        /**
         * Styles a paragraph
         *
         * @param p the paragraph
         * @param t the tag
         * @param configuration the MarginMemory
         * @return a styled {@link Paragraph}
         */

        public virtual Paragraph Apply(Paragraph p, Tag t, IMarginMemory configuration) {
            return (Paragraph) Apply(p, t, configuration, null, null);
        }

        public override Paragraph Apply(Paragraph p, Tag t, IMarginMemory configuration, IPageSizeContainable psc, HtmlPipelineContext ctx) {
            /*MaxLeadingAndSize m = new MaxLeadingAndSize();
            if (configuration.GetRootTags().Contains(t.GetName())) {
                m.SetLeading(t);
            } else {
                m.SetVariablesBasedOnChildren(t);
            }*/
            CssUtils utils = CssUtils.GetInstance();
            float fontSize = FontSizeTranslator.GetInstance().GetFontSize(t);
            if (fontSize == Font.UNDEFINED) {
                fontSize = 0;
            }
            float lmb = 0;
            bool hasLMB = false;
            IDictionary<String, String> css = t.CSS;
            foreach (KeyValuePair<String, String> entry in css) {
                String key = entry.Key;
                String value = entry.Value;
                if (Util.EqualsIgnoreCase(CSS.Property.MARGIN_TOP, key)) {
                    p.SpacingBefore = p.SpacingBefore + utils.CalculateMarginTop(value, fontSize, configuration);
                } else if (Util.EqualsIgnoreCase(CSS.Property.PADDING_TOP, key)) {
                    p.SpacingBefore = p.SpacingBefore + utils.ParseValueToPt(value, fontSize);
                    p.PaddingTop = utils.ParseValueToPt(value, fontSize);
                } else if (Util.EqualsIgnoreCase(CSS.Property.MARGIN_BOTTOM, key)) {
                    float after = utils.ParseValueToPt(value, fontSize);
                    p.SpacingAfter = p.SpacingAfter + after;
                    lmb = after;
                    hasLMB = true;
                } else if (Util.EqualsIgnoreCase(CSS.Property.PADDING_BOTTOM, key)) {
                    p.SpacingAfter = p.SpacingAfter + utils.ParseValueToPt(value, fontSize);
                } else if (Util.EqualsIgnoreCase(CSS.Property.MARGIN_LEFT, key)) {
                    p.IndentationLeft = p.IndentationLeft + utils.ParseValueToPt(value, fontSize);
                } else if (Util.EqualsIgnoreCase(CSS.Property.MARGIN_RIGHT, key)) {
                    p.IndentationRight = p.IndentationRight + utils.ParseValueToPt(value, fontSize);
                } else if (Util.EqualsIgnoreCase(CSS.Property.PADDING_LEFT, key)) {
                    p.IndentationLeft = p.IndentationLeft + utils.ParseValueToPt(value, fontSize);
                } else if (Util.EqualsIgnoreCase(CSS.Property.PADDING_RIGHT, key)) {
                    p.IndentationRight = p.IndentationRight + utils.ParseValueToPt(value, fontSize);
                } else if (Util.EqualsIgnoreCase(CSS.Property.TEXT_ALIGN, key)) {
                    p.Alignment = CSS.GetElementAlignment(value);
                } else if (Util.EqualsIgnoreCase(CSS.Property.TEXT_INDENT, key)) {
                    p.FirstLineIndent = utils.ParseValueToPt(value, fontSize);
                } else if (Util.EqualsIgnoreCase(CSS.Property.LINE_HEIGHT, key)) {
                    if (utils.IsNumericValue(value)) {
                        p.Leading = float.Parse(value, CultureInfo.InvariantCulture) *fontSize;
                    } else if (utils.IsRelativeValue(value)) {
                        p.Leading = utils.ParseRelativeValue(value, fontSize);
                    } else if (utils.IsMetricValue(value)) {
                        p.Leading = utils.ParsePxInCmMmPcToPt(value);
                    }
                }
            }

            if (t.Attributes.ContainsKey(HTML.Attribute.ALIGN)) {
                String value = t.Attributes[HTML.Attribute.ALIGN];

                if (value != null) {
                    p.Alignment = CSS.GetElementAlignment(value);
                }
            }

            // setDefaultMargin to largestFont if no margin-bottom is set and p-tag is child of the root tag.
            /*if (null != t.GetParent()) {
                String parent = t.GetParent().GetName();
                if (css[CSS.Property.MARGIN_TOP] == null && configuration.GetRootTags().Contains(parent)) {
                    p.SetSpacingBefore(p.GetSpacingBefore() + utils.CalculateMarginTop(fontSize + "pt", 0, configuration));
                }
                if (css[CSS.Property.MARGIN_BOTTOM] == null && configuration.GetRootTags().Contains(parent)) {
                    p.SetSpacingAfter(p.GetSpacingAfter() + fontSize);
                    css.Put(CSS.Property.MARGIN_BOTTOM, fontSize + "pt");
                    lmb = fontSize;
                    hasLMB = true;
                }
                //p.SetLeading(m.GetLargestLeading());  We need possibility to detect that line-height undefined;
                if (p.GetAlignment() == -1) {
                    p.SetAlignment(Element.ALIGN_LEFT);
                }
            }*/

            if (hasLMB)
            {
                configuration.LastMarginBottom = lmb;
            }
            Font font = appliers.ChunkCssAplier.ApplyFontStyles(t);
            p.Font = font;
            // TODO reactive for positioning and implement more
            return p;
        }
    }
}
