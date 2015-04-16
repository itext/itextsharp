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
using LineDashPattern = iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup.PdfCleanUpGraphicsState.LineDashPattern;

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

        private static readonly HashSet2<String> textShowingOperators = new HashSet2<String>(new string[] {"TJ", "Tj", "'", "\""});
        private static readonly HashSet2<String> pathConstructionOperators = new HashSet2<String>(new string[] {"m", "l", "c", "v", "y", "h", "re"});

        private static readonly HashSet2<String> strokeOperators = new HashSet2<String>(new string[] {"S", "s", "B", "B*", "b", "b*"});
        private static readonly HashSet2<String> nwFillOperators = new HashSet2<string>(new string[] {"f", "F", "B", "b"});
        private static readonly HashSet2<String> eoFillOperators = new HashSet2<string>(new string[] {"f*", "B*", "b*"}); 
        private static readonly HashSet2<String> pathPaintingOperators; // initialized in the static constructor

        private static readonly HashSet2<String> clippingPathOperators = new HashSet2<String>(new string[] {"W", "W*"});

        private static readonly HashSet2<String> lineStyleOperators = new HashSet2<String>(new string[] {"w", "J", "j", "M", "d"}); 

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
            } else if ("q" == operatorStr) {
                cleanUpStrategy.Context.SaveGraphicsState();
            } else if ("Q" == operatorStr) {
                cleanUpStrategy.Context.RestoreGraphicsState();
            } else if ("Tf" == operatorStr) {
                cleanUpStrategy.Context.FontSize = ((PdfNumber) operands[1]).FloatValue;
            } else if ("Tc" == operatorStr) {
                cleanUpStrategy.Context.CharacterSpacing= ((PdfNumber) operands[0]).FloatValue;
            } else if ("Tw" == operatorStr) {
                cleanUpStrategy.Context.WordSpacing = ((PdfNumber) operands[0]).FloatValue;
            } else if ("Tz" == operatorStr) {
                cleanUpStrategy.Context.HorizontalScaling = ((PdfNumber) operands[0]).FloatValue;
            } else if (lineStyleOperators.Contains(operatorStr)) {
                if ("w" == operatorStr) {
                    cleanUpStrategy.Context.LineWidth = ((PdfNumber) operands[0]).FloatValue;
                } else if ("J" == operatorStr) {
                    cleanUpStrategy.Context.LineCapStyle = ((PdfNumber) operands[0]).IntValue;
                } else if ("j" == operatorStr) {
                    cleanUpStrategy.Context.LineJoinStyle = ((PdfNumber) operands[0]).IntValue;
                } else if ("M" == operatorStr) {
                    cleanUpStrategy.Context.MiterLimit = ((PdfNumber) operands[0]).FloatValue;
                } else if ("d" == operatorStr) {
                    cleanUpStrategy.Context.SetLineDashPattern(new LineDashPattern(((PdfArray) operands[0]), 
                                                                                   ((PdfNumber) operands[1]).FloatValue));
                }

                disableOutput = true;
            } else if (textShowingOperators.Contains(operatorStr) && !AllChunksAreVisible(cleanUpStrategy.Chunks)) {
                disableOutput = true;

                if ("'" == operatorStr) {
                    canvas.InternalBuffer.Append(TStar);
                } else if ("\"" == operatorStr) {
                    operands[0].ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
                    canvas.InternalBuffer.Append(Tw);

                    operands[1].ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
                    canvas.InternalBuffer.Append(TcTStar);

                    cleanUpStrategy.Context.WordSpacing = ((PdfNumber) operands[0]).FloatValue;
                    cleanUpStrategy.Context.CharacterSpacing = ((PdfNumber) operands[1]).FloatValue;
                } else if ("TJ" == operatorStr) {
                    structuredTJoperands = StructureTJarray((PdfArray) operands[0]);
                }

                WriteTextChunks(structuredTJoperands, chunks, canvas);
            } else if ("\"" == operatorStr) {
                cleanUpStrategy.Context.WordSpacing = ((PdfNumber) operands[0]).FloatValue;
                cleanUpStrategy.Context.CharacterSpacing = ((PdfNumber) operands[1]).FloatValue;
            } else if (pathPaintingOperators.Contains(operatorStr)) {
                WritePath(operatorStr, canvas);
            }

            if (!disableOutput) {
                int index = 0;

                foreach (PdfObject o in operands) {
                    ToPdf(o, canvas.PdfWriter, canvas.InternalBuffer);
                    canvas.InternalBuffer.Append(operands.Count > ++index ? (byte) ' ' : (byte) '\n');
                }
            }

            cleanUpStrategy.ClearChunks();
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
        private void WriteTextChunks(IDictionary<int, float> structuredTJoperands, IList<PdfCleanUpContentChunk> chunks, PdfContentByte canvas) {
            canvas.SetCharacterSpacing(0);
            canvas.SetWordSpacing(0);
            canvas.InternalBuffer.Append((byte) '[');

            float characterSpacing = cleanUpStrategy.Context.CharacterSpacing;
            float convertedCharacterSpacing = -characterSpacing * 1000f / cleanUpStrategy.Context.FontSize;

            float wordSpacing = cleanUpStrategy.Context.WordSpacing;
            float convertedWordSpacing = -wordSpacing * 1000f / cleanUpStrategy.Context.FontSize;

            float shift = structuredTJoperands != null ? structuredTJoperands[0] : 0;
            PdfCleanUpContentChunk.Text prevChunk = null;

            foreach (PdfCleanUpContentChunk chunk in chunks) {
                PdfCleanUpContentChunk.Text textChunk = (PdfCleanUpContentChunk.Text) chunk;

                if (prevChunk != null && prevChunk.NumOfStrTextBelongsTo != textChunk.NumOfStrTextBelongsTo &&
                        structuredTJoperands != null) {
                    shift += structuredTJoperands[prevChunk.NumOfStrTextBelongsTo];
                }

                if (textChunk.Visible) {
                    if (Util.compare(shift, 0.0f) != 0 && Util.compare(shift, -0.0f) != 0) {
                        canvas.InternalBuffer.Append(shift).Append(' ');
                    }

                    textChunk.GetText().ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
                    canvas.InternalBuffer.Append(' ');

                    shift = convertedCharacterSpacing + (IsSpace(textChunk) ? convertedWordSpacing : 0);
                } else {
                    shift += GetUnscaledTextChunkWidth(textChunk);
                }

                prevChunk = textChunk;
            }

            if (Util.compare(shift, 0) != 0 && Util.compare(shift, -0.0f) != 0) {
                canvas.InternalBuffer.Append(shift);
            }

            canvas.InternalBuffer.Append(TJ);

            if (Util.compare(characterSpacing, 0) != 0 && Util.compare(characterSpacing, -0.0f) != 0) {
                new PdfNumber(characterSpacing).ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
                canvas.InternalBuffer.Append(Tc);
            }

            if (Util.compare(wordSpacing, 0) != 0 && Util.compare(wordSpacing, -0.0f) != 0) {
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
        private float GetUnscaledTextChunkWidth(PdfCleanUpContentChunk.Text chunk) {
            PdfCleanUpContext context = cleanUpStrategy.Context;
            float fontSize = context.FontSize;
            float characterSpacing = context.CharacterSpacing;
            float wordSpacing = context.WordSpacing;
            float horizontalScaling = context.HorizontalScaling;

            // We should multiply by 100 because iText stores horizontal scaling as the value in [0, 1] interval;
            // also we need to add character and word spaces because TextRenderInfo class truncates them from the end of the string
            // (single character string in our case is also truncated)
            float scaledChunkWidth = (chunk.EndX - chunk.StartX) * 100f +
                    (characterSpacing + (IsSpace(chunk) ? wordSpacing : 0)) * horizontalScaling;

            return -scaledChunkWidth * 1000f / (horizontalScaling * fontSize);
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

        private void WritePath(String operatorStr, PdfContentByte canvas) {
            if (cleanUpStrategy.Clipped) {
                byte[] clippingOperator = (cleanUpStrategy.ClippingRule == PathPaintingRenderInfo.NONZERO_WINDING_RULE) ? W : eoW;
                WritePath(cleanUpStrategy.NewClipPath, clippingOperator, canvas);

                if ("n".Equals(operatorStr)) {
                    canvas.InternalBuffer.Append(n);
                    cleanUpStrategy.Clipped = false;
                    return;
                }
            }

            if (nwFillOperators.Contains(operatorStr)) {
                WriteFillAfterClip(canvas, cleanUpStrategy.CurrentFillPath, f, 
                                   cleanUpStrategy.ClippingRule, PathPaintingRenderInfo.NONZERO_WINDING_RULE);
            } else if (eoFillOperators.Contains(operatorStr)) {
                WriteFillAfterClip(canvas, cleanUpStrategy.CurrentFillPath, eoF,
                                   cleanUpStrategy.ClippingRule, PathPaintingRenderInfo.EVEN_ODD_RULE);
            } else if (cleanUpStrategy.Clipped) {
                canvas.InternalBuffer.Append(n);
            }

            if (strokeOperators.Contains(operatorStr)) {
                WritePath(cleanUpStrategy.CurrentStrokePath, f, canvas);
            }

            cleanUpStrategy.Clipped = false;
        }

        private void WritePath(Path path, byte[] pathPaintingOperator, PdfContentByte canvas) {
            if (path.Subpaths.Count == 0) {
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

        private void WriteFillAfterClip(PdfContentByte canvas, Path path, byte[] fillOperator, int clippingRule, int fillRule) {
            if (clippingRule == fillRule) {
                canvas.InternalBuffer.Append(fillOperator);
            } else {
                canvas.InternalBuffer.Append(n);
                WritePath(path, fillOperator, canvas);
            }
        }
    }
}
