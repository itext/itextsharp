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
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using iTextSharp.tool.xml.exceptions;
using iTextSharp.tool.xml.html.pdfelement;
using iTextSharp.tool.xml.pipeline.html;

namespace iTextSharp.tool.xml.html.table {

/**
 * @author redlab_b
 * 
 */
    public class TableData : AbstractTagProcessor {

	    public override IList<IElement> Content(IWorkerContext ctx, Tag tag,
			    String content) {
            return TextContent(ctx, tag, content);
	    }

	    /*
	     * (non-Javadoc)
	     * 
	     * @see
	     * com.itextpdf.tool.xml.TagProcessor#endElement(com.itextpdf.tool.xml.Tag,
	     * java.util.List, com.itextpdf.text.Document)
	     */
	    public override IList<IElement> End(IWorkerContext ctx, Tag tag, IList<IElement> currentContent) {
		    HtmlCell cell = new HtmlCell();
            int direction = GetRunDirection(tag);

            if (direction != PdfWriter.RUN_DIRECTION_NO_BIDI) {
                cell.RunDirection = direction;
            }

            if (HTML.Tag.TH.Equals(tag.Name, StringComparison.OrdinalIgnoreCase)) {
                cell.Role = PdfName.TH;
            }
            try {
                HtmlPipelineContext htmlPipelineContext = GetHtmlPipelineContext(ctx);
                cell = (HtmlCell) GetCssAppliers().Apply(cell, tag, htmlPipelineContext);
            } catch (NoCustomContextException e1) {
                throw new RuntimeWorkerException(LocaleMessages.GetInstance().GetMessage(LocaleMessages.NO_CUSTOM_CONTEXT), e1);
            }
		    IList<IElement> l = new List<IElement>(1);
            IList<IElement> chunks = new List<IElement>();
            IList<ListItem> listItems = new List<ListItem>();
	        int index = -1;
		    foreach (IElement e in currentContent) {
		        index++;
                if (e is Chunk || e is NoNewLineParagraph || e is LineSeparator) {
                    if (listItems.Count > 0) {
                        ProcessListItems(ctx, tag, listItems, cell);
                    }
                    if (e is Chunk && Chunk.NEWLINE.Content.Equals(((Chunk)e).Content)) {
                        if (index == currentContent.Count - 1) {
                            continue;
                        } else {
                            IElement nextElement = currentContent[index + 1];
                            if (chunks.Count > 0 && !(nextElement is Chunk) && !(nextElement is NoNewLineParagraph)) {
                                continue;
                            }
                        }
                    } else if (e is LineSeparator) {
                        try {
                            HtmlPipelineContext htmlPipelineContext = GetHtmlPipelineContext(ctx);
                            Chunk newLine = (Chunk)GetCssAppliers().Apply(new Chunk(Chunk.NEWLINE), tag, htmlPipelineContext);
                            chunks.Add(newLine);
                        } catch (NoCustomContextException exc) {
                            throw new RuntimeWorkerException(LocaleMessages.GetInstance().GetMessage(LocaleMessages.NO_CUSTOM_CONTEXT), exc);
                        }
                    }
                    chunks.Add(e);
                    continue;
                } else if (e is ListItem) {
                    if (chunks.Count >0 ) {
                        ProcessChunkItems(chunks, cell);
                    }
                    listItems.Add((ListItem)e);
                    continue;
                } else {
		            if (chunks.Count > 0) {
                       ProcessChunkItems(chunks, cell); 
                    }
                    if (listItems.Count > 0) {
                        ProcessListItems(ctx, tag, listItems, cell);
                    }
                }

                if (e is Paragraph) {
                    if (((Paragraph) e).Alignment == Element.ALIGN_UNDEFINED) {
                        ((Paragraph) e).Alignment = cell.HorizontalAlignment;
                    }
                }

			    cell.AddElement(e);
		    }
            if ( chunks.Count > 0 ) {
                ProcessChunkItems(chunks, cell);
            }
    	    l.Add(cell);
		    return l;
	    }

	    public override bool IsStackOwner() {
		    return true;
	    }

        protected void ProcessChunkItems(IList<IElement> chunks, HtmlCell cell) {
            Paragraph p = new Paragraph();
            p.MultipliedLeading = 1.2f;
            p.AddAll(chunks);
            p.Alignment = cell.HorizontalAlignment;
            if (p.Trim()) {
                cell.AddElement(p);
            }
            chunks.Clear();    
        }

        protected void ProcessListItems(IWorkerContext ctx, Tag tag, IList<ListItem> listItems, HtmlCell cell) {
            try {
                List list = new List();
                list.Autoindent = false;
                list = (List) GetCssAppliers().Apply(list, tag, GetHtmlPipelineContext(ctx));
                list.IndentationLeft = 0;
                foreach (ListItem li in listItems) {
                    ListItem listItem = (ListItem) GetCssAppliers().Apply(li, tag, GetHtmlPipelineContext(ctx));
                    listItem.SpacingAfter = 0;
                    listItem.SpacingBefore = 0;

                    listItem.MultipliedLeading = 1.2f;
                    list.Add(listItem);
                }
                cell.AddElement(list);
                listItems.Clear();
            } catch (NoCustomContextException e) {
                throw new RuntimeWorkerException(LocaleMessages.GetInstance().GetMessage(LocaleMessages.NO_CUSTOM_CONTEXT), e);
            }
        }
    }
}
