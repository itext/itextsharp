using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.util;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using iTextSharp.text.xml.simpleparser;
/*
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

namespace iTextSharp.text.html.simpleparser {
    /**
     * Old iText class that allows you to convert HTML to PDF.
     * We've completely rewritten HTML to PDF conversion and we made it a separate project named XML Worker.
     * @deprecated since 5.5.2; please switch to XML Worker instead (this is a separate project)
     */
    [Obsolete]
    public class HTMLWorker : ISimpleXMLDocHandler, IDocListener {

        private static readonly ILogger LOGGER = LoggerFactory.GetLogger(typeof(HTMLWorker));
        /**
         * DocListener that will listen to the Elements
         * produced by parsing the HTML.
         * This can be a com.lowagie.text.Document adding
         * the elements to a Document directly, or an
         * HTMLWorker instance strong the objects in a List
         */
        protected IDocListener document;

        /**
         * The map with all the supported tags.
         * @since 5.0.6
         */
        protected internal IDictionary<String, IHTMLTagProcessor> tags;

        /** The object defining all the styles. */
        private StyleSheet style = new StyleSheet();

        /**
         * Creates a new instance of HTMLWorker
         * @param document A class that implements <CODE>DocListener</CODE>
         */
        public HTMLWorker(IDocListener document) : this(document, null, null) {
        }

        /**
         * Creates a new instance of HTMLWorker
         * @param document  A class that implements <CODE>DocListener</CODE>
         * @param tags      A map containing the supported tags
         * @param style     A StyleSheet
         * @since 5.0.6
         */
        public HTMLWorker(IDocListener document, IDictionary<String, IHTMLTagProcessor> tags, StyleSheet style) {
            this.document = document;
            SetSupportedTags(tags);
            SetStyleSheet(style);
        }

        /**
         * Sets the map with supported tags.
         * @param tags
         * @since 5.0.6
         */
        virtual public void SetSupportedTags(IDictionary<String, IHTMLTagProcessor> tags) {
            if (tags == null)
                tags = new HTMLTagProcessors();
            this.tags = tags;
        }

        /**
         * Setter for the StyleSheet
         * @param style the StyleSheet
         */
        virtual public void SetStyleSheet(StyleSheet style) {
            if (style == null)
                style = new StyleSheet();
            this.style = style;
        }

        /**
         * Parses content read from a java.io.Reader object.
         * @param reader    the content
         * @throws IOException
         */
        virtual public void Parse(TextReader reader) {
            LOGGER.Info("Please note, there is a more extended version of the HTMLWorker available in the iText XMLWorker");
            SimpleXMLParser.Parse(this, null, reader, true);
        }

        // state machine

        /**
         * Stack with the Elements that already have been processed.
         * @since iText 5.0.6 (private => protected)
         */
        protected Stack<IElement> stack = new Stack<IElement>();

        /**
         * Keeps the content of the current paragraph
         * @since iText 5.0.6 (private => protected)
         */
        protected Paragraph currentParagraph;

        /**
         * The current hierarchy chain of tags.
         * @since 5.0.6
         */
        private ChainedProperties chain = new ChainedProperties();

        /**
         * @see com.itextpdf.text.xml.simpleparser.SimpleXMLDocHandler#startDocument()
         */
        public virtual void StartDocument() {
            Dictionary<String, String> attrs = new Dictionary<String, String>();
            style.ApplyStyle(HtmlTags.BODY, attrs);
            chain.AddToChain(HtmlTags.BODY, attrs);
        }

        /**
         * @see com.itextpdf.text.xml.simpleparser.SimpleXMLDocHandler#startElement(java.lang.String, java.util.Dictionary)
         */
        public virtual void StartElement(String tag, IDictionary<String, String> attrs) {
            IHTMLTagProcessor htmlTag;
            tags.TryGetValue(tag, out htmlTag);
            if (htmlTag == null) {
                return;
            }
            // apply the styles to attrs
            style.ApplyStyle(tag, attrs);
            // deal with the style attribute
            StyleSheet.ResolveStyleAttribute(attrs, chain);
            // process the tag
            htmlTag.StartElement(this, tag, attrs);
        }

        /**
         * @see com.itextpdf.text.xml.simpleparser.SimpleXMLDocHandler#text(java.lang.String)
         */
        public virtual void Text(String content) {
            if (skipText)
                return;
            if (currentParagraph == null) {
                currentParagraph = CreateParagraph();
            }
            if (!insidePRE) {
                // newlines and carriage returns are ignored
                if (content.Trim().Length == 0 && content.IndexOf(' ') < 0) {
                    return;
                }
                content = HtmlUtilities.EliminateWhiteSpace(content);
            }
            Chunk chunk = CreateChunk(content);
            currentParagraph.Add(chunk);
        }

        /**
         * @see com.itextpdf.text.xml.simpleparser.SimpleXMLDocHandler#endElement(java.lang.String)
         */
        public virtual void EndElement(String tag) {
            IHTMLTagProcessor htmlTag;
            tags.TryGetValue(tag, out htmlTag);
            if (htmlTag == null) {
                return;
            }
            // process the tag
            htmlTag.EndElement(this, tag);
        }

        /**
         * @see com.itextpdf.text.xml.simpleparser.SimpleXMLDocHandler#endDocument()
         */
        public virtual void EndDocument() {
            // flush the stack
            foreach (IElement e in stack)
                document.Add(e);
            // add current paragraph
            if (currentParagraph != null)
                document.Add(currentParagraph);
            currentParagraph = null;
        }

        // stack and current paragraph operations

        /**
         * Adds a new line to the currentParagraph.
         * @since 5.0.6
         */
        virtual public void NewLine() {
            if (currentParagraph == null) {
                currentParagraph = new Paragraph();
            }
            currentParagraph.Add(CreateChunk("\n"));
        }

        /**
         * Flushes the current paragraph, indicating that we're starting
         * a new block.
         * If the stack is empty, the paragraph is added to the document.
         * Otherwise the Paragraph is added to the stack.
         * @since 5.0.6
         */
        virtual public void CarriageReturn() {
            if (currentParagraph == null)
                return;
            if (stack.Count == 0)
                document.Add(currentParagraph);
            else {
                IElement obj = stack.Pop();
                if (obj is ITextElementArray) {
                    ITextElementArray current = (ITextElementArray) obj;
                    current.Add(currentParagraph);
                }
                stack.Push(obj);
            }
            currentParagraph = null;
        }

        /**
         * Stacks the current paragraph, indicating that we're starting
         * a new span.
         * @since 5.0.6
         */
        virtual public void FlushContent() {
            PushToStack(currentParagraph);
            currentParagraph = new Paragraph();
        }

        /**
         * Pushes an element to the Stack.
         * @param element
         * @since 5.0.6
         */
        virtual public void PushToStack(IElement element) {
            if (element != null)
                stack.Push(element);
        }

        /**
         * Updates the chain with a new tag and new attributes.
         * @param tag   the new tag
         * @param attrs the corresponding attributes
         * @since 5.0.6
         */
        virtual public void UpdateChain(String tag, IDictionary<String, String> attrs) {
            chain.AddToChain(tag, attrs);
        }

        /**
         * Updates the chain by removing a tag.
         * @param tag   the new tag
         * @since 5.0.6
         */
        virtual public void UpdateChain(String tag) {
            chain.RemoveChain(tag);
        }

        // providers that help find resources such as images and fonts

        /**
         * Key used to store the image provider in the providers map.
         * @since 5.0.6
         */
        public const String IMG_PROVIDER = "img_provider";

        /**
         * Key used to store the image processor in the providers map.
         * @since 5.0.6
         */
        public const String IMG_PROCESSOR = "img_interface";

        /**
         * Key used to store the image store in the providers map.
         * @since 5.0.6
         */
        public const String IMG_STORE = "img_static";

        /**
         * Key used to store the image baseurl provider in the providers map.
         * @since 5.0.6
         */
        public const String IMG_BASEURL = "img_baseurl";

        /**
         * Key used to store the font provider in the providers map.
         * @since 5.0.6
         */
        public const String FONT_PROVIDER = "font_factory";

        /**
         * Key used to store the link provider in the providers map.
         * @since 5.0.6
         */
        public const String LINK_PROVIDER = "alink_interface";

        /**
         * IDictionary containing providers such as a FontProvider or ImageProvider.
         * @since 5.0.6 (renamed from interfaceProps)
         */
        private IDictionary<String, Object> providers = new Dictionary<String, Object>();

        /**
         * Setter for the providers.
         * If a FontProvider is added, the ElementFactory is updated.
         * @param providers a IDictionary with different providers
         * @since 5.0.6
         */
        virtual public void SetProviders(IDictionary<String, Object> providers) {
            if (providers == null)
                return;
            this.providers = providers;
            IFontProvider ff = null;
            if (providers.ContainsKey(FONT_PROVIDER))
                ff = (IFontProvider)providers[FONT_PROVIDER];
            if (ff != null)
                factory.FontProvider = ff;
        }

        // factory that helps create objects

        /**
         * Factory that is able to create iText Element objects.
         * @since 5.0.6
         */
        private ElementFactory factory = new ElementFactory();

        /**
         * Creates a Chunk using the factory.
         * @param content   the content of the chunk
         * @return  a Chunk with content
         * @since 5.0.6
         */
        virtual public Chunk CreateChunk(String content) {
            return factory.CreateChunk(content, chain);
        }
        /**
         * Creates a Paragraph using the factory.
         * @return  a Paragraph without any content
         * @since 5.0.6
         */
        virtual public Paragraph CreateParagraph() {
            return factory.CreateParagraph(chain);
        }
        /**
         * Creates a List object.
         * @param tag should be "ol" or "ul"
         * @return  a List object
         * @since 5.0.6
         */
        virtual public List CreateList(String tag) {
            return factory.CreateList(tag, chain);
        }
        /**
         * Creates a ListItem object.
         * @return a ListItem object
         * @since 5.0.6
         */
        virtual public ListItem CreateListItem() {
            return factory.CreateListItem(chain);
        }
        /**
         * Creates a LineSeparator object.
         * @param attrs properties of the LineSeparator
         * @return a LineSeparator object
         * @since 5.0.6
         */
        virtual public LineSeparator CreateLineSeparator(IDictionary<String, String> attrs) {
            return factory.CreateLineSeparator(attrs, currentParagraph.Leading / 2);
        }

        /**
         * Creates an Image object.
         * @param attrs properties of the Image
         * @return an Image object (or null if the Image couldn't be found)
         * @throws DocumentException
         * @throws IOException
         * @since 5.0.6
         */
        virtual public Image CreateImage(IDictionary<String, String> attrs) {
            String src;
            attrs.TryGetValue(HtmlTags.SRC, out src);
            if (src == null)
                return null;
            Image img = factory.CreateImage(
                    src, attrs, chain, document,
                    providers.ContainsKey(IMG_PROVIDER) ? (IImageProvider)providers[IMG_PROVIDER] : null, 
                    providers.ContainsKey(IMG_STORE) ? (ImageStore)providers[IMG_STORE] : null, 
                    providers.ContainsKey(IMG_BASEURL) ? (string)providers[IMG_BASEURL] : null);
            return img;
        }

        /**
         * Creates a Cell.
         * @param tag   the tag
         * @return  a CellWrapper object
         * @since 5.0.6
         */
        virtual public CellWrapper CreateCell(String tag) {
            return new CellWrapper(tag, chain);
        }

        // processing objects

        /**
         * Adds a link to the current paragraph.
         * @since 5.0.6
         */
        virtual public void ProcessLink() {
            if (currentParagraph == null) {
                currentParagraph = new Paragraph();
            }
            // The link provider allows you to do additional processing
            ILinkProcessor i = null;
            if (providers.ContainsKey(LINK_PROVIDER))
                i = (ILinkProcessor) providers[LINK_PROVIDER];
            if (i == null || !i.Process(currentParagraph, chain)) {
                // sets an Anchor for all the Chunks in the current paragraph
                String href = chain[HtmlTags.HREF];
                if (href != null) {
                    foreach (Chunk ck in currentParagraph.Chunks) {
                        ck.SetAnchor(href);
                    }
                }
            }
            // a link should be added to the current paragraph as a phrase
            if (stack.Count == 0) {
                // no paragraph to add too, 'a' tag is first element
                Paragraph tmp = new Paragraph(new Phrase(currentParagraph));
                currentParagraph = tmp;
            } else {
                Paragraph tmp = (Paragraph) stack.Pop();
                tmp.Add(new Phrase(currentParagraph));
                currentParagraph = tmp;
            }
        }

        /**
         * Fetches the List from the Stack and adds it to
         * the TextElementArray on top of the Stack,
         * or to the Document if the Stack is empty.
         * @throws DocumentException
         * @since 5.0.6
         */
        virtual public void ProcessList() {
            if (stack.Count == 0)
                return;
            IElement obj = stack.Pop();
            if (!(obj is List)) {
                stack.Push(obj);
                return;
            }
            if (stack.Count == 0)
                document.Add(obj);
            else
                ((ITextElementArray) stack.Peek()).Add(obj);
        }

        /**
         * Looks for the List object on the Stack,
         * and adds the ListItem to the List.
         * @throws DocumentException
         * @since 5.0.6
         */
        virtual public void ProcessListItem() {
            if (stack.Count == 0)
                return;
            IElement obj = stack.Pop();
            if (!(obj is ListItem)) {
                stack.Push(obj);
                return;
            }
            if (stack.Count == 0) {
                document.Add(obj);
                return;
            }
            ListItem item = (ListItem) obj;
            IElement list = stack.Pop();
            if (!(list is List)) {
                stack.Push(list);
                return;
            }
            ((List) list).Add(item);
            item.AdjustListSymbolFont();
            stack.Push(list);
        }

        /**
         * Processes an Image.
         * @param img
         * @param attrs
         * @throws DocumentException
         * @since   5.0.6
         */
        virtual public void ProcessImage(Image img, IDictionary<String, String> attrs) {
            IImageProcessor processor = null;
            if (providers.ContainsKey(IMG_PROCESSOR))
                processor = (IImageProcessor)providers[IMG_PROCESSOR];
            if (processor == null || !processor.Process(img, attrs, chain, document)) {
                String align;
                attrs.TryGetValue(HtmlTags.ALIGN, out align);
                if (align != null) {
                    CarriageReturn();
                }
                if (currentParagraph == null) {
                    currentParagraph = CreateParagraph();
                }
                currentParagraph.Add(new Chunk(img, 0, 0, true));
                currentParagraph.Alignment = HtmlUtilities.AlignmentValue(align);
                if (align != null) {
                    CarriageReturn();
                }
            }
        }

        /**
         * Processes the Table.
         * @throws DocumentException
         * @since 5.0.6
         */
        virtual public void ProcessTable() {
            TableWrapper table = (TableWrapper) stack.Pop();
            PdfPTable tb = table.CreateTable();
            tb.SplitRows = true;
            if (stack.Count == 0)
                document.Add(tb);
            else
                ((ITextElementArray) stack.Peek()).Add(tb);
        }

        /**
         * Gets the TableWrapper from the Stack and adds a new row.
         * @since 5.0.6
         */
        virtual public void ProcessRow() {
            List<PdfPCell> row = new List<PdfPCell>();
            List<float> cellWidths = new List<float>();
            bool percentage = false;
            float width;
            float totalWidth = 0;
            int zeroWidth = 0;
            TableWrapper table = null;
            while (true) {
                IElement obj = stack.Pop();
                if (obj is CellWrapper) {
                    CellWrapper cell = (CellWrapper)obj;
                    width = cell.Width;
                    cellWidths.Add(width);
                    percentage |= cell.IsPercentage;
                    if (width == 0) {
                        zeroWidth++;
                    }
                    else {
                        totalWidth += width;
                    }
                    row.Add(cell.Cell);
                }
                if (obj is TableWrapper) {
                    table = (TableWrapper) obj;
                    break;
                }
            }
            table.AddRow(row);
            if (cellWidths.Count > 0) {
                // cells come off the stack in reverse, naturally
                totalWidth = 100 - totalWidth;
                cellWidths.Reverse();
                float[] widths = new float[cellWidths.Count];
                bool hasZero = false;
                for (int i = 0; i < widths.Length; i++) {
                    widths[i] = cellWidths[i];
                    if (widths[i] == 0 && percentage && zeroWidth > 0) {
                        widths[i] = totalWidth / zeroWidth;
                    }
                    if (widths[i] == 0) {
                        hasZero = true;
                        break;
                    }
                }
                if (!hasZero)
                    table.ColWidths = widths;
            }
            stack.Push(table);
        }

        // state variables and methods

        /** Stack to keep track of table tags. */
        private Stack<bool[]> tableState = new Stack<bool[]>();

        /** Boolean to keep track of TR tags. */
        private bool pendingTR = false;

        /** Boolean to keep track of TD and TH tags */
        private bool pendingTD = false;

        /** Boolean to keep track of LI tags */
        private bool pendingLI = false;

        /**
         * Boolean to keep track of PRE tags
         * @since 5.0.6 renamed from isPRE
         */
        private bool insidePRE = false;

        /**
         * Indicates if text needs to be skipped.
         * @since iText 5.0.6 (private => protected)
         */
        protected internal bool skipText = false;

        /**
         * Pushes the values of pendingTR and pendingTD
         * to a state stack.
         * @since 5.0.6
         */
        virtual public void PushTableState() {
            tableState.Push(new bool[] { pendingTR, pendingTD });
        }

        /**
         * Pops the values of pendingTR and pendingTD
         * from a state stack.
         * @since 5.0.6
         */
        virtual public void PopTableState() {
            bool[] state = tableState.Pop();
            pendingTR = state[0];
            pendingTD = state[1];
        }

        /**
         * @return the pendingTR
         * @since 5.0.6
         */
        virtual public bool IsPendingTR() {
            return pendingTR;
        }

        /**
         * @param pendingTR the pendingTR to set
         * @since 5.0.6
         */
        virtual public void SetPendingTR(bool pendingTR) {
            this.pendingTR = pendingTR;
        }

        /**
         * @return the pendingTD
         * @since 5.0.6
         */
        virtual public bool IsPendingTD() {
            return pendingTD;
        }

        /**
         * @param pendingTD the pendingTD to set
         * @since 5.0.6
         */
        virtual public void SetPendingTD(bool pendingTD) {
            this.pendingTD = pendingTD;
        }

        /**
         * @return the pendingLI
         * @since 5.0.6
         */
        virtual public bool IsPendingLI() {
            return pendingLI;
        }

        /**
         * @param pendingLI the pendingLI to set
         * @since 5.0.6
         */
        virtual public void SetPendingLI(bool pendingLI) {
            this.pendingLI = pendingLI;
        }

        /**
         * @return the insidePRE
         * @since 5.0.6
         */
        virtual public bool IsInsidePRE() {
            return insidePRE;
        }

        /**
         * @param insidePRE the insidePRE to set
         * @since 5.0.6
         */
        virtual public void SetInsidePRE(bool insidePRE) {
            this.insidePRE = insidePRE;
        }

        /**
         * @return the skipText
         * @since 5.0.6
         */
        virtual public bool IsSkipText() {
            return skipText;
        }

        /**
         * @param skipText the skipText to set
         * @since 5.0.6
         */
        virtual public void SetSkipText(bool skipText) {
            this.skipText = skipText;
        }

        // static methods to parse HTML to a List of Element objects.

        /** The resulting list of elements. */
        protected List<IElement> objectList;

        /**
         * Parses an HTML source to a List of Element objects
         * @param reader    the HTML source
         * @param style     a StyleSheet object
         * @return a List of Element objects
         * @throws IOException
         */
        public static List<IElement> ParseToList(TextReader reader, StyleSheet style) {
            return ParseToList(reader, style, null);
        }

        /**
         * Parses an HTML source to a List of Element objects
         * @param reader    the HTML source
         * @param style     a StyleSheet object
         * @param providers map containing classes with extra info
         * @return a List of Element objects
         * @throws IOException
         */
        public static List<IElement> ParseToList(TextReader reader, StyleSheet style,
                Dictionary<String, Object> providers) {
            return ParseToList(reader, style, null, providers);
        }

        /**
         * Parses an HTML source to a List of Element objects
         * @param reader    the HTML source
         * @param style     a StyleSheet object
         * @param tags      a map containing supported tags and their processors
         * @param providers map containing classes with extra info
         * @return a List of Element objects
         * @throws IOException
         * @since 5.0.6
         */
        public static List<IElement> ParseToList(TextReader reader, StyleSheet style,
                IDictionary<String, IHTMLTagProcessor> tags, Dictionary<String, Object> providers) {
            HTMLWorker worker = new HTMLWorker(null, tags, style);
            worker.document = worker;
            worker.SetProviders(providers);
            worker.objectList = new List<IElement>();
            worker.Parse(reader);
            return worker.objectList;
        }

        // DocListener interface

        /**
         * @see com.itextpdf.text.ElementListener#add(com.itextpdf.text.Element)
         */
        virtual public bool Add(IElement element) {
            objectList.Add(element);
            return true;
        }

        /**
         * @see com.itextpdf.text.DocListener#close()
         */
        virtual public void Close() {
        }

        /**
         * @see com.itextpdf.text.DocListener#newPage()
         */
        virtual public bool NewPage() {
            return true;
        }

        /**
         * @see com.itextpdf.text.DocListener#open()
         */
        virtual public void Open() {
        }

        /**
         * @see com.itextpdf.text.DocListener#resetPageCount()
         */
        virtual public void ResetPageCount() {
        }

        /**
         * @see com.itextpdf.text.DocListener#setMarginMirroring(bool)
         */
        virtual public bool SetMarginMirroring(bool marginMirroring) {
            return false;
        }

        /**
         * @see com.itextpdf.text.DocListener#setMarginMirroring(bool)
         * @since   2.1.6
         */
        virtual public bool SetMarginMirroringTopBottom(bool marginMirroring) {
            return false;
        }

        /**
         * @see com.itextpdf.text.DocListener#setMargins(float, float, float, float)
         */
        virtual public bool SetMargins(float marginLeft, float marginRight,
                float marginTop, float marginBottom) {
            return true;
        }

        /**
         * @see com.itextpdf.text.DocListener#setPageCount(int)
         */
        virtual public int PageCount {
            set {
            }
        }

        /**
         * @see com.itextpdf.text.DocListener#setPageSize(com.itextpdf.text.Rectangle)
         */
        virtual public bool SetPageSize(Rectangle pageSize) {
            return true;
        }

        // deprecated methods

        /**
         * Sets the providers.
         * @deprecated use SetProviders() instead
         */
        virtual public void SetInterfaceProps(Dictionary<String, Object> providers) {
            SetProviders(providers);
        }
        /**
         * Gets the providers
         * @deprecated use GetProviders() instead
         */
        virtual public IDictionary<String, Object> GetInterfaceProps() {
            return providers;
        }

        public virtual void Dispose() {
            Close();
        }
    }
}
