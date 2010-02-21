using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.util;
using iTextSharp.text;
using iTextSharp.text.html;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using iTextSharp.text.xml.simpleparser;
/*
 * This file is part of the iText project.
 * Copyright (c) 1998-2009 1T3XT BVBA
 * Authors: Bruno Lowagie, Paulo Soares, et al.
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
 * you must retain the producer line in every PDF that is created or manipulated
 * using iText.
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

    public class HTMLWorker : ISimpleXMLDocHandler, IDocListener {
        
        protected List<IElement> objectList;
        protected IDocListener document;
        private Paragraph currentParagraph;
        private ChainedProperties cprops = new ChainedProperties();
        private Stack<IElement> stack = new Stack<IElement>();
        private bool pendingTR = false;
        private bool pendingTD = false;
        private bool pendingLI = false;
        private StyleSheet style = new StyleSheet();
        private bool isPRE = false;
        private Stack<bool[]> tableState = new Stack<bool[]>();
        private bool skipText = false;
        private Dictionary<String, Object> interfaceProps;
        private FactoryProperties factoryProperties = new FactoryProperties();
        
        /** Creates a new instance of HTMLWorker */
        public HTMLWorker(IDocListener document) {
            this.document = document;
        }
        
        public StyleSheet Style {
            set {
                style = value;
            }
            get {
                return style;
            }
        }
        
        public Dictionary<String, Object> InterfaceProps {
            set {
                interfaceProps = value;
                IFontProvider ff = null;
                if (interfaceProps != null && interfaceProps.ContainsKey("font_factory"))
                    ff = (IFontProvider)interfaceProps["font_factory"];
                if (ff != null)
                    factoryProperties.FontImp = ff;
            }
            get {
                return interfaceProps;
            }
        }

        public void Parse(TextReader reader) {
            SimpleXMLParser.Parse(this, null, reader, true);
        }
        
        public static List<IElement> ParseToList(TextReader reader, StyleSheet style) {
            return ParseToList(reader, style, null);
        }

        public static List<IElement> ParseToList(TextReader reader, StyleSheet style, Dictionary<String, Object> interfaceProps) {
            HTMLWorker worker = new HTMLWorker(null);
            if (style != null)
                worker.Style = style;
            worker.document = worker;
            worker.InterfaceProps = interfaceProps;
            worker.objectList = new List<IElement>();
            worker.Parse(reader);
            return worker.objectList;
        }
        
        public virtual void EndDocument() {
            foreach (IElement e in stack)
                document.Add(e);
            if (currentParagraph != null)
                document.Add(currentParagraph);
            currentParagraph = null;
        }
        
        public virtual void StartDocument() {
            Dictionary<String, String> h = new Dictionary<string,string>();
            style.ApplyStyle("body", h);
            cprops.AddToChain("body", h);
        }
        
        public virtual void StartElement(String tag, Dictionary<String, String> h) {
            if (!tagsSupported.ContainsKey(tag))
                return;
            style.ApplyStyle(tag, h);
            String follow = null;
            if (FactoryProperties.followTags.ContainsKey(tag))
                follow = FactoryProperties.followTags[tag];
            if (follow != null) {
                Dictionary<String, String> prop = new Dictionary<string,string>();
                prop[follow] = null;
                cprops.AddToChain(follow, prop);
                return;
            }
            FactoryProperties.InsertStyle(h, cprops);
            if (tag.Equals(HtmlTags.ANCHOR)) {
                cprops.AddToChain(tag, h);
                if (currentParagraph == null)
                    currentParagraph = new Paragraph();
                stack.Push(currentParagraph);
                currentParagraph = new Paragraph();
                return;
            }
            if (tag.Equals(HtmlTags.NEWLINE)) {
                if (currentParagraph == null)
                    currentParagraph = new Paragraph();
                currentParagraph.Add(factoryProperties.CreateChunk("\n", cprops));
                return;
            }
            if (tag.Equals(HtmlTags.HORIZONTALRULE)) {
                // Attempting to duplicate the behavior seen on Firefox with
                // http://www.w3schools.com/tags/tryit.asp?filename=tryhtml_hr_test
                // where an initial break is only inserted when the preceding element doesn't
                // end with a break, but a trailing break is always inserted.
                bool addLeadingBreak = true;
                if (currentParagraph == null) {
                    currentParagraph = new Paragraph();
                    addLeadingBreak = false;
                }
                if (addLeadingBreak) { // Not a new paragraph
                    int numChunks = currentParagraph.Chunks.Count;
                    if (numChunks == 0 ||
                        currentParagraph.Chunks[numChunks - 1].Content.EndsWith("\n"))
                        addLeadingBreak = false;
                }
                String align;
                int hrAlign = Element.ALIGN_CENTER;
                if (h.TryGetValue("align", out align)) {
                    if (Util.EqualsIgnoreCase(align, "left"))
                        hrAlign = Element.ALIGN_LEFT; 
                    if (Util.EqualsIgnoreCase(align, "right"))
                        hrAlign = Element.ALIGN_RIGHT;
                }
                String width;
                float hrWidth = 1;
                if (h.TryGetValue("width", out width)) {
                    float tmpWidth = Markup.ParseLength(width, Markup.DEFAULT_FONT_SIZE);
                    if (tmpWidth > 0) hrWidth = tmpWidth;
                    if (!width.EndsWith("%"))
                        hrWidth = 100; // Treat a pixel width as 100% for now.
                }
                String size;;
                float hrSize = 1;
                if (h.TryGetValue("size", out size)) {
                    float tmpSize = Markup.ParseLength(size, Markup.DEFAULT_FONT_SIZE);
                    if (tmpSize > 0)
                        hrSize = tmpSize;
                }
                if (addLeadingBreak)
                    currentParagraph.Add(Chunk.NEWLINE);
                currentParagraph.Add(new LineSeparator(hrSize, hrWidth, null, hrAlign, currentParagraph.Leading/2));
                currentParagraph.Add(Chunk.NEWLINE);
                return;
            }
            if (tag.Equals(HtmlTags.CHUNK) || tag.Equals(HtmlTags.SPAN)) {
                cprops.AddToChain(tag, h);
                return;
            }
            if (tag.Equals(HtmlTags.IMAGE)) {
                String src;
                if (!h.TryGetValue(ElementTags.SRC, out src))
                    return;
                cprops.AddToChain(tag, h);
                Image img = null;
                if (interfaceProps != null) {
                    IImageProvider ip;
                    if (interfaceProps.ContainsKey("img_provider")) {
                        ip = (IImageProvider)interfaceProps["img_provider"];
                        img = ip.GetImage(src, h, cprops, document);
                    }
                    if (img == null) {
                        Dictionary<String, Image> images;
                        if (interfaceProps.ContainsKey("img_static")) {
                            images = (Dictionary<string,Image>)interfaceProps["img_static"];
                            Image tim;
                            if (images.TryGetValue(src, out tim))
                                img = Image.GetInstance(tim);
                        } else {
                            if (!src.StartsWith("http")) { // relative src references only
                                if (interfaceProps.ContainsKey("img_baseurl")) {
                                    src = (string)interfaceProps["img_baseurl"] + src;
                                    img = Image.GetInstance(src);
                                }
                            }
                        }
                    }
                }
                if (img == null) {
                    if (!src.StartsWith("http")) {
                        String path = cprops["image_path"];
                        if (path == null)
                            path = "";
                        src = Path.Combine(path, src);
                    }
                    img = Image.GetInstance(src);
                }
                String align; h.TryGetValue("align", out align);
                String width; h.TryGetValue("width", out width);
                String height; h.TryGetValue("height", out height);
                String before = cprops["before"];
                String after = cprops["after"];
                if (before != null)
                    img.SpacingBefore = float.Parse(before, System.Globalization.NumberFormatInfo.InvariantInfo);
                if (after != null)
                    img.SpacingAfter = float.Parse(after, System.Globalization.NumberFormatInfo.InvariantInfo);
                float actualFontSize = Markup.ParseLength(cprops[ElementTags.SIZE], Markup.DEFAULT_FONT_SIZE);
                if (actualFontSize <= 0f)
                    actualFontSize = Markup.DEFAULT_FONT_SIZE;
                float widthInPoints = Markup.ParseLength(width, actualFontSize);
                float heightInPoints = Markup.ParseLength(height, actualFontSize);
                if (widthInPoints > 0 && heightInPoints > 0) {
                    img.ScaleAbsolute(widthInPoints, heightInPoints);
                } else if (widthInPoints > 0) {
                    heightInPoints = img.Height * widthInPoints / img.Width;
                    img.ScaleAbsolute(widthInPoints, heightInPoints);
                } else if (heightInPoints > 0) {
                    widthInPoints = img.Width * heightInPoints / img.Height;
                    img.ScaleAbsolute(widthInPoints, heightInPoints);
                }
                img.WidthPercentage = 0;
                if (align != null) {
                    EndElement("p");
                    int ralign = Image.MIDDLE_ALIGN;
                    if (Util.EqualsIgnoreCase(align, "left"))
                        ralign = Image.LEFT_ALIGN;
                    else if (Util.EqualsIgnoreCase(align, "right"))
                        ralign = Image.RIGHT_ALIGN;
                    img.Alignment = ralign;
                    IImg i = null;
                    bool skip = false;
                    if (interfaceProps != null) {
                        i = (IImg)interfaceProps["img_interface"];
                        if (interfaceProps.ContainsKey("img_interface"))
                            i = (IImg)interfaceProps["img_interface"];
                        if (i != null)
                            skip = i.Process(img, h, cprops, document);
                    }
                    if (!skip)
                        document.Add(img);
                    cprops.RemoveChain(tag);
                }
                else {
                    cprops.RemoveChain(tag);
                    if (currentParagraph == null)
                        currentParagraph = FactoryProperties.CreateParagraph(cprops);
                    currentParagraph.Add(new Chunk(img, 0, 0));
                }
                return;
            }

            EndElement("p");
            if (tag.Equals("h1") || tag.Equals("h2") || tag.Equals("h3") || tag.Equals("h4") || tag.Equals("h5") || tag.Equals("h6")) {
                if (!h.ContainsKey(ElementTags.SIZE)) {
                    int v = 7 - int.Parse(tag.Substring(1));
                    h[ElementTags.SIZE] = v.ToString();
                }
                cprops.AddToChain(tag, h);
                return;
            }
            if (tag.Equals(HtmlTags.UNORDEREDLIST)) {
                if (pendingLI)
                    EndElement(HtmlTags.LISTITEM);
                skipText = true;
                cprops.AddToChain(tag, h);
                List list = new List(false);
                try{
                    list.IndentationLeft = float.Parse(cprops["indent"], System.Globalization.NumberFormatInfo.InvariantInfo);
                }catch {
                    list.Autoindent = true;
                }
                list.SetListSymbol("\u2022");
                stack.Push(list);
                return;
            }
            if (tag.Equals(HtmlTags.ORDEREDLIST)) {
                if (pendingLI)
                    EndElement(HtmlTags.LISTITEM);
                skipText = true;
                cprops.AddToChain(tag, h);
                List list = new List(true);
                try{
                    list.IndentationLeft = float.Parse(cprops["indent"], System.Globalization.NumberFormatInfo.InvariantInfo);
                }catch {
                    list.Autoindent = true;
                }
                stack.Push(list);
                return;
            }
            if (tag.Equals(HtmlTags.LISTITEM)) {
                if (pendingLI)
                    EndElement(HtmlTags.LISTITEM);
                skipText = false;
                pendingLI = true;
                cprops.AddToChain(tag, h);
                stack.Push(FactoryProperties.CreateListItem(cprops));
                return;
            }
            if (tag.Equals(HtmlTags.DIV) || tag.Equals(HtmlTags.BODY) || tag.Equals("p")) {
                cprops.AddToChain(tag, h);
                return;
            }
            if (tag.Equals(HtmlTags.PRE)) {
                if (!h.ContainsKey(ElementTags.FACE)) {
                    h[ElementTags.FACE] = "Courier";
                }
                cprops.AddToChain(tag, h);
                isPRE = true;
                return;
            }
            if (tag.Equals("tr")) {
                if (pendingTR)
                    EndElement("tr");
                skipText = true;
                pendingTR = true;
                cprops.AddToChain("tr", h);
                return;
            }
            if (tag.Equals("td") || tag.Equals("th")) {
                if (pendingTD)
                    EndElement(tag);
                skipText = false;
                pendingTD = true;
                cprops.AddToChain("td", h);
                stack.Push(new IncCell(tag, cprops));
                return;
            }
            if (tag.Equals("table")) {
                cprops.AddToChain("table", h);
                IncTable table = new IncTable(h);
                stack.Push(table);
                tableState.Push(new bool[]{pendingTR, pendingTD});
                pendingTR = pendingTD = false;
                skipText = true;
                return;
            }
        }
        
        public virtual void EndElement(String tag) {
            if (!tagsSupported.ContainsKey(tag))
                return;
            String follow;
            if (FactoryProperties.followTags.TryGetValue(tag, out follow)) {
                cprops.RemoveChain(follow);
                return;
            }
            if (tag.Equals("font") || tag.Equals("span")) {
                cprops.RemoveChain(tag);
                return;
            }
            if (tag.Equals("a")) {
                if (currentParagraph == null)
                    currentParagraph = new Paragraph();
                IALink i = null;
                bool skip = false;
                if (interfaceProps != null) {
                    if (interfaceProps.ContainsKey("alink_interface"))
                        i = (IALink)interfaceProps["alink_interface"];
                    if (i != null)
                        skip = i.Process(currentParagraph, cprops);
                }
                if (!skip) {
                    String href = cprops["href"];
                    if (href != null) {
                        foreach (Chunk ck in currentParagraph.Chunks) {
                            ck.SetAnchor(href);
                        }
                    }
                }
                Paragraph tmp = (Paragraph)stack.Pop();
                Phrase tmp2 = new Phrase();
                tmp2.Add(currentParagraph);
                tmp.Add(tmp2);
                currentParagraph = tmp;
                cprops.RemoveChain("a");
                return;
            }
            if (tag.Equals("br")) {
                return;
            }
            if (currentParagraph != null) {
                if (stack.Count == 0)
                    document.Add(currentParagraph);
                else {
                    IElement obj = stack.Pop();
                    if (obj is ITextElementArray) {
                        ITextElementArray current = (ITextElementArray)obj;
                        current.Add(currentParagraph);
                    }
                    stack.Push(obj);
                }
            }
            currentParagraph = null;
            if (tag.Equals(HtmlTags.UNORDEREDLIST) || tag.Equals(HtmlTags.ORDEREDLIST)) {
                if (pendingLI)
                    EndElement(HtmlTags.LISTITEM);
                skipText = false;
                cprops.RemoveChain(tag);
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
                    ((ITextElementArray)stack.Peek()).Add(obj);
                return;
            }
            if (tag.Equals(HtmlTags.LISTITEM)) {
                pendingLI = false;
                skipText = true;
                cprops.RemoveChain(tag);
                if (stack.Count == 0)
                    return;
                IElement obj = stack.Pop();
                if (!(obj is ListItem)) {
                    stack.Push(obj);
                    return;
                }
                if (stack.Count == 0) {
                    document.Add((IElement)obj);
                    return;
                }
                IElement list = stack.Pop();
                if (!(list is List)) {
                    stack.Push(list);
                    return;
                }
                ListItem item = (ListItem)obj;
                ((List)list).Add(item);
                List<Chunk> cks = item.Chunks;
                if (cks.Count > 0)
                    item.ListSymbol.Font = cks[0].Font;
                stack.Push(list);
                return;
            }
            if (tag.Equals("div") || tag.Equals("body")) {
                cprops.RemoveChain(tag);
                return;
            }
            if (tag.Equals(HtmlTags.PRE)) {
                cprops.RemoveChain(tag);
                isPRE = false;
                return;
            }
            if (tag.Equals("p")) {
                cprops.RemoveChain(tag);
                return;
            }
            if (tag.Equals("h1") || tag.Equals("h2") || tag.Equals("h3") || tag.Equals("h4") || tag.Equals("h5") || tag.Equals("h6")) {
                cprops.RemoveChain(tag);
                return;
            }
            if (tag.Equals("table")) {
                if (pendingTR)
                    EndElement("tr");
                cprops.RemoveChain("table");
                IncTable table = (IncTable) stack.Pop();
                PdfPTable tb = table.BuildTable();
                tb.SplitRows = true;
                if (stack.Count == 0)
                    document.Add(tb);
                else
                    ((ITextElementArray)stack.Peek()).Add(tb);
                bool[] state = tableState.Pop();
                pendingTR = state[0];
                pendingTD = state[1];
                skipText = false;
                return;
            }
            if (tag.Equals("tr")) {
                if (pendingTD)
                    EndElement("td");
                pendingTR = false;
                cprops.RemoveChain("tr");
                List<PdfPCell> cells = new List<PdfPCell>();
                IncTable table = null;
                while (true) {
                    IElement obj = stack.Pop();
                    if (obj is IncCell) {
                        cells.Add(((IncCell)obj).Cell);
                    }
                    if (obj is IncTable) {
                        table = (IncTable)obj;
                        break;
                    }
                }
                table.AddCols(cells);
                table.EndRow();
                stack.Push(table);
                skipText = true;
                return;
            }
            if (tag.Equals("td") || tag.Equals("th")) {
                pendingTD = false;
                cprops.RemoveChain("td");
                skipText = true;
                return;
            }
        }
        
        public virtual void Text(String str) {
            if (skipText)
                return;
            String content = str;
            if (isPRE) {
                if (currentParagraph == null)
                    currentParagraph = FactoryProperties.CreateParagraph(cprops);
                currentParagraph.Add(factoryProperties.CreateChunk(content, cprops));
                return;
            }
            if (content.Trim().Length == 0 && content.IndexOf(' ') < 0) {
                return;
            }
            
            StringBuilder buf = new StringBuilder();
            int len = content.Length;
            char character;
            bool newline = false;
            for (int i = 0; i < len; i++) {
                switch (character = content[i]) {
                    case ' ':
                        if (!newline) {
                            buf.Append(character);
                        }
                        break;
                    case '\n':
                        if (i > 0) {
                            newline = true;
                            buf.Append(' ');
                        }
                        break;
                    case '\r':
                        break;
                    case '\t':
                        break;
                    default:
                        newline = false;
                        buf.Append(character);
                        break;
                }
            }
            if (currentParagraph == null)
                currentParagraph = FactoryProperties.CreateParagraph(cprops);
            currentParagraph.Add(factoryProperties.CreateChunk(buf.ToString(), cprops));
        }
        
        public bool Add(IElement element) {
            objectList.Add(element);
            return true;
        }
        
        public void ClearTextWrap() {
        }
        
        public void Close() {
        }
        
        public bool NewPage() {
            return true;
        }
        
        public void Open() {
        }
        
        public void ResetFooter() {
        }
        
        public void ResetHeader() {
        }
        
        public void ResetPageCount() {
        }
        
        public bool SetMarginMirroring(bool marginMirroring) {
            return false;
        }
        
        /**
         * @see com.lowagie.text.DocListener#setMarginMirroring(boolean)
         * @since	2.1.6
         */
        public bool SetMarginMirroringTopBottom(bool marginMirroring) {
            return false;
        }

        public bool SetMargins(float marginLeft, float marginRight, float marginTop, float marginBottom) {
            return true;
        }
        
        public bool SetPageSize(Rectangle pageSize) {
            return true;
        }
        
        public const String tagsSupportedString = "ol ul li a pre font span br p div body table td th tr i b u sub sup em strong s strike"
            + " h1 h2 h3 h4 h5 h6 img hr";
        
        public static Dictionary<string,object> tagsSupported = new Dictionary<string,object>();
        
        static HTMLWorker() {
            StringTokenizer tok = new StringTokenizer(tagsSupportedString);
            while (tok.HasMoreTokens())
                tagsSupported[tok.NextToken()] = null;
        }
    
        public int PageCount {
            set {
            }
        }
    }
}
