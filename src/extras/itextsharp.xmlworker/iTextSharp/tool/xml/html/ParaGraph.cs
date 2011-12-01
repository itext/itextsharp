using System;
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf.draw;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html.pdfelement;
using iTextSharp.tool.xml.pipeline.html;
/*
 * $Id: ParaGraph.java 122 2011-05-27 12:20:58Z redlab_b $
 *
 * This file is part of the iText (R) project. Copyright (c) 1998-2011 1T3XT BVBA Authors: Balder Van Camp, Emiel
 * Ackermann, et al.
 *
 * This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General
 * Public License version 3 as published by the Free Software Foundation with the addition of the following permission
 * added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY
 * 1T3XT, 1T3XT DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied
 * warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more
 * details. You should have received a copy of the GNU Affero General Public License along with this program; if not,
 * see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor,
 * Boston, MA, 02110-1301 USA, or download the license from the following URL: http://itextpdf.com/terms-of-use/
 *
 * The interactive user interfaces in modified source and object code versions of this program must display Appropriate
 * Legal Notices, as required under Section 5 of the GNU Affero General Public License.
 *
 * In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer
 * line in every PDF that is created or manipulated using iText.
 *
 * You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is
 * mandatory as soon as you develop commercial activities involving the iText software without disclosing the source
 * code of your own applications. These activities include: offering paid services to customers as an ASP, serving PDFs
 * on the fly in a web application, shipping iText with a closed source product.
 *
 * For more information, please contact iText Software Corp. at this address: sales@itextpdf.com
 */
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
            String sanitized = HTMLUtils.Sanitize(content);
            IList<IElement> l = new List<IElement>(1);
            if (sanitized.Length > 0) {
                HtmlPipelineContext myctx;
                try {
                    myctx = GetHtmlPipelineContext(ctx);
                } catch (NoCustomContextException e) {
                    throw new RuntimeWorkerException(e);
                }
                if (tag.CSS.ContainsKey(CSS.Property.TAB_INTERVAL)) {
                    TabbedChunk tabbedChunk = new TabbedChunk(sanitized);
                    if (null != GetLastChild(tag) && GetLastChild(tag).CSS.ContainsKey(CSS.Property.XFA_TAB_COUNT)) {
                        tabbedChunk.TabCount = int.Parse(GetLastChild(tag).CSS[CSS.Property.XFA_TAB_COUNT]);
                    }
                    l.Add(CssAppliers.GetInstance().Apply(tabbedChunk, tag,myctx));
                } else if (null != GetLastChild(tag) && GetLastChild(tag).CSS.ContainsKey(CSS.Property.XFA_TAB_COUNT)) {
                    TabbedChunk tabbedChunk = new TabbedChunk(sanitized);
                    tabbedChunk.TabCount = int.Parse(GetLastChild(tag).CSS[CSS.Property.XFA_TAB_COUNT]);
                    l.Add(CssAppliers.GetInstance().Apply(tabbedChunk, tag, myctx));
                } else {
                    l.Add(CssAppliers.GetInstance().Apply(new Chunk(sanitized), tag, myctx));
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
                Paragraph p = new Paragraph();
                IDictionary<String, String> css = tag.CSS;
                if (css.ContainsKey(CSS.Property.TAB_INTERVAL)) {
                    AddTabIntervalContent(currentContent, p, css[CSS.Property.TAB_INTERVAL]);
                    l.Add(p);
                } else if (css.ContainsKey(CSS.Property.TAB_STOPS)) { // <para tabstops=".." /> could use same implementation page 62
                    AddTabStopsContent(currentContent, p, css[CSS.Property.TAB_STOPS]);
                    l.Add(p);
                } else if (css.ContainsKey(CSS.Property.XFA_TAB_STOPS)) { // <para tabStops=".." /> could use same implementation page 63
                    AddTabStopsContent(currentContent, p, css[CSS.Property.XFA_TAB_STOPS]); // leader elements needs to be
                    l.Add(p);                                                                   // extracted.
                } else {
                    foreach (IElement e in CurrentContentToParagraph(currentContent, true, true, tag, ctx)) {
                        l.Add(e);
                    }
                }
            }
            return l;
        }

        /**
         * Applies the tab interval of the p tag on its {@link TabbedChunk} elements. <br />
         * The style "xfa-tab-count" of the {@link TabbedChunk} is multiplied with the tab interval of the p tag. This width is then added to a new {@link TabbedChunk}.
         *
         * @param currentContent containing the elements inside the p tag.
         * @param p paragraph to which the tabbed chunks will be added.
         * @param value the value of style "tab-interval".
         */
        private void AddTabIntervalContent(IList<IElement> currentContent, Paragraph p, String value) {
            float width = 0;
            foreach (IElement e in currentContent) {
                if (e is TabbedChunk) {
                    width += ((TabbedChunk) e).TabCount*CssUtils.GetInstance().ParsePxInCmMmPcToPt(value);
                    TabbedChunk tab = new TabbedChunk(new VerticalPositionMark(), width, false);
                    p.Add(new Chunk(tab));
                    p.Add(new Chunk((TabbedChunk) e));
                } else {
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
            for (int i = 0 , j = 1; j < alignAndWidth.Length ; i+=2, j+=2) {
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
    //              } else { // wat doet een tabCount van groter dan 1? sla een tab over of count * tabWidth?
    //                  int widthMultiplier = ((TabbedChunk) e).GetTabCount();
                    }
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