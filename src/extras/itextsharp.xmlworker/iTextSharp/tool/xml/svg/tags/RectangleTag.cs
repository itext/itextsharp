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

namespace iTextSharp.tool.xml.svg.tags {
    public class RectangleTag : AbstractGraphicProcessor
    {
        //<rect x="60" y="60" width="" height="" rx="5" cy="5"/>

        public override IList<IElement> End(IWorkerContext ctx, Tag tag,
                    IList<IElement> currentContent)
        {

            IDictionary<String, String> attributes = tag.Attributes;
            if (attributes != null) {

                //width and height wrong or not existing -> draw nothing
                float width, height;
                try
                {
                    width = int.Parse(attributes["width"]);
                }
                catch 
                {
                    return new List<IElement>(0);
                }
                try
                {
                    height = int.Parse(attributes["height"]);
                }
                catch
                {
                    return new List<IElement>(0);
                }

                if (width <= 0 || height <= 0)
                {
                    return new List<IElement>(0);
                }

                //x,y missing or wrong -> just zero, negative value is acceptable			
                float x = 0, y = 0, rx = 0, ry = 0;
                try
                {
                    x = int.Parse(attributes["x"]);
                }
                catch 
                {
                    // TODO: handle exception
                }

                try
                {
                    y = int.Parse(attributes["y"]);
                }
                catch 
                {
                    // TODO: handle exception
                }

                //if one of these is wrong or missing, take the other ones value
                //if the value is specified but less or equal than 0 -> no rounding
                try
                {
                    rx = int.Parse(attributes["rx"]);
                }
                catch 
                {
                    rx = -1;
                }

                try
                {
                    ry = int.Parse(attributes["ry"]);
                }
                catch 
                {
                    ry = -1;
                }

                //one out of the two is zero (but was specified)
                if ((attributes.ContainsKey("rx") && rx == 0) || (attributes.ContainsKey("ry") && ry == 0))
                {
                    rx = 0;
                    ry = 0;
                } else { //copy the wrong values
                    if (rx <= 0)
                    {
                        rx = ry;
                    }
                    if (ry <= 0)
                    {
                        ry = rx;
                    }

                    if (rx <= 0 && ry <= 0)
                    {
                        rx = 0;
                        ry = 0;
                    }
                }

                IList<IElement> l = new List<IElement>(1);

                l.Add(new graphic.Rectangle(x, y, width, height, rx, ry, tag.CSS));
                return l;
            } else {
                return new List<IElement>(0);
            }
        }

        public override bool IsElementWithId() {
            return true;
        }
    }
}
