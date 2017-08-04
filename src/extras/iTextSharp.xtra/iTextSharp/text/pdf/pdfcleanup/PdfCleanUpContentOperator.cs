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
using System.Text;
using iTextSharp.awt.geom;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup;
using System.Collections.Generic;
using System.IO;
using System.util;
using System.util.collections;
using Path = iTextSharp.text.pdf.parser.Path;

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {

    class PdfCleanUpContentOperator : IContentOperator {

        private static readonly byte[] TStar = DocWriter.GetISOBytes("T*\n");
        private static readonly byte[] Tw = DocWriter.GetISOBytes(" Tw ");
        private static readonly byte[] TcTStar = DocWriter.GetISOBytes(" Tc T*\n");
        private static readonly byte[] TJ = DocWriter.GetISOBytes("] TJ\n");
        private static readonly byte[] Tc = DocWriter.GetISOBytes(" Tc\n");
        private static readonly byte[] m = DocWriter.GetISOBytes(" m\n");
        private static readonly byte[] l = DocWriter.GetISOBytes(" l\n");
        private static readonly byte[] c = DocWriter.GetISOBytes(" c\n");
        private static readonly byte[] h = DocWriter.GetISOBytes("h\n");
        private static readonly byte[] S = DocWriter.GetISOBytes("S\n");
        private static readonly byte[] f = DocWriter.GetISOBytes("f\n");
        private static readonly byte[] eoF = DocWriter.GetISOBytes("f*\n");
        private static readonly byte[] n = DocWriter.GetISOBytes("n\n");
        private static readonly byte[] W = DocWriter.GetISOBytes("W\n");
        private static readonly byte[] eoW = DocWriter.GetISOBytes("W*\n");
        private static readonly byte[] q = DocWriter.GetISOBytes("q\n");
        private static readonly byte[] Q = DocWriter.GetISOBytes("Q\n");
        private static readonly byte[] cs = DocWriter.GetISOBytes("cs\n");

        private static readonly HashSet2<String> textShowingOperators = new HashSet2<String>(new string[] {"TJ", "Tj", "'", "\""});
        private static readonly HashSet2<String> pathConstructionOperators = new HashSet2<String>(new string[] {"m", "l", "c", "v", "y", "h", "re"});

        private static readonly HashSet2<String> strokeOperators = new HashSet2<String>(new string[] {"S", "s", "B", "B*", "b", "b*"});
        private static readonly HashSet2<String> nwFillOperators = new HashSet2<string>(new string[] {"f", "F", "B", "b"});
        private static readonly HashSet2<String> eoFillOperators = new HashSet2<string>(new string[] {"f*", "B*", "b*"}); 
        private static readonly HashSet2<String> pathPaintingOperators; // initialized in the static constructor

        private static readonly HashSet2<String> clippingPathOperators = new HashSet2<String>(new string[] {"W", "W*"});

        private static readonly HashSet2<String> lineStyleOperators = new HashSet2<String>(new string[] {"w", "J", "j", "M", "d"}); 

        private static readonly HashSet2<String> strokeColorOperators = new HashSet2<string>(new string[] {"CS", "SC", "SCN", "G", "RG", "K"}); 

        protected PdfCleanUpRenderListener cleanUpStrategy;
        protected IContentOperator originalContentOperator;

        public PdfCleanUpContentOperator(PdfCleanUpRenderListener cleanUpStrategy) {
            this.cleanUpStrategy = cleanUpStrategy;
        }

        static PdfCleanUpContentOperator() {
            pathPaintingOperators = new HashSet2<string>(strokeOperators);
            pathPaintingOperators.AddAll(nwFillOperators);
            pathPaintingOperators.AddAll(eoFillOperators);
            pathPaintingOperators.Add("n");
        }

        public static void PopulateOperators(PdfContentStreamProcessor contentProcessor,
                                     PdfCleanUpRenderListener pdfCleanUpRenderListener) {
            foreach (String oper in contentProcessor.RegisteredOperatorStrings) {
                PdfCleanUpContentOperator contentOperator = new PdfCleanUpContentOperator(pdfCleanUpRenderListener);
                contentOperator.originalContentOperator = contentProcessor.RegisterContentOperator(oper, contentOperator);
            }
        }

        public virtual void Invoke(PdfContentStreamProcessor pdfContentStreamProcessor, PdfLiteral oper, List<PdfObject> operands) {
            String operatorStr = oper.ToString();
            PdfContentByte canvas = cleanUpStrategy.Context.Canvas;
            PRStream xFormStream = null;
            bool disableOutput = pathConstructionOperators.Contains(operatorStr) || pathPaintingOperators.Contains(operatorStr) || clippingPathOperators.Contains(operatorStr);
            GraphicsState gs = pdfContentStreamProcessor.Gs();

            // key - number of a string in the TJ operator, value - number following the string; the first number without string (if it's presented) is stored under 0.
            // BE AWARE: zero-length strings are ignored!!!
            IDictionary<int, float> structuredTJoperands = null;

            if ("Do" == operatorStr) {
                if (operands.Count == 2 && operands[0].IsName()) {
                    PdfDictionary xObjResources = cleanUpStrategy.Context.Resources.GetAsDict(PdfName.XOBJECT);

                    if (xObjResources != null) {
                        PdfStream xObj = xObjResources.GetAsStream((PdfName) operands[0]);

                        if (xObj is PRStream && xObj.GetAsName(PdfName.SUBTYPE) != null &&
                                xObj.GetAsName(PdfName.SUBTYPE).CompareTo(PdfName.FORM) == 0) {
                            xFormStream = (PRStream) xObj;
                            cleanUpStrategy.RegisterNewContext(xObj.GetAsDict(PdfName.RESOURCES), null);
                        }
                    }
                }
            }

            originalContentOperator.Invoke(pdfContentStreamProcessor, oper, operands);
            IList<PdfCleanUpContentChunk> chunks = cleanUpStrategy.Chunks;

            if (xFormStream != null) {
                xFormStream.SetData(cleanUpStrategy.Context.Canvas.ToPdf(cleanUpStrategy.Context.Canvas.PdfWriter));
                cleanUpStrategy.PopContext();
                canvas = cleanUpStrategy.Context.Canvas;
            }

            if ("Do" == operatorStr) {
                if (chunks.Count > 0 && chunks[0] is PdfCleanUpContentChunk.Image) {
                    PdfCleanUpContentChunk.Image chunk = (PdfCleanUpContentChunk.Image) chunks[0];

                    if (chunk.Visible) {
                        PdfDictionary xObjResources = cleanUpStrategy.Context.Resources.GetAsDict(PdfName.XOBJECT);
                        PRStream imageStream = (PRStream) xObjResources.GetAsStream((PdfName) operands[0]);
                        UpdateImageStream(imageStream, chunk.NewImageData);
                    } else {
                        disableOutput = true;
                    }
                }
            } else if (textShowingOperators.Contains(operatorStr) && !AllChunksAreVisible(cleanUpStrategy.Chunks)) {
                disableOutput = true;

                if ("'" == operatorStr) {
                    canvas.InternalBuffer.Append(TStar);
                } else if ("\"" == operatorStr) {
                    operands[0].ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
                    canvas.InternalBuffer.Append(Tw);

                    operands[1].ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
                    canvas.InternalBuffer.Append(TcTStar);
                } else if ("TJ" == operatorStr) {
                    structuredTJoperands = StructureTJarray((PdfArray) operands[0]);
                }

                WriteTextChunks(structuredTJoperands, chunks, canvas, gs.CharacterSpacing, gs.WordSpacing, 
                    gs.FontSize, gs.HorizontalScaling);
            } else if (pathPaintingOperators.Contains(operatorStr)) {
                WritePath(operatorStr, canvas, gs.ColorSpaceStroke);
            } else if (strokeColorOperators.Contains(operatorStr)) {
                // Replace current color with the new one.
                cleanUpStrategy.Context.PopStrokeColor();
                cleanUpStrategy.Context.PushStrokeColor(operands);
            } else if ("q" == operatorStr) {
                cleanUpStrategy.Context.PushStrokeColor(cleanUpStrategy.Context.PeekStrokeColor());
            } else if ("Q" == operatorStr) {
                cleanUpStrategy.Context.PopStrokeColor();
            }

            if (!disableOutput) {
                WriteOperands(canvas, operands);
            }

            cleanUpStrategy.ClearChunks();
        }

        private void WriteOperands(PdfContentByte canvas, IList<PdfObject> operands) {
            int index = 0;

            foreach (PdfObject o in operands) {
                ToPdf(o, canvas.PdfWriter, canvas.InternalBuffer);
                canvas.InternalBuffer.Append(operands.Count > ++index ? (byte) ' ' : (byte) '\n');
            }
        }

        private bool AllChunksAreVisible(IList<PdfCleanUpContentChunk> chunks) {
            foreach (PdfCleanUpContentChunk chunk in chunks) {
                if (!chunk.Visible) {
                    return false;
                }
            }

            return true;
        }

        /** 
         * Overriding standard PdfObject.ToPdf because we need sorted PdfDictionaries.
         */
        private static void ToPdf(PdfObject @object, PdfWriter writer, ByteBuffer os) {
            if (@object is PdfDictionary) {
                os.Append('<');
                os.Append('<');

                List<PdfName> keys = new List<PdfName>(((PdfDictionary) @object).Keys);
                keys.Sort();

                foreach (PdfName key in keys) {
                    ToPdf(key, writer, os);
                    PdfObject value = ((PdfDictionary) @object).Get(key);
                    int type = value.Type;

                    if (type != PdfObject.ARRAY && type != PdfObject.DICTIONARY && type != PdfObject.NAME &&
                            type != PdfObject.STRING) {
                        os.Append(' ');
                    }

                    ToPdf(value, writer, os);
                }

                os.Append('>');
                os.Append('>');
            } else {
                @object.ToPdf(writer, os);
            }
        }

        /**
         * Example.
         *      TJ = [(h) 3 4 (q) 7 (w) (e)]
         *      Result = {0:0, 1:7, 2:7, 3:0, 4:0}
         *
         * @return Map whose key is an ordinal number of the string in the TJ array and value
         *         is the position adjustment.
         */
        private IDictionary<int, float> StructureTJarray(PdfArray array) {
            IDictionary<int, float> structuredTJoperands = new Dictionary<int, float>();

            if (array.Size == 0) {
                return structuredTJoperands;
            }

            int previousStrNum = 0;
            structuredTJoperands.Add(previousStrNum, 0f);

            for (int i = 0; i < array.Size; ++i) {
                PdfObject currentObj = array.GetPdfObject(i);

                if (currentObj is PdfString && ((PdfString) currentObj).ToUnicodeString().Length > 0) {
                    ++previousStrNum;
                    structuredTJoperands.Add(previousStrNum, 0f);
                } else {
                    float oldOffset = structuredTJoperands[previousStrNum];
                    structuredTJoperands[previousStrNum] = oldOffset + ((PdfNumber) currentObj).FloatValue;
                }
            }

            return structuredTJoperands;
        }

        /**
         * Writes parts of text which are visible into a content stream.
         */
        private void WriteTextChunks(IDictionary<int, float> structuredTJoperands, IList<PdfCleanUpContentChunk> chunks, PdfContentByte canvas, 
                                     float characterSpacing, float wordSpacing, float fontSize, float horizontalScaling) {
            canvas.SetCharacterSpacing(0);
            canvas.SetWordSpacing(0);
            canvas.InternalBuffer.Append((byte) '[');

            float convertedCharacterSpacing = -characterSpacing * 1000f / fontSize;
            float convertedWordSpacing = -wordSpacing * 1000f / fontSize;

            float shift = structuredTJoperands != null ? structuredTJoperands[0] : 0;
            PdfCleanUpContentChunk.Text prevChunk = null;

            foreach (PdfCleanUpContentChunk chunk in chunks) {
                PdfCleanUpContentChunk.Text textChunk = (PdfCleanUpContentChunk.Text) chunk;

                if (prevChunk != null && prevChunk.NumOfStrTextBelongsTo != textChunk.NumOfStrTextBelongsTo &&
                        structuredTJoperands != null) {
                    shift += structuredTJoperands[prevChunk.NumOfStrTextBelongsTo];
                }

                if (textChunk.Visible) {
                    if (Util.Compare(shift, 0.0f) != 0 && Util.Compare(shift, -0.0f) != 0) {
                        canvas.InternalBuffer.Append(shift).Append(' ');
                    }

                    textChunk.GetText().ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
                    canvas.InternalBuffer.Append(' ');

                    shift = convertedCharacterSpacing + (IsSpace(textChunk) ? convertedWordSpacing : 0);
                } else {
                    shift += GetUnscaledTextChunkWidth(textChunk, characterSpacing, wordSpacing, 
                        fontSize, horizontalScaling);
                }

                prevChunk = textChunk;
            }

            if (Util.Compare(shift, 0) != 0 && Util.Compare(shift, -0.0f) != 0) {
                canvas.InternalBuffer.Append(shift);
            }

            canvas.InternalBuffer.Append(TJ);

            if (Util.Compare(characterSpacing, 0) != 0 && Util.Compare(characterSpacing, -0.0f) != 0) {
                new PdfNumber(characterSpacing).ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
                canvas.InternalBuffer.Append(Tc);
            }

            if (Util.Compare(wordSpacing, 0) != 0 && Util.Compare(wordSpacing, -0.0f) != 0) {
                new PdfNumber(wordSpacing).ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
                canvas.InternalBuffer.Append(Tw);
            }
        }

        /**
         * We get into this method when the current chunk is not visible.
         * Here we are calculating a piece of the Tj coefficient for a previous visible chunk.
         * For details see PDF spec., Text Space Details, formula for "tx" coefficient
         * and TextRenderInfo class (getUnscaledBaseline)
         */
        private float GetUnscaledTextChunkWidth(PdfCleanUpContentChunk.Text chunk, float characterSpacing, 
                                                float wordSpacing, float fontSize, float horizontalScaling) {
            // We should multiply by 100 because iText stores horizontal scaling as the value in [0, 1] interval;
            // also we need to add character and word spaces because TextRenderInfo class truncates them from the end of the string
            // (single character string in our case is also truncated)
            float scaledChunkWidth = (chunk.EndX - chunk.StartX) * 100f +
                    (characterSpacing + (IsSpace(chunk) ? wordSpacing : 0)) * horizontalScaling * 100f;

            return -scaledChunkWidth * 1000f / (horizontalScaling * 100f * fontSize);
        }

        private bool IsSpace(PdfCleanUpContentChunk.Text chunk) {
            return chunk.GetText().ToUnicodeString() == " ";
        }

        private void UpdateImageStream(PRStream imageStream, byte[] newData) {
            PdfImage image = new PdfImage(Image.GetInstance(newData), "", null);

            if (imageStream.Contains(PdfName.SMASK)) {
                image.Put(PdfName.SMASK, imageStream.Get(PdfName.SMASK));
            }

            if (imageStream.Contains(PdfName.MASK)) {
                image.Put(PdfName.MASK, imageStream.Get(PdfName.MASK));
            }

            if (imageStream.Contains(PdfName.SMASKINDATA)) {
                image.Put(PdfName.SMASKINDATA, imageStream.Get(PdfName.SMASKINDATA));
            }

            imageStream.Clear();
            imageStream.PutAll(image);
            imageStream.SetDataRaw(image.GetBytes());
        }

        private void WritePath(String operatorStr, PdfContentByte canvas, PdfName strokeColorSpace) {
            if (nwFillOperators.Contains(operatorStr)) {
                WritePath(cleanUpStrategy.CurrentFillPath, f, canvas);
            } else if (eoFillOperators.Contains(operatorStr)) {
                WritePath(cleanUpStrategy.CurrentFillPath, eoF, canvas);
            }

            if (strokeOperators.Contains(operatorStr)) {
                WriteStroke(canvas, cleanUpStrategy.CurrentStrokePath, strokeColorSpace);
            }

            if (cleanUpStrategy.Clipped) {
                if (!cleanUpStrategy.NewClipPath.IsEmpty()) {
                    byte[] clippingOperator = (cleanUpStrategy.ClippingRule == PathPaintingRenderInfo.NONZERO_WINDING_RULE) ? W : eoW;
                    WritePath(cleanUpStrategy.NewClipPath, clippingOperator, canvas);
                } else {
                    // If the clipping path from the source document is cleaned (it happens when reduction
                    // area covers the path completely), then you should treat it as an empty set (no points
                    // are included in the path). Then the current clipping path (which is the intersection
                    // between previous clipping path and the new one) is also empty set, which means that
                    // there is no visible content at all. But at the same time as we removed the clipping
                    // path, the invisible content would become visible. So, to emulate the correct result,
                    // we would simply put a degenerate clipping path which consists of a single point at (0, 0).
                    Path degeneratePath = new Path();
                    degeneratePath.MoveTo(0, 0);
                    WritePath(degeneratePath, W, canvas);
                }
                canvas.InternalBuffer.Append(n);
                cleanUpStrategy.Clipped = false;
            }
        }

        private void WritePath(Path path, byte[] pathPaintingOperator, PdfContentByte canvas) {
            if (path.IsEmpty()) {
                return;
            }

            foreach (Subpath subpath in path.Subpaths) {
                WriteMoveTo(subpath.GetStartPoint(), canvas);

                foreach (IShape segment in subpath.GetSegments()) {
                    if (segment is BezierCurve) {
                        WriteBezierCurve((BezierCurve) segment, canvas);
                    } else {
                        WriteLine((Line) segment, canvas);
                    }
                }

                if (subpath.Closed) {
                    canvas.InternalBuffer.Append(h);
                }
            }

            if (pathPaintingOperator != null) {
                canvas.InternalBuffer.Append(pathPaintingOperator);
            }
        }

        private void WriteMoveTo(Point2D destinationPoint, PdfContentByte canvas) {
            new PdfNumber(destinationPoint.GetX()).ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
            canvas.InternalBuffer.Append(' ');
            new PdfNumber(destinationPoint.GetY()).ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
            canvas.InternalBuffer.Append(m);
        }

        private void WriteBezierCurve(BezierCurve curve, PdfContentByte canvas) {
            IList<Point2D> basePoints = curve.GetBasePoints();
            Point2D p2 = basePoints[1];
            Point2D p3 = basePoints[2];
            Point2D p4 = basePoints[3];

            new PdfNumber(p2.GetX()).ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
            canvas.InternalBuffer.Append(' ');

            new PdfNumber(p2.GetY()).ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
            canvas.InternalBuffer.Append(' ');

            new PdfNumber(p3.GetX()).ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
            canvas.InternalBuffer.Append(' ');

            new PdfNumber(p3.GetY()).ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
            canvas.InternalBuffer.Append(' ');

            new PdfNumber(p4.GetX()).ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
            canvas.InternalBuffer.Append(' ');

            new PdfNumber(p4.GetY()).ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
            canvas.InternalBuffer.Append(c);
        }

        private void WriteLine(Line line, PdfContentByte canvas) {
            Point2D destination = line.GetBasePoints()[1];

            new PdfNumber(destination.GetX()).ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
            canvas.InternalBuffer.Append(' ');

            new PdfNumber(destination.GetY()).ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
            canvas.InternalBuffer.Append(l);
        }

        private void WriteStroke(PdfContentByte canvas, Path path, PdfName strokeColorSpace) {
            canvas.InternalBuffer.Append(q);

            if (strokeColorSpace != null) {
                strokeColorSpace.ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
                canvas.InternalBuffer.Append(' ').Append(cs);
            }

            IList<PdfObject> strokeColorOperands = cleanUpStrategy.Context.PeekStrokeColor();
            String strokeOperatorStr = strokeColorOperands[strokeColorOperands.Count - 1].ToString();
            // Below expression converts stroke color operator to its fill analogue.
            strokeColorOperands[strokeColorOperands.Count - 1] = new PdfLiteral(strokeOperatorStr.ToLower());
            WriteOperands(canvas, strokeColorOperands);

            WritePath(cleanUpStrategy.CurrentStrokePath, f, canvas);

            canvas.InternalBuffer.Append(Q);
        }
    }
}
