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
using iTextSharp.awt.geom;
using iTextSharp.text;

namespace iTextSharp.javaone.edition14.part4.helper
{
    /// <summary>
    /// Abstract class that is used as a superclass for specific item classes
    /// such as TextItem, ImageItem, Line and Structure.
    /// </summary>
    public abstract class MyItem : IComparable<MyItem>
    {
        /// <summary>
        /// If we want to compare item positions we should add some tolerance.
        /// </summary>
        public static readonly float ItemPositionTolerance = 3f;

        /// <summary>
        /// Rectangle that defines the coordinates of an item.
        /// </summary>
        protected Rectangle rectangle;

        /// <summary>
        /// Color that will be used to mark the item.
        /// </summary>
        protected BaseColor color;

        /// <summary>
        /// Rectangle that defines the coordinates of an item.
        /// </summary>
        /// <returns>coordinates in the form of a Rectangle object</returns>
        public Rectangle Rectangle 
        {
            get { return rectangle; }
        }

        /// <summary>
        /// Color that will be used to mark the item.
        /// </summary>
        /// <returns>a BaseColor object</returns>
        public BaseColor Color 
        {
            get { return color; }
        }

        /// <summary>
        /// * Gets the lower left corner of the item.
        /// For image items it returns lower left corner of bounding box.
        /// For text items it returns start point of a baseline.
        /// </summary>
        /// <returns>point of the lower left corner</returns>
        public virtual Point GetLL()
        {
            return new Point(Rectangle.Left, Rectangle.Bottom);
        }

        /// <summary>
        /// Creates an instance of the MyItem object
        /// </summary>
        protected MyItem()
        {
            this.rectangle = rectangle;
            this.color = color;
        }
        
        public int CompareTo(MyItem o)
        {
            double left = GetLL().x;
            double bottom = GetLL().y;
            double oLeft = o.GetLL().x;
            double oBottom = o.GetLL().y;
            if (bottom - oBottom > ItemPositionTolerance)
                return -1;
            else if (oBottom - bottom > ItemPositionTolerance)
                return 1;
            else
                return left.CompareTo(oLeft);
        }
    }
}
