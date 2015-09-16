/*
 * $Id$
 *
 * This file is part of the iText (R) project.
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

using iTextSharp.awt.geom;

namespace System.util {
    public class RectangleJ {
        public const int OUT_LEFT = 1;
        public const int OUT_TOP = 2;
        public const int OUT_RIGHT = 4;
        public const int OUT_BOTTOM = 8;

        private float x;
        private float y;
        private float width;
        private float height;

        public RectangleJ(float x, float y, float width, float height) {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public RectangleJ(iTextSharp.text.Rectangle rect) {
            rect.Normalize();
            x = rect.Left;
            y = rect.Bottom;
            width = rect.Width;
            height = rect.Height;
        }

        virtual public float X {
            get {
                return this.x;
            }
            set {
                this.x = value;
            }
        }
        virtual public float Y {
            get {
                return this.y;
            }
            set {
                this.y = value;
            }
        }
        virtual public float Width {
            get {
                return this.width;
            }
            set {
                this.width = value;
            }
        }
        virtual public float Height {
            get {
                return this.height;
            }
            set {
                this.height = value;
            }
        }

        virtual public void Add(RectangleJ rect) {
            float x1 = Math.Min(Math.Min(x, x + width), Math.Min(rect.x, rect.x + rect.width));
            float x2 = Math.Max(Math.Max(x, x + width), Math.Max(rect.x, rect.x + rect.width));
            float y1 = Math.Min(Math.Min(y, y + height), Math.Min(rect.y, rect.y + rect.height));
            float y2 = Math.Max(Math.Max(y, y + height), Math.Max(rect.y, rect.y + rect.height));
            x = x1;
            y = y1;
            width = x2 - x1;
            height = y2 - y1;
        }

        virtual public int Outcode(double x, double y) {
            int outp = 0;
            if (this.width <= 0) {
                outp |= OUT_LEFT | OUT_RIGHT;
            }
            else if (x < this.x) {
                outp |= OUT_LEFT;
            }
            else if (x > this.x + (double)this.width) {
                outp |= OUT_RIGHT;
            }
            if (this.height <= 0) {
                outp |= OUT_TOP | OUT_BOTTOM;
            }
            else if (y < this.y) {
                outp |= OUT_TOP;
            }
            else if (y > this.y + (double)this.height) {
                outp |= OUT_BOTTOM;
            }
            return outp;
        }

        virtual public bool IntersectsLine(double x1, double y1, double x2, double y2) {
            int out1, out2;
            if ((out2 = Outcode(x2, y2)) == 0) {
                return true;
            }
            while ((out1 = Outcode(x1, y1)) != 0) {
                if ((out1 & out2) != 0) {
                    return false;
                }
                if ((out1 & (OUT_LEFT | OUT_RIGHT)) != 0) {
                    float x = X;
                    if ((out1 & OUT_RIGHT) != 0) {
                        x += Width;
                    }
                    y1 = y1 + (x - x1) * (y2 - y1) / (x2 - x1);
                    x1 = x;
                }
                else {
                    float y = Y;
                    if ((out1 & OUT_BOTTOM) != 0) {
                        y += Height;
                    }
                    x1 = x1 + (y - y1) * (x2 - x1) / (y2 - y1);
                    y1 = y;
                }
            }
            return true;
        }

        virtual public RectangleJ Intersection(RectangleJ r) {
            float x1 = Math.Max(x, r.x);
            float y1 = Math.Max(y, r.y);
            float x2 = Math.Min(x + width, r.x + r.width);
            float y2 = Math.Min(y + height, r.y + r.height);
            return new RectangleJ(x1, y1, x2 - x1, y2 - y1);
        }

        virtual public bool IsEmpty() {
            return width <= 0 || height <= 0;
        }

        public virtual bool Contains(Point2D point) {
            return Contains(point.GetX(), point.GetY());
        }

        public virtual bool Contains(double x, double y) {
            if (IsEmpty()) {
                return false;
            }

            if (x < this.x || y < this.y) {
                return false;
            }

            x -= this.x;
            y -= this.y;

            return x < width && y < height;
        }
    }
}
