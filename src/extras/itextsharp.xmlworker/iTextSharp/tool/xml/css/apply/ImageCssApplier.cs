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
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.pipeline.html;
using Image = iTextSharp.text.Image;

namespace iTextSharp.tool.xml.css.apply {

    /**
     * Class that applies the parsed CSS to an Image object.
     *
     * @author redlab_b
     */
    public class ImageCssApplier : CssApplier<Image> {

        /**
         * Applies CSS to an Image. Currently supported:
         * - width
         * - height
         * - borders (color, width)
         * - spacing before and after
         *
         * @param img the image
         * @param tag the tag with the css
         * @return a styled Image
         */

        public virtual Image Apply(Image img, Tag tag) {
            return (Image) Apply(img, tag, null, null, null);
        }

        public override Image Apply(Image img, Tag tag, IMarginMemory mm, IPageSizeContainable psc, HtmlPipelineContext ctx) {
            IDictionary<String, String> cssMap = tag.CSS;

            String widthValue = null;
            cssMap.TryGetValue(HTML.Attribute.WIDTH, out widthValue);
            if (widthValue == null) {
                tag.Attributes.TryGetValue(HTML.Attribute.WIDTH, out widthValue);
            }

            String heightValue = null;
            cssMap.TryGetValue(HTML.Attribute.HEIGHT, out heightValue);
            if (heightValue == null) {
                tag.Attributes.TryGetValue(HTML.Attribute.HEIGHT, out heightValue);
            }

            if (widthValue == null) {
                img.ScaleToFitLineWhenOverflow = true;
            } else {
                img.ScaleToFitLineWhenOverflow = false;
            }

            img.ScaleToFitHeight = false;


            CssUtils utils = CssUtils.GetInstance();
            float widthInPoints = utils.ParsePxInCmMmPcToPt(widthValue);

            float heightInPoints = utils.ParsePxInCmMmPcToPt(heightValue);

            if (widthInPoints > 0 && heightInPoints > 0)
            {
                img.ScaleAbsolute(widthInPoints, heightInPoints);
            }
            else if (widthInPoints > 0)
            {
                heightInPoints = img.Height * widthInPoints / img.Width;
                img.ScaleAbsolute(widthInPoints, heightInPoints);
            }
            else if (heightInPoints > 0)
            {
                widthInPoints = img.Width * heightInPoints / img.Height;
                img.ScaleAbsolute(widthInPoints, heightInPoints);
            }

            // apply border CSS
            String borderTopColor = null;
            cssMap.TryGetValue(CSS.Property.BORDER_TOP_COLOR, out borderTopColor);
            if (borderTopColor != null) {
                img.BorderColorTop = HtmlUtilities.DecodeColor(borderTopColor);
            }

            String borderTopWidth = null;
            cssMap.TryGetValue(CSS.Property.BORDER_TOP_WIDTH, out borderTopWidth);
            if (borderTopWidth != null) {
                img.BorderWidthTop = utils.ParseValueToPt(borderTopWidth, 1f);
            }

            String borderRightColor = null;
            cssMap.TryGetValue(CSS.Property.BORDER_RIGHT_COLOR, out borderRightColor);
            if (borderRightColor != null) {
                img.BorderColorRight = HtmlUtilities.DecodeColor(borderRightColor);
            }

            String borderRightWidth = null;
            cssMap.TryGetValue(CSS.Property.BORDER_RIGHT_WIDTH, out borderRightWidth);
            if (borderRightWidth != null) {
                img.BorderWidthRight = utils.ParseValueToPt(borderRightWidth, 1f);
            }

            String borderBottomColor = null;
            cssMap.TryGetValue(CSS.Property.BORDER_BOTTOM_COLOR, out borderBottomColor);
            if (borderBottomColor != null) {
                img.BorderColorBottom = HtmlUtilities.DecodeColor(borderBottomColor);
            }

            String borderBottomWidth = null;
            cssMap.TryGetValue(CSS.Property.BORDER_BOTTOM_WIDTH, out borderBottomWidth);
            if (borderBottomWidth != null) {
                img.BorderWidthBottom = utils.ParseValueToPt(borderBottomWidth, 1f);
            }

            String borderLeftColor = null;
            cssMap.TryGetValue(CSS.Property.BORDER_LEFT_COLOR, out borderLeftColor);
            if (borderLeftColor != null) {
                img.BorderColorLeft = HtmlUtilities.DecodeColor(borderLeftColor);
            }

            String borderLeftWidth = null;
            cssMap.TryGetValue(CSS.Property.BORDER_LEFT_WIDTH, out borderLeftWidth);
            if (borderLeftWidth != null) {
                img.BorderWidthLeft = utils.ParseValueToPt(borderLeftWidth, 1f);
            }
            // end of border CSS

            String before = null;
            cssMap.TryGetValue(CSS.Property.BEFORE, out before);
            if (before != null) {
                img.SpacingBefore = float.Parse(before, CultureInfo.InvariantCulture);
            }
            String after = null;
            cssMap.TryGetValue(CSS.Property.AFTER, out after);
            if (after != null) {
                img.SpacingAfter = float.Parse(after, CultureInfo.InvariantCulture);
            }

            img.WidthPercentage = 0;
            return img;
        }
    }
}
