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

using System;

namespace iTextSharp.awt.geom
{
    public abstract class Point2D
    {
        public class Float : Point2D {

            public float x;
            public float y;

            public Float() {
            }

            public Float(float x, float y) {
                this.x = x;
                this.y = y;
            }

            public override double GetX() {
                return x;
            }

            public override double GetY() {
                return y;
            }

            virtual public void SetLocation(float x, float y) {
                this.x = x;
                this.y = y;
            }

            public override void SetLocation(double x, double y) {
                this.x = (float)x;
                this.y = (float)y;
            }

            public override string ToString() {
                return "Point2D:[x=" + x + ",y=" + y + "]"; //$NON-NLS-1$ //$NON-NLS-2$ //$NON-NLS-3$
            }
        }

        public class Double : Point2D {

            public double x;
            public double y;

            public Double() {
            }

            public Double(double x, double y) {
                this.x = x;
                this.y = y;
            }

            public override double GetX() {
                return x;
            }

            public override double GetY() {
                return y;
            }

            public override void SetLocation(double x, double y) {
                this.x = x;
                this.y = y;
            }

            public override string ToString() {
                return "Point2D: [x=" + x + ",y=" + y + "]"; //$NON-NLS-1$ //$NON-NLS-2$ //$NON-NLS-3$
            }
        }

        protected Point2D() {
        }

        public abstract double GetX();

        public abstract double GetY();

        public abstract void SetLocation(double x, double y);

        virtual public void SetLocation(Point2D p) {
            SetLocation(p.GetX(), p.GetY());
        }

        public static double DistanceSq(double x1, double y1, double x2, double y2) {
            x2 -= x1;
            y2 -= y1;
            return x2 * x2 + y2 * y2;
        }

        virtual public double DistanceSq(double px, double py) {
            return Point2D.DistanceSq(GetX(), GetY(), px, py);
        }

        virtual public double DistanceSq(Point2D p) {
            return Point2D.DistanceSq(GetX(), GetY(), p.GetX(), p.GetY());
        }

        public static double Distance(double x1, double y1, double x2, double y2) {
            return Math.Sqrt(DistanceSq(x1, y1, x2, y2));
        }

        virtual public double Distance(double px, double py) {
            return Math.Sqrt(DistanceSq(px, py));
        }

        virtual public double Distance(Point2D p) {
            return Math.Sqrt(DistanceSq(p));
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            if (obj is Point2D) {
                Point2D p = (Point2D) obj;
                return GetX() == p.GetX() && GetY() == p.GetY();
            }
            return false;
        }

        public override int GetHashCode() {
            return GetX().GetHashCode() + GetY().GetHashCode();
        }
    }
}
