using System;
using System.Collections.Generic;
using System.Text;
using System.util;
using iTextSharp.awt.geom;
using iTextSharp.text;
using iTextSharp.text.pdf.parser;
using System.util;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser.clipper;
using LineDashPattern = iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup.PdfCleanUpGraphicsState.LineDashPattern;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {

    class PdfCleanUpRegionFilter : RenderFilter {

        private Rectangle rectangle;

        public PdfCleanUpRegionFilter(Rectangle rectangle) {
            this.rectangle = rectangle;
        }

        /**
         * Checks if the text is inside render filter region.
         */
        public override bool AllowText(TextRenderInfo renderInfo) {
            LineSegment ascent = renderInfo.GetAscentLine();
            LineSegment descent = renderInfo.GetDescentLine();

            Rectangle r1 = new Rectangle(Math.Min(descent.GetStartPoint()[0], descent.GetEndPoint()[0]),
                                         descent.GetStartPoint()[1],
                                         Math.Max(descent.GetStartPoint()[0], descent.GetEndPoint()[0]),
                                         ascent.GetEndPoint()[1]);
            Rectangle r2 = rectangle;

            return Intersect(r1, r2);
        }

        public override bool AllowImage(ImageRenderInfo renderInfo) {
            throw new NotImplementedException();
        }

        /**
         * Calculates intersection of the image and the render filter region in the coordinate system relative to the image.
         */
        protected internal virtual PdfCleanUpCoveredArea Intersection(ImageRenderInfo renderInfo) {
            Rectangle imageRect = CalcImageRect(renderInfo);

            if (imageRect == null) {
                return null;
            }

            Rectangle intersectionRect = Intersection(imageRect, rectangle);
            Rectangle transformedIntersection = null;

            if (intersectionRect != null) {
                transformedIntersection = TransformIntersection(renderInfo.GetImageCTM(), intersectionRect); 
            }

            return new PdfCleanUpCoveredArea(transformedIntersection, imageRect.Equals(intersectionRect));
        }

        protected internal virtual Path FilterStrokePath(Path path, Matrix ctm, float lineWidth, int lineCapStyle,
                                                int lineJoinStyle, float miterLimit, LineDashPattern lineDashPattern) {
            JoinType joinType = GetJoinType(lineJoinStyle);
            EndType endType = GetEndType(lineCapStyle);

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
            Point2D[] transfRectVertices = TransformPoints(ctm, false, GetVertices(rectangle));
            PolyFillType fillType = PolyFillType.pftNonZero;

            if (fillingRule == PathPaintingRenderInfo.EVEN_ODD_RULE) {
                fillType = PolyFillType.pftEvenOdd;
            }

            Clipper clipper = new Clipper();
            AddPath(clipper, path);
            AddRect(clipper, transfRectVertices);

            PolyTree resultTree = new PolyTree();
            clipper.Execute(ClipType.ctDifference, resultTree, fillType, PolyFillType.pftNonZero);

            return ConvertToPath(resultTree);
        }

        private static JoinType GetJoinType(int lineJoinStyle) {
            switch (lineJoinStyle) {
                case PdfContentByte.LINE_JOIN_BEVEL:
                    return JoinType.jtSquare;

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
                    // Offsetting is never used for path to be filled
                    if (subpath.Closed) {
                        endType = EndType.etClosedLine;
                    }

                    IList<Point2D> linearApproxPoints = subpath.GetPiecewiseLinearApproximation();
                    offset.AddPath(ConvertToIntPoints(linearApproxPoints), joinType, endType);
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
                convertedPoints.Add(new Point2D.Double(point.X / PdfCleanUpProcessor.FloatMultiplier,
                                                       point.Y / PdfCleanUpProcessor.FloatMultiplier));
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
