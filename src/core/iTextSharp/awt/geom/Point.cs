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

namespace iTextSharp.awt.geom
{
    public class Point : Point2D
    {
        public double x;
        public double y;

        public Point() {
            SetLocation(0, 0);
        }

        public Point(int x, int y) {
            SetLocation(x, y);
        }

        public Point(double x, double y) {
            SetLocation(x, y);
        }

        public Point(Point p) {
            SetLocation(p.x, p.y);
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            if (obj is Point) {
                Point p = (Point)obj;
                return x == p.x && y == p.y;
            }
            return false;
        }

        public override string ToString() {
            return "Point: [x=" + x + ",y=" + y + "]"; //$NON-NLS-1$ //$NON-NLS-2$ //$NON-NLS-3$
        }

        public override double GetX() {
            return x;
        }

        public override double GetY() {
            return y;
        }

        virtual public Point GetLocation() {
            return new Point(x, y);
        }

        virtual public void SetLocation(Point p) {
            SetLocation(p.x, p.y);
        }

        virtual public void SetLocation(int x, int y) {
            SetLocation((double)x, (double)y);
        }

        public override void SetLocation(double x, double y) {
    	    this.x = x;
    	    this.y = y;
        }

        virtual public void Move(int x, int y) {
            Move((double)x, (double)y);
        }

        virtual public void Move(double x, double y) {
            SetLocation(x, y);
        }

        virtual public void Translate(int dx, int dy) {
            Translate((double)x, (double)y);
        }
        virtual public void Translate(double dx, double dy) {
            x += dx;
            y += dy;
        } 
    }
}
