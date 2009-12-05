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
    /// The <code>Dimension</code> class encapsulates the width and
    /// height of a component (in int precision) in a single object. 
    /// </summary>
    /// <remarks>
    /// The class is 
    /// associated with certain properties of components. Several methods 
    /// defined by the <code>Component</code> class and the 
    /// <code>LayoutManager</code> interface return a <code>Dimension</code> object.
    /// <p/>
    /// Normally the values of <code>width</code> 
    /// and <code>height</code> are non-negative ints. 
    /// The constructors that allow you to create a dimension do 
    /// not prevent you from setting a negative value for these properties. 
    /// If the value of <code>width</code> or <code>height</code> is 
    /// negative, the behavior of some methods defined by other objects is 
    /// undefined. 
    /// </remarks>
    public class Dimension : Dimension2D {
    
        /// <summary>
        /// The width dimension. Negative values can be used. 
        /// </summary>
        public int width;

        /// <summary>
        /// The height dimension. Negative values can be used. 
        /// </summary>
        public int height;

        /// <summary>
        /// Creates an instance of <code>Dimension</code> with a width 
        /// of zero and a height of zero. 
        /// </summary>
        public Dimension() : this(0, 0) {}

        /// <summary>
        /// Creates an instance of <code>Dimension</code> whose width 
        /// and height are the same as for the specified dimension. 
        /// </summary>
        /// <param name="d">
        /// the specified dimension for the 
        /// <code>width</code> and 
        /// <code>height</code> values.
        /// </param>
        public Dimension(Dimension d) : this(d.width, d.height) {}

        /// <summary>
        /// Constructs a Dimension and initializes it to the specified width and
        /// specified height.
        /// </summary>
        /// <param name="width">the specified width dimension</param>
        /// <param name="height">the specified height dimension</param>
        public Dimension(int width, int height) {
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Returns the width of this dimension in double precision.
        /// </summary>
        /// <value>the width</value>
        public override double Width {
            get {
                return width;
            }
        }

        /// <summary>
        /// Returns the height of this dimension in double precision.
        /// </summary>
        /// <value>the height</value>
        public override double Height {
            get {
                return height;
            }
        }

        /// <summary>
        /// Set the size of this Dimension object to the specified width
        /// and height in double precision.
        /// </summary>
        /// <param name="width">the new width for the Dimension object</param>
        /// <param name="height">the new height for the Dimension object</param>
        public override void SetSize(double width, double height) {
            width = (int) Math.Ceiling(width);
            height = (int) Math.Ceiling(height);
        }

        /// <summary>
        /// Get/set the size of this <code>Dimension</code> object.
        /// </summary>
        /// <value>the size</value>
        public new Dimension Size {
            get {
                return new Dimension(width, height);
            }

            set {
                SetSize(value.width, value.height);
            }
        }    

        /// <summary>
        /// Set the size of this <code>Dimension</code> object 
        /// to the specified width and height.
        /// </summary>
        /// <param name="width">the new width for this <code>Dimension</code> object.</param>
        /// <param name="height">the new height for this <code>Dimension</code> object.</param>
        public void SetSize(int width, int height) {
            this.width = width;
            this.height = height;
        }    

        /// <summary>
        /// Checks whether two dimension objects have equal values.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(Object obj) {
            if (obj is Dimension) {
                Dimension d = (Dimension)obj;
                return (width == d.width) && (height == d.height);
            }
            return false;
        }

        /// <summary>
        /// Returns the hash code for this Dimension.
        /// </summary>
        /// <returns>a hash code</returns>
        public override int GetHashCode() {
            int sum = width + height;
            return sum * (sum + 1)/2 + width;
        }

        /// <summary>
        /// Returns a string representation of the values of this 
        /// <code>Dimension</code> object's <code>height</code> and 
        /// <code>width</code> fields.
        /// </summary>
        /// <remarks>
        /// This method is intended to be used only 
        /// for debugging purposes, and the content and format of the returned 
        /// string may vary between implementations. The returned string may be 
        /// empty but may not be <code>null</code>.
        /// </remarks>
        /// <returns>a string representation of this <code>Dimension</code>
        /// object.
        /// </returns>
        public override string ToString() {
            return this.GetType().Name + "[width=" + width + ",height=" + height + "]";
        }
    }
}
