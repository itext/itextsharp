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
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.html.pdfelement;
using iTextSharp.tool.xml.pipeline.html;
namespace iTextSharp.tool.xml.css.apply {

    /**
     *
     * @author itextpdf.com
     *
     */
    public class NoNewLineParagraphCssApplier : CssApplier<NoNewLineParagraph> {
        private CssUtils utils = CssUtils.GetInstance();

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.css.CssApplier#apply(com.itextpdf.text.Element, com.itextpdf.tool.xml.Tag)
         */

        public virtual NoNewLineParagraph Apply(NoNewLineParagraph p, Tag t, IMarginMemory configuration) {
            return (NoNewLineParagraph) Apply(p, t, configuration, null, null);
        }

        public override NoNewLineParagraph Apply(NoNewLineParagraph p, Tag t, IMarginMemory configuration, IPageSizeContainable psc, HtmlPipelineContext ctx) {
            /*if (this.configuration.GetRootTags().Contains(t.Name)) {
                m.SetLeading(t);
            } else {
                m.SetVariablesBasedOnChildren(t);
            }*/
            float fontSize = FontSizeTranslator.GetInstance().GetFontSize(t);
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
                }
            }
            // setDefaultMargin to largestFont if no margin-top is set and p-tag is child of the root tag.
            if (null != t.Parent) {
                String parent = t.Parent.Name;
                if (!css.ContainsKey(CSS.Property.MARGIN_TOP) && configuration.GetRootTags().Contains(parent)) {
                    p.SpacingBefore = p.SpacingBefore+utils.CalculateMarginTop(fontSize.ToString(CultureInfo.InvariantCulture) +"pt", 0, configuration);
                }
                if (!css.ContainsKey(CSS.Property.MARGIN_BOTTOM) && configuration.GetRootTags().Contains(parent)) {
                    p.SpacingAfter = p.SpacingAfter+fontSize;
                    css[CSS.Property.MARGIN_BOTTOM]=  fontSize.ToString(CultureInfo.InvariantCulture)+"pt";
                    lmb = fontSize;
                    hasLMB = true;
                }
                //p.Leading = m.GetLargestLeading();
                if (p.Alignment == -1) {
                    p.Alignment = Element.ALIGN_LEFT;
                }
            }

            if (hasLMB) {
                configuration.LastMarginBottom = lmb;
            }
            return p;
        }
    }
}
