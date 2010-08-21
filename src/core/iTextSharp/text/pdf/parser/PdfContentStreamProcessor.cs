using System;
using System.IO;
using System.Collections.Generic;
using iTextSharp.text.pdf;
using iTextSharp.text.error_messages;
/*
 * $Id$
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Kevin Day, Bruno Lowagie, Paulo Soares, et al.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License version 3
 * as published by the Free Software Foundation with the addition of the
 * following permission added to Section 15 as permitted in Section 7(a):
 * FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY 1T3XT,
 * 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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
        
        /** A map with all supported operators operators (PDF syntax). */
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
        public IXObjectDoHandler RegisterXObjectDoHandler(PdfName xobjectSubType, IXObjectDoHandler handler){
            IXObjectDoHandler old;
            xobjectDoHandlers.TryGetValue(xobjectSubType, out old);
            xobjectDoHandlers[xobjectSubType] = handler;
            return old;
        }
        
        /**
         * Loads all the supported graphics and text state operators in a map.
         */
        private void PopulateOperators(){

            RegisterContentOperator(DEFAULTOPERATOR, new IgnoreOperatorContentOperator());
            
            RegisterContentOperator("q", new PushGraphicsState());
            RegisterContentOperator("Q", new PopGraphicsState());
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
            RegisterContentOperator("Tj", new ShowText());
            MoveNextLineAndShowText tickOperator = new MoveNextLineAndShowText(tstarOperator, tjOperator);
            RegisterContentOperator("'", tickOperator);
            RegisterContentOperator("\"", new MoveNextLineAndShowTextWithSpacing(twOperator, tcOperator, tickOperator));
            RegisterContentOperator("TJ", new ShowTextArray());

            RegisterContentOperator("Do", new Do());
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
        public IContentOperator RegisterContentOperator(String operatorString, IContentOperator oper){
            return operators[operatorString] = oper;
        }

        /**
         * Resets the graphics state stack, matrices and resources.
         */
        public void Reset(){
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
        private GraphicsState Gs(){
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
         * Decodes a PdfString (which will contain glyph ids encoded in the font's encoding)
         * based on the active font, and determine the unicode equivalent
         * @param in    the String that needs to be encoded
         * @return  the encoded String
         * @since 2.1.7
         */
        private String Decode(PdfString inp){
            byte[] bytes = inp.GetBytes();
            return Gs().font.Decode(bytes, 0, bytes.Length);
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
        private void DisplayPdfString(PdfString str){

            String unicode = Decode(str);

            TextRenderInfo renderInfo = new TextRenderInfo(unicode, Gs(), textMatrix, markedContentStack);

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
         * Adjusts the text matrix for the specified adjustment value (see TJ oper in the PDF spec for information)
         * @param tj the text adjustment
         */
        private void ApplyTextAdjust(float tj){
            float adjustBy = -tj/1000f * Gs().fontSize * Gs().horizontalScaling;

            textMatrix = new Matrix(adjustBy, 0).Multiply(textMatrix);
        }
        
        /**
         * Processes PDF syntax
         * @param contentBytes  the bytes of a content stream
         * @param resources     the resources that come with the content stream
         */
        public void ProcessContent(byte[] contentBytes, PdfDictionary resources){

            this.resources.Push(resources);
            PRTokeniser tokeniser = new PRTokeniser(contentBytes);
            PdfContentParser ps = new PdfContentParser(tokeniser);
            List<PdfObject> operands = new List<PdfObject>();
            while (ps.Parse(operands).Count > 0){
                PdfLiteral oper = (PdfLiteral)operands[operands.Count-1];
                if ("BI".Equals(oper.ToString())){
                    // we don't call invokeOperator for embedded images - this is one area of the PDF spec that is particularly nasty and inconsistent
                    ImageRenderInfo renderInfo = ImageRenderInfo.CreatedForEmbeddedImage(Gs().ctm, InlineImageUtils.ParseInlineImage(ps));
                    renderListener.RenderImage(renderInfo);
                } else {
                    InvokeOperator(oper, operands);
                }
            }

            this.resources.Pop();
        }
        
        /**
         * A resource dictionary that allows stack-like behavior to support resource dictionary inheritance
         */
        private class ResourceDictionary : PdfDictionary {
            private IList<PdfDictionary> resourcesStack = new List<PdfDictionary>();
            public ResourceDictionary() {
            }

            public void Push(PdfDictionary resources){
                resourcesStack.Add(resources);
            }

            public void Pop(){
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
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands){
                // ignore the oper
            }
        }
        
        /**
         * A content oper implementation (TJ).
         */
        private class ShowTextArray : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfArray array = (PdfArray)operands[0];
                float tj = 0;
                foreach (PdfObject entryObj in array.ArrayList) {
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

            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
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

            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                textMoveNextLine.Invoke(processor, null, new List<PdfObject>(0));
                showText.Invoke(processor, null, operands);
            }
        }

        /**
         * A content oper implementation (Tj).
         */
        private class ShowText : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
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

            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
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
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
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
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
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
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
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
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfName fontResourceName = (PdfName)operands[0];
                float size = ((PdfNumber)operands[1]).FloatValue;

                PdfDictionary fontsDictionary = processor.resources.GetAsDict(PdfName.FONT);
                CMapAwareDocumentFont font = new CMapAwareDocumentFont((PRIndirectReference)fontsDictionary.Get(fontResourceName));

                processor.Gs().font = font;
                processor.Gs().fontSize = size;

            }
        }

        /**
         * A content oper implementation (Tr).
         */
        private class SetTextRenderMode : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfNumber render = (PdfNumber)operands[0];
                processor.Gs().renderMode = render.IntValue;
            }
        }

        /**
         * A content oper implementation (Ts).
         */
        private class SetTextRise : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfNumber rise = (PdfNumber)operands[0];
                processor.Gs().rise = rise.FloatValue;
            }
        }

        /**
         * A content oper implementation (TL).
         */
        private class SetTextLeading : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfNumber leading = (PdfNumber)operands[0];
                processor.Gs().leading = leading.FloatValue;
            }
        }

        /**
         * A content oper implementation (Tz).
         */
        private class SetTextHorizontalScaling : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfNumber scale = (PdfNumber)operands[0];
                processor.Gs().horizontalScaling = scale.FloatValue/100f;
            }
        }

        /**
         * A content oper implementation (Tc).
         */
        private class SetTextCharacterSpacing : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfNumber charSpace = (PdfNumber)operands[0];
                processor.Gs().characterSpacing = charSpace.FloatValue;
            }
        }

        /**
         * A content oper implementation (Tw).
         */
        private class SetTextWordSpacing : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfNumber wordSpace = (PdfNumber)operands[0];
                processor.Gs().wordSpacing = wordSpace.FloatValue;
            }
        }

        /**
         * A content oper implementation (gs).
         */
        private class ProcessGraphicsStateResource : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {

                PdfName dictionaryName = (PdfName)operands[0];
                PdfDictionary extGState = processor.resources.GetAsDict(PdfName.EXTGSTATE);
                if (extGState == null)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("resources.do.not.contain.extgstate.entry.unable.to.process.oper.1", oper));
                PdfDictionary gsDic = extGState.GetAsDict(dictionaryName);
                if (gsDic == null)
                    throw new ArgumentException(MessageLocalization.GetComposedMessage("1.is.an.unknown.graphics.state.dictionary", dictionaryName));

                // at this point, all we care about is the FONT entry in the GS dictionary
                PdfArray fontParameter = gsDic.GetAsArray(PdfName.FONT);
                if (fontParameter != null){
                    CMapAwareDocumentFont font = new CMapAwareDocumentFont((PRIndirectReference)fontParameter[0]);
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
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                GraphicsState gs = processor.gsStack.Peek();
                GraphicsState copy = new GraphicsState(gs);
                processor.gsStack.Push(copy);
            }
        }

        /**
         * A content oper implementation (cm).
         */
        private class ModifyCurrentTransformationMatrix : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
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
         * A content oper implementation (Q).
         */
        private class PopGraphicsState : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.gsStack.Pop();
            }
        }

        /**
         * A content oper implementation (BT).
         */
        private class BeginTextC : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                processor.textMatrix = new Matrix();
                processor.textLineMatrix = processor.textMatrix;
                processor.BeginText();
            }
        }

        /**
         * A content oper implementation (ET).
         */
        private class EndTextC : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
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

            public void Invoke(PdfContentStreamProcessor processor,
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

            public void Invoke(PdfContentStreamProcessor processor,
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
         * A content oper implementation (BMC).
         * @since 5.0.2
         */
        private class EndMarkedContentC : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor,
                    PdfLiteral oper, List<PdfObject> operands)
                    {
                processor.EndMarkedContent();
            }
        }
        
        /**
         * A content oper implementation (Do).
         */
        private class Do : IContentOperator{
            public void Invoke(PdfContentStreamProcessor processor, PdfLiteral oper, List<PdfObject> operands) {
                PdfName xobjectName = (PdfName)operands[0];
                processor.DisplayXObject(xobjectName);
            }
        }
        
        /**
         * An XObject subtype handler for FORM
         */
        private class FormXObjectDoHandler : IXObjectDoHandler{

            public void HandleXObject(PdfContentStreamProcessor processor, PdfStream stream, PdfIndirectReference refi) {
                
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

            public void HandleXObject(PdfContentStreamProcessor processor, PdfStream xobjectStream, PdfIndirectReference refi) {
                ImageRenderInfo renderInfo = ImageRenderInfo.CreateForXObject(processor.Gs().ctm, refi);
                processor.renderListener.RenderImage(renderInfo);
            }
        }
        
        /**
         * An XObject subtype handler that does nothing
         */
        private class IgnoreXObjectDoHandler : IXObjectDoHandler{
            public void HandleXObject(PdfContentStreamProcessor processor, PdfStream xobjectStream, PdfIndirectReference refi) {
                // ignore XObject subtype
            }
        }    
    }
}