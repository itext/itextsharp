/*
 * $Id: $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2012 1T3XT BVBA
 * Authors: VVB, Bruno Lowagie, et al.
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

using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.svg.tags;

namespace iTextSharp.tool.xml.svg.graphic {

    public class Group : Graphic
    {
        IList<IElement> list;
        float width, height, x, y;
        bool applyCSSToElements;

        public Group(IList<IElement> list, float x, float y, float width, float height, IDictionary<String, String> css, bool applyCSSToElements) : base(css) {
            this.list = list;
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;
            this.applyCSSToElements = applyCSSToElements;
        }

        protected override void Draw(PdfContentByte cb)
        {
            //TODO if width and height not know: what to do
            //PdfTemplate template = cb.CreateTemplate(this.width, this.height);		
            PdfTemplate template = cb.CreateTemplate(500, 500);
            //draw the list of elements on the new template
            foreach (IElement elem in this.list)
            {
                Graphic graphic = (Graphic)elem;

                if (applyCSSToElements)
                {
                    graphic.Draw(template, GetCombinedCss(graphic.GetCss(), GetCss()));
                }
                else
                {
                    graphic.Draw(template, graphic.GetCss());
                }
            }
            //add the template at the x, y position
            System.Drawing.Drawing2D.Matrix translation = new System.Drawing.Drawing2D.Matrix();
            translation.Translate(this.x, this.y);
            cb.ConcatCTM(translation);

            cb.Add(template);
        }

        private IDictionary<String, String> GetCombinedCss(IDictionary<String, String> element, IDictionary<String, String> parent)
        {
            IDictionary<String, String> result = new Dictionary<String, String>();
            xml.css.CssUtils.MapPutAll(result, parent);
            xml.css.CssUtils.MapPutAll(result, element);
            return result;
        }
    }
}
