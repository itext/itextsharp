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
using System.util;
using iTextSharp.text;
using iTextSharp.text.pdf.draw;
using iTextSharp.text.html;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.html;
/**
 *
 */
namespace iTextSharp.tool.xml.css.apply {

    /**
     * @author Emiel Ackermann
     *
     */
    public class LineSeparatorCssApplier : CssApplier<LineSeparator> {

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.css.CssApplier#apply(com.itextpdf.text.Element, com.itextpdf.tool.xml.Tag)
         */

        public virtual LineSeparator Apply(LineSeparator ls, Tag t, IPageSizeContainable psc) {
            return (LineSeparator) Apply(ls, t, null, psc, null);
        }

        public override LineSeparator Apply(LineSeparator ls, Tag t, IMarginMemory mm, IPageSizeContainable psc, HtmlPipelineContext ctx) {
            float lineWidth = 1;
            IDictionary<String, String> css = t.CSS;
            if (t.Attributes.ContainsKey(HTML.Attribute.SIZE)){
                lineWidth = CssUtils.GetInstance().ParsePxInCmMmPcToPt(t.Attributes[HTML.Attribute.SIZE]);
            } else if (css.ContainsKey(CSS.Property.HEIGHT)) {
                lineWidth = CssUtils.GetInstance().ParsePxInCmMmPcToPt(css[CSS.Property.HEIGHT]);
            }
            ls.LineWidth = lineWidth;
            BaseColor lineColor = BaseColor.BLACK;
            if (t.Attributes.ContainsKey(CSS.Property.COLOR)) {
                lineColor = HtmlUtilities.DecodeColor(t.Attributes[CSS.Property.COLOR]);
            }
            else if (css.ContainsKey(CSS.Property.COLOR)) {
                lineColor  = HtmlUtilities.DecodeColor(css[CSS.Property.COLOR]);
            } else if (css.ContainsKey(CSS.Property.BACKGROUND_COLOR)) {
                lineColor = HtmlUtilities.DecodeColor(css[CSS.Property.BACKGROUND_COLOR]);
            }
            ls.LineColor = lineColor;
            float percentage = 100;
            String widthStr;
            css.TryGetValue(CSS.Property.WIDTH, out widthStr);
            if (widthStr == null) {
                 t.Attributes.TryGetValue(CSS.Property.WIDTH,out widthStr);
            }
            if (widthStr != null) {
                if (widthStr.Contains("%")) {
                    percentage = float.Parse(widthStr.Replace("%", ""), CultureInfo.InvariantCulture);
                } else {
                    percentage = (CssUtils.GetInstance().ParsePxInCmMmPcToPt(widthStr)/psc.PageSize.Width)*100;
                }
            }
            ls.Percentage = percentage;
            String align;
            t.Attributes.TryGetValue(HTML.Attribute.ALIGN,out align);
            if (CSS.Value.RIGHT.Equals(align)) {
                ls.Alignment = Element.ALIGN_RIGHT;
            }
            else if (CSS.Value.LEFT.Equals(align)) {
                ls.Alignment = Element.ALIGN_LEFT;
            }
            else if (CSS.Value.CENTER.Equals(align)) {
                ls.Alignment = Element.ALIGN_CENTER;
            }
            return ls;
        }
    }
}
