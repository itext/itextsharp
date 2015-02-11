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
         *
         * @param renderInfo
         * @return
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
         * Calculates intersection of the image and the render filter region in the coordinate system relative to image.
         * The transformed coordinate system here is the coordinate system center of which is in (image.leftX, image.topY) and
         * y' = -y
         *
         * @return intersection
         */
        protected internal virtual PdfCleanUpCoveredArea Intersection(ImageRenderInfo renderInfo) {
            Rectangle imageRect = CalcImageRect(renderInfo);

            if (imageRect == null) {
                return null;
            }

            Rectangle intersectionRect = Intersection(imageRect, rectangle);
            Rectangle transformedIntersection = null;

            if (intersectionRect != null) {
                transformedIntersection = ShearCoordinatesAndInverseY(imageRect.Left, imageRect.Top, intersectionRect);
            }

            return new PdfCleanUpCoveredArea(transformedIntersection, imageRect.Equals(intersectionRect));
        }

        private bool Intersect(Rectangle r1, Rectangle r2) {
            return (r1.Left < r2.Right && r1.Right > r2.Left &&
                    r1.Bottom < r2.Top && r1.Top > r2.Bottom);
        }

        private Rectangle CalcImageRect(ImageRenderInfo renderInfo) {
            Matrix ctm = renderInfo.GetImageCTM();

            if (ctm == null) {
                return null;
            }

            AffineTransform t = new AffineTransform(ctm[Matrix.I11], ctm[Matrix.I12],
                                                    ctm[Matrix.I21], ctm[Matrix.I22],
                                                    ctm[Matrix.I31], ctm[Matrix.I32]);
            Point2D p1 = t.Transform(new Point(0, 0), null);
            Point2D p2 = t.Transform(new Point(0, 1), null);
            Point2D p3 = t.Transform(new Point(1, 0), null);
            Point2D p4 = t.Transform(new Point(1, 1), null);

            double[] xs = {p1.GetX(), p2.GetX(), p3.GetX(), p4.GetX()};
            double[] ys = {p1.GetY(), p2.GetY(), p3.GetY(), p4.GetY()};

            double left = Util.Min(xs);
            double bottom = Util.Min(ys);
            double right = Util.Max(xs);
            double top = Util.Max(ys);

            return new Rectangle((float) left, (float) bottom, (float) right, (float) top);
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

        private Rectangle ShearCoordinatesAndInverseY(float dx, float dy, Rectangle rect) {
            AffineTransform affineTransform = new AffineTransform(1, 0, 0, -1, -dx, dy);

            Point2D leftBottom = affineTransform.Transform(new Point(rect.Left, rect.Bottom), null);
            Point2D rightTop = affineTransform.Transform(new Point(rect.Right, rect.Top), null);

            return new Rectangle((float) leftBottom.GetX(), (float) leftBottom.GetY(),
                                 (float) rightTop.GetX(), (float) rightTop.GetY());
        }
    }
}
