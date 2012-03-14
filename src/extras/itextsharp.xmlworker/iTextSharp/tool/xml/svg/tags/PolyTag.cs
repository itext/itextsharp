using System;
using System.Collections.Generic;
using System.Text;
using System.util;
using iTextSharp.text;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.svg;
using iTextSharp.tool.xml.svg.graphic;

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
namespace iTextSharp.tool.xml.svg.tags {

    public class PolyTag : AbstractGraphicProcessor
    {
        static String POINTS = "points";

        public override IList<IElement> End(IWorkerContext ctx, Tag tag,
                    IList<IElement> currentContent)
        {

            IList<IElement> elems = new List<IElement>();

            IDictionary<String, String> attributes = tag.Attributes;

            if (attributes != null && attributes.ContainsKey(POINTS))
            {
                String str = attributes[POINTS];

                StringTokenizer tokenizer = new StringTokenizer(CleanPath(str), ", \t\n\r\f");
                IList<String> values = new List<String>();
                while (tokenizer.HasMoreTokens())
                {
                    String value = tokenizer.NextToken().Trim();
                    //System.out.Println(value);
                    values.Add(value);
                }
                PathBean.Builder pathBuilder = new PathBean.Builder();
                if (values.Count % 2 == 1)
                {
                    values.RemoveAt(values.Count - 1);
                }

                if (values.Count % 2 == 0)
                {
                    for (int i = 0; i < (values.Count / 2); i++)
                    {
                        PathItem.Builder itemBuilder = new PathItem.Builder();
                        if (i == 0)
                        {
                            itemBuilder.SetType('M');
                        }
                        else
                        {
                            itemBuilder.SetType('L');
                        }
                        itemBuilder.AddCoordinate(values[i * 2]);
                        itemBuilder.AddCoordinate(values[(i * 2) + 1]);
                        pathBuilder.SetPathItem(itemBuilder.Build());
                    }
                }
                if (tag.Name.Equals(SvgTagNames.POLYGON))
                {
                    PathItem.Builder itemBuilder = new PathItem.Builder();
                    itemBuilder.SetType('z');
                    pathBuilder.SetPathItem(itemBuilder.Build());
                }

                elems.Add(new Path(pathBuilder.Build(), tag.CSS));
                return elems;
            } else {
                return new List<IElement>(0);
            }
        }

        //clean this write new tokenizer
        protected String CleanPath(String path)
        {
            //System.out.Println(path);
            StringBuilder buffer = new StringBuilder();
            buffer.Append(path[0]);
            for (int i = 1; i < path.Length; i++)
            {
                //add a space between the letters and the numbers
                if ((Char.IsLetter(path[i - 1]) && !Char.IsLetter(path[i]))
                        || (!Char.IsLetter(path[i - 1]) && Char.IsLetter(path[i])))
                {
                    buffer.Append(' ');
                }
                buffer.Append(path[i]);
            }
            //System.out.Println(buffer.ToString());
            return buffer.ToString();
        }

        public override bool IsElementWithId()
        {
            return true;
        }
    }
}
