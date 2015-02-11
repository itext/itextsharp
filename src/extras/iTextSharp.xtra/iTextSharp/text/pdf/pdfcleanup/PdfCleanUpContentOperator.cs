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

namespace iTextSharp.xtra.iTextSharp.text.pdf.pdfcleanup {

    class PdfCleanUpContentOperator : IContentOperator {

        static private readonly byte[] TStar = DocWriter.GetISOBytes("T*\n");
        static private readonly byte[] Tw = DocWriter.GetISOBytes(" Tw ");
        static private readonly byte[] TcTStar = DocWriter.GetISOBytes(" Tc T*\n");
        static private readonly byte[] TJ = DocWriter.GetISOBytes("] TJ\n");
        static private readonly byte[] Tc = DocWriter.GetISOBytes(" Tc\n");

        private static readonly HashSet2<string> textShowingOperators = new HashSet2<string>() {
            "TJ", "Tj", "'", "\""
        };

        protected PdfCleanUpRenderListener cleanUpStrategy;
        protected IContentOperator originalContentOperator;

        public static void PopulateOperators(PdfContentStreamProcessor contentProcessor,
                                             PdfCleanUpRenderListener pdfCleanUpRenderListener) {
            String[] operators = {
                    PdfContentStreamProcessor.DEFAULTOPERATOR, "q", "Q", "g", "G", "rg", "RG", "k", "K",
                    "cs", "CS", "sc", "SC", "scn", "SCN", "cm", "gs", "Tc", "Tw", "Tz", "TL", "Tf", "Tr",
                    "Ts", "BT", "ET", "BMC", "BDC", "EMC", "Td", "TD", "Tm", "T*", "Tj", "'", "\"", "TJ", "Do"
            };

            foreach (String @operator in operators) {
                PdfCleanUpContentOperator contentOperator = new PdfCleanUpContentOperator(pdfCleanUpRenderListener);
                contentOperator.originalContentOperator = contentProcessor.RegisterContentOperator(@operator, contentOperator);
            }
        }

        public PdfCleanUpContentOperator(PdfCleanUpRenderListener cleanUpStrategy) {
            this.cleanUpStrategy = cleanUpStrategy;
        }

        public void Invoke(PdfContentStreamProcessor pdfContentStreamProcessor, PdfLiteral @operator, List<PdfObject> operands) {
            String operatorStr = @operator.ToString();
            PdfContentByte canvas = cleanUpStrategy.Context.Canvas;
            PRStream xFormStream = null;

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

            originalContentOperator.Invoke(pdfContentStreamProcessor, @operator, operands);
            IList<PdfCleanUpContentChunk> chunks = cleanUpStrategy.Chunks;
            bool disableOutput = false;

            if (xFormStream != null) {
                xFormStream.SetData(cleanUpStrategy.Context.Canvas.ToPdf(cleanUpStrategy.Context.Canvas.PdfWriter));
                cleanUpStrategy.PopContext();
                canvas = cleanUpStrategy.Context.Canvas;
            }

            if ("Do" == operatorStr) {
                if (chunks.Count > 0 && chunks[0].IsImage()) {
                    PdfCleanUpContentChunk chunk = chunks[0];

                    if (chunk.IsVisible()) {
                        PdfDictionary xObjResources = cleanUpStrategy.Context.Resources.GetAsDict(PdfName.XOBJECT);
                        PRStream imageStream = (PRStream) xObjResources.GetAsStream((PdfName) operands[0]);
                        UpdateImage(imageStream, chunk.NewImageData);
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
            } else if (textShowingOperators.Contains(operatorStr) && !AllChunksAreVisible(cleanUpStrategy.Chunks)) {
                disableOutput = true;

                if ("'" == operatorStr) {
                    canvas.InternalBuffer.Append(TStar);
                } else if ("\"" == operatorStr) {
                    operands[0].ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
                    canvas.InternalBuffer.Append(Tw);

                    operands[1].ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
                    canvas.InternalBuffer.Append(TcTStar);

                    cleanUpStrategy.Context.CharacterSpacing = ((PdfNumber) operands[1]).FloatValue;
                } else if ("TJ" == operatorStr) {
                    structuredTJoperands = StructureTJarray((PdfArray) operands[0]);
                }

                RenderChunks(structuredTJoperands, chunks, canvas);
            } else if ("\"" == operatorStr) {
                cleanUpStrategy.Context.CharacterSpacing = ((PdfNumber) operands[1]).FloatValue;
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
                if (!chunk.IsVisible()) {
                    return false;
                }
            }

            return true;
        }

        /** 
         * Overriding standard PdfObject.toPdf because we need sorted PdfDictionaries.
         */
        static private void ToPdf(PdfObject @object, PdfWriter writer, ByteBuffer os) {
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

        private void RenderChunks(IDictionary<int, float> structuredTJoperands, IList<PdfCleanUpContentChunk> chunks, PdfContentByte canvas) {
            canvas.SetCharacterSpacing(0);
            canvas.SetWordSpacing(0);
            canvas.InternalBuffer.Append((byte) '[');

            float characterSpacing = cleanUpStrategy.Context.CharacterSpacing;
            float convertedCharacterSpacing = -characterSpacing * 1000f / cleanUpStrategy.Context.FontSize;

            float wordSpacing = cleanUpStrategy.Context.WordSpacing;
            float convertedWordSpacing = -wordSpacing * 1000f / cleanUpStrategy.Context.FontSize;

            float shift = structuredTJoperands != null ? structuredTJoperands[0] : 0;
            PdfCleanUpContentChunk prevChunk = null;

            foreach (PdfCleanUpContentChunk chunk in chunks) {
                if (prevChunk != null && prevChunk.NumOfStrChunkBelongsTo != chunk.NumOfStrChunkBelongsTo &&
                        structuredTJoperands != null) {
                    shift += structuredTJoperands[prevChunk.NumOfStrChunkBelongsTo];
                }

                if (chunk.IsVisible()) {
                    if (Util.compare(shift, 0.0f) != 0 && Util.compare(shift, -0.0f) != 0) {
                        canvas.InternalBuffer.Append(shift).Append(' ');
                    }

                    chunk.Text.ToPdf(canvas.PdfWriter, canvas.InternalBuffer);
                    canvas.InternalBuffer.Append(' ');

                    shift = convertedCharacterSpacing + (IsSpace(chunk) ? convertedWordSpacing : 0);
                } else {
                    float unscaledChunkWidth = GetUnscaledChunkWidth(chunk);
                    shift += unscaledChunkWidth;
                }

                prevChunk = chunk;
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
         * Here we are calculating Tj coefficient for previous chunk
         * For details see PDF spec., Text Space Details, formula for "tx" coefficient
         * and TextRenderInfo class (getUnscaledBaseline)
         */
        private float GetUnscaledChunkWidth(PdfCleanUpContentChunk chunk) {
            PdfCleanUpContext context = cleanUpStrategy.Context;
            float fontSize = context.FontSize;
            float characterSpacing = context.CharacterSpacing;
            float wordSpacing = context.WordSpacing;
            float horizontalScaling = context.HorizontalScaling;

            // we should multiply by 100 because iText stores horizontal scaling as the value in [0, 1] interval;
            // also we need to add character and word spaces because TextRenderInfo class truncates them from the end of the string
            // (single character string in our case is also truncated)
            float scaledChunkWidth = (chunk.EndX - chunk.StartX) * 100f +
                    (characterSpacing + (IsSpace(chunk) ? wordSpacing : 0)) * horizontalScaling;

            return -scaledChunkWidth * 1000f / (horizontalScaling * fontSize);
        }

        private bool IsSpace(PdfCleanUpContentChunk chunk) {
            return chunk.Text.ToUnicodeString() == " ";
        }

        private void UpdateImage(PRStream imageStream, byte[] newData) {
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
    }
}
