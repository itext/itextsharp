using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using iTextSharp.text.error_messages;
using iTextSharp.text.io;

/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Kevin Day, Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * ITEXT GROUP. ITEXT GROUP DISCLAIMS THE WARRANTY OF NON INFRINGEMENT
 * OF THIRD PARTY RIGHTS
 *
 * This program is distributed in the hope that it will be useful, but
 * WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
 * or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program; if not, see http://www.gnu.org/licenses or write to
 * the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL:
 * http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions
 * of this program must display Appropriate Legal Notices, as required under
 * Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License,
 * a covered work must retain the producer line in every PDF that is created
 * or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing
 * a commercial license. Buying such a license is mandatory as soon as you
 * develop commercial activities involving the iText software without
 * disclosing the source code of your own applications.
 * These activities include: offering paid services to customers as an ASP,
 * serving PDFs on the fly in a web application, shipping iText with a closed
 * source product.
 *
 * For more information, please contact iText Software Corp. at this
 * address: sales@itextpdf.com
 */
namespace iTextSharp.text.pdf.parser {

    /**
     * Processor for a PDF content Stream.
     * @since   2.1.4
     */
    public class PdfContentStreamProcessor {
        /**
         * Default oper
         * @since 5.0.1
         */
        public const String DEFAULTOPERATOR = "DefaultOperator";
        
        /** A map with all supported operators (PDF syntax). */
        private IDictionary<String, IContentOperator> operators;
        /** Resources for the content stream. */
        private ResourceDictionary resources;
        /** Stack keeping track of the graphics state. */
        private Stack<GraphicsState> gsStack = new Stack<GraphicsState>();
        /** Text matrix. */
        private Matrix textMatrix;
        /** Text line matrix. */
        private Matrix textLineMatrix;
        /** Listener that will be notified of render events */
        private IRenderListener renderListener;
        /** A map with all supported XObject handlers */
        private IDictionary<PdfName, IXObjectDoHandler> xobjectDoHandlers;
        /**
         * The font cache.
         * @since 5.0.6
         */
        /**  */
        private IDictionary<int,CMapAwareDocumentFont> cachedFonts = new Dictionary<int, CMapAwareDocumentFont>();
        /**
         * A stack containing marked content info.
         * @since 5.0.2
         */
        private Stack<MarkedContentInfo> markedContentStack = new Stack<MarkedContentInfo>();

        /**
         * Creates a new PDF Content Stream Processor that will send it's output to the
         * designated render listener.
         *
         * @param renderListener the {@link RenderListener} that will receive rendering notifications
         */
        public PdfContentStreamProcessor(IRenderListener renderListener) {
            this.renderListener = renderListener;
            operators = new Dictionary<String, IContentOperator>();
            PopulateOperators();
            xobjectDoHandlers = new Dictionary<PdfName, IXObjectDoHandler>();
            PopulateXObjectDoHandlers();
            Reset();
        }

        private void PopulateXObjectDoHandlers(){
            RegisterXObjectDoHandler(PdfName.DEFAULT, new IgnoreXObjectDoHandler());
            RegisterXObjectDoHandler(PdfName.FORM, new FormXObjectDoHandler());
            RegisterXObjectDoHandler(PdfName.IMAGE, new ImageXObjectDoHandler());       
        }
        
        /**
         * Registers a Do handler that will be called when Do for the provided XObject subtype is encountered during content processing.
         * <br>
         * If you register a handler, it is a very good idea to pass the call on to the existing registered handler (returned by this call), otherwise you
         * may inadvertently change the internal behavior of the processor.
         * @param xobjectSubType the XObject subtype this handler will process, or PdfName.DEFAULT for a catch-all handler
         * @param handler the handler that will receive notification when the Do oper for the specified subtype is encountered
         * @return the existing registered handler, if any
         * @since 5.0.1
         */
        virtual public IXObjectDoHandler RegisterXObjectDoHandler(PdfName xobjectSubType, IXObjectDoHandler handler){
            IXObjectDoHandler old;
            xobjectDoHandlers.TryGetValue(xobjectSubType, out old);
            xobjectDoHandlers[xobjectSubType] = handler;
            return old;
        }
        
        /**
         * Gets the font pointed to by the indirect reference. The font may have been cached.
         * @param ind the indirect reference ponting to the font
         * @return the font
         * @since 5.0.6
         */
        private CMapAwareDocumentFont GetFont(PRIndirectReference ind) {
            CMapAwareDocumentFont font;
            cachedFonts.TryGetValue(ind.Number, out font);
            if (font == null) {
                font = new CMapAwareDocumentFont(ind);
                cachedFonts[ind.Number] = font;
            }
            return font;
        }

        private CMapAwareDocumentFont GetFont(PdfDictionary fontResource) {
            return new CMapAwareDocumentFont(fontResource);
        }

        /**
         * Loads all the supported graphics and text state operators in a map.
         */
        private void PopulateOperators(){

            RegisterContentOperator(DEFAULTOPERATOR, new IgnoreOperatorContentOperator());
            
            RegisterContentOperator("q", new PushGraphicsState());
            RegisterContentOperator("Q", new PopGraphicsState());
            RegisterContentOperator("g", new SetGrayFill());
            RegisterContentOperator("G", new SetGrayStroke());
            RegisterContentOperator("rg", new SetRGBFill());
            RegisterContentOperator("RG", new SetRGBStroke());
            RegisterContentOperator("k", new SetCMYKFill());
            RegisterContentOperator("K", new SetCMYKStroke());
            RegisterContentOperator("cs", new SetColorSpaceFill());
            RegisterContentOperator("CS", new SetColorSpaceStroke());
            RegisterContentOperator("sc", new SetColorFill());
            RegisterContentOperator("SC", new SetColorStroke());
            RegisterContentOperator("scn", new SetColorFill());
            RegisterContentOperator("SCN", new SetColorStroke());
            RegisterContentOperator("cm", new ModifyCurrentTransformationMatrix());
            RegisterContentOperator("gs", new ProcessGraphicsStateResource());

            SetTextCharacterSpacing tcOperator = new SetTextCharacterSpacing();
            RegisterContentOperator("Tc", tcOperator);
            SetTextWordSpacing twOperator = new SetTextWordSpacing();
            RegisterContentOperator("Tw", twOperator);
            RegisterContentOperator("Tz", new SetTextHorizontalScaling());
            SetTextLeading tlOperator = new SetTextLeading();
            RegisterContentOperator("TL", tlOperator);
            RegisterContentOperator("Tf", new SetTextFont());
            RegisterContentOperator("Tr", new SetTextRenderMode());
            RegisterContentOperator("Ts", new SetTextRise());

            RegisterContentOperator("BT", new BeginTextC());
            RegisterContentOperator("ET", new EndTextC());
            RegisterContentOperator("BMC", new BeginMarkedContentC());
            RegisterContentOperator("BDC", new BeginMarkedContentDictionary());
            RegisterContentOperator("EMC", new EndMarkedContentC());

            TextMoveStartNextLine tdOperator = new TextMoveStartNextLine();
            RegisterContentOperator("Td", tdOperator);
            RegisterContentOperator("TD", new TextMoveStartNextLineWithLeading(tdOperator, tlOperator));
            RegisterContentOperator("Tm", new TextSetTextMatrix());
            TextMoveNextLine tstarOperator = new TextMoveNextLine(tdOperator);
            RegisterContentOperator("T*", tstarOperator);

            ShowText tjOperator = new ShowText();
            RegisterContentOperator("Tj", tjOperator);
            MoveNextLineAndShowText tickOperator = new MoveNextLineAndShowText(tstarOperator, tjOperator);
            RegisterContentOperator("'", tickOperator);
            RegisterContentOperator("\"", new MoveNextLineAndShowTextWithSpacing(twOperator, tcOperator, tickOperator));
            RegisterContentOperator("TJ", new ShowTextArray());

            RegisterContentOperator("Do", new Do());

            RegisterContentOperator("w", new SetLineWidth());
            RegisterContentOperator("J", new SetLineCap());
            RegisterContentOperator("j", new SetLineJoin());
            RegisterContentOperator("M", new SetMiterLimit());
            RegisterContentOperator("d", new SetLineDashPattern());

            // Path construction and painting operators
            if (renderListener is IExtRenderListener) {
                int fillStroke = PathPaintingRenderInfo.FILL | PathPaintingRenderInfo.STROKE;
                RegisterContentOperator("m", new MoveTo());
                RegisterContentOperator("l", new LineTo());
                RegisterContentOperator("c", new Curve());
                RegisterContentOperator("v", new CurveFirstPointDuplicated());
                RegisterContentOperator("y", new CurveFourhPointDuplicated());
                RegisterContentOperator("h", new CloseSubpath());
                RegisterContentOperator("re", new Rectangle());
                RegisterContentOperator("S", new PaintPathOp(PathPaintingRenderInfo.STROKE, -1, false));
                RegisterContentOperator("s", new PaintPathOp(PathPaintingRenderInfo.STROKE, -1, true));
                RegisterContentOperator("f", new PaintPathOp(PathPaintingRenderInfo.FILL, PathPaintingRenderInfo.NONZERO_WINDING_RULE, false));
                RegisterContentOperator("F", new PaintPathOp(PathPaintingRenderInfo.FILL, PathPaintingRenderInfo.NONZERO_WINDING_RULE, false));
                RegisterContentOperator("f*", new PaintPathOp(PathPaintingRenderInfo.FILL, PathPaintingRenderInfo.EVEN_ODD_RULE, false));
                RegisterContentOperator("B", new PaintPathOp(fillStroke, PathPaintingRenderInfo.NONZERO_WINDING_RULE, false));
                RegisterContentOperator("B*", new PaintPathOp(fillStroke, PathPaintingRenderInfo.EVEN_ODD_RULE, false));
                RegisterContentOperator("b", new PaintPathOp(fillStroke, PathPaintingRenderInfo.NONZERO_WINDING_RULE, true));
                RegisterContentOperator("b*", new PaintPathOp(fillStroke, PathPaintingRenderInfo.EVEN_ODD_RULE, true));
                RegisterContentOperator("n", new PaintPathOp(PathPaintingRenderInfo.NO_OP, -1, false));
                RegisterContentOperator("W", new ClipPathOp(PathPaintingRenderInfo.NONZERO_WINDING_RULE));
                RegisterContentOperator("W*", new ClipPathOp(PathPaintingRenderInfo.EVEN_ODD_RULE));
            }
        }

        /**
         * Registers a content oper that will be called when the specified oper string is encountered during content processing.
         * <br>
         * If you register an oper, it is a very good idea to pass the call on to the existing registered oper (returned by this call), otherwise you
         * may inadvertently change the internal behavior of the processor.
         * @param operatorString the oper id, or DEFAULTOPERATOR for a catch-all oper
         * @param oper the oper that will receive notification when the oper is encountered
         * @return the existing registered oper, if any
         * @since 2.1.7
         */
        virtual public IContentOperator RegisterContentOperator(String operatorString, IContentOperator oper){
            IContentOperator value = null;
            if (operators.ContainsKey(operatorString))
                value = operators[operatorString];
            operators[operatorString] = oper;
            return value;
        }

        /**
         * @return {@link java.util.Collection} containing all the registered operators strings
         * @since 5.5.6
         */
        public virtual ICollection<String> RegisteredOperatorStrings {
            get { return new List<string>(operators.Keys); }
        }

        /**
         * Resets the graphics state stack, matrices and resources.
         */
        virtual public void Reset() {
            gsStack.Clear();
            gsStack.Push(new GraphicsState());
            textMatrix = null;
            textLineMatrix = null;
            resources = new ResourceDictionary();
        }

        /**
         * Returns the current graphics state.
         * @return  the graphics state
         */
        public GraphicsState Gs() {
            return gsStack.Peek();
        }

        /**
         * Invokes an oper.
         * @param oper  the PDF Syntax of the oper
         * @param operands  a list with operands
         */
        private void InvokeOperator(PdfLiteral oper, List<PdfObject> operands) {
            IContentOperator op;
            operators.TryGetValue(oper.ToString(), out op);
            if (op == null)
                op = operators[DEFAULTOPERATOR];

            op.Invoke(this, oper, operands);
        }

        /**
         * Add to the marked content stack
         * @param tag the tag of the marked content
         * @param dict the PdfDictionary associated with the marked content
         * @since 5.0.2
         */
        private void BeginMarkedContent(PdfName tag, PdfDictionary dict) {
            markedContentStack.Push(new MarkedContentInfo(tag, dict));
        }
       
        /**
         * Remove the latest marked content from the stack.  Keeps track of the BMC, BDC and EMC operators.
         * @since 5.0.2
         */
        private void EndMarkedContent() {
            markedContentStack.Pop();
        }

        /**
         * Used to trigger beginTextBlock on the renderListener
         */
        private void BeginText(){
            renderListener.BeginTextBlock();
        }

        /**
         * Used to trigger endTextBlock on the renderListener
         */
        private void EndText(){
            renderListener.EndTextBlock();
        }

        /**
         * Displays text.
         * @param string    the text to display
         */
        private void DisplayPdfString(PdfString str) {

            TextRenderInfo renderInfo = new TextRenderInfo(str, Gs(), textMatrix, markedContentStack);

            renderListener.RenderText(renderInfo);

            textMatrix = new Matrix(renderInfo.GetUnscaledWidth(), 0).Multiply(textMatrix);
        }
        
        /**
         * Displays an XObject using the registered handler for this XObject's subtype
         * @param xobjectName the name of the XObject to retrieve from the resource dictionary
         */
        private void DisplayXObject(PdfName xobjectName) {
            PdfDictionary xobjects = resources.GetAsDict(PdfName.XOBJECT);
            PdfObject xobject = xobjects.GetDirectObject(xobjectName);
            PdfStream xobjectStream = (PdfStream)xobject;
            
            PdfName subType = xobjectStream.GetAsName(PdfName.SUBTYPE);
            if (xobject.IsStream()){
                IXObjectDoHandler handler;
                xobjectDoHandlers.TryGetValue(subType, out handler);
                if (handler == null)
                    handler = xobjectDoHandlers[PdfName.DEFAULT];
                handler.HandleXObject(this, xobjectStream, xobjects.GetAsIndirectObject(xobjectName));
            } else {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("XObject.1.is.not.a.stream", xobjectName));
            }
            
        }

        /**
         * Displays the current path.
         *
         * @param operation One of the possible combinations of {@link com.itextpdf.text.pdf.parser.PathPaintingRenderInfo#STROKE}
         *                  and {@link com.itextpdf.text.pdf.parser.PathPaintingRenderInfo#FILL} values or
         *                  {@link com.itextpdf.text.pdf.parser.PathPaintingRenderInfo#NO_OP}
         * @param rule      Either {@link com.itextpdf.text.pdf.parser.PathPaintingRenderInfo#NONZERO_WINDING_RULE} or
         *                  {@link com.itextpdf.text.pdf.parser.PathPaintingRenderInfo#EVEN_ODD_RULE}
         *                  In case it isn't applicable pass any <CODE>int</CODE> value.
         * @param close     Indicates whether the path should be closed or not.
         * @since 5.5.6
         */
        private void PaintPath(int operation, int rule, bool close) {
            if (close) {
                ModifyPath(PathConstructionRenderInfo.CLOSE, null);
            }

            PathPaintingRenderInfo renderInfo = new PathPaintingRenderInfo(operation, rule, Gs());
            ((IExtRenderListener) renderListener).RenderPath(renderInfo);
        }

        /**
         * Modifies the current path.
         *
         * @param operation   Indicates which path-construction operation should be performed.
         * @param segmentData Contains x, y components of points of a new segment being added to the current path.
         *                    E.g. x1 y1 x2 y2 x3 y3 etc. It's ignored for "close subpath" operarion (h).
         */
        private void ModifyPath(int operation, IList<float> segmentData) {
            PathConstructionRenderInfo renderInfo = new PathConstructionRenderInfo(operation, segmentData, Gs().GetCtm());
            ((IExtRenderListener) renderListener).ModifyPath(renderInfo);
        }

        private void ClipPath(int rule) {
            ((IExtRenderListener) renderListener).ClipPath(rule);
        }


        /**
         * Adjusts the text matrix for the specified adjustment value (see TJ oper in the PDF spec for information)
         * @param tj the text adjustment
         */
        private void ApplyTextAdjust(float tj){
            float adjustBy = -tj/1000f * Gs().fontSize * Gs().horizontalScaling;

            textMatrix = new Matrix(adjustBy, 0).Multiply(textMatrix);
        }
        
        /**
         * Processes PDF syntax.
         * <b>Note:</b> If you re-use a given {@link PdfContentStreamProcessor}, you must call {@link PdfContentStreamProcessor#reset()}
         * @param contentBytes  the bytes of a content stream
         * @param resources     the resources that come with the content stream
         */
        virtual public void ProcessContent(byte[] contentBytes, PdfDictionary resources){
            this.resources.Push(resources);
            PRTokeniser tokeniser = new PRTokeniser(new RandomAccessFileOrArray(new RandomAccessSourceFactory().CreateSource(contentBytes)));
            PdfContentParser ps = new PdfContentParser(tokeniser);
            List<PdfObject> operands = new List<PdfObject>();
            while (ps.Parse(operands).Count > 0){
                PdfLiteral oper = (PdfLiteral)operands[operands.Count-1];
                if ("BI".Equals(oper.ToString())){
                    // we don't call invokeOperator for embedded images - this is one area of the PDF spec that is particularly nasty and inconsistent
                    PdfDictionary colorSpaceDic = resources != null ? resources.GetAsDict(PdfName.COLORSPACE) : null;
                    HandleInlineImage(InlineImageUtils.ParseInlineImage(ps, colorSpaceDic), colorSpaceDic);
                } else {
                    InvokeOperator(oper, operands);
                }
            }
            this.resources.Pop();
        }

        /**
         * Callback when an inline image is found.  This requires special handling because inline images don't follow the standard operator syntax
         * @param info the inline image
         * @param colorSpaceDic the color space for the inline immage
         */
        virtual protected void HandleInlineImage(InlineImageInfo info, PdfDictionary colorSpaceDic)
        {
            ImageRenderInfo renderInfo = ImageRenderInfo.CreateForEmbeddedImage(Gs(), info, colorSpaceDic);
            renderListener.RenderImage(renderInfo);
        }


        /**
         * A resource dictionary that allows stack-like behavior to support resource dictionary inheritance
         */
        private class ResourceDictionary : PdfDictionary {
            private IList<PdfDictionary> resourcesStack = new List<PdfDictionary>();
            
            virtual public void Push(PdfDictionary resources){
                resourcesStack.Add(resources);
            }

            virtual public void Pop(){
                resourcesStack.RemoveAt(resourcesStack.Count-1);
            }

            public override PdfObject GetDirectObject(PdfName key) {
                for (int i = resourcesStack.Count - 1; i >= 0; i--){
                    PdfDictionary subResource = resourcesStack[i];
                    if (subResource != null){
                        PdfObject obj =  subResource.GetDirectObject(key);
                        if (obj != null) return obj;
                    }
                }
                return base.GetDirectObject(key); // shouldn't be necessary, but just in case we've done something crazy
            }
        }
        
        /**
         * A content oper implementation (unregistered).
         */
        private class IgnoreOperatorContentOperator : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands){
                // ignore the oper
            }
        }
        
        /**
         * A content oper implementation (TJ).
         */
        private class ShowTextArray : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfArray array = (PdfArray)operands[0];
                float tj = 0;
                foreach (PdfObject entryObj in array) {
                    if (entryObj is PdfString){
                        processor.DisplayPdfString((PdfString)entryObj);
                        tj = 0;
                    } else {
                        tj = ((PdfNumber)entryObj).FloatValue;
                        processor.ApplyTextAdjust(tj);
                    }
                }

            }
        }

        /**
         * A content oper implementation (").
         */
        private class MoveNextLineAndShowTextWithSpacing : IContentOperator{
            private SetTextWordSpacing setTextWordSpacing;
            private SetTextCharacterSpacing setTextCharacterSpacing;
            private MoveNextLineAndShowText moveNextLineAndShowText;

            public MoveNextLineAndShowTextWithSpacing(SetTextWordSpacing setTextWordSpacing, SetTextCharacterSpacing setTextCharacterSpacing, MoveNextLineAndShowText moveNextLineAndShowText) {
                this.setTextWordSpacing = setTextWordSpacing;
                this.setTextCharacterSpacing = setTextCharacterSpacing;
                this.moveNextLineAndShowText = moveNextLineAndShowText;
            }

            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfNumber aw = (PdfNumber)operands[0];
                PdfNumber ac = (PdfNumber)operands[1];
                PdfString str = (PdfString)operands[2];

                List<PdfObject> twOperands = new List<PdfObject>(1);
                twOperands.Insert(0, aw);
                setTextWordSpacing.Invoke(processor, null, twOperands);

                List<PdfObject> tcOperands = new List<PdfObject>(1);
                tcOperands.Insert(0, ac);
                setTextCharacterSpacing.Invoke(processor, null, tcOperands);

                List<PdfObject> tickOperands = new List<PdfObject>(1);
                tickOperands.Insert(0, str);
                moveNextLineAndShowText.Invoke(processor, null, tickOperands);
            }
        }

        /**
         * A content oper implementation (').
         */
        private class MoveNextLineAndShowText : IContentOperator{
            private TextMoveNextLine textMoveNextLine;
            private ShowText showText;
            public MoveNextLineAndShowText(TextMoveNextLine textMoveNextLine, ShowText showText) {
                this.textMoveNextLine = textMoveNextLine;
                this.showText = showText;
            }

            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                textMoveNextLine.Invoke(processor, null, new List<PdfObject>(0));
                showText.Invoke(processor, null, operands);
            }
        }

        /**
         * A content oper implementation (Tj).
         */
        private class ShowText : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfString str = (PdfString)operands[0];

                processor.DisplayPdfString(str);
            }
        }


        /**
         * A content oper implementation (T*).
         */
        private class TextMoveNextLine : IContentOperator{
            private TextMoveStartNextLine moveStartNextLine;
            public TextMoveNextLine(TextMoveStartNextLine moveStartNextLine){
                this.moveStartNextLine = moveStartNextLine;
            }

            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                List<PdfObject> tdoperands = new List<PdfObject>(2);
                tdoperands.Insert(0, new PdfNumber(0));
                tdoperands.Insert(1, new PdfNumber(-processor.Gs().leading));
                moveStartNextLine.Invoke(processor, null, tdoperands);
            }
        }

        /**
         * A content oper implementation (Tm).
         */
        private class TextSetTextMatrix : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                float a = ((PdfNumber)operands[0]).FloatValue;
                float b = ((PdfNumber)operands[1]).FloatValue;
                float c = ((PdfNumber)operands[2]).FloatValue;
                float d = ((PdfNumber)operands[3]).FloatValue;
                float e = ((PdfNumber)operands[4]).FloatValue;
                float f = ((PdfNumber)operands[5]).FloatValue;

                processor.textLineMatrix = new Matrix(a, b, c, d, e, f);
                processor.textMatrix = processor.textLineMatrix;
            }
        }

        /**
         * A content oper implementation (TD).
         */
        private class TextMoveStartNextLineWithLeading : IContentOperator{
            private TextMoveStartNextLine moveStartNextLine;
            private SetTextLeading setTextLeading;
            public TextMoveStartNextLineWithLeading(TextMoveStartNextLine moveStartNextLine, SetTextLeading setTextLeading){
                this.moveStartNextLine = moveStartNextLine;
                this.setTextLeading = setTextLeading;
            }
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                float ty = ((PdfNumber)operands[1]).FloatValue;

                List<PdfObject> tlOperands = new List<PdfObject>(1);
                tlOperands.Insert(0, new PdfNumber(-ty));
                setTextLeading.Invoke(processor, null, tlOperands);
                moveStartNextLine.Invoke(processor, null, operands);
            }
        }

        /**
         * A content oper implementation (Td).
         */
        private class TextMoveStartNextLine : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                float tx = ((PdfNumber)operands[0]).FloatValue;
                float ty = ((PdfNumber)operands[1]).FloatValue;

                Matrix translationMatrix = new Matrix(tx, ty);
                processor.textMatrix =  translationMatrix.Multiply(processor.textLineMatrix);
                processor.textLineMatrix = processor.textMatrix;
            }
        }

        /**
         * A content oper implementation (Tf).
         */
        private class SetTextFont : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfName fontResourceName = (PdfName)operands[0];
                float size = ((PdfNumber)operands[1]).FloatValue;

                PdfDictionary fontsDictionary = processor.resources.GetAsDict(PdfName.FONT);
                CMapAwareDocumentFont font;
                PdfObject fontObject = fontsDictionary.Get(fontResourceName);
                if (fontObject is PdfDictionary)
                    font = processor.GetFont((PdfDictionary)fontObject);
                else
                    font = processor.GetFont((PRIndirectReference)fontObject);

                processor.Gs().font = font;
                processor.Gs().fontSize = size;

            }
        }

        /**
         * A content oper implementation (Tr).
         */
        private class SetTextRenderMode : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfNumber render = (PdfNumber)operands[0];
                processor.Gs().renderMode = render.IntValue;
            }
        }

        /**
         * A content oper implementation (Ts).
         */
        private class SetTextRise : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfNumber rise = (PdfNumber)operands[0];
                processor.Gs().rise = rise.FloatValue;
            }
        }

        /**
         * A content oper implementation (TL).
         */
        private class SetTextLeading : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfNumber leading = (PdfNumber)operands[0];
                processor.Gs().leading = leading.FloatValue;
            }
        }

        /**
         * A content oper implementation (Tz).
         */
        private class SetTextHorizontalScaling : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfNumber scale = (PdfNumber)operands[0];
                processor.Gs().horizontalScaling = scale.FloatValue/100f;
            }
        }

        /**
         * A content oper implementation (Tc).
         */
        private class SetTextCharacterSpacing : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfNumber charSpace = (PdfNumber)operands[0];
                processor.Gs().characterSpacing = charSpace.FloatValue;
            }
        }

        /**
         * A content oper implementation (Tw).
         */
        private class SetTextWordSpacing : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfNumber wordSpace = (PdfNumber)operands[0];
                processor.Gs().wordSpacing = wordSpace.FloatValue;
            }
        }

        /**
         * A content oper implementation (gs).
         */
        private class ProcessGraphicsStateResource : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {

                PdfName dictionaryName = (PdfName)operands[0];
                PdfDictionary extGState = processor.resources.GetAsDict(PdfName.EXTGSTATE);
                if (extGState == null)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("resources.do.not.contain.extgstate.entry.unable.to.process.operator.1", oper));
                PdfDictionary gsDic = extGState.GetAsDict(dictionaryName);
                if (gsDic == null)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("1.is.an.unknown.graphics.state.dictionary", dictionaryName));

                // at this point, all we care about is the FONT entry in the GS dictionary
                PdfArray fontParameter = gsDic.GetAsArray(PdfName.FONT);
                if (fontParameter != null){
                    CMapAwareDocumentFont font = processor.GetFont((PRIndirectReference)fontParameter[0]);
                    float size = fontParameter.GetAsNumber(1).FloatValue;

                    processor.Gs().font = font;
                    processor.Gs().fontSize = size;
                }
            }
        }

        /**
         * A content oper implementation (q).
         */
        private class PushGraphicsState : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                GraphicsState gs = processor.gsStack.Peek();
                GraphicsState copy = new GraphicsState(gs);
                processor.gsStack.Push(copy);
            }
        }

        /**
         * A content oper implementation (cm).
         */
        private class ModifyCurrentTransformationMatrix : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                float a = ((PdfNumber)operands[0]).FloatValue;
                float b = ((PdfNumber)operands[1]).FloatValue;
                float c = ((PdfNumber)operands[2]).FloatValue;
                float d = ((PdfNumber)operands[3]).FloatValue;
                float e = ((PdfNumber)operands[4]).FloatValue;
                float f = ((PdfNumber)operands[5]).FloatValue;
                Matrix matrix = new Matrix(a, b, c, d, e, f);
                GraphicsState gs = processor.gsStack.Peek();
                gs.ctm = matrix.Multiply(gs.ctm);
            }
        }

            
        /**
         * Gets a color based on a list of operands.
         */
        private static BaseColor GetColor(PdfName colorSpace, List<PdfObject> operands) {
    	    if (PdfName.DEVICEGRAY.Equals(colorSpace)) {
    		    return GetColor(1, operands);
    	    }
    	    if (PdfName.DEVICERGB.Equals(colorSpace)) {
    		    return GetColor(3, operands);
    	    }
    	    if (PdfName.DEVICECMYK.Equals(colorSpace)) {
    		    return GetColor(4, operands);
    	    }
    	    return null;
        }
    
        /**
         * Gets a color based on a list of operands.
         */
        private static BaseColor GetColor(int nOperands, List<PdfObject> operands) {
    	    float[] c = new float[nOperands];
    	    for (int i = 0; i < nOperands; i++) {
    		    c[i] = ((PdfNumber)operands[i]).FloatValue;
                // fallbacks for illegal values: handled as Acrobat and Foxit do
                if (c[i] > 1f) {
                    c[i] = 1f;
                }
                else if (c[i] < 0f) {
                    c[i] = 0f;
                }
    	    }
    	    switch (nOperands) {
    	    case 1:
    		    return new GrayColor(c[0]);
    	    case 3:
    		    return new BaseColor(c[0], c[1], c[2]);
    	    case 4:
    		    return new CMYKColor(c[0], c[1], c[2], c[3]);
    	    }
    	    return null;
        }
    
        /**
         * A content operator implementation (g).
         */
        private class SetGrayFill : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.Gs().fillColor = GetColor(1, operands);
            }
        }
    
        /**
         * A content operator implementation (G).
         */
        private class SetGrayStroke : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.Gs().strokeColor = GetColor(1, operands);
            }
        }
    
        /**
         * A content operator implementation (rg).
         */
        private class SetRGBFill : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.Gs().fillColor = GetColor(3, operands);
            }
        }
    
        /**
         * A content operator implementation (RG).
         */
        private class SetRGBStroke : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.Gs().strokeColor = GetColor(3, operands);
            }
        }
    
        /**
         * A content operator implementation (rg).
         */
        private class SetCMYKFill : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.Gs().fillColor = GetColor(4, operands);
            }
        }
    
        /**
         * A content operator implementation (RG).
         */
        private class SetCMYKStroke : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.Gs().strokeColor = GetColor(4, operands);
            }
        }

        /**
         * A content operator implementation (cs).
         */
        private class SetColorSpaceFill : IContentOperator{
		    virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
			    processor.Gs().colorSpaceFill = (PdfName)operands[0];		
		    }
        }

        /**
         * A content operator implementation (CS).
         */
        private class SetColorSpaceStroke : IContentOperator{
		    virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
			    processor.Gs().colorSpaceStroke = (PdfName)operands[0];		
		    }
        }
    
        /**
         * A content operator implementation (sc / scn).
         */
        private class SetColorFill : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.Gs().fillColor = GetColor(processor.Gs().colorSpaceFill, operands);
            }
        }
    
        /**
         * A content operator implementation (SC / SCN).
         */
        private class SetColorStroke : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.Gs().strokeColor = GetColor(processor.Gs().colorSpaceStroke, operands);
            }
        }

        /**
         * A content oper implementation (Q).
         */
        private class PopGraphicsState : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.gsStack.Pop();
            }
        }

        /**
         * A content oper implementation (BT).
         */
        private class BeginTextC : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.textMatrix = new Matrix();
                processor.textLineMatrix = processor.textMatrix;
                processor.BeginText();
            }
        }

        /**
         * A content oper implementation (ET).
         */
        private class EndTextC : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.textMatrix = null;
                processor.textLineMatrix = null;
                processor.EndText();
            }
        }

        /**
         * A content oper implementation (BMC).
         * @since 5.0.2
         */
        private class BeginMarkedContentC : IContentOperator{

            virtual public void Invoke(PdfContentStreamProcessor processor,
                    PdfLiteral oper, List<PdfObject> operands)
                    {
                processor.BeginMarkedContent((PdfName)operands[0], new PdfDictionary());
            }
            
        }

        /**
         * A content oper implementation (BDC).
         * @since 5.0.2
         */
        private class BeginMarkedContentDictionary : IContentOperator{

            virtual public void Invoke(PdfContentStreamProcessor processor,
                    PdfLiteral oper, List<PdfObject> operands)
                    {
                
                PdfObject properties = operands[1];
                
                processor.BeginMarkedContent((PdfName)operands[0], GetPropertiesDictionary(properties, processor.resources));
            }
            
            private PdfDictionary GetPropertiesDictionary(PdfObject operand1, ResourceDictionary resources){
                if (operand1.IsDictionary())
                    return (PdfDictionary)operand1;

                PdfName dictionaryName = ((PdfName)operand1);
                return resources.GetAsDict(dictionaryName);
            }
        }

        /**
         * A content oper implementation (EMC).
         * @since 5.0.2
         */
        private class EndMarkedContentC : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor,
                    PdfLiteral oper, List<PdfObject> operands)
                    {
                processor.EndMarkedContent();
            }
        }
        
        /**
         * A content oper implementation (Do).
         */
        private class Do : IContentOperator{
            virtual public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfName xobjectName = (PdfName)operands[0];
                processor.DisplayXObject(xobjectName);
            }
        }

        /**
         * A content operator implementation (w).
         */
        private class SetLineWidth : IContentOperator {

            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                float lineWidth = ((PdfNumber) operands[0]).FloatValue;
                processor.Gs().LineWidth = lineWidth;
            }
        }

        /**
         * A content operator implementation (J).
         */
        private class SetLineCap : IContentOperator {

            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                int lineCap = ((PdfNumber) operands[0]).IntValue;
                processor.Gs().LineCapStyle = lineCap;
            }
        }

        /**
         * A content operator implementation (j).
         */
        private class SetLineJoin : IContentOperator {

            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                int lineJoin = ((PdfNumber) operands[0]).IntValue;
                processor.Gs().LineJoinStyle = lineJoin;
            }
        }

        /**
         * A content operator implementation (M).
         */
        private class SetMiterLimit : IContentOperator {

            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                float miterLimit = ((PdfNumber) operands[0]).FloatValue;
                processor.Gs().MiterLimit = miterLimit;
            }
        }

        /**
         * A content operator implementation (d).
         */
        private class SetLineDashPattern : IContentOperator {

            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                LineDashPattern pattern = new LineDashPattern(((PdfArray) operands[0]),
                                                              ((PdfNumber) operands[1]).FloatValue);
                processor.Gs().SetLineDashPattern(pattern);
            }
        }

        /**
         * A content operator implementation (m).
         *
         * @since 5.5.6
         */
        private class MoveTo : IContentOperator {

            public virtual void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                float x = ((PdfNumber) operands[0]).FloatValue;
                float y = ((PdfNumber) operands[1]).FloatValue;
                processor.ModifyPath(PathConstructionRenderInfo.MOVETO, new List<float>(new float[] {x, y}));
            }
        }

        /**
         * A content operator implementation (l).
         *
         * @since 5.5.6
         */
        private class LineTo : IContentOperator {

            public virtual void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                float x = ((PdfNumber) operands[0]).FloatValue;
                float y = ((PdfNumber) operands[1]).FloatValue;
                processor.ModifyPath(PathConstructionRenderInfo.LINETO, new List<float>(new float[] {x, y}));
            }
        }

        /**
         * A content operator implementation (c).
         *
         * @since 5.5.6
         */
        private class Curve : IContentOperator {

            public virtual void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                float x1 = ((PdfNumber) operands[0]).FloatValue;
                float y1 = ((PdfNumber) operands[1]).FloatValue;
                float x2 = ((PdfNumber) operands[2]).FloatValue;
                float y2 = ((PdfNumber) operands[3]).FloatValue;
                float x3 = ((PdfNumber) operands[4]).FloatValue;
                float y3 = ((PdfNumber) operands[5]).FloatValue;
                processor.ModifyPath(PathConstructionRenderInfo.CURVE_123, new List<float>(new float[] {x1, y1, x2, y2, x3, y3}));
            }
        }

        /**
         * A content operator implementation (v).
         *
         * @since 5.5.6
         */
        private class CurveFirstPointDuplicated : IContentOperator {

            public virtual void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                float x2 = ((PdfNumber) operands[0]).FloatValue;
                float y2 = ((PdfNumber) operands[1]).FloatValue;
                float x3 = ((PdfNumber) operands[2]).FloatValue;
                float y3 = ((PdfNumber) operands[3]).FloatValue;
                processor.ModifyPath(PathConstructionRenderInfo.CURVE_23, new List<float>(new float[] {x2, y2, x3, y3}));
            }
        }

        /**
         * A content operator implementation (y).
         *
         * @since 5.5.6
         */
        private class CurveFourhPointDuplicated : IContentOperator {

            public virtual void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                float x1 = ((PdfNumber) operands[0]).FloatValue;
                float y1 = ((PdfNumber) operands[1]).FloatValue;
                float x3 = ((PdfNumber) operands[2]).FloatValue;
                float y3 = ((PdfNumber) operands[3]).FloatValue;
                processor.ModifyPath(PathConstructionRenderInfo.CURVE_13, new List<float>(new float[] {x1, y1, x3, y3}));
            }
        }

        /**
         * A content operator implementation (h).
         *
         * @since 5.5.6
         */
        private class CloseSubpath : IContentOperator {

            public virtual void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.ModifyPath(PathConstructionRenderInfo.CLOSE, null);
            }
        }

        /**
         * A content operator implementation (re).
         *
         * @since 5.5.6
         */
        private class Rectangle : IContentOperator {

            public virtual void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                float x = ((PdfNumber) operands[0]).FloatValue;
                float y = ((PdfNumber) operands[1]).FloatValue;
                float w = ((PdfNumber) operands[2]).FloatValue;
                float h = ((PdfNumber) operands[3]).FloatValue;
                processor.ModifyPath(PathConstructionRenderInfo.RECT, new List<float>(new float[] {x, y, w, h}));
            }
        }

        /**
         * A content operator implementation (S, s, f, F, f*, B, B*, b, b*).
         *
         * @since 5.5.6
         */
        private class PaintPathOp : IContentOperator {

            private int operation;
            private int rule;
            private bool close;

            /**
             * Constructs PainPath object.
             *
             * @param operation One of the possible combinations of {@link com.itextpdf.text.pdf.parser.PathPaintingRenderInfo#STROKE}
             *                  and {@link com.itextpdf.text.pdf.parser.PathPaintingRenderInfo#FILL} values or
             *                  {@link com.itextpdf.text.pdf.parser.PathPaintingRenderInfo#NO_OP}
             * @param rule      Either {@link com.itextpdf.text.pdf.parser.PathPaintingRenderInfo#NONZERO_WINDING_RULE} or
             *                  {@link com.itextpdf.text.pdf.parser.PathPaintingRenderInfo#EVEN_ODD_RULE}
             *                  In case it isn't applicable pass any value.
             * @param close     Indicates whether the path should be closed or not.
             */
            public PaintPathOp(int operation, int rule, bool close) {
                this.operation = operation;
                this.rule = rule;
                this.close = close;
            }

            public virtual void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.PaintPath(operation, rule, close);
                // TODO: add logic for clipping path (before add it to the graphics state)
            }
        }

        private class ClipPathOp : IContentOperator {

            private int rule;

            public ClipPathOp(int rule) {
                this.rule = rule;
            }

            public virtual void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.ClipPath(rule);
            }
        }

        /**
         * A content operator implementation (n).
         *
         * @since 5.5.6
         */
        private class EndPath : IContentOperator {

            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.PaintPath(PathPaintingRenderInfo.NO_OP, -1, false);
            }
        }
        
        /**
         * An XObject subtype handler for FORM
         */
        private class FormXObjectDoHandler : IXObjectDoHandler{

            virtual public void HandleXObject(PdfContentStreamProcessor processor, PdfStream stream, PdfIndirectReference refi) {
                
                PdfDictionary resources = stream.GetAsDict(PdfName.RESOURCES);

                // we read the content bytes up here so if it fails we don't leave the graphics state stack corrupted
                // this is probably not necessary (if we fail on this, probably the entire content stream processing
                // operation should be rejected
                byte[] contentBytes;
                contentBytes = ContentByteUtils.GetContentBytesFromContentObject(stream);
                PdfArray matrix = stream.GetAsArray(PdfName.MATRIX);

                new PushGraphicsState().Invoke(processor, null, null);

                if (matrix != null){
                    float a = matrix.GetAsNumber(0).FloatValue;
                    float b = matrix.GetAsNumber(1).FloatValue;
                    float c = matrix.GetAsNumber(2).FloatValue;
                    float d = matrix.GetAsNumber(3).FloatValue;
                    float e = matrix.GetAsNumber(4).FloatValue;
                    float f = matrix.GetAsNumber(5).FloatValue;
                    Matrix formMatrix = new Matrix(a, b, c, d, e, f);

                    processor.Gs().ctm = formMatrix.Multiply(processor.Gs().ctm);
                }

                processor.ProcessContent(contentBytes, resources);

                new PopGraphicsState().Invoke(processor, null, null);
                
            }
            
        }
        
        /**
         * An XObject subtype handler for IMAGE
         */
        private class ImageXObjectDoHandler : IXObjectDoHandler{

            virtual public void HandleXObject(PdfContentStreamProcessor processor, PdfStream xobjectStream, PdfIndirectReference refi) {
                PdfDictionary colorSpaceDic = processor.resources.GetAsDict(PdfName.COLORSPACE);
                ImageRenderInfo renderInfo = ImageRenderInfo.CreateForXObject(processor.Gs(), refi, colorSpaceDic);
                processor.renderListener.RenderImage(renderInfo);
            }
        }
        
        /**
         * An XObject subtype handler that does nothing
         */
        private class IgnoreXObjectDoHandler : IXObjectDoHandler{
            virtual public void HandleXObject(PdfContentStreamProcessor processor, PdfStream xobjectStream, PdfIndirectReference refi) {
                // ignore XObject subtype
            }
        }    
    }
}
