using System;

/*
 * $Id$
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

namespace iTextSharp.text {
    /// <summary>
    /// Base class for Color, serves as wrapper class for <see cref="T:System.Drawing.Color"/>
    /// to allow extension.
    /// </summary>
    public class BaseColor {
        public static readonly BaseColor WHITE = new BaseColor(255, 255, 255);
        public static readonly BaseColor LIGHT_GRAY = new BaseColor(192, 192, 192);
        public static readonly BaseColor GRAY      = new BaseColor(128, 128, 128);
        public static readonly BaseColor DARK_GRAY  = new BaseColor(64, 64, 64);
        public static readonly BaseColor BLACK     = new BaseColor(0, 0, 0);
        public static readonly BaseColor RED       = new BaseColor(255, 0, 0);
        public static readonly BaseColor PINK      = new BaseColor(255, 175, 175);
        public static readonly BaseColor ORANGE     = new BaseColor(255, 200, 0);
        public static readonly BaseColor YELLOW     = new BaseColor(255, 255, 0);
        public static readonly BaseColor GREEN     = new BaseColor(0, 255, 0);
        public static readonly BaseColor MAGENTA    = new BaseColor(255, 0, 255);
        public static readonly BaseColor CYAN     = new BaseColor(0, 255, 255);
        public static readonly BaseColor BLUE     = new BaseColor(0, 0, 255);
        private const double FACTOR = 0.7;
        System.Drawing.Color color;

        /// <summary>
        /// Constuctor for Color object.
        /// </summary>
        /// <param name="red">The red component value for the new Color structure. Valid values are 0 through 255.</param>
        /// <param name="green">The green component value for the new Color structure. Valid values are 0 through 255.</param>
        /// <param name="blue">The blue component value for the new Color structure. Valid values are 0 through 255.</param>
        public BaseColor(int red, int green, int blue) {
            color = System.Drawing.Color.FromArgb(red, green, blue);
        }

        /// <summary>
        /// Constuctor for Color object.
        /// </summary>
        /// <param name="red">The red component value for the new Color structure. Valid values are 0 through 255.</param>
        /// <param name="green">The green component value for the new Color structure. Valid values are 0 through 255.</param>
        /// <param name="blue">The blue component value for the new Color structure. Valid values are 0 through 255.</param>
        /// <param name="alpha">The transparency component value for the new Color structure. Valid values are 0 through 255.</param>
        public BaseColor(int red, int green, int blue, int alpha) {
            color = System.Drawing.Color.FromArgb(alpha, red, green, blue);
        }

        /// <summary>
        /// Constructor for Color object
        /// </summary>
        /// <param name="red">The red component value for the new Color structure. Valid values are 0 through 1.</param>
        /// <param name="green">The green component value for the new Color structure. Valid values are 0 through 1.</param>
        /// <param name="blue">The blue component value for the new Color structure. Valid values are 0 through 1.</param>
        public BaseColor(float red, float green, float blue) {
            color = System.Drawing.Color.FromArgb((int)(red * 255 + .5), (int)(green * 255 + .5), (int)(blue * 255 + .5));
        }

        /// <summary>
        /// Constructor for Color object
        /// </summary>
        /// <param name="red">The red component value for the new Color structure. Valid values are 0 through 1.</param>
        /// <param name="green">The green component value for the new Color structure. Valid values are 0 through 1.</param>
        /// <param name="blue">The blue component value for the new Color structure. Valid values are 0 through 1.</param>
        /// <param name="alpha">The transparency component value for the new Color structure. Valid values are 0 through 1.</param>
        public BaseColor(float red, float green, float blue, float alpha) {
            color = System.Drawing.Color.FromArgb((int)(alpha * 255 + .5), (int)(red * 255 + .5), (int)(green * 255 + .5), (int)(blue * 255 + .5));
        }

        public BaseColor(int argb) {
            color = System.Drawing.Color.FromArgb(argb);
        }

        /// <summary>
        /// Constructor for Color object
        /// </summary>
        /// <param name="color">a Color object</param>
        /// <overloads>
        /// Has three overloads.
        /// </overloads>
        public BaseColor(System.Drawing.Color color) {
            this.color = color;
        }

        /// <summary>
        /// Gets the red component value of this <see cref="T:System.Drawing.Color"/> structure.
        /// </summary>
        /// <value>The red component value of this <see cref="T:System.Drawing.Color"/> structure.</value>
        public int R {
            get {
                return color.R;
            }
        }

        /// <summary>
        /// Gets the green component value of this <see cref="T:System.Drawing.Color"/> structure.
        /// </summary>
        /// <value>The green component value of this <see cref="T:System.Drawing.Color"/> structure.</value>
        public int G {
            get {
                return color.G;
            }
        }

        /// <summary>
        /// Gets the blue component value of this <see cref="T:System.Drawing.Color"/> structure.
        /// </summary>
        /// <value>The blue component value of this <see cref="T:System.Drawing.Color"/> structure.</value>
        public int B {
            get {
                return color.B;
            }
        }

        public BaseColor Brighter() {
            int r = color.R;
            int g = color.G;
            int b = color.B;

            int i = (int)(1.0/(1.0-FACTOR));
            if ( r == 0 && g == 0 && b == 0)
                return new BaseColor(i, i, i);

            if ( r > 0 && r < i ) r = i;
            if ( g > 0 && g < i ) g = i;
            if ( b > 0 && b < i ) b = i;

            return new BaseColor(Math.Min((int)(r/FACTOR), 255), 
                    Math.Min((int)(g/FACTOR), 255),
                    Math.Min((int)(b/FACTOR), 255));
        }

        public BaseColor Darker() {
            return new BaseColor(Math.Max((int)(color.R * FACTOR), 0), 
                    Math.Max((int)(color.G * FACTOR), 0),
                    Math.Max((int)(color.B * FACTOR), 0));
        }
    
        public override bool Equals(object obj) {
            if (!(obj is BaseColor))
                return false;
            return color.Equals(((BaseColor)obj).color);
        }
    
        public override int GetHashCode() {
            return color.GetHashCode();
        }

        public int ToArgb() {
            return color.ToArgb();
        }
    }
}
