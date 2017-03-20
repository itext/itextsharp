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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.util;
using iTextSharp.awt.geom;
using iTextSharp.text;
using iTextSharp.text.pdf.parser;
using System.util;
using System.util.collections;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser.clipper;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {

    class PdfCleanUpRegionFilter : RenderFilter {

        private IList<Rectangle> rectangles;

        private static readonly double circleApproximationConst = 0.55191502449;

        public PdfCleanUpRegionFilter(IList<Rectangle> rectangles) {
            this.rectangles = rectangles;
        }

        public override bool AllowText(TextRenderInfo renderInfo) {
            LineSegment ascent = renderInfo.GetAscentLine();
            LineSegment descent = renderInfo.GetDescentLine();

        Point2D[] glyphRect = new Point2D[] {
                new Point2D.Float(ascent.GetStartPoint()[0], ascent.GetStartPoint()[1]),
                new Point2D.Float(ascent.GetEndPoint()[0], ascent.GetEndPoint()[1]),
                new Point2D.Float(descent.GetEndPoint()[0], descent.GetEndPoint()[1]),
                new Point2D.Float(descent.GetStartPoint()[0], descent.GetStartPoint()[1]),
        };

            foreach (Rectangle rectangle in rectangles) {
                Point2D[] redactRect = GetVertices(rectangle);

                if (Intersect(glyphRect, redactRect)) {
                    return false;
                }
            }

            return true;
        }

        public override bool AllowImage(ImageRenderInfo renderInfo) {
            throw new NotImplementedException();
        }

        /**
         * Calculates intersection of the image and the render filter region in the coordinate system relative to the image.
         * 
         * @return <code>null</code> if the image is not allowed, {@link java.util.List} of 
         * {@link com.itextpdf.text.Rectangle} objects otherwise.
         */
        protected internal virtual IList<Rectangle> GetCoveredAreas(ImageRenderInfo renderInfo) {
            Rectangle imageRect = CalcImageRect(renderInfo);
            IList<Rectangle> coveredAreas = new List<Rectangle>();

            if (imageRect == null) {
                return null;
            }

            foreach (Rectangle rectangle in rectangles) {
                Rectangle intersectionRect = Intersection(imageRect, rectangle);

                if (intersectionRect != null) {
                    // True if the image is completely covered
                    if (imageRect.Equals(intersectionRect)) {
                        return null;
                    }
                    
                    coveredAreas.Add(TransformIntersection(renderInfo.GetImageCTM(), intersectionRect));
                }
            }

            return coveredAreas;
        }

        protected internal Path FilterStrokePath(Path sourcePath, Matrix ctm, float lineWidth, int lineCapStyle,
                                int lineJoinStyle, float miterLimit, LineDashPattern lineDashPattern) {
            Path path = sourcePath;
            JoinType joinType = GetJoinType(lineJoinStyle);
            EndType endType = GetEndType(lineCapStyle);

            if (lineDashPattern != null && !lineDashPattern.IsSolid()) {
                path = ApplyDashPattern(path, lineDashPattern);
            }

            ClipperOffset offset = new ClipperOffset(miterLimit, PdfCleanUpProcessor.ArcTolerance * PdfCleanUpProcessor.FloatMultiplier);
            IList<Subpath> degenerateSubpaths = AddPath(offset, path, joinType, endType);

            PolyTree resultTree = new PolyTree();
            offset.Execute(ref resultTree, lineWidth * PdfCleanUpProcessor.FloatMultiplier / 2);
            Path offsetedPath = ConvertToPath(resultTree);

            if (degenerateSubpaths.Count > 0) {
                if (endType == EndType.etOpenRound) {
                    IList<Subpath> circles = ConvertToCircles(degenerateSubpaths, lineWidth / 2);
                    offsetedPath.AddSubpaths(circles);
                } else if (endType == EndType.etOpenSquare && lineDashPattern != null) {
                    IList<Subpath> squares = ConvertToSquares(degenerateSubpaths, lineWidth, sourcePath);
                    offsetedPath.AddSubpaths(squares);
                }
            }

            return FilterFillPath(offsetedPath, ctm, PathPaintingRenderInfo.NONZERO_WINDING_RULE);
        }

        /**
         * Note: this method will close all unclosed subpaths of the passed path.
         *
         * @param fillingRule If the subpath is contour, pass any value.
         */
        protected internal Path FilterFillPath(Path path, Matrix ctm, int fillingRule) {
            path.CloseAllSubpaths();

            Clipper clipper = new Clipper();
            AddPath(clipper, path);

            foreach (Rectangle rectangle in rectangles) {
                Point2D[] transfRectVertices = TransformPoints(ctm, true, GetVertices(rectangle));
                AddRect(clipper, transfRectVertices, PolyType.ptClip);
            }

            PolyFillType fillType = PolyFillType.pftNonZero;

            if (fillingRule == PathPaintingRenderInfo.EVEN_ODD_RULE) {
                fillType = PolyFillType.pftEvenOdd;
            }

            PolyTree resultTree = new PolyTree();
            clipper.Execute(ClipType.ctDifference, resultTree, fillType, PolyFillType.pftNonZero);

            return ConvertToPath(resultTree);
        }

        private static JoinType GetJoinType(int lineJoinStyle) {
            switch (lineJoinStyle) {
                case PdfContentByte.LINE_JOIN_BEVEL:
                    return JoinType.jtBevel;

                case PdfContentByte.LINE_JOIN_MITER:
                    return JoinType.jtMiter;
            }

            return JoinType.jtRound;
        }

        private static EndType GetEndType(int lineCapStyle) {
            switch (lineCapStyle) {
                case PdfContentByte.LINE_CAP_BUTT:
                    return EndType.etOpenButt;

                case PdfContentByte.LINE_CAP_PROJECTING_SQUARE:
                    return EndType.etOpenSquare;
            }

            return EndType.etOpenRound;
        }

        /**
     * Converts specified degenerate subpaths to circles.
     * Note: actually the resultant subpaths are not real circles but approximated.
     *
     * @param radius Radius of each constructed circle.
     * @return {@link java.util.List} consisting of circles constructed on given degenerated subpaths.
     */
        private static IList<Subpath> ConvertToCircles(IList<Subpath> degenerateSubpaths, double radius) {
            IList<Subpath> circles = new List<Subpath>(degenerateSubpaths.Count);

            foreach (Subpath subpath in degenerateSubpaths) {
                BezierCurve[] circleSectors = ApproximateCircle(subpath.GetStartPoint(), radius);

                Subpath circle = new Subpath();
                circle.AddSegment(circleSectors[0]);
                circle.AddSegment(circleSectors[1]);
                circle.AddSegment(circleSectors[2]);
                circle.AddSegment(circleSectors[3]);

                circles.Add(circle);
            }

            return circles;
        }

        /**
         * Converts specified degenerate subpaths to squares.
         * Note: the list of degenerate subpaths should contain at least 2 elements. Otherwise
         * we can't determine the direction which the rotation of each square depends on.
         *
         * @param squareWidth Width of each constructed square.
         * @param sourcePath The path which dash pattern applied to. Needed to calc rotation angle of each square.
         * @return {@link java.util.List} consisting of squares constructed on given degenerated subpaths.
         */
        private static IList<Subpath> ConvertToSquares(IList<Subpath> degenerateSubpaths, double squareWidth, Path sourcePath) {
            IList<Point2D> pathApprox = GetPathApproximation(sourcePath);

            if (pathApprox.Count < 2) {
                return new List<Subpath>();
            }

            IEnumerator<Point2D> approxIter = pathApprox.GetEnumerator();

            approxIter.MoveNext();
            Point2D approxPt1 = approxIter.Current;

            approxIter.MoveNext();
            Point2D approxPt2 = approxIter.Current;

            StandardLine line = new StandardLine(approxPt1, approxPt2);

            IList<Subpath> squares = new List<Subpath>(degenerateSubpaths.Count);
            float widthHalf = (float) squareWidth / 2;

            for (int i = 0; i < degenerateSubpaths.Count; ++i) {
                Point2D point = degenerateSubpaths[i].GetStartPoint();

                while (!line.Contains(point)) {
                    approxPt1 = approxPt2;

                    approxIter.MoveNext();
                    approxPt2 = approxIter.Current;

                    line = new StandardLine(approxPt1, approxPt2);
                }

                float slope = line.GetSlope();
                double angle;

                if (!float.IsPositiveInfinity(slope)) {
                    angle = Math.Atan(slope);
                } else {
                    angle = Math.PI / 2;
                }

                squares.Add(ConstructSquare(point, widthHalf, angle));
            }

            return squares;
        }

        private static IList<Point2D> GetPathApproximation(Path path) {
            IList<Point2D> approx = new List<Point2D>();

            foreach (Subpath subpath in path.Subpaths) {
                IList<Point2D> subpathApprox = subpath.GetPiecewiseLinearApproximation();
                Point2D prevPoint = null;

                foreach (Point2D approxPt in subpathApprox) {
                    if (!approxPt.Equals(prevPoint)) {
                        approx.Add(approxPt);
                        prevPoint = approxPt;
                    }
                }
            }

            return approx;
        }

        private static Subpath ConstructSquare(Point2D squareCenter, double widthHalf, double rotationAngle) {
            // Orthogonal square is the square with sides parallel to one of the axes.
            Point2D[] ortogonalSquareVertices = {
                new Point2D.Double(-widthHalf, -widthHalf),
                new Point2D.Double(-widthHalf, widthHalf),
                new Point2D.Double(widthHalf, widthHalf),
                new Point2D.Double(widthHalf, -widthHalf)
        };

            Point2D[] rotatedSquareVertices = GetRotatedSquareVertices(ortogonalSquareVertices, rotationAngle, squareCenter);

            Subpath square = new Subpath();
            square.AddSegment(new Line(rotatedSquareVertices[0], rotatedSquareVertices[1]));
            square.AddSegment(new Line(rotatedSquareVertices[1], rotatedSquareVertices[2]));
            square.AddSegment(new Line(rotatedSquareVertices[2], rotatedSquareVertices[3]));
            square.AddSegment(new Line(rotatedSquareVertices[3], rotatedSquareVertices[0]));

            return square;
        }

        private static Point2D[] GetRotatedSquareVertices(Point2D[] orthogonalSquareVertices, double angle, Point2D squareCenter) {
            Point2D[] rotatedSquareVertices = new Point2D[orthogonalSquareVertices.Length];

            AffineTransform.GetRotateInstance(angle).
                    Transform(orthogonalSquareVertices, 0, rotatedSquareVertices, 0, rotatedSquareVertices.Length);
            AffineTransform.GetTranslateInstance(squareCenter.GetX(), squareCenter.GetY()).
                    Transform(rotatedSquareVertices, 0, rotatedSquareVertices, 0, orthogonalSquareVertices.Length);

            return rotatedSquareVertices;
        }

        /**
         * Adds all subpaths of the path to the {@link ClipperOffset} object with one
         * note: it doesn't add degenerate subpaths.
         *
         * @return {@link java.util.List} consisting of all degenerate subpaths of the path.
         */
        private static IList<Subpath> AddPath(ClipperOffset offset, Path path, JoinType joinType, EndType endType) {
            IList<Subpath> degenerateSubpaths = new List<Subpath>();

            foreach (Subpath subpath in path.Subpaths) {
                if (subpath.IsDegenerate()) {
                    degenerateSubpaths.Add(subpath);
                    continue;
                }

                if (!subpath.IsSinglePointClosed() && !subpath.IsSinglePointOpen()) {
                    EndType et;

                    if (subpath.Closed) {
                        // Offsetting is never used for path being filled
                        et = EndType.etClosedLine;
                    } else {
                        et = endType;
                    }

                    IList<Point2D> linearApproxPoints = subpath.GetPiecewiseLinearApproximation();
                    offset.AddPath(ConvertToIntPoints(linearApproxPoints), joinType, et);
                }
            }

            return degenerateSubpaths;
        }

        private static BezierCurve[] ApproximateCircle(Point2D center, double radius) {
            // The circle is split into 4 sectors. Arc of each sector
            // is approximated  with bezier curve separately.
            BezierCurve[] approximation = new BezierCurve[4];
            double x = center.GetX();
            double y = center.GetY();

            approximation[0] = new BezierCurve(new List<Point2D>(new Point2D[] {
                new Point2D.Double(x, y + radius),
                new Point2D.Double(x + radius * circleApproximationConst, y + radius),
                new Point2D.Double(x + radius, y + radius * circleApproximationConst),
                new Point2D.Double(x + radius, y)
            }));

            approximation[1] = new BezierCurve(new List<Point2D>(new Point2D[] {
                new Point2D.Double(x + radius, y),
                new Point2D.Double(x + radius, y - radius * circleApproximationConst),
                new Point2D.Double(x + radius * circleApproximationConst, y - radius),
                new Point2D.Double(x, y - radius)
            }));

            approximation[2] = new BezierCurve(new List<Point2D>(new Point2D[] {
                new Point2D.Double(x, y - radius),
                new Point2D.Double(x - radius * circleApproximationConst, y - radius),
                new Point2D.Double(x - radius, y - radius * circleApproximationConst),
                new Point2D.Double(x - radius, y)
            }));

            approximation[3] = new BezierCurve(new List<Point2D>(new Point2D[] {
                new Point2D.Double(x - radius, y),
                new Point2D.Double(x - radius, y + radius * circleApproximationConst),
                new Point2D.Double(x - radius * circleApproximationConst, y + radius),
                new Point2D.Double(x, y + radius)
            }));

            return approximation;
        }

        private static void AddPath(Clipper clipper, Path path) {
            foreach (Subpath subpath in path.Subpaths) {
                if (!subpath.IsSinglePointClosed() && !subpath.IsSinglePointOpen()) {
                    IList<Point2D> linearApproxPoints = subpath.GetPiecewiseLinearApproximation();
                    clipper.AddPath(ConvertToIntPoints(linearApproxPoints), PolyType.ptSubject, subpath.Closed);
                }
            }
        }

        private static void AddRect(Clipper clipper, Point2D[] rectVertices, PolyType polyType) {
            clipper.AddPath(ConvertToIntPoints(new List<Point2D>(rectVertices)), polyType, true);
        }

        private static List<IntPoint> ConvertToIntPoints(IList<Point2D> points) {
            List<IntPoint> convertedPoints = new List<IntPoint>(points.Count);

            foreach (Point2D point in points) {
                convertedPoints.Add(new IntPoint(PdfCleanUpProcessor.FloatMultiplier * point.GetX(), 
                                                 PdfCleanUpProcessor.FloatMultiplier * point.GetY()));
            }

            return convertedPoints;
        }

        private static IList<Point2D> ConvertToFloatPoints(IList<IntPoint> points) {
            IList<Point2D> convertedPoints = new List<Point2D>(points.Count);

            foreach (IntPoint point in points) {
                convertedPoints.Add(new Point2D.Float((float) (point.X / PdfCleanUpProcessor.FloatMultiplier),
                                                      (float) (point.Y / PdfCleanUpProcessor.FloatMultiplier)));
            }

            return convertedPoints;
        }

        private static Path ConvertToPath(PolyTree result) {
            Path path = new Path();
            PolyNode node = result.GetFirst();

            while (node != null) {
                AddContour(path, node.Contour, !node.IsOpen);
                node = node.GetNext();
            }

            return path;
        }

        private static void AddContour(Path path, List<IntPoint> contour, Boolean close) {
            IList<Point2D> floatContour = ConvertToFloatPoints(contour);

            Point2D point = floatContour[0];
            path.MoveTo((float) point.GetX(), (float) point.GetY());

            for (int i = 1; i < floatContour.Count; ++i) {
                point = floatContour[i];
                path.LineTo((float) point.GetX(), (float) point.GetY());
            }

            if (close) {
                path.CloseSubpath();
            }
        }

        private Point2D[] GetVertices(Rectangle rect) {
            Point2D[] points = {
                new Point2D.Double(rect.Left, rect.Bottom),
                new Point2D.Double(rect.Right, rect.Bottom),
                new Point2D.Double(rect.Right, rect.Top),
                new Point2D.Double(rect.Left, rect.Top)
            };

            return points;
        }


        private bool Intersect(Point2D[] rect1, Point2D[] rect2) {
            Clipper clipper = new Clipper();
            AddRect(clipper, rect1, PolyType.ptSubject);
            AddRect(clipper, rect2, PolyType.ptClip);

            List<List<IntPoint>> paths = new List<List<IntPoint>>();
            clipper.Execute(ClipType.ctIntersection, paths, PolyFillType.pftNonZero, PolyFillType.pftNonZero);

            return paths.Count != 0;
        }

        /**
         * @return Image boundary rectangle in device space.
         */
        private Rectangle CalcImageRect(ImageRenderInfo renderInfo) {
            Matrix ctm = renderInfo.GetImageCTM();

            if (ctm == null) {
                return null;
            }

            Point2D[] points = TransformPoints(ctm, false, new Point(0, 0), new Point(0, 1),
                                                           new Point(1, 0), new Point(1, 1));
            return GetRectangle(points[0], points[1], points[2], points[3]);
        }

        /**
         * @return null if the intersection is empty, {@link com.itextpdf.text.Rectangle} representing intersection otherwise
         */
        private Rectangle Intersection(Rectangle rect1, Rectangle rect2) {
            RectangleJ awtRect1 = new RectangleJ(rect1);
            RectangleJ awtRect2 = new RectangleJ(rect2);
            RectangleJ awtIntersection = awtRect1.Intersection(awtRect2);

            return awtIntersection.IsEmpty() ? null : new Rectangle(awtIntersection);
        }

        /**
         * Transforms the given Rectangle into the image coordinate system which is [0,1]x[0,1] by default
         */
        private Rectangle TransformIntersection(Matrix imageCTM, Rectangle rect) {
            Point2D[] points = TransformPoints(imageCTM, true, new Point(rect.Left, rect.Bottom),
                                                               new Point(rect.Left, rect.Top),
                                                               new Point(rect.Right, rect.Bottom),
                                                               new Point(rect.Right, rect.Top));
            return GetRectangle(points[0], points[1], points[2], points[3]);
        }

        private Rectangle GetRectangle(Point2D p1, Point2D p2, Point2D p3, Point2D p4) {
            double[] xs = { p1.GetX(), p2.GetX(), p3.GetX(), p4.GetX() };
            double[] ys = { p1.GetY(), p2.GetY(), p3.GetY(), p4.GetY() };

            double left = Util.Min(xs);
            double bottom = Util.Min(ys);
            double right = Util.Max(xs);
            double top = Util.Max(ys);

            return new Rectangle((float)left, (float)bottom, (float)right, (float)top);
        }

        private static Path ApplyDashPattern(Path path, LineDashPattern lineDashPattern) {
            HashSet2<int> modifiedSubpaths = new HashSet2<int>(path.ReplaceCloseWithLine());
            Path dashedPath = new Path();
            int currentSubpath = 0;

            foreach (Subpath subpath in path.Subpaths) {
                IList<Point2D> subpathApprox = subpath.GetPiecewiseLinearApproximation();

                if (subpathApprox.Count > 1) {
                    dashedPath.MoveTo((float) subpathApprox[0].GetX(), (float) subpathApprox[0].GetY());
                    float remainingDist = 0;
                    bool remainingIsGap = false;

                    for (int i = 1; i < subpathApprox.Count; ++i) {
                        Point2D nextPoint = null;

                        if (remainingDist != 0) {
                            nextPoint = GetNextPoint(subpathApprox[i - 1], subpathApprox[i], remainingDist);
                            remainingDist = ApplyDash(dashedPath, subpathApprox[i - 1], subpathApprox[i], nextPoint, remainingIsGap);
                        }

                        while ((Util.Compare(remainingDist, 0) == 0) && !dashedPath.CurrentPoint.Equals(subpathApprox[i])) {
                            LineDashPattern.DashArrayElem currentElem = lineDashPattern.Next();
                            nextPoint = GetNextPoint(nextPoint ?? subpathApprox[i - 1], subpathApprox[i], currentElem.Value);
                            remainingDist = ApplyDash(dashedPath, subpathApprox[i - 1], subpathApprox[i], nextPoint, currentElem.IsGap);
                            remainingIsGap = currentElem.IsGap;
                        }
                    }

                    // If true, then the line closing the subpath was explicitly added (see Path.ReplaceCloseWithLine).
                    // This causes a loss of a visual effect of line join style parameter, so in this clause
                    // we simply add overlapping dash (or gap, no matter), which continues the last dash and equals to 
                    // the first dash (or gap) of the path.
                    if (modifiedSubpaths.Contains(currentSubpath)) {
                        lineDashPattern.Reset();
                        LineDashPattern.DashArrayElem currentElem = lineDashPattern.Next();
                        Point2D nextPoint = GetNextPoint(subpathApprox[0], subpathApprox[1], currentElem.Value);
                        ApplyDash(dashedPath, subpathApprox[0], subpathApprox[1], nextPoint, currentElem.IsGap);
                    }
                }

                // According to PDF spec. line dash pattern should be restarted for each new subpath.
                lineDashPattern.Reset();
                ++currentSubpath;
            }

            return dashedPath;
        }

        private static Point2D GetNextPoint(Point2D segStart, Point2D segEnd, float dist) {
            Point2D vector = ComponentwiseDiff(segEnd, segStart);
            Point2D unitVector = GetUnitVector(vector);

            return new Point2D.Float((float) (segStart.GetX() + dist * unitVector.GetX()),
                                     (float) (segStart.GetY() + dist * unitVector.GetY()));
        }

        private static Point2D ComponentwiseDiff(Point2D minuend, Point2D subtrahend) {
            return new Point2D.Float((float) (minuend.GetX() - subtrahend.GetX()),
                                     (float) (minuend.GetY() - subtrahend.GetY()));
        }

        private static Point2D GetUnitVector(Point2D vector) {
            double vectorLength = GetVectorEuclideanNorm(vector);
            return new Point2D.Float((float) (vector.GetX() / vectorLength),
                                     (float) (vector.GetY() / vectorLength));
        }

        private static double GetVectorEuclideanNorm(Point2D vector) {
            return vector.Distance(0, 0);
        }

        private static float ApplyDash(Path dashedPath, Point2D segStart, Point2D segEnd, Point2D dashTo, bool isGap) {
            float remainingDist = 0;

            if (!LiesOnSegment(segStart, segEnd, dashTo)) {
                remainingDist = (float) dashTo.Distance(segEnd);
                dashTo = segEnd;
            }

            if (isGap) {
                dashedPath.MoveTo((float) dashTo.GetX(), (float) dashTo.GetY());
            } else {
                dashedPath.LineTo((float) dashTo.GetX(), (float) dashTo.GetY());
            }

            return remainingDist;
        }

        private static bool LiesOnSegment(Point2D segStart, Point2D segEnd, Point2D point) {
            return point.GetX() >= Math.Min(segStart.GetX(), segEnd.GetX()) &&
                   point.GetX() <= Math.Max(segStart.GetX(), segEnd.GetX()) &&
                   point.GetY() >= Math.Min(segStart.GetY(), segEnd.GetY()) &&
                   point.GetY() <= Math.Max(segStart.GetY(), segEnd.GetY());
        }

        private Point2D[] TransformPoints(Matrix transormationMatrix, bool inverse, params Point2D[] points) {
            AffineTransform t = new AffineTransform(transormationMatrix[Matrix.I11], transormationMatrix[Matrix.I12],
                                                    transormationMatrix[Matrix.I21], transormationMatrix[Matrix.I22],
                                                    transormationMatrix[Matrix.I31], transormationMatrix[Matrix.I32]);
            Point2D[] transformed = new Point2D[points.Length];

            if (inverse) {
                t = t.CreateInverse();
            }

            t.Transform(points, 0, transformed, 0, points.Length);

            return transformed;
        }

        // Constants from the standard line representation: Ax+By+C
        private class StandardLine {

            float A;
            float B;
            float C;

            internal StandardLine(Point2D p1, Point2D p2) {
                A = (float) (p2.GetY() - p1.GetY());
                B = (float) (p1.GetX() - p2.GetX());
                C = (float) (p1.GetY() * (-B) - p1.GetX() * A);
            }

            internal float GetSlope() {
                if (Util.Compare(B, 0) == 0) {
                    return Single.PositiveInfinity;
                }

                return -A / B;
            }

            internal bool Contains(Point2D point) {
                return Util.Compare(Math.Abs(A * (float) point.GetX() + B * (float) point.GetY() + C), 0.1f) < 0;
            }
        }
    }
}
