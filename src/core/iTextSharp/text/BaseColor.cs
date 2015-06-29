using System;
/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
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
using iTextSharp.text.error_messages;

namespace iTextSharp.text {
    /// <summary>
    /// Base class for Color, serves as wrapper class for <see cref="T:System.Drawing.Color"/>
    /// to allow extension.
    /// </summary>
    public class BaseColor {
        public static readonly BaseColor WHITE = new BaseColor(255, 255, 255);
        public static readonly BaseColor LIGHT_GRAY = new BaseColor(192, 192, 192);
        public static readonly BaseColor GRAY = new BaseColor(128, 128, 128);
        public static readonly BaseColor DARK_GRAY = new BaseColor(64, 64, 64);
        public static readonly BaseColor BLACK = new BaseColor(0, 0, 0);
        public static readonly BaseColor RED = new BaseColor(255, 0, 0);
        public static readonly BaseColor PINK = new BaseColor(255, 175, 175);
        public static readonly BaseColor ORANGE = new BaseColor(255, 200, 0);
        public static readonly BaseColor YELLOW = new BaseColor(255, 255, 0);
        public static readonly BaseColor GREEN = new BaseColor(0, 255, 0);
        public static readonly BaseColor MAGENTA = new BaseColor(255, 0, 255);
        public static readonly BaseColor CYAN = new BaseColor(0, 255, 255);
        public static readonly BaseColor BLUE = new BaseColor(0, 0, 255);
        private const double FACTOR = 0.7;
        private int value;

        /**
         * Construct a new BaseColor.
         * @param red the value for the red gamma
         * @param green the value for the green gamma
         * @param blue the value for the blue gamma
         * @param alpha the value for the alpha gamma
         */
        public BaseColor(int red, int green, int blue, int alpha) {
            SetValue(red, green, blue, alpha);
        }

        /**
         * @param red
         * @param green
         * @param blue
         */
        public BaseColor(int red, int green, int blue): 
            this(red, green, blue, 255) {
        }

        /**
         * Construct a BaseColor with float values.
         * @param red
         * @param green
         * @param blue
         * @param alpha
         */
        public BaseColor(float red, float green, float blue, float alpha) :
            this((int)(red * 255 + .5), (int)(green * 255 + .5), (int)(blue * 255 + .5), (int)(alpha * 255 + .5))
        {
        }

        /**
         * Construct a BaseColor with float values.
         * @param red
         * @param green
         * @param blue
         */
        public BaseColor(float red, float green, float blue) :
            this(red, green, blue, 1f)
        {
        }
    
        /**
         * Construct a BaseColor by setting the combined value.
         * @param argb
         */
        public BaseColor(int argb) {
            value = argb;
        }

#if DRAWING
        /**
         * Construct a BaseColor by System.Drawing.Color.
         * @param color
         */
        public BaseColor(System.Drawing.Color color) :
            this(color.R, color.G, color.B, color.A){
        }
#endif// DRAWING

        /**
         * @return the combined color value
         */
        virtual public int RGB {
            get { return value; }
        }
        /**
         *
         * @return the value for red
         */
        virtual public int R {
            get { return (RGB >> 16) & 0xFF; }
        }
        /**
         *
         * @return the value for green
         */
        virtual public int G {
            get { return (RGB >> 8) & 0xFF; }
        }
        /**
         *
         * @return the value for blue
         */
        virtual public int B {
            get { return (RGB >> 0) & 0xFF; }
        }
        /**
         *
         * @return the value for the alpha channel
         */
        virtual public int A {
            get { return (RGB >> 24) & 0xff; }
        }

        /**
         * Make this BaseColor brighter. Factor used is 0.7.
         * @return the new BaseColor
         */
        virtual public BaseColor Brighter() {
            int r = R;
            int g = G;
            int b = B;

            int i = (int) (1.0 / (1.0 - FACTOR));
            if (r == 0 && g == 0 && b == 0) {
                return new BaseColor(i, i, i);
            }
            if (r > 0 && r < i)
                r = i;
            if (g > 0 && g < i)
                g = i;
            if (b > 0 && b < i)
                b = i;

            return new BaseColor(Math.Min((int) (r / FACTOR), 255),
                    Math.Min((int) (g / FACTOR), 255),
                    Math.Min((int) (b / FACTOR), 255));
        }

        /**
         * Make this color darker. Factor used is 0.7
         * @return the new BaseColor
         */
        virtual public BaseColor Darker() {
            return new BaseColor(Math.Max((int) (R * FACTOR), 0),
                    Math.Max((int) (G * FACTOR), 0),
                    Math.Max((int) (B * FACTOR), 0));
        }

        [Obsolete ("Use RGB instead")]
        virtual public int ToArgb() {
            return value;
        }

        public override bool Equals(object obj) {
            return obj is BaseColor && ((BaseColor) obj).value == value;
        }

        public override int GetHashCode() {
            return value;
        }

        protected internal virtual void SetValue(int red, int green, int blue, int alpha) {
            Validate(red);
            Validate(green);
            Validate(blue);
            Validate(alpha);
            value = ((alpha & 0xFF) << 24) | ((red & 0xFF) << 16) | ((green & 0xFF) << 8) | ((blue & 0xFF) << 0);
        }

        private static void Validate(int value) {
            if (value < 0 || value > 255)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("color.value.outside.range.0.255"));
        }

        public override String ToString() {
            return "Color value[" + value.ToString("X") + "]";
        }
    }
}
