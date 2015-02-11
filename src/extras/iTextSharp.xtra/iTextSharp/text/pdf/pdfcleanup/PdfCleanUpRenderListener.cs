using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;
using SharpImage = System.Drawing.Image;
using Encoder = System.Drawing.Imaging.Encoder;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {
    
    class PdfCleanUpRenderListener : IRenderListener {

        private static readonly Color CLEANED_AREA_FILL_COLOR = Color.White;

        private PdfStamper pdfStamper;
        private IList<PdfCleanUpRegionFilter> filters;
        private IList<PdfCleanUpContentChunk> chunks = new List<PdfCleanUpContentChunk>();
        private Stack<PdfCleanUpContext> contextStack = new Stack<PdfCleanUpContext>();
        private int strNumber = 1; // Represents number of string under processing. Needed for processing TJ operator.

        public PdfCleanUpRenderListener(PdfStamper pdfStamper, IList<PdfCleanUpRegionFilter> filters) {
            this.pdfStamper = pdfStamper;
            this.filters = filters;
        }

        public void RenderText(TextRenderInfo renderInfo) {
            if (renderInfo.PdfString.ToUnicodeString().Length == 0) {
                return;
            }

            foreach (TextRenderInfo ri in renderInfo.GetCharacterRenderInfos()) {
                bool textIsInsideRegion = TextIsInsideRegion(ri);
                LineSegment baseline = ri.GetUnscaledBaseline();

                chunks.Add(new PdfCleanUpContentChunk(ri.PdfString, baseline.GetStartPoint(), baseline.GetEndPoint(), !textIsInsideRegion, strNumber));
            }

            ++strNumber;
        }

        public void RenderImage(ImageRenderInfo renderInfo) {
            IList<Rectangle> areasToBeCleaned = GetImageAreasToBeCleaned(renderInfo);

            if (areasToBeCleaned == null) {
                chunks.Add(new PdfCleanUpContentChunk(false, null));
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
                    chunks.Add(new PdfCleanUpContentChunk(true, imageBytes));
                }
            }
        }

        public void BeginTextBlock() {
        }

        public void EndTextBlock() {
        }

        public IList<PdfCleanUpContentChunk> Chunks {
            get { return chunks; }
        }

        public PdfCleanUpContext Context {
            get { return contextStack.Peek(); }
        }

        public void RegisterNewContext(PdfDictionary resources, PdfContentByte canvas) {
            canvas = canvas ?? new PdfContentByte(pdfStamper.Writer);
            contextStack.Push(new PdfCleanUpContext(resources, canvas));
        }

        public void PopContext() {
            contextStack.Pop();
        }

        public void ClearChunks() {
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
                foreach (Rectangle rect in areasToBeCleaned) {
                    int x = (int) Math.Ceiling(rect.Left);
                    int y = (int) Math.Ceiling(rect.Top);
                    int width = (int) Math.Floor(rect.Right) - x;
                    int height = (int) Math.Floor(rect.Bottom) - y;

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
    }
}
