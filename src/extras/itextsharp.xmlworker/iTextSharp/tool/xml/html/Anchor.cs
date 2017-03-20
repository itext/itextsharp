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
using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.log;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html.pdfelement;
namespace iTextSharp.tool.xml.html {


    /**
     * @author redlab_b
     *
     */
    public class Anchor : AbstractTagProcessor {

        private static ILogger LOGGER = LoggerFactory.GetLogger(typeof(Anchor));

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.ITagProcessor#content(com.itextpdf.tool.xml.Tag,
         * java.util.List, com.itextpdf.text.Document, java.lang.String)
         */
        public override IList<IElement> Content(IWorkerContext ctx, Tag tag, String content) {
            return TextContent(ctx, tag, content);
        }

        /*
         * (non-Javadoc)
         *
         * @see
         * com.itextpdf.tool.xml.ITagProcessor#endElement(com.itextpdf.tool.xml.Tag,
         * java.util.List, com.itextpdf.text.Document)
         */
        public override IList<IElement> End(IWorkerContext ctx, Tag tag, IList<IElement> currentContent) {
            try {
                String name;
                tag.Attributes.TryGetValue(HTML.Attribute.NAME, out name);
                IList<IElement> elems = new List<IElement>(0);
                if (currentContent.Count > 0) {
                    NoNewLineParagraph p = new NoNewLineParagraph();
                    String url;
                    tag.Attributes.TryGetValue(HTML.Attribute.HREF, out url);
                    foreach (IElement e in currentContent) {
                        if (e is Chunk) {
                            if (null != url) {
                                if (url.StartsWith("#")) {
                                    if (LOGGER.IsLogging(Level.TRACE)) {
                                        LOGGER.Trace(String.Format(LocaleMessages.GetInstance().GetMessage(LocaleMessages.A_LOCALGOTO), url));
                                    }
                                    ((Chunk) e).SetLocalGoto(url.Substring(1));
                                } else {
                                    // TODO check url validity?
                                    if (null != GetHtmlPipelineContext(ctx).GetLinkProvider() && !url.StartsWith("http")) {
                                        String root = GetHtmlPipelineContext(ctx).GetLinkProvider().GetLinkRoot();
                                        if (root.EndsWith("/") && url.StartsWith("/")) {
                                            root = root.Substring(0, root.Length - 1);
                                        }
                                        url = root + url;
                                    }
                                    if (LOGGER.IsLogging(Level.TRACE)) {
                                        LOGGER.Trace(String.Format(LocaleMessages.GetInstance().GetMessage(LocaleMessages.A_EXTERNAL), url));
                                    }
                                    ((Chunk) e).SetAnchor(url);
                                }
                            } else if (null != name) {
                                ((Chunk) e).SetLocalDestination(name);
                                if (LOGGER.IsLogging(Level.TRACE)) {
                                    LOGGER.Trace(String.Format(LocaleMessages.GetInstance().GetMessage(LocaleMessages.A_SETLOCALGOTO), name));
                                }
                            }
                        }
                        p.Add(e);
                    }
                    elems.Add(GetCssAppliers().Apply(p, tag, GetHtmlPipelineContext(ctx)));
                } else
                // !currentContent > 0 ; An empty "a" tag has been encountered.
                // we're using an anchor space hack here. without the space, reader
                // does
                // not jump to destination
                if (null != name) {
                    if (LOGGER.IsLogging(Level.TRACE)) {
                        LOGGER.Trace(String.Format(LocaleMessages.GetInstance().GetMessage(LocaleMessages.SPACEHACK), name));
                    }
                    elems.Add(new WriteP(name));
                    /*
                     * PdfWriter writer = configuration.GetWriter(); ColumnText c =
                     * new ColumnText(writer.GetDirectContent());
                     * c.SetSimpleColumn(new Phrase(dest), 1,
                     * writer.GetVerticalPosition(false), 1,
                     * writer.GetVerticalPosition(false), 5, Element.ALIGN_LEFT);
                     * try { c.Go(); } catch (DocumentException e) { throw new
                     * RuntimeWorkerException(e); }
                     */
                }
                return elems;
            } catch (NoCustomContextException e) {
                throw new RuntimeWorkerException(LocaleMessages.GetInstance().GetMessage(LocaleMessages.NO_CUSTOM_CONTEXT), e);
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

        private class WriteP : WritableDirectElement {
            string name;
            public WriteP(string name) {
                this.name = name;
            }
            public override void Write(PdfWriter writer, Document doc) {
                ColumnText c = new ColumnText(writer.DirectContent);
                float verticalPosition = writer.GetVerticalPosition(false);
                c.SetSimpleColumn(new Phrase(new Chunk(" ").SetLocalDestination(name)), 1,
                        verticalPosition - 5, 6, verticalPosition, 5, Element.ALIGN_LEFT);
                try {
                    c.Go();
                }
                catch (DocumentException e) {
                    throw new RuntimeWorkerException(e);
                }
            }
        }

    }
}
