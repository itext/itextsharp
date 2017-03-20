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
using System.Collections.Generic;
using iTextSharp.text;

namespace iTextSharp.javaone.edition14.part4.helper
{
    /// <summary>
    /// MyItem implementation that gets its coordinates and color from a list
    /// of items that are all on the same line.
    /// </summary>
    public class Line : MyItem
    {
        /// <summary>
        /// Creates a Line object based on a list of items that have the same
        /// offset of their baseline.
        /// </summary>
        /// <param name="items">a list of MyItem objects</param>
        public Line(List<MyItem> items) : base()
        {
            rectangle = GetItemsRect(items);
            color = items[0].Color;
        }

        /// <summary>
        /// Creates a rectangle that encompasses all the coordinate rectangles
        /// of the items that belong to this line.
        /// </summary>
        /// <param name="items">the items that belong to a line</param>
        /// <returns>a rectangle that encompasses all items belonging to a line</returns>
        private static Rectangle GetItemsRect(List<MyItem> items)
        {
            float left = float.MaxValue;
            float right = 0;
            float top = 0;
            float bottom = float.MaxValue;
            foreach (MyItem item in items)
            {
                if (item.Rectangle.Left < left)
                    left = item.Rectangle.Left;
                if (item.Rectangle.Right > right)
                    right = item.Rectangle.Right;
                if (item.Rectangle.Top > top)
                    top = item.Rectangle.Top;
                if (item.Rectangle.Bottom < bottom)
                    bottom = item.Rectangle.Bottom;
            }
            return new Rectangle(left, bottom, right, top);
        }
    }
}
