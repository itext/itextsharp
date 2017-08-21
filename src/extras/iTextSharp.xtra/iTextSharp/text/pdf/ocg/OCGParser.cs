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
using System.IO;
using System.util.collections;
using iTextSharp.text.pdf;

namespace iTextSharp.text.pdf.ocg {
    /// <summary>
    /// A helper class for OCGRemover.
    /// </summary>
    public class OCGParser {
        /// <summary>
        /// Constant used for the default operator. </summary>
        public const string DEFAULTOPERATOR = "DefaultOperator";

        /// <summary>
        /// A map with all supported operators operators (PDF syntax). </summary>
        protected internal static readonly IDictionary<string, PdfOperator> operators;

        /// <summary>
        /// The OutputStream of this worker object. </summary>
        protected internal MemoryStream baos;

        /// <summary>
        /// Keeps track of BMC/EMC balance. </summary>
        protected internal int mc_balance;

        /// <summary>
        /// The OCGs that need to be removed. </summary>
        protected internal ICollection<string> ocgs;

        /// <summary>
        /// The OCG properties. </summary>
        protected internal PdfDictionary properties;

        /// <summary>
        /// The names of XObjects that shouldn't be shown. </summary>
        protected internal ICollection<PdfName> xobj;

        static OCGParser() {
            operators = new Dictionary<string, PdfOperator>();
            PopulateOperators();
        }

        /// <summary>
        /// Creates an instance of the OCGParser. </summary>
        /// <param name="ocgs">	a set of String values with the names of the OCGs that need to be removed. </param>
        public OCGParser(ICollection<string> ocgs) {
            this.ocgs = ocgs;
        }

        /// <summary>
        /// Checks if the parser is currently parsing content that needs to be ignored.
        /// @return	true if the content needs to be ignored
        /// </summary>
        protected internal virtual bool ToRemoved {
            get {
                if (mc_balance > 0) {
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Parses a stream object and removes OCGs. </summary>
        /// <param name="stream">	a stream object </param>
        /// <param name="resources">	the resources dictionary of that object (containing info about the OCGs) </param>
        public virtual void Parse(PRStream stream, PdfDictionary resources) {
            baos = new MemoryStream();
            properties = resources.GetAsDict(PdfName.PROPERTIES);
            xobj = new HashSet2<PdfName>();
            PdfDictionary xobjects = resources.GetAsDict(PdfName.XOBJECT);
            if (xobjects != null) {
                // remove XObject (form or image) that belong to an OCG that needs to be removed
                foreach (PdfName name in xobjects.Keys) {
                    PRStream xobject = (PRStream) xobjects.GetAsStream(name);
                    PdfDictionary oc = xobject.GetAsDict(PdfName.OC);
                    if (oc != null) {
                        PdfString ocname = oc.GetAsString(PdfName.NAME);
                        if (ocname != null && ocgs.Contains(ocname.ToString())) {
                            xobj.Add(name);
                        }
                    }
                }
                foreach (PdfName name in xobj) {
                    xobjects.Remove(name);
                }
            }
            // parse the content stream
            byte[] contentBytes = PdfReader.GetStreamBytes(stream);
            PRTokeniser tokeniser = new PRTokeniser(new RandomAccessFileOrArray(contentBytes));
            PdfContentParser ps = new PdfContentParser(tokeniser);
            List<PdfObject> operands = new List<PdfObject>();
            while (ps.Parse(operands).Count > 0) {
                PdfLiteral @operator = (PdfLiteral) operands[operands.Count - 1];
                ProcessOperator(this, @operator, operands);
                if ("BI".Equals(@operator.ToString()))
                {
                    int found = 0;
                    int ch;
                    bool immediateAfterBI = true;
                    while ((ch = tokeniser.Read()) != -1)
                    {
                        if (!immediateAfterBI || !PRTokeniser.IsWhitespace(ch))
                        {
                            baos.WriteByte((byte) ch);
                        }
                        immediateAfterBI = false;
                        if (found == 0 && PRTokeniser.IsWhitespace(ch))
                        {
                            found++;
                        }
                        else if (found == 1 && ch == 'E')
                        {
                            found++;
                        }
                        else if (found == 1 && PRTokeniser.IsWhitespace(ch))
                        {
                            // this clause is needed if we have a white space character that is part of the image data
                            // followed by a whitespace character that precedes the EI operator.  In this case, we need
                            // to flush the first whitespace, then treat the current whitespace as the first potential
                            // character for the end of stream check. Note that we don't increment 'found' here.
                        }
                        else if (found == 2 && ch == 'I')
                        {
                            found++;
                        }
                        else if (found == 3 && PRTokeniser.IsWhitespace(ch))
                        {
                            break;
                        }
                        else
                        {
                            found = 0;
                        }
                    }
                }
            }
            baos.Flush();
            baos.Close();
            stream.SetData(baos.GetBuffer());
        }

        /// <summary>
        /// Processes an operator. </summary>
        /// <param name="parser">	the parser that needs to process the operator </param>
        /// <param name="operator">	the operator </param>
        /// <param name="operands">	its operands </param>
        /// <exception cref="Exception"> </exception>
        protected internal static void ProcessOperator(OCGParser parser, PdfLiteral @operator, IList<PdfObject> operands) {
            PdfOperator op;
            if (!operators.TryGetValue(@operator.ToString(), out op) || op == null)
            {
                op = operators[DEFAULTOPERATOR];
            }
            op.Process(parser, @operator, operands);
        }

        /// <summary>
        /// Populates the operators variable.
        /// </summary>
        protected internal static void PopulateOperators() {
            operators[DEFAULTOPERATOR] = new CopyContentOperator();
            PathConstructionOrPaintingOperator opConstructionPainting = new PathConstructionOrPaintingOperator();
            operators["m"] = opConstructionPainting;
            operators["l"] = opConstructionPainting;
            operators["c"] = opConstructionPainting;
            operators["v"] = opConstructionPainting;
            operators["y"] = opConstructionPainting;
            operators["h"] = opConstructionPainting;
            operators["re"] = opConstructionPainting;
            operators["S"] = opConstructionPainting;
            operators["s"] = opConstructionPainting;
            operators["f"] = opConstructionPainting;
            operators["F"] = opConstructionPainting;
            operators["f*"] = opConstructionPainting;
            operators["B"] = opConstructionPainting;
            operators["B*"] = opConstructionPainting;
            operators["b"] = opConstructionPainting;
            operators["b*"] = opConstructionPainting;
            operators["n"] = opConstructionPainting;
            operators["W"] = opConstructionPainting;
            operators["W*"] = opConstructionPainting;
            GraphicsOperator graphics = new GraphicsOperator();
            operators["q"] = graphics;
            operators["Q"] = graphics;
            operators["w"] = graphics;
            operators["J"] = graphics;
            operators["j"] = graphics;
            operators["M"] = graphics;
            operators["d"] = graphics;
            operators["ri"] = graphics;
            operators["i"] = graphics;
            operators["gs"] = graphics;
            operators["cm"] = graphics;
            operators["g"] = graphics;
            operators["G"] = graphics;
            operators["rg"] = graphics;
            operators["RG"] = graphics;
            operators["k"] = graphics;
            operators["K"] = graphics;
            operators["cs"] = graphics;
            operators["CS"] = graphics;
            operators["sc"] = graphics;
            operators["SC"] = graphics;
            operators["scn"] = graphics;
            operators["SCN"] = graphics;
            operators["sh"] = graphics;
            XObjectOperator xObject = new XObjectOperator();
            operators["Do"] = xObject;
            InlineImageOperator inlineImage = new InlineImageOperator();
            operators["BI"] = inlineImage;
            operators["EI"] = inlineImage;
            TextOperator text = new TextOperator();
            operators["BT"] = text;
            operators["ID"] = text;
            operators["ET"] = text;
            operators["Tc"] = text;
            operators["Tw"] = text;
            operators["Tz"] = text;
            operators["TL"] = text;
            operators["Tf"] = text;
            operators["Tr"] = text;
            operators["Ts"] = text;
            operators["Td"] = text;
            operators["TD"] = text;
            operators["Tm"] = text;
            operators["T*"] = text;
            operators["Tj"] = text;
            operators["'"] = text;
            operators["\""] = text;
            operators["TJ"] = text;
            MarkedContentOperator markedContent = new MarkedContentOperator();
            operators["BMC"] = markedContent;
            operators["BDC"] = markedContent;
            operators["EMC"] = markedContent;
        }

        /// <summary>
        /// Checks operands to find out if the corresponding operator needs to be present or not. </summary>
        /// <param name="operands">	a list of operands
        /// @return	true if the operators needs to be present. </param>
        protected internal virtual bool IsVisible(IList<PdfObject> operands) {
            if (operands.Count > 1 && xobj.Contains((PdfName) operands[0])) {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Keeps track of the MarkedContent state. </summary>
        /// <param name="ocref">	a reference to an OCG dictionary </param>
        protected internal virtual void CheckMarkedContentStart(PdfName ocref) {
            if (mc_balance > 0) {
                mc_balance++;
                return;
            }
            if (properties == null)
                return;
            PdfDictionary ocdict = properties.GetAsDict(ocref);
            if (ocdict == null) {
                return;
            }
            PdfString ocname = ocdict.GetAsString(PdfName.NAME);
            if (ocname == null) {
                return;
            }
            if (ocgs.Contains(ocname.ToString())) {
                mc_balance++;
            }
        }

        /// <summary>
        /// Keeps track of the MarkedContent state.
        /// </summary>
        protected internal virtual void CheckMarkedContentEnd() {
            if (mc_balance > 0) {
                mc_balance--;
            }
        }

        /// <summary>
        /// Processes an operator </summary>
        /// <param name="operator">	the operator </param>
        /// <param name="operands">	its operands </param>
        /// <param name="removable">	is the operator eligable for removal? </param>
        /// <exception cref="IOException"> </exception>
        protected internal virtual void Process(PdfLiteral @operator, IList<PdfObject> operands, bool removable) {
            if (removable && ToRemoved) {
                return;
            }
            operands.Remove(@operator);
            foreach (PdfObject o in operands) {
                PrintSp(o);
            }
            PrintLn(@operator);
        }

        /// <summary>
        /// Writes a PDF object to the OutputStream, followed by a space character. </summary>
        /// <param name="o"> </param>
        /// <exception cref="IOException"> </exception>
        protected internal virtual void PrintSp(PdfObject o) {
            o.ToPdf(null, baos);
            baos.WriteByte((byte) ' ');
        }

        /// <summary>
        /// Writes a PDF object to the OutputStream, followed by a newline character. </summary>
        /// <param name="o"> </param>
        /// <exception cref="IOException"> </exception>
        protected internal virtual void PrintLn(PdfObject o) {
            o.ToPdf(null, baos);
            baos.WriteByte((byte) '\n');
        }

        #region Nested type: CopyContentOperator

        /// <summary>
        /// Class that processes unknown content.
        /// </summary>
        private class CopyContentOperator : PdfOperator {
            #region PdfOperator Members

            /// <seealso cref= "PdfOperator.Process(OCGParser, PdfLiteral, List{T})"> </seealso>
            virtual public void Process(OCGParser parser, PdfLiteral @operator, IList<PdfObject> operands) {
                parser.Process(@operator, operands, true);
            }

            #endregion
        }

        #endregion

        #region Nested type: GraphicsOperator

        /// <summary>
        /// Class that knows how to process graphics state operators.
        /// </summary>
        private class GraphicsOperator : PdfOperator {
            #region PdfOperator Members

            /// <seealso cref= "PdfOperator.Process(OCGParser, PdfLiteral, List{T})"> </seealso>
            virtual public void Process(OCGParser parser, PdfLiteral @operator, IList<PdfObject> operands) {
                parser.Process(@operator, operands, false);
            }

            #endregion
        }

        #endregion

        #region Nested type: InlineImageOperator

        /// <summary>
        /// Class that knows how to process inline image operators.
        /// </summary>
        private class InlineImageOperator : PdfOperator {
            #region PdfOperator Members

            /// <seealso cref= "PdfOperator.Process(OCGParser, PdfLiteral, List{T})"> </seealso>
            virtual public void Process(OCGParser parser, PdfLiteral @operator, IList<PdfObject> operands) {
                parser.Process(@operator, operands, true);
            }

            #endregion
        }

        #endregion

        #region Nested type: MarkedContentOperator

        /// <summary>
        /// Class that knows how to process marked content operators.
        /// </summary>
        private class MarkedContentOperator : PdfOperator {
            #region PdfOperator Members

            /// <seealso cref= "PdfOperator.Process(OCGParser, PdfLiteral, List{T})"> </seealso>
            virtual public void Process(OCGParser parser, PdfLiteral @operator, IList<PdfObject> operands) {
                if ("BDC".Equals(@operator.ToString())) {
                    PdfName operand = null;
                    if (operands.Count > 1 && PdfName.OC.Equals(operands[0])) {
                        operand = (PdfName)operands[1];
                    }
                    parser.CheckMarkedContentStart(operand);
                }else if ("BMC".Equals(@operator.ToString())) {
                    parser.CheckMarkedContentStart(null);
                }
                parser.Process(@operator, operands, true);
                if ("EMC".Equals(@operator.ToString())) {
                    parser.CheckMarkedContentEnd();
                }
            }

            #endregion
        }

        #endregion

        #region Nested type: PathConstructionOrPaintingOperator

        /// <summary>
        /// Class that knows how to process path construction, path painting and path clipping operators.
        /// </summary>
        private class PathConstructionOrPaintingOperator : PdfOperator {
            #region PdfOperator Members

            /// <seealso cref= "PdfOperator.Process(OCGParser, PdfLiteral, List{T})"> </seealso>
            virtual public void Process(OCGParser parser, PdfLiteral @operator, IList<PdfObject> operands) {
                parser.Process(@operator, operands, true);
            }

            #endregion
        }

        #endregion

        #region Nested type: PdfOperator

        /// <summary>
        /// PDF Operator interface.
        /// </summary>
        public interface PdfOperator {
            /// <summary>
            /// Methods that processes an operator </summary>
            /// <param name="parser">	the parser </param>
            /// <param name="operator">	the operator </param>
            /// <param name="operands">	its operands </param>
            /// <exception cref="IOException"> </exception>
            void Process(OCGParser parser, PdfLiteral @operator, IList<PdfObject> operands);
        }

        #endregion

        #region Nested type: TextOperator

        /// <summary>
        /// Class that knows how to process text state operators.
        /// </summary>
        private class TextOperator : PdfOperator {
            #region PdfOperator Members

            /// <seealso cref= "PdfOperator.Process(OCGParser, PdfLiteral, List{T})"> </seealso>
            virtual public void Process(OCGParser parser, PdfLiteral @operator, IList<PdfObject> operands) {
                parser.Process(@operator, operands, true);
            }

            #endregion
        }

        #endregion

        #region Nested type: XObjectOperator

        /// <summary>
        /// Class that knows how to process XObject operators.
        /// </summary>
        private class XObjectOperator : PdfOperator {
            #region PdfOperator Members

            /// <seealso cref= "PdfOperator.Process(OCGParser, PdfLiteral, List{T})"> </seealso>
            virtual public void Process(OCGParser parser, PdfLiteral @operator, IList<PdfObject> operands) {
                if (parser.IsVisible(operands)) {
                    parser.Process(@operator, operands, true);
                }
            }

            #endregion
        }

        #endregion
    }
}
