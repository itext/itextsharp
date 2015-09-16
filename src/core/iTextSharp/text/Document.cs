using System;
using System.Collections.Generic;
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
    /// A generic Document class.
    /// </summary>
    /// <remarks>
    /// All kinds of Text-elements can be added to a HTMLDocument.
    /// The Document signals all the listeners when an element
    /// has been added.<p/>
    /// <OL>
    ///     <LI/>Once a document is created you can add some meta information.
    ///     <LI/>You can also set the headers/footers.
    ///     <LI/>You have to open the document before you can write content.
    ///     <LI/>You can only write content (no more meta-formation!) once a document is opened.
    ///     <LI/>When you change the header/footer on a certain page, this will be effective starting on the next page.
    ///     <LI/>Ater closing the document, every listener (as well as its OutputStream) is closed too.
    /// </OL>
    /// </remarks>
    /// <example>
    /// <code>
    /// // creation of the document with a certain size and certain margins
    /// <strong>Document document = new Document(PageSize.A4, 50, 50, 50, 50);</strong>
    /// try {
    ///     // creation of the different writers
    ///     HtmlWriter.GetInstance(<strong>document</strong>, System.out);
    ///     PdfWriter.GetInstance(<strong>document</strong>, new FileOutputStream("text.pdf"));
    ///     // we add some meta information to the document
    ///     <strong>document.AddAuthor("Bruno Lowagie");
    ///     document.AddSubject("This is the result of a Test.");</strong>
    ///     
    ///     // we define a header and a footer
    ///     HeaderFooter header = new HeaderFooter(new Phrase("This is a header."), false);
    ///     HeaderFooter footer = new HeaderFooter(new Phrase("This is page "), new Phrase("."));
    ///     footer.SetAlignment(Element.ALIGN_CENTER);
    ///     <strong>document.SetHeader(header);
    ///     document.SetFooter(footer);</strong>
    ///     // we open the document for writing
    ///     <strong>document.Open();
    ///     document.Add(new Paragraph("Hello world"));</strong>
    /// }
    /// catch (DocumentException de) {
    ///     Console.Error.WriteLine(de.Message);
    /// }
    /// <strong>document.Close();</strong>
    /// </code>
    /// </example>
    public class Document : IDocListener, IAccessibleElement {

        ///<summary> Allows the pdf documents to be produced without compression for debugging purposes. </summary>
        public static bool Compress = true;

        ///<summary> Scales the WMF font size. The default value is 0.86.  </summary>
        public static float WmfFontCorrection = 0.86f;

        ///<summary> The IDocListener. </summary>
        protected List<IDocListener> listeners = new List<IDocListener>();

        ///<summary> Is the document open or not? </summary>
        protected bool open;

        ///<summary> Has the document already been closed? </summary>
        protected bool close;

        // membervariables concerning the layout

        ///<summary> The size of the page. </summary>
        protected Rectangle pageSize;

        ///<summary> margin in x direction starting from the left </summary>
        protected float marginLeft = 0;

        ///<summary> margin in x direction starting from the right </summary>
        protected float marginRight = 0;

        ///<summary> margin in y direction starting from the top </summary>
        protected float marginTop = 0;

        ///<summary> margin in y direction starting from the bottom </summary>
        protected float marginBottom = 0;

        protected bool marginMirroring = false;

        /**
        * mirroring of the top/bottom margins
        * @since	2.1.6
        */
        protected bool marginMirroringTopBottom = false;

        ///<summary> Content of JavaScript onLoad function </summary>
        protected string javaScript_onLoad = null;

        ///<summary> Content of JavaScript onUnLoad function  </summary>
        protected string javaScript_onUnLoad = null;

        ///<summary> Style class in HTML body tag </summary>
        protected string htmlStyleClass = null;

        // headers, footers

        ///<summary> Current pagenumber </summary>
        protected int pageN = 0;

        /** This is a chapter number in case ChapterAutoNumber is used. */
        protected int chapternumber = 0;
        protected PdfName role = PdfName.DOCUMENT;
        protected Dictionary<PdfName, PdfObject> accessibleAttributes = null;
        protected AccessibleElementId id = new AccessibleElementId();
        // constructor

        /// <summary>
        /// Constructs a new Document-object.
        /// </summary>
        /// <overloads>
        /// Has three overloads.
        /// </overloads>
        public Document() : this(iTextSharp.text.PageSize.A4) {}

        /// <summary>
        /// Constructs a new Document-object.
        /// </summary>
        /// <param name="pageSize">the pageSize</param>
        public Document(Rectangle pageSize) : this(pageSize, 36, 36, 36, 36) {}

        /// <summary>
        /// Constructs a new Document-object.
        /// </summary>
        /// <param name="pageSize">the pageSize</param>
        /// <param name="marginLeft">the margin on the left</param>
        /// <param name="marginRight">the margin on the right</param>
        /// <param name="marginTop">the margin on the top</param>
        /// <param name="marginBottom">the margin on the bottom</param>
        public Document(Rectangle pageSize, float marginLeft, float marginRight, float marginTop, float marginBottom) {
            this.pageSize = pageSize;
            this.marginLeft = marginLeft;
            this.marginRight = marginRight;
            this.marginTop = marginTop;
            this.marginBottom = marginBottom;
        }

        // listener methods

        /// <summary>
        /// Adds a IDocListener to the Document.
        /// </summary>
        /// <param name="listener">the new IDocListener</param>
        virtual public void AddDocListener(IDocListener listener) {
            listeners.Add(listener);
            if (listener is IAccessibleElement) {
                IAccessibleElement ae = (IAccessibleElement)listener;
                ae.Role = this.role;
                ae.ID = this.id;
                if (this.accessibleAttributes != null) {
                    foreach (PdfName key in this.accessibleAttributes.Keys)
                        ae.SetAccessibleAttribute(key, this.accessibleAttributes[key]);
                }
            }
        }

        /// <summary>
        /// Removes a IDocListener from the Document.
        /// </summary>
        /// <param name="listener">the IDocListener that has to be removed.</param>
        virtual public void RemoveIDocListener(IDocListener listener) {
            listeners.Remove(listener);
        }

        // methods implementing the IDocListener interface

        /// <summary>
        /// Adds an Element to the Document.
        /// </summary>
        /// <param name="element">the Element to add</param>
        /// <returns>true if the element was added, false if not</returns>
        public virtual bool Add(IElement element) {
            if (close) {
                throw new DocumentException(MessageLocalization.GetComposedMessage("the.document.has.been.closed.you.can.t.add.any.elements"));
            }
            if (!open && element.IsContent()) {
                throw new DocumentException(MessageLocalization.GetComposedMessage("the.document.is.not.open.yet.you.can.only.add.meta.information"));
            }
            bool success = false;
            if (element is ChapterAutoNumber) {
        	    chapternumber = ((ChapterAutoNumber)element).SetAutomaticNumber(chapternumber);
            }
            foreach (IDocListener listener in listeners) {
                success |= listener.Add(element);
            }
		    if (element is ILargeElement) {
			    ILargeElement e = (ILargeElement)element;
			    if (!e.ElementComplete)
				    e.FlushContent();
		    }
            return success;
        }

        /// <summary>
        /// Opens the document.
        /// </summary>
        /// <remarks>
        /// Once the document is opened, you can't write any Header- or Meta-information
        /// anymore. You have to open the document before you can begin to add content
        /// to the body of the document.
        /// </remarks>
        public virtual void Open() {
            if (! close) {
                open = true;
            }
            foreach (IDocListener listener in listeners) {
                listener.SetPageSize(pageSize);
                listener.SetMargins(marginLeft, marginRight, marginTop, marginBottom);
                listener.Open();
            }
        }

        /// <summary>
        /// Opens the document.
        /// </summary>
        /// <remarks>
        /// Version for languages that are not case-dependant.
        /// Once the document is opened, you can't write any Header- or Meta-information
        /// anymore. You have to open the document before you can begin to add content
        /// to the body of the document.
        /// </remarks>
        public virtual void OpenDocument() {
            Open();
        }

        /// <summary>
        /// Sets the pagesize.
        /// </summary>
        /// <param name="pageSize">the new pagesize</param>
        /// <returns>a bool</returns>
        public virtual bool SetPageSize(Rectangle pageSize) {
            this.pageSize = pageSize;
            foreach (IDocListener listener in listeners) {
                listener.SetPageSize(pageSize);
            }
            return true;
        }

        /// <summary>
        /// Sets the margins.
        /// </summary>
        /// <param name="marginLeft">the margin on the left</param>
        /// <param name="marginRight">the margin on the right</param>
        /// <param name="marginTop">the margin on the top</param>
        /// <param name="marginBottom">the margin on the bottom</param>
        /// <returns></returns>
        public virtual bool SetMargins(float marginLeft,float marginRight,float marginTop,float marginBottom) {
            this.marginLeft = marginLeft;
            this.marginRight = marginRight;
            this.marginTop = marginTop;
            this.marginBottom = marginBottom;
            foreach (IDocListener listener in listeners) {
                listener.SetMargins(marginLeft, marginRight, marginTop, marginBottom);
            }
            return true;
        }

        /// <summary>
        /// Signals that an new page has to be started.
        /// </summary>
        /// <returns>true if the page was added, false if not.</returns>
        public virtual bool NewPage() {
            if (!open || close) {
                return false;
            }
            foreach (IDocListener listener in listeners) {
                listener.NewPage();
            }
            return true;
        }

        /// <summary>
        /// Sets the page number to 0.
        /// </summary>
        public virtual void ResetPageCount() {
            pageN = 0;
            foreach (IDocListener listener in listeners) {
                listener.ResetPageCount();
            }
        }

        /// <summary>
        /// Sets the page number.
        /// </summary>
        /// <value>an int</value>
        public virtual int PageCount {
            set {
                this.pageN = value;
                foreach (IDocListener listener in listeners) {
                    listener.PageCount = value;
                }
            }
        }

        /// <summary>
        /// Returns the current page number.
        /// </summary>
        /// <value>an int</value>
        virtual public int PageNumber {
            get {
                return this.pageN;
            }
        }

        /// <summary>
        /// Closes the document.
        /// </summary>
        /// <remarks>
        /// Once all the content has been written in the body, you have to close
        /// the body. After that nothing can be written to the body anymore.
        /// </remarks>
        public virtual void Close() {
            if (!close) {
                open = false;
                close = true;
            }
            foreach (IDocListener listener in listeners) {
                listener.Close();
            }
        }

        /// <summary>
        /// Closes the document.
        /// </summary>
        /// <remarks>
        /// Version for languages that are not case-dependant.
        /// Once all the content has been written in the body, you have to close
        /// the body. After that nothing can be written to the body anymore.
        /// </remarks>
        public virtual void CloseDocument() {
            Close();
        }

        // methods concerning the header or some meta information

        /// <summary>
        /// Adds a user defined header to the document.
        /// </summary>
        /// <param name="name">the name of the header</param>
        /// <param name="content">the content of the header</param>
        /// <returns>true if successful, false otherwise</returns>
        virtual public bool AddHeader(string name, string content) {
            return Add(new Header(name, content));
        }

        /// <summary>
        /// Adds the title to a Document.
        /// </summary>
        /// <param name="title">the title</param>
        /// <returns>true if successful, false otherwise</returns>
        virtual public bool AddTitle(string title) {
            return Add(new Meta(Element.TITLE, title));
        }

        /// <summary>
        /// Adds the subject to a Document.
        /// </summary>
        /// <param name="subject">the subject</param>
        /// <returns>true if successful, false otherwise</returns>
        virtual public bool AddSubject(string subject) {
            return Add(new Meta(Element.SUBJECT, subject));
        }

        /// <summary>
        /// Adds the keywords to a Document.
        /// </summary>
        /// <param name="keywords">keywords to add</param>
        /// <returns>true if successful, false otherwise</returns>
        virtual public bool AddKeywords(string keywords) {
            return Add(new Meta(Element.KEYWORDS, keywords));
        }

        /// <summary>
        /// Adds the author to a Document.
        /// </summary>
        /// <param name="author">the name of the author</param>
        /// <returns>true if successful, false otherwise</returns>
        virtual public bool AddAuthor(string author) {
            return Add(new Meta(Element.AUTHOR, author));
        }

        /// <summary>
        /// Adds the creator to a Document.
        /// </summary>
        /// <param name="creator">the name of the creator</param>
        /// <returns>true if successful, false otherwise</returns>
        virtual public bool AddCreator(string creator) {
            return Add(new Meta(Element.CREATOR, creator));
        }

        /// <summary>
        /// Adds the producer to a Document.
        /// </summary>
        /// <returns>true if successful, false otherwise</returns>
        virtual public bool AddProducer() {
            return Add(new Meta(Element.PRODUCER, Version.GetInstance().GetVersion));
        }

        /**
         * Adds a language to th document. Required for PDF/UA compatible documents.
         * @param language
         * @return <code>true</code> if successfull, <code>false</code> otherwise
        */ 
        virtual public bool AddLanguage(String language) {
            try {
                return Add(new Meta(Element.LANGUAGE, language));
            } catch (DocumentException de) {
                throw de;
            }
        }

        /// <summary>
        /// Adds the current date and time to a Document.
        /// </summary>
        /// <returns>true if successful, false otherwise</returns>
        virtual public bool AddCreationDate() {
            return Add(new Meta(Element.CREATIONDATE, DateTime.Now.ToString("ddd MMM dd HH:mm:ss zzz yyyy")));
        }

        // methods to get the layout of the document.

        /// <summary>
        /// Returns the left margin.
        /// </summary>
        /// <value>the left margin</value>
        virtual public float LeftMargin {
            get {
                return marginLeft;
            }
        }

        /// <summary>
        /// Return the right margin.
        /// </summary>
        /// <value>the right margin</value>
        virtual public float RightMargin {
            get {
                return marginRight;
            }
        }

        /// <summary>
        /// Returns the top margin.
        /// </summary>
        /// <value>the top margin</value>
        virtual public float TopMargin {
            get {
                return marginTop;
            }
        }

        /// <summary>
        /// Returns the bottom margin.
        /// </summary>
        /// <value>the bottom margin</value>
        virtual public float BottomMargin {
            get {
                return marginBottom;
            }
        }

        /// <summary>
        /// Returns the lower left x-coordinate.
        /// </summary>
        /// <value>the lower left x-coordinate</value>
        virtual public float Left {
            get {
                return pageSize.GetLeft(marginLeft);
            }
        }

        /// <summary>
        /// Returns the upper right x-coordinate.
        /// </summary>
        /// <value>the upper right x-coordinate.</value>
        virtual public float Right {
            get {
                return pageSize.GetRight(marginRight);
            }
        }

        /// <summary>
        /// Returns the upper right y-coordinate.
        /// </summary>
        /// <value>the upper right y-coordinate.</value>
        virtual public float Top {
            get {
                return pageSize.GetTop(marginTop);
            }
        }

        /// <summary>
        /// Returns the lower left y-coordinate.
        /// </summary>
        /// <value>the lower left y-coordinate.</value>
        virtual public float Bottom {
            get {
                return pageSize.GetBottom(marginBottom);
            }
        }

        /// <summary>
        /// Returns the lower left x-coordinate considering a given margin.
        /// </summary>
        /// <param name="margin">a margin</param>
        /// <returns>the lower left x-coordinate</returns>
        virtual public float GetLeft(float margin) {
            return pageSize.GetLeft(marginLeft + margin);
        }

        /// <summary>
        /// Returns the upper right x-coordinate, considering a given margin.
        /// </summary>
        /// <param name="margin">a margin</param>
        /// <returns>the upper right x-coordinate</returns>
        virtual public float GetRight(float margin) {
            return pageSize.GetRight(marginRight + margin);
        }

        /// <summary>
        /// Returns the upper right y-coordinate, considering a given margin.
        /// </summary>
        /// <param name="margin">a margin</param>
        /// <returns>the upper right y-coordinate</returns>
        virtual public float GetTop(float margin) {
            return pageSize.GetTop(marginTop + margin);
        }

        /// <summary>
        /// Returns the lower left y-coordinate, considering a given margin.
        /// </summary>
        /// <param name="margin">a margin</param>
        /// <returns>the lower left y-coordinate</returns>
        virtual public float GetBottom(float margin) {
            return pageSize.GetBottom(marginBottom + margin);
        }

        /// <summary>
        /// Gets the pagesize.
        /// </summary>
        /// <value>the page size</value>
        virtual public Rectangle PageSize {
            get {
                return this.pageSize;
            }
        }

        /// <summary>
        /// Checks if the document is open.
        /// </summary>
        /// <returns>true if the document is open</returns>
        virtual public bool IsOpen() {
            return open;
        }

        /// <summary>
        /// Gets the JavaScript onLoad command.
        /// </summary>
        /// <value>the JavaScript onLoad command.</value>
        virtual public string JavaScript_onLoad {
            get {
                return this.javaScript_onLoad;
            }

            set {
                this.javaScript_onLoad = value;
            }
        }

        /// <summary>
        /// Gets the JavaScript onUnLoad command.
        /// </summary>
        /// <value>the JavaScript onUnLoad command</value>
        virtual public string JavaScript_onUnLoad {
            get {
                return this.javaScript_onUnLoad;
            }

            set {
                this.javaScript_onUnLoad = value;
            }
        }

        /// <summary>
        /// Gets the style class of the HTML body tag
        /// </summary>
        /// <value>the style class of the HTML body tag</value>
        virtual public string HtmlStyleClass {
            get {
                return this.htmlStyleClass;
            }

            set {
                this.htmlStyleClass = value;
            }
        }

        /**
        * Set the margin mirroring. It will mirror right/left margins for odd/even pages.
        * <p>
        * Note: it will not work with {@link Table}.
        * 
        * @param marginMirroring
        *            <CODE>true</CODE> to mirror the margins
        * @return always <CODE>true</CODE>
        */    
        public virtual bool SetMarginMirroring(bool marginMirroring) {
            this.marginMirroring = marginMirroring;
            foreach (IDocListener listener in listeners) {
                listener.SetMarginMirroring(marginMirroring);
            }
            return true;
        }
        
        /**
        * Set the margin mirroring. It will mirror top/bottom margins for odd/even pages.
        * <p>
        * Note: it will not work with {@link Table}.
	    * 
	    * @param marginMirroringTopBottom
	    *            <CODE>true</CODE> to mirror the margins
        * @return always <CODE>true</CODE>
        * @since	2.1.6
        */    
        public virtual bool SetMarginMirroringTopBottom(bool marginMirroringTopBottom) {
            this.marginMirroringTopBottom = marginMirroringTopBottom;
            foreach (IDocListener listener in listeners) {
                listener.SetMarginMirroringTopBottom(marginMirroringTopBottom);
            }
            return true;
        }

        /**
        * Gets the margin mirroring flag.
        * 
        * @return the margin mirroring flag
        */    
        virtual public bool IsMarginMirroring() {
            return marginMirroring;
        }

        public virtual void Dispose() {
            if (IsOpen()) {
                Close();
            }
        }

        virtual public PdfObject GetAccessibleAttribute(PdfName key) {
            if (accessibleAttributes != null) {
                PdfObject value;
                accessibleAttributes.TryGetValue(key, out value);
                return value;
            } else
                return null;
        }

        virtual public void SetAccessibleAttribute(PdfName key, PdfObject value) {
            if (accessibleAttributes == null)
                accessibleAttributes = new Dictionary<PdfName, PdfObject>();
            accessibleAttributes[key] = value;
        }

        virtual public Dictionary<PdfName, PdfObject> GetAccessibleAttributes() {
            return accessibleAttributes;
        }

        virtual public PdfName Role {
            get {
                return role;
            }
            set {
                role = value;
            }
        }
        
        virtual public AccessibleElementId ID {
            get { return id; }
            set { id = value; }
        }

        public virtual bool IsInline {
            get { return false; }
        }
    }
}
