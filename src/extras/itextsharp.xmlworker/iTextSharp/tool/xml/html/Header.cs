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
using iTextSharp.text.log;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.pipeline.html;
using iTextSharp.tool.xml.util;

namespace iTextSharp.tool.xml.html {

    /**
     * @author Emiel Ackermann, redlab_b
     *
     */
    public class Header : AbstractTagProcessor {

        private static ILogger LOGGER = LoggerFactory.GetLogger(typeof(Header));
        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.ITagProcessor#content(com.itextpdf.tool.xml.Tag, java.util.List, com.itextpdf.text.Document, java.lang.String)
         */
        public override IList<IElement> Content(IWorkerContext ctx, Tag tag, String content) {
            return TextContent(ctx, tag, content);
        }

        /* (non-Javadoc)
         * @see com.itextpdf.tool.xml.ITagProcessor#endElement(com.itextpdf.tool.xml.Tag, java.util.List, com.itextpdf.text.Document)
         */
        public override IList<IElement> End(IWorkerContext ctx, Tag tag, IList<IElement> currentContent) {
            List<IElement> l = new List<IElement>(1);
            if (currentContent.Count > 0) {
                IList<IElement> currentContentToParagraph = CurrentContentToParagraph(currentContent, true, true, tag, ctx);
                foreach (IElement p in currentContentToParagraph) {
                    ((Paragraph) p).Role = (getHeaderRole(GetLevel(tag)));
                }
                ParentTreeUtil pt = new ParentTreeUtil();
                try {
                    HtmlPipelineContext context = GetHtmlPipelineContext(ctx);
                    
                    bool oldBookmark = context.AutoBookmark();
                    if (pt.GetParentTree(tag).Contains(HTML.Tag.TD))
                        context.AutoBookmark(false);

                    if (context.AutoBookmark()) {
                        Paragraph title = new Paragraph();
                        foreach (IElement w in currentContentToParagraph) {
                                title.Add(w);
                        }

                        l.Add(new WriteH(context, tag, this, title));
                    }

                    context.AutoBookmark(oldBookmark);
                } catch (NoCustomContextException e) {
                    if (LOGGER.IsLogging(Level.ERROR)) {
                        LOGGER.Error(LocaleMessages.GetInstance().GetMessage(LocaleMessages.HEADER_BM_DISABLED), e);
                    }
                }
                l.AddRange(currentContentToParagraph);
            }
            return l;
        }

        private PdfName getHeaderRole(int level) {
            switch (level) {
                case 1:
                    return PdfName.H1;
                case 2:
                    return PdfName.H2;
                case 3:
                    return PdfName.H3;
                case 4:
                    return PdfName.H4;
                case 5:
                    return PdfName.H5;
                case 6:
                    return PdfName.H6;
            }
            return PdfName.H;
        }

        /**
         * @param tag
         * @return
         */
        private int GetLevel(Tag tag) {
            return int.Parse(tag.Name.Substring(1,1));
        }

        /*
         * (non-Javadoc)
         *
         * @see com.itextpdf.tool.xml.ITagProcessor#isStackOwner()
         */
        public override bool IsStackOwner() {
            return true;
        }

        private class WriteH : WritableDirectElement {
            HtmlPipelineContext context;
            Tag tag;
            Header header;
            Paragraph title;
            public WriteH(HtmlPipelineContext context, Tag tag, Header header, Paragraph title) {
                this.context = context;
                this.tag = tag;
                this.header = header;
                this.title = title;
                base.directElementType = DIRECT_ELEMENT_TYPE_HEADER;
            }
            public override void Write(PdfWriter writer, Document doc) {
                PdfDestination destination = new PdfDestination(PdfDestination.XYZ, 20,
                        writer.GetVerticalPosition(false), 0);
                IDictionary<String, Object> memory = context.GetMemory();
                HeaderNode tree = null;
                if (memory.ContainsKey(HtmlPipelineContext.BOOKMARK_TREE))
                    tree = (HeaderNode)memory[HtmlPipelineContext.BOOKMARK_TREE];
                int level = header.GetLevel(tag);
                if (null == tree) {
                    // first h tag encounter
                    tree = new HeaderNode(0, writer.RootOutline, null);
                }
                else {
                    // calculate parent
                    int lastLevel = tree.Level;
                    if (lastLevel == level) {
                        tree = tree.Parent;
                    }
                    else if (lastLevel > level) {
                        while (lastLevel >= level) {
                            lastLevel = tree.Parent.Level;
                            tree = tree.Parent;
                        }
                    }
                }
                if (LOGGER.IsLogging(Level.TRACE)) {
                    LOGGER.Trace(String.Format(LocaleMessages.GetInstance().GetMessage(LocaleMessages.ADD_HEADER), title.ToString()));
                }
                HeaderNode node = new HeaderNode(level, new PdfOutline(tree.Outline, destination, title), tree);
                memory[HtmlPipelineContext.BOOKMARK_TREE] = node;
            }
        }
    }
}
