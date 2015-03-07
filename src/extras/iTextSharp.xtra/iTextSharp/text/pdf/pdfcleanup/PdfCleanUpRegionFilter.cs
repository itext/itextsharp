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
         * Calculates intersection of the image and the render filter region in the coordinate system relative to the image.
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
                transformedIntersection = TransformIntersection(renderInfo.GetImageCTM(), intersectionRect); 
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

            return GetRectangle(p1, p2, p3, p4);
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
            AffineTransform t = new AffineTransform(imageCTM[Matrix.I11], imageCTM[Matrix.I12],
                                                    imageCTM[Matrix.I21], imageCTM[Matrix.I22],
                                                    imageCTM[Matrix.I31], imageCTM[Matrix.I32]);
            Point2D p1;
            Point2D p2;
            Point2D p3;
            Point2D p4;

            try
            {
                p1 = t.InverseTransform(new Point(rect.Left, rect.Bottom), null);
                p2 = t.InverseTransform(new Point(rect.Left, rect.Top), null);
                p3 = t.InverseTransform(new Point(rect.Right, rect.Bottom), null);
                p4 = t.InverseTransform(new Point(rect.Right, rect.Top), null);
            }
            catch (InvalidOperationException e)
            {
                throw new SystemException(e.Message, e);
            }

            return GetRectangle(p1, p2, p3, p4);
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
    }
}
