using System;
using System.Text;
using System.Collections.Generic;
using System.util;
using iTextSharp.text.api;
using iTextSharp.text.error_messages;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.interfaces;

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

namespace iTextSharp.text {
    /// <summary>
    /// A Section is a part of a Document containing
    /// other Sections, Paragraphs, List
    /// and/or Tables.
    /// </summary>
    /// <remarks>
    /// You can not construct a Section yourself.
    /// You will have to ask an instance of Section to the
    /// Chapter or Section to which you want to
    /// add the new Section.
    /// </remarks>
    /// <example>
    /// <code>
    /// Paragraph title2 = new Paragraph("This is Chapter 2", FontFactory.GetFont(FontFactory.HELVETICA, 18, Font.BOLDITALIC, new Color(0, 0, 255)));
    /// Chapter chapter2 = new Chapter(title2, 2);
    /// Paragraph someText = new Paragraph("This is some text");
    /// chapter2.Add(someText);
    /// Paragraph title21 = new Paragraph("This is Section 1 in Chapter 2", FontFactory.GetFont(FontFactory.HELVETICA, 16, Font.BOLD, new Color(255, 0, 0)));
    /// <strong>Section section1 = chapter2.AddSection(title21);</strong>
    /// Paragraph someSectionText = new Paragraph("This is some silly paragraph in a chapter and/or section. It contains some text to test the functionality of Chapters and Section.");
    /// <strong>section1.Add(someSectionText);</strong>
    /// Paragraph title211 = new Paragraph("This is SubSection 1 in Section 1 in Chapter 2", FontFactory.GetFont(FontFactory.HELVETICA, 14, Font.BOLD, new Color(255, 0, 0)));
    /// <strong>Section section11 = section1.AddSection(40, title211, 2);
    /// section11.Add(someSectionText);</strong>strong>
    /// </code>
    /// </example>
    public class Section : List<IElement>, ITextElementArray, ILargeElement, IIndentable, IAccessibleElement {
        
        // constant
        /**
        * A possible number style. The default number style: "1.2.3."
        * @since   iText 2.0.8
        */
        public const int NUMBERSTYLE_DOTTED = 0;
        /**
        * A possible number style. For instance: "1.2.3"
        * @since   iText 2.0.8
        */
        public const int NUMBERSTYLE_DOTTED_WITHOUT_FINAL_DOT = 1;
        
        // membervariables
    
        ///<summary> This is the title of this section. </summary>
        protected Paragraph title;
    
        ///<summary> This is the number of sectionnumbers that has to be shown before the section title. </summary>
        protected int numberDepth;
    
        /**
        * The style for sectionnumbers.
        * @since    iText 2.0.8
        */
        protected int numberStyle = NUMBERSTYLE_DOTTED;
        
        ///<summary> The indentation of this section on the left side. </summary>
        protected float indentationLeft;
    
        ///<summary> The indentation of this section on the right side. </summary>
        protected float indentationRight;
    
        ///<summary> The additional indentation of the content of this section. </summary>
        protected float indentation;
    
        ///<summary> This is the number of subsections. </summary>
        protected int subsections = 0;
    
        ///<summary> This is the complete list of sectionnumbers of this section and the parents of this section. </summary>
        protected internal List<int> numbers = null;
    
    /**
        * Indicates if the Section will be complete once added to the document.
        * @since   iText 2.0.8
        */
        protected bool complete = true;
        
        /**
        * Indicates if the Section was added completely to the document.
        * @since   iText 2.0.8
        */
        protected bool addedCompletely = false;
        
        /**
        * Indicates if this is the first time the section was added.
        * @since   iText 2.0.8
        */
        protected bool notAddedYet = true;
        
        ///<summary> false if the bookmark children are not visible </summary>
        protected bool bookmarkOpen = true;
    
        /** true if the section has to trigger a new page */
        protected bool triggerNewPage = false;

        /** The bookmark title if different from the content title */
        protected string bookmarkTitle;

        // constructors
    
        /// <summary>
        /// Constructs a new Section.
        /// </summary>
        /// <overloads>
        /// Has 2 overloads.
        /// </overloads>
        protected internal Section() {
            title = new Paragraph();
            numberDepth = 1;
            title.Role = new PdfName("H" + numberDepth);
        }
    
        /// <summary>
        /// Constructs a new Section.
        /// </summary>
        /// <param name="title">a Paragraph</param>
        /// <param name="numberDepth">the numberDepth</param>
        protected internal Section(Paragraph title, int numberDepth) {
            this.numberDepth = numberDepth;
            this.title = title;
            if (title != null)
                title.Role = new PdfName("H" + numberDepth);
        }
    
        // private methods
    
        /// <summary>
        /// Sets the number of this section.
        /// </summary>
        /// <param name="number">the number of this section</param>
        /// <param name="numbers">an ArrayList, containing the numbers of the Parent</param>
        private void SetNumbers(int number, List<int> numbers) {
            this.numbers = new List<int>();
            this.numbers.Add(number);
            this.numbers.AddRange(numbers);
        }
    
        // implementation of the Element-methods
    
        /// <summary>
        /// Processes the element by adding it (or the different parts) to an
        /// IElementListener.
        /// </summary>
        /// <param name="listener">the IElementListener</param>
        /// <returns>true if the element was processed successfully</returns>
        virtual public bool Process(IElementListener listener) {
            try {
                foreach (IElement ele in this) {
                    listener.Add(ele);
                }
                return true;
            }
            catch (DocumentException) {
                return false;
            }
        }
    
        /// <summary>
        /// Gets the type of the text element.
        /// </summary>
        /// <value>a type</value>
        public virtual int Type {
            get {
                return Element.SECTION;
            }
        }
    
        /// <summary>
        /// Gets all the chunks in this element.
        /// </summary>
        /// <value>an ArrayList</value>
        virtual public IList<Chunk> Chunks {
            get {
                List<Chunk> tmp = new List<Chunk>();
                foreach (IElement ele in this) {
                    tmp.AddRange(ele.Chunks);
                }
                return tmp;
            }
        }
    
        /**
        * @see com.lowagie.text.Element#isContent()
        * @since   iText 2.0.8
        */
        virtual public bool IsContent() {
            return true;
        }

        /**
        * @see com.lowagie.text.Element#isNestable()
        * @since   iText 2.0.8
        */
        public virtual bool IsNestable() {
            return false;
        }
        
        // overriding some of the ArrayList-methods
    
        /// <summary>
        /// Adds a Paragraph, List or Table
        /// to this Section.
        /// </summary>
        /// <param name="index">index at which the specified element is to be inserted</param>
        /// <param name="o">an object of type Paragraph, List or Table</param>
        virtual public void Add(int index, IElement element) {
            if (AddedCompletely) {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("this.largeelement.has.already.been.added.to.the.document"));
            }
            try {
                if (element.IsNestable()) {
                    base.Insert(index, element);
                }
                else {
                    throw new Exception(element.Type.ToString());
                }
            }
            catch (Exception cce) {
                throw new Exception(MessageLocalization.GetComposedMessage("insertion.of.illegal.element.1", cce.Message));
            }
        }
    
        /// <summary>
        /// Adds a Paragraph, List, Table or another Section
        /// to this Section.
        /// </summary>
        /// <param name="o">an object of type Paragraph, List, Table or another Section</param>
        /// <returns>a bool</returns>
        public new bool Add(IElement element) {
            try {
                if (element.Type == Element.SECTION) {
                    Section section = (Section)element;
                    section.SetNumbers(++subsections, numbers);
                    base.Add(section);
                    return true;
                }
                else if (element is MarkedSection && ((MarkedObject)element).element.Type == Element.SECTION) {
                    MarkedSection mo = (MarkedSection)element;
                    Section section = (Section)(mo.element);
                    section.SetNumbers(++subsections, numbers);
                    base.Add(mo);
                    return true;
                }
                else if (element.IsNestable()) {
                    base.Add(element);
                    return true;
                }
                else {
                    throw new InvalidCastException(MessageLocalization.GetComposedMessage("you.can.t.add.a.1.to.a.section", element.Type.ToString()));
                }
            }
            catch (InvalidCastException cce) {
                throw new InvalidCastException(MessageLocalization.GetComposedMessage("insertion.of.illegal.element.1", cce.Message));
            }
        }
    
        /// <summary>
        /// Adds a collection of Elements
        /// to this Section.
        /// </summary>
        /// <param name="collection">a collection of Paragraphs, Lists and/or Tables</param>
        /// <returns>true if the action succeeded, false if not.</returns>
        virtual public bool AddAll<T>(ICollection<T> collection) where T : IElement {
            if (collection.Count == 0)
                return false;
            foreach (IElement itm in collection) {
                this.Add(itm);
            }
            return true;
        }
    
        // methods that return a Section
    
        /// <summary>
        /// Creates a Section, adds it to this Section and returns it.
        /// </summary>
        /// <param name="indentation">the indentation of the new section</param>
        /// <param name="title">the title of the new section</param>
        /// <param name="numberDepth">the numberDepth of the section</param>
        /// <returns>the newly added Section</returns>
        public virtual Section AddSection(float indentation, Paragraph title, int numberDepth) {
            if (AddedCompletely) {
                throw new InvalidOperationException(MessageLocalization.GetComposedMessage("this.largeelement.has.already.been.added.to.the.document"));
            }
            Section section = new Section(title, numberDepth);
            section.Indentation = indentation;
            Add(section);
            return section;
        }
    
        /// <summary>
        /// Creates a Section, adds it to this Section and returns it.
        /// </summary>
        /// <param name="indentation">the indentation of the new section</param>
        /// <param name="title">the title of the new section</param>
        /// <returns>the newly added Section</returns>
        public virtual Section AddSection(float indentation, Paragraph title) {
            return AddSection(indentation, title, numberDepth + 1);
        }
    
        /// <summary>
        /// Creates a Section, add it to this Section and returns it.
        /// </summary>
        /// <param name="title">the title of the new section</param>
        /// <param name="numberDepth">the numberDepth of the section</param>
        /// <returns>the newly added Section</returns>
        public virtual Section AddSection(Paragraph title, int numberDepth) {
            return AddSection(0, title, numberDepth);
        }
    
        /**
        * Adds a marked section. For use in class MarkedSection only!
        */
        virtual public MarkedSection AddMarkedSection() {
            MarkedSection section = new MarkedSection(new Section(null, numberDepth + 1));
            Add(section);
            return section;
        }

        /// <summary>
        /// Creates a Section, adds it to this Section and returns it.
        /// </summary>
        /// <param name="title">the title of the new section</param>
        /// <returns>the newly added Section</returns>
        public virtual Section AddSection(Paragraph title) {
            return AddSection(0, title, numberDepth + 1);
        }
    
        /**
         * Adds a Section to this Section and returns it.
         *
         * @param    indentation    the indentation of the new section
         * @param    title        the title of the new section
         * @param    numberDepth    the numberDepth of the section
         */
        /// <summary>
        /// Adds a Section to this Section and returns it.
        /// </summary>
        /// <param name="indentation">the indentation of the new section</param>
        /// <param name="title">the title of the new section</param>
        /// <param name="numberDepth">the numberDepth of the section</param>
        /// <returns>the newly added Section</returns>
        public virtual Section AddSection(float indentation, string title, int numberDepth) {
            return AddSection(indentation, new Paragraph(title), numberDepth);
        }
    
        /**
         * Adds a Section to this Section and returns it.
         *
         * @param    title        the title of the new section
         * @param    numberDepth    the numberDepth of the section
         */
        /// <summary>
        /// Adds a Section to this Section and returns it.
        /// </summary>
        /// <param name="title">the title of the new section</param>
        /// <param name="numberDepth">the numberDepth of the section</param>
        /// <returns>the newly added Section</returns>
        public virtual Section AddSection(string title, int numberDepth) {
            return AddSection(new Paragraph(title), numberDepth);
        }
    
        /// <summary>
        /// Adds a Section to this Section and returns it.
        /// </summary>
        /// <param name="indentation">the indentation of the new section</param>
        /// <param name="title">the title of the new section</param>
        /// <returns>the newly added Section</returns>
        public virtual Section AddSection(float indentation, string title) {
            return AddSection(indentation, new Paragraph(title));
        }
    
        /// <summary>
        /// Adds a Section to this Section and returns it.
        /// </summary>
        /// <param name="title">the title of the new section</param>
        /// <returns>the newly added Section</returns>
        public virtual Section AddSection(string title) {
            return AddSection(new Paragraph(title));
        }
    
        // public methods
    
        /// <summary>
        /// Get/set the title of this section
        /// </summary>
        /// <value>a Paragraph</value>
        virtual public Paragraph Title {
            get {
                return ConstructTitle(title, numbers, numberDepth, numberStyle);
            }
            
            set {
                this.title = value;
            }
        }

        /**
        * Sets the style for numbering sections.
        * Possible values are NUMBERSTYLE_DOTTED: 1.2.3. (the default)
        * or NUMBERSTYLE_DOTTED_WITHOUT_FINAL_DOT: 1.2.3
        * @since    iText 2.0.8
        */
        virtual public int NumberStyle {
            set {
                numberStyle = value;
            }
            get {
                return numberStyle;
            }
        }
        
        /**
        * Constructs a Paragraph that will be used as title for a Section or Chapter.
        * @param    title   the title of the section
        * @param    numbers a list of sectionnumbers
        * @param    numberDepth how many numbers have to be shown
        * @param    numberStyle the numbering style
        * @return   a Paragraph object
        * @since    iText 2.0.8
        */
        public static Paragraph ConstructTitle(Paragraph title, List<int> numbers, int numberDepth, int numberStyle) {
            if (title == null) {
                return null;
            }
            int depth = Math.Min(numbers.Count, numberDepth);
            if (depth < 1) {
                return title;
            }
            StringBuilder buf = new StringBuilder(" ");
            for (int i = 0; i < depth; i++) {
                buf.Insert(0, ".");
                buf.Insert(0, numbers[i]);
            }
            if (numberStyle == NUMBERSTYLE_DOTTED_WITHOUT_FINAL_DOT) {
                buf.Remove(buf.Length - 2, 1);
            }
            Paragraph result = new Paragraph(title);
            result.Insert(0, new Chunk(buf.ToString(), title.Font));
            return result;
        }

        // methods to retrieve information
    
        /// <summary>
        /// Checks if this object is a Chapter.
        /// </summary>
        /// <returns>
        /// true if it is a Chapter,
        /// false if it is a Section
        /// </returns>
        virtual public bool IsChapter() {
            return Type == Element.CHAPTER;
        }
    
        /// <summary>
        /// Checks if this object is a Section.
        /// </summary>
        /// <returns>
        /// true if it is a Section,
        /// false if it is a Chapter.
        /// </returns>
        virtual public bool IsSection() {
            return Type == Element.SECTION;
        }
    
        /// <summary>
        /// Get/set the numberdepth of this Section.
        /// </summary>
        /// <value>a int</value>
        virtual public int NumberDepth {
            get {
                return numberDepth;
            }

            set {
                this.numberDepth = value;
            }
        }
    
        /// <summary>
        /// Get/set the indentation of this Section on the left side.
        /// </summary>
        /// <value>the indentation</value>
        virtual public float IndentationLeft {
            get {
                return indentationLeft;
            }

            set {
                indentationLeft = value;
            }
        }
    
        /// <summary>
        /// Get/set the indentation of this Section on the right side.
        /// </summary>
        /// <value>the indentation</value>
        virtual public float IndentationRight {
            get {
                return indentationRight;
            }

            set {
                indentationRight = value;
            }
        }
    
        /// <summary>
        /// Get/set the indentation of the content of this Section.
        /// </summary>
        /// <value>the indentation</value>
        virtual public float Indentation {
            get {
                return indentation;
            }

            set {
                indentation = value;
            }
        }
    
        /// <summary>
        /// Returns the depth of this section.
        /// </summary>
        /// <value>the depth</value>
        virtual public int Depth {
            get {
                return numbers.Count;
            }
        }
    
        /// <summary>
        /// Get/set the bookmark
        /// </summary>
        /// <value>a bool</value>
        virtual public bool BookmarkOpen {
            get {
                return bookmarkOpen;
            }

            set {
                this.bookmarkOpen = value;
            }
        }

        /**
        * Gets the bookmark title.
        * @return the bookmark title
        */    
        virtual public Paragraph GetBookmarkTitle() {
            if (bookmarkTitle == null)
                return Title;
            else
                return new Paragraph(bookmarkTitle);
        }
        
        /**
        * Sets the bookmark title. The bookmark title is the same as the section title but
        * can be changed with this method.
        * @param bookmarkTitle the bookmark title
        */    
        virtual public String BookmarkTitle {
            set {
                this.bookmarkTitle = value;
            }
        }

        public override string ToString() {
            return base.ToString();
        }

        public virtual bool TriggerNewPage {
            get {
                return triggerNewPage && notAddedYet;
            }
            set {
                triggerNewPage = value;
            }
        }

        /**
        * Changes the Chapter number.
        */
        virtual public void SetChapterNumber(int number) {
            numbers[numbers.Count - 1] = number;
            foreach (IElement s in this) {
                if (s is Section) {
                    ((Section)s).SetChapterNumber(number);
                }
            }
        }

        /**
        * Indicates if this is the first time the section is added.
        * @since   iText2.0.8
        * @return  true if the section wasn't added yet
        */
        virtual public bool NotAddedYet {
            get {
                return notAddedYet;
            }
            set {
                notAddedYet = value;
            }
        }

        /**
        * @see com.lowagie.text.LargeElement#isAddedCompletely()
        * @since   iText 2.0.8
        */
        virtual protected bool AddedCompletely {
            get {
                return addedCompletely;
            }
            set {
                addedCompletely = value;
            }
        }
        
        /**
        * @since   iText 2.0.8
        * @see com.lowagie.text.LargeElement#flushContent()
        */
        virtual public void FlushContent() {
            NotAddedYet = false;
            title = null;
            for (int k = 0; k < Count; ++k) {
                IElement element = this[k];
                if (element is Section) {
                    Section s = (Section)element;
                    if (!s.ElementComplete && Count == 1) {
                        s.FlushContent();
                        return;
                    }
                    else {
                        s.AddedCompletely = true;
                    }
                }
                this.RemoveAt(k);
                --k;
            }
        }

        /**
        * @since   iText 2.0.8
        * @see com.lowagie.text.LargeElement#isComplete()
        */
        virtual public bool ElementComplete {
            get {
                return complete;
            }
            set {
                complete = value;
            }
        }

        /**
        * Adds a new page to the section.
        * @since   2.1.1
        */
        virtual public void NewPage() {
            this.Add(Chunk.NEXTPAGE);
        }

        virtual public PdfObject GetAccessibleAttribute(PdfName key) {
            return title.GetAccessibleAttribute(key);
        }

        virtual public void SetAccessibleAttribute(PdfName key, PdfObject value) {
            title.SetAccessibleAttribute(key, value);
        }

        virtual public Dictionary<PdfName, PdfObject> GetAccessibleAttributes() {
            return title.GetAccessibleAttributes();
        }

        virtual public PdfName Role {
            get { return title.Role; }
            set { title.Role = value; }
        }

        virtual public AccessibleElementId ID {
            get { return title.ID; }
            set { title.ID = value; }
        }

        public virtual bool IsInline {
            get { return false; }
        }
    }
}
