using System;
using System.Collections.Generic;
using System.Text;
using System.util;
using iTextSharp.awt.geom;
using iTextSharp.text;
using iTextSharp.text.pdf.parser;
using System.util;

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

        // Current implementation is for completely covered line arts only.
        protected internal virtual IList<Subpath> FilterSubpath(Subpath subpath, Matrix ctm, bool isContour) {
            IList<Subpath> filteredSubpaths = new List<Subpath>();
            RectangleJ rect = new RectangleJ(rectangle);

            if (subpath.IsSinglePointClosed()) {
                Point2D transformedStartPoint = TransformPoints(ctm, false, subpath.GetStartPoint())[0];

                if (!ContainsAll(rect, transformedStartPoint)) {
                    filteredSubpaths.Add(subpath);
                }

                return filteredSubpaths;
            } else if (subpath.IsSinglePointOpen()) {
                return filteredSubpaths;
            }

            Subpath newSubpath = new Subpath();
            IList<IShape> subpathSegments = new List<IShape>(subpath.GetSegments());

            // Create the line which is implicitly added by PDF, when you use 'h' operator
            if (subpath.Closed) {
                IShape lastSegment = subpathSegments[subpathSegments.Count - 1];
                IList<Point2D> segmentBasePoints = lastSegment.GetBasePoints();
                subpathSegments.Add(new Line(segmentBasePoints[segmentBasePoints.Count - 1], subpath.GetStartPoint()));
            }

            foreach (IShape segment in subpathSegments) {
                Point2D[] transformedSegBasePoints = TransformPoints(ctm, false, segment.GetBasePoints());

                if (ContainsAll(rect, transformedSegBasePoints)) {
                    if (!newSubpath.IsEmpty()) {
                        filteredSubpaths.Add(newSubpath);
                        newSubpath = new Subpath();
                    }
                } else {
                    newSubpath.AddSegment(segment);

                    // if it's filled area and we have got here, then it isn't completely covered
                    if (!isContour) {
                        break;
                    }
                }
            }

            if (!newSubpath.IsEmpty()) {
                filteredSubpaths.Add( !isContour ? subpath : newSubpath );
            }

            return filteredSubpaths;
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
