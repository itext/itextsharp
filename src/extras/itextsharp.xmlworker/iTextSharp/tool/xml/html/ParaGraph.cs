/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
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
using iTextSharp.text;
using iTextSharp.text.pdf.draw;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html.pdfelement;
using iTextSharp.tool.xml.pipeline.html;
namespace iTextSharp.tool.xml.html {

    /**
     * @author redlab_b
     *
     */
    public class ParaGraph : AbstractTagProcessor {


        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.ITagProcessor#content(com.itextpdf.tool.xml.Tag,
         * java.util.List, com.itextpdf.text.Document, java.lang.String)
         */
        public override IList<IElement> Content(IWorkerContext ctx, Tag tag, String content) {
            List<Chunk> sanitizedChunks = HTMLUtils.Sanitize(content, false);
		    List<IElement> l = new List<IElement>(1);
            foreach (Chunk sanitized in sanitizedChunks) {
                HtmlPipelineContext myctx;
                try {
                    myctx = GetHtmlPipelineContext(ctx);
                } catch (NoCustomContextException e) {
                    throw new RuntimeWorkerException(e);
                }
                if (tag.CSS.ContainsKey(CSS.Property.TAB_INTERVAL)) {
                    TabbedChunk tabbedChunk = new TabbedChunk(sanitized.Content);
                    if (null != GetLastChild(tag) && GetLastChild(tag).CSS.ContainsKey(CSS.Property.XFA_TAB_COUNT)) {
                        tabbedChunk.TabCount = int.Parse(GetLastChild(tag).CSS[CSS.Property.XFA_TAB_COUNT]);
                    }
                    l.Add(GetCssAppliers().Apply(tabbedChunk, tag,myctx));
                } else if (null != GetLastChild(tag) && GetLastChild(tag).CSS.ContainsKey(CSS.Property.XFA_TAB_COUNT)) {
                    TabbedChunk tabbedChunk = new TabbedChunk(sanitized.Content);
                    tabbedChunk.TabCount = int.Parse(GetLastChild(tag).CSS[CSS.Property.XFA_TAB_COUNT]);
                    l.Add(GetCssAppliers().Apply(tabbedChunk, tag, myctx));
                } else {
                    l.Add(GetCssAppliers().Apply(sanitized, tag, myctx));
                }
            }
            return l;
        }

        private Tag GetLastChild(Tag tag) {
            if (0 != tag.Children.Count)
                return tag.Children[tag.Children.Count - 1];
            else
                return null;
        }

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.ITagProcessor#endElement(com.itextpdf.tool.xml.Tag,
         * java.util.List, com.itextpdf.text.Document)
         */
	public override IList<IElement> End(IWorkerContext ctx, Tag tag, IList<IElement> currentContent) {
        IList<IElement> l = new List<IElement>(1);
        if (currentContent.Count > 0) {
            List<IElement> elements = new List<IElement>();
            List<ListItem> listItems = new List<ListItem>();
            foreach (IElement el in currentContent) {
                if (el is ListItem) {
                    if (elements.Count > 0) {
                        ProcessParagraphItems(ctx, tag, elements, l);
                        elements.Clear();
                    }
                    listItems.Add((ListItem)el);
                } else {
                    if (listItems.Count > 0) {
                        ProcessListItems(ctx, tag, listItems, l);
                        listItems.Clear();
                    }
                    elements.Add(el);
                }
            }
            if (elements.Count > 0) {
                ProcessParagraphItems(ctx, tag, elements, l);
                elements.Clear();
            } else if (listItems.Count > 0) {
                ProcessListItems(ctx, tag, listItems, l);
                listItems.Clear();
            }
        }
		return l;
	}

    virtual protected void ProcessParagraphItems(IWorkerContext ctx, Tag tag, IList<IElement> paragraphItems, IList<IElement> l) {
                Paragraph p = new Paragraph();
        p.MultipliedLeading = 1.2f;
//        IElement lastElement = paragraphItems[paragraphItems.Count - 1];
//        if (lastElement is Chunk && Chunk.NEWLINE.Content.Equals(((Chunk)lastElement).Content)) {
//            paragraphItems.RemoveAt(paragraphItems.Count - 1);
//        }
        IDictionary<String, String> css = tag.CSS;
        if (css.ContainsKey(CSS.Property.TAB_INTERVAL)) {
            AddTabIntervalContent(ctx, tag, paragraphItems, p, css[CSS.Property.TAB_INTERVAL]);
            l.Add(p);
        } else if (css.ContainsKey(CSS.Property.TAB_STOPS)) { // <para tabstops=".." /> could use same implementation page 62
            AddTabStopsContent(paragraphItems, p, css[CSS.Property.TAB_STOPS]);
            l.Add(p);
        } else if (css.ContainsKey(CSS.Property.XFA_TAB_STOPS)) { // <para tabStops=".." /> could use same implementation page 63
            AddTabStopsContent(paragraphItems, p, css[CSS.Property.XFA_TAB_STOPS]); // leader elements needs to be
            l.Add(p);                                                                    // extracted.
        } else {
            IList<IElement> paraList = CurrentContentToParagraph(paragraphItems, true, true, tag, ctx);
            if (l.Count > 0 && paraList.Count > 0) {
                IElement firstElement = paraList[0];
                if (firstElement is Paragraph ) {
                    ((Paragraph) firstElement).SpacingBefore = 0;
                }
            }
            foreach (IElement e in paraList) {
                l.Add(e);
            }
        }
    }
    virtual protected void ProcessListItems(IWorkerContext ctx, Tag tag, IList<ListItem> listItems, IList<IElement> l) {
        try {
            List list = new List();
            list.Autoindent = false;
            list = (List) GetCssAppliers().Apply(list, tag, GetHtmlPipelineContext(ctx));
            list.IndentationLeft = 0;
            int i = 0;
            foreach (ListItem li in listItems) {
                ListItem listItem = (ListItem) GetCssAppliers().Apply(li, tag, GetHtmlPipelineContext(ctx));
                if (i != listItems.Count - 1) {
                    listItem.SpacingAfter = 0;
                }
                if (i != 0 ) {
                    listItem.SpacingBefore = 0;
                }
                i++;
                listItem.MultipliedLeading = 1.2f;
                list.Add(listItem);
            }
            if (l.Count > 0) {
                IElement latestElement = l[l.Count - 1];
                if (latestElement is Paragraph ) {
                    ((Paragraph) latestElement).SpacingAfter = 0;
                }
            }
            l.Add(list);
        } catch (NoCustomContextException e) {
            throw new RuntimeWorkerException(LocaleMessages.GetInstance().GetMessage(LocaleMessages.NO_CUSTOM_CONTEXT), e);
        }
    }

        /**
         * Applies the tab interval of the p tag on its {@link TabbedChunk} elements. <br />
         * The style "xfa-tab-count" of the {@link TabbedChunk} is multiplied with the tab interval of the p tag. This width is then added to a new {@link TabbedChunk}.
         *
         * @param currentContent containing the elements inside the p tag.
         * @param p paragraph to which the tabbed chunks will be added.
         * @param value the value of style "tab-interval".
         */
        private void AddTabIntervalContent(IWorkerContext ctx, Tag tag, IList<IElement> currentContent, Paragraph p, String value) {
        float width = 0;
		foreach(IElement e in currentContent) {
		    if (e is TabbedChunk) {
			    width += ((TabbedChunk) e).TabCount * CssUtils.GetInstance().ParsePxInCmMmPcToPt(value);
                TabbedChunk tab = new TabbedChunk(new VerticalPositionMark(), width, false);
			    p.Add(new Chunk(tab));
			    p.Add(new Chunk((TabbedChunk) e));
            } else {
                if (e is LineSeparator) {
                    try {
                        HtmlPipelineContext htmlPipelineContext = GetHtmlPipelineContext(ctx);
                        Chunk newLine = (Chunk)GetCssAppliers().Apply(new Chunk(Chunk.NEWLINE), tag, htmlPipelineContext);
                        p.Add(newLine);
                    } catch (NoCustomContextException exc) {
                        throw new RuntimeWorkerException(LocaleMessages.GetInstance().GetMessage(LocaleMessages.NO_CUSTOM_CONTEXT), exc);
                    }
                }
			    p.Add(e);
            }
        }
    }

        /**
         * Applies the tab stops of the p tag on its {@link TabbedChunk} elements.
         *
         * @param currentContent containing the elements inside the p tag.
         * @param p paragraph to which the tabbed chunks will be added.
         * @param value the value of style "tab-stops".
         */

        private void AddTabStopsContent(IList<IElement> currentContent, Paragraph p, String value) {
            IList<Chunk> tabs = new List<Chunk>();
            String[] alignAndWidth = value.Split(' ');
            float tabWidth = 0;
            for (int i = 0, j = 1; j < alignAndWidth.Length; i += 2, j += 2) {
                tabWidth += CssUtils.GetInstance().ParsePxInCmMmPcToPt(alignAndWidth[j]);
                TabbedChunk tab = new TabbedChunk(new VerticalPositionMark(), tabWidth, true, alignAndWidth[i]);
                tabs.Add(tab);
            }
            int tabsPerRow = tabs.Count;
            int currentTab = 0;
            foreach (IElement e in currentContent) {
                if (e is TabbedChunk) {
                    if (currentTab == tabsPerRow) {
                        currentTab = 0;
                    }
                    if (((TabbedChunk) e).TabCount != 0 /* == 1*/) {
                        p.Add(new Chunk(tabs[currentTab]));
                        p.Add(new Chunk((TabbedChunk) e));
                        ++currentTab;
//                    } else {//wat doet een tabCount van groter dan 1? sla een tab over of count * tabWidth?
//                        int widthMultiplier = ((TabbedChunk) e).GetTabCount();
                    }
                } else {
                    p.Add(e);
                }
            }
        }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.ITagProcessor#isStackOwner()
         */
        public override bool IsStackOwner() {
            return true;
        }
    }
}
