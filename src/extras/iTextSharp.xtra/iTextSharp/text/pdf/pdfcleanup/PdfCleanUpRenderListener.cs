using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.util;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;
using SharpImage = System.Drawing.Image;
using Encoder = System.Drawing.Imaging.Encoder;
using Path = iTextSharp.text.pdf.parser.Path;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {
    
    class PdfCleanUpRenderListener : IExtRenderListener {

        private static readonly Color CLEANED_AREA_FILL_COLOR = Color.White;

        private PdfStamper pdfStamper;
        private PdfCleanUpRegionFilter filter;
        private IList<PdfCleanUpContentChunk> chunks = new List<PdfCleanUpContentChunk>();
        private Stack<PdfCleanUpContext> contextStack = new Stack<PdfCleanUpContext>();
        private int strNumber = 1; // Represents ordinal number of string under processing. Needed for processing TJ operator.

        // Represents current path as if there were no segments to cut
        private Path unfilteredCurrentPath = new Path();

        // Represents actual current path to be stroked ("actual" means that it is filtered current path)
        private Path currentStrokePath = new Path();

        // Represents actual current path to be filled.
        private Path currentFillPath = new Path();

        // Represents the latest path used as a new clipping path. If the new path from the source document
        // is cleaned, then you should treat it as an empty set. Then the intersection (current clipping path)
        // between previous clipping path and the new one is also empty set, which means that there is no visible 
        // content at all. But we also can't write invisible content, which wasn't cleaned, to the resultant document, 
        // because in this case it will become visible. The latter case is incorrect from user's point of view.
        private Path newClippingPath;

        private bool clipPath;
        private int clippingRule;

        public PdfCleanUpRenderListener(PdfStamper pdfStamper, PdfCleanUpRegionFilter filter) {
            this.pdfStamper = pdfStamper;
            this.filter = filter;
            InitClippingPath();
        }

        public virtual void RenderText(TextRenderInfo renderInfo) {
            if (renderInfo.PdfString.ToUnicodeString().Length == 0) {
                return;
            }

            // if true, than clipping path was completely cleaned
            if (newClippingPath.IsEmpty()) {
                LineSegment baseline = renderInfo.GetUnscaledBaseline();
                chunks.Add(new PdfCleanUpContentChunk.Text(renderInfo.PdfString, baseline.GetStartPoint(), 
                    baseline.GetEndPoint(), false, strNumber));
            } else {
                foreach (TextRenderInfo ri in renderInfo.GetCharacterRenderInfos()) {
                    bool isAllowed = filter.AllowText(ri);
                    LineSegment baseline = ri.GetUnscaledBaseline();

                    chunks.Add(new PdfCleanUpContentChunk.Text(ri.PdfString, baseline.GetStartPoint(),
                        baseline.GetEndPoint(), isAllowed, strNumber));
                }
            }

            ++strNumber;
        }

        public virtual void RenderImage(ImageRenderInfo renderInfo) {
            IList<Rectangle> areasToBeCleaned = GetImageAreasToBeCleaned(renderInfo);

            if (areasToBeCleaned == null || newClippingPath.IsEmpty()) {
                chunks.Add(new PdfCleanUpContentChunk.Image(false, null));
            } else {
                PdfImageObject pdfImage = renderInfo.GetImage();
                byte[] imageBytes = ProcessImage(pdfImage.GetImageAsBytes(), areasToBeCleaned);

                if (renderInfo.GetRef() == null && pdfImage != null) { // true => inline image
                    PdfDictionary dict = pdfImage.GetDictionary();
                    PdfObject imageMask = dict.Get(PdfName.IMAGEMASK);
                    Image image = Image.GetInstance(imageBytes);

                    if (imageMask == null) {
                        imageMask = dict.Get(PdfName.IM);
                    }

                    if (imageMask != null && imageMask.Equals(PdfBoolean.PDFTRUE)) {
                        image.MakeMask();
                    }

                    PdfContentByte canvas = Context.Canvas;
                    canvas.AddImage(image, 1, 0, 0, 1, 0, 0, true);
                } else if (pdfImage != null && imageBytes != pdfImage.GetImageAsBytes()) {
                    chunks.Add(new PdfCleanUpContentChunk.Image(true, imageBytes));
                }
            }
        }

        public virtual void BeginTextBlock() {
        }

        public virtual void EndTextBlock() {
        }

        public virtual void ModifyPath(PathConstructionRenderInfo renderInfo) {
            // See the comment on the newClippingPath field.
            if (newClippingPath.IsEmpty()) {
                return;
            }

            IList<float> segmentData = renderInfo.SegmentData;

            switch (renderInfo.Operation) {
                case PathConstructionRenderInfo.MOVETO:
                    unfilteredCurrentPath.MoveTo(segmentData[0], segmentData[1]);
                    break;

                case PathConstructionRenderInfo.LINETO:
                    unfilteredCurrentPath.LineTo(segmentData[0], segmentData[1]);
                    break;

                case PathConstructionRenderInfo.CURVE_123:
                    unfilteredCurrentPath.CurveTo(segmentData[0], segmentData[1], segmentData[2],
                                                  segmentData[3], segmentData[4], segmentData[5]);
                    break;

                case PathConstructionRenderInfo.CURVE_23:
                    unfilteredCurrentPath.CurveTo(segmentData[0], segmentData[1], segmentData[2], segmentData[3]);
                    break;

                case PathConstructionRenderInfo.CURVE_13:
                    unfilteredCurrentPath.CurveFromTo(segmentData[0], segmentData[1], segmentData[2], segmentData[3]);
                    break;

                case PathConstructionRenderInfo.CLOSE:
                    unfilteredCurrentPath.CloseSubpath();
                    break;

                case PathConstructionRenderInfo.RECT:
                    unfilteredCurrentPath.Rectangle(segmentData[0], segmentData[1], segmentData[2], segmentData[3]);
                    break;
            }
        }

        public virtual Path RenderPath(PathPaintingRenderInfo renderInfo) {
            // If previous clipping is empty, then we shouldn't compute the new one
            // because their intersection is empty.
            if (newClippingPath.IsEmpty()) {
                currentStrokePath = new Path();
                currentFillPath = currentStrokePath;
                return newClippingPath;
            }

            bool stroke = (renderInfo.Operation & PathPaintingRenderInfo.STROKE) != 0;
            bool fill = (renderInfo.Operation & PathPaintingRenderInfo.FILL) != 0;

            float lineWidth = renderInfo.LineWidth;
            int lineCapStyle = renderInfo.LineCapStyle;
            int lineJoinStyle = renderInfo.LineJoinStyle;
            float miterLimit = renderInfo.MiterLimit;
            LineDashPattern lineDashPattern = renderInfo.LineDashPattern;

            if (stroke) {
                currentStrokePath = FilterCurrentPath(renderInfo.Ctm, true, -1, 
                    lineWidth, lineCapStyle, lineJoinStyle, miterLimit, lineDashPattern);
            }

            if (fill) {
                currentFillPath = FilterCurrentPath(renderInfo.Ctm, false, renderInfo.Rule, 
                    lineWidth, lineCapStyle, lineJoinStyle, miterLimit, lineDashPattern);
            } 
            
            if (clipPath) {
                if (fill && renderInfo.Rule == clippingRule) {
                    newClippingPath = currentFillPath;
                } else {
                    newClippingPath = FilterCurrentPath(renderInfo.Ctm, false, clippingRule, 
                        lineWidth, lineCapStyle, lineJoinStyle, miterLimit, lineDashPattern);
                }
            }

            unfilteredCurrentPath = new Path();
            return newClippingPath;
        }

        public virtual void ClipPath(int rule) {
            clipPath = true;
            clippingRule = rule;
        }

        public virtual bool Clipped {
            get { return clipPath; }
            set { clipPath = value; }
        }

        public virtual int ClippingRule {
            get { return clippingRule; }
        }

        public virtual Path CurrentStrokePath {
            get { return currentStrokePath; }
        }

        public virtual Path CurrentFillPath {
            get { return currentFillPath; }
        }

        public virtual Path NewClipPath {
            get { return newClippingPath; }
        }


        public virtual IList<PdfCleanUpContentChunk> Chunks {
            get { return chunks; }
        }

        public virtual PdfCleanUpContext Context {
            get { return contextStack.Peek(); }
        }

        public virtual void RegisterNewContext(PdfDictionary resources, PdfContentByte canvas) {
            canvas = canvas ?? new PdfContentByte(pdfStamper.Writer);
            contextStack.Push(new PdfCleanUpContext(resources, canvas));
        }

        public virtual void PopContext() {
            contextStack.Pop();
        }

        public virtual void ClearChunks() {
            chunks.Clear();
            strNumber = 1;
        }

        private void InitClippingPath() {
            /* For our purposes it is enough to initialize clipping path as arbitrary !non-empty! path.
               In other cases, initially, it shall include the entire page as it stated in PDF spec. */
            newClippingPath = new Path();
            newClippingPath.MoveTo(30, 30);
            newClippingPath.LineTo(30, 40);
        }

        /**
         * @return null if the image is not allowed (either it is fully covered or ctm == null).
         * List of covered image areas otherwise.
         */
        private IList<Rectangle> GetImageAreasToBeCleaned(ImageRenderInfo renderInfo) {
            return filter.GetCoveredAreas(renderInfo);
        }

        private byte[] ProcessImage(byte[] imageBytes, IList<Rectangle> areasToBeCleaned) {
            if (areasToBeCleaned.Count == 0) {
                return imageBytes;
            }

            using (Stream imageStream = new MemoryStream(imageBytes)) {
                SharpImage image = SharpImage.FromStream(imageStream);
                CleanImage(image, areasToBeCleaned);

                using (MemoryStream outStream = new MemoryStream()) {
                    if (Equals(image.RawFormat, ImageFormat.Tiff)) {
                        EncoderParameters encParams = new EncoderParameters(1);
                        encParams.Param[0] = new EncoderParameter(Encoder.Compression, (long) EncoderValue.CompressionLZW);
                        image.Save(outStream, GetEncoderInfo(image.RawFormat), encParams);
                    }
                    else if (Equals(image.RawFormat, ImageFormat.Jpeg)) {
                        EncoderParameters encParams = new EncoderParameters(1);
                        encParams.Param[0] = new EncoderParameter(Encoder.Quality, 100L);
                        image.Save(outStream, GetEncoderInfo(image.RawFormat), encParams);
                    }
                    else {
                        image.Save(outStream, image.RawFormat);
                    }

                    return outStream.ToArray();
                }
            }
        }

        private void CleanImage(SharpImage image, IList<Rectangle> areasToBeCleaned) {
            using (Graphics graphics = Graphics.FromImage(image)) {

                // A rectangle in the areasToBeCleaned list is treated to be in standard [0, 1]x[0,1] image space
                // (y varies from bottom to top and x from left to right), so we should scale the rectangle and also
                // invert and shear the y axe
                foreach (Rectangle rect in areasToBeCleaned) {
                    float scaledBottom = rect.Bottom*image.Height;
                    float scaledTop = rect.Top*image.Height;
                    float scaledLeft = rect.Left*image.Width;
                    float scaledRight = rect.Right*image.Width;

                    int bottomY = (int)Math.Ceiling(scaledBottom);
                    int topY = (int)Math.Floor(scaledTop);

                    int x = (int)Math.Ceiling(scaledLeft);
                    int y = topY * -1 + image.Height;
                    int width = (int)Math.Floor(scaledRight) - x;
                    int height = topY - bottomY;

                    graphics.FillRectangle(new SolidBrush(CLEANED_AREA_FILL_COLOR), x, y, width, height);
                }
            }
        }

        private static ImageCodecInfo GetEncoderInfo(ImageFormat format) {
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

            for (int j = 0; j < encoders.Length; ++j) {
                if (encoders[j].FormatID == format.Guid)
                    return encoders[j];
            }

            return null;
        }

        /**
         * @param fillingRule If the path is contour, pass any value.
         */

        private Path FilterCurrentPath(Matrix ctm, bool stroke, int fillingRule, float lineWidth, int lineCapStyle, 
                                       int lineJoinStyle, float miterLimit, LineDashPattern lineDashPattern) {
            Path path = new Path(unfilteredCurrentPath.Subpaths);

            if (stroke) {
                return filter.FilterStrokePath(path, ctm, lineWidth, lineCapStyle, lineJoinStyle,
                    miterLimit, lineDashPattern);
            } else {
                path.CloseAllSubpaths();
                return filter.FilterFillPath(path, ctm, fillingRule);
            }
        }
    }
}
