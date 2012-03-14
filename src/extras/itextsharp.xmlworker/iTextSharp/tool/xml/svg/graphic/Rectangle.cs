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
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.svg.tags;

namespace iTextSharp.tool.xml.svg.graphic {
    public class Rectangle : Graphic
    {
        float x, y, width, height, rx, ry;

        public float GetX()
        {
            return x;
        }

        public float GetY()
        {
            return y;
        }

        public float GetWidth()
        {
            return width;
        }

        public float GetHeight()
        {
            return height;
        }

        public float GetRx()
        {
            return rx;
        }

        public float GetRy()
        {
            return ry;
        }

        public Rectangle(float x, float y, float width, float height, float rx, float ry, IDictionary<String, String> css) : base(css)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            this.rx = rx;
            this.ry = ry;
        }

        protected override void Draw(PdfContentByte cb)
        {
            //TODO check the line width with this rectangles SVG takes 5 pixel out and 5 pixels in when asking line-width=10
            //TODO check the values for rx and ry what if they get to big

            if (rx == 0 || ry == 0)
            {
                cb.Rectangle(x, y, width, height);
            }
            else
            { //corners
                /*
			
                if(rx > x / 2){
                    rx = x/2;
                }
                if(ry > y / 2){
                    ry = y/2;
                }*/

                cb.MoveTo(x + rx, y);
                cb.LineTo(x + width - rx, y);
                Arc(x + width - 2 * rx, y, x + width, y + 2 * ry, -90, 90, cb);
                cb.LineTo(x + width, y + height - ry);
                Arc(x + width, y + height - 2 * ry, x + width - 2 * rx, y + height, 0, 90, cb);
                cb.LineTo(x + rx, y + height);
                Arc(x + 2 * rx, y + height, x, y + height - 2 * ry, 90, 90, cb);
                cb.LineTo(x, y + ry);
                Arc(x, y + 2 * ry, x + 2 * rx, y, 180, 90, cb);
                cb.ClosePath();
            }

        }

        //copied this because of the moveTo
        public void Arc(float x1, float y1, float x2, float y2, float startAng, float extent, PdfContentByte cb)
        {
            List<float[]> ar = PdfContentByte.BezierArc(x1, y1, x2, y2, startAng, extent);
            if (ar.Count == 0)
                return;
            float[] pt = ar[0];
            //moveTo(pt[0], pt[1]);
            for (int k = 0; k < ar.Count; ++k)
            {
                pt = ar[k];
                cb.CurveTo(pt[2], pt[3], pt[4], pt[5], pt[6], pt[7]);
            }
        }
    }
}
