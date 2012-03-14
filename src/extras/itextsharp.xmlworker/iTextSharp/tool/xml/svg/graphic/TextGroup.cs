using System;
using System.Collections.Generic;

using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml.svg.tags;

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
namespace iTextSharp.tool.xml.svg.graphic {

    public class TextGroup : Graphic
    {
        IList<IElement> list;
        float width, height, x, y;

        public TextGroup(IList<IElement> list, float x, float y, float width, float height, IDictionary<String, String> css) : base(css)
        {
            this.list = list;
            this.width = width;
            this.height = height;
            this.x = x;
            this.y = y;
        }

        void DrawGroup(PdfContentByte cb)
        {
            //TODO if width and height not know: what to do
            //PdfTemplate template = cb.CreateTemplate(this.width, this.height);		
            PdfTemplate template = cb.CreateTemplate(500, 500);
            //draw the list of elements on the new template

            IList<int> xSpacing = null, ySpacing = null;
            float defaultSpacing = template.CharacterSpacing;
            float rise = 0;

            template.BeginText();
            //
            foreach (IElement elem in this.list)
            {
                Text text = (Text)elem;
                //CssSvgAppliers.GetInstance().ApplyForText(, text.GetCss(), text.GetChunk());
                CssSvgAppliers.GetInstance().ApplyForText(template, text.GetCss(), text.GetChunk());

                if (!text.IsRelative())
                {
                    //when there are x,y coordinates in the text or tspan
                    template.SetTextMatrix(text.GetX(), -1 * text.GetY());
                }

                //System.out.Println(text.chunk.GetFont());

                //the spacing
                if (text.Dx != null)
                {
                    xSpacing = text.Dx;
                }
                if (text.Dy != null)
                {
                    ySpacing = text.Dy;
                    rise = 0;
                }
                if (xSpacing != null || ySpacing != null)
                {
                    String display = text.GetText();
                    for (int i = 0; i < display.Length; i++)
                    {
                        if (xSpacing != null && xSpacing.Count > 0)
                        {
                            template.SetCharacterSpacing(xSpacing[0]);
                            xSpacing.Remove(0);
                        }
                        if (ySpacing != null && ySpacing.Count > 0)
                        {
                            rise = rise - ySpacing[0];
                            template.SetTextRise(rise);
                            ySpacing.Remove(0);
                        }
                        else
                        {
                            template.SetTextRise(rise);
                        }
                        template.ShowText(display.Substring(i, 1));

                        template.SetCharacterSpacing(defaultSpacing);
                    }
                }
                else
                {
                    template.ShowText(text.GetText());
                }
            }
            template.EndText();
            //add the template at the x, y position
            cb.ConcatCTM(1, 0, 0, -1, 0, 0);

            cb.Add(template);
        }

        protected override void Draw(PdfContentByte cb)
        {
            DrawGroup(cb);
        }
    }
}
