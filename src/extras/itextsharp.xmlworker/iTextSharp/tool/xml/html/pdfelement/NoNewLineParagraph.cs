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
using iTextSharp.text;
using iTextSharp.text.api;

namespace iTextSharp.tool.xml.html.pdfelement {

    /**
     * A <CODE>NoNewLineParagraph</CODE> is a series of <CODE>Chunk</CODE>s and/or <CODE>Phrases</CODE>.
     * <P>
     * A <CODE>NoNewLineParagraph</CODE> has the same qualities of a <CODE>Phrase</CODE>, but also
     * some additional layout-parameters:
     * <UL>
     * <LI>the indentation
     * <LI>the alignment of the text
     * </UL>
     *
     * Example:
     * <BLOCKQUOTE><PRE>
     * <STRONG>NoNewLineParagraph p = new NoNewLineParagraph("This is a paragraph",
     *               FontFactory.GetFont(FontFactory.HELVETICA, 18, Font.BOLDITALIC, new Color(0, 0, 255)));</STRONG>
     * </PRE></BLOCKQUOTE>
     *
     * @see     Element
     * @see     Phrase
     * @see     ListItem
     */

    public class NoNewLineParagraph : Phrase, IIndentable {

        // membervariables

        /** The alignment of the text. */
        protected internal int alignment = Element.ALIGN_UNDEFINED;

        /** The indentation of this paragraph on the left side. */
        protected internal float indentationLeft;

        /** The indentation of this paragraph on the right side. */
        protected internal float indentationRight;

        /** Holds value of property firstLineIndent. */
        private float firstLineIndent = 0;

        /** The spacing before the paragraph. */
        protected internal float spacingBefore;

        /** The spacing after the paragraph. */
        protected internal float spacingAfter;

        /** Holds value of property extraParagraphSpace. */
        private float extraParagraphSpace = 0;

        /** Does the paragraph has to be kept together on 1 page. */
        protected internal bool keeptogether = false;

        /**
         * Constructs a <CODE>Paragraph</CODE>.
         */
        public NoNewLineParagraph() : base() {
        }

        /**
         * Constructs a <CODE>Paragraph</CODE> with a certain leading.
         *
         * @param   leading     the leading
         */
        public NoNewLineParagraph(float leading) : base(leading) {
        }

        /**
         * Constructs a <CODE>Paragraph</CODE> with a certain <CODE>Chunk</CODE>.
         *
         * @param   chunk       a <CODE>Chunk</CODE>
         */
        public NoNewLineParagraph(Chunk chunk) : base(chunk) {
        }

        /**
         * Constructs a <CODE>Paragraph</CODE> with a certain <CODE>Chunk</CODE>
         * and a certain leading.
         *
         * @param   leading     the leading
         * @param   chunk       a <CODE>Chunk</CODE>
         */
        public NoNewLineParagraph(float leading, Chunk chunk) : base(leading, chunk) {
        }

        /**
         * Constructs a <CODE>Paragraph</CODE> with a certain <CODE>String</CODE>.
         *
         * @param   string      a <CODE>String</CODE>
         */
        public NoNewLineParagraph(String str) : base(str) {
        }

        /**
         * Constructs a <CODE>Paragraph</CODE> with a certain <CODE>String</CODE>
         * and a certain <CODE>Font</CODE>.
         *
         * @param   string      a <CODE>String</CODE>
         * @param   font        a <CODE>Font</CODE>
         */
        public NoNewLineParagraph(String str, Font font) : base(str, font) {
        }

        /**
         * Constructs a <CODE>Paragraph</CODE> with a certain <CODE>String</CODE>
         * and a certain leading.
         *
         * @param   leading     the leading
         * @param   string      a <CODE>String</CODE>
         */
        public NoNewLineParagraph(float leading, String str) : base(leading, str) {
        }

        /**
         * Constructs a <CODE>Paragraph</CODE> with a certain leading, <CODE>String</CODE>
         * and <CODE>Font</CODE>.
         *
         * @param   leading     the leading
         * @param   string      a <CODE>String</CODE>
         * @param   font        a <CODE>Font</CODE>
         */
        public NoNewLineParagraph(float leading, String str, Font font) : base(leading, str, font) {
        }

        /**
         * Constructs a <CODE>Paragraph</CODE> with a certain <CODE>Phrase</CODE>.
         *
         * @param   phrase      a <CODE>Phrase</CODE>
         */
        public NoNewLineParagraph(Phrase phrase) : base(phrase) {
            if (phrase is NoNewLineParagraph) {
                NoNewLineParagraph p = (NoNewLineParagraph)phrase;
                alignment = p.Alignment;
                indentationLeft = p.IndentationLeft;
                indentationRight = p.IndentationRight;
                firstLineIndent = p.FirstLineIndent;
                spacingAfter = p.SpacingAfter;
                spacingBefore = p.SpacingBefore;
                extraParagraphSpace = p.ExtraParagraphSpace;
            }
            if (phrase is Paragraph) {
                Paragraph p = (Paragraph)phrase;
                Alignment = p.Alignment;
                IndentationLeft = p.IndentationLeft;
                IndentationRight = p.IndentationRight;
                FirstLineIndent = p.FirstLineIndent;
                SpacingAfter = p.SpacingAfter;
                SpacingBefore = p.SpacingBefore;
                ExtraParagraphSpace = p.ExtraParagraphSpace;
            }
        }

        /**
         * Adds an <CODE>Element</CODE> to the <CODE>Paragraph</CODE>.
         *
         * @param   o the element to add.
         * @return true is adding the object succeeded
         */
        public override bool Add(IElement o) {
            if (o is List) {
                List list = (List) o;
                list.IndentationLeft = list.IndentationLeft + indentationLeft;
                list.IndentationRight = indentationRight;
                return base.Add(list);
            }
            else if (o is text.Image) {
                base.AddSpecial(o);
                return true;
            }
            return base.Add(o);
        }
        /**
         * Sets the alignment of this paragraph.
         *
         * @param   alignment       the new alignment
         */
        public virtual int Alignment {
            set {
                this.alignment = value;
            }
            get {
                return this.alignment;
            }
        }


        /**
         * Sets the indentation of this paragraph on the left side.
         *
         * @param   indentation     the new indentation
         */
        public virtual float IndentationLeft {
            set {
                this.indentationLeft = value;
            }
            get {
                return this.indentationLeft;
            }
        }

        /**
         * Sets the indentation of this paragraph on the right side.
         *
         * @param   indentation     the new indentation
         */
        public virtual float IndentationRight {
            set {
                this.indentationRight = value;
            }
            get {
                return this.indentationRight;
            }
        }

        /**
         * Setter for property firstLineIndent.
         * @param firstLineIndent New value of property firstLineIndent.
         */
        public virtual float FirstLineIndent {
            set {
                this.firstLineIndent = value;
            }
            get {
                return this.firstLineIndent;
            }
        }

        /**
         * Sets the spacing before this paragraph.
         *
         * @param   spacing     the new spacing
         */
        public virtual float SpacingBefore {
            set {
                this.spacingBefore = value;
            }
            get {
                return this.spacingBefore;
            }
        }

        /**
         * Sets the spacing after this paragraph.
         *
         * @param   spacing     the new spacing
         */
        public virtual float SpacingAfter {
            set {
                this.spacingAfter = value;
            }
            get {
                return this.spacingAfter;
            }
        }

        /**
         * Indicates that the paragraph has to be kept together on one page.
         *
         * @param   keeptogether    true of the paragraph may not be split over 2 pages
         */
        public virtual bool KeepTogether {
            set {
                this.keeptogether = value;
            }
            get {
                return this.keeptogether;
            }
        }

        /**
         * Setter for property extraParagraphSpace.
         * @param extraParagraphSpace New value of property extraParagraphSpace.
         */
        public virtual float ExtraParagraphSpace {
            set {
                this.extraParagraphSpace = value;
            }
            get {
                return this.extraParagraphSpace;
            }
        }
    }
}
