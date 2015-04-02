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
        private IList<PdfCleanUpRegionFilter> filters;
        private IList<PdfCleanUpContentChunk> chunks = new List<PdfCleanUpContentChunk>();
        private Stack<PdfCleanUpContext> contextStack = new Stack<PdfCleanUpContext>();
        private int strNumber = 1; // Represents ordinal number of string under processing. Needed for processing TJ operator.

        private Path unfilteredCurrentPath = new Path(); // Represents current path as if there were no segments to cut
        private Path currentStrokePath = new Path(); // Represents actual current path ("actual" means that it is filtered current path)

        /**
         * Represents actual current path to be filled. In general case in context of cleanup tool it completely differs from
         * the current path with implicitly closed subpaths.
         */
        private Path currentFillPath = new Path();

        private bool clipPath = false;

        public PdfCleanUpRenderListener(PdfStamper pdfStamper, IList<PdfCleanUpRegionFilter> filters) {
            this.pdfStamper = pdfStamper;
            this.filters = filters;
        }

        public virtual void RenderText(TextRenderInfo renderInfo) {
            if (renderInfo.PdfString.ToUnicodeString().Length == 0) {
                return;
            }

            foreach (TextRenderInfo ri in renderInfo.GetCharacterRenderInfos()) {
                bool textIsInsideRegion = TextIsInsideRegion(ri);
                LineSegment baseline = ri.GetUnscaledBaseline();

                chunks.Add(new PdfCleanUpContentChunk.Text(ri.PdfString, baseline.GetStartPoint(), baseline.GetEndPoint(), !textIsInsideRegion, strNumber));
            }

            ++strNumber;
        }

        public virtual void RenderImage(ImageRenderInfo renderInfo) {
            IList<Rectangle> areasToBeCleaned = GetImageAreasToBeCleaned(renderInfo);

            if (areasToBeCleaned == null) {
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
            if ((renderInfo.Operation & PathPaintingRenderInfo.STROKE) != 0) {
                currentStrokePath = FilterCurrentPath(renderInfo.Ctm, true);
            }

            if ((renderInfo.Operation & PathPaintingRenderInfo.FILL) != 0 || clipPath) {
                currentFillPath = FilterCurrentPath(renderInfo.Ctm, false);
            }

            unfilteredCurrentPath = new Path();
            return currentFillPath;
        }

        public virtual void ClipPath(int rule) {
            clipPath = true;
        }

        public virtual bool Clipped {
            get { return clipPath; }
            set { clipPath = value; }
        }

        public virtual Path CurrentStrokePath {
            get { return currentStrokePath; }
        }

        public virtual Path CurrentFillPath {
            get { return currentFillPath; }
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

        private bool TextIsInsideRegion(TextRenderInfo renderInfo) {
            foreach (PdfCleanUpRegionFilter filter in filters) {
                if (filter.AllowText(renderInfo)) {
                    return true;
                }
            }

            return false;
        }

        /**
         * @return null if the image is not allowed (either it is fully covered or ctm == null).
         * List of covered image areas otherwise.
         */
        private IList<Rectangle> GetImageAreasToBeCleaned(ImageRenderInfo renderInfo) {
            IList<Rectangle> areasToBeCleaned = new List<Rectangle>();

            foreach (PdfCleanUpRegionFilter filter in filters) {
                PdfCleanUpCoveredArea coveredArea = filter.Intersection(renderInfo);

                if (coveredArea == null || coveredArea.MatchesObjRect) {
                    return null;
                } else if (coveredArea.Rect != null) {
                    areasToBeCleaned.Add(coveredArea.Rect);
                }
            }

            return areasToBeCleaned;
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

        private Path FilterCurrentPath(Matrix ctm, bool isContour) {
            Queue<Subpath> filteredSubpaths = new Queue<Subpath>(unfilteredCurrentPath.Subpaths);

            foreach (PdfCleanUpRegionFilter filter in filters) {
                int queueSize = filteredSubpaths.Count;

                while (queueSize-- != 0) {
                    Subpath subpath = filteredSubpaths.Dequeue();

                    if (!isContour) {
                        subpath = new Subpath(subpath);
                        subpath.Closed = true;
                    }

                    IList<Subpath> filteredSubpath = filter.FilterSubpath(subpath, ctm, isContour);
                    Util.AddAll(filteredSubpaths, filteredSubpath);
                }
            }

            return new Path(new List<Subpath>(filteredSubpaths));
        }
    }
}
