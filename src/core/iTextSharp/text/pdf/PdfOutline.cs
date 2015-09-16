using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

using iTextSharp.text;

/*
 * $Id$
 * 
 *
 * This file is part of the iText project.
 * Copyright (c) 1998-2015 iText Group NV
 * Authors: Bruno Lowagie, Paulo Soares, et al.
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

namespace iTextSharp.text.pdf {

    /**
     * <CODE>PdfOutline</CODE> is an object that represents a PDF outline entry.
     * <P>
     * An outline allows a user to access views of a document by name.<BR>
     * This object is described in the 'Portable Document Format Reference Manual version 1.3'
     * section 6.7 (page 104-106)
     *
     * @see     PdfDictionary
     */

    public class PdfOutline : PdfDictionary {
    
        // membervariables
    
        /** the <CODE>PdfIndirectReference</CODE> of this object */
        private PdfIndirectReference reference;
    
        /** value of the <B>Count</B>-key */
        private int count = 0;
    
        /** value of the <B>Parent</B>-key */
        private PdfOutline parent;
    
        /** value of the <B>Destination</B>-key */
        private PdfDestination destination;
    
        /** The <CODE>PdfAction</CODE> for this outline.
         */
        private PdfAction action;
       
        protected List<PdfOutline> kids = new List<PdfOutline>();
    
        protected PdfWriter writer;
    
        /** Holds value of property tag. */
        private string tag;
    
        /** Holds value of property open. */
        private bool open;
    
        /** Holds value of property color. */
        private BaseColor color;
        
        /** Holds value of property style. */
        private int style = 0;

        // constructors
    
        /**
         * Constructs a <CODE>PdfOutline</CODE>.
         * <P>
         * This is the constructor for the <CODE>outlines object</CODE>.
         */
    
        internal PdfOutline(PdfWriter writer) : base(OUTLINES) {
            open = true;
            parent = null;
            this.writer = writer;
        }
    
        /**
         * Constructs a <CODE>PdfOutline</CODE>.
         * <P>
         * This is the constructor for an <CODE>outline entry</CODE>. The open mode is
         * <CODE>true</CODE>.
         *
         * @param parent the parent of this outline item
         * @param action the <CODE>PdfAction</CODE> for this outline item
         * @param title the title of this outline item
         */
    
        public PdfOutline(PdfOutline parent, PdfAction action, string title) : this(parent, action, title, true) {}
    
        /**
         * Constructs a <CODE>PdfOutline</CODE>.
         * <P>
         * This is the constructor for an <CODE>outline entry</CODE>.
         *
         * @param parent the parent of this outline item
         * @param action the <CODE>PdfAction</CODE> for this outline item
         * @param title the title of this outline item
         * @param open <CODE>true</CODE> if the children are visible
         */
        public PdfOutline(PdfOutline parent, PdfAction action, string title, bool open) : base() {
            this.action = action;
            InitOutline(parent, title, open);
        }
    
        /**
         * Constructs a <CODE>PdfOutline</CODE>.
         * <P>
         * This is the constructor for an <CODE>outline entry</CODE>. The open mode is
         * <CODE>true</CODE>.
         *
         * @param parent the parent of this outline item
         * @param destination the destination for this outline item
         * @param title the title of this outline item
         */
    
        public PdfOutline(PdfOutline parent, PdfDestination destination, string title) : this(parent, destination, title, true) {}
    
        /**
         * Constructs a <CODE>PdfOutline</CODE>.
         * <P>
         * This is the constructor for an <CODE>outline entry</CODE>.
         *
         * @param parent the parent of this outline item
         * @param destination the destination for this outline item
         * @param title the title of this outline item
         * @param open <CODE>true</CODE> if the children are visible
         */
        public PdfOutline(PdfOutline parent, PdfDestination destination, string title, bool open) : base() {
            this.destination = destination;
            InitOutline(parent, title, open);
        }
    
        /**
         * Constructs a <CODE>PdfOutline</CODE>.
         * <P>
         * This is the constructor for an <CODE>outline entry</CODE>. The open mode is
         * <CODE>true</CODE>.
         *
         * @param parent the parent of this outline item
         * @param action the <CODE>PdfAction</CODE> for this outline item
         * @param title the title of this outline item
         */
        public PdfOutline(PdfOutline parent, PdfAction action, PdfString title) : this(parent, action, title, true) {}
    
        /**
         * Constructs a <CODE>PdfOutline</CODE>.
         * <P>
         * This is the constructor for an <CODE>outline entry</CODE>.
         *
         * @param parent the parent of this outline item
         * @param action the <CODE>PdfAction</CODE> for this outline item
         * @param title the title of this outline item
         * @param open <CODE>true</CODE> if the children are visible
         */
        public PdfOutline(PdfOutline parent, PdfAction action, PdfString title, bool open) : this(parent, action, title.ToString(), open) {}
    
        /**
         * Constructs a <CODE>PdfOutline</CODE>.
         * <P>
         * This is the constructor for an <CODE>outline entry</CODE>. The open mode is
         * <CODE>true</CODE>.
         *
         * @param parent the parent of this outline item
         * @param destination the destination for this outline item
         * @param title the title of this outline item
         */
    
        public PdfOutline(PdfOutline parent, PdfDestination destination, PdfString title) : this(parent, destination, title, true) {}
    
        /**
         * Constructs a <CODE>PdfOutline</CODE>.
         * <P>
         * This is the constructor for an <CODE>outline entry</CODE>.
         *
         * @param parent the parent of this outline item
         * @param destination the destination for this outline item
         * @param title the title of this outline item
         * @param open <CODE>true</CODE> if the children are visible
         */
        public PdfOutline(PdfOutline parent, PdfDestination destination, PdfString title, bool open) : this(parent, destination, title.ToString(), true) {}
    
        /**
         * Constructs a <CODE>PdfOutline</CODE>.
         * <P>
         * This is the constructor for an <CODE>outline entry</CODE>. The open mode is
         * <CODE>true</CODE>.
         *
         * @param parent the parent of this outline item
         * @param action the <CODE>PdfAction</CODE> for this outline item
         * @param title the title of this outline item
         */
    
        public PdfOutline(PdfOutline parent, PdfAction action, Paragraph title) : this(parent, action, title, true) {}
    
        /**
         * Constructs a <CODE>PdfOutline</CODE>.
         * <P>
         * This is the constructor for an <CODE>outline entry</CODE>.
         *
         * @param parent the parent of this outline item
         * @param action the <CODE>PdfAction</CODE> for this outline item
         * @param title the title of this outline item
         * @param open <CODE>true</CODE> if the children are visible
         */
        public PdfOutline(PdfOutline parent, PdfAction action, Paragraph title, bool open) : base() {
            StringBuilder buf = new StringBuilder();
            foreach (Chunk chunk in title.Chunks) {
                buf.Append(chunk.Content);
            }
            this.action = action;
            InitOutline(parent, buf.ToString(), open);
        }
    
        /**
         * Constructs a <CODE>PdfOutline</CODE>.
         * <P>
         * This is the constructor for an <CODE>outline entry</CODE>. The open mode is
         * <CODE>true</CODE>.
         *
         * @param parent the parent of this outline item
         * @param destination the destination for this outline item
         * @param title the title of this outline item
         */
    
        public PdfOutline(PdfOutline parent, PdfDestination destination, Paragraph title) : this(parent, destination, title, true) {}
    
        /**
         * Constructs a <CODE>PdfOutline</CODE>.
         * <P>
         * This is the constructor for an <CODE>outline entry</CODE>.
         *
         * @param parent the parent of this outline item
         * @param destination the destination for this outline item
         * @param title the title of this outline item
         * @param open <CODE>true</CODE> if the children are visible
         */
        public PdfOutline(PdfOutline parent, PdfDestination destination, Paragraph title, bool open) : base() {
            StringBuilder buf = new StringBuilder();
            foreach (Chunk chunk in title.Chunks) {
                buf.Append(chunk.Content);
            }
            this.destination = destination;
            InitOutline(parent, buf.ToString(), open);
        }
    
    
        // methods
    
        /** Helper for the constructors.
         * @param parent the parent outline
         * @param title the title for this outline
         * @param open <CODE>true</CODE> if the children are visible
         */
        internal void InitOutline(PdfOutline parent, string title, bool open) {
            this.open = open;
            this.parent = parent;
            writer = parent.writer;
            Put(PdfName.TITLE, new PdfString(title, PdfObject.TEXT_UNICODE));
            parent.AddKid(this);
            if (destination != null && !destination.HasPage()) // bugfix Finn Bock
                SetDestinationPage(writer.CurrentPage);
        }
    
        /**
         * Gets the indirect reference of this <CODE>PdfOutline</CODE>.
         *
         * @return      the <CODE>PdfIndirectReference</CODE> to this outline.
         */
    
        virtual public PdfIndirectReference IndirectReference {
            get {
                return reference;
            }

            set {
                this.reference = value;
            }
        }
    
        /**
         * Gets the parent of this <CODE>PdfOutline</CODE>.
         *
         * @return      the <CODE>PdfOutline</CODE> that is the parent of this outline.
         */
    
        virtual public PdfOutline Parent {
            get {
                return parent;
            }
        }
    
        /**
         * Set the page of the <CODE>PdfDestination</CODE>-object.
         *
         * @param pageReference indirect reference to the page
         * @return <CODE>true</CODE> if this page was set as the <CODE>PdfDestination</CODE>-page.
         */
    
        virtual public bool SetDestinationPage(PdfIndirectReference pageReference) {
            if (destination == null) {
                return false;
            }
            return destination.AddPage(pageReference);
        }
    
        /**
         * Gets the destination for this outline.
         * @return the destination
         */
        virtual public PdfDestination PdfDestination {
            get {
                return destination;
            }
        }
    
        internal int Count {
            get {
                return count;
            }

            set {
                this.count = value;
            }
        }

        /**
         * returns the level of this outline.
         *
         * @return      a level
         */
    
        virtual public int Level {
            get {
                if (parent == null) {
                    return 0;
                }
                return (parent.Level + 1);
            }
        }
    
        /**
        * Returns the PDF representation of this <CODE>PdfOutline</CODE>.
        *
        * @param writer the encryption information
        * @param os
        * @throws IOException
        */
        
        public override void ToPdf(PdfWriter writer, Stream os) {
            if (color != null && !color.Equals(BaseColor.BLACK)) {
                Put(PdfName.C, new PdfArray(new float[]{color.R/255f,color.G/255f,color.B/255f}));
            }
            int flag = 0;
            if ((style & Font.BOLD) != 0)
                flag |= 2;
            if ((style & Font.ITALIC) != 0)
                flag |= 1;
            if (flag != 0)
                Put(PdfName.F, new PdfNumber(flag));
            if (parent != null) {
                Put(PdfName.PARENT, parent.IndirectReference);
            }
            if (destination != null && destination.HasPage()) {
                Put(PdfName.DEST, destination);
            }
            if (action != null)
                Put(PdfName.A, action);
            if (count != 0) {
                Put(PdfName.COUNT, new PdfNumber(count));
            }
            base.ToPdf(writer, os);
        }
    
        virtual public void AddKid(PdfOutline outline) {
            kids.Add(outline);
        }
    
        virtual public List<PdfOutline> Kids {
            get {
                return kids;
            }

            set {
                this.kids = value;
            }
        }
    
        /** Getter for property tag.
         * @return Value of property tag.
         */
        virtual public string Tag {
            get {
                return tag;
            }

            set {
                this.tag = value;
            }
        }
    
        virtual public string Title {
            get {
                PdfString title = (PdfString)Get(PdfName.TITLE);
                return title.ToString();
            }

            set {
                Put(PdfName.TITLE, new PdfString(value, PdfObject.TEXT_UNICODE));
            }
        }
    
        /** Setter for property open.
         * @param open New value of property open.
         */
        virtual public bool Open {
            set {
                this.open = value;
            }
            get {
                return open;
            }
        }

        virtual public BaseColor Color {
            get {
                return color;
            }
            set {
                color = value;
            }
        }

        virtual public int Style {
            get {
                return style;
            }
            set {
                style = value;
            }
        }
    }
}
