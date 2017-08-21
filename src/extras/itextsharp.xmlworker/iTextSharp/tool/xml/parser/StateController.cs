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
using System.Runtime.Remoting.Messaging;
using iTextSharp.tool.xml.parser.state;
namespace iTextSharp.tool.xml.parser {

    /**
     * Switches the current state in the {@link XMLParser}.
     * @author redlab_b
     *
     */
    public class StateController {

        private IState unknown;
        private IState tagEncountered;
        private IState tagAttributes;
        private IState inTag;
        private IState attrValue;
        private IState singleQuoted;
        private IState doubleQuoted;
        private IState selfClosing;
        private IState specialChar;
        private IState closingTag;
        private IState comment;
        private IState closeComment;
        private IState cdata;
        private IState xml;
        private IState doctype;
        private IState unquoted;
        private IState processingInstruction;
        private XMLParser parser;
        private IState currentState;
        private IState previousState;
        private IState starComment;
        private IState closeStarComment;

        /**
         * Constructs a StateController with the given parser.
         * @param parser the Parser
         * @param isHTML true if this parser is going to parse HTML, this results in different whitespace handling.
         */
        public StateController(XMLParser parser, bool isHTML) {
            this.parser = parser;
            unknown = new UnknownState(parser);
            tagEncountered = new TagEncounteredState(parser);
            tagAttributes = new TagAttributeState(parser);
            inTag = (isHTML)?(IState)new InsideTagHTMLState(parser):(IState)new InsideTagState(parser);
            attrValue = new AttributeValueState(parser);
            singleQuoted = new SingleQuotedAttrValueState(parser);
            doubleQuoted = new DoubleQuotedAttrValueState(parser);
            selfClosing = new SelfClosingTagState(parser);
            specialChar = new SpecialCharState(parser);
            closingTag = new ClosingTagState(parser);
            comment = new CommentState(parser);
            closeComment = new CloseCommentState(parser);
            cdata = new CdataState(parser);
            xml = new XmlState(parser);
            doctype = new DocTypeState(parser);
            unquoted = new UnquotedAttrState(parser);
            processingInstruction = new ProcessingInstructionEncounteredState(parser);
            previousState = null;
            currentState = null;
            starComment = new StarCommentState(parser);
            closeStarComment = new CloseStarCommentState(parser);
        }
        /**
         *  Changes the state.
         * @param state the state to set as current state.
         * @return the Parser
         */
        virtual public XMLParser SetState(IState state) {
            previousState = currentState;
            currentState = state;
            parser.SetState(state);
            return parser;
        }

        /**
         * Changes the state to the previous one.
         * @return Parser
         */
        virtual public XMLParser PreviousState() {
            parser.SetState(previousState);
            IState temp = currentState;
            currentState = previousState;
            previousState = temp;
            return parser;
        }

        /**
         * set Parser state to {@link UnknownState}.
         * @return Parser
         */
        virtual public XMLParser Unknown() {
            return SetState(unknown);
        }

        /**
         * set Parser state to {@link TagEncounteredState}.
         * @return Parser
         */
        virtual public XMLParser TagEncountered() {
            return SetState(tagEncountered);
        }

        /**
         * set Parser state to {@link TagAttributeState}.
         * @return Parser
         */
        virtual public XMLParser TagAttributes() {
            return SetState(tagAttributes);
        }

        /**
         * set Parser state to {@link InsideTagState}.
         * @return Parser
         */
        virtual public XMLParser InTag() {
            return SetState(inTag);
        }

        /**
         * set Parser state to {@link AttributeValueState}.
         * @return Parser
         */
        virtual public XMLParser AttributeValue() {
            return SetState(attrValue);
        }

        /**
         * set Parser state to {@link SingleQuotedAttrValueState}.
         * @return Parser
         */
        virtual public XMLParser SingleQuotedAttr() {
            return SetState(singleQuoted);
        }

        /**
         * set Parser state to {@link DoubleQuotedAttrValueState}.
         * @return Parser
         */
        virtual public XMLParser DoubleQuotedAttr() {
            return SetState(doubleQuoted);
        }

        /**
	     * set Parser state to {@link ProcessingInstructionEncounteredState}.
	     * @return Parser
	     */
        virtual public XMLParser ProcessingInstructions() {
            return SetState(processingInstruction);
        }

        /**
         * set Parser state to {@link SelfClosingTagState}.
         * @return Parser
         */
        virtual public XMLParser SelfClosing() {
            return SetState(selfClosing);
        }

        /**
         *set Parser state to {@link SpecialCharState}.
         * @return Parser
         */
        virtual public XMLParser SpecialChar() {
            return SetState(this.specialChar);
        }

        /**
         * set Parser state to {@link ClosingTagState}.
         * @return Parser
         */
        virtual public XMLParser ClosingTag() {
            return SetState(this.closingTag);
        }

        /**
         * set Parser state to {@link CommentState}.
         * @return Parser
         */
        virtual public XMLParser Comment() {
            return SetState(this.comment);
        }

        /**
         * set Parser state to {@link CloseCommentState}.
         * @return Parser
         */
        virtual public XMLParser CloseComment() {
            return SetState(closeComment);
        }

        /**
         * set Parser state to {@link CdataState}.
         * @return Parser
         */
        virtual public XMLParser Cdata() {
            return SetState(cdata);
        }

        /**
         * set Parser state to {@link XmlState}.
         * @return Parser
         */
        virtual public XMLParser Xml() {
            return SetState(xml);
        }

        /**
         * set Parser state to {@link DocTypeState}.
         * @return Parser
         */
        virtual public XMLParser Doctype() {
            return SetState(doctype);
        }
        /**
         * set Parser state to {@link UnquotedAttrState}.
         * @return Parser
         *
         */
        virtual public XMLParser UnquotedAttr() {
            return SetState(unquoted);

        }

        /**
	     * set Parser state to {@link StarCommentState}.
	     * @return Parser
	     */
        virtual public XMLParser StarComment()
        {
            return SetState(this.starComment);
        }

        /**
         * set Parser state to {@link CloseStarCommentState}.
         * @return Parser
         */
        virtual public XMLParser CloseStarComment()
        {
            return SetState(this.closeStarComment);
        }

        virtual public IState GetPreviousState() {
            return previousState;
        }
    }
}
