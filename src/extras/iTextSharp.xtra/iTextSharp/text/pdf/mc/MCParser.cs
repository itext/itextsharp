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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using iTextSharp.text.error_messages;
using iTextSharp.text.io;
using iTextSharp.text.log;

namespace iTextSharp.text.pdf.mc {

    /**
     * This class will parse page content streams and add Do operators
     * in a marked-content sequence for every field that needs to be
     * flattened.
     */
    public class MCParser {
        /** The Logger instance */
        protected static ILogger LOGGER = LoggerFactory.GetLogger(typeof(MCParser));

        /** Factory that will help us build a RandomAccessSource. */
        protected static readonly RandomAccessSourceFactory RASFACTORY = new RandomAccessSourceFactory();

        /** Constant used for the default operator. */
        public static String DEFAULTOPERATOR = "DefaultOperator";

        /** A new line operator */
        public static PdfLiteral TSTAR = new PdfLiteral("T*");

        // variables needed when parsing

        /** A map with all supported operators operators (PDF syntax). */
        protected Dictionary<String, PdfOperator> operators = null;

        /** The list with structure items. */
        protected StructureItems items;

        // properties of the page that is being processed

        /** The contents of the new content stream of the page. */
        protected MemoryStream baos;

        /** The page dictionary */
        protected PdfDictionary page;
    
        /** The reference to the page dictionary */
        protected PdfIndirectReference pageref;

        /** the annotations of the page that is being processed. */
        protected PdfArray annots;

        /** the StructParents of the page that is being processed. */
        protected PdfNumber structParents;

        /** the XObject dictionary of the page that is being processed. */
        protected PdfDictionary xobjects;

        // Keeping track of text state

        /** Did we postpone writing a BT operator? */
        protected bool btWrite = false;

        /** Did we postpone writing a BT operator? */
        protected bool etExtra = false;

        /** Are we inside a BT/ET sequence? */
        protected bool inText = false;

        /** A buffer containing text state. */
        protected StringBuilder text;

        /**
         * Creates an MCParser object.
         * @param items a list of StructureItem objects
         */
        public MCParser(StructureItems items) {
            PopulateOperators();
            this.items = items;
        }

        /**
         * Populates the operators variable.
         */
        virtual protected void PopulateOperators() {
            if(operators != null)
                return;
            operators = new Dictionary<string, PdfOperator>();
            operators[DEFAULTOPERATOR] = new CopyContentOperator();
            PdfOperator markedContent = new BeginMarkedContentDictionaryOperator();
            operators["BDC"] = markedContent;
            PdfOperator doOperator = new DoOperator();
            operators["Do"] = doOperator;
            PdfOperator beginText = new BeginTextOperator();
            operators["BT"] = beginText;
            PdfOperator endText = new EndTextOperator();
            operators["ET"] = endText;
            PdfOperator textPos = new TextPositioningOperator();
            operators["Td"] = textPos;
            operators["TD"] = textPos;
            operators["Tm"] = textPos;
            operators["T*"] = textPos;
            PdfOperator textState = new TextStateOperator();
            operators["Tc"] = textState;
            operators["Tw"] = textState;
            operators["Tz"] = textState;
            operators["TL"] = textState;
            operators["Tf"] = textState;
            operators["Tr"] = textState;
            operators["Ts"] = textState;
            PdfOperator textNL = new TextNewLineOperator();
            operators["'"] = textNL;
            operators["\""] = textNL;
        }
    


        /**
         * Parses the content of a page, replacing appearances of annotations
         * with Form XObjects.
         * @param page a page dictionary
         * @throws IOException
         */
        virtual public void Parse(PdfDictionary page, PdfIndirectReference pageref) {
            LOGGER.Info("Parsing page with reference " + pageref);
            // initializing member variables
            baos = new MemoryStream();
            this.page = page;
            this.pageref = pageref;
            
            structParents = page.GetAsNumber(PdfName.STRUCTPARENTS);
            if(structParents == null)
                throw new DocumentException(MessageLocalization.GetComposedMessage("can.t.read.document.structure"));
            annots = page.GetAsArray(PdfName.ANNOTS);
            if(annots == null)
                annots = new PdfArray();
            PdfDictionary resources = page.GetAsDict(PdfName.RESOURCES);
            xobjects = resources.GetAsDict(PdfName.XOBJECT);
            if (xobjects == null) {
                xobjects = new PdfDictionary();
                resources.Put(PdfName.XOBJECT, xobjects);
            }
            // parsing the content stream of the page
            PRStream stream = (PRStream) page.GetAsStream(PdfName.CONTENTS);
            byte[] contentBytes = PdfReader.GetStreamBytes(stream);
            PRTokeniser tokeniser = new PRTokeniser(new RandomAccessFileOrArray(RASFACTORY.CreateSource(contentBytes)));
            PdfContentParser ps = new PdfContentParser(tokeniser);
            List<PdfObject> operands = new List<PdfObject>();
            while (ps.Parse(operands).Count > 0) {
                PdfLiteral opr = (PdfLiteral) operands[operands.Count - 1];
                ProcessOperator(opr, operands);
            }
            // dealing with orphans
            while (items.Count > 0 && items[0].GetPageref() == pageref.Number) {
                StructureItem item = items[0];
                if (item is StructureObject) {
                    ConvertToXObject((StructureObject) item);
                    items.RemoveAt(0);
                }
            }
            if(annots.Length == 0) {
                page.Remove(PdfName.ANNOTS);
            }
            else {
                PdfDictionary annot;
                for(int i = 0; i < annots.Size; i++) {
                    annot = annots.GetAsDict(i);
                    if(annot.GetAsNumber(PdfName.STRUCTPARENT) == null)
                        throw new DocumentException(MessageLocalization.GetComposedMessage("could.not.flatten.file.untagged.annotations.found"));
                }
            }
            // replacing the content stream
            baos.Flush();
            baos.Close();
            stream.SetData(baos.ToArray());
            // showing how many items are left
            if (LOGGER.IsLogging(Level.INFO)) {
                LOGGER.Info(String.Format("There are {0} items left for processing", items.Count));
            }
        }

        /**
         * When an XObject with a StructParent is encountered,
         * we want to remove it from the stack.
         * @param xobj	the name of an XObject
         */
        virtual protected void DealWithXObj(PdfName xobj) {
            PdfDictionary dict = xobjects.GetAsStream(xobj);
            PdfNumber structParent = dict.GetAsNumber(PdfName.STRUCTPARENT);
            if (LOGGER.IsLogging(Level.INFO)) {
                LOGGER.Info(String.Format("Encountered StructParent {0} in content", structParent));
            }
            if(structParent == null)
                return;
            StructureItem item = items[0];
            if(item.CheckStructParent(pageref.Number, structParent.IntValue) == 1)
                items.RemoveAt(0);
        }


        /**
         * When an MCID is encountered, the parser will check the list
         * structure items and turn an annotation into an XObject if
         * necessary.
         * @param mcid	the MCID that was encountered in the content stream
         * @throws IOException
         */
        virtual protected void DealWithMcid(PdfNumber mcid)
        {
            if (mcid == null)
                return;
            StructureItem item = items[0];
            if (LOGGER.IsLogging(Level.INFO)) {
                LOGGER.Info(String.Format("Encountered MCID {0} in content, comparing with {1}", mcid, item));
            }
            switch (item.CheckMCID(pageref.Number, mcid.IntValue))
            {
                case 0:
                    StructureObject obj = (StructureObject) item;
                    ConvertToXObject(obj);
                    LOGGER.Info("Removed structure item from stack.");
                    items.RemoveAt(0);
                    DealWithMcid(mcid);
                    return;
                case 1:
                    LOGGER.Info("Removed structure item from stack.");
                    items.RemoveAt(0);
                    return;
                default:
                    LOGGER.Warn("MCID not found! There's probably an error in your form!");
                    // hack to deal with MCIDs that are added in the wrong order
                    int check;
                    for (int i = 1; i < items.Count; i++)
                    {
                        item = items[i];
                        check = item.CheckMCID(pageref.Number, mcid.IntValue);
                        switch (check)
                        {
                            case 1:
                                LOGGER.Info("Removed structure item from stack.");
                                items.RemoveAt(i);
                                return;
                            case 0:
                                break;
                        }
                    }
                    throw new DocumentException(MessageLocalization.GetComposedMessage("can.t.read.document.structure"));
            }
        }

        /**
         * Converts an annotation structure item to a Form XObject annotation.
         * @param item the structure item
         * @throws IOException
         */
        virtual protected void ConvertToXObject(StructureObject item) {
            PdfDictionary structElem = item.GetStructElem();
            if (structElem == null)
                return;
            PdfDictionary dict =  item.GetObjAsDict();
            if (dict == null || !dict.CheckType(PdfName.ANNOT))
                return;
            PdfDictionary ap = dict.GetAsDict(PdfName.AP);
            if (ap == null)
                return;
            PdfNumber structParent = dict.GetAsNumber(PdfName.STRUCTPARENT);
            if (structParent == null)
                return;
            PdfStream stream = ap.GetAsStream(PdfName.N);
            if (stream == null)
                return;
            stream.Put(PdfName.STRUCTPARENT, structParent);
            PdfIndirectReference xobjr = ap.GetAsIndirectObject(PdfName.N);
            if (xobjr == null)
                return;
            // remove the annotation from the page
    	    for (int i = 0; i < annots.Length; i++) {
    		    PdfIndirectReference annotref = annots.GetAsIndirectObject(i);
    		    if (item.GetObjRef().Number == annotref.Number) {
    			    annots.Remove(i);
    			    break;
    		    }
    	    }
    	    // replace the existing attributes by a PrintField attribute
            PdfDictionary attribute = new PdfDictionary();
            attribute.Put(PdfName.O, PdfName.PRINTFIELD);
            PdfString description = dict.GetAsString(PdfName.TU);
            if (description == null)
                description = dict.GetAsString(PdfName.T);
            if (PdfName.BTN.Equals(dict.Get(PdfName.FT))) {
                PdfNumber fflags = dict.GetAsNumber(PdfName.FF);
                if (fflags != null) {
                    int ff = fflags.IntValue;
                    if ((ff & PdfFormField.FF_PUSHBUTTON) != 0)
                        attribute.Put(PdfName.ROLE, PdfName.PB);
                    // I don't think the condition below will ever be true
                    if ((ff & PdfFormField.FF_RADIO) != 0)
                        attribute.Put(PdfName.ROLE, PdfName.rb);
                    else
                        attribute.Put(PdfName.ROLE, PdfName.CB);
                }
            }
            else {
                attribute.Put(PdfName.ROLE, PdfName.TV);
            }
            attribute.Put(PdfName.DESC, description);
            // Updating the values of the StructElem dictionary
            PdfString t = structElem.GetAsString(PdfName.T);
            if (t == null || t.ToString().Trim().Length == 0)
                structElem.Put(PdfName.T, dict.GetAsString(PdfName.T));
            structElem.Put(PdfName.A, attribute);
            structElem.Put(PdfName.S, PdfName.P);
            structElem.Put(PdfName.PG, pageref);
          	// Defining a new MCID
           	int mcid = items.ProcessMCID(structParents, item.GetRef());
            LOGGER.Info("Using MCID " + mcid);
            structElem.Put(PdfName.K, new PdfNumber(mcid));
            // removing the annotation from the parent tree
            items.RemoveFromParentTree(structParent);
            // Adding the XObject to the page
            PdfName xobj = new PdfName("XObj" + structParent.IntValue);
            LOGGER.Info("Creating XObject with name " + xobj);
            xobjects.Put(xobj, xobjr);
            PdfArray array = dict.GetAsArray(PdfName.RECT);
            // Getting the position of the annotation
            Rectangle rect = new Rectangle(
                array.GetAsNumber(0).FloatValue, array.GetAsNumber(1).FloatValue,
                array.GetAsNumber(2).FloatValue, array.GetAsNumber(3).FloatValue);
            rect.Normalize();
            // A Do operator is forbidden inside a text block
            if (inText && !btWrite) {
                LOGGER.Debug("Introducing extra ET");
                byte[] bytes = Encoding.ASCII.GetBytes("ET\n");
                baos.Write(bytes, 0, bytes.Length);
                etExtra = true;
            }
    	    // Writing the marked-content sequence with the Do operator
    	    // Note that the position assumes that the CTM wasn't changed in the graphics state
    	    // TODO: do the math if the CTM did change!
            ByteBuffer buf = new ByteBuffer();
            buf.Append("/P <</MCID ");
            buf.Append(mcid);
            buf.Append(">> BDC\n");
            buf.Append("q 1 0 0 1 ");
            buf.Append(rect.Left.ToString(CultureInfo.InvariantCulture));
            buf.Append(" ");
            buf.Append(rect.Bottom.ToString(CultureInfo.InvariantCulture));
            buf.Append(" cm ");
            buf.Append(xobj.GetBytes());
            buf.Append(" Do Q\n");
            buf.Append("EMC\n");
            buf.Flush();
            buf.WriteTo(baos);
    	    // if we were inside a text block, we've introduced an ET, so we'll need to write a BT
            if (inText)
                btWrite = true;
        }

        // Operator actions
    
        /**
         * Processes an operator, for instance: write the operator and its operands to baos.
         * @param operator	the operator
         * @param operands	the operator's operands
         * @throws IOException
         * @throws DocumentException 
         */
        virtual protected void ProcessOperator(PdfLiteral operatora, List<PdfObject> operands) {
            PdfOperator op;
            operators.TryGetValue(operatora.ToString(), out op);
            if (op == null)
                operators.TryGetValue(DEFAULTOPERATOR, out op);
            op.Process(this, operatora, operands);
        }

        /**
         * Adds an operator and its operands (if any) to baos, keeping track of the text state.
         * @param opr	the operator
         * @param operands	its operands
         * @throws IOException
         */
        virtual protected void PrintOperator(PdfLiteral opr, IList<PdfObject> operands) {
            operands.Remove(opr);
            foreach (PdfObject o in operands) {
                Printsp(o);
            }
            Println(opr);
        }

        /**
         * Adds an operator and its operands (if any) to baos.
         * @param opr	the operator
         * @param operands	its operands
         * @throws IOException
         */
        virtual protected void PrintTextOperator(PdfLiteral opr, IList<PdfObject> operands) {
            foreach (PdfObject obj in operands)
                text.Append(obj).Append(" ");
            text.Append("\n");
            PrintOperator(opr, operands);
        }

        /**
         * Writes a PDF object to the Stream, followed by a space character.
         * @param o a PdfObject
         * @throws IOException
         */
        virtual protected void Printsp(PdfObject o) {
            CheckBT();
            o.ToPdf(null, baos);
            baos.WriteByte((byte) ' ');
        }

        /**
         * Writes a PDF object to the Stream, followed by a newline character.
         * @param o a PdfObject
         * @throws IOException
         */
        virtual protected void Println(PdfObject o) {
            CheckBT();
            o.ToPdf(null, baos);
            baos.WriteByte((byte) '\n');
        }

        /**
         * Checks if a BT operator is waiting to be added.
         */
        virtual protected void CheckBT() {
            if (btWrite) {
                byte[] bytes = Encoding.ASCII.GetBytes("BT ");
                baos.Write(bytes, 0, bytes.Length);
                if (etExtra) {
                    bytes = Encoding.ASCII.GetBytes(text.ToString());
                    baos.Write(bytes, 0, bytes.Length);
                    etExtra = false;
                    text = new StringBuilder();
                }
                LOGGER.Debug("BT written");
            }
            btWrite = false;
        }

        /**
         * Informs the parser that we're inside or outside a text object.
         * Also sets a parameter indicating that BT needs to be written.
         * @param inText	true if we're inside.
         */
        virtual protected void SetInText(bool inText) {
            if(inText) {
                text = new StringBuilder();
                btWrite = true;
            }
            else {
                etExtra = false;
            }
            this.inText = inText;
        }

        // Operator interface and implementing classes
    

        /**
         * PDF Operator interface.
         */
        public interface PdfOperator {
            /**
             * Methods that processes an operator
             * @param parser	the parser
             * @param opr	the operator
             * @param operands	its operands
             * @throws IOException
             */
            void Process(MCParser parser, PdfLiteral opr, IList<PdfObject> operands);
        }


        /**
         * Class that processes content by just printing the operator and its operands.
         */
        private class CopyContentOperator : PdfOperator {

            /**
             * @see com.itextpdf.text.pdf.mc.MCParser.PdfOperator#process(com.itextpdf.text.pdf.ocg.OCGParser, com.itextpdf.text.pdf.PdfLiteral, java.util.List)
             */
            virtual public void Process(MCParser parser, PdfLiteral opr, IList<PdfObject> operands) {
                parser.PrintOperator(opr, operands);
            }
        }

        /**
         * Class that knows how to process marked content operators.
         */
        private class BeginMarkedContentDictionaryOperator : PdfOperator {

            /**
             * @see com.itextpdf.text.pdf.mc.MCParser.PdfOperator#process(com.itextpdf.text.pdf.ocg.OCGParser, com.itextpdf.text.pdf.PdfLiteral, java.util.List)
             */
            virtual public void Process(MCParser parser, PdfLiteral opr, IList<PdfObject> operands) {
                if(operands[1].IsDictionary()) {
                    PdfDictionary dict = (PdfDictionary)operands[1];
                    parser.DealWithMcid(dict.GetAsNumber(PdfName.MCID));
                }
                parser.PrintOperator(opr, operands);
            }
        }

        /**
         * Class that knows how to process Do operators.
         */
        private class DoOperator : PdfOperator {
		    /**
		     * @see com.itextpdf.text.pdf.mc.MCParser.PdfOperator#process(com.itextpdf.text.pdf.mc.MCParser, com.itextpdf.text.pdf.PdfLiteral, java.util.List)
		     */
		    virtual public void Process(MCParser parser, PdfLiteral operatora, IList<PdfObject> operands) {
			    if (operands[0].IsName())
				    parser.DealWithXObj((PdfName)operands[0]);
			    parser.PrintOperator(operatora, operands);
		    }
        }

        /**
         * Class that knows how to process the BT operator.
         */
        private class BeginTextOperator : PdfOperator {
            /**
             * @see com.itextpdf.text.pdf.mc.MCParser.PdfOperator#process(com.itextpdf.text.pdf.mc.MCParser, com.itextpdf.text.pdf.PdfLiteral, java.util.List)
             */
            virtual public void Process(MCParser parser, PdfLiteral opr, IList<PdfObject> operands) {
                LOGGER.Debug("BT: begin text on hold");
                parser.SetInText(true);
            }
        }

        /**
         * Class that knows how to the ET operators.
         */
        private class EndTextOperator : PdfOperator {
            /**
             * @see com.itextpdf.text.pdf.mc.MCParser.PdfOperator#process(com.itextpdf.text.pdf.mc.MCParser, com.itextpdf.text.pdf.PdfLiteral, java.util.List)
             */
            virtual public void Process(MCParser parser, PdfLiteral opr, IList<PdfObject> operands) {
                LOGGER.Debug("ET: end text block");
                parser.SetInText(false);
                parser.PrintOperator(opr, operands);
            }

        }

        /**
         * Class that knows how to the ET operators.
         */
        private class TextPositioningOperator : PdfOperator {
            /**
             * @see com.itextpdf.text.pdf.mc.MCParser.PdfOperator#process(com.itextpdf.text.pdf.mc.MCParser, com.itextpdf.text.pdf.PdfLiteral, java.util.List)
             */
            virtual public void Process(MCParser parser, PdfLiteral opr, IList<PdfObject> operands) {
                parser.PrintTextOperator(opr, operands);
            }
        }

        /**
         * Class that knows how to the text state operators.
         */
        private class TextStateOperator : PdfOperator {
            /**
             * @see com.itextpdf.text.pdf.mc.MCParser.PdfOperator#process(com.itextpdf.text.pdf.mc.MCParser, com.itextpdf.text.pdf.PdfLiteral, java.util.List)
             */
            virtual public void Process(MCParser parser, PdfLiteral opr, IList<PdfObject> operands) {
                parser.PrintTextOperator(opr, operands);
            }
        }

        /**
         * Class that knows how to the text state operators that result in a newline.
         */
        private class TextNewLineOperator : PdfOperator {

            /**
             * @see com.itextpdf.text.pdf.mc.MCParser.PdfOperator#process(com.itextpdf.text.pdf.mc.MCParser, com.itextpdf.text.pdf.PdfLiteral, java.util.List)
             */
            virtual public void Process(MCParser parser, PdfLiteral opr, IList<PdfObject> operands) {
                IList<PdfObject> list = new List<PdfObject>();
                list.Add(TSTAR);
                parser.PrintTextOperator(MCParser.TSTAR, list);
            }
        }
    }
}
