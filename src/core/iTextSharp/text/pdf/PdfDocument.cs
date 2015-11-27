using System;
using System.Collections.Generic;
using com.itextpdf.text.pdf;
using iTextSharp.text.api;
using iTextSharp.text.pdf.interfaces;
using iTextSharp.text.pdf.intern;
using iTextSharp.text.pdf.draw;
using iTextSharp.text.pdf.collection;
using iTextSharp.text.error_messages;
/*
 * 
 * $Id$
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
    * <CODE>PdfDocument</CODE> is the class that is used by <CODE>PdfWriter</CODE>
    * to translate a <CODE>Document</CODE> into a PDF with different pages.
    * <P>
    * A <CODE>PdfDocument</CODE> always listens to a <CODE>Document</CODE>
    * and adds the Pdf representation of every <CODE>Element</CODE> that is
    * added to the <CODE>Document</CODE>.
    *
    * @see      com.lowagie.text.Document
    * @see      com.lowagie.text.DocListener
    * @see      PdfWriter
    */

    public class PdfDocument : Document {
        
        /**
        * <CODE>PdfInfo</CODE> is the PDF InfoDictionary.
        * <P>
        * A document's trailer may contain a reference to an Info dictionary that provides information
        * about the document. This optional dictionary may contain one or more keys, whose values
        * should be strings.<BR>
        * This object is described in the 'Portable Document Format Reference Manual version 1.3'
        * section 6.10 (page 120-121)
        */
        
        public class PdfInfo : PdfDictionary {
            
            // constructors
            
            /**
            * Construct a <CODE>PdfInfo</CODE>-object.
            */
            
            internal PdfInfo() {
                AddProducer();
                AddCreationDate();
            }
            
            /**
            * Constructs a <CODE>PdfInfo</CODE>-object.
            *
            * @param        author      name of the author of the document
            * @param        title       title of the document
            * @param        subject     subject of the document
            */
            
            internal PdfInfo(String author, String title, String subject) : base() {
                AddTitle(title);
                AddSubject(subject);
                AddAuthor(author);
            }
            
            /**
            * Adds the title of the document.
            *
            * @param    title       the title of the document
            */
            
            internal void AddTitle(String title) {
                Put(PdfName.TITLE, new PdfString(title, PdfObject.TEXT_UNICODE));
            }
            
            /**
            * Adds the subject to the document.
            *
            * @param    subject     the subject of the document
            */
            
            internal void AddSubject(String subject) {
                Put(PdfName.SUBJECT, new PdfString(subject, PdfObject.TEXT_UNICODE));
            }
            
            /**
            * Adds some keywords to the document.
            *
            * @param    keywords        the keywords of the document
            */
            
            internal void AddKeywords(String keywords) {
                Put(PdfName.KEYWORDS, new PdfString(keywords, PdfObject.TEXT_UNICODE));
            }
            
            /**
            * Adds the name of the author to the document.
            *
            * @param    author      the name of the author
            */
            
            internal void AddAuthor(String author) {
                Put(PdfName.AUTHOR, new PdfString(author, PdfObject.TEXT_UNICODE));
            }
            
            /**
            * Adds the name of the creator to the document.
            *
            * @param    creator     the name of the creator
            */
            
            internal void AddCreator(String creator) {
                Put(PdfName.CREATOR, new PdfString(creator, PdfObject.TEXT_UNICODE));
            }
            
            /**
            * Adds the name of the producer to the document.
            */
            
            internal void AddProducer() {
                // This line may only be changed by Bruno Lowagie or Paulo Soares
                Put(PdfName.PRODUCER, new PdfString(Version.GetInstance().GetVersion));
                // Do not edit the line above!
            }
            
            /**
            * Adds the date of creation to the document.
            */
            
            internal void AddCreationDate() {
                PdfString date = new PdfDate();
                Put(PdfName.CREATIONDATE, date);
                Put(PdfName.MODDATE, date);
            }
            
            internal void Addkey(String key, String value) {
                if (key.Equals("Producer") || key.Equals("CreationDate"))
                    return;
                Put(new PdfName(key), new PdfString(value, PdfObject.TEXT_UNICODE));
            }
        }
        
        /**
        * <CODE>PdfCatalog</CODE> is the PDF Catalog-object.
        * <P>
        * The Catalog is a dictionary that is the root node of the document. It contains a reference
        * to the tree of pages contained in the document, a reference to the tree of objects representing
        * the document's outline, a reference to the document's article threads, and the list of named
        * destinations. In addition, the Catalog indicates whether the document's outline or thumbnail
        * page images should be displayed automatically when the document is viewed and wether some location
        * other than the first page should be shown when the document is opened.<BR>
        * In this class however, only the reference to the tree of pages is implemented.<BR>
        * This object is described in the 'Portable Document Format Reference Manual version 1.3'
        * section 6.2 (page 67-71)
        */
        
        internal class PdfCatalog : PdfDictionary {
            
            internal PdfWriter writer;
            // constructors
            
            /**
            * Constructs a <CODE>PdfCatalog</CODE>.
            *
            * @param        pages       an indirect reference to the root of the document's Pages tree.
            * @param writer the writer the catalog applies to
            */
            
            internal PdfCatalog(PdfIndirectReference pages, PdfWriter writer) : base(CATALOG) {
                this.writer = writer;
                Put(PdfName.PAGES, pages);
            }
            
            /**
            * Adds the names of the named destinations to the catalog.
            * @param localDestinations the local destinations
            * @param documentJavaScript the javascript used in the document
            * @param writer the writer the catalog applies to
            */
            internal void AddNames(SortedDictionary<string,Destination> localDestinations, Dictionary<String, PdfObject> documentLevelJS, Dictionary<String, PdfObject> documentFileAttachment, PdfWriter writer) {
                if (localDestinations.Count == 0 && documentLevelJS.Count == 0 && documentFileAttachment.Count == 0)
                    return;
                PdfDictionary names = new PdfDictionary();
                if (localDestinations.Count > 0) {
                    Dictionary<String, PdfObject> destmap = new Dictionary<String, PdfObject>();
                    foreach (String name in localDestinations.Keys) {
                        Destination dest;
                        if (!localDestinations.TryGetValue(name, out dest))
                            continue;
                        destmap[name] = dest.reference;
                    }
                    if (destmap.Count > 0) {
                        names.Put(PdfName.DESTS, writer.AddToBody(PdfNameTree.WriteTree(destmap, writer)).IndirectReference);
                    }
                }
                if (documentLevelJS.Count > 0) {
                    PdfDictionary tree = PdfNameTree.WriteTree(documentLevelJS, writer);
                    names.Put(PdfName.JAVASCRIPT, writer.AddToBody(tree).IndirectReference);
                }
                if (documentFileAttachment.Count > 0) {
                    names.Put(PdfName.EMBEDDEDFILES, writer.AddToBody(PdfNameTree.WriteTree(documentFileAttachment, writer)).IndirectReference);
                }
                if (names.Size > 0)
                    Put(PdfName.NAMES, writer.AddToBody(names).IndirectReference);
            }
            
            internal PdfAction OpenAction {
                set {
                    Put(PdfName.OPENACTION, value);
                }
            }
            
            
            /** Sets the document level additional actions.
            * @param actions   dictionary of actions
            */
            internal PdfDictionary AdditionalActions {
                set {
                    Put(PdfName.AA, writer.AddToBody(value).IndirectReference);
                }
            }
        }
        
    // CONSTRUCTING A PdfDocument/PdfWriter INSTANCE

        /**
        * Constructs a new PDF document.
        * @throws DocumentException on error
        */
        public PdfDocument() {
            AddProducer();
            AddCreationDate();
        }
        
        /** The <CODE>PdfWriter</CODE>. */
        protected internal PdfWriter writer;


        internal Dictionary<AccessibleElementId, PdfStructureElement> structElements = new Dictionary<AccessibleElementId, PdfStructureElement>();

        protected internal bool openMCDocument = false;

        protected Dictionary<Object, int[]> structParentIndices = new Dictionary<Object, int[]>();

        protected Dictionary<Object, int> markPoints = new Dictionary<Object, int>();

        /**
        * Adds a <CODE>PdfWriter</CODE> to the <CODE>PdfDocument</CODE>.
        *
        * @param writer the <CODE>PdfWriter</CODE> that writes everything
        *                     what is added to this document to an outputstream.
        * @throws DocumentException on error
        */
        virtual public void AddWriter(PdfWriter writer) {
            if (this.writer == null) {
                this.writer = writer;
                annotationsImp = new PdfAnnotationsImp(writer);
                return;
            }
            throw new DocumentException(MessageLocalization.GetComposedMessage("you.can.only.add.a.writer.to.a.pdfdocument.once"));
        }
        
    // LISTENER METHODS START
        
    //  [L0] ElementListener interface
        
        /** This is the PdfContentByte object, containing the text. */
        protected internal PdfContentByte text;
        
        /** This is the PdfContentByte object, containing the borders and other Graphics. */
        protected internal PdfContentByte graphics;
        
        /** This represents the leading of the lines. */
        protected internal float leading = 0;
        
        /**
        * Getter for the current leading.
        * @return  the current leading
        * @since   2.1.2
        */
        virtual public float Leading {
            get {
                return leading;
            }
            set {
                leading = value;
            }
        }
        /** This is the current height of the document. */
        protected internal float currentHeight = 0;
        
        /**
        * Signals that onParagraph is valid (to avoid that a Chapter/Section title is treated as a Paragraph).
        * @since 2.1.2
        */
        protected bool isSectionTitle = false;

        /** This represents the current alignment of the PDF Elements. */
        protected internal int alignment = Element.ALIGN_LEFT;
        
        /** The current active <CODE>PdfAction</CODE> when processing an <CODE>Anchor</CODE>. */
        protected internal PdfAction anchorAction = null;

        /**
         * The current tab settings.
         * @return	the current
         * @since 5.4.0
         */
        protected TabSettings tabSettings;

        /**
         * Signals that the current leading has to be subtracted from a YMark object when positive
         * and save current leading
         * @since 2.1.2
         */
        private Stack<float> leadingStack = new Stack<float>();

        private PdfBody body;

        /**
         * Save current @leading
         */
        virtual protected void PushLeading() {
            leadingStack.Push(leading);
        }

        /**
         * Restore @leading from leadingStack
         */
        virtual protected void PopLeading()
        {
            leading = leadingStack.Pop();
            if (leadingStack.Count > 0)
                leading = leadingStack.Peek();
        }

        /**
         * Getter and setter for the current tab stops.
         * @since	5.4.0
         */
        virtual public TabSettings TabSettings {
            get { return tabSettings; }
            set { tabSettings = value; }
        }

        /**
        * Signals that an <CODE>Element</CODE> was added to the <CODE>Document</CODE>.
        *
        * @param element the element to add
        * @return <CODE>true</CODE> if the element was added, <CODE>false</CODE> if not.
        * @throws DocumentException when a document isn't open yet, or has been closed
        */
        public override bool Add(IElement element) {
            if (writer != null && writer.IsPaused()) {
                return false;
            }
            try {
                if (element.Type != Element.DIV) {
                    FlushFloatingElements();
                }
                switch (element.Type) {

                        // Information (headers)
                    case Element.HEADER:
                        info.Addkey(((Meta) element).Name, ((Meta) element).Content);
                        break;
                    case Element.TITLE:
                        info.AddTitle(((Meta) element).Content);
                        break;
                    case Element.SUBJECT:
                        info.AddSubject(((Meta) element).Content);
                        break;
                    case Element.KEYWORDS:
                        info.AddKeywords(((Meta) element).Content);
                        break;
                    case Element.AUTHOR:
                        info.AddAuthor(((Meta) element).Content);
                        break;
                    case Element.CREATOR:
                        info.AddCreator(((Meta) element).Content);
                        break;
                    case Element.LANGUAGE:
                        SetLanguage(((Meta) element).Content);
                        break;
                    case Element.PRODUCER:
                        // you can not change the name of the producer
                        info.AddProducer();
                        break;
                    case Element.CREATIONDATE:
                        // you can not set the creation date, only reset it
                        info.AddCreationDate();
                        break;

                        // content (text)
                    case Element.CHUNK: {
                        // if there isn't a current line available, we make one
                        if (line == null) {
                            CarriageReturn();
                        }

                        // we cast the element to a chunk
                        PdfChunk chunk = new PdfChunk((Chunk) element, anchorAction, tabSettings);
                        // we try to add the chunk to the line, until we succeed
                        {
                            PdfChunk overflow;
                            while ((overflow = line.Add(chunk, leading)) != null) {
                                CarriageReturn();
                                bool newlineSplit = chunk.IsNewlineSplit();
                                chunk = overflow;
                                if (!newlineSplit)
                                    chunk.TrimFirstSpace();
                            }
                        }
                        pageEmpty = false;
                        if (chunk.IsAttribute(Chunk.NEWPAGE)) {
                            NewPage();
                        }
                        break;
                    }
                    case Element.ANCHOR: {
                        Anchor anchor = (Anchor) element;
                        String url = anchor.Reference;
                        leading = anchor.Leading;
                        PushLeading();
                        if (url != null) {
                            anchorAction = new PdfAction(url);
                        }

                        // we process the element
                        element.Process(this);
                        anchorAction = null;
                        PopLeading();
                        break;
                    }
                    case Element.ANNOTATION: {
                        if (line == null) {
                            CarriageReturn();
                        }
                        Annotation annot = (Annotation) element;
                        Rectangle rect = new Rectangle(0, 0);
                        if (line != null)
                            rect = new Rectangle(annot.GetLlx(IndentRight - line.WidthLeft),
                                annot.GetUry(IndentTop - currentHeight - 20), annot.GetUrx(IndentRight - line.WidthLeft + 20),
                                annot.GetLly(IndentTop - currentHeight));
                        PdfAnnotation an = PdfAnnotationsImp.ConvertAnnotation(writer, annot, rect);
                        annotationsImp.AddPlainAnnotation(an);
                        pageEmpty = false;
                        break;
                    }
                    case Element.PHRASE: {
                        TabSettings backupTabSettings = tabSettings;
                        if (((Phrase) element).TabSettings != null)
                            tabSettings = ((Phrase) element).TabSettings;

                        // we cast the element to a phrase and set the leading of the document
                        leading = ((Phrase) element).TotalLeading;
                        PushLeading();
                        // we process the element
                        element.Process(this);
                        tabSettings = backupTabSettings;
                        PopLeading();
                        break;
                    }
                    case Element.PARAGRAPH: {
                        TabSettings backupTabSettings = tabSettings;
                        if (((Phrase) element).TabSettings != null)
                            tabSettings = ((Phrase) element).TabSettings;

                        // we cast the element to a paragraph
                        Paragraph paragraph = (Paragraph) element;
                        if (IsTagged(writer)) {
                            FlushLines();
                            text.OpenMCBlock(paragraph);
                        }
                        AddSpacing(paragraph.SpacingBefore, leading, paragraph.Font);

                        // we adjust the parameters of the document
                        alignment = paragraph.Alignment;
                        leading = paragraph.TotalLeading;
                        PushLeading();

                        CarriageReturn();
                        // we don't want to make orphans/widows
                        if (currentHeight + CalculateLineHeight() > IndentTop - IndentBottom) {
                            NewPage();
                        }

                        indentation.indentLeft += paragraph.IndentationLeft;
                        indentation.indentRight += paragraph.IndentationRight;

                        CarriageReturn();

                        IPdfPageEvent pageEvent = writer.PageEvent;
                        if (pageEvent != null && !isSectionTitle)
                            pageEvent.OnParagraph(writer, this, IndentTop - currentHeight);

                        // if a paragraph has to be kept together, we wrap it in a table object
                        if (paragraph.KeepTogether) {
                            CarriageReturn();
                            PdfPTable table = new PdfPTable(1);
                            table.KeepTogether = paragraph.KeepTogether;
                            table.WidthPercentage = 100f;
                            PdfPCell cell = new PdfPCell();
                            cell.AddElement(paragraph);
                            cell.Border = Rectangle.NO_BORDER;
                            cell.Padding = 0;
                            table.AddCell(cell);
                            indentation.indentLeft -= paragraph.IndentationLeft;
                            indentation.indentRight -= paragraph.IndentationRight;
                            this.Add(table);
                            indentation.indentLeft += paragraph.IndentationLeft;
                            indentation.indentRight += paragraph.IndentationRight;
                        } else {
                            line.SetExtraIndent(paragraph.FirstLineIndent);
                            element.Process(this);
                            CarriageReturn();
                            AddSpacing(paragraph.SpacingAfter, paragraph.TotalLeading, paragraph.Font, true);
                        }

                        if (pageEvent != null && !isSectionTitle)
                            pageEvent.OnParagraphEnd(writer, this, IndentTop - currentHeight);

                        alignment = Element.ALIGN_LEFT;
                        if (floatingElements != null && floatingElements.Count != 0) {
                            FlushFloatingElements();
                        }
                        indentation.indentLeft -= paragraph.IndentationLeft;
                        indentation.indentRight -= paragraph.IndentationRight;
                        CarriageReturn();
                        tabSettings = backupTabSettings;
                        PopLeading();
                        if (IsTagged(writer)) {
                            FlushLines();
                            text.CloseMCBlock(paragraph);
                        }
                        break;
                    }
                    case Element.SECTION:
                    case Element.CHAPTER: {
                        // Chapters and Sections only differ in their constructor
                        // so we cast both to a Section
                        Section section = (Section) element;
                        IPdfPageEvent pageEvent = writer.PageEvent;

                        bool hasTitle = section.NotAddedYet && section.Title != null;

                        // if the section is a chapter, we begin a new page
                        if (section.TriggerNewPage) {
                            NewPage();
                        }

                        if (hasTitle) {
                            float fith = IndentTop - currentHeight;
                            int rotation = pageSize.Rotation;
                            if (rotation == 90 || rotation == 180)
                                fith = pageSize.Height - fith;
                            PdfDestination destination = new PdfDestination(PdfDestination.FITH, fith);
                            while (currentOutline.Level >= section.Depth) {
                                currentOutline = currentOutline.Parent;
                            }
                            PdfOutline outline = new PdfOutline(currentOutline, destination, section.GetBookmarkTitle(),
                                section.BookmarkOpen);
                            currentOutline = outline;
                        }

                        // some values are set
                        CarriageReturn();
                        indentation.sectionIndentLeft += section.IndentationLeft;
                        indentation.sectionIndentRight += section.IndentationRight;
                        if (section.NotAddedYet && pageEvent != null)
                            if (element.Type == Element.CHAPTER)
                                pageEvent.OnChapter(writer, this, IndentTop - currentHeight, section.Title);
                            else
                                pageEvent.OnSection(writer, this, IndentTop - currentHeight, section.Depth, section.Title);

                        // the title of the section (if any has to be printed)
                        if (hasTitle) {
                            isSectionTitle = true;
                            Add(section.Title);
                            isSectionTitle = false;
                        }
                        indentation.sectionIndentLeft += section.Indentation;
                        // we process the section
                        element.Process(this);
                        // some parameters are set back to normal again
                        indentation.sectionIndentLeft -= (section.IndentationLeft + section.Indentation);
                        indentation.sectionIndentRight -= section.IndentationRight;

                        if (section.ElementComplete && pageEvent != null)
                            if (element.Type == Element.CHAPTER)
                                pageEvent.OnChapterEnd(writer, this, IndentTop - currentHeight);
                            else
                                pageEvent.OnSectionEnd(writer, this, IndentTop - currentHeight);

                        break;
                    }
                    case Element.LIST: {
                        // we cast the element to a List
                        List list = (List) element;
                        if (IsTagged(writer)) {
                            FlushLines();
                            text.OpenMCBlock(list);
                        }
                        if (list.Alignindent) {
                            list.NormalizeIndentation();
                        }
                        // we adjust the document
                        indentation.listIndentLeft += list.IndentationLeft;
                        indentation.indentRight += list.IndentationRight;
                        // we process the items in the list
                        element.Process(this);
                        // some parameters are set back to normal again
                        indentation.listIndentLeft -= list.IndentationLeft;
                        indentation.indentRight -= list.IndentationRight;
                        CarriageReturn();
                        if (IsTagged(writer)) {
                            FlushLines();
                            text.CloseMCBlock(list);
                        }
                        break;
                    }
                    case Element.LISTITEM: {
                        // we cast the element to a ListItem
                        ListItem listItem = (ListItem) element;
                        if (IsTagged(writer)) {
                            FlushLines();
                            text.OpenMCBlock(listItem);
                        }
                        AddSpacing(listItem.SpacingBefore, leading, listItem.Font);

                        // we adjust the document
                        alignment = listItem.Alignment;
                        indentation.listIndentLeft += listItem.IndentationLeft;
                        indentation.indentRight += listItem.IndentationRight;
                        leading = listItem.TotalLeading;
                        PushLeading();
                        CarriageReturn();
                        // we prepare the current line to be able to show us the listsymbol
                        line.ListItem = listItem;
                        // we process the item
                        element.Process(this);

                        AddSpacing(listItem.SpacingAfter, listItem.TotalLeading, listItem.Font, true);

                        // if the last line is justified, it should be aligned to the left
                        if (line.HasToBeJustified()) {
                            line.ResetAlignment();
                        }
                        // some parameters are set back to normal again
                        CarriageReturn();
                        indentation.listIndentLeft -= listItem.IndentationLeft;
                        indentation.indentRight -= listItem.IndentationRight;
                        PopLeading();
                        if (IsTagged(writer)) {
                            FlushLines();
                            text.CloseMCBlock(listItem.ListBody);
                            text.CloseMCBlock(listItem);
                        }
                        break;
                    }
                    case Element.RECTANGLE: {
                        Rectangle rectangle = (Rectangle) element;
                        graphics.Rectangle(rectangle);
                        pageEmpty = false;
                        break;
                    }
                    case Element.PTABLE: {
                        PdfPTable ptable = (PdfPTable) element;
                        if (ptable.Size <= ptable.HeaderRows)
                            break; //nothing to do

                        // before every table, we add a new line and flush all lines
                        EnsureNewLine();
                        FlushLines();

                        AddPTable(ptable);
                        pageEmpty = false;
                        NewLine();
                        break;
                    }
                    case Element.JPEG:
                    case Element.JPEG2000:
                    case Element.JBIG2:
                    case Element.IMGRAW:
                    case Element.IMGTEMPLATE: {
                        //carriageReturn(); suggestion by Marc Campforts
                        if (IsTagged(writer) && !((Image) element).IsImgTemplate()) {
                            FlushLines();
                            text.OpenMCBlock((Image) element);
                        }
                        Add((Image) element);
                        if (IsTagged(writer) && !((Image) element).IsImgTemplate()) {
                            FlushLines();
                            text.CloseMCBlock((Image) element);
                        }
                        break;
                    }
                    case Element.YMARK: {
                        IDrawInterface zh = (IDrawInterface) element;
                        zh.Draw(graphics, IndentLeft, IndentBottom, IndentRight, IndentTop,
                            IndentTop - currentHeight - (leadingStack.Count > 0 ? leading : 0));
                        pageEmpty = false;
                        break;
                    }
                    case Element.MARKED: {
                        MarkedObject mo;
                        if (element is MarkedSection) {
                            mo = ((MarkedSection) element).Title;
                            if (mo != null) {
                                mo.Process(this);
                            }
                        }
                        mo = (MarkedObject) element;
                        mo.Process(this);
                        break;
                    }
                    case Element.WRITABLE_DIRECT:
                        if (null != writer) {
                            ((IWriterOperation) element).Write(writer, this);
                        }
                        break;
                    case Element.DIV:
                        EnsureNewLine();
                        FlushLines();
                        AddDiv((PdfDiv) element);
                        pageEmpty = false;
                        //newLine();
                        break;
                    case Element.BODY:
                        body = (PdfBody)element;
                        graphics.Rectangle(body);
                        break;
                    default:
                        return false;
                }
                lastElementType = element.Type;
                return true;
            } catch (Exception e) {
                throw new DocumentException(e.Message);
            }
        }
        
    //  [L1] DocListener interface
        
        /**
        * Opens the document.
        * <P>
        * You have to open the document before you can begin to add content
        * to the body of the document.
        */
        public override void Open() {
            if (!open) {
                base.Open();
                writer.Open();
                rootOutline = new PdfOutline(writer);
                currentOutline = rootOutline;
            }
            if (IsTagged(writer))
                openMCDocument = true;
            InitPage();
        }
        
    //  [L2] DocListener interface
    
        /**
        * Closes the document.
        * <B>
        * Once all the content has been written in the body, you have to close
        * the body. After that nothing can be written to the body anymore.
        */        
        public override void Close() {
            if (close) {
                return;
            }
            if (IsTagged(writer)) {
                FlushFloatingElements();
                FlushLines();
                writer.FlushAcroFields();
                writer.FlushTaggedObjects();
                if (PageEmpty) {
                    int pageReferenceCount = writer.pageReferences.Count;
                    if (pageReferenceCount > 0 && writer.CurrentPageNumber == pageReferenceCount) {
                        writer.pageReferences.RemoveAt(pageReferenceCount - 1);
                    }
                }
            } else
                writer.FlushAcroFields();
            if (imageWait != null) NewPage();
            EndPage();
            if (IsTagged(writer)) {
                writer.DirectContent.CloseMCBlock(this);
            }
            if (annotationsImp.HasUnusedAnnotations())
                throw new Exception(MessageLocalization.GetComposedMessage("not.all.annotations.could.be.added.to.the.document.the.document.doesn.t.have.enough.pages"));
            IPdfPageEvent pageEvent = writer.PageEvent;
            if (pageEvent != null)
                pageEvent.OnCloseDocument(writer, this);
            base.Close();
            
            writer.AddLocalDestinations(localDestinations);
            CalculateOutlineCount();
            WriteOutlines();
            
            writer.Close();
        }
    
    //  [L3] DocListener interface

        protected internal int textEmptySize;

        // [C9] Metadata for the page
        /**
         * Use this method to set the XMP Metadata.
         * @param xmpMetadata The xmpMetadata to set.
         * @throws IOException 
         */
        virtual public byte[] XmpMetadata {
            set {
                PdfStream xmp = new PdfStream(value);
                xmp.Put(PdfName.TYPE, PdfName.METADATA);
                xmp.Put(PdfName.SUBTYPE, PdfName.XML);
                PdfEncryption crypto = writer.Encryption;
                if (crypto != null && !crypto.IsMetadataEncrypted()) {
                    PdfArray ar = new PdfArray();
                    ar.Add(PdfName.CRYPT);
                    xmp.Put(PdfName.FILTER, ar);
                }
                writer.AddPageDictEntry(PdfName.METADATA, writer.AddToBody(xmp).IndirectReference);
            }
        }

        /**
        * Makes a new page and sends it to the <CODE>PdfWriter</CODE>.
        *
        * @return true if new page was added
        * @throws DocumentException on error
        */
        public override bool NewPage() {
            if (PageEmpty) {
                SetNewPageSizeAndMargins();
                return false;
            }
            if (!open || close) {
                throw new Exception(MessageLocalization.GetComposedMessage("the.document.is.not.open"));
            }

            //we end current page
            IList<IAccessibleElement> savedMcBlocks = EndPage();
            
            //Added to inform any listeners that we are moving to a new page (added by David Freels)
            base.NewPage();
            
            // the following 2 lines were added by Pelikan Stephan
            indentation.imageIndentLeft = 0;
            indentation.imageIndentRight = 0;
            
            if (IsTagged(writer)) {
                // Java vs C# difference here because isToUseExternalCache is not ported into C#.
                writer.DirectContentUnder.RestoreMCBlocks(savedMcBlocks);
            }

            // we initialize the new page
            InitPage();

            if (body != null && body.BackgroundColor != null)
                graphics.Rectangle(body);

            return true;
        }

        public IList<IAccessibleElement> EndPage() {
            if (PageEmpty) {
                return null;
            }

            IList<IAccessibleElement> savedMcBlocks = null;

            FlushFloatingElements();
            lastElementType = -1;

            IPdfPageEvent pageEvent = writer.PageEvent;
            if (pageEvent != null)
                pageEvent.OnEndPage(writer, this);

            // we flush the arraylist with recently written lines
            FlushLines();
            // we prepare the elements of the page dictionary

            // [U1] page size and rotation
            int rotation = pageSize.Rotation;

            // [C10]
            if (writer.IsPdfIso()) {
                if (thisBoxSize.ContainsKey("art") && thisBoxSize.ContainsKey("trim"))
                    throw new PdfXConformanceException(MessageLocalization.GetComposedMessage("only.one.of.artbox.or.trimbox.can.exist.in.the.page"));
                if (!thisBoxSize.ContainsKey("art") && !thisBoxSize.ContainsKey("trim")) {
                    if (thisBoxSize.ContainsKey("crop"))
                        thisBoxSize["trim"] = thisBoxSize["crop"];
                    else
                        thisBoxSize["trim"] = new PdfRectangle(pageSize, pageSize.Rotation);
                }
            }

            // [M1]
            pageResources.AddDefaultColorDiff(writer.DefaultColorspace);
            if (writer.RgbTransparencyBlending) {
                PdfDictionary dcs = new PdfDictionary();
                dcs.Put(PdfName.CS, PdfName.DEVICERGB);
                pageResources.AddDefaultColorDiff(dcs);
            }
            PdfDictionary resources = pageResources.Resources;

            // we create the page dictionary

            PdfPage page = new PdfPage(new PdfRectangle(pageSize, rotation), thisBoxSize, resources, rotation);
            if (IsTagged(writer)) {
                page.Put(PdfName.TABS, PdfName.S);
            } else {
                page.Put(PdfName.TABS, writer.Tabs);
            }
            page.Merge(writer.PageDictEntries);
            writer.ResetPageDictEntries();

            // we complete the page dictionary

            // [U3] page actions: additional actions
            if (pageAA != null) {
                page.Put(PdfName.AA, writer.AddToBody(pageAA).IndirectReference);
                pageAA = null;
            }

            // [C5] and [C8] we add the annotations
            if (annotationsImp.HasUnusedAnnotations()) {
                PdfArray array = annotationsImp.RotateAnnotations(writer, pageSize);
                if (array.Size != 0)
                    page.Put(PdfName.ANNOTS, array);
            }

            // [F12] we add tag info
            if (IsTagged(writer))
                page.Put(PdfName.STRUCTPARENTS, new PdfNumber(GetStructParentIndex(writer.CurrentPage)));

            if (text.Size > textEmptySize || IsTagged(writer))
                text.EndText();
            else
                text = null;
            if (IsTagged(writer)) {
                savedMcBlocks = writer.DirectContent.SaveMCBlocks();
            }
            writer.Add(page, new PdfContents(writer.DirectContentUnder, graphics, !IsTagged(writer) ? text : null, writer.DirectContent, pageSize));

            annotationsImp.ResetAnnotations();
            writer.ResetContent();

            return savedMcBlocks;
        }

    //  [L4] DocListener interface

        /**
        * Sets the pagesize.
        *
        * @param pageSize the new pagesize
        * @return <CODE>true</CODE> if the page size was set
        */
        public override bool SetPageSize(Rectangle pageSize) {
            if (writer != null && writer.IsPaused()) {
                return false;
            }
            nextPageSize = new Rectangle(pageSize);
            return true;
        }
        

        /** margin in x direction starting from the left. Will be valid in the next page */
        protected float nextMarginLeft;
        
        /** margin in x direction starting from the right. Will be valid in the next page */
        protected float nextMarginRight;
        
        /** margin in y direction starting from the top. Will be valid in the next page */
        protected float nextMarginTop;
        
        /** margin in y direction starting from the bottom. Will be valid in the next page */
        protected float nextMarginBottom;

        /**
        * Sets the margins.
        *
        * @param    marginLeft      the margin on the left
        * @param    marginRight     the margin on the right
        * @param    marginTop       the margin on the top
        * @param    marginBottom    the margin on the bottom
        * @return   a <CODE>bool</CODE>
        */
        public override bool SetMargins(float marginLeft, float marginRight, float marginTop, float marginBottom) {
            if (writer != null && writer.IsPaused()) {
                return false;
            }
            nextMarginLeft = marginLeft;
            nextMarginRight = marginRight;
            nextMarginTop = marginTop;
            nextMarginBottom = marginBottom;
            return true;
        }
        
    //  [L6] DocListener interface
        
        /**
        * @see com.lowagie.text.DocListener#setMarginMirroring(bool)
        */
        public override bool SetMarginMirroring(bool MarginMirroring) {
            if (writer != null && writer.IsPaused()) {
                return false;
            }
            return base.SetMarginMirroring(MarginMirroring);
        }

       /**
        * @see com.lowagie.text.DocListener#setMarginMirroring(boolean)
        * @since    2.1.6
        */
        public override bool SetMarginMirroringTopBottom(bool MarginMirroringTopBottom) {
            if (writer != null && writer.IsPaused()) {
                return false;
            }
            return base.SetMarginMirroringTopBottom(MarginMirroringTopBottom);
        }
    //  [L7] DocListener interface
        
        /**
        * Sets the page number.
        *
        * @param    pageN       the new page number
        */
        public override int PageCount {
            set {
                if (writer != null && writer.IsPaused()) {
                    return;
                }
                base.PageCount = value;
            }
        }
        
    //  [L8] DocListener interface
        
        /**
        * Sets the page number to 0.
        */
        public override void ResetPageCount() {
            if (writer != null && writer.IsPaused()) {
                return;
            }
            base.ResetPageCount();
        }

        // DOCLISTENER METHODS END
    
        /** Signals that OnOpenDocument should be called. */
        protected internal bool firstPageEvent = true;
    
        /**
        * Initializes a page.
        * <P>
        * If the footer/header is set, it is printed.
        * @throws DocumentException on error
        */
        
        virtual protected internal void InitPage() {
            // the pagenumber is incremented
            pageN++;
            
            // initialisation of some page objects
            pageResources = new PageResources();

            if (IsTagged(writer)) {
                graphics = writer.DirectContentUnder.Duplicate;
                writer.DirectContent.duplicatedFrom = graphics;
            } else{
                graphics = new PdfContentByte(writer);
            }

            SetNewPageSizeAndMargins();
            imageEnd = -1;
            indentation.imageIndentRight = 0;
            indentation.imageIndentLeft = 0;
            indentation.indentBottom = 0;
            indentation.indentTop = 0;
            currentHeight = 0;
            
            // backgroundcolors, etc...
            thisBoxSize = new Dictionary<String, PdfRectangle>(boxSize);
            if (pageSize.BackgroundColor != null
            || pageSize.HasBorders()
            || pageSize.BorderColor != null) {
                Add(pageSize);
            }

            float oldleading = leading;
            int oldAlignment = alignment;
            pageEmpty = true;
            // if there is an image waiting to be drawn, draw it
            if (imageWait != null) {
                Add(imageWait);
                imageWait = null;
            }
            leading = oldleading;
            alignment = oldAlignment;
            CarriageReturn();
            
            IPdfPageEvent pageEvent = writer.PageEvent;
            if (pageEvent != null) {
                if (firstPageEvent) {
                    pageEvent.OnOpenDocument(writer, this);
                }
                pageEvent.OnStartPage(writer, this);
            }
            firstPageEvent = false;
        }

        /** The line that is currently being written. */
        protected internal PdfLine line = null;
        
        /** The lines that are written until now. */
        protected internal List<PdfLine> lines = new List<PdfLine>();
        
        /**
        * Adds the current line to the list of lines and also adds an empty line.
        * @throws DocumentException on error
        */
        
        virtual protected internal void NewLine() {
            lastElementType = -1;
            CarriageReturn();
            if (lines != null && lines.Count > 0) {
                lines.Add(line);
                currentHeight += line.Height;
            }
            line = new PdfLine(IndentLeft, IndentRight, alignment, leading);
        }

        /**
         * line.height() is usually the same as the leading
         * We should take leading into account if it is not the same as the line.height
         *
         * @return float combined height of the line
         * @since 5.5.1
         */
        protected virtual float CalculateLineHeight() {
            float tempHeight = line.Height;

            if (tempHeight != leading) {
                tempHeight += leading;
            }

            return tempHeight;
        }
        
        /**
        * If the current line is not empty or null, it is added to the arraylist
        * of lines and a new empty line is added.
        * @throws DocumentException on error
        */
        virtual protected internal void CarriageReturn() {
            // the arraylist with lines may not be null
            if (lines == null) {
                lines = new List<PdfLine>();
            }
            // If the current line is not null or empty
            if (line != null && line.Size > 0) {
                // we check if the end of the page is reached (bugfix by Francois Gravel)
                if (currentHeight + CalculateLineHeight() > IndentTop - IndentBottom) {
                    // if the end of the line is reached, we start a newPage which will flush existing lines
                    // then move to next page but before then we need to exclude the current one that does not fit
                    // After the new page we add the current line back in
                    if (currentHeight != 0) {
                        PdfLine overflowLine = line;
                        line = null;
                        NewPage();
                        line = overflowLine;
                        //update left indent because of mirror margins.
                        overflowLine.left = IndentLeft;
                    }
                }
                currentHeight += line.Height;
                lines.Add(line);
                pageEmpty = false;
            }
            if (imageEnd > -1 && currentHeight > imageEnd) {
                imageEnd = -1;
                indentation.imageIndentRight = 0;
                indentation.imageIndentLeft = 0;
            }
            // a new current line is constructed
            line = new PdfLine(IndentLeft, IndentRight, alignment, leading);
        }
        
        /**
        * Gets the current vertical page position.
        * @param ensureNewLine Tells whether a new line shall be enforced. This may cause side effects 
        *   for elements that do not terminate the lines they've started because those lines will get
        *   terminated. 
        * @return The current vertical page position.
        */
        virtual public float GetVerticalPosition(bool ensureNewLine) {
            // ensuring that a new line has been started.
            if (ensureNewLine) {
                EnsureNewLine();
            }
            return Top - currentHeight - indentation.indentTop;
        }

        /** Holds the type of the last element, that has been added to the document. */
        protected internal int lastElementType = -1;    

        /**
        * Ensures that a new line has been started. 
        */
        virtual protected internal void EnsureNewLine() {
            if ((lastElementType == Element.PHRASE) || 
                (lastElementType == Element.CHUNK)) {
                NewLine();
                FlushLines();
            }
        }
        
        /**
        * Writes all the lines to the text-object.
        *
        * @return the displacement that was caused
        * @throws DocumentException on error
        */
        virtual protected internal float FlushLines() {
            // checks if the ArrayList with the lines is not null
            if (lines == null) {
                return 0;
            }
            // checks if a new Line has to be made.
            if (line != null && line.Size > 0) {
                lines.Add(line);
                line = new PdfLine(IndentLeft, IndentRight, alignment, leading);
            }
            
            // checks if the ArrayList with the lines is empty
            if (lines.Count == 0) {
                return 0;
            }
            
            // initialisation of some parameters
            Object[] currentValues = new Object[2];
            PdfFont currentFont = null;
            float displacement = 0;

            currentValues[1] = (float)0;
            // looping over all the lines
            foreach (PdfLine l in lines) {
                
                // this is a line in the loop
                
                float moveTextX = l.IndentLeft - IndentLeft + indentation.indentLeft + indentation.listIndentLeft + indentation.sectionIndentLeft;
                text.MoveText(moveTextX, -l.Height);
                // is the line preceeded by a symbol?
                l.Flush();

                if (l.ListSymbol != null) {
                    ListLabel lbl = null;
                    Chunk symbol = l.ListSymbol;
                    if (IsTagged(writer))
                    {
                        lbl = l.listItem.ListLabel;
                        graphics.OpenMCBlock(lbl);
                        symbol = new Chunk(symbol);
                        symbol.Role = null;
                    }
                    ColumnText.ShowTextAligned(graphics, Element.ALIGN_LEFT, new Phrase(symbol), text.XTLM - l.ListIndent, text.YTLM, 0);
                    if (lbl != null)
                    {
                        graphics.CloseMCBlock(lbl);
                    }
                }
                
                currentValues[0] = currentFont;

                if (IsTagged(writer) && l.ListItem != null)
                    text.OpenMCBlock(l.listItem.ListBody);

                WriteLineToContent(l, text, graphics, currentValues, writer.SpaceCharRatio);
                
                currentFont = (PdfFont)currentValues[0];
                
                displacement += l.Height;
                text.MoveText(-moveTextX, 0);
            }
            lines = new List<PdfLine>();
            return displacement;
        }

        /** The characters to be applied the hanging punctuation. */
        internal const String hangingPunctuation = ".,;:'";
        
        /**
        * Writes a text line to the document. It takes care of all the attributes.
        * <P>
        * Before entering the line position must have been established and the
        * <CODE>text</CODE> argument must be in text object scope (<CODE>beginText()</CODE>).
        * @param line the line to be written
        * @param text the <CODE>PdfContentByte</CODE> where the text will be written to
        * @param graphics the <CODE>PdfContentByte</CODE> where the graphics will be written to
        * @param currentValues the current font and extra spacing values
        * @param ratio
        * @throws DocumentException on error
        */
        internal float WriteLineToContent(PdfLine line, PdfContentByte text, PdfContentByte graphics, Object[] currentValues, float ratio)  {
            PdfFont currentFont = (PdfFont)(currentValues[0]);
            float lastBaseFactor = (float)currentValues[1];
            //PdfChunk chunkz;
            int numberOfSpaces;
            int lineLen;
            bool isJustified;
            float hangingCorrection = 0;
            float hScale = 1;
            float lastHScale = float.NaN;
            float baseWordSpacing = 0;
            float baseCharacterSpacing = 0;
            float glueWidth = 0;
            float lastX = text.XTLM + line.OriginalWidth;
            
            numberOfSpaces = line.NumberOfSpaces;
            lineLen = line.GetLineLengthUtf32();
            // does the line need to be justified?
            isJustified = line.HasToBeJustified() && (numberOfSpaces != 0 || lineLen > 1);
            int separatorCount = line.GetSeparatorCount();
            if (separatorCount > 0) {
                glueWidth = line.WidthLeft / separatorCount;
            }
            else if (isJustified && separatorCount == 0) {
                if (line.NewlineSplit && line.WidthLeft >= (lastBaseFactor * (ratio * numberOfSpaces + lineLen - 1))) {
                    if (line.RTL) {
                        text.MoveText(line.WidthLeft - lastBaseFactor * (ratio * numberOfSpaces + lineLen - 1), 0);
                    }
                    baseWordSpacing = ratio * lastBaseFactor;
                    baseCharacterSpacing = lastBaseFactor;
                }
                else {
                    float width = line.WidthLeft;
                    PdfChunk last = line.GetChunk(line.Size - 1);
                    if (last != null) {
                        String s = last.ToString();
                        char c;
                        if (s.Length > 0 && hangingPunctuation.IndexOf((c = s[s.Length - 1])) >= 0) {
                            float oldWidth = width;
                            width += last.Font.Width(c) * 0.4f;
                            hangingCorrection = width - oldWidth;
                        }
                    }
                    float baseFactor = width / (ratio * numberOfSpaces + lineLen - 1);
                    baseWordSpacing = ratio * baseFactor;
                    baseCharacterSpacing = baseFactor;
                    lastBaseFactor = baseFactor;
                }
            } else if (line.alignment == Element.ALIGN_LEFT || line.alignment == Element.ALIGN_UNDEFINED) {
                lastX -= line.WidthLeft;
            }
            
            int lastChunkStroke = line.LastStrokeChunk;
            int chunkStrokeIdx = 0;
            float xMarker = text.XTLM;
            float baseXMarker = xMarker;
            float yMarker = text.YTLM;
            bool adjustMatrix = false;
            float tabPosition = 0;

            bool isMCBlockOpened = false;
            // looping over all the chunks in 1 line
            foreach (PdfChunk chunk in line) {
                if (IsTagged(writer) && chunk.accessibleElement != null) {
                    text.OpenMCBlock(chunk.accessibleElement);
                    isMCBlockOpened = true;
                }
                BaseColor color = chunk.Color;
                float fontSize = chunk.Font.Size;
                float ascender;
                float descender;
                if (chunk.IsImage())
                {
                    ascender = chunk.Height();
                    descender = 0;
                }
                else
                {
                    ascender = chunk.Font.Font.GetFontDescriptor(BaseFont.ASCENT, fontSize);
                    descender = chunk.Font.Font.GetFontDescriptor(BaseFont.DESCENT, fontSize);
                }
                hScale = 1;
                
                if (chunkStrokeIdx <= lastChunkStroke) {
                    float width;
                    if (isJustified) {
                        width = chunk.GetWidthCorrected(baseCharacterSpacing, baseWordSpacing);
                    }
                    else {
                        width = chunk.Width();
                    }
                    if (chunk.IsStroked()) {
                        PdfChunk nextChunk = line.GetChunk(chunkStrokeIdx + 1);
                        if (chunk.IsSeparator()) {
                            width = glueWidth;
                            Object[] sep = (Object[])chunk.GetAttribute(Chunk.SEPARATOR);
                            IDrawInterface di = (IDrawInterface)sep[0];
                            bool vertical = (bool)sep[1];
                            if (vertical) {
                                di.Draw(graphics, baseXMarker, yMarker + descender, baseXMarker + line.OriginalWidth, ascender - descender, yMarker);      
                            }
                            else {
                                di.Draw(graphics, xMarker, yMarker + descender, xMarker + width, ascender - descender, yMarker);
                            }
                        }
                        if (chunk.IsTab()) {
                            if (chunk.IsAttribute(Chunk.TABSETTINGS))
                            {
                                TabStop tabStop = chunk.TabStop;
                                if (tabStop != null) {
                                    tabPosition = tabStop.Position + baseXMarker;
                                    if (tabStop.Leader != null)
                                        tabStop.Leader.Draw(graphics, xMarker, yMarker + descender, tabPosition, ascender - descender, yMarker);
                                }
                                else {
                                    tabPosition = xMarker;
                                }
                            } else {
                                //Keep deprecated tab logic for backward compatibility...
                                Object[] tab = (Object[])chunk.GetAttribute(Chunk.TAB);
                                IDrawInterface di = (IDrawInterface)tab[0];
                                tabPosition = (float)tab[1] + (float)tab[3];
                                if (tabPosition > xMarker)
                                    di.Draw(graphics, xMarker, yMarker + descender, tabPosition, ascender - descender, yMarker);
                            }
                            float tmp = xMarker;
                            xMarker = tabPosition;
                            tabPosition = tmp;
                        }
                        if (chunk.IsAttribute(Chunk.BACKGROUND)) {
                             Object[] bgr = (Object[])chunk.GetAttribute(Chunk.BACKGROUND);
                            if (bgr[0] != null) {
                                bool inText = graphics.InText;
                                if (inText && IsTagged(writer)) {
                                    graphics.EndText();
                                }
                                graphics.SaveState();
                                float subtract = lastBaseFactor;
                                if (nextChunk != null && nextChunk.IsAttribute(Chunk.BACKGROUND)) {
                                    subtract = 0;
                                }
                                if (nextChunk == null) {
                                    subtract += hangingCorrection;
                                }
                                BaseColor c = (BaseColor)bgr[0];
                                graphics.SetColorFill(c);
                                float[] extra = (float[]) bgr[1];
                                graphics.Rectangle(xMarker - extra[0],
                                        yMarker + descender - extra[1] + chunk.TextRise,
                                        width - subtract + extra[0] + extra[2],
                                        ascender - descender + extra[1] + extra[3]);
                                graphics.Fill();
                                graphics.SetGrayFill(0);
                                graphics.RestoreState();
                                if (inText && IsTagged(writer)) {
                                    graphics.BeginText(true);
                                }
                        }
                        }
                        if (chunk.IsAttribute(Chunk.UNDERLINE)) {
                            bool inText = graphics.InText;
                            if (inText && IsTagged(writer)) {
                                graphics.EndText();
                            }
                            float subtract = lastBaseFactor;
                            if (nextChunk != null && nextChunk.IsAttribute(Chunk.UNDERLINE))
                                subtract = 0;
                            if (nextChunk == null)
                                subtract += hangingCorrection;
                            Object[][] unders = (Object[][])chunk.GetAttribute(Chunk.UNDERLINE);
                            BaseColor scolor = null;
                            for (int k = 0; k < unders.Length; ++k) {
                                Object[] obj = unders[k];
                                scolor = (BaseColor)obj[0];
                                float[] ps = (float[])obj[1];
                                if (scolor == null)
                                    scolor = color;
                                if (scolor != null)
                                    graphics.SetColorStroke(scolor);
                                graphics.SetLineWidth(ps[0] + fontSize * ps[1]);
                                float shift = ps[2] + fontSize * ps[3];
                                int cap2 = (int)ps[4];
                                if (cap2 != 0)
                                    graphics.SetLineCap(cap2);
                                graphics.MoveTo(xMarker, yMarker + shift);
                                graphics.LineTo(xMarker + width - subtract, yMarker + shift);
                                graphics.Stroke();
                                if (scolor != null)
                                    graphics.ResetGrayStroke();
                                if (cap2 != 0)
                                    graphics.SetLineCap(0);
                            }
                            graphics.SetLineWidth(1);
                            if (inText && IsTagged(writer)) {
                                graphics.BeginText(true);
                            }
                        }
                        if (chunk.IsAttribute(Chunk.ACTION)) {
                            float subtract = lastBaseFactor;
                            if (nextChunk != null && nextChunk.IsAttribute(Chunk.ACTION))
                                subtract = 0;
                            if (nextChunk == null)
                                subtract += hangingCorrection;
                            PdfAnnotation annot = null;
                            if (chunk.IsImage()) {
                                annot = writer.CreateAnnotation(xMarker, yMarker + chunk.ImageOffsetY, xMarker + width - subtract, yMarker + chunk.ImageHeight + chunk.ImageOffsetY, (PdfAction)chunk.GetAttribute(Chunk.ACTION), null);
                            }
                            else {
                                annot = writer.CreateAnnotation(xMarker, yMarker + descender + chunk.TextRise, xMarker + width - subtract, yMarker + ascender + chunk.TextRise, (PdfAction)chunk.GetAttribute(Chunk.ACTION), null);
                            }
                            text.AddAnnotation(annot, true);
                            if (IsTagged(writer) && chunk.accessibleElement != null) {
                                PdfStructureElement strucElem;
                                structElements.TryGetValue(chunk.accessibleElement.ID, out strucElem);
                                if (strucElem != null) {
                                    int structParent = GetStructParentIndex(annot);
                                    annot.Put(PdfName.STRUCTPARENT, new PdfNumber(structParent));
                                    strucElem.SetAnnotation(annot, writer.CurrentPage);
                                    writer.StructureTreeRoot.SetAnnotationMark(structParent, strucElem.Reference);
                                }
                            }
                        }
                        if (chunk.IsAttribute(Chunk.REMOTEGOTO)) {
                            float subtract = lastBaseFactor;
                            if (nextChunk != null && nextChunk.IsAttribute(Chunk.REMOTEGOTO))
                                subtract = 0;
                            if (nextChunk == null)
                                subtract += hangingCorrection;
                            Object[] obj = (Object[])chunk.GetAttribute(Chunk.REMOTEGOTO);
                            String filename = (String)obj[0];
                            if (obj[1] is String)
                                RemoteGoto(filename, (String)obj[1], xMarker, yMarker + descender + chunk.TextRise, xMarker + width - subtract, yMarker + ascender + chunk.TextRise);
                            else
                                RemoteGoto(filename, (int)obj[1], xMarker, yMarker + descender + chunk.TextRise, xMarker + width - subtract, yMarker + ascender + chunk.TextRise);
                        }
                        if (chunk.IsAttribute(Chunk.LOCALGOTO)) {
                            float subtract = lastBaseFactor;
                            if (nextChunk != null && nextChunk.IsAttribute(Chunk.LOCALGOTO))
                                subtract = 0;
                            if (nextChunk == null)
                                subtract += hangingCorrection;
                            LocalGoto((String)chunk.GetAttribute(Chunk.LOCALGOTO), xMarker, yMarker, xMarker + width - subtract, yMarker + fontSize);
                        }
                        if (chunk.IsAttribute(Chunk.LOCALDESTINATION)) {
                            /*float subtract = lastBaseFactor;
                            if (nextChunk != null && nextChunk.IsAttribute(Chunk.LOCALDESTINATION))
                                subtract = 0;
                            if (nextChunk == null)
                                subtract += hangingCorrection;*/
                            LocalDestination((String)chunk.GetAttribute(Chunk.LOCALDESTINATION), new PdfDestination(PdfDestination.XYZ, xMarker, yMarker + fontSize, 0));
                        }
                        if (chunk.IsAttribute(Chunk.GENERICTAG)) {
                            float subtract = lastBaseFactor;
                            if (nextChunk != null && nextChunk.IsAttribute(Chunk.GENERICTAG))
                                subtract = 0;
                            if (nextChunk == null)
                                subtract += hangingCorrection;
                            Rectangle rect = new Rectangle(xMarker, yMarker, xMarker + width - subtract, yMarker + fontSize);
                            IPdfPageEvent pev = writer.PageEvent;
                            if (pev != null)
                                pev.OnGenericTag(writer, this, rect, (String)chunk.GetAttribute(Chunk.GENERICTAG));
                        }
                        if (chunk.IsAttribute(Chunk.PDFANNOTATION)) {
                            float subtract = lastBaseFactor;
                            if (nextChunk != null && nextChunk.IsAttribute(Chunk.PDFANNOTATION))
                                subtract = 0;
                            if (nextChunk == null)
                                subtract += hangingCorrection;
                            PdfAnnotation annot = PdfFormField.ShallowDuplicate((PdfAnnotation)chunk.GetAttribute(Chunk.PDFANNOTATION));
                            annot.Put(PdfName.RECT, new PdfRectangle(xMarker, yMarker + descender, xMarker + width - subtract, yMarker + ascender));
                            text.AddAnnotation(annot, true);
                        }
                        float[] paramsx = (float[])chunk.GetAttribute(Chunk.SKEW);
                        object hs = chunk.GetAttribute(Chunk.HSCALE);
                        if (paramsx != null || hs != null) {
                            float b = 0, c = 0;
                            if (paramsx != null) {
                                b = paramsx[0];
                                c = paramsx[1];
                            }
                            if (hs != null)
                                hScale = (float)hs;
                            text.SetTextMatrix(hScale, b, c, 1, xMarker, yMarker);
                        }
                        if (!isJustified) {
                            if (chunk.IsAttribute(Chunk.WORD_SPACING)) {
                                float ws = (float) chunk.GetAttribute(Chunk.WORD_SPACING);
                                text.SetWordSpacing(ws);
                            }

                            if (chunk.IsAttribute(Chunk.CHAR_SPACING)) {
                                float? cs = (float?) chunk.GetAttribute(Chunk.CHAR_SPACING);
                                text.SetCharacterSpacing(cs.Value);
                            }
                        }
                        if (chunk.IsImage()) {
                            Image image = chunk.Image;
                            width = chunk.ImageWidth;
                            float[] matrix = image.GetMatrix(chunk.ImageScalePercentage);
                            matrix[Image.CX] = xMarker + chunk.ImageOffsetX - matrix[Image.CX];
                            matrix[Image.CY] = yMarker + chunk.ImageOffsetY - matrix[Image.CY];
                            graphics.AddImage(image, matrix[0], matrix[1], matrix[2], matrix[3], matrix[4], matrix[5], false, isMCBlockOpened);
                            text.MoveText(xMarker + lastBaseFactor + chunk.ImageWidth - text.XTLM, 0);
                        }
                    }

                    xMarker += width;
                    ++chunkStrokeIdx;
                }

                if (!chunk.IsImage() && chunk.Font.CompareTo(currentFont) != 0) {
                    currentFont = chunk.Font;
                    text.SetFontAndSize(currentFont.Font, currentFont.Size);
                }
                float rise = 0;
                Object[] textRender = (Object[])chunk.GetAttribute(Chunk.TEXTRENDERMODE);
                int tr = 0;
                float strokeWidth = 1;
                BaseColor strokeColor = null;
                object fr = chunk.GetAttribute(Chunk.SUBSUPSCRIPT);
                if (textRender != null) {
                    tr = (int)textRender[0] & 3;
                    if (tr != PdfContentByte.TEXT_RENDER_MODE_FILL)
                        text.SetTextRenderingMode(tr);
                    if (tr == PdfContentByte.TEXT_RENDER_MODE_STROKE || tr == PdfContentByte.TEXT_RENDER_MODE_FILL_STROKE) {
                        strokeWidth = (float)textRender[1];
                        if (strokeWidth != 1)
                            text.SetLineWidth(strokeWidth);
                        strokeColor = (BaseColor)textRender[2];
                        if (strokeColor == null)
                            strokeColor = color;
                        if (strokeColor != null)
                            text.SetColorStroke(strokeColor);
                    }
                }
                if (fr != null)
                    rise = (float)fr;
                if (color != null)
                    text.SetColorFill(color);
                if (rise != 0)
                    text.SetTextRise(rise);
                if (chunk.IsImage()) {
                    adjustMatrix = true;
                }
                else if (chunk.IsHorizontalSeparator()) {
                    PdfTextArray array = new PdfTextArray();
                    array.Add(-glueWidth * 1000f / chunk.Font.Size / hScale);
                    text.ShowText(array);
                }
                else if (chunk.IsTab() && tabPosition != xMarker)
                {
                    PdfTextArray array = new PdfTextArray();
                    array.Add((tabPosition - xMarker) * 1000f / chunk.Font.Size / hScale);
                    text.ShowText(array);
                }
                
                // If it is a CJK chunk or Unicode TTF we will have to simulate the
                // space adjustment.
                else if (isJustified && numberOfSpaces > 0 && chunk.IsSpecialEncoding()) {
                    if (hScale != lastHScale) {
                        lastHScale = hScale;
                        text.SetWordSpacing(baseWordSpacing / hScale);
                        text.SetCharacterSpacing(baseCharacterSpacing / hScale + text.CharacterSpacing);
                    }
                    String s = chunk.ToString();
                    int idx = s.IndexOf(' ');
                    if (idx < 0)
                        text.ShowText(s);
                    else {
                        float spaceCorrection = - baseWordSpacing * 1000f / chunk.Font.Size / hScale;
                        PdfTextArray textArray = new PdfTextArray(s.Substring(0, idx));
                        int lastIdx = idx;
                        while ((idx = s.IndexOf(' ', lastIdx + 1)) >= 0) {
                            textArray.Add(spaceCorrection);
                            textArray.Add(s.Substring(lastIdx, idx - lastIdx));
                            lastIdx = idx;
                        }
                        textArray.Add(spaceCorrection);
                        textArray.Add(s.Substring(lastIdx));
                        text.ShowText(textArray);
                    }
                }
                else {
                    if (isJustified && hScale != lastHScale) {
                        lastHScale = hScale;
                        text.SetWordSpacing(baseWordSpacing / hScale);
                        text.SetCharacterSpacing(baseCharacterSpacing / hScale + text.CharacterSpacing);
                    }
                    text.ShowText(chunk.ToString());
                }
                
                if (rise != 0)
                    text.SetTextRise(0);
                if (color != null)
                    text.ResetRGBColorFill();
                if (tr != PdfContentByte.TEXT_RENDER_MODE_FILL)
                    text.SetTextRenderingMode(PdfContentByte.TEXT_RENDER_MODE_FILL);
                if (strokeColor != null)
                    text.ResetRGBColorStroke();
                if (strokeWidth != 1)
                    text.SetLineWidth(1);            
                if (chunk.IsAttribute(Chunk.SKEW) || chunk.IsAttribute(Chunk.HSCALE)) {
                    adjustMatrix = true;
                    text.SetTextMatrix(xMarker, yMarker);
                }
                if (!isJustified) {
                    if (chunk.IsAttribute(Chunk.CHAR_SPACING)) {
                        text.SetCharacterSpacing(baseCharacterSpacing);
                    }
                    if (chunk.IsAttribute(Chunk.WORD_SPACING)) {
                        text.SetWordSpacing(baseWordSpacing);
                    }
                }
                if (IsTagged(writer) && chunk.accessibleElement != null) {
                    text.CloseMCBlock(chunk.accessibleElement);
                }
            }
            if (isJustified) {
                text.SetWordSpacing(0);
                text.SetCharacterSpacing(0);
                if (line.NewlineSplit)
                    lastBaseFactor = 0;
            }
            if (adjustMatrix)
                text.MoveText(baseXMarker - text.XTLM, 0);
            currentValues[0] = currentFont;
            currentValues[1] = lastBaseFactor;
            return lastX;
        }
        
        protected internal Indentation indentation = new Indentation();
        public class Indentation {
            /** This represents the current indentation of the PDF Elements on the left side. */
            internal float indentLeft = 0;
            
            /** Indentation to the left caused by a section. */
            internal float sectionIndentLeft = 0;
            
            /** This represents the current indentation of the PDF Elements on the left side. */
            internal float listIndentLeft = 0;
            
            /** This is the indentation caused by an image on the left. */
            internal float imageIndentLeft = 0;
            
            /** This represents the current indentation of the PDF Elements on the right side. */
            internal float indentRight = 0;
            
            /** Indentation to the right caused by a section. */
            internal float sectionIndentRight = 0;
            
            /** This is the indentation caused by an image on the right. */
            internal float imageIndentRight = 0;
            
            /** This represents the current indentation of the PDF Elements on the top side. */
            internal float indentTop = 0;
            
            /** This represents the current indentation of the PDF Elements on the bottom side. */
            internal float indentBottom = 0;
        }
        
        /**
        * Gets the indentation on the left side.
        *
        * @return   a margin
        */
        
        virtual protected internal float IndentLeft {
            get {
                return GetLeft(indentation.indentLeft + indentation.listIndentLeft + indentation.imageIndentLeft + indentation.sectionIndentLeft);
            }
        }
        
        /**
        * Gets the indentation on the right side.
        *
        * @return   a margin
        */
        
        virtual protected internal float IndentRight {
            get {
                return GetRight(indentation.indentRight + indentation.sectionIndentRight + indentation.imageIndentRight);
            }
        }
        
        /**
        * Gets the indentation on the top side.
        *
        * @return   a margin
        */
        
        virtual protected internal float IndentTop {
            get {
                return GetTop(indentation.indentTop);
            }
        }
        
        /**
        * Gets the indentation on the bottom side.
        *
        * @return   a margin
        */
        
        virtual protected internal float IndentBottom {
            get {
                return GetBottom(indentation.indentBottom);
            }
        }

        /**
         * Calls addSpacing(float, float, Font, boolean (false)).
         */
        protected internal virtual void AddSpacing(float extraspace, float oldleading, Font f) {
            AddSpacing(extraspace, oldleading, f, false);
        }

        /**
        * Adds extra space.
        */
        // this method should probably be rewritten
        virtual protected internal void AddSpacing(float extraspace, float oldleading, Font f, bool spacingAfter) {
            if (extraspace == 0) 
                return;

            if (pageEmpty) 
                return;

            if (spacingAfter && !pageEmpty) {
                if (lines.Count == 0 && line.Size == 0) {
                    return;
                }
            }

            float height = spacingAfter ? extraspace : CalculateLineHeight();

            if (currentHeight + height > IndentTop - IndentBottom) {
                NewPage();
                return;
            }

            leading = extraspace;
            CarriageReturn();
            if (f.IsUnderlined() || f.IsStrikethru()) {
                f = new Font(f);
                int style = f.Style;
                style &= ~Font.UNDERLINE;
                style &= ~Font.STRIKETHRU;
                f.SetStyle(style);
            }
            Chunk space = new Chunk(" ", f);
            if (spacingAfter && pageEmpty) {
                space = new Chunk("", f);
            }
            space.Process(this);
            CarriageReturn();

            leading = oldleading;
        }
        
    //  Info Dictionary and Catalog

        /** some meta information about the Document. */
        protected internal PdfInfo info = new PdfInfo();

        /**
        * Gets the <CODE>PdfInfo</CODE>-object.
        *
        * @return   <CODE>PdfInfo</COPE>
        */
        internal PdfInfo Info {
            get {
                return info;
            }
        }
        
        /**
        * Gets the <CODE>PdfCatalog</CODE>-object.
        *
        * @param pages an indirect reference to this document pages
        * @return <CODE>PdfCatalog</CODE>
        */
        internal PdfCatalog GetCatalog(PdfIndirectReference pages) {
            PdfCatalog catalog = new PdfCatalog(pages, writer);
            
            // [C1] outlines
            if (rootOutline.Kids.Count > 0) {
                catalog.Put(PdfName.PAGEMODE, PdfName.USEOUTLINES);
                catalog.Put(PdfName.OUTLINES, rootOutline.IndirectReference);
            }
            
            // [C2] version
            writer.GetPdfVersion().AddToCatalog(catalog);
            
            // [C3] preferences
            viewerPreferences.AddToCatalog(catalog);
            
            // [C4] pagelabels
            if (pageLabels != null) {
                catalog.Put(PdfName.PAGELABELS, pageLabels.GetDictionary(writer));
            }
            
            // [C5] named objects
            catalog.AddNames(localDestinations, GetDocumentLevelJS(), documentFileAttachment, writer);
            
            // [C6] actions
            if (openActionName != null) {
                PdfAction action = GetLocalGotoAction(openActionName);
                catalog.OpenAction = action;
            }
            else if (openActionAction != null)
                catalog.OpenAction = openActionAction;
            if (additionalActions != null)   {
                catalog.AdditionalActions = additionalActions;
            }
            
            // [C7] portable collections
            if (collection != null) {
                catalog.Put(PdfName.COLLECTION, collection);
            }

            // [C8] AcroForm
            if (annotationsImp.HasValidAcroForm()) {
                catalog.Put(PdfName.ACROFORM, writer.AddToBody(annotationsImp.AcroForm).IndirectReference);
            }

            if (language != null) {
                catalog.Put(PdfName.LANG, language);
            }

            return catalog;
        }

    //  [C1] outlines

        /** This is the root outline of the document. */
        protected internal PdfOutline rootOutline;
        
        /** This is the current <CODE>PdfOutline</CODE> in the hierarchy of outlines. */
        protected internal PdfOutline currentOutline;
        
        /**
        * Adds a named outline to the document .
        * @param outline the outline to be added
        * @param name the name of this local destination
        */
        internal void AddOutline(PdfOutline outline, String name) {
            LocalDestination(name, outline.PdfDestination);
        }
        
        /**
        * Gets the root outline. All the outlines must be created with a parent.
        * The first level is created with this outline.
        * @return the root outline
        */
        virtual public PdfOutline RootOutline {
            get {
                return rootOutline;
            }
        }
            
        internal void CalculateOutlineCount() {
            if (rootOutline.Kids.Count == 0)
                return;
            TraverseOutlineCount(rootOutline);
        }

        internal void TraverseOutlineCount(PdfOutline outline) {
            List<PdfOutline> kids = outline.Kids;
            PdfOutline parent = outline.Parent;
            if (kids.Count == 0) {
                if (parent != null) {
                    parent.Count = parent.Count + 1;
                }
            }
            else {
                for (int k = 0; k < kids.Count; ++k) {
                    TraverseOutlineCount(kids[k]);
                }
                if (parent != null) {
                    if (outline.Open) {
                        parent.Count = outline.Count + parent.Count + 1;
                    }
                    else {
                        parent.Count = parent.Count + 1;
                        outline.Count = -outline.Count;
                    }
                }
            }
        }
        
        internal void WriteOutlines() {
            if (rootOutline.Kids.Count == 0)
                return;
            OutlineTree(rootOutline);
            writer.AddToBody(rootOutline, rootOutline.IndirectReference);
        }
        
        internal void OutlineTree(PdfOutline outline) {
            outline.IndirectReference = writer.PdfIndirectReference;
            if (outline.Parent != null)
                outline.Put(PdfName.PARENT, outline.Parent.IndirectReference);
            List<PdfOutline> kids = outline.Kids;
            int size = kids.Count;
            for (int k = 0; k < size; ++k)
                OutlineTree(kids[k]);
            for (int k = 0; k < size; ++k) {
                if (k > 0)
                    kids[k].Put(PdfName.PREV, kids[k - 1].IndirectReference);
                if (k < size - 1)
                    kids[k].Put(PdfName.NEXT, kids[k + 1].IndirectReference);
            }
            if (size > 0) {
                outline.Put(PdfName.FIRST, kids[0].IndirectReference);
                outline.Put(PdfName.LAST, kids[size - 1].IndirectReference);
            }
            for (int k = 0; k < size; ++k) {
                PdfOutline kid = kids[k];
                writer.AddToBody(kid, kid.IndirectReference);
            }
        }
        
    //  [C3] PdfViewerPreferences interface

        /** Contains the Viewer preferences of this PDF document. */
        protected PdfViewerPreferencesImp viewerPreferences = new PdfViewerPreferencesImp();
        /** @see com.lowagie.text.pdf.interfaces.PdfViewerPreferences#setViewerPreferences(int) */
        internal int ViewerPreferences {
            set {
                this.viewerPreferences.ViewerPreferences = value;
            }
        }

        /** @see com.lowagie.text.pdf.interfaces.PdfViewerPreferences#addViewerPreference(com.lowagie.text.pdf.PdfName, com.lowagie.text.pdf.PdfObject) */
        internal void AddViewerPreference(PdfName key, PdfObject value) {
            this.viewerPreferences.AddViewerPreference(key, value);
        }

    //  [C4] Page labels

        protected internal PdfPageLabels pageLabels;

        virtual public PdfPageLabels PageLabels {
            get {
                return pageLabels;
            }
            internal set {
                this.pageLabels = value;
            }
        }

    //  [C5] named objects: local destinations, javascript, embedded files

        /**
        * Implements a link to other part of the document. The jump will
        * be made to a local destination with the same name, that must exist.
        * @param name the name for this link
        * @param llx the lower left x corner of the activation area
        * @param lly the lower left y corner of the activation area
        * @param urx the upper right x corner of the activation area
        * @param ury the upper right y corner of the activation area
        */
        internal void LocalGoto(String name, float llx, float lly, float urx, float ury) {
            PdfAction action = GetLocalGotoAction(name);
            annotationsImp.AddPlainAnnotation(writer.CreateAnnotation(llx, lly, urx, ury, action, null));
        }
        
        /**
        * Implements a link to another document.
        * @param filename the filename for the remote document
        * @param name the name to jump to
        * @param llx the lower left x corner of the activation area
        * @param lly the lower left y corner of the activation area
        * @param urx the upper right x corner of the activation area
        * @param ury the upper right y corner of the activation area
        */
        internal void RemoteGoto(String filename, String name, float llx, float lly, float urx, float ury) {
            annotationsImp.AddPlainAnnotation(writer.CreateAnnotation(llx, lly, urx, ury, new PdfAction(filename, name), null));
        }
        
        /**
        * Implements a link to another document.
        * @param filename the filename for the remote document
        * @param page the page to jump to
        * @param llx the lower left x corner of the activation area
        * @param lly the lower left y corner of the activation area
        * @param urx the upper right x corner of the activation area
        * @param ury the upper right y corner of the activation area
        */
        internal void RemoteGoto(String filename, int page, float llx, float lly, float urx, float ury) {
            AddAnnotation(writer.CreateAnnotation(llx, lly, urx, ury, new PdfAction(filename, page), null));
        }
        
        /** Implements an action in an area.
        * @param action the <CODE>PdfAction</CODE>
        * @param llx the lower left x corner of the activation area
        * @param lly the lower left y corner of the activation area
        * @param urx the upper right x corner of the activation area
        * @param ury the upper right y corner of the activation area
        */
        internal void SetAction(PdfAction action, float llx, float lly, float urx, float ury) {
            AddAnnotation(writer.CreateAnnotation(llx, lly, urx, ury, action, null));
        }
        
        /**
        * Stores the destinations keyed by name. Value is
        * <CODE>Object[]{PdfAction,PdfIndirectReference,PdfDestintion}</CODE>.
        */
        protected internal SortedDictionary<string,Destination> localDestinations = new SortedDictionary<string,Destination>(StringComparer.Ordinal);

        internal PdfAction GetLocalGotoAction(String name) {
            PdfAction action;
            Destination dest;
            if (localDestinations.ContainsKey(name))
                dest = localDestinations[name];
            else
                dest = new Destination();
            if (dest.action == null) {
                if (dest.reference == null) {
                    dest.reference = writer.PdfIndirectReference;
                }
                action = new PdfAction(dest.reference);
                dest.action = action;
                localDestinations[name] = dest;
            }
            else {
                action = dest.action;
            }
            return action;
        }
        
        /**
        * The local destination to where a local goto with the same
        * name will jump to.
        * @param name the name of this local destination
        * @param destination the <CODE>PdfDestination</CODE> with the jump coordinates
        * @return <CODE>true</CODE> if the local destination was added,
        * <CODE>false</CODE> if a local destination with the same name
        * already existed
        */
        internal bool LocalDestination(String name, PdfDestination destination) {
            Destination dest;
            if (localDestinations.ContainsKey(name))
                dest = localDestinations[name];
            else
                dest = new Destination();
            if (dest.destination != null)
                return false;
            dest.destination = destination;
            localDestinations[name] = dest;
            if (!destination.HasPage())
                destination.AddPage(writer.CurrentPage);
            return true;
        }
        
        /**
        * Stores a list of document level JavaScript actions.
        */
        private int jsCounter;
        protected internal Dictionary<String, PdfObject> documentLevelJS = new Dictionary<string,PdfObject>();

        internal void AddJavaScript(PdfAction js) {
            if (js.Get(PdfName.JS) == null)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("only.javascript.actions.are.allowed"));
            documentLevelJS[jsCounter.ToString().PadLeft(16, '0')] = writer.AddToBody(js).IndirectReference;
            jsCounter++;
        }
        
        internal void AddJavaScript(String name, PdfAction js) {
            if (js.Get(PdfName.JS) == null)
                throw new ArgumentException(MessageLocalization.GetComposedMessage("only.javascript.actions.are.allowed"));
            documentLevelJS[name] = writer.AddToBody(js).IndirectReference;
        }

        internal Dictionary<String, PdfObject> GetDocumentLevelJS() {
            return documentLevelJS;
        }

        protected internal Dictionary<String, PdfObject> documentFileAttachment = new Dictionary<String, PdfObject>();

        internal void AddFileAttachment(String description, PdfFileSpecification fs) {
            if (description == null) {
                PdfString desc = (PdfString)fs.Get(PdfName.DESC);
                if (desc == null) {
                    description = ""; 
                }
                else {
                    description = PdfEncodings.ConvertToString(desc.GetBytes(), null);
                }
            }
            fs.AddDescription(description, true);
            if (description.Length == 0)
                description = "Unnamed";
            String fn = PdfEncodings.ConvertToString(new PdfString(description, PdfObject.TEXT_UNICODE).GetBytes(), null);
            int k = 0;
            while (documentFileAttachment.ContainsKey(fn)) {
                ++k;
                fn = PdfEncodings.ConvertToString(new PdfString(description + " " + k, PdfObject.TEXT_UNICODE).GetBytes(), null);
            }
            documentFileAttachment[fn] = fs.Reference;
        }
        
        internal Dictionary<String, PdfObject> GetDocumentFileAttachment() {
            return documentFileAttachment;
        }

    //  [C6] document level actions

        protected internal String openActionName;

        internal void SetOpenAction(String name) {
            openActionName = name;
            openActionAction = null;
        }
        
        protected internal PdfAction openActionAction;

        internal void SetOpenAction(PdfAction action) {
            openActionAction = action;
            openActionName = null;
        }

        protected internal PdfDictionary additionalActions;

        internal void AddAdditionalAction(PdfName actionType, PdfAction action)  {
            if (additionalActions == null)  {
                additionalActions = new PdfDictionary();
            }
            if (action == null)
                additionalActions.Remove(actionType);
            else
                additionalActions.Put(actionType, action);
            if (additionalActions.Size == 0)
                additionalActions = null;
        }
        
    //  [C7] portable collections

        protected internal PdfCollection collection;

        /**
        * Sets the collection dictionary.
        * @param collection a dictionary of type PdfCollection
        */
        virtual public PdfCollection Collection {
            set {
                this.collection = value;
            }
        }

    //  [C8] AcroForm
        
        internal PdfAnnotationsImp annotationsImp;

        /**
        * Gets the AcroForm object.
        * @return the PdfAcroform object of the PdfDocument
        */
        virtual public PdfAcroForm AcroForm {
            get {
                return annotationsImp.AcroForm;
            }
        }
        
        internal int SigFlags {
            set {
                annotationsImp.SigFlags = value;
            }
        }
        
        internal void AddCalculationOrder(PdfFormField formField) {
            annotationsImp.AddCalculationOrder(formField);
        }

        internal void AddAnnotation(PdfAnnotation annot) {
            pageEmpty = false;
            annotationsImp.AddAnnotation(annot);
        }
        
        protected PdfString language;
        internal void SetLanguage(String language) {
            this.language = new PdfString(language);
        }

    //	[F12] tagged PDF
    //	[U1] page sizes

        /** This is the size of the next page. */
        protected Rectangle nextPageSize = null;
        
        /** This is the size of the several boxes of the current Page. */
        protected Dictionary<string, PdfRectangle> thisBoxSize = new Dictionary<string,PdfRectangle>();
        
        /** This is the size of the several boxes that will be used in
        * the next page. */
        protected Dictionary<string, PdfRectangle> boxSize = new Dictionary<string, PdfRectangle>();
        
        internal Rectangle CropBoxSize {
            set {
                SetBoxSize("crop", value);
            }
        }
        
        internal void SetBoxSize(String boxName, Rectangle size) {
            if (size == null)
                boxSize.Remove(boxName);
            else
                boxSize[boxName] = new PdfRectangle(size);
        }
        
        virtual protected internal void SetNewPageSizeAndMargins() {
            pageSize = nextPageSize;
            if (marginMirroring && (PageNumber & 1) == 0) {
                marginRight = nextMarginLeft;
                marginLeft = nextMarginRight;
            }
            else {
                marginLeft = nextMarginLeft;
                marginRight = nextMarginRight;
            }
            if (marginMirroringTopBottom && (PageNumber & 1) == 0) {
                marginTop = nextMarginBottom;
                marginBottom = nextMarginTop;
            }
            else {
                marginTop = nextMarginTop;
                marginBottom = nextMarginBottom;
            }
            if (!IsTagged(writer)) {
                text = new PdfContentByte(writer);
                text.Reset();
            } else {
                text = graphics;
            }
            text.BeginText();
            // we move to the left/top position of the page
            text.MoveText(Left, Top);
            if (IsTagged(writer))
                textEmptySize = text.Size;
        }

        /**
        * Gives the size of a trim, art, crop or bleed box, or null if not defined.
        * @param boxName crop, trim, art or bleed
        */
        internal Rectangle GetBoxSize(String boxName) {
            PdfRectangle r;
            thisBoxSize.TryGetValue(boxName, out r);
            if (r != null)
                return r.Rectangle;
            return null;
        }
        
    //	[U2] empty pages

        /** This checks if the page is empty. */
        private bool pageEmpty = true;
        
        internal bool PageEmpty {
            set {
                this.pageEmpty = value;
            }
            get {
                if (IsTagged(writer))
                    return writer == null || writer.DirectContent.GetSize(false) == 0 && writer.DirectContentUnder.GetSize(false) == 0 && text.GetSize(false) - textEmptySize == 0 && (pageEmpty || writer.IsPaused());
                else
                    return writer == null || writer.DirectContent.Size == 0 && writer.DirectContentUnder.Size == 0 && (pageEmpty || writer.IsPaused());

            }
        }


    //	[U3] page actions

        /**
        * Sets the display duration for the page (for presentations)
        * @param seconds   the number of seconds to display the page
        */
        internal int Duration {
            set {
                if (value > 0)
                    writer.AddPageDictEntry(PdfName.DUR, new PdfNumber(value));            }
        }
        
        /**
        * Sets the transition for the page
        * @param transition   the PdfTransition object
        */
        internal PdfTransition Transition {
            set {
                writer.AddPageDictEntry(PdfName.TRANS, value.TransitionDictionary);
            }
        }

        protected PdfDictionary pageAA = null;

        internal void SetPageAction(PdfName actionType, PdfAction action) {
            if (pageAA == null) {
                pageAA = new PdfDictionary();
            }
            pageAA.Put(actionType, action);
        }
        
    //	[U8] thumbnail images

        internal Image Thumbnail {
            set {
                writer.AddPageDictEntry(PdfName.THUMB, writer.GetImageReference(writer.AddDirectImageSimple(value)));
            }
        }

    //	[M0] Page resources contain references to fonts, extgstate, images,...

        /** This are the page resources of the current Page. */
        protected internal PageResources pageResources;
        
        internal PageResources PageResources {
            get {
                return pageResources;
            }
        }
        
    //	[M3] Images

        /** Holds value of property strictImageSequence. */
        protected internal bool strictImageSequence = false;    

        /** Setter for property strictImageSequence.
        * @param strictImageSequence New value of property strictImageSequence.
        *
        */
        internal bool StrictImageSequence {
            set {
                this.strictImageSequence = value;
            }
            get {
                return strictImageSequence;
            }
        }
     
        /** This is the position where the image ends. */
        protected internal float imageEnd = -1;
        
        /**
        * Method added by Pelikan Stephan
        * @see com.lowagie.text.DocListener#clearTextWrap()
        */
        virtual public void ClearTextWrap() {
            float tmpHeight = imageEnd - currentHeight;
            if (line != null) {
                tmpHeight += line.Height;
            }
            if ((imageEnd > -1) && (tmpHeight > 0)) {
                CarriageReturn();
                currentHeight += tmpHeight;
            }
        }

        virtual public int GetStructParentIndex(Object obj) {
            int[] i;
            structParentIndices.TryGetValue(obj, out i);
            if(i == null) {
                i = new int[] { structParentIndices.Count, 0 };
                structParentIndices[obj] = i;
            }
            return i[0];
        }

        virtual public int GetNextMarkPoint(Object obj) {
            int[] i;
            structParentIndices.TryGetValue(obj, out i);
            if(i == null) {
                i = new int[] { structParentIndices.Count, 0 };
                structParentIndices[obj] = i;
            }
            int markPoint = i[1];
            i[1]++;
            return markPoint;
        }

        virtual public int[] GetStructParentIndexAndNextMarkPoint(Object obj) {
            int[] i;
            structParentIndices.TryGetValue(obj, out i);
            if(i == null) {
                i = new int[] { structParentIndices.Count, 0 };
                structParentIndices[obj] = i;
            }
            int markPoint = i[1];
            i[1]++;
            return new int[] {i[0], markPoint};
        }


        /** This is the image that could not be shown on a previous page. */
        protected internal Image imageWait = null;
        
        /**
        * Adds an image to the document.
        * @param image the <CODE>Image</CODE> to add
        * @throws PdfException on error
        * @throws DocumentException on error
        */        
        virtual protected internal void Add(Image image) {
            if (image.HasAbsolutePosition()) {
                graphics.AddImage(image);
                pageEmpty = false;
                return;
            }
            
            // if there isn't enough room for the image on this page, save it for the next page
            if (currentHeight != 0 && IndentTop - currentHeight - image.ScaledHeight < IndentBottom) {
                if (!strictImageSequence && imageWait == null) {
                    imageWait = image;
                    return;
                }
                NewPage();
                if (currentHeight != 0 && IndentTop - currentHeight - image.ScaledHeight < IndentBottom) {
                    imageWait = image;
                    return;
                }
            }
            pageEmpty = false;
            // avoid endless loops
            if (image == imageWait)
                imageWait = null;
            bool textwrap = (image.Alignment & Image.TEXTWRAP) == Image.TEXTWRAP
            && (image.Alignment & Image.MIDDLE_ALIGN) != Image.MIDDLE_ALIGN;
            bool underlying = (image.Alignment & Image.UNDERLYING) == Image.UNDERLYING;
            float diff = leading / 2;
            if (textwrap) {
                diff += leading;
            }
            float lowerleft = IndentTop - currentHeight - image.ScaledHeight - diff;
            float[] mt = image.GetMatrix();
            float startPosition = IndentLeft - mt[4];
            if ((image.Alignment & Image.RIGHT_ALIGN) == Image.RIGHT_ALIGN) startPosition = IndentRight - image.ScaledWidth - mt[4];
            if ((image.Alignment & Image.MIDDLE_ALIGN) == Image.MIDDLE_ALIGN) startPosition = IndentLeft + ((IndentRight - IndentLeft - image.ScaledWidth) / 2) - mt[4];
            if (image.HasAbsoluteX()) startPosition = image.AbsoluteX;
            if (textwrap) {
                if (imageEnd < 0 || imageEnd < currentHeight + image.ScaledHeight + diff) {
                    imageEnd = currentHeight + image.ScaledHeight + diff;
                }
                if ((image.Alignment & Image.RIGHT_ALIGN) == Image.RIGHT_ALIGN) {
                    // indentation suggested by Pelikan Stephan
                    indentation.imageIndentRight += image.ScaledWidth + image.IndentationLeft;
                }
                else {
                    // indentation suggested by Pelikan Stephan
                    indentation.imageIndentLeft += image.ScaledWidth + image.IndentationRight;
                }
            }
            else {
                if ((image.Alignment & Image.RIGHT_ALIGN) == Image.RIGHT_ALIGN) startPosition -= image.IndentationRight;
                else if ((image.Alignment & Image.MIDDLE_ALIGN) == Image.MIDDLE_ALIGN) startPosition += image.IndentationLeft - image.IndentationRight;
                else startPosition -= image.IndentationRight;
            }
            graphics.AddImage(image, mt[0], mt[1], mt[2], mt[3], startPosition, lowerleft - mt[5]);
            if (!(textwrap || underlying)) {
                currentHeight += image.ScaledHeight + diff;
                FlushLines();
                text.MoveText(0, - (image.ScaledHeight + diff));
                NewLine();
            }
        }
        
    //	[M4] Adding a PdfPTable

        /** Adds a <CODE>PdfPTable</CODE> to the document.
        * @param ptable the <CODE>PdfPTable</CODE> to be added to the document.
        * @throws DocumentException on error
        */
        internal void AddPTable(PdfPTable ptable) {
            ColumnText ct = new ColumnText(writer.DirectContent);
            ct.RunDirection = ptable.RunDirection;
            // if the table prefers to be on a single page, and it wouldn't
            //fit on the current page, start a new page.
            if (ptable.KeepTogether && !FitsPage(ptable, 0f) && currentHeight > 0)  {
                NewPage();
                if (IsTagged(writer)) {
                    ct.Canvas = text;
                }
            }
            if (currentHeight == 0) {
                ct.AdjustFirstLine = false;
            }
            ct.AddElement(ptable);
            bool he = ptable.HeadersInEvent;
            ptable.HeadersInEvent = true;
            int loop = 0;
            while (true) {
                ct.SetSimpleColumn(IndentLeft, IndentBottom, IndentRight, IndentTop - currentHeight);
                int status = ct.Go();
                if ((status & ColumnText.NO_MORE_TEXT) != 0) {
                    if (IsTagged(writer))
                        text.SetTextMatrix(IndentLeft, ct.YLine);
                    else
                        text.MoveText(0, ct.YLine - IndentTop + currentHeight);

                    currentHeight = IndentTop - ct.YLine;
                    break;
                }
                if (IndentTop - currentHeight == ct.YLine)
                    ++loop;
                else
                    loop = 0;
                if (loop == 3) {
                    throw new DocumentException(MessageLocalization.GetComposedMessage("infinite.table.loop"));
                }
                currentHeight = IndentTop - ct.YLine;
                NewPage();
                if (IsTagged(writer))
                    ct.Canvas = text;
            }
            ptable.HeadersInEvent = he;
        }

        internal List<IElement> floatingElements = new List<IElement>();

        internal void AddDiv(PdfDiv div) {
            if (floatingElements == null) {
                floatingElements = new List<IElement>();
            }
            floatingElements.Add(div);
        }

        internal void FlushFloatingElements() {
            if (floatingElements != null && floatingElements.Count > 0) {
                List<IElement> cachedFloatingElements = floatingElements;
                floatingElements = null;
                FloatLayout fl = new FloatLayout(cachedFloatingElements, false);
                int loop = 0;
                while (true) {
                    float left = IndentLeft;
                    fl.SetSimpleColumn(IndentLeft, IndentBottom, IndentRight, IndentTop - currentHeight);
                    try {
                        int status = fl.Layout(IsTagged(writer) ? text : writer.DirectContent, false);
                        if ((status & ColumnText.NO_MORE_TEXT) != 0)
                        {
                            if (IsTagged(writer))
                                text.SetTextMatrix(IndentLeft, fl.YLine);
                            else
                                text.MoveText(0, fl.YLine - IndentTop + currentHeight);

                            currentHeight = IndentTop - fl.YLine;
                            break;
                        }
                    } catch(Exception) {
                        return;
                    }
                    if (IndentTop - currentHeight == fl.YLine || PageEmpty)
                        ++loop;
                    else {
                        loop = 0;
                    }
                    if (loop == 2) {
                        return;
                    }
                    NewPage();
                }
            }
        }
        
        internal bool FitsPage(PdfPTable table, float margin) {
            if (!table.LockedWidth) {
                float totalWidth = (IndentRight - IndentLeft) * table.WidthPercentage / 100;
                table.TotalWidth = totalWidth;
            }
            // ensuring that a new line has been started.
            EnsureNewLine();
            float spaceNeeded = table.SkipFirstHeader ? table.TotalHeight - table.HeaderHeight : table.TotalHeight;
            return spaceNeeded + (currentHeight > 0 ? table.SpacingBefore : 0f)
                <= IndentTop - currentHeight - IndentBottom - margin;
        } 
       
        private static bool IsTagged(PdfWriter writer) {
            return (writer != null) && writer.IsTagged();
        }

        private PdfLine GetLastLine() {
            if (lines.Count > 0)
                return lines[lines.Count - 1];
            else
                return null;
        }

        /**
         * @since 5.0.1
         */
        public class Destination {
            public PdfAction action;
            public PdfIndirectReference reference;
            public PdfDestination destination;
        }
    }
}
