using System;
using System.Globalization;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
/*
 * $Id: ImageCssApplier.java 28 2011-05-05 20:33:36Z redlab_b $
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
using iTextSharp.tool.xml.html;
using Image = iTextSharp.text.Image;

namespace iTextSharp.tool.xml.css.apply {

    /**
     * @author redlab_b
     *
     */
    public class ImageCssApplier {

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.css.CssApplier#apply(com.itextpdf.text.Element, com.itextpdf.tool.xml.Tag)
         */
        virtual public Image Apply(Image img, Tag tag) {
            String widthValue;
            tag.CSS.TryGetValue(HTML.Attribute.WIDTH, out widthValue);
            if (widthValue == null)
            {
                tag.Attributes.TryGetValue(HTML.Attribute.WIDTH, out widthValue);
            }

            String heightValue;
            tag.CSS.TryGetValue(HTML.Attribute.HEIGHT, out heightValue);
            if (heightValue == null)
            {
                tag.Attributes.TryGetValue(HTML.Attribute.HEIGHT, out heightValue);
            }

            if (widthValue == null)
                img.ScaleToFitLineWhenOverflow = true;
            else
                img.ScaleToFitLineWhenOverflow = false;

            if (heightValue == null)
                img.ScaleToFitHeight = true;
            else
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

            String before;
            tag.CSS.TryGetValue(CSS.Property.BEFORE, out before);
            if (before != null) {
                img.SpacingBefore = float.Parse(before, CultureInfo.InvariantCulture);
            }
            String after;
            tag.CSS.TryGetValue(CSS.Property.AFTER, out after);
            if (after != null) {
                img.SpacingAfter = float.Parse(after, CultureInfo.InvariantCulture);
            }
            img.WidthPercentage = 0;
            return img;
        }
    }
}
