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
using System.Collections.ObjectModel;
using System.Text;
using System.util.collections;
using iTextSharp.text;
using iTextSharp.text.io;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {

    /**
     * Represents the main mechanism for cleaning a PDF document.
     *
     * @since 5.5.5
     */
    public class PdfCleanUpProcessor {

        /**
         * When a document with line arts is being cleaned up, there are lot of
         * calculations with floating point numbers. All of them are translated
         * into fixed point numbers by multiplying by this coefficient. Vary it
         * to adjust the preciseness of the calculations.
         */
        public static double FloatMultiplier = Math.Pow(10, 14);

        public static bool FillCleanedArea = true;

        /**
         * Used as the criterion of a good approximation of rounded line joins
         * and line caps.
         */
        public static double ArcTolerance = 0.0025;

        private static readonly String XOBJ_NAME_PREFIX = "Fm";

        private static readonly String STROKE_COLOR = "StrokeColor";
        private static readonly String FILL_COLOR = "FillColor";

        private int currentXObjNum = 0;

        private PdfStamper pdfStamper;

        // key - page number, value - list of locations related to the page
        private IDictionary<int, IList<PdfCleanUpLocation>> pdfCleanUpLocations;

        // key - number of page containing redact annotations, value - look at variable name
        private IDictionary<int, HashSet2<string>> redactAnnotIndirRefs;

        // stores list of rectangles for annotation identified by it's index in Annots array
        private IDictionary<int, IList<Rectangle>> clippingRects; // stores list of rectangles for annotation identified by it's index in Annots array

        /**
         * Creates a {@link com.itextpdf.text.pdf.pdfcleanup.PdfCleanUpProcessor} object based on the
         * given {@link java.util.List} of {@link com.itextpdf.text.pdf.pdfcleanup.PdfCleanUpLocation}s
         * representing regions to be erased from the document.
         *
         * @param pdfCleanUpLocations list of locations to be cleaned up {@see PdfCleanUpLocation}
         * @param pdfStamper          A{@link com.itextpdf.text.pdf.PdfStamper} object representing the document which redaction
         *                            applies to.
         */
        public PdfCleanUpProcessor(IList<PdfCleanUpLocation> pdfCleanUpLocations, PdfStamper pdfStamper) {
            this.pdfCleanUpLocations = OrganizeLocationsByPage(pdfCleanUpLocations);
            this.pdfStamper = pdfStamper;
        }

        /**
         * Creates a {@link com.itextpdf.text.pdf.pdfcleanup.PdfCleanUpProcessor} object. Regions to be erased from
         * the document are extracted from the redact annotations contained inside the given document.
         *
         * @param pdfStamper A{@link com.itextpdf.text.pdf.PdfStamper} object representing the document which redaction
         *                   applies to.
         */
        public PdfCleanUpProcessor(PdfStamper pdfStamper) {
            this.redactAnnotIndirRefs = new Dictionary<int, HashSet2<string>>();
            this.clippingRects = new Dictionary<int, IList<Rectangle>>();
            this.pdfStamper = pdfStamper;
            ExtractLocationsFromRedactAnnots();
        }

        /**
         * Cleans the document by erasing all the areas which are either provided or
         * extracted from redaction annotations.
         *
         * @throws IOException
         * @throws DocumentException
         */
        public void CleanUp() {
            foreach (KeyValuePair<int, IList<PdfCleanUpLocation>> entry in pdfCleanUpLocations) {
                CleanUpPage(entry.Key, entry.Value);
            }

            pdfStamper.Reader.RemoveUnusedObjects();
        }

        private void CleanUpPage(int pageNum, IList<PdfCleanUpLocation> cleanUpLocations) {
            if (cleanUpLocations.Count == 0) {
                return;
            }

            PdfReader pdfReader = pdfStamper.Reader;
            PdfDictionary page = pdfReader.GetPageN(pageNum);
            PdfContentByte canvas = pdfStamper.GetUnderContent(pageNum);
            byte[] pageContentInput = ContentByteUtils.GetContentBytesForPage(pdfReader, pageNum);
            page.Remove(PdfName.CONTENTS);

            canvas.SaveState();

            PdfCleanUpRegionFilter filter = CreateFilter(cleanUpLocations);
            PdfCleanUpRenderListener pdfCleanUpRenderListener = new PdfCleanUpRenderListener(pdfStamper, filter);
            pdfCleanUpRenderListener.RegisterNewContext(pdfReader.GetPageResources(page), canvas);

            PdfContentStreamProcessor contentProcessor = new PdfContentStreamProcessor(pdfCleanUpRenderListener);
            PdfCleanUpContentOperator.PopulateOperators(contentProcessor, pdfCleanUpRenderListener);
            contentProcessor.ProcessContent(pageContentInput, page.GetAsDict(PdfName.RESOURCES));
            pdfCleanUpRenderListener.PopContext();

            canvas.RestoreState();

            ColorCleanedLocations(canvas, cleanUpLocations);

            if (redactAnnotIndirRefs != null) { // if it isn't null, then we are in "extract locations from redact annots" mode
                DeleteRedactAnnots(pageNum);
            }
        }

        private PdfCleanUpRegionFilter CreateFilter(IList<PdfCleanUpLocation> cleanUpLocations) {
            IList<Rectangle> regions = new List<Rectangle>(cleanUpLocations.Count);

            foreach (PdfCleanUpLocation location in cleanUpLocations) {
                regions.Add(location.Region);
            }

            return new PdfCleanUpRegionFilter(regions);
        }

        private void ColorCleanedLocations(PdfContentByte canvas, IList<PdfCleanUpLocation> cleanUpLocations) {
            if (FillCleanedArea) {
                foreach (PdfCleanUpLocation location in cleanUpLocations) {
                    if (location.CleanUpColor != null) {
                        AddColoredRectangle(canvas, location);
                    }
                }
            }
        }

        private void AddColoredRectangle(PdfContentByte canvas, PdfCleanUpLocation cleanUpLocation) {
            Rectangle cleanUpRegion = cleanUpLocation.Region;

            canvas.SaveState();
            canvas.SetColorFill(cleanUpLocation.CleanUpColor);
            canvas.MoveTo(cleanUpRegion.Left, cleanUpRegion.Bottom);
            canvas.LineTo(cleanUpRegion.Right, cleanUpRegion.Bottom);
            canvas.LineTo(cleanUpRegion.Right, cleanUpRegion.Top);
            canvas.LineTo(cleanUpRegion.Left, cleanUpRegion.Top);
            canvas.ClosePath();
            canvas.Fill();
            canvas.RestoreState();
        }

        private IDictionary<int, IList<PdfCleanUpLocation>> OrganizeLocationsByPage(ICollection<PdfCleanUpLocation> pdfCleanUpLocations) {
            IDictionary<int, IList<PdfCleanUpLocation>> organizedLocations = new Dictionary<int, IList<PdfCleanUpLocation>>();

            foreach (PdfCleanUpLocation location in pdfCleanUpLocations) {
                int page = location.Page;

                if (!organizedLocations.ContainsKey(page)) {
                    organizedLocations.Add(page, new List<PdfCleanUpLocation>());
                }

                organizedLocations[page].Add(location);
            }

            return organizedLocations;
        }

        /**
         * Extracts locations from the redact annotations contained in the document.
         */
        private void ExtractLocationsFromRedactAnnots() {
            this.pdfCleanUpLocations = new Dictionary<int, IList<PdfCleanUpLocation>>();
            PdfReader reader = pdfStamper.Reader;

            for (int i = 1; i <= reader.NumberOfPages; ++i) {
                PdfDictionary pageDict = reader.GetPageN(i);
                this.pdfCleanUpLocations.Add(i, ExtractLocationsFromRedactAnnots(i, pageDict));
            }
        }

        /**
         * Extracts locations from the redact annotations contained in the document and applied to the given page.
         */
        private IList<PdfCleanUpLocation> ExtractLocationsFromRedactAnnots(int page, PdfDictionary pageDict) {
            List<PdfCleanUpLocation> locations = new List<PdfCleanUpLocation>();

            if (pageDict.Contains(PdfName.ANNOTS)) {
                PdfArray annotsArray = pageDict.GetAsArray(PdfName.ANNOTS);

                for (int i = 0; i < annotsArray.Size; ++i) {
                    PdfIndirectReference annotIndirRef = annotsArray.GetAsIndirectObject(i);
                    PdfDictionary annotDict = annotsArray.GetAsDict(i);
                    PdfName annotSubtype = annotDict.GetAsName(PdfName.SUBTYPE);

                    if (annotSubtype.Equals(PdfName.REDACT)) {
                        SaveRedactAnnotIndirRef(page, annotIndirRef.ToString());
                        locations.AddRange(ExtractLocationsFromRedactAnnot(page, i, annotDict));
                    }
                }
            }

            return locations;
        }

        private void SaveRedactAnnotIndirRef(int page, String indRefStr) {
            if (!redactAnnotIndirRefs.ContainsKey(page)) {
                redactAnnotIndirRefs.Add(page, new HashSet2<String>());
            }

            redactAnnotIndirRefs[page].Add(indRefStr);
        }

        /**
         * Extracts locations from the concrete annotation.
         * Note: annotation can consist not only of one area specified by the RECT entry, but also of multiple areas specified
         * by the QuadPoints entry in the annotation dictionary.
         */
        private IList<PdfCleanUpLocation> ExtractLocationsFromRedactAnnot(int page, int annotIndex, PdfDictionary annotDict) {
            IList<PdfCleanUpLocation> locations = new List<PdfCleanUpLocation>();
            List<Rectangle> markedRectangles = new List<Rectangle>();
            PdfArray quadPoints = annotDict.GetAsArray(PdfName.QUADPOINTS);

            if (quadPoints.Size != 0) {
                markedRectangles.AddRange(TranslateQuadPointsToRectangles(quadPoints));
            } else {
                PdfArray annotRect = annotDict.GetAsArray(PdfName.RECT);
                markedRectangles.Add(new Rectangle(annotRect.GetAsNumber(0).FloatValue,
                                                   annotRect.GetAsNumber(1).FloatValue,
                                                   annotRect.GetAsNumber(2).FloatValue,
                                                   annotRect.GetAsNumber(3).FloatValue));
            }

            clippingRects.Add(annotIndex, markedRectangles);

            BaseColor cleanUpColor = null;
            PdfArray ic = annotDict.GetAsArray(PdfName.IC);

            if (ic != null) {
                cleanUpColor = new BaseColor(
                    ic.GetAsNumber(0).FloatValue,
                    ic.GetAsNumber(1).FloatValue,
                    ic.GetAsNumber(2).FloatValue
                );
            }


            PdfStream ro = annotDict.GetAsStream(PdfName.RO);

            if (ro != null) {
                cleanUpColor = null;
            }

            foreach (Rectangle rect in markedRectangles) {
                locations.Add(new PdfCleanUpLocation(page, rect, cleanUpColor));
            }

            return locations;
        }

        private IList<Rectangle> TranslateQuadPointsToRectangles(PdfArray quadPoints) {
            IList<Rectangle> rectangles = new List<Rectangle>();

            for (int i = 0; i < quadPoints.Size; i += 8) {
                rectangles.Add(new Rectangle(quadPoints.GetAsNumber(i + 4).FloatValue, // QuadPoints have "Z" order
                                             quadPoints.GetAsNumber(i + 5).FloatValue,
                                             quadPoints.GetAsNumber(i + 2).FloatValue,
                                             quadPoints.GetAsNumber(i + 3).FloatValue));
            }

            return rectangles;
        }

        /**
         * Deletes redact annotations from the page and substitutes them with either OverlayText or RO object if it's needed.
         */
        private void DeleteRedactAnnots(int pageNum) {
            HashSet2<String> indirRefs;
            redactAnnotIndirRefs.TryGetValue(pageNum, out indirRefs);

            if (indirRefs == null || indirRefs.Count == 0) {
                return;
            }

            PdfReader reader = pdfStamper.Reader;
            PdfContentByte canvas = pdfStamper.GetOverContent(pageNum);
            PdfDictionary pageDict = reader.GetPageN(pageNum);
            PdfArray annotsArray = pageDict.GetAsArray(PdfName.ANNOTS);

            // j is for access annotRect (i can be decreased, so we need to store additional index,
            // indicating current position in ANNOTS array in case if we don't remove anything
            for (int i = 0, j = 0; i < annotsArray.Size; ++i, ++j) {
                PdfIndirectReference annotIndRef = annotsArray.GetAsIndirectObject(i);
                PdfDictionary annotDict = annotsArray.GetAsDict(i);

                if (indirRefs.Contains(annotIndRef.ToString()) || indirRefs.Contains(GetParentIndRefStr(annotDict))) {
                    PdfStream formXObj = annotDict.GetAsStream(PdfName.RO);
                    PdfString overlayText = annotDict.GetAsString(PdfName.OVERLAYTEXT);

                    if (FillCleanedArea && formXObj != null) {
                        PdfArray rectArray = annotDict.GetAsArray(PdfName.RECT);
                        Rectangle annotRect = new Rectangle(rectArray.GetAsNumber(0).FloatValue,
                                                            rectArray.GetAsNumber(1).FloatValue,
                                                            rectArray.GetAsNumber(2).FloatValue,
                                                            rectArray.GetAsNumber(3).FloatValue);

                        InsertFormXObj(canvas, pageDict, formXObj, clippingRects[j], annotRect);
                    } else if (FillCleanedArea && overlayText != null && overlayText.ToUnicodeString().Length > 0) {
                        DrawOverlayText(canvas, clippingRects[j], overlayText,
                                        annotDict.GetAsString(PdfName.DA),
                                        annotDict.GetAsNumber(PdfName.Q),
                                        annotDict.GetAsBoolean(PdfName.REPEAT));
                    }

                    annotsArray.Remove(i--); // array size is changed, so we need to decrease i
                }
            }

            if (annotsArray.Size == 0) {
                pageDict.Remove(PdfName.ANNOTS);
            }
        }

        private void InsertFormXObj(PdfContentByte canvas, PdfDictionary pageDict, PdfStream formXObj, IList<Rectangle> clippingRects, Rectangle annotRect) {
            PdfName xobjName = GenerateNameForXObj(pageDict);
            canvas.SaveState();

            foreach (Rectangle rect in clippingRects) {
                canvas.Rectangle(rect.Left, rect.Bottom, rect.Width, rect.Height);
            }

            canvas.Clip();
            canvas.NewPath();

            canvas.AddFormXObj(formXObj, xobjName, 1, 0, 0, 1, annotRect.Left, annotRect.Bottom);

            canvas.RestoreState();
        }

        private void DrawOverlayText(PdfContentByte canvas, IList<Rectangle> textRectangles, PdfString overlayText, 
                                     PdfString otDA, PdfNumber otQ, PdfBoolean otRepeat) {
            ColumnText ct = new ColumnText(canvas);
            ct.SetLeading(0, 1.2F);
            ct.UseAscender = true;

            String otStr = overlayText.ToUnicodeString();

            canvas.SaveState();
            IDictionary<string, IList<object>> parsedDA = ParseDAParam(otDA);

            Font font = null;

            if (parsedDA.ContainsKey(STROKE_COLOR)) {
                IList<object> strokeColorArgs = parsedDA[STROKE_COLOR];
                SetStrokeColor(canvas, strokeColorArgs);
            }

            if (parsedDA.ContainsKey(FILL_COLOR)) {
                IList<object> fillColorArgs = parsedDA[FILL_COLOR];
                SetFillColor(canvas, fillColorArgs);
            }

            if (parsedDA.ContainsKey("Tf")) {
                IList<object> tfArgs = parsedDA["Tf"];
                font = RetrieveFontFromAcroForm((PdfName) tfArgs[0], (PdfNumber) tfArgs[1]);
            }

            foreach (Rectangle textRect in textRectangles) {
                ct.SetSimpleColumn(textRect);

                if (otQ != null) {
                    ct.Alignment = otQ.IntValue;
                }

                Phrase otPhrase;

                if (font != null) {
                    otPhrase = new Phrase(otStr, font);
                } else {
                    otPhrase = new Phrase(otStr);
                }

                float y = ct.YLine;

                if (otRepeat != null && otRepeat.BooleanValue) {
                    int status = ct.Go(true);

                    while (!ColumnText.HasMoreText(status)) {
                        otPhrase.Add(otStr);
                        ct.SetText(otPhrase);
                        ct.YLine = y;
                        status = ct.Go(true);
                    }
                }

                ct.SetText(otPhrase);
                ct.YLine = y;
                ct.Go();
            }

            canvas.RestoreState();
        }

        private Font RetrieveFontFromAcroForm(PdfName fontName, PdfNumber size) {
            PdfIndirectReference fontIndirReference = pdfStamper.Reader.AcroForm.GetAsDict(PdfName.DR).GetAsDict(PdfName.FONT).GetAsIndirectObject(fontName);
            BaseFont bfont = BaseFont.CreateFont((PRIndirectReference) fontIndirReference);

            return new Font(bfont, size.FloatValue);
        }

        IDictionary<string, IList<object>> ParseDAParam(PdfString DA) {
            IDictionary<string, IList<object>> commandArguments = new Dictionary<string, IList<object>>();

            PRTokeniser tokeniser = new PRTokeniser(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(DA.GetBytes())));
            IList<object> currentArguments = new List<object>();

            while (tokeniser.NextToken()) {
                if (tokeniser.TokenType == PRTokeniser.TokType.OTHER) {
                    String key = tokeniser.StringValue;

                    if (key == "RG" || key == "G" || key == "K") {
                        key = STROKE_COLOR;
                    } else if (key == "rg" || key == "g" || key == "k") {
                        key = FILL_COLOR;
                    }

                    if (commandArguments.ContainsKey(key)) {
                        commandArguments[key] = currentArguments;
                    } else {
                        commandArguments.Add(key, currentArguments);
                    }

                    currentArguments = new List<object>();
                } else {
                    switch (tokeniser.TokenType) {
                        case PRTokeniser.TokType.NUMBER:
                            currentArguments.Add(new PdfNumber(tokeniser.StringValue));
                            break;

                        case PRTokeniser.TokType.NAME:
                            currentArguments.Add(new PdfName(tokeniser.StringValue));
                            break;

                        default:
                            currentArguments.Add(tokeniser.StringValue);
                            break;
                    }
                }
            }

            return commandArguments;
        }

        private String GetParentIndRefStr(PdfDictionary dict) {
            return dict.GetAsIndirectObject(PdfName.PARENT).ToString();
        }

        private PdfName GenerateNameForXObj(PdfDictionary pageDict) {
            PdfDictionary resourcesDict = pageDict.GetAsDict(PdfName.RESOURCES);
            PdfDictionary xobjDict = resourcesDict.GetAsDict(PdfName.XOBJECT);

            if (xobjDict != null) {
                foreach (PdfName xobjName in xobjDict.Keys) {
                    int xobjNum = GetXObjNum(xobjName);

                    if (currentXObjNum <= xobjNum) {
                        currentXObjNum = xobjNum + 1;
                    }
                }
            }

            return new PdfName(XOBJ_NAME_PREFIX + currentXObjNum++);
        }

        private int GetXObjNum(PdfName xobjName) {
            String decodedPdfName = PdfName.DecodeName(xobjName.ToString());

            if (decodedPdfName.LastIndexOf(XOBJ_NAME_PREFIX) == -1) {
                return 0;
            }

            String numStr = decodedPdfName.Substring( decodedPdfName.LastIndexOf(XOBJ_NAME_PREFIX) + XOBJ_NAME_PREFIX.Length );
            return Int32.Parse(numStr);
        }

        private void SetFillColor(PdfContentByte canvas, IList<object> fillColorArgs) {
            switch (fillColorArgs.Count) {
                case 1:
                    canvas.SetGrayFill(((PdfNumber) fillColorArgs[0]).FloatValue);
                    break;

                case 3:
                    canvas.SetRGBColorFillF(((PdfNumber) fillColorArgs[0]).FloatValue,
                                            ((PdfNumber) fillColorArgs[1]).FloatValue,
                                            ((PdfNumber) fillColorArgs[2]).FloatValue);
                    break;

                case 4:
                    canvas.SetCMYKColorFillF(((PdfNumber) fillColorArgs[0]).FloatValue,
                                             ((PdfNumber) fillColorArgs[1]).FloatValue,
                                             ((PdfNumber) fillColorArgs[2]).FloatValue,
                                             ((PdfNumber) fillColorArgs[3]).FloatValue);
                    break;

            }
        }

        private void SetStrokeColor(PdfContentByte canvas, IList<object> strokeColorArgs) {
            switch (strokeColorArgs.Count) {
                case 1:
                    canvas.SetGrayStroke(((PdfNumber) strokeColorArgs[0]).FloatValue);
                    break;

                case 3:
                    canvas.SetRGBColorStrokeF(((PdfNumber) strokeColorArgs[0]).FloatValue,
                                              ((PdfNumber) strokeColorArgs[1]).FloatValue,
                                              ((PdfNumber) strokeColorArgs[2]).FloatValue);
                    break;

                case 4:
                    canvas.SetCMYKColorFillF(((PdfNumber) strokeColorArgs[0]).FloatValue,
                                             ((PdfNumber) strokeColorArgs[1]).FloatValue,
                                             ((PdfNumber) strokeColorArgs[2]).FloatValue,
                                             ((PdfNumber) strokeColorArgs[3]).FloatValue);
                    break;

            }
        }
    }
}
