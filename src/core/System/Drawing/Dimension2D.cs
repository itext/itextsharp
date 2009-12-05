using System;

/*
 * $Id:  $
 * 
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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

 namespace System.Drawing {
    /// <summary>
    /// The <code>Dimension2D</code> class is to encapsulate a width 
    /// and a height dimension.
    /// </summary>
    /// <remarks>
    /// This class is only the abstract baseclass for all objects that
    /// store a 2D dimension.
    /// The actual storage representation of the sizes is left to
    /// the subclass.
    /// </remarks>
    public abstract class Dimension2D : ICloneable {
        /// <summary>
        /// This is an abstract class that cannot be instantiated directly.
        /// Type-specific implementation subclasses are available for
        /// instantiation and provide a number of formats for storing
        /// the information necessary to satisfy the various accessor
        /// methods below.
        /// </summary>
        /// <seealso cref="T:System.Drawing.Dimension"/>
        protected Dimension2D() {
        }

        /// <summary>
        /// Returns the width of this <code>Dimension</code> in double 
        /// precision.
        /// </summary>
        ///    <value>the width</value>
        public abstract double Width {get;}

        /// <summary>
        /// Returns the height of this <code>Dimension</code> in double 
        /// precision.
        /// </summary>
        /// <value>the height</value>
        public abstract double Height {get;}

        /// <summary>
        /// Sets the size of this <code>Dimension</code> object to the 
        /// specified width and height.
        /// </summary>
        /// <param name="width">the new width for the <code>Dimension</code>
        /// object</param>
        /// <param name="height">the new height for the <code>Dimension</code> 
        /// object</param>
        public abstract void SetSize(double width, double height);

        /// <summary>
        /// Sets the size of this <code>Dimension2D</code> object to 
        /// match the specified size.
        /// </summary>
        /// <value>the size</value>
        public Dimension2D Size {
            set {
                SetSize(value.Width, value.Height);
            }
        }

        /// <summary>
        /// Creates a new object of the same class as this object.
        /// </summary>
        /// <returns>a clone of this instance</returns>
        public Object Clone() {
            throw new Exception("not implemented");
        }
    }
}
