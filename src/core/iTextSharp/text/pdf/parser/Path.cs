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
using System.Collections.Generic;
using System.util;
using iTextSharp.awt.geom;

namespace iTextSharp.text.pdf.parser {

    /**
     * Paths define shapes, trajectories, and regions of all sorts. They shall be used
     * to draw lines, define the shapes of filled areas, and specify boundaries for clipping
     * other graphics. A path shall be composed of straight and curved line segments, which
     * may connect to one another or may be disconnected.
     *
     * @since 5.5.6
     */
    public class Path {

        private static readonly String START_PATH_ERR_MSG = "Path shall start with \"re\" or \"m\" operator";

        private IList<Subpath> subpaths = new List<Subpath>();
        private Point2D currentPoint;

        public Path() {
        }

        public Path(IList<Subpath> subpaths) {
            AddSubpaths(subpaths);
        }

        /**
         * @return A {@link java.util.List} of subpaths forming this path.
         */
        public virtual IList<Subpath> Subpaths {
            get { return subpaths; }
        }

        /**
         * Adds the subpath to this path.
         *
         * @param subpath The subpath to be added to this path.
         */
        public void AddSubpath(Subpath subpath) {
            subpaths.Add(subpath);
            currentPoint = subpath.GetLastPoint();
        }

        /**
         * Adds the subpaths to this path.
         *
         * @param subpaths {@link java.util.List} of subpaths to be added to this path.
         */
        public void AddSubpaths(IList<Subpath> subpaths) {
            if (subpaths.Count > 0) {
                Util.AddAll(this.subpaths, subpaths);
                currentPoint = this.subpaths[subpaths.Count - 1].GetLastPoint();
            }
        }

        /**
         * The current point is the trailing endpoint of the segment most recently added to the current path.
         *
         * @return The current point.
         */
        public virtual Point2D CurrentPoint {
            get { return currentPoint; }
        }

        /**
         * Begins a new subpath by moving the current point to coordinates <CODE>(x, y)</CODE>.
         */
        public virtual void MoveTo(float x, float y) {
            currentPoint = new Point2D.Float(x, y);
            Subpath lastSubpath = this.LastSubpath;

            if (lastSubpath != null && lastSubpath.IsSinglePointOpen()) {
                lastSubpath.SetStartPoint(currentPoint);
            } else {
                subpaths.Add(new Subpath(currentPoint));
            }
        }

        /**
         * Appends a straight line segment from the current point to the point <CODE>(x, y)</CODE>.
         */
        public virtual void LineTo(float x, float y) {
            if (currentPoint == null) {
                throw new Exception(START_PATH_ERR_MSG);
            }

            Point2D targetPoint = new Point2D.Float(x, y);
            this.LastSubpath.AddSegment(new Line(currentPoint, targetPoint));
            currentPoint = targetPoint;
        }

        /**
         * Appends a cubic Bezier curve to the current path. The curve shall extend from
         * the current point to the point <CODE>(x3, y3)</CODE>.
         */
        public virtual void CurveTo(float x1, float y1, float x2, float y2, float x3, float y3) {
            if (currentPoint == null) {
                throw new Exception(START_PATH_ERR_MSG);
            }
            // Numbered in natural order
            Point2D secondPoint = new Point2D.Float(x1, y1);
            Point2D thirdPoint = new Point2D.Float(x2, y2);
            Point2D fourthPoint = new Point2D.Float(x3, y3);

            IList<Point2D> controlPoints = new List<Point2D>(new Point2D[] {currentPoint, secondPoint, thirdPoint, fourthPoint});
            this.LastSubpath.AddSegment(new BezierCurve(controlPoints));

            currentPoint = fourthPoint;
        }

        /**
         * Appends a cubic Bezier curve to the current path. The curve shall extend from
         * the current point to the point <CODE>(x3, y3)</CODE> with the note that the current
         * point represents two control points.
         */
        public virtual void CurveTo(float x2, float y2, float x3, float y3) {
            if (currentPoint == null) {
                throw new Exception(START_PATH_ERR_MSG);
            }

            CurveTo((float) currentPoint.GetX(), (float) currentPoint.GetY(), x2, y2, x3, y3);
        }

        /**
         * Appends a cubic Bezier curve to the current path. The curve shall extend from
         * the current point to the point <CODE>(x3, y3)</CODE> with the note that the (x3, y3)
         * point represents two control points.
         */
        public virtual void CurveFromTo(float x1, float y1, float x3, float y3) {
            if (currentPoint == null) {
                throw new Exception(START_PATH_ERR_MSG);
            }

            CurveTo(x1, y1, x3, y3, x3, y3);
        }

        /**
         * Appends a rectangle to the current path as a complete subpath.
         */
        public virtual void Rectangle(float x, float y, float w, float h) {
            MoveTo(x, y);
            LineTo(x + w, y);
            LineTo(x + w, y + h);
            LineTo(x, y + h);
            CloseSubpath();
        }

        /**
         * Closes the current subpath.
         */
        public virtual void CloseSubpath() {
            Subpath lastSubpath = this.LastSubpath;
            lastSubpath.Closed = true;

            Point2D startPoint = lastSubpath.GetStartPoint();
            MoveTo((float) startPoint.GetX(), (float) startPoint.GetY());
        }

        /**
         * Closes all subpathes contained in this path.
         */ 
        public virtual void CloseAllSubpaths() {
            foreach (Subpath subpath in subpaths) {
                subpath.Closed = true;
            }
        }

        /**
         * Adds additional line to each closed subpath and makes the subpath unclosed. 
         * The line connects the last and the first points of the subpaths.
         * 
         * @returns Indices of modified subpaths.
         */
        public virtual IList<int> ReplaceCloseWithLine() {
            IList<int> modifiedSubpathsIndices = new List<int>();
            int i = 0;

            /* It could be replaced with "for" cycle, because IList in C# provides effective 
             * access by index. In Java List interface has at least one implementation (LinkedList)
             * which is "bad" for access elements by index.
             */
            foreach (Subpath subpath in subpaths) {
                if (subpath.Closed) {
                    subpath.Closed = false;
                    subpath.AddSegment(new Line(subpath.GetLastPoint(), subpath.GetStartPoint()));
                    modifiedSubpathsIndices.Add(i);
                }

                ++i;
            }

            return modifiedSubpathsIndices;
        }

        /**
         * Path is empty if it contains no subpaths.
         */
        public virtual bool IsEmpty() {
            return subpaths.Count == 0;
        }

        private Subpath LastSubpath {
            get { return subpaths.Count > 0 ? subpaths[subpaths.Count - 1] : null; }
        }
    }
}
