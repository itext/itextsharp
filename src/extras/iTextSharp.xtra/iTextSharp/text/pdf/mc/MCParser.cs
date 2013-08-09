/*
 * $Id: MCParser.java 5948 2013-08-08 13:52:26Z blowagie $
 *
 * This file is part of the iText (R) project.
 * Copyright (c) 1998-2013 1T3XT BVBA
 * Authors: Bruno Lowagie, et al.
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using iTextSharp.text.io;
using iTextSharp.text.log;

namespace iTextSharp.text.pdf.mc {

    public class MCParser {
        /** The Logger instance */
        protected static ILogger LOGGER = LoggerFactory.GetLogger(typeof(MCParser));

        /** Factory that will help us build a RandomAccessSource. */
        protected RandomAccessSourceFactory factory = new RandomAccessSourceFactory();

        /** A map with all supported operators operators (PDF syntax). */
        protected static IDictionary<String, PdfOperator> operators = null;

        /** Constant used for the default operator. */
        public static String DEFAULTOPERATOR = "DefaultOperator";

        /** A new line operator */
        public static PdfLiteral TSTAR = new PdfLiteral("T*");

        /** The Stream of this worker object. */
        protected static MemoryStream baos;

        /** The list with structure items. */
        protected StructureItems items;

        // properties of the page that is being processed

        /** The reference to the page dictionary */
        protected PdfIndirectReference pageref;

        /** the XObject dictionary of the page that is being processed. */
        protected PdfDictionary xobjects;

        /** the StructParents of the page that is being processed. */
        protected PdfNumber structParents;

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
         * Parses the content of a page, replacing appearances of annotations
         * with Form XObjects.
         * @param page a page dictionary
         * @throws IOException
         */

        public void Parse(PdfDictionary page, PdfIndirectReference pageref, bool finalPage) {
            this.pageref = pageref;
            baos = new MemoryStream();
            structParents = page.GetAsNumber(PdfName.STRUCTPARENTS);
            PdfDictionary resources = page.GetAsDict(PdfName.RESOURCES);
            xobjects = resources.GetAsDict(PdfName.XOBJECT);
            if (xobjects == null) {
                xobjects = new PdfDictionary();
                resources.Put(PdfName.XOBJECT, xobjects);
            }
            PRStream stream = (PRStream) page.GetAsStream(PdfName.CONTENTS);
            byte[] contentBytes = PdfReader.GetStreamBytes(stream);
            PRTokeniser tokeniser = new PRTokeniser(new RandomAccessFileOrArray(factory.CreateSource(contentBytes)));
            PdfContentParser ps = new PdfContentParser(tokeniser);
            List<PdfObject> operands = new List<PdfObject>();
            while (ps.Parse(operands).Count > 0) {
                PdfLiteral opr = (PdfLiteral) operands[operands.Count - 1];
                ProcessOperator(opr, operands);
            }
            if (finalPage) {
                LOGGER.Info(String.Format("There are %d items left for processing", items.Count));
                foreach (StructureItem item in items) {
                    ConvertToXObject(item);
                }
            }
            baos.Flush();
            baos.Close();
            stream.SetData(baos.ToArray());
        }

        /**
         * Processes an operator, for instance: write the operator and its operands to baos.
         * @param opr	the operator
         * @param operands	the operator's operands
         * @throws IOException
         */

        protected void ProcessOperator(PdfLiteral opr, IList<PdfObject> operands) {
            PdfOperator op;
            operators.TryGetValue(opr.ToString(), out op);
            if (op == null)
                op = operators[DEFAULTOPERATOR];
            op.Process(this, opr, operands);
        }

        /**
         * Informs the parser that we're inside or outside a text object.
         * Also sets a parameter indicating that BT needs to be written.
         * @param inText	true if we're inside.
         */

        protected void SetInText(bool inText) {
            if (inText) {
                text = new StringBuilder();
                btWrite = true;
            } else
                etExtra = false;
            this.inText = inText;
        }

        /**
         * When an MCID is encountered, the parser will check the list
         * structure items and turn an annotation into an XObject if
         * necessary.
         * @param mcid	the MCID that was encountered in the content stream
         * @throws IOException
         */

        protected void DealWithMcid(PdfNumber mcid) {
            if (mcid == null)
                return;
            LOGGER.Info(String.Format("Encountered MCID %s in content", mcid));
            StructureItem item = items[0];
            switch (item.Process(mcid.IntValue)) {
                case 0:
                    items.RemoveAt(0);
                    LOGGER.Info(String.Format("Discovered %s as an object reference", item.GetObj()));
                    ConvertToXObject(item);
                    DealWithMcid(mcid);
                    return;
                case 1:
                    LOGGER.Info("Removed structure item from stack.");
                    items.RemoveAt(0);
                    return;
                case 2:
                    LOGGER.Info("Removed MCID from structure item.");
                    return;

                default:
                    LOGGER.Warn("MCID not found!");
                    break;
            }
        }

        /**
         * Converts an annotation structure item to a Form XObject annotation.
         * @param item the structure item
         * @throws IOException
         */

        protected void ConvertToXObject(StructureItem item) {
            PdfDictionary structElem = item.GetStructElem();
            if (structElem == null)
                return;
            PdfDictionary dict = item.GetObj();
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
            PdfDictionary attribute = new PdfDictionary();
            attribute.Put(PdfName.O, PdfName.PRINTFIELD);
            PdfString description = dict.GetAsString(PdfName.TU);
            if (description == null)
                description = dict.GetAsString(PdfName.T);
            // TODO: what if the field is a button?
            if (!PdfName.BTN.Equals(dict.Get(PdfName.FT)))
                attribute.Put(PdfName.ROLE, PdfName.TV);
            attribute.Put(PdfName.DESC, description);
            PdfString t = structElem.GetAsString(PdfName.T);
            if (t == null || t.ToString().Trim().Length == 0)
                structElem.Put(PdfName.T, dict.GetAsString(PdfName.T));
            structElem.Put(PdfName.A, attribute);
            structElem.Put(PdfName.S, PdfName.P);
            structElem.Put(PdfName.PG, pageref);
            int mcid = items.ProcessMCID(structParents, item);
            LOGGER.Info("Using MCID " + mcid);
            structElem.Put(PdfName.K, new PdfNumber(mcid));
            item.GetObjr().Put(PdfName.OBJ, xobjr);
            items.RemoveFromParentTree(structParent);
            PdfName xobj = new PdfName("XObj" + structParent.IntValue);
            LOGGER.Info("Creating XObject with name " + xobj);
            xobjects.Put(xobj, xobjr);
            PdfArray array = dict.GetAsArray(PdfName.RECT);
            Rectangle rect = new Rectangle(
                array.GetAsNumber(0).FloatValue, array.GetAsNumber(1).FloatValue,
                array.GetAsNumber(2).FloatValue, array.GetAsNumber(3).FloatValue);
            rect.Normalize();
            if (inText && !btWrite) {
                LOGGER.Debug("Introducing extra ET");
                byte[] bytes = Encoding.ASCII.GetBytes("ET\n");
                baos.Write(bytes, 0, bytes.Length);
                etExtra = true;
            }
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
            if (inText)
                btWrite = true;
        }

        /**
         * Adds an operator and its operands (if any) to baos.
         * @param opr	the operator
         * @param operands	its operands
         * @throws IOException
         */

        protected void PrintOperator(PdfLiteral opr, IList<PdfObject> operands) {
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

        protected void PrintTextOperator(PdfLiteral opr, IList<PdfObject> operands) {
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

        protected void Printsp(PdfObject o) {
            CheckBT();
            o.ToPdf(null, baos);
            baos.WriteByte((byte) ' ');
        }

        /**
         * Writes a PDF object to the Stream, followed by a newline character.
         * @param o a PdfObject
         * @throws IOException
         */

        protected void Println(PdfObject o) {
            CheckBT();
            o.ToPdf(null, baos);
            baos.WriteByte((byte) '\n');
        }

        /**
         * Checks if a BT operator is waiting to be added.
         */

        protected void CheckBT() {
            if (btWrite) {
                LOGGER.Debug("BT written");
                byte[] bytes = Encoding.ASCII.GetBytes("BT ");
                baos.Write(bytes, 0, bytes.Length);
                if (etExtra) {
                    bytes = Encoding.ASCII.GetBytes(text.ToString());
                    baos.Write(bytes, 0, bytes.Length);
                    etExtra = false;
                    text = new StringBuilder();
                }
            }
            btWrite = false;
        }

        /**
         * Populates the operators variable.
         */

        protected void PopulateOperators() {
            if (operators != null)
                return;
            operators = new Dictionary<String, PdfOperator>();
            operators[DEFAULTOPERATOR] = new CopyContentOperator();
            PdfOperator markedContent = new BeginMarkedContentDictionaryOperator();
            operators["BDC"] = markedContent;
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
             * @see com.itextpdf.text.pdf.ocg.OCGParser.PdfOperator#process(com.itextpdf.text.pdf.ocg.OCGParser, com.itextpdf.text.pdf.PdfLiteral, java.util.List)
             */

            public void Process(MCParser parser,
                                PdfLiteral opr, IList<PdfObject> operands) {
                parser.PrintOperator(opr, operands);
            }
        }

        /**
         * Class that knows how to process marked content operators.
         */

        private class BeginMarkedContentDictionaryOperator : PdfOperator {

            /**
             * @see com.itextpdf.text.pdf.ocg.OCGParser.PdfOperator#process(com.itextpdf.text.pdf.ocg.OCGParser, com.itextpdf.text.pdf.PdfLiteral, java.util.List)
             */

            public void Process(MCParser parser, PdfLiteral opr,
                                IList<PdfObject> operands) {
                if ("BDC".Equals(opr.ToString())) {
                    if (operands[1].IsDictionary()) {
                        PdfDictionary dict = (PdfDictionary) operands[1];
                        parser.DealWithMcid(dict.GetAsNumber(PdfName.MCID));
                    }
                }
                parser.PrintOperator(opr, operands);
            }
        }

        /**
         * Class that knows how to process the BT operator.
         */

        private class BeginTextOperator : PdfOperator {

            /**
             * @see com.itextpdf.text.pdf.mc.MCParser.PdfOperator#process(com.itextpdf.text.pdf.mc.MCParser, com.itextpdf.text.pdf.PdfLiteral, java.util.List)
             */

            public void Process(MCParser parser, PdfLiteral opr,
                                IList<PdfObject> operands) {
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

            public void Process(MCParser parser, PdfLiteral opr,
                                IList<PdfObject> operands) {
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

            public void Process(MCParser parser, PdfLiteral opr,
                                IList<PdfObject> operands) {
                parser.PrintTextOperator(opr, operands);
            }

        }

        /**
         * Class that knows how to the ET operators.
         */

        private class TextStateOperator : PdfOperator {

            /**
             * @see com.itextpdf.text.pdf.mc.MCParser.PdfOperator#process(com.itextpdf.text.pdf.mc.MCParser, com.itextpdf.text.pdf.PdfLiteral, java.util.List)
             */

            public void Process(MCParser parser, PdfLiteral opr,
                                IList<PdfObject> operands) {
                parser.PrintTextOperator(opr, operands);
            }

        }

        /**
         * Class that knows how to the ET operators.
         */

        private class TextNewLineOperator : PdfOperator {

            /**
             * @see com.itextpdf.text.pdf.mc.MCParser.PdfOperator#process(com.itextpdf.text.pdf.mc.MCParser, com.itextpdf.text.pdf.PdfLiteral, java.util.List)
             */

            public void Process(MCParser parser, PdfLiteral opr,
                                IList<PdfObject> operands) {
                IList<PdfObject> list = new List<PdfObject>();
                list.Add(TSTAR);
                parser.PrintTextOperator(MCParser.TSTAR, list);
            }

        }
    }
}