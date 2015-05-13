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

        public PdfCleanUpRegionFilter(IList<Rectangle> rectangles) {
            this.rectangles = rectangles;
        }

        public override bool AllowText(TextRenderInfo renderInfo) {
            LineSegment ascent = renderInfo.GetAscentLine();
            LineSegment descent = renderInfo.GetDescentLine();

            Rectangle r1 = new Rectangle(Math.Min(descent.GetStartPoint()[0], descent.GetEndPoint()[0]),
                                         descent.GetStartPoint()[1],
                                         Math.Max(descent.GetStartPoint()[0], descent.GetEndPoint()[0]),
                                         ascent.GetEndPoint()[1]);

            foreach (Rectangle rectangle in rectangles) {
                if (Intersect(r1, rectangle)) {
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
         *         {@link com.itextpdf.text.Rectangle} objects otherwise.
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

        protected internal virtual Path FilterStrokePath(Path path, Matrix ctm, float lineWidth, int lineCapStyle,
                                                int lineJoinStyle, float miterLimit, LineDashPattern lineDashPattern) {
            JoinType joinType = GetJoinType(lineJoinStyle);
            EndType endType = GetEndType(lineCapStyle);

            if (lineDashPattern != null) {
                if (IsZeroDash(lineDashPattern)) {
                    return new Path();
                }
                
                if (!IsSolid(lineDashPattern)) {
                    path = ApplyDashPattern(path, lineDashPattern);
                }
            }

            ClipperOffset offset = new ClipperOffset(miterLimit, PdfCleanUpProcessor.ArcTolerance * PdfCleanUpProcessor.FloatMultiplier);
            AddPath(offset, path, joinType, endType);

            PolyTree resultTree = new PolyTree();
            offset.Execute(ref resultTree, lineWidth * PdfCleanUpProcessor.FloatMultiplier / 2);

            return FilterFillPath(ConvertToPath(resultTree), ctm, PathPaintingRenderInfo.NONZERO_WINDING_RULE);
        }

        /**
         * @param fillingRule If the subpath is contour, pass any value.
         */
        protected internal virtual Path FilterFillPath(Path path, Matrix ctm, int fillingRule) {
            Clipper clipper = new Clipper();
            AddPath(clipper, path);

            foreach (Rectangle rectangle in rectangles) {
                Point2D[] transfRectVertices = TransformPoints(ctm, true, GetVertices(rectangle));
                AddRect(clipper, transfRectVertices);
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

        private static void AddPath(ClipperOffset offset, Path path, JoinType joinType, EndType endType) {
            foreach (Subpath subpath in path.Subpaths) {
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
        }

        private static void AddPath(Clipper clipper, Path path) {
            foreach (Subpath subpath in path.Subpaths) {
                if (!subpath.IsSinglePointClosed() && !subpath.IsSinglePointOpen()) {
                    IList<Point2D> linearApproxPoints = subpath.GetPiecewiseLinearApproximation();
                    clipper.AddPath(ConvertToIntPoints(linearApproxPoints), PolyType.ptSubject, subpath.Closed);
                }
            }
        }

        private static void AddRect(Clipper clipper, Point2D[] rectVertices) {
            clipper.AddPath(ConvertToIntPoints(new List<Point2D>(rectVertices)), PolyType.ptClip, true);
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


        private bool Intersect(Rectangle r1, Rectangle r2) {
            return (r1.Left < r2.Right && r1.Right > r2.Left &&
                    r1.Bottom < r2.Top && r1.Top > r2.Bottom);
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

        private static bool IsZeroDash(LineDashPattern lineDashPattern) {
            PdfArray dashArray = lineDashPattern.DashArray;
            float total = 0;

            // We should only iterate over the numbers specifying lengths of dashes
            for (int i = 0; i < dashArray.Size; i += 2) {
                float currentDash = dashArray.GetAsNumber(i).FloatValue;
                // Should be nonnegative according to spec.
                if (currentDash < 0) {
                    currentDash = 0;
                }

                total += currentDash;
            }

            return Util.compare(total, 0) == 0;
        }

        private static bool IsSolid(LineDashPattern lineDashPattern) {
            return lineDashPattern.DashArray.IsEmpty();
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

                        while ((Util.compare(remainingDist, 0) == 0) && !dashedPath.CurrentPoint.Equals(subpathApprox[i])) {
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

        private static bool ContainsAll(RectangleJ rect, params Point2D[] points) {
            foreach (Point2D point in points) {
                if (!rect.Contains(point)) {
                    return false;
                }
            }

            return true;
        }

        private Point2D[] TransformPoints(Matrix transormationMatrix, bool inverse, ICollection<Point2D> points) {
            Point2D[] pointsArr = new Point2D[points.Count];
            points.CopyTo(pointsArr, 0);

            return TransformPoints(transormationMatrix, inverse, pointsArr);
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
    }
}
